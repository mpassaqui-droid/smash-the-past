"""
Construye un luchador low poly con esqueleto compatible Mixamo y lo exporta a FBX.

Se ejecuta sin abrir Blender:
    blender --background --python tools/blender/personaje.py -- --salida ruta.fbx

Por que un esqueleto Mixamo: con esos nombres de hueso, Unity lo reconoce como
Humanoid y todo el catalogo gratuito de animaciones de Mixamo vale sin retocar nada.
Esa es la razon de pasar por Blender en vez de seguir apilando cubos en Unity.

Los pesos son rigidos: cada trozo del cuerpo pertenece entero a un hueso. En un
personaje de formas duras eso da mejor resultado que pintar pesos, y es reproducible.
"""

import bpy
import bmesh
import sys
from mathutils import Vector

ALTURA_CADERA = 1.00
ALTURA_PECHO = 1.48
ALTURA_CUELLO = 1.58
ALTURA_CABEZA = 1.66
ANCHO_HOMBRO = 0.19
ANCHO_CADERA = 0.09

# nombre -> (cabeza, cola). Los nombres son los de Mixamo, sin inventar nada.
HUESOS = {
    "mixamorig:Hips":          ((0, 0, ALTURA_CADERA), (0, 0, 1.15)),
    "mixamorig:Spine":         ((0, 0, 1.15), (0, 0, 1.28)),
    "mixamorig:Spine1":        ((0, 0, 1.28), (0, 0, 1.40)),
    "mixamorig:Spine2":        ((0, 0, 1.40), (0, 0, ALTURA_PECHO)),
    "mixamorig:Neck":          ((0, 0, ALTURA_PECHO), (0, 0, ALTURA_CUELLO)),
    "mixamorig:Head":          ((0, 0, ALTURA_CUELLO), (0, 0, 1.86)),

    "mixamorig:LeftShoulder":  ((0.05, 0, 1.44), (ANCHO_HOMBRO, 0, 1.44)),
    "mixamorig:LeftArm":       ((ANCHO_HOMBRO, 0, 1.44), (ANCHO_HOMBRO + 0.02, 0, 1.16)),
    "mixamorig:LeftForeArm":   ((ANCHO_HOMBRO + 0.02, 0, 1.16), (ANCHO_HOMBRO + 0.03, 0, 0.92)),
    "mixamorig:LeftHand":      ((ANCHO_HOMBRO + 0.03, 0, 0.92), (ANCHO_HOMBRO + 0.03, 0, 0.82)),

    "mixamorig:RightShoulder": ((-0.05, 0, 1.44), (-ANCHO_HOMBRO, 0, 1.44)),
    "mixamorig:RightArm":      ((-ANCHO_HOMBRO, 0, 1.44), (-(ANCHO_HOMBRO + 0.02), 0, 1.16)),
    "mixamorig:RightForeArm":  ((-(ANCHO_HOMBRO + 0.02), 0, 1.16), (-(ANCHO_HOMBRO + 0.03), 0, 0.92)),
    "mixamorig:RightHand":     ((-(ANCHO_HOMBRO + 0.03), 0, 0.92), (-(ANCHO_HOMBRO + 0.03), 0, 0.82)),

    "mixamorig:LeftUpLeg":     ((ANCHO_CADERA, 0, ALTURA_CADERA), (ANCHO_CADERA, 0, 0.55)),
    "mixamorig:LeftLeg":       ((ANCHO_CADERA, 0, 0.55), (ANCHO_CADERA, 0, 0.10)),
    "mixamorig:LeftFoot":      ((ANCHO_CADERA, 0, 0.10), (ANCHO_CADERA, -0.14, 0.03)),

    "mixamorig:RightUpLeg":    ((-ANCHO_CADERA, 0, ALTURA_CADERA), (-ANCHO_CADERA, 0, 0.55)),
    "mixamorig:RightLeg":      ((-ANCHO_CADERA, 0, 0.55), (-ANCHO_CADERA, 0, 0.10)),
    "mixamorig:RightFoot":     ((-ANCHO_CADERA, 0, 0.10), (-ANCHO_CADERA, -0.14, 0.03)),
}

PADRES = {
    "mixamorig:Spine": "mixamorig:Hips",
    "mixamorig:Spine1": "mixamorig:Spine",
    "mixamorig:Spine2": "mixamorig:Spine1",
    "mixamorig:Neck": "mixamorig:Spine2",
    "mixamorig:Head": "mixamorig:Neck",
    "mixamorig:LeftShoulder": "mixamorig:Spine2",
    "mixamorig:LeftArm": "mixamorig:LeftShoulder",
    "mixamorig:LeftForeArm": "mixamorig:LeftArm",
    "mixamorig:LeftHand": "mixamorig:LeftForeArm",
    "mixamorig:RightShoulder": "mixamorig:Spine2",
    "mixamorig:RightArm": "mixamorig:RightShoulder",
    "mixamorig:RightForeArm": "mixamorig:RightArm",
    "mixamorig:RightHand": "mixamorig:RightForeArm",
    "mixamorig:LeftUpLeg": "mixamorig:Hips",
    "mixamorig:LeftLeg": "mixamorig:LeftUpLeg",
    "mixamorig:LeftFoot": "mixamorig:LeftLeg",
    "mixamorig:RightUpLeg": "mixamorig:Hips",
    "mixamorig:RightLeg": "mixamorig:RightUpLeg",
    "mixamorig:RightFoot": "mixamorig:RightLeg",
}

# (nombre, hueso, centro, tamano, material)
# (nombre, hueso, centro, tamano, material)
# Los tramos se solapan un centimetro a proposito: sin ese solape quedan huecos entre
# las piezas y el personaje se ve desmontado, que es justo lo que paso en la primera
# version. Ninguna medida es libre, cada una encaja con la de al lado.
PARTES_BASE = [
    ("pieIzq",        "mixamorig:LeftFoot",     (0.09, -0.04, 0.06),  (0.14, 0.26, 0.12), "bota"),
    ("piernaIzq",     "mixamorig:LeftLeg",      (0.09, 0, 0.34),      (0.13, 0.13, 0.46), "ropa"),
    ("musloIzq",      "mixamorig:LeftUpLeg",    (0.09, 0, 0.79),      (0.15, 0.15, 0.48), "ropa"),
    ("pieDer",        "mixamorig:RightFoot",    (-0.09, -0.04, 0.06), (0.14, 0.26, 0.12), "bota"),
    ("piernaDer",     "mixamorig:RightLeg",     (-0.09, 0, 0.34),     (0.13, 0.13, 0.46), "ropa"),
    ("musloDer",      "mixamorig:RightUpLeg",   (-0.09, 0, 0.79),     (0.15, 0.15, 0.48), "ropa"),

    ("cadera",        "mixamorig:Hips",         (0, 0, 1.08),  (0.26, 0.18, 0.18), "ropa"),
    ("torso",         "mixamorig:Spine1",       (0, 0, 1.32),  (0.32, 0.20, 0.38), "ropa"),
    ("cuello",        "mixamorig:Neck",         (0, 0, 1.53),  (0.11, 0.11, 0.12), "piel"),
    ("cabeza",        "mixamorig:Head",         (0, 0, 1.69),  (0.23, 0.22, 0.26), "piel"),

    ("brazoIzq",      "mixamorig:LeftArm",      (0.21, 0, 1.31),  (0.11, 0.11, 0.32), "ropa"),
    ("antebrazoIzq",  "mixamorig:LeftForeArm",  (0.22, 0, 1.05),  (0.10, 0.10, 0.28), "piel"),
    ("manoIzq",       "mixamorig:LeftHand",     (0.22, 0, 0.87),  (0.12, 0.12, 0.14), "piel"),
    ("brazoDer",      "mixamorig:RightArm",     (-0.21, 0, 1.31), (0.11, 0.11, 0.32), "ropa"),
    ("antebrazoDer",  "mixamorig:RightForeArm", (-0.22, 0, 1.05), (0.10, 0.10, 0.28), "piel"),
    ("manoDer",       "mixamorig:RightHand",    (-0.22, 0, 0.87), (0.12, 0.12, 0.14), "piel"),
]

# Lo que distingue a cada personaje va aparte: el cuerpo es el mismo para todos.
EXTRAS = {
    "TupacAmaruII": [
        # El ala apoya en la coronilla (la cabeza acaba en 1.82), no flotando encima.
        ("alaSombrero", "mixamorig:Head", (0, 0, 1.81),  (0.50, 0.50, 0.035), "acento"),
        ("copaSombrero","mixamorig:Head", (0, 0, 1.875), (0.25, 0.25, 0.13), "acento"),
        ("poncho",      "mixamorig:Spine1", (0, 0, 1.32), (0.40, 0.26, 0.36), "principal"),
        ("franjaPoncho","mixamorig:Spine1", (0, 0, 1.15), (0.41, 0.27, 0.05), "acento2"),
    ],
}

COLORES = {
    "TupacAmaruII": {
        "principal": (0.62, 0.09, 0.10, 1),
        "acento": (0.06, 0.06, 0.07, 1),
        "acento2": (0.88, 0.62, 0.14, 1),
        "ropa": (0.45, 0.07, 0.08, 1),
        "piel": (0.58, 0.36, 0.23, 1),
        "bota": (0.10, 0.09, 0.08, 1),
    },
}


def limpiar():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for coleccion in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials):
        for bloque in list(coleccion):
            if bloque.users == 0:
                coleccion.remove(bloque)


def material(nombre, color):
    mat = bpy.data.materials.new(nombre)
    mat.use_nodes = True
    principal = mat.node_tree.nodes.get("Principled BSDF")
    if principal:
        principal.inputs["Base Color"].default_value = color
        principal.inputs["Roughness"].default_value = 0.85
        if "Specular IOR Level" in principal.inputs:
            principal.inputs["Specular IOR Level"].default_value = 0.15
    return mat


def caja(nombre, centro, tamano):
    malla = bpy.data.meshes.new(nombre)
    objeto = bpy.data.objects.new(nombre, malla)
    bpy.context.collection.objects.link(objeto)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for vertice in bm.verts:
        vertice.co.x *= tamano[0]
        vertice.co.y *= tamano[1]
        vertice.co.z *= tamano[2]
        vertice.co += Vector(centro)
    bm.to_mesh(malla)
    bm.free()
    return objeto


def construir(personaje):
    limpiar()
    paleta = {clave: material(f"{personaje}_{clave}", color)
              for clave, color in COLORES[personaje].items()}

    partes = PARTES_BASE + EXTRAS.get(personaje, [])
    objetos = []
    for nombre, hueso, centro, tamano, clave in partes:
        objeto = caja(f"{personaje}_{nombre}", centro, tamano)
        objeto.data.materials.append(paleta[clave])
        # Un bisel corto quita el aspecto de cubo crudo sin subir el recuento.
        bisel = objeto.modifiers.new("Bisel", "BEVEL")
        bisel.width = 0.012
        bisel.segments = 2
        objeto["hueso"] = hueso
        objetos.append(objeto)

    # Unir todo en una malla, guardando a que hueso va cada trozo.
    bpy.ops.object.select_all(action="DESELECT")
    for objeto in objetos:
        objeto.select_set(True)
    bpy.context.view_layer.objects.active = objetos[0]
    for objeto in objetos:
        bpy.context.view_layer.objects.active = objeto
        bpy.ops.object.modifier_apply(modifier="Bisel")

    asignaciones = []
    desplazamiento = 0
    for objeto in objetos:
        cuenta = len(objeto.data.vertices)
        asignaciones.append((objeto["hueso"], desplazamiento, cuenta))
        desplazamiento += cuenta

    bpy.ops.object.select_all(action="DESELECT")
    for objeto in objetos:
        objeto.select_set(True)
    bpy.context.view_layer.objects.active = objetos[0]
    bpy.ops.object.join()
    cuerpo = bpy.context.view_layer.objects.active
    cuerpo.name = personaje

    # Pesos rigidos: cada trozo entero a su hueso.
    for hueso, inicio, cuenta in asignaciones:
        grupo = cuerpo.vertex_groups.get(hueso) or cuerpo.vertex_groups.new(name=hueso)
        grupo.add(list(range(inicio, inicio + cuenta)), 1.0, "REPLACE")

    # Esqueleto.
    armadura = bpy.data.armatures.new(f"{personaje}_esqueleto")
    objeto_armadura = bpy.data.objects.new(f"{personaje}_esqueleto", armadura)
    bpy.context.collection.objects.link(objeto_armadura)
    bpy.context.view_layer.objects.active = objeto_armadura
    bpy.ops.object.mode_set(mode="EDIT")
    creados = {}
    for nombre, (cabeza, cola) in HUESOS.items():
        hueso = armadura.edit_bones.new(nombre)
        hueso.head = Vector(cabeza)
        hueso.tail = Vector(cola)
        creados[nombre] = hueso
    for hijo, padre in PADRES.items():
        creados[hijo].parent = creados[padre]
        creados[hijo].use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")

    cuerpo.parent = objeto_armadura
    modificador = cuerpo.modifiers.new("Esqueleto", "ARMATURE")
    modificador.object = objeto_armadura

    return cuerpo, objeto_armadura


def exportar(ruta, armadura, cuerpo):
    bpy.ops.object.select_all(action="DESELECT")
    armadura.select_set(True)
    cuerpo.select_set(True)
    bpy.context.view_layer.objects.active = armadura
    bpy.ops.export_scene.fbx(
        filepath=ruta,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False,
        bake_anim=False,
        object_types={"ARMATURE", "MESH"},
        mesh_smooth_type="FACE",
    )


def main():
    argumentos = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    salida = "/tmp/personaje.fbx"
    personaje = "TupacAmaruII"
    for i, argumento in enumerate(argumentos):
        if argumento == "--salida" and i + 1 < len(argumentos):
            salida = argumentos[i + 1]
        if argumento == "--personaje" and i + 1 < len(argumentos):
            personaje = argumentos[i + 1]

    cuerpo, armadura = construir(personaje)
    exportar(salida, armadura, cuerpo)
    print(f"PERSONAJE_OK {personaje} huesos={len(armadura.data.bones)} "
          f"vertices={len(cuerpo.data.vertices)} caras={len(cuerpo.data.polygons)} -> {salida}")


main()

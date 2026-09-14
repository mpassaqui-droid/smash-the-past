"""
Construye piezas de vestuario para los modelos de Quaternius y las exporta a FBX.

    blender --background --python tools/blender/vestuario.py -- --carpeta carpeta/

Las piezas NO se pintan aqui ni se pesan a varios huesos: cada una es una malla
rigida que en Unity cuelga de UN hueso. Para un poncho sobre un torso de pocos
poligonos eso basta, y evita tener que pintar pesos a mano.

Las medidas salen del esqueleto real del Farmer, medido y no supuesto:
pecho en z 1.377, hombros en z 1.395 a +-0.098, caderas en z 0.999, y el torso
mide +-0.20 en X. El personaje mira hacia -Y.
"""

import bpy
import bmesh
import os
import sys
from mathutils import Vector

# nombre -> (hueso de Unity, [(centro, tamano, material)])
PIEZAS = {
    "TupacAmaruII_poncho": ("Chest", [
        # La tela cae desde encima de los hombros hasta la cadera, mas ancha que
        # el torso para que se lea como una prenda y no como una camiseta.
        ("tela",   (0, -0.055, 1.235), (0.54, 0.34, 0.42)),
        # Franja baja, el detalle que lo hace reconocible de lejos.
        ("franja", (0, -0.055, 1.045), (0.55, 0.35, 0.06)),
        # Cuello: tapa el hueco por donde asoma el hombro.
        ("cuello", (0, -0.050, 1.430), (0.30, 0.26, 0.07)),
    ]),
}


def limpiar():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for coleccion in (bpy.data.meshes, bpy.data.materials):
        for bloque in list(coleccion):
            if bloque.users == 0:
                coleccion.remove(bloque)


def caja(nombre, centro, tamano):
    malla = bpy.data.meshes.new(nombre)
    objeto = bpy.data.objects.new(nombre, malla)
    bpy.context.collection.objects.link(objeto)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x *= tamano[0]
        v.co.y *= tamano[1]
        v.co.z *= tamano[2]
        v.co += Vector(centro)
    bm.to_mesh(malla)
    bm.free()
    return objeto


def construir(nombre, partes):
    limpiar()
    objetos = []
    for sub, centro, tamano in partes:
        o = caja(f"{nombre}_{sub}", centro, tamano)
        o.data.materials.append(bpy.data.materials.new(f"{nombre}_{sub}"))
        bisel = o.modifiers.new("Bisel", "BEVEL")
        bisel.width = 0.01
        bisel.segments = 1
        objetos.append(o)

    bpy.ops.object.select_all(action="DESELECT")
    for o in objetos:
        bpy.context.view_layer.objects.active = o
        bpy.ops.object.modifier_apply(modifier="Bisel")
    for o in objetos:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objetos[0]
    bpy.ops.object.join()
    pieza = bpy.context.view_layer.objects.active
    pieza.name = nombre
    return pieza


def main():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    salida = "/tmp/vestuario"
    for i, a in enumerate(args):
        if a == "--carpeta" and i + 1 < len(args):
            salida = args[i + 1]
    os.makedirs(salida, exist_ok=True)

    # Las mismas opciones que el resto del proyecto: si la unidad declarada no
    # coincide, Unity divide entre 100 y la pieza aparece minuscula.
    ruta = os.path.join(os.path.dirname(os.path.abspath(__file__)), "personaje.py")
    espacio = {}
    exec(compile(open(ruta).read(), ruta, "exec"), espacio)

    for nombre, (hueso, partes) in PIEZAS.items():
        pieza = construir(nombre, partes)
        bpy.ops.object.select_all(action="DESELECT")
        pieza.select_set(True)
        bpy.context.view_layer.objects.active = pieza
        destino = os.path.join(salida, f"{nombre}.fbx")
        opciones = dict(espacio["OPCIONES_FBX"])
        opciones["object_types"] = {"MESH"}
        bpy.ops.export_scene.fbx(filepath=destino, use_selection=True,
                                 bake_anim=False, mesh_smooth_type="FACE", **opciones)
        print(f"PIEZA_OK {nombre} hueso={hueso} vertices={len(pieza.data.vertices)} -> {destino}")

    print("VESTUARIO_LISTO", len(PIEZAS))


main()

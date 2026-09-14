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

# nombre -> (hueso, [(subnombre_color, centro, tamano)])
# El sufijo despues del ultimo "_" es la etiqueta de color que lee VestuarioSetup.
#
# REGLA QUE SALIO DE MIRAR EL JUEGO: esto se ve DE PERFIL. Una pieza plana en el eje
# de profundidad (Y en Blender) se ve de canto y parece una tabla. La primera version
# fallo en los nueve por eso: barbas como planchas, capas como hojas de papel.
#   - capa, tunica o sotana: panyo delante + panyo detras + PUENTE sobre los hombros.
#     Sin el puente se ven dos laminas separadas. El poncho ya funcionaba porque lo tenia.
#   - sombrero: el ala ancha en X *y* en Y, para que se lea desde el lado.
#   - barba: colgando BAJO la barbilla (z 1.45 a 1.61), no pegada delante de la cara.
#   - lo que es mas pequenyo que unos 8 cm no se lee a esta distancia de camara: las
#     gafas de Gandhi salian como una venda blanca, y se han quitado.
#
# Medidas del esqueleto, identico en los nueve: caderas 0.894, pecho 1.377,
# hombros 1.395 a +-0.098, cabeza 1.587, coronilla 1.86. La malla de la cabeza ocupa
# X +-0.18, Y -0.24 a 0.11, Z 1.45 a 1.86. El personaje mira hacia -Y.
PIEZAS = {
    "TupacAmaruII_poncho": ("Chest", [
        ("hombros_rojo",  (0, -0.055, 1.430), (0.44, 0.25, 0.05)),
        ("frente_rojo",   (0, -0.165, 1.215), (0.50, 0.05, 0.45)),
        ("espalda_rojo",  (0,  0.055, 1.215), (0.50, 0.05, 0.45)),
        ("franjaf_oro",   (0, -0.167, 1.030), (0.51, 0.05, 0.06)),
        ("franjae_oro",   (0,  0.057, 1.030), (0.51, 0.05, 0.06)),
    ]),
    # Bicornio puesto de proa a popa: de perfil se ven las dos puntas, que es lo unico
    # que lo distingue de un sombrero cualquiera. Cruzado no se leeria desde el lado.
    "NapoleonBonaparte_bicornio": ("Head", [
        ("ala_negro",     (0, -0.050, 1.870), (0.21, 0.54, 0.045)),
        ("copa_negro",    (0, -0.050, 1.905), (0.17, 0.26, 0.10)),
    ]),
    "AbrahamLincoln_chistera": ("Head", [
        ("ala_negro",     (0, -0.045, 1.865), (0.40, 0.44, 0.035)),
        ("copa_negro",    (0, -0.045, 1.990), (0.26, 0.26, 0.25)),
    ]),
    "AbrahamLincoln_barba": ("Head", [
        ("pelo_marron",   (0, -0.115, 1.545), (0.17, 0.20, 0.21)),
    ]),
    "Socrates_toga": ("Chest", [
        ("hombros_crema", (0, -0.058, 1.430), (0.46, 0.30, 0.06)),
        ("frente_crema",  (0, -0.185, 1.195), (0.52, 0.07, 0.50)),
        ("espalda_crema", (0,  0.070, 1.195), (0.52, 0.07, 0.50)),
    ]),
    "Socrates_barba": ("Head", [
        ("pelo_blanco",   (0, -0.115, 1.545), (0.18, 0.21, 0.24)),
    ]),
    "GenghisKhan_casco": ("Head", [
        ("borde_metal",   (0, -0.045, 1.745), (0.36, 0.40, 0.04)),
        ("cuerpo_metal",  (0, -0.045, 1.835), (0.28, 0.30, 0.17)),
        ("punta_oro",     (0, -0.045, 1.945), (0.05, 0.05, 0.10)),
    ]),
    "Rasputin_sotana": ("Chest", [
        ("hombros_negro", (0, -0.055, 1.430), (0.42, 0.26, 0.05)),
        ("frente_negro",  (0, -0.155, 1.175), (0.45, 0.06, 0.53)),
        ("espalda_negro", (0,  0.045, 1.175), (0.45, 0.06, 0.53)),
    ]),
    "Rasputin_barba": ("Head", [
        ("pelo_negro",    (0, -0.115, 1.535), (0.18, 0.21, 0.26)),
    ]),
    "Gandhi_chal": ("Chest", [
        ("hombros_blanco",(0, -0.055, 1.425), (0.40, 0.27, 0.05)),
        ("frente_blanco", (0, -0.150, 1.260), (0.42, 0.06, 0.34)),
        ("espalda_blanco",(0,  0.045, 1.260), (0.42, 0.06, 0.34)),
    ]),
    # El nemes cae por detras de la cabeza: de perfil es esa masa la que se reconoce.
    "Cleopatra_nemes": ("Head", [
        ("tocado_azul",   (0, -0.040, 1.790), (0.30, 0.34, 0.20)),
        ("caida_azul",    (0,  0.105, 1.620), (0.27, 0.13, 0.28)),
        ("banda_oro",     (0, -0.045, 1.700), (0.31, 0.36, 0.05)),
    ]),
    "Cleopatra_collar": ("Chest", [
        ("collar_oro",    (0, -0.055, 1.430), (0.34, 0.28, 0.07)),
    ]),
    "MiyamotoMusashi_monyo": ("Head", [
        ("nudo_negro",    (0, -0.010, 1.840), (0.10, 0.14, 0.12)),
    ]),
    "MiyamotoMusashi_obi": ("Hips", [
        ("cinturon_negro",(0, -0.055, 1.020), (0.42, 0.32, 0.11)),
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

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
# Medidas tomadas del esqueleto real, identico en los nueve modelos del pack:
# caderas 0.894, pecho 1.377, hombros 1.395 a +-0.098, cabeza 1.587, coronilla 1.86.
# El torso mide +-0.20 en X y el personaje mira hacia -Y.
PIEZAS = {
    # Poncho de verdad: dos panyos que cuelgan por delante y por detras, unidos por
    # encima de los hombros. Antes era un solo bloque y se leia como una tabla.
    "TupacAmaruII_poncho": ("Chest", [
        ("hombros_rojo",  (0, -0.055, 1.430), (0.44, 0.25, 0.05)),
        ("frente_rojo",   (0, -0.165, 1.215), (0.50, 0.05, 0.45)),
        ("espalda_rojo",  (0,  0.055, 1.215), (0.50, 0.05, 0.45)),
        ("franjaf_oro",   (0, -0.167, 1.030), (0.51, 0.05, 0.06)),
        ("franjae_oro",   (0,  0.057, 1.030), (0.51, 0.05, 0.06)),
    ]),
    "NapoleonBonaparte_bicornio": ("Head", [
        ("ala_negro",     (0, -0.040, 1.880), (0.52, 0.17, 0.05)),
        ("copa_negro",    (0, -0.040, 1.915), (0.26, 0.15, 0.09)),
        ("escarapela_oro",(0.10, -0.115, 1.895), (0.07, 0.04, 0.07)),
    ]),
    "AbrahamLincoln_chistera": ("Head", [
        ("ala_negro",     (0, -0.040, 1.865), (0.42, 0.42, 0.03)),
        ("copa_negro",    (0, -0.040, 1.995), (0.25, 0.25, 0.26)),
        ("barba_gris",    (0, -0.160, 1.600), (0.17, 0.11, 0.15)),
    ]),
    "Socrates_toga": ("Chest", [
        ("frente_crema",  (0, -0.155, 1.200), (0.46, 0.06, 0.50)),
        ("espalda_crema", (0,  0.045, 1.200), (0.46, 0.06, 0.50)),
        ("banda_crema",   (0.10, -0.055, 1.440), (0.36, 0.25, 0.08)),
    ]),
    "Socrates_barba": ("Head", [
        ("pelo_blanco",   (0, -0.165, 1.565), (0.19, 0.12, 0.22)),
    ]),
    # El casco envuelve la cabeza, no se posa encima: la coronilla esta en 1.86 y la
    # primera version quedaba centrada en 1.91, flotando como una caja.
    "GenghisKhan_casco": ("Head", [
        ("borde_metal",   (0, -0.040, 1.745), (0.36, 0.36, 0.035)),
        ("cuerpo_metal",  (0, -0.040, 1.835), (0.28, 0.28, 0.17)),
        ("punta_oro",     (0, -0.040, 1.945), (0.05, 0.05, 0.10)),
    ]),
    "Rasputin_sotana": ("Chest", [
        ("frente_negro",  (0, -0.150, 1.180), (0.44, 0.06, 0.54)),
        ("espalda_negro", (0,  0.040, 1.180), (0.44, 0.06, 0.54)),
        ("cruz_oro",      (0, -0.180, 1.300), (0.06, 0.03, 0.16)),
    ]),
    "Rasputin_barba": ("Head", [
        ("pelo_negro",    (0, -0.160, 1.500), (0.20, 0.13, 0.30)),
    ]),
    "Gandhi_gafas": ("Head", [
        ("izq_metal",     (0.070, -0.190, 1.700), (0.09, 0.03, 0.09)),
        ("der_metal",     (-0.070, -0.190, 1.700), (0.09, 0.03, 0.09)),
        ("puente_metal",  (0, -0.190, 1.700), (0.06, 0.02, 0.02)),
    ]),
    "Gandhi_chal": ("Chest", [
        ("banda_blanco",  (0.095, -0.055, 1.380), (0.36, 0.26, 0.10)),
    ]),
    "Cleopatra_nemes": ("Head", [
        ("tocado_azul",   (0, -0.030, 1.805), (0.31, 0.31, 0.22)),
        ("panelizq_oro",  (0.160, -0.100, 1.620), (0.08, 0.21, 0.27)),
        ("panelder_oro",  (-0.160, -0.100, 1.620), (0.08, 0.21, 0.27)),
    ]),
    "Cleopatra_collar": ("Chest", [
        ("collar_oro",    (0, -0.060, 1.435), (0.35, 0.27, 0.07)),
    ]),
    "MiyamotoMusashi_monyo": ("Head", [
        ("nudo_negro",    (0, 0.020, 1.900), (0.09, 0.15, 0.10)),
    ]),
    "MiyamotoMusashi_obi": ("Hips", [
        ("cinturon_negro",(0, -0.055, 1.020), (0.43, 0.31, 0.10)),
        ("nudo_gris",     (0, 0.095, 1.020), (0.12, 0.10, 0.12)),
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

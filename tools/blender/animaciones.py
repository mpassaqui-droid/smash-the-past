"""Crea las animaciones del luchador sobre el esqueleto Mixamo y las renderiza.

    blender --background --python tools/blender/animaciones.py -- \
            --fbx modelo.fbx --carpeta salida/ [--exportar carpeta_fbx/]

Se escriben a mano, no se bajan de ningun sitio: asi cada golpe dura lo que dura en
el juego (el ligero 0,3 s, el fuerte 0,55 s) en vez de tener que recortar un clip
generico. Los angulos van en grados, sobre los huesos de Mixamo.
"""

import bpy
import sys
import math
import os

# accion -> (fotogramas, ciclica, {fotograma: {hueso: (x, y, z) en grados}})
ANIMACIONES = {
    # Desde la T-pose los brazos bajan girando sobre X del propio hueso, los dos con el
    # mismo signo: -78 deja las manos en (+-0.296, 0, 0.941), junto al cuerpo, que es el
    # reposo real del luchador. Sobre Y no se mueven: en Blender el eje Y del hueso corre
    # a lo largo del hueso, asi que girar sobre Y es torcer el brazo sin desplazarlo.
    "idle": (40, True, {
        1:  {"mixamorig:Spine": (2, 0, 0), "mixamorig:LeftArm": (-78, 0, 0), "mixamorig:RightArm": (-78, 0, 0)},
        20: {"mixamorig:Spine": (5, 0, 0), "mixamorig:LeftArm": (-74, 0, 0), "mixamorig:RightArm": (-74, 0, 0)},
        40: {"mixamorig:Spine": (2, 0, 0), "mixamorig:LeftArm": (-78, 0, 0), "mixamorig:RightArm": (-78, 0, 0)},
    }),
    # Los brazos van sobre Z del hueso, no sobre X: desde la T-pose, X sube y baja el
    # brazo y Z lo lleva adelante y atras. Con el mismo valor de Z en los dos brazos
    # salen opuestos, porque el derecho es el espejo del izquierdo.
    # Medido en Blender: Z=-70 pone la mano izquierda en (0,26, -0,20, 0,98), adelante.
    "andar": (24, True, {
        1:  {"mixamorig:LeftUpLeg": (25, 0, 0), "mixamorig:RightUpLeg": (-25, 0, 0),
             "mixamorig:LeftLeg": (-10, 0, 0),
             "mixamorig:LeftArm": (-65, 0, 70), "mixamorig:RightArm": (-65, 0, 70)},
        12: {"mixamorig:LeftUpLeg": (-25, 0, 0), "mixamorig:RightUpLeg": (25, 0, 0),
             "mixamorig:RightLeg": (-10, 0, 0),
             "mixamorig:LeftArm": (-65, 0, -70), "mixamorig:RightArm": (-65, 0, -70)},
        24: {"mixamorig:LeftUpLeg": (25, 0, 0), "mixamorig:RightUpLeg": (-25, 0, 0),
             "mixamorig:LeftLeg": (-10, 0, 0),
             "mixamorig:LeftArm": (-65, 0, 70), "mixamorig:RightArm": (-65, 0, 70)},
    }),
    # Golpe: carga atras (-35, 0, +-70, la mano vuelve a las costillas) y extension
    # (0, 0, -+95, la mano sale a (0,15, -0,51, 1,44), delante y a la altura del hombro).
    # La torsion del tronco va en el sentido que lleva el puno hacia el centro sin
    # perder alcance: Spine1 NEGATIVO para el golpe izquierdo, POSITIVO para el
    # derecho. Al reves el tronco tira del brazo hacia atras (de -0,51 a -0,37).
    # El brazo que no golpea se queda en guardia, y hay que repetir sus angulos en cada
    # clave: crear_accion pone a cero todos los huesos antes de aplicar la pose.
    # Lo que hace que un golpe se lea no son las poses sino el REPARTO DEL TIEMPO:
    # carga lenta, salida en dos fotogramas, y una retencion en la extension antes de
    # volver. La primera version repartia los fotogramas por igual y el punetazo se
    # deslizaba en vez de golpear.
    #   1  guardia
    #   5  carga (4 fotogramas, 0,17 s)
    #   7  extension COMPLETA (2 fotogramas: el golpe sale de golpe)
    #   10 sigue extendido (retencion: aqui es donde se ve que ha impactado)
    #   18 vuelta a la guardia (8 fotogramas, la recuperacion es lo lento)
    "golpe_ligero": (18, False, {
        1:  {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90)},
        5:  {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-35, 0, -70), "mixamorig:Spine1": (0, -16, 0),
             "mixamorig:Hips": (0, -8, 0)},
        7:  {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (0, 0, 95), "mixamorig:Spine1": (0, 22, 0),
             "mixamorig:Hips": (0, 12, 0)},
        10: {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (0, 0, 92), "mixamorig:Spine1": (0, 20, 0),
             "mixamorig:Hips": (0, 11, 0)},
        18: {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90)},
    }),
    # El fuerte es el mismo reparto pero mas marcado: carga mas larga y mas profunda,
    # retencion mas larga, y el cuerpo entero gira detras del brazo.
    "golpe_fuerte": (30, False, {
        1:  {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90)},
        10: {"mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90),
             "mixamorig:LeftArm": (-40, 0, 80), "mixamorig:LeftForeArm": (0, 0, -30),
             "mixamorig:Spine1": (0, 30, 0), "mixamorig:Hips": (0, 16, 0),
             "mixamorig:LeftUpLeg": (-12, 0, 0)},
        13: {"mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90),
             "mixamorig:LeftArm": (0, 0, -95),
             "mixamorig:Spine1": (0, -32, 0), "mixamorig:Hips": (0, -18, 0),
             "mixamorig:RightUpLeg": (-10, 0, 0)},
        18: {"mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90),
             "mixamorig:LeftArm": (0, 0, -90),
             "mixamorig:Spine1": (0, -28, 0), "mixamorig:Hips": (0, -16, 0)},
        30: {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90)},
    }),
    # Guardia: de los brazos caidos del idle a las manos delante de la cara,
    # (+-0,15, -0,36, 1,39), con el codo doblado.
    "guardia": (12, False, {
        1:  {"mixamorig:LeftArm": (-78, 0, 0), "mixamorig:RightArm": (-78, 0, 0)},
        12: {"mixamorig:LeftArm": (-10, 0, -55), "mixamorig:LeftForeArm": (0, 0, -90),
             "mixamorig:RightArm": (-10, 0, 55), "mixamorig:RightForeArm": (0, 0, 90),
             "mixamorig:Spine1": (6, 0, 0)},
    }),
    # Encajar: el retroceso en DOS fotogramas y la recuperacion en catorce. Antes
    # subia y bajaba con el mismo tiempo y parecia que se agachaba, no que le pegaban.
    "encajar": (16, False, {
        1:  {"mixamorig:Spine1": (0, 0, 0), "mixamorig:Head": (0, 0, 0)},
        3:  {"mixamorig:Spine1": (-26, 0, 0), "mixamorig:Head": (-22, 0, 0),
             "mixamorig:LeftArm": (16, 0, 18), "mixamorig:Hips": (-10, 0, 0)},
        6:  {"mixamorig:Spine1": (-18, 0, 0), "mixamorig:Head": (-14, 0, 0),
             "mixamorig:LeftArm": (10, 0, 12), "mixamorig:Hips": (-6, 0, 0)},
        16: {"mixamorig:Spine1": (0, 0, 0), "mixamorig:Head": (0, 0, 0),
             "mixamorig:LeftArm": (0, 0, 6)},
    }),
    "ko": (30, False, {
        1:  {"mixamorig:Spine1": (0, 0, 0), "mixamorig:Hips": (0, 0, 0)},
        10: {"mixamorig:Spine1": (-40, 0, 0), "mixamorig:Head": (-26, 0, 0), "mixamorig:Hips": (-20, 0, 0)},
        30: {"mixamorig:Spine1": (-84, 0, 0), "mixamorig:Head": (-30, 0, 0), "mixamorig:Hips": (-88, 0, 0),
             "mixamorig:LeftUpLeg": (70, 0, 0), "mixamorig:RightUpLeg": (60, 0, 0)},
    }),
}


def armadura_de_la_escena():
    for objeto in bpy.data.objects:
        if objeto.type == "ARMATURE":
            return objeto
    raise RuntimeError("el FBX no trae esqueleto")


def crear_accion(armadura, nombre, largo, ciclica, claves):
    accion = bpy.data.actions.new(f"{nombre}")
    armadura.animation_data_create()
    armadura.animation_data.action = accion

    for hueso in armadura.pose.bones:
        hueso.rotation_mode = "XYZ"
        hueso.rotation_euler = (0, 0, 0)

    for fotograma, poses in sorted(claves.items()):
        bpy.context.scene.frame_set(fotograma)
        # Todos los huesos vuelven a cero en cada clave: si no, una rotacion de una
        # clave anterior se queda pegada y la animacion se deforma sola.
        for hueso in armadura.pose.bones:
            hueso.rotation_euler = (0, 0, 0)
        for nombre_hueso, angulos in poses.items():
            hueso = armadura.pose.bones.get(nombre_hueso)
            if hueso is None:
                continue
            hueso.rotation_euler = tuple(math.radians(a) for a in angulos)
        for hueso in armadura.pose.bones:
            hueso.keyframe_insert(data_path="rotation_euler", frame=fotograma)

    accion.use_frame_range = True
    accion.frame_start = 1
    accion.frame_end = largo
    return accion


def main():
    argumentos = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    carpeta, exportar, base, personaje = "/tmp/anim", None, None, "TupacAmaruII"
    for i, a in enumerate(argumentos):
        if a == "--carpeta" and i + 1 < len(argumentos): carpeta = argumentos[i + 1]
        if a == "--exportar" and i + 1 < len(argumentos): exportar = argumentos[i + 1]
        if a == "--base" and i + 1 < len(argumentos): base = argumentos[i + 1]
        if a == "--personaje" and i + 1 < len(argumentos): personaje = argumentos[i + 1]

    # Se construye el personaje aqui mismo en vez de importar su FBX: reimportar y
    # volver a exportar reaplica la conversion de ejes de Blender.
    ruta_constructor = os.path.join(os.path.dirname(os.path.abspath(__file__)), "personaje.py")
    espacio = {}
    exec(compile(open(ruta_constructor).read(), ruta_constructor, "exec"), espacio)
    cuerpo, armadura = espacio["construir"](personaje)

    if base:
        espacio["exportar"](base, armadura, cuerpo)
        print(f"BASE_OK {base}")

    os.makedirs(carpeta, exist_ok=True)
    if exportar: os.makedirs(exportar, exist_ok=True)

    for nombre, (largo, ciclica, claves) in ANIMACIONES.items():
        crear_accion(armadura, nombre, largo, ciclica, claves)

        if exportar:
            # El exportador FBX hornea la plage de la ESCENA, no la de la accion: sin
            # esto todos los clips salian de 10 segundos y un golpe ligero no sirve.
            bpy.context.scene.frame_start = 1
            bpy.context.scene.frame_end = largo

            # Convencion de Unity: modelo@animacion.fbx se importa como clip suelto.
            bpy.ops.object.select_all(action="DESELECT")
            for objeto in bpy.data.objects:
                if objeto.type in {"ARMATURE", "MESH"}:
                    objeto.select_set(True)
            bpy.context.view_layer.objects.active = armadura
            bpy.ops.export_scene.fbx(
                filepath=os.path.join(exportar, f"{personaje}@{nombre}.fbx"),
                use_selection=True, bake_anim=True,
                bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False,
                **espacio["OPCIONES_FBX"],
            )
        print(f"ANIMACION_OK {nombre} fotogramas={largo}")

    print("ANIMACIONES_LISTAS", len(ANIMACIONES))


main()

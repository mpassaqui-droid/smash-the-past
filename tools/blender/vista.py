"""Renderiza un FBX a PNG para poder mirarlo sin abrir Blender ni Unity.

    blender --background --python tools/blender/vista.py -- --fbx x.fbx --salida x.png
"""

import bpy
import sys
import math
from mathutils import Vector


def limpiar():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def luz(nombre, energia, color, posicion, objetivo=(0, 0, 1)):
    datos = bpy.data.lights.new(nombre, type="AREA")
    datos.energy = energia
    datos.color = color
    datos.size = 3.0
    objeto = bpy.data.objects.new(nombre, datos)
    bpy.context.collection.objects.link(objeto)
    objeto.location = posicion
    direccion = Vector(objetivo) - Vector(posicion)
    objeto.rotation_euler = direccion.to_track_quat("-Z", "Y").to_euler()


def main():
    argumentos = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    fbx, salida = None, "/tmp/vista.png"
    for i, argumento in enumerate(argumentos):
        if argumento == "--fbx" and i + 1 < len(argumentos):
            fbx = argumentos[i + 1]
        if argumento == "--salida" and i + 1 < len(argumentos):
            salida = argumentos[i + 1]

    limpiar()
    bpy.ops.import_scene.fbx(filepath=fbx)

    luz("clave", 320, (1.0, 0.95, 0.85), (2.4, -2.6, 3.0))
    luz("relleno", 90, (0.65, 0.78, 1.0), (-2.8, -1.6, 1.6))
    luz("contra", 160, (1.0, 0.9, 0.8), (0.4, 3.2, 2.4))

    camara_datos = bpy.data.cameras.new("camara")
    camara_datos.lens = 70
    camara = bpy.data.objects.new("camara", camara_datos)
    bpy.context.collection.objects.link(camara)
    camara.location = (0.9, -3.6, 1.35)
    direccion = Vector((0, 0, 1.0)) - Vector(camara.location)
    camara.rotation_euler = direccion.to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.camera = camara

    mundo = bpy.data.worlds.new("mundo")
    mundo.use_nodes = True
    mundo.node_tree.nodes["Background"].inputs[0].default_value = (0.05, 0.06, 0.08, 1)
    bpy.context.scene.world = mundo

    escena = bpy.context.scene
    escena.render.engine = "BLENDER_EEVEE"
    escena.render.resolution_x = 900
    escena.render.resolution_y = 1100
    escena.render.filepath = salida
    escena.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)
    print(f"VISTA_OK {salida}")


main()

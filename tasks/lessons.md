# Lecciones de validacion

- Un tema TSS usa una referencia de importador (tipo 3 y fileID generado), no la referencia tipo 2 de un ScriptableObject. Obtener la referencia con AssetDatabase y comprobarla en una importacion limpia antes de atribuir el fallo a Unity.
- Separar las pruebas del ciclo de combate y las de input nativo: verificar comandos y estado en la suite y teclado en el player enfocado. Un evento de teclado sintetico en el runner EditMode no demuestra recepcion del input nativo.
- Un build compilado y los tests de datos no prueban la interfaz. Verificar seleccion, pausa, KO y revancha en el player antes de marcar el flujo completo.

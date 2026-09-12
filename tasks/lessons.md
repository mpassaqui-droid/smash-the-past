# Lecciones de validacion

- Un tema TSS usa una referencia de importador (tipo 3 y fileID generado), no la referencia tipo 2 de un ScriptableObject. Obtener la referencia con AssetDatabase y comprobarla en una importacion limpia antes de atribuir el fallo a Unity.
- Separar las pruebas del ciclo de combate y las de input nativo: verificar comandos y estado en la suite y teclado en el player enfocado. Un evento de teclado sintetico en el runner EditMode no demuestra recepcion del input nativo.
- Un build compilado y los tests de datos no prueban la interfaz. Verificar seleccion, pausa, KO y revancha en el player antes de marcar el flujo completo.
- Si Munay pide completar el juego de punta a punta, continuar de forma autonoma sin pedir aprobaciones de alcance entre piezas. Solo detenerse ante un bloqueo externo real o una accion irreversible.
- Una escena reutilizable puede cubrir varios escenarios tematicos si la seleccion activa un set visual verificable, sin duplicar la logica de combate ni el suelo fisico.
- Las pruebas EditMode no deben llamar flujos de runtime que usan Destroy; probar el controlador con referencias serializadas evita warnings y mantiene la comprobacion del comportamiento.
- Un entitlement de tienda debe fallar cerrado cuando falta Steamworks o el SKU, nunca habilitar un DLC desde un fallback local en el build de release.

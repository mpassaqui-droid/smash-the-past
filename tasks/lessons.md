# Lecciones de validacion

- Correccion de Munay: calidad visual, sonido real y movimientos accesibles no se demuestran con el numero de tests. Probar golpes contra cada defensa especial y cargar/activar specials desde input antes de declarar completo el combate; una llamada directa a ReceiveDamage no cubre HitDetector.
- Para probar un Update concreto no usar SendMessage: alcanza otros componentes del GameObject, incluidos controladores deshabilitados. Invocar solo el metodo bajo prueba; InputTestFixture.Press en UnityTest requiere procesar su cola antes de comprobar el estado.
- En EditMode con EnterPlayMode, usar el patron WaitFrames del proyecto para esperar tiempo de gameplay. No asumir que yield WaitForSeconds se ejecuta como en una coroutine MonoBehaviour: comprobar frames/tiempo acumulado antes de diagnosticar un special que no termina.
- Si un componente externo asume el polling de input combinado, el controlador original debe quedar desactivado tambien desde MatchController al iniciar la pelea. Dejarlo activo permite ataques duplicados por frame aunque otro componente lo apague despues.

- Un tema TSS usa una referencia de importador (tipo 3 y fileID generado), no la referencia tipo 2 de un ScriptableObject. Obtener la referencia con AssetDatabase y comprobarla en una importacion limpia antes de atribuir el fallo a Unity.
- Separar las pruebas del ciclo de combate y las de input nativo: verificar comandos y estado en la suite y teclado en el player enfocado. Un evento de teclado sintetico en el runner EditMode no demuestra recepcion del input nativo.
- Un build compilado y los tests de datos no prueban la interfaz. Verificar seleccion, pausa, KO y revancha en el player antes de marcar el flujo completo.
- Si Munay pide completar el juego de punta a punta, continuar de forma autonoma sin pedir aprobaciones de alcance entre piezas. Solo detenerse ante un bloqueo externo real o una accion irreversible.
- Una escena reutilizable puede cubrir varios escenarios tematicos si la seleccion activa un set visual verificable, sin duplicar la logica de combate ni el suelo fisico.
- Las pruebas EditMode no deben llamar flujos de runtime que usan Destroy; probar el controlador con referencias serializadas evita warnings y mantiene la comprobacion del comportamiento.
- Un entitlement de tienda debe fallar cerrado cuando falta Steamworks o el SKU, nunca habilitar un DLC desde un fallback local en el build de release.

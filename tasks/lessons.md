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

- Los tests EditMode corren dentro del editor, donde un ajuste de ProjectSettings puede estar arreglado solo en la copia local sin commitear. `activeInputHandler` estuvo en 0 en el repo desde el primer commit mientras cinco scripts usaban el nuevo Input System: suite en verde y builds de CI sin input. Cualquier ajuste del proyecto del que dependa el runtime necesita un test que lea el valor persistido en `ProjectSettings.asset`, no el estado del editor.
- Un test guardian no vale nada hasta verlo fallar. Para probarlo: rama temporal desde el commit roto con solo el test encima, PR en draft para disparar CI, comprobar el fallo (68/70) y cerrar. Sin eso es una afirmacion, no una prueba.
- Un asset seguido por git sin su `.meta` commiteado recibe un GUID nuevo en cada clon. Se detecta en el log de checkout de CI ("Removing ... .meta") y rompe referencias serializadas en la maquina de otro.

- Con un SRP activo (URP), `Camera.Render()` no pasa por el pipeline: dibuja por la ruta antigua y la captura sale gris y plana aunque los materiales esten bien. Para renderizar fuera de juego hay que usar `RenderPipeline.SubmitRenderRequest` con `UniversalRenderPipeline.SingleCameraRequest`. Una captura que parece rota puede ser la herramienta de captura, no la escena: antes de tocar luces, comprobar que el render usa el pipeline.
- Cambiar `material.shader` a URP/Lit deja dentro las propiedades del shader viejo. Con esa basura el SRP Batcher agrupa mal y pinta toda la escena con las constantes de un solo material (salia todo dorado). Hay que reescribir el material entero con `EditorUtility.CopySerialized` desde uno nuevo, que ademas mantiene el GUID y las referencias.
- Mirar la imagen no basta: medir. Sacar la saturacion maxima del PNG dijo en un segundo que no habia ni un pixel rojo en toda la escena, cuando el poncho es (0.8, 0.16, 0.17). Esa medida separo "esta mal iluminado" de "el albedo no llega".
- Un test que parece estorbar suele estar protegiendo algo: mover el suelo para mejorar el encuadre cambiaba el suelo de colision. La solucion no es relajar el test sino separar lo visual de lo fisico.

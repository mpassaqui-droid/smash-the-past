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

- Un MonoBehaviour tiene que vivir en un archivo con su mismo nombre. `DlcEntitlementService` estaba dentro de `DlcEntitlement.cs`, asi que Unity no generaba su MonoScript y el componente de la escena guardaba un `m_Script` sin guid. El player moria al arrancar con "level0 is corrupted" y el build de CI no lo veia porque solo comprueba que la compilacion termine.
- Construir no es arrancar. El job de CI daba verde con un player que no abria. Un build solo demuestra que compila; hace falta lanzarlo y leer su Player.log (`~/Library/Logs/<compania>/<producto>/Player.log`).
- Una prueba puede certificar el fallo. `PrototypeSceneWiresFailClosedDlcService` buscaba la cadena `m_ClassName: DlcEntitlementService` en el YAML, texto que Unity solo escribe cuando NO resuelve el script. Verde equivalia a "el componente sigue roto". Probar el objeto cargado, nunca el texto serializado.
- Antes de guardar una escena desde batchmode, comprobar que ningun GameObject tiene componentes nulos. Guardar con un script sin resolver escribe una escena que revienta en el player.

- Ajustar la camara en la escena no sirve de nada si un script la reescribe en LateUpdate. `ArenaCamera` fijaba posicion, rotacion y tamano cada fotograma: todo el encuadre ajustado en el editor moria al arrancar el juego. Antes de tocar valores guardados, buscar quien los escribe en ejecucion.
- El player se congela sin foco. Una prueba automatica que espera fotogramas necesita `Application.runInBackground = true`, si no se queda colgada sin decir por que.

- `AudioSource.PlayOneShot` comparte el tono de su fuente: con una sola fuente, todos los golpes seguidos suenan identicos por mucho que se cambie el volumen. Hacen falta varias voces en rotacion para variar el tono de verdad.
- En un juego de pelea la sensacion de impacto es tiempo, no detalle: la parada de fotograma y el destello pesan mas que el numero de poligonos. Y cualquier codigo que toque `Time.timeScale` tiene que respetar la pausa del jugador y deshacerse al apagarse, o deja el juego congelado.

- Un efecto que no se ve no siempre esta roto. El contorno parecia no dibujarse: subiendo el grosor a 0.2 se vio un halo enorme, lo que probo que la pasada funcionaba y que solo era demasiado fino. Probar con un valor absurdo separa "no funciona" de "no se nota" en un solo intento.
- En zsh una variable sin comillas NO se parte en palabras como en bash. `$EXTRA` con "-golpe 1" dentro llega al programa como un unico argumento y la bandera se ignora en silencio. Si una bandera no hace nada, imprimir los argumentos recibidos antes de tocar el codigo.

- Suscribirse a un evento con una lambda que captura una variable local DENTRO de un `UnityTest` con `EnterPlayMode` revienta con NullReferenceException: el iterador y su cierre no sobreviven igual al cambio de dominio. Comprobar el comportamiento por sus efectos (vida, aturdimiento) en vez de por un flag capturado.

- Una regla nueva de combate vuelve aleatorias las pruebas de input si esas pruebas dejan que los golpes conecten: el aturdimiento del que encaja falsea la pulsacion siguiente. Los sondeos de `CombatInputTests` limpian el aturdimiento antes de cada llamada, porque esas pruebas miden a quien obedece cada tecla, no el combate.
- Tres banderas `wasPressedThisFrame` vivas en el mismo tick del Input System hacen una prueba aleatoria: unas veces se leen todas y otras solo la primera. Una pulsacion por fotograma de input, con su `InputSystem.Update()` y su sondeo.

- Un arreglo sin commitear se pierde al hacer `git checkout <rama> -- archivo` para volver de una prueba: el checkout trae la version COMMITEADA, no la del disco. El mensaje del commit anuncio el arreglo del ParticleSystem y el codigo no lo tenia; solo se vio porque la prueba volvio a fallar. Commitear antes de tocar el arbol para experimentar.

- Las pruebas de `CombatInputTests` fallaban una de cada tres corridas y nunca la misma. La causa era el bucle del reproductor: procesa eventos de input por su cuenta entre el `Press` y el sondeo, y "pulsado este fotograma" se apagaba antes de que la prueba lo leyera. Se arregla poniendo `InputSystem.settings.updateMode = ProcessEventsManually` en el arranque de la prueba y restaurandolo al final. Efecto secundario: leer el stick de un mando parado en modo manual saca un error interno del paquete, que se ignora solo en la unica prueba que toca ejes. Comprobado con tres corridas completas seguidas: 100/100, 100/100, 100/100.
- `[Retry(3)]` no sirve para `[UnityTest]`: compila y no reintenta nada. Si una prueba de corrutina es intermitente, hay que arreglar la causa.

- Construir un personaje por script es medir tramos, no elegir numeros bonitos. La primera version salio desmontada: la cabeza flotaba sobre el cuello y el sombrero sobre la cabeza, porque cada pieza tenia su centro y su tamano sin comprobar que tocara la siguiente. Se arregla escribiendo los tramos de arriba abajo con un centimetro de solape. Renderizar a PNG antes de importar cuesta veinte segundos y lo enseña.

- El exportador FBX de Blender hornea la plage de fotogramas de la ESCENA, no la de la accion. Sin fijar `scene.frame_start` y `frame_end` antes de cada exportacion, los siete clips salieron de 10,38 segundos cada uno: un golpe ligero de diez segundos no sirve para nada y Unity los importa sin quejarse. Comprobar siempre la duracion que reporta Unity, no solo que el clip exista.
- En Blender el giro de un hueso va sobre sus ejes locales: para un hueso que apunta a lo largo del cuerpo, X es inclinarse adelante y atras, Y es girar sobre si mismo. Puse el giro del torso en Z y el punetazo salio como una reverencia. Renderizar una tira de seis fotogramas lo enseño de un vistazo.

- Dos exportaciones del mismo esqueleto tienen que declarar la MISMA unidad. `personaje.py` exportaba con `apply_scale_options="FBX_SCALE_ALL"` y `animaciones.py` sin ese argumento, asi que el cuerpo salia con `UnitScaleFactor=100` ("una unidad = un metro") y los siete clips con `UnitScaleFactor=1` ("una unidad = un centimetro"), con las MISMAS coordenadas en los dos ficheros. Unity hace caso a la cabecera: en cuanto empezaba una animacion dividia entre 100 cada hueso, la cadera pasaba de 1,000 a 0,010 y el luchador se encogia a dos centimetros. En Humanoid el mismo desajuste salia como una malla flotando a siete metros. Blender no lo enseña porque su importador lee la cabecera y compensa: el ida y vuelta parecia perfecto. Las opciones de exportacion viven ahora en `OPCIONES_FBX`, una sola copia que `animaciones.py` importa de `personaje.py`.

- Comparar la cabecera de los ficheros antes de acusar al modelo. Cuatro pistas se descartaron a mano (animacion, ida y vuelta de Blender, pose A contra T, bounds de la malla) cuando bastaba con leer `UnitScaleFactor` en los ocho FBX y ver que dos valores distintos convivian. Medir el hueso en Unity (`clip.SampleAnimation` sobre el objeto que lleva el Animator, no sobre la raiz del prefab) da el numero exacto del fallo: 1,000 -> 0,010 es un factor 100 redondo, no un problema de pose.

- Cambiar el esqueleto obliga a reescribir los clips. Al pasar el reposo a T-pose los brazos quedaron apuntando a lo largo de X, asi que el eje local Y de `LeftArm` pasa a correr a lo largo del brazo: girar sobre Y ya no baja el brazo, lo retuerce sin moverlo. El `idle` pedia (0, -78, 0) y las manos se quedaban clavadas en la T-pose. Sobre X si bajan, las dos con el mismo signo: -78 las deja en (+-0,296, 0, 0,941). Medir la posicion de la mano en Blender antes de dar el angulo por bueno.

- Los clips en **Generic** se atan a la ruta de cada hueso: solo valen para ESE esqueleto. Para poder cambiar de personaje (Mixamo, un modelo comprado, cualquier cosa) tienen que ser **Humanoid**, que pasa por el espacio de musculos del avatar. Comprobado con Cameron, el humanoide de los ejemplos de Cinemachine: los siete clips salen con `isHumanMotion=True` y mueven un esqueleto que no tiene ni los mismos nombres de hueso ni las mismas proporciones (0,958 m contra 2,05), con los pies en el suelo.

- Copiar un FBX de un paquete sin su `.meta` deja que Unity adivine el mapeo del esqueleto y los materiales. La cabeza de Cameron salio flotando por encima del cuerpo y la malla en magenta. Se ve que la culpa era de la copia y no del retarget renderizando la pose de REPOSO, sin ningun clip aplicado: la cabeza flotaba igual. Antes de acusar a la tuberia, quitar de en medio el propio montaje de la prueba.

- Reescribir una animacion se hace buscando el angulo, no eligiendolo. Un script de Blender que barre X y Z del hueso y se queda con el par que deja la mano mas cerca de un objetivo dio los valores en un minuto: golpe extendido = Arm (0, 0, -95) a la izquierda y (0, 0, +95) a la derecha, mano en (+-0,15, -0,51, 1,44). A ojo habrian hecho falta diez idas y vueltas a Unity.

- El buscador puntuaba SOLO la mano y saco poses con el codo en cualquier sitio: dos soluciones con la misma mano y el brazo completamente distinto, y encima asimetricas entre izquierda y derecha. Se arregla puntuando mano Y codo a la vez, y forzando el espejo (la derecha es la izquierda con la Z cambiada de signo) en vez de buscar cada lado por separado.

- Al reescribir los clips reutilice los angulos de tronco del clip viejo sin volver a comprobarlos, y la torsion tiraba del brazo hacia atras: el puno perdia alcance (-0,51 a -0,37) en vez de ganarlo. El sentido correcto es Spine1 NEGATIVO para el golpe izquierdo y POSITIVO para el derecho. Cambiar de esqueleto invalida TODOS los angulos del clip, tambien los que parecen no tener que ver con el brazo.

- Cambiar la malla de un luchador no toca su logica: el prefab tiene un hijo "Modelo" con el Animator y el controlador, y basta con reemplazar lo que cuelga de el y poner el avatar nuevo. Funciona porque los clips son Humanoid. Dos cosas que hay que acordarse: el FBX importado trae su PROPIO Animator, que hay que quitar para que no haya dos, y `FighterVisual` busca los Renderer en tiempo de ejecucion, asi que se adapta solo. `CambiarModelo.cs` guarda el mapa personaje -> modelo, una linea por luchador.

- `Camera.fieldOfView` es el VERTICAL. Calcular la distancia de encuadre con el sin pasar por el aspecto deja la camara lejisimos: en una plancha de 1800x620 salieron once hormigas. dist = (ancho/2) / (tan(fov/2) * aspecto).

- Un modelo simetrico esconde los errores de orientacion. Los luchadores llevaban desde siempre la espalda vuelta a la camara y NUNCA se giraban el uno hacia el otro; con un personaje de cajas casi simetrico no se notaba, y se vio en cuanto el modelo tuvo cara. El giro estaba en `FighterVisual`, que ademas no se ejecutaba: su primera linea era `if (model == null ...) return;` y las cinco referencias del prefab estaban vacias, asi que tambien se perdia el destello blanco al encajar un golpe. Un componente que sale por la primera linea no falla, no avisa, y parece que funciona.

- El modelo nace mirando a +Z y la camara del combate tambien mira a +Z: dejarlo a 0 grados es ensenarle la espalda al jugador. De perfil (+-90) ademas los punetazos salen hacia el rival y no hacia el fondo.

- **La subida por navegador de GitHub no sabe manejar Git LFS.** No lo convierte, no lo avisa antes: rechaza el archivo con "must be pushed using the Git command line with Git LFS installed". La documentacion de GitHub no lo dice en ninguna parte, ni a favor ni en contra, y escribi una guia entera para Diego dando por hecho que funcionaria. Se descubrio cuando el lo intento. Para un repo con LFS, la web solo sirve para los archivos de texto que no estan en `.gitattributes`.

- Una guia escrita para otra persona hay que probarla antes de mandarsela, o decir claramente que parte no esta probada. La mia le hizo perder un viaje.

- `UnityEngine.Object` sobrecarga `!=` para dar falso en objetos destruidos. Guardar la referencia, llamar a `PrefabUtility.UnloadPrefabContents` y comprobar `referencia != null` despues devuelve SIEMPRE falso, aunque todo haya salido bien. La funcion daba error en cada prefab sin decir por que. Calcular el booleano ANTES de descargar.

- Los nueve personajes de cajas nunca tuvieron Animator ni `AnimadorDeLuchador`: se movian a mano desde `FighterVisual`. Al darles un esqueleto no basta con cambiar la malla, hay que anadirles las dos piezas. El error de Unity lo dijo claro (`There is no Animator attached to the "Modelo" game object`) pero solo salio al llegar al primer personaje que no era Tupac, tres pasadas despues.

- Cambiar algo en los nueve prefabs se hace reversible ANTES de tocarlos, no despues. `CambiarModelo` guarda una copia intacta en `Prefabs/Anteriores/` la primera vez que toca cada prefab, y `Volver` las restituye. La copia se hace solo si no existe ya: si no, la segunda pasada guardaria lo nuevo encima de lo viejo y no habria vuelta atras.

- Colgar una malla de un hueso obliga a anular las TRES cosas que hereda, no dos. Posicion y rotacion no bastan: los FBX de Quaternius llevan un factor 100 en la raiz, y el poncho aparecio a y = -5,50 hasta que se compenso tambien la escala con `1 / destino.lossyScale`.

- Y la rotacion no se pone a identidad, se copia la del modelo importado. Un FBX exportado de Blender llega con su rotacion de conversion de ejes (Z arriba a Y arriba) en la raiz; ponerla a identidad la anula y la pieza queda tumbada, con el centro en z en vez de en y. Se vio en una sola cifra: centro (0, -0,05, 1,24) en vez de (0, 1,24, 0,05).

- `personaje.py` se ejecuta solo si ve `--salida` en `sys.argv`, y `vestuario.py` lo importa con exec. Como vestuario.py usaba el mismo nombre de argumento, personaje.py arrancaba su propio main y trataba de exportar a una carpeta. Un script que se auto-ejecuta segun argv contamina a cualquiera que lo importe: el argumento del que importa tiene que llamarse distinto.

- Una escena de prueba vacia no tiene el pipeline URP, asi que todos los materiales salen blancos. Dos planchas seguidas parecieron decir que el color no se habia aplicado, y estaba aplicado: se veia en la captura del player. Para juzgar color hay que mirar el juego, no el banco de pruebas.

- Un gris claro no tiene color propio: toma el de la luz. El casco de Genghis salia rosa y lo achaque a "shader que URP no sabe dibujar", que es el rosa tipico. Era falso: al abrir el .mat, el shader era el MISMO que el de las piezas que si se veian bien y el valor era el gris pedido (0,42 0,44 0,48). Lo que pasa es que el lado derecho de la arena tiene luz calida, y se ve hasta en el suelo, que tambien sale rosado. Se arregla dando a la pieza un color propio y oscuro, no tocando el pipeline. Leer el archivo antes de acusar al motor.

- Las piezas de vestuario no clonan el material que trae su FBX sino uno del propio personaje, que ya sabemos que se dibuja. Asi no hay que adivinar el nombre del shader ni depender de `Shader.Find`, que en batch puede no encontrarlo.

- Esto se ve DE PERFIL, y lo primero que hice fueron piezas planas en el eje de profundidad: se veian de canto y parecian tablas. La barba de Rasputin era una plancha delante de la cara, la sotana dos hojas de papel separadas, el bicornio un monton de cubos. El poncho funcionaba desde el principio y ninguno de los otros, por una sola razon: tenia un PUENTE sobre los hombros que cierra la silueta entre los dos panyos. Toda capa, tunica o sotana necesita ese puente.

- Un sombrero de un juego de perfil lleva el ala ancha tambien en profundidad, no solo a los lados. El bicornio de Napoleon puesto cruzado (como se llevaba) no se leia; puesto de proa a popa se ven las dos puntas y se reconoce al instante.

- Lo que mide menos de unos 8 cm no se lee a esta distancia de camara. Las gafas de Gandhi salian como una venda blanca cruzada en la cara. Se quitaron: a esta escala manda la silueta, no el detalle.

- Y hay que verlo, no deducirlo. Puse catorce piezas con medidas correctas del esqueleto, comprobe dos personajes en el juego y di por buenos los otros siete. Estaban mal los siete. Solo se vio cuando se arreglo `-escenario`, que decia en su propio comentario que elegia el luchador y no lo hacia: `-pelea` llamaba despues a `BeginMatch(0, 1)` con los indices escritos a mano y pisaba la eleccion.

- Quitar un zoom de un video obliga a quedarse con el encuadre MAS CERRADO, porque los fotogramas ampliados ya no tienen los bordes. Al aplicarlo sobre los 8 s enteros (21 % de zoom) el titulo salia cortado, porque al final ya lo estaba. Se arregla usando una ventana mas corta: con 4 s el zoom es del 10 % y el titulo entra completo. Medir el zoom antes: una busqueda de escala sobre una ventana central dio escala(t) = 0,998 + 0,0264 t.

- Para que un video enlace, el fundido va al PRINCIPIO, no al final: la salida empieza con la cola del original fundiendose hacia su propia cabeza, y asi el ultimo fotograma coincide con el primero. Puesto al reves el bucle saltaba un segundo. Medido: la costura paso de 12,8 a 5,9 sobre 255.

- Dos componentes en el mismo objeto no tienen orden de Awake garantizado. `TituloDelMenu` creaba su RenderTexture en Awake y `PrototypeUI` se la pedia antes: la encontraba vacia y caia al titulo escrito, sin error ninguno. La inicializacion tiene que ser perezosa, en la propiedad.

- El menu principal se muestra estando ya en fase `Selection`, asi que al darle musica propia al titulo sonaban DOS pistas a la vez: la del titulo y la que `MusicaDelJuego` pone para la seleccion. No se ve en ninguna captura, solo se oye, y lo canto Munay. `MusicaDelJuego.TituloEnPantalla` la calla mientras el titulo esta a la vista.

- Para comprobarlo hay que sacar por escrito que suena: `PruebaDeHumo` ahora lista los AudioSource activos con su clip, una vez en el menu y otra con el combate corriendo. La primera version del sondeo estaba colocada ANTES de que empezara el combate, asi que las dos medidas daban lo mismo y parecia que el arreglo no funcionaba. Un sondeo mal situado miente igual que no tenerlo.

- Un `.fbx` recien copiado entra con los ajustes por defecto: sin avatar humano. `CambiarModelo` fallaba con "no es un humanoide valido" y la culpa no era del modelo sino de que nadie le habia puesto los ajustes. Ahora el propio script se los pone si hacen falta, asi que anadir un personaje nuevo es copiar el fichero y anadir su linea al mapa.

- El pack de mujeres de Quaternius comparte Chest (1,377) y Head (1,587) con el de hombres, que es donde van casi todas las piezas de vestuario; solo cambia la cadera (1,002 contra 0,894). Conviene comprobarlo hueso a hueso antes de dar por bueno que una pieza vale para los dos packs.

- Una pieza que ENVUELVE la cabeza en vez de posarse encima se lee como un casco cuadrado. El nemes de Cleopatra medida 0,41 de alto y se comia la cara; a 0,13 se apoya en la coronilla y el rostro se ve. La cifra que hay que mirar es el alto de la pieza contra el alto de la cabeza (1,45 a 1,86).

- Diego pidio "boton de saltar y para cubrirse" y los dos existian desde siempre: W y L para el jugador 1, flecha arriba y Pad 3 para el 2, o los botones de abajo y de la izquierda del mando. Lo que faltaba no era la funcion sino decirla en algun sitio. Antes de implementar lo que pide un tester, comprobar si ya esta y solo es invisible.

- Ni `PlayerInputController` ni la IA miraban la fase del combate, asi que la maquina se movia y atacaba durante la seleccion de personaje. La guarda va en `Fighter`, que es por donde pasan todos: el jugador, la maquina y las pruebas.

- Y esa guarda tiene que empezar ABIERTA. Puesta en falso por defecto tumbo veinte pruebas que montan un luchador suelto sin MatchController al lado. Una barrera nueva se cierra solo cuando alguien lo pide expresamente, no por omision.

- La sombra larga no venia de las tres luces sino de una: solo `LuzClave` proyecta. La primera pasada subio las tres a 62 grados y se cargo el relleno y el contraluz, que tienen sus angulos por una razon. Mirar cual proyecta antes de tocar ninguna: a 48 grados un cuerpo de 2 m da 1,80 m de sombra, a 62 da 1,06.

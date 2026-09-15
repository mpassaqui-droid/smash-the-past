# Historial

## v0.7.0-alpha - 15/09/2026

Las arenas dejan de parecer un prototipo.

- Post proceso y oclusion ambiental encendidos: llevaban apagados desde el
  principio porque `postProcessData` del renderizador estaba a nulo, y URP se
  salta todo el post proceso sin avisar.
- Texturas en los nueve materiales del escenario, que eran color plano.
- 120 espectadores en las gradas.
- Parallax en el fondo. La camara es ortografica, asi que no habia ninguno.

## v0.6.0-alpha - 15/09/2026

- Los golpes golpean: carga lenta, salida en dos fotogramas y retencion en la
  extension. El pico de movimiento de la mano es 4,2 veces la media en el
  ligero y 7,0 en el fuerte.
- Encajar un golpe tira la cabeza atras en dos fotogramas, no en ocho.
- Las piezas de vestuario dejan de ser cajas de aristas vivas.
- Sonido al parar un golpe con la guardia y al empezar cada asalto.

## v0.5.0-alpha - 15/09/2026

Primera tanda de retornos de Diego.

- La maquina ya no se mueve ni ataca durante la seleccion de personaje.
- Fuera los sonidos al pasar por encima de los botones y al pulsarlos.
- Sombra mas corta: un cuerpo de 2 m pasa de 1,80 m de sombra a 1,06.
- La lista de CONTROLES esta en Opciones. Saltar y cubrirse existian desde
  siempre, lo que faltaba era decirlo en algun sitio.

## v0.4.0-alpha - 15/09/2026

- Cleopatra pasa a un modelo femenino (Ultimate Modular Women Pack de
  Quaternius, tambien CC0). Era el unico personaje claramente equivocado.
- Su nemes se posa sobre la cabeza en vez de envolverla.
- Las arenas dejan de sonar con la musica de la seleccion de personaje.

## v0.3.0-alpha - 15/09/2026

Pantalla de titulo.

- El menu principal lleva de fondo la carta de titulo animada, en bucle y sin
  el zoom que traia el original.
- Su musica, preparada para enlazar: 78 compases justos, sin el hueco que
  dejaban los fundidos de entrada y salida.
- Dos sonidos de interfaz, al pasar por encima de un boton y al pulsarlo.

## v0.2.0-alpha - 14/09/2026

Los nueve luchadores dejan de ser cajas, y las animaciones por fin llegan al juego.

### Personajes
- Los nueve pasan a modelos humanoides de Quaternius (CC0, uso comercial libre), cada
  uno con su paleta: 146 materiales, para que dos personajes puedan compartir modelo sin
  parecerse.
- Catorce piezas de vestuario modeladas en Blender y colgadas del hueso que les toca:
  el poncho de Tupac, el casco de Genghis, el bicornio de Napoleon, la chistera de
  Lincoln, el nemes de Cleopatra, las barbas de Rasputin y Socrates, la toga, la sotana,
  el chal de Gandhi, el monyo y el obi de Musashi.
- Es reversible: `CambiarModelo.Volver` restituye los prefabs anteriores.

### Animacion
- Arreglado el fallo que lo bloqueaba todo: el FBX del cuerpo declaraba una unidad y los
  clips otra, con las mismas coordenadas, asi que Unity dividia entre 100 cada hueso en
  cuanto empezaba una animacion.
- Los siete clips pasan a Humanoid, asi que valen sobre cualquier esqueleto que Unity
  acepte como humano.
- Reescritos `andar`, `guardia` y los dos golpes para la T-pose: los punetazos salian
  hacia la cadera en vez de hacia delante.
- `FighterVisual` no se ejecutaba. Ahora los luchadores se giran hacia su rival y vuelve
  el destello blanco al encajar.

### Sonido
- Musica y dos efectos de Diego: su bucle en la pantalla de seleccion y en las arenas,
  su sonido de atacar y el de encajar.

### Herramientas
- `-escenario N` elegia el luchador segun su propio comentario, pero `-pelea` pisaba la
  eleccion con indices escritos a mano. Arreglado: se puede mirar cualquiera de los
  nueve desde la linea de ordenes.

## v0.1.0-alpha - 13/09/2026

Primera version jugable, distribuida para Windows y macOS.

### Combate
- Aturdimiento tras encajar un golpe (0,25 s, 0,38 s en golpe fuerte) y block stun corto
  (0,12 s). Es lo que da consecuencia a acertar.
- Presa que atraviesa la guardia, con castigo de 0,85 s si se falla.
- Asaltos al mejor de tres, con anuncio y marcador.
- Parada de fotograma al impacto (80 ms, 150 ms en fuerte), destello sobre quien encaja,
  sacudida y empujon de camara.

### Adversario
- Maquina en el puesto del jugador 2, con tres niveles que cambian tiempo de reaccion,
  ganas de taparse y mezcla de golpes. Agarra a quien se queda tapado.

### Imagen
- Paso a Universal Render Pipeline 17.0.4. Los 74 materiales reescritos.
- Contorno negro por casco invertido sobre los luchadores.
- Luz de tres puntos con acento de color por jugador, post proceso (ACES, contraste,
  saturacion, bloom, vineta) y encuadre cerrado.
- Atmosfera propia para cada una de las nueve arenas: sol, ambiente, niebla y fondo.
- Seis retratos de seleccion generados. Faltan Cleopatra, Lincoln y Socrates.

### Sonido
- Cuatro voces en rotacion con variacion de tono y capa grave bajo el golpe fuerte.
  Muestras CC0 de Kenney.

### Herramientas
- Prueba de humo: el player arranca, se fotografia a si mismo y se cierra. La CI la corre
  despues de cada build y falla si el juego no abre.
- Build de Windows ademas del de macOS.
- 88 pruebas EditMode.

### Arreglos que merecen mencion
- El player moria al arrancar con `level0 is corrupted`: `DlcEntitlementService` vivia en
  un archivo con otro nombre y Unity no podia resolver su script. La CI daba verde porque
  solo comprobaba que compilase.
- `activeInputHandler` estaba en 0 en el repo desde el primer commit: cualquier build hecho
  fuera del Mac de Munay salia sin teclado ni mando.
- La prueba que vigilaba el servicio DLC buscaba en el YAML una cadena que solo existe
  cuando el script esta roto. Verde significaba exactamente lo contrario de lo que parecia.

# Plan hasta la venta — Smash the Past

Estado a 13/09/2026. El tablero del dia a dia esta en `TASKS.md`; esto es el mapa
hasta tener algo que se pueda vender.

## Donde estamos

El combate ya tiene fondo: golpes ligero y fuerte, guardia, special por personaje,
aturdimiento que da consecuencia a cada golpe, presa que atraviesa la guardia, y una
maquina con tres niveles que juega de verdad. Nueve personajes con sus datos y frases,
nueve arenas con atmosfera propia, seis retratos de seleccion, URP con contorno y
post proceso, e impacto trabajado (parada de fotograma, destello, empujon de camara).
88 pruebas en verde, CI que compila, lanza el player y guarda una captura en cada PR.

Lo que falta no es pulido: es estructura de producto, personajes de verdad y tienda.

## Hitos

### H1 — Estructura de partida (siguiente, 1 sesion)
Sin esto el juego parece una demo aunque el combate sea bueno.
- Asaltos al mejor de tres, con anuncio de "Asalto 1" y cuenta atras.
- Pantalla de victoria con la frase del ganador (los textos ya existen en cada personaje).
- Menu principal: jugar contra la maquina, jugar a dos, opciones, salir.
- Opciones: volumen, dificultad, reconfiguracion de teclas.
- Criterio de hecho: se puede empezar, perder, ver el resultado y volver al menu sin tocar el teclado a ciegas.

### H2 — Modo arcade (1 sesion)
- Escalera de adversarios con dificultad creciente y final.
- Guardado de partida: dificultad, volumen, teclas, arcade terminado.
- Criterio: alguien que compra el juego y esta solo tiene una razon para seguir jugando.

### H3 — Personajes de verdad (depende de creditos)
- Prueba con UN personaje: imagen a cuerpo entero en pose A, conversion 3D con rig
  (35 creditos), import en Unity con glTFast, Animator conectado. 36 creditos en total.
- Si la prueba convence: los ocho restantes, unos 315 creditos, que caben en un mes del
  plan Plus ya pagado.
- Si no convence: pack de assets comprado (30 a 60 EUR) con animaciones Mixamo.
- En paralelo y barato: los 3 retratos que faltan (3 creditos) y fondos pintados por
  arena (9 creditos).

### H4 — Sonido (10 EUR y una tarde)
- Bruitage que falta: guardia, seleccion de menu, inicio de asalto, pasos. Fuentes CC0
  (Kenney, bundles Sonniss GDC), cero coste.
- Musica: Suno o Udio con **plan de pago**, el gratuito no da derechos comerciales.
  Un tema por arena mas menu y victoria. Mejor cuando H1 y H2 esten cerrados, porque un
  tema se compone mejor sabiendo como suena el combate.

### H5 — Tienda (bloqueado por credenciales)
- Cuenta de publisher y Steam Direct (100 USD).
- Ficha, capsulas, trailer, capturas, configuracion de mando, comprobacion en Steam Deck.
- El DLC de Hitler necesita SKU real, precio y verificacion de edad, y descriptores de
  contenido declarados. Alemania bloquea contenido nazi: la regla de cero simbolos no es
  estetica, es condicion para vender alli.

## Quien hace que

- **Munay**: decisiones de alcance y dinero, pruebas a mano (sensacion de los golpes,
  equilibrio de la maquina, teclado y dos mandos), cuenta de Steam, musica.
- **Claude**: codigo, pruebas, CI, herramientas, integracion de arte generado.
- **Diego**: llega ahora. `Assets/Characters/`, `Assets/Stages/` y `Assets/Audio/` son
  suyos segun `AGENTS.md`, pero Claude ha trabajado mucho ahi hoy: antes de repartir
  tareas hay que decidir si esa division sigue valiendo o se cambia.

## Lo que bloquea de verdad

1. **Creditos de Higgsfield a cero.** Bloquea H3 entero y los 3 retratos.
2. **Credenciales de publisher.** Bloquean H5 y el DLC.
3. **El juicio humano.** Nadie ha jugado tres partidas seguidas contra la maquina. Hasta
   que eso pase, cualquier ajuste de equilibrio es una suposicion.

## Decisiones pendientes de Munay

- Fecha objetivo de publicacion, aunque sea aproximada. Sin ella no hay forma de decir
  que se corta.
- Alcance de la v1: se mantienen los 9 personajes o se sale con menos y mejor acabados.
- Personajes 3D: se prueba la via IA o se compra un pack directamente.
- Reparto de carpetas con Diego.

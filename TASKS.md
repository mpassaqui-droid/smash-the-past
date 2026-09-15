# Tablero — Smash the Past

Reglas: cada tarea tiene un dueño y un criterio de "hecho" (qué test pasa o qué se ve en el build). Actualizar antes/después de cada sesión de trabajo.

## Por hacer
- [ ] Diego: cuatro efectos de combate que siguen siendo de Kenney (golpe fuerte,
      impacto fuerte, especial y K.O.) y un tema de combate propio. No necesita Unity
      para nada de eso: manda los archivos y se sustituyen por el mismo nombre.
- [ ] Mejores animaciones de golpe segun Diego. Se reparo el REPARTO DEL TIEMPO (pico
      de movimiento 4,2 veces la media en el ligero y 7,0 en el fuerte), pero las poses
      siguen siendo las mismas.
- [ ] Munay: probar teclado y dos mandos fisicos en el player construido por CI desde un clon limpio: el player publicado en Releases. Es lo unico que la maquina no puede demostrar sola.
- [ ] Generar los 3 retratos que faltan (Cleopatra, Abraham Lincoln, Socrates) cuando haya creditos en Higgsfield. Mismo prompt que los otros seis para que el estilo no se separe.
- [ ] Munay: mirar el player URP construido por CI y decir si el aspecto va por buen camino. Es lo unico que falta para cerrar la PR #5 (en la captura headless hay que apagar el SRP Batcher o sale todo dorado; hace falta ver el player real para saber si eso pasa tambien ahi).
- [ ] Musica: Suno o Udio con plan de pago (los derechos comerciales no vienen con el plan gratuito). Mejor cuando el combate este cerrado de punta a punta.
- [ ] Las piezas de vestuario son cajas biseladas. Se leen a distancia de juego, no de
      cerca. Pasar de ahi es modelar de verdad, no cambiar medidas.
- [ ] Release: configurar Steamworks, SKU real, precios, compra y verificacion de edad con credenciales del publisher.
- [x] Configurar UnityYAMLMerge como merge driver en este worktree (Unity 6000.0.83f1).
- [x] Escena de prueba real: `Assets/Stages/PrototypeArena.unity`, 2 Fighters con CharacterController y PlayerInputController, teclado y 2 mandos, pausa, timer, KO y revancha.
- [x] Configurar CI (GitHub Actions + runner privado de este Mac) para correr EditMode tests en cada PR automáticamente. Workflow: `.github/workflows/unity-tests.yml`. Runner: `munay-smash-the-past`, etiqueta `unity`.
- [x] CI de build macOS añadido después de EditMode: genera y publica el player standalone como artefacto.
- [x] Codex: feedback de audio procedural para golpes, impactos, specials y K.O., conectado desde fuera de Core. Suite EditMode aislada: **41/41**.
- [x] Codex: impacto visual y camera shake para golpes confirmados, sin cambios en Core. Suite EditMode aislada: **41/41**.
- [x] Codex: entry point de build macOS y validación de app standalone: **103581376 bytes**, suite EditMode posterior **41/41**.
- [x] Codex: prueba de QA de audio e impactos presentes en la escena. Suite EditMode: **42/42**.
- [x] Codex: corregido arranque macOS por shader faltante y añadido fallback UI runtime para selección/HUD. Build final: **103585312 bytes**, suite EditMode: **42/42**, transición `J` + `Enter` verificada en player.

## En curso

(nada en curso)

## Hecho

- [x] Claude: musica que se anade dejando archivos en `Assets/Audio/Resources/Musica/`, sin tocar codigo. Guia completa en `docs/AUDIO.md`.
- [x] Claude: arreglada la intermitencia de las pruebas de input. Tres corridas seguidas 100/100.

- [x] Claude: modo arcade. Escalera de ocho rivales por personaje, dificultad que sube por tramos y final siempre en dificil, progreso guardado. 7 pruebas nuevas sobre la escalera.

- [x] Claude: menu principal (contra la maquina, dos jugadores, opciones, salir) y opciones con volumen y dificultad guardados entre partidas. La pantalla de victoria con las frases ya existia.
- [x] Claude: asaltos al mejor de tres con anuncio y marcador.

- [x] Claude: adversario de la maquina en el puesto de P2, tres niveles, F1 lo apaga y F2 cambia de dificultad. La decision es una funcion pura probada con sorteo fijado. Probado en el player: 12 s sin tocar una tecla y P1 baja de 100 a 35.
- [x] Claude: presa que atraviesa la guardia y castigo al fallarla. Aturdimiento unificado en Fighter.

- [x] Claude: contorno negro por casco invertido sobre los luchadores, shader propio en `Stages/Resources`. PR #8.
- [x] Claude: empujon de camara al impacto, en tiempo sin escalar para que se vea durante la parada de fotograma. Sin Cinemachine: el encuadre de grupo y la sacudida ya existian. PR #9.
- [x] Claude: parada de fotograma al impacto (50 ms ligero, 90 ms fuerte), destello blanco sobre el que encaja, y una atmosfera propia por arena (sol, ambiente, niebla y fondo). PR #6.
- [x] Claude: cuatro voces de audio en rotacion con variacion de tono y capa grave bajo el golpe fuerte. PR #7.
- [x] Claude: la prueba de humo acepta `-escenario N` y recorre las arenas sin teclado.
- [x] Claude: seis retratos de seleccion generados y enchufados en `PrototypeUI` (faltan 3).
- [x] Claude: arreglado el crash del player. `DlcEntitlementService` vivia en un archivo con otro nombre, Unity no resolvia su MonoScript y el build escribia un `level0` invalido. Player local arrancado y vivo con el log limpio.
- [x] Claude: la prueba del servicio DLC comprueba el componente cargado en vez de buscar en el YAML la cadena que solo aparece cuando el script esta roto.
- [x] Claude: paso a URP 17.0.4 + Cinemachine 3.1.7, 74 materiales reescritos, luces de tres puntos con acento por jugador, post proceso (ACES, contraste, saturacion, bloom, vineta) y encuadre nuevo. PR #5, CI en clon limpio **70/70** y build macOS.
- [x] Claude: herramientas de render en `Assets/Editor` para trabajar sin abrir el editor: instalar paquetes, configurar pipeline y capturar la escena a PNG.
- [x] Claude: rama `feature/characters` mergeada en `main` (PR #3). CI verde en clon limpio: EditMode **70/70** y build macOS.
- [x] Claude: `activeInputHandler` pasa a 2. Estaba en 0 (solo Input Manager antiguo) desde el primer commit con cinco scripts usando el nuevo Input System, asi que todo build hecho fuera de este Mac salia sin input.
- [x] Claude: commiteados los cinco `.meta` que faltaban; Unity ya no regenera GUID en cada clon.
- [x] Claude: dos tests guardian sobre el ajuste de input, probados al reves en una rama temporal con el valor roto: **68/70** con el fallo, **70/70** con el arreglo.

- [x] Codex: corregir ruta de golpes contra defensas especiales, carga antes del KO, input simultaneo teclado/mando y audio con muestras CC0. EditMode completo: **68/68**. Build macOS: **105976940 bytes**. Sin cambios en Core.
- [x] Codex: patio andino reutilizable con terrazas de piedra, cordillera y estandartes. Cambios en PrototypeArena y nuevo prefab de arte; suelo fisico y Core sin cambios. EditMode: **44/44**. Build macOS: **104368704 bytes**. Player verificado visualmente.
- [x] Codex: capa de animacion externa para los 9 personajes, conectada a eventos publicos de Fighter. Prefabs regenerados y player macOS validado.
- [x] Codex: nueve variantes visuales de escenario, seleccionadas por el personaje P1 en la escena reutilizable. EditMode: **45/45**. Build macOS: **105211660 bytes**. Player validado.
- [x] Codex: contenido DLC de Hitler aislado del roster base, oferta con edad minima y proveedor Steam fail-closed. EditMode: **47/47**. Build macOS: **105213052 bytes**.
- [x] Codex: QA runtime del special DLC y validacion de mallas/colliders en las 9 variantes. EditMode: **48/48**. Build macOS: **105213052 bytes**.
- [x] Codex: servicio de entitlement DLC cableado en la escena y fail-closed verificado. EditMode: **49/49**. Build macOS: **105214236 bytes**.
- [x] Codex (`feature/characters`): roster base, golpes externos a Core, escena de prueba, escenario reutilizable y selección/HUD. Core queda sin cambios.
- [x] Codex: nueva escena `Assets/Stages/PrototypeArena.unity` y prefabs nuevos en `Assets/Characters/Prefabs/`; no editar otras escenas o prefabs.

- [x] Codex: restaurada la interfaz UI Toolkit con referencia TSS importada correcta; eliminado el fallback con seleccion independiente. Seleccion y pausa verificadas visualmente en player macOS. Suite EditMode: **43/43** (Core **13/13**), incluido ciclo de seleccion, dano, pausa/reanudar, KO, revancha y nueva seleccion. Build macOS: **104258736 bytes**. Prueba automatizada de comandos; teclado verificado en player, dos mandos fisicos pendientes de comprobacion.

- [x] Codex: HitDetector externo a Core, conectado a eventos de Fighter; alcance horizontal/vertical, dano ligero/fuerte, carga del atacante y bloqueo/KO respetados. EditMode: 32/32, incluida integracion real de eventos en Play Mode.
- [x] Codex: specials externos a Core, efectos visuales, prefabs low-poly, escenario reutilizable y UI de selección/HUD. Suite EditMode final: **41/41**, incluidos Core, roster, golpes, specials y escena/prefabs.

- [x] Codex: 9 CharacterData base con stats universales, bio, special del GDD y 3 frases originales de victoria/derrota por personaje. EditMode: 23/23 (13 Core + 10 roster), ejecutados en copia aislada con Unity 6000.0.83f1.
- [x] GDD completo (`GDD.md`)
- [x] Repo git, `.gitignore`, `.gitattributes` (LFS), `AGENTS.md`, `CLAUDE.md`
- [x] Unity Hub + Unity 6 LTS (6000.0.83f1) instalado y licenciado (Munay)
- [x] Proyecto Unity creado en `SmashThePast/`, Force Text + Visible Meta Files (ya vienen por defecto en Unity 6)
- [x] Input System + Test Framework instalados
- [x] Lógica core: `HealthSystem`, `SpecialMeter`, `CharacterData`, `Fighter`, `PlayerInputController`
- [x] Tests EditMode: **13/13 pasando** (corridos de verdad con `-runTests`, no autoreportado; encontraron y permitieron corregir un bug real de redondeo flotante en el chip damage)
- [x] Push a GitHub (`d79b712`)

# Tablero — Smash the Past

Reglas: cada tarea tiene un dueño y un criterio de "hecho" (qué test pasa o qué se ve en el build). Actualizar antes/después de cada sesión de trabajo.

## Por hacer
- [ ] Diego: instalar Unity Hub + Unity 6 LTS (6000.0.83f1, misma versión que Munay) y activar licencia Personal
- [ ] Diego: `git clone`, leer `AGENTS.md`, empezar Character pipeline (1 `CharacterData` de prueba + su special)
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

- [x] Codex: patio andino reutilizable con terrazas de piedra, cordillera y estandartes. Cambios en PrototypeArena y nuevo prefab de arte; suelo fisico y Core sin cambios. EditMode: **44/44**. Build macOS: **104368704 bytes**. Player verificado visualmente.
- [x] Codex: capa de animacion externa para los 9 personajes, conectada a eventos publicos de Fighter. Prefabs regenerados y player macOS validado.
- [x] Codex: nueve variantes visuales de escenario, seleccionadas por el personaje P1 en la escena reutilizable. EditMode: **45/45**. Build macOS: **105211660 bytes**. Player validado.
- [x] Codex: contenido DLC de Hitler aislado del roster base, oferta con edad minima y proveedor Steam fail-closed. EditMode: **47/47**. Build macOS: **105213052 bytes**.
- [x] Codex: QA runtime del special DLC y validacion de mallas/colliders en las 9 variantes. EditMode: **48/48**. Build macOS: **105213052 bytes**.
- [ ] Release: configurar Steamworks, SKU real, precios, compra y verificacion de edad con credenciales del publisher.

- [x] Codex (`feature/characters`): roster base, golpes externos a Core, escena de prueba, escenario reutilizable y selección/HUD. Core queda sin cambios.
- [x] Codex: nueva escena `Assets/Stages/PrototypeArena.unity` y prefabs nuevos en `Assets/Characters/Prefabs/`; no editar otras escenas o prefabs.

## Hecho

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

# Tablero — Smash the Past

Reglas: cada tarea tiene un dueño y un criterio de "hecho" (qué test pasa o qué se ve en el build). Actualizar antes/después de cada sesión de trabajo.

## Por hacer
- [ ] Diego: instalar Unity Hub + Unity 6 LTS (6000.0.83f1, misma versión que Munay) y activar licencia Personal
- [ ] Diego: `git clone`, leer `AGENTS.md`, empezar Character pipeline (1 `CharacterData` de prueba + su special)
- [ ] Configurar UnityYAMLMerge como merge driver (pendiente, ruta del binario depende de la instalación local de cada uno)
- [ ] Sistema de hitbox/hurtbox real (quién golpea a quién): `Fighter` ya expone `OnLightAttackPerformed`/`OnHeavyAttackPerformed`/`ReceiveHit`, falta el componente que detecta el golpe en la escena y conecta ambos fighters
- [ ] Escena de prueba real: 2 fighters en una escena gris, jugables con teclado/mando, para validar visualmente que el kit se siente bien (esto todavía no se ha visto correr, solo la lógica está testeada)
- [ ] Configurar GameCI (GitHub Actions) para correr tests en cada PR automáticamente

## En curso

## Hecho
- [x] GDD completo (`GDD.md`)
- [x] Repo git, `.gitignore`, `.gitattributes` (LFS), `AGENTS.md`, `CLAUDE.md`
- [x] Unity Hub + Unity 6 LTS (6000.0.83f1) instalado y licenciado (Munay)
- [x] Proyecto Unity creado en `SmashThePast/`, Force Text + Visible Meta Files (ya vienen por defecto en Unity 6)
- [x] Input System + Test Framework instalados
- [x] Lógica core: `HealthSystem`, `SpecialMeter`, `CharacterData`, `Fighter`, `PlayerInputController`
- [x] Tests EditMode: **13/13 pasando** (corridos de verdad con `-runTests`, no autoreportado; encontraron y permitieron corregir un bug real de redondeo flotante en el chip damage)
- [x] Push a GitHub (`d79b712`)

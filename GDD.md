# Smash the Past

Juego de pelea 1v1 de personajes históricos. Munay Passaqui + Diego Satori.

## Competencia directa a tener en cuenta
"History Warriors" (Steam) ya es un fighting 2D con Cleopatra, Napoleón, Lincoln e Hitler como jefe final. Roster muy parecido al nuestro. Diferenciadores necesarios: 3D low-poly (ellos son 2D), tag team en v2, nuestro roster específico (Túpac Amaru II, Genghis Khan, Musashi, Gandhi, Rasputín, Sócrates no están en el suyo).

## Motor y stack
- Unity (última LTS) + C#
- 3D low-poly, estética tipo party game colorido
- Unity Input System (nuevo) — teclado + 2 mandos en simultáneo

## Roster (10, cerrado)
1. Túpac Amaru II
2. Genghis Khan
3. Miyamoto Musashi
4. Gandhi
5. Rasputín
6. Napoleón Bonaparte
7. Cleopatra
8. Abraham Lincoln
9. Sócrates
10. Hitler

Decisión del equipo: incluir a Hitler pese al riesgo señalado de restricciones/edad en Steam. Confirmado explícitamente, no revisar de nuevo salvo que el equipo lo pida.

## Controles
- Movimiento (stick/flechas), Salto, Bloqueo, Puñete (golpe ligero), Patada (golpe fuerte), Special (botón aparte, se activa con una barra que se llena al dar/recibir daño)
- Esquema clásico de 4 botones + movimiento, igual en teclado y mando

## Moveset

Kit base universal: todos saltan igual, mismo movimiento, mismo puñete/patada, mismo bloqueo. El special es lo único distinto por personaje, ligado a por qué esa figura es conocida:

| Personaje | Special | Por qué |
|---|---|---|
| Túpac Amaru II | **Grito de Libertad** — ola expansiva que aturde brevemente | líder de una rebelión, convoca a la gente |
| Genghis Khan | **Carga Mongola** — volea de flechas cruzando la pantalla | arquería a caballo, su arma icónica |
| Miyamoto Musashi | **Niten Ichi-ryū** — dash con doble espada, multi-hit | esgrima a dos espadas, su técnica real |
| Gandhi | **Ayuno** — se vuelve invulnerable unos segundos y contraataca | resistencia no violenta; ironía: el más pacífico tiene el contraataque más fuerte |
| Rasputín | **Indestructible** — sobrevive con 1 de vida la primera vez que llegaría a 0 | sobrevivió veneno, disparos y ahogamiento en la vida real |
| Napoleón | **Artillería** — cañonazo desde fuera de pantalla | estratega de artillería |
| Cleopatra | **Áspid** — mordida que envenena (daño en el tiempo) | la serpiente con la que se suicidó, ya es su símbolo |
| Abraham Lincoln | **Hat Trick** — lanza su galera como bumerán | su sombrero de copa es su silueta icónica |
| Sócrates | **Mayéutica** — pregunta retórica que invierte los controles del rival un momento | su método de confundir con preguntas |
| Hitler | **Berlin Tantrum** — berrinche que aturde a su alrededor | tono cómico/ridiculizante, no glorificante, coherente con cómo ya se trata al personaje en internet |

## Escenarios
- 1 escenario por personaje (10 total), temático a su país/época

## Modos
- v1 (lanzamiento): Versus local 1v1, mismo dispositivo/pantalla, sin online
- v2 (post-lanzamiento): Tag team + poderes únicos ampliados

## Plataforma y precio
- Steam primero. Móvil a evaluar para v2 (versus local en móvil es raro, revisar factibilidad)
- $6.99 USD, pago único, sin ads ni suscripción

## Multijugador
- Solo local. Sin servidores ni netcode.

---

## Plan técnico

### Arquitectura de personajes
- Clase base `Fighter` (MonoBehaviour): vida, velocidad, salto, golpe ligero/fuerte, bloqueo, hit/KO — igual para los 10
- `CharacterData` (ScriptableObject) por personaje: nombre, stats, prefab/clip del special, ícono de selección
- Un solo esqueleto humanoide (Mecanim Humanoid) compartido, animaciones base (idle/walk/jump/ataque/bloqueo/hit/KO) reutilizadas entre los 10 vía retargeting — así el modelo se cambia pero la animación no se rehace cada vez. Solo el special de cada uno necesita animación propia.
- Combate: colliders simples (hurtbox/hitbox) + eventos de daño por trigger. Nada de sistema de combos frame-perfect estilo Street Fighter — es un party fighter, no un fighting game competitivo.

### Estructura de proyecto Unity
`Assets/{Characters, Stages, UI, Core, Animations, Audio, Scripts}`

### Escenarios
1 escena base reutilizable + set de props/skybox por personaje, en vez de 10 escenas completas independientes (mucho más barato).

### Build targets
Windows/Mac/Linux vía Steam para v1. Android/iOS se evalúa en v2 — el modo versus local en un solo celular es incómodo, no asumir que se traduce directo.

### Roles sugeridos (2 personas)
- Uno: programación (Fighter base, input, UI, integración)
- Otro: arte/animación (modelos low-poly, rig compartido, escenarios) + audio (ya tienen ventaja ahí, son músicos)
- Ambos: diseño de moves y balance

### Hitos
1. **Prototipo**: 1 personaje jugable contra sí mismo, kit base completo, 1 escenario gris — valida que el combate se sienta bien
2. **Vertical slice**: 2 personajes completos con su special + 1 escenario final + menú de selección — ya es material de trailer
3. **Roster completo**: 10 personajes + 10 escenarios + pase de balance
4. **Pulido**: UI final, sonido, opciones/rebind, logros de Steam, página de Steam
5. **Lanzamiento**

## Progresión y diferenciador (decidido)

**Progresión v1: frase histórica al final del combate**, no atuendos. Al ganar/perder aparece una línea de texto (real o irónica) ligada al personaje — ej. gana Napoleón contra alguien: *"Esta vez no hubo Waterloo."* Es prácticamente gratis (solo texto, cero arte extra) y le da personalidad al juego partida tras partida. **Atuendos extra quedan como stretch goal de v2** — cuestan arte real y no bloquean el lanzamiento.

**Diferenciador frente a "History Warriors":** 3D low-poly (ellos son 2D) + roster propio sin solape real (Túpac Amaru II, Genghis Khan, Musashi, Gandhi, Rasputín, Sócrates no están en el suyo) + el sistema de frase histórica post-combate como gancho de marketing/redes (la gente comparte la frase que le tocó).

## Pendiente / abierto
- Redactar el set de frases post-combate (varias por personaje, gana/pierde)
- Balance fino de daño/velocidad entre los 10 una vez estén jugables

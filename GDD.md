# Smash the Past

Juego de pelea 1v1 de personajes históricos. Munay Passaqui + Diego Satori.

## Competencia directa a tener en cuenta
"History Warriors" (Steam) ya es un fighting 2D con Cleopatra, Napoleón, Lincoln e Hitler como jefe final. Roster muy parecido al nuestro. Diferenciadores necesarios: 3D low-poly (ellos son 2D), tag team en v2, nuestro roster específico (Túpac Amaru II, Genghis Khan, Musashi, Gandhi, Rasputín, Sócrates no están en el suyo).

## Motor y stack
- Unity (última LTS) + C#
- 3D low-poly, estética tipo party game colorido
- Unity Input System (nuevo), teclado + 2 mandos en simultáneo

## Roster (9 base + 1 DLC)
1. Túpac Amaru II
2. Genghis Khan
3. Miyamoto Musashi
4. Gandhi
5. Rasputín
6. Napoleón Bonaparte
7. Cleopatra
8. Abraham Lincoln
9. Sócrates
10. Hitler (DLC, ver abajo)

Decisión final del equipo: los 9 primeros vienen en el juego base ($6.99). **Hitler es un unlock de pago aparte** (no incluido en el precio base), con verificación de edad (+15 recomendado) al comprarlo/activarlo en Steam. Confirmado explícitamente, no revisar de nuevo salvo que el equipo lo pida.

**Regla no negociable para el DLC de Hitler:** cero esvásticas, insignias o saludos en ningún asset, animación o escenario. El personaje existe como caricatura, los símbolos no. Steam se vende en Alemania y Austria, donde eso se evalúa caso por caso (StGB §86a).

## Controles
- Movimiento (stick/flechas), Salto, Bloqueo, Puñete (golpe ligero), Patada (golpe fuerte), Special (botón aparte, se activa con una barra que se llena al dar/recibir daño)
- Esquema clásico de 4 botones + movimiento, igual en teclado y mando
- Balance de carga (12/09/2026): la capa de combate externa duplica la carga por dano efectivo dado/recibido. Con 50 puntos de dano sin bloqueo se llena la barra de 100, antes del KO. El minimo de carga al recibir bloqueo sigue siendo el de Core.
- Teclado y mando pueden usarse simultaneamente. P2 admite 1/2/3/0 de la fila superior o del teclado numerico. Rasputin activa su supervivencia automaticamente una sola vez por combate.

## Moveset

Kit base universal: todos saltan igual, mismo movimiento, mismo puñete/patada, mismo bloqueo. El special es lo único distinto por personaje, ligado a por qué esa figura es conocida:

| Personaje | Special | Por qué |
|---|---|---|
| Túpac Amaru II | **Grito de Libertad** (ola expansiva que aturde brevemente) | líder de una rebelión, convoca a la gente |
| Genghis Khan | **Carga Mongola** (volea de flechas cruzando la pantalla) | arquería a caballo, su arma icónica |
| Miyamoto Musashi | **Niten Ichi-ryū** (dash con doble espada, multi-hit) | esgrima a dos espadas, su técnica real |
| Gandhi | **Ayuno** (se vuelve invulnerable unos segundos y contraataca) | resistencia no violenta; ironía: el más pacífico tiene el contraataque más fuerte |
| Rasputín | **Indestructible** (sobrevive con 1 de vida la primera vez que llegaría a 0) | sobrevivió veneno, disparos y ahogamiento en la vida real |
| Napoleón | **Artillería** (cañonazo desde fuera de pantalla) | estratega de artillería |
| Cleopatra | **Áspid** (mordida que envenena, daño en el tiempo) | la serpiente con la que se suicidó, ya es su símbolo |
| Abraham Lincoln | **Hat Trick** (lanza su galera como bumerán) | su sombrero de copa es su silueta icónica |
| Sócrates | **Mayéutica** (pregunta retórica que invierte los controles del rival un momento) | su método de confundir con preguntas |
| Hitler | **Berlin Tantrum** (berrinche que aturde a su alrededor) | tono cómico y ridiculizante, no glorificante, coherente con cómo ya se trata al personaje en internet |

## Escenarios
- 1 escenario por personaje (9 base + 1 del DLC), temático a su país/época

## Modos
- v1 (lanzamiento): Versus local 1v1, mismo dispositivo/pantalla, sin online
- v2 (post-lanzamiento): Tag team + poderes únicos ampliados

## Plataforma y precio
- Steam primero. Móvil a evaluar para v2 (versus local en móvil es raro, revisar factibilidad)
- $6.99 USD, pago único, sin ads ni suscripción, incluye 9 personajes
- Hitler: unlock de pago aparte (precio bajo, ej. ~$1), con verificación de edad en Steam

## Multijugador
- Solo local. Sin servidores ni netcode.

---

## Plan técnico

### Arquitectura de personajes
- Clase base `Fighter` (MonoBehaviour): vida, velocidad, salto, golpe ligero/fuerte, bloqueo, hit/KO, igual para todos
- `CharacterData` (ScriptableObject) por personaje: nombre, stats, prefab/clip del special, ícono de selección
- Un solo esqueleto humanoide (Mecanim Humanoid) compartido, animaciones base (idle/walk/jump/ataque/bloqueo/hit/KO) reutilizadas entre todos vía retargeting: así el modelo se cambia pero la animación no se rehace cada vez. Solo el special de cada uno necesita animación propia.
- Combate: colliders simples (hurtbox/hitbox) + eventos de daño por trigger. Nada de sistema de combos frame-perfect estilo Street Fighter, es un party fighter, no un fighting game competitivo.

### Estructura de proyecto Unity
`Assets/{Characters, Stages, UI, Core, Animations, Audio, Scripts}`

### Escenarios
1 escena base reutilizable + set de props/skybox por personaje, en vez de escenas completas independientes (mucho más barato).

### Build targets
Windows/Mac/Linux vía Steam para v1. Android/iOS se evalúa en v2, el modo versus local en un solo celular es incómodo, no asumir que se traduce directo.

### Roles sugeridos (2 personas)
- Uno: programación (Fighter base, input, UI, integración)
- Otro: arte/animación (modelos low-poly, rig compartido, escenarios) + audio (ya tienen ventaja ahí, son músicos)
- Ambos: diseño de moves y balance

### Hitos
1. **Prototipo**: 1 personaje jugable contra sí mismo, kit base completo, 1 escenario gris. Valida que el combate se sienta bien
2. **Vertical slice**: 2 personajes completos con su special + 1 escenario final + menú de selección. Ya es material de trailer
3. **Roster base completo**: 9 personajes + 9 escenarios + pase de balance
4. **Hitler DLC**: personaje + escenario + flujo de compra/unlock con verificación de edad
5. **Pulido**: UI final, sonido, opciones/rebind, logros de Steam, página de Steam
6. **Lanzamiento**

## Progresión y diferenciador (decidido)

**Progresión v1: frase histórica al final del combate**, no atuendos. Al ganar/perder aparece una línea de texto (real o irónica) ligada al personaje, ej. gana Napoleón contra alguien: *"Esta vez no hubo Waterloo."* Es prácticamente gratis (solo texto, cero arte extra) y le da personalidad al juego partida tras partida. **Atuendos extra quedan como stretch goal de v2**, cuestan arte real y no bloquean el lanzamiento.

**Diferenciador frente a "History Warriors":** 3D low-poly (ellos son 2D) + roster propio sin solape real (Túpac Amaru II, Genghis Khan, Musashi, Gandhi, Rasputín, Sócrates no están en el suyo) + el sistema de frase histórica post-combate como gancho de marketing/redes (la gente comparte la frase que le tocó).

## Ideas futuras (no bloquean v1, guardadas para después)
- Expansión de roster: decidir entre update gratis (modelo Hollow Knight, refuerza el juego sin cobrar) o DLC barato (~$1 por personaje). Todavía sin decidir
- "Pack de presidentes" como DLC temático futuro (surgió al descartar a Fujimori del roster base por ser demasiado actual y políticamente sensible)

## Pendiente / abierto
- Redactar el set de frases post-combate (varias por personaje, gana/pierde)
- Balance fino de daño/velocidad entre todos una vez estén jugables
- Implementación técnica del unlock de pago de Hitler (Steam DLC o in-app) + verificación de edad

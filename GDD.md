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

## Moveset
- Kit base universal: todos saltan igual, mismo movimiento, mismo golpe ligero/fuerte, mismo bloqueo
- 1 special/poder único por personaje (por definir, uno por uno)

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

## Pendiente / abierto (bloquea empezar a programar)
- Special move de cada uno de los 10 personajes
- Esquema de controles exacto (cuántos botones por jugador)
- Progresión: el "desbloqueo entre rondas" pensado antes (roguelite/co-op) no aplica igual a un fighting 1v1 clásico. Definir qué se desbloquea: ¿skins, escenarios alternos, movimientos alternativos, moneda del juego?
- Diferenciador final frente a "History Warriors" para la página de Steam

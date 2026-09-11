# Smash the Past — instrucciones para agentes IA (Claude Code, Codex, cualquier otro)

Lee esto completo antes de tocar código. El diseño completo está en `GDD.md`, esto es el resumen operativo.

## Qué es el juego
Fighting 1v1 local (mismo dispositivo, sin online), 3D low-poly, 9 personajes históricos en el juego base + Hitler como DLC de pago aparte con verificación de edad. Unity + C#. Precio $6.99.

## Arquitectura (no te la inventes distinta)
- `Fighter` (MonoBehaviour): vida, movimiento, salto, puñete, patada, bloqueo, hit/KO. Es la MISMA clase para los 10 personajes.
- `CharacterData` (ScriptableObject): un asset por personaje con sus stats y referencia a su special. La diferencia entre personajes vive AQUÍ, no en clases separadas por personaje.
- Un solo esqueleto humanoide compartido (Mecanim Humanoid). Las animaciones base (idle/walk/jump/ataque/bloqueo/hit/KO) se reutilizan entre todos vía retargeting. Solo el special de cada uno tiene animación propia.
- Combate con hitbox/hurtbox simples + trigger events. Nada de sistema de combos frame-perfect, esto no es un fighting game competitivo.

## Quién es dueño de qué carpeta (para no pisarnos)
- `Assets/Core/`, `Assets/UI/`, `Tests/` → Munay
- `Assets/Characters/`, `Assets/Stages/`, `Assets/Audio/` → Diego
- Si necesitas tocar una carpeta que no es tuya, avísale al otro antes, no lo hagas directo.

## Lo que NO existe en v1 (no lo construyas por iniciativa propia)
- Multijugador online / netcode
- Modo historia / campaña con narrativa
- Tag team (es v2)
- Versión móvil
- Sistema de combos avanzado tipo Street Fighter

## Reglas duras
- **Cero esvásticas, insignias o saludos nazis en ningún asset, animación o escenario.** El personaje de Hitler existe como caricatura cómica (special: "Berlin Tantrum", un berrinche), los símbolos reales no van en ningún archivo.
- Una regla de gameplay ya establecida no se rompe sin avisar y sin actualizar el test correspondiente (ej: si bloquear anula el puñete, eso es una regla fija).
- Antes de dar una tarea por terminada: correr los tests relevantes y reportar el número real (ej. "8/8"), no decir "ya funciona" sin haberlo corrido.
- Nunca editar la misma escena `.unity` o el mismo `.prefab` que el otro esté tocando la misma semana. Avisar en `TASKS.md`.
- Ramas cortas (1-3 días), PR a `main` seguido. Nada de ramas largas con cambios de escenas o prefabs.

## Convenciones de código
- Un script por clase, nombre de archivo = nombre de clase
- Namespace `SmashThePast.<Sistema>` (ej. `SmashThePast.Core`, `SmashThePast.Characters`)
- Lógica de combate en clases C# puras (sin MonoBehaviour) cuando sea posible, para poder testearla sin abrir Unity

## Dónde está todo
- Diseño completo: `GDD.md`
- Tablero de tareas: `TASKS.md`
- Proyecto Unity: `SmashThePast/` (se crea en el siguiente paso)

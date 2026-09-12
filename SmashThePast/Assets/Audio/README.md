# Audio de combate

Seis muestras Ogg Vorbis originales de Kenney, sin recortar ni recodificar.
Licencia CC0 1.0, comprobada en las paginas oficiales el 2026-09-12.
Creditos: Kenney (https://kenney.nl). La atribucion es voluntaria.

## Fuentes

- Impact Sounds: https://kenney.nl/assets/impact-sounds
  Archivo: https://kenney.nl/media/pages/assets/impact-sounds/87b4ddecda-1677589768/kenney_impact-sounds.zip
  Licencia original: `Licenses/ImpactSounds/License.txt`.
- RPG Audio: https://kenney.nl/assets/rpg-audio
  Archivo: https://kenney.nl/media/pages/assets/rpg-audio/8e99002d76-1677590336/kenney_rpg-audio.zip
  Licencia original: `Licenses/RpgAudio/License.txt`.

## Mapeo

Cada muestra conserva su nombre del directorio `Audio/` del ZIP y vive en
`Resources/CombatAudio/`. Las licencias se copian sin cambios de ambos ZIP.

| Uso | Archivo original | Pack | Ganancia relativa |
| --- | --- | --- | --- |
| Swing ligero aceptado | knifeSlice.ogg | RPG Audio | 0.6 |
| Swing fuerte aceptado | knifeSlice2.ogg | RPG Audio | 0.8 |
| Impacto de 1 a 9 de dano | impactPunch_medium_000.ogg | Impact Sounds | 0.65 + dano/80, max. 1 |
| Impacto de 10 o mas de dano | impactPunch_heavy_000.ogg | Impact Sounds | 0.65 + dano/80, max. 1 |
| Special, incluida supervivencia de Rasputin | impactBell_heavy_000.ogg | Impact Sounds | 0.7 |
| KO | impactPunch_heavy_002.ogg | Impact Sounds | 1 |

`PrototypeAudio` carga las muestras por `Resources.Load<AudioClip>` en Awake.
No necesita referencias de clips en la escena. Conserva el campo serializado
`match` y el volumen maestro existente (0.45 por defecto).
La importacion usa Decompress On Load, precarga sin carga en segundo plano y PCM
para estas muestras cortas. Los Ogg originales suman 73,988 bytes.

Los swings proceden de `HitDetector.AttackPerformed(bool)`, despues del control
de cooldown. Los impactos proceden de `HitLanded` con dano positivo. El special
procede de `Combatant.SpecialActivated(SpecialKind)`, incluida la activacion
pasiva de Rasputin. El KO conserva `Fighter.OnKnockedOut`. No se escuchan los
intentos de ataque ni de special de Fighter. Desactivar el componente elimina
las suscripciones y detiene el audio; reactivarlo o cambiar luchadores las renueva.

No se incluyen pasos. Los impactos de specials no tienen un evento HitLanded
comun en Characters, por lo que aqui reciben el sonido de activacion y el KO
si corresponde. No se modifica su resolucion de dano.

## Validacion

`Tests/EditMode/` contiene seis casos de integridad SHA-256, carga por Resources
y muestras decodificadas no silenciosas, mas una prueba de suscripciones durante
desactivacion, reactivacion, reemplazo de luchadores y retirada de la partida.
La prueba comprueba que no existen suscripciones a intentos de Fighter.
Las pruebas Unity y la escucha en partida quedan a cargo de la sesion principal.

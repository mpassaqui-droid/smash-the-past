# Anadir musica y sonidos

Escrito para trabajar sin tocar codigo. Si al final hace falta tocarlo, esta explicado
tambien.

## Lo que ya existe

El combate ya avisa de todo lo que pasa: golpe ligero, golpe fuerte, impacto, impacto
fuerte, special y K.O. `Assets/Audio/PrototypeAudio.cs` escucha esos avisos y dispara un
sonido por cada uno. Hay cuatro voces en rotacion con variacion de tono, para que dos
golpes seguidos no suenen identicos, y una capa grave bajo el golpe fuerte.

Las muestras actuales son de **Kenney**, dominio publico (CC0), y viven en
`Assets/Audio/Resources/CombatAudio/`.

## Cambiar un sonido de combate

Es lo mas facil: **sustituir el archivo, mismo nombre**. No hay que tocar nada mas.

| Que suena | Archivo |
|---|---|
| Golpe ligero al aire | `knifeSlice.ogg` |
| Golpe fuerte al aire | `knifeSlice2.ogg` |
| Impacto normal | `impactPunch_medium_000.ogg` |
| Impacto fuerte | `impactPunch_heavy_000.ogg` |
| Special | `impactBell_heavy_000.ogg` |
| K.O. | `impactPunch_heavy_002.ogg` |

Si prefieres nombres nuevos en vez de sustituir, hay que cambiar las seis lineas de
`LoadClip(...)` en `PrototypeAudio.Awake`. Hay una prueba que comprueba el hash de los
originales de Kenney: si cambias un archivo, esa prueba falla a proposito, para que nadie
sustituya audio sin darse cuenta. Actualiza el hash en `CombatAudioAssetTests` en el mismo
commit.

### Ajustes de importacion para efectos

Cortos, asi que: **Decompress on load**, mono si la fuente es mono, 22 kHz o 44.1 kHz. Hay
una prueba que exige `DecompressOnLoad`, duracion entre 0,05 y 2 segundos, y que el clip no
este en silencio.

## Sonidos que faltan

Guardia, seleccion de menu, inicio de asalto y pasos. Ninguno esta conectado todavia: hace
falta anadir el clip y una linea en `PrototypeAudio` que lo dispare desde el evento
correspondiente.

## Musica

Aqui no hace falta tocar codigo **en absoluto**. Deja el archivo en
`Assets/Audio/Resources/Musica/` con el nombre correcto y suena solo:

- `menu.ogg` en el menu y en la seleccion de personaje.
- `arena1.ogg` ... `arena9.ogg` en cada arena, en el orden del roster (`arena1` es la de
  Tupac Amaru II, `arena9` la de Socrates).
- `combate.ogg` es el respaldo: suena en cualquier arena que todavia no tenga la suya.

Puedes empezar con un solo `combate.ogg` y el juego entero ya tiene musica. Luego se van
anadiendo las de cada arena y cada una sustituye al respaldo cuando aparece.

### Ajustes de importacion para musica

Son archivos largos: **Streaming** y **Load in background**. Cargarlos enteros en memoria
no tiene sentido. Formato `.ogg`, estereo, 44.1 kHz.

## Trabajar la musica en tiempo real, con los personajes delante

No hace falta compilar nada para oir como queda.

1. En Unity, abre la escena `Assets/Stages/PrototypeArena.unity` haciendo doble click en
   ella desde la ventana Project. **Si acabas de abrir el proyecto y ves una pantalla
   vacia, es que no hay ninguna escena abierta: esto lo arregla.**
2. Pulsa **Play**. El juego corre dentro del editor, con sonido.
3. Deja tus archivos en `Assets/Audio/Resources/Musica/`. Unity los importa solo. Puedes
   hacerlo **con el juego corriendo**: vuelve a Unity, suelta el archivo, y al cambiar de
   pantalla o de arena la pista nueva entra sin reiniciar.
4. Para ajustar el volumen mientras suena: selecciona el objeto `Musica` en la Hierarchy y
   mueve el campo **Volumen** del componente. Los cambios hechos en Play se pierden al
   parar, asi que cuando des con el valor bueno, paralo y ponlo otra vez.
5. Para saltar rapido entre arenas sin pelear: en el menu elige DOS JUGADORES, y en la
   seleccion cambia el personaje del jugador 1. Cada personaje trae su propia arena, asi
   que oiras `arena1` a `arena9` una detras de otra.

Atajo util: con el juego corriendo, la tecla `M` enciende y apaga a la maquina. Asi puedes
dejar que pelee sola y concentrarte en escuchar.

## Licencias, esto no es opcional

El juego se va a vender. Cada pista y cada efecto necesita una de estas tres:

1. **Hecho por ti.**
2. **CC0 o dominio publico**: Kenney, los bundles de Sonniss GDC, Freesound filtrando por CC0.
3. **Generado con un plan de pago** que ceda derechos comerciales. El plan gratuito de Suno
   y de Udio **no los cede**: musica generada ahi no se puede vender.

Por cada fuente nueva, una nota en `Assets/Audio/Licenses/` diciendo de donde sale, quien
es el autor y bajo que licencia. Es lo que nos salva si alguien pregunta.

## Como mandarlo al repo

```bash
git checkout main
git pull                                  # partir de lo ultimo, siempre
git checkout -b audio/musica-de-arenas    # rama propia, nunca commitear en main

# copiar los archivos a su carpeta, abrir Unity para que genere los .meta

git add SmashThePast/Assets/Audio
git commit -m "Musica de las arenas andina y mongola

Generada con Suno (plan de pago, derechos comerciales incluidos), nota en
Assets/Audio/Licenses/. Streaming activado: son pistas de dos minutos."
git push -u origin audio/musica-de-arenas
```

Luego, en GitHub, **Compare & pull request**. La CI arranca sola: corre las 100 pruebas,
construye el juego, lo **lanza** y guarda una captura. Cuando este en verde, *Merge pull
request*.

Ojo: el runner de la CI es el Mac de Munay. Si su ordenador esta apagado, el PR se queda
esperando y no es culpa tuya.

### Cosas que duelen si no se saben

- **Los `.ogg` y `.wav` van por Git LFS.** Ya esta configurado, pero hace falta
  `git lfs install` una vez en tu maquina antes del primer clone. Sin eso subiras archivos
  de texto en lugar de audio.
- **Sube siempre el `.meta` junto al archivo.** Unity genera un `.meta` por cada asset y sin
  el no reconoce el archivo. `git add` de la carpeta entera los coge todos.
- **No edites la escena `PrototypeArena.unity` a la vez que otro.** Las escenas de Unity se
  fusionan mal; el repo tiene UnityYAMLMerge configurado, pero es mejor avisar en
  `TASKS.md` antes de tocarla. Para la musica no hace falta: los archivos se cargan por
  nombre y la escena no cambia.
- Si tu PR sale en rojo y no entiendes por que, mira el artefacto `humo.png` del run: es una
  captura de como se veia el juego en ese commit.

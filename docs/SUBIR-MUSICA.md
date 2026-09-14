# Subir musica

Escrito para Diego. La musica es el unico trabajo del proyecto que no toca codigo: dejas
el archivo con el nombre correcto y suena solo. No hace falta abrir Unity.

> **Aviso, corregido el 14/09/2026.** La primera version de esta guia decia que se podia
> subir el `.ogg` desde la web de GitHub. **Es falso y lo comprobamos en la practica.**
> GitHub responde: *"The following files are configured to be stored in Git LFS and must
> be pushed using the Git command line with Git LFS installed"*. La subida por navegador
> **no sabe** manejar Git LFS, y en este repo los `.ogg` estan en LFS. Abajo estan las
> tres rutas que si funcionan.

## 1. Preparar el archivo

Tres cosas que importan, y ninguna es negociable:

- **Formato `.ogg`**, estereo, 44.1 kHz. Si exportas en `.wav` el archivo pesa diez veces
  mas y GitHub lo rechaza por tamano.
- **Cuanto mas ligero mejor.** Un tema de tres minutos en `.ogg` pesa entre 3 y 5 MB, y
  con eso sobra.
- **El nombre exacto**, en minusculas. El juego busca estos nombres y ninguno mas:

| Nombre | Donde suena |
|---|---|
| `combate.ogg` | En cualquier arena que no tenga la suya. **Empieza por este**: con un solo archivo el juego entero ya tiene musica. |
| `menu.ogg` | Menu principal y seleccion de personaje |
| `arena1.ogg` | Arena de Tupac Amaru II |
| `arena2.ogg` ... `arena9.ogg` | Las demas arenas, en el orden del roster. `arena9` es la de Socrates. |

## 2. Como hacerlo llegar al repo

Tres rutas. La 1 no necesita instalar nada.

### Ruta 1: mandarselo a Munay (la mas rapida)

Mandas el `.ogg` por WhatsApp o Drive y el lo sube. Cero instalacion, cero configuracion.
Si vas a mandar una o dos pistas, esta es la buena.

### Ruta 2: GitHub Desktop (te vuelve autonomo)

Vale la pena si vas a mandar muchas. Una sola vez:

1. Instala **Git LFS** desde https://git-lfs.com (siguiente, siguiente).
2. Instala **GitHub Desktop** desde https://desktop.github.com
3. En GitHub Desktop: *File > Clone repository*, pestana URL, pega
   `mpassaqui-droid/smash-the-past`.

Y despues, cada vez:

1. Copia tu `.ogg` dentro de `SmashThePast/Assets/Audio/Resources/Musica/`.
2. GitHub Desktop lo detecta solo. Abajo a la izquierda escribe una linea de que es.
3. **Commit to main**, y arriba **Push origin**.

GitHub Desktop si sabe manejar LFS. Por eso funciona y la web no.

### Ruta 3: la web de GitHub

**Solo sirve para archivos que NO esten en LFS**, o sea texto: los `.md` como este o como
`REFERENCIAS.md`. Para `.ogg` no funciona, GitHub lo rechaza.

Para esos archivos de texto: boton **Add file > Upload files**, o el lapiz **Edit this
file**. Abajo elige siempre **Create a new branch and start a pull request**, asi no
escribes directo en la version buena.

## Por que ningun archivo abre el juego de un click

Esto sale cada vez que aparece una extension nueva, asi que queda escrito.

Un `.unity` no es el juego, y un `.uproject` tampoco. Son archivos de **texto** que solo
guardan **referencias**. Nuestro `PrototypeArena.unity` pesa **75 KB** y lleva dentro
**222 identificadores** que apuntan a archivos que estan en otro sitio: modelos, sonidos,
materiales, scripts. Todo eso junto pesa **26 MB**. El archivo de escena es el plano de
montaje, no las piezas. Fuera de su proyecto no abre nada, porque los 222 identificadores
no encuentran a que apuntar.

La linea "Tipo de archivo" de las propiedades de Windows es solo el nombre que Windows le
pone a la extension. No dice nada de lo que hay dentro. Un `.docx` pone "Documento de
Word" y tampoco lleva el Word adentro.

**No existe ningun archivo suelto que abra un proyecto de juego.** Ni en Unity, ni en
Unreal, ni en ningun motor. Un proyecto siempre es una carpeta con su arbol entero.

Lo que si existe, segun lo que quieras hacer:

| Quiero... | Uso |
|---|---|
| Ver el juego funcionando | El build. Un `.exe`, doble click y arranca. Es la release `v0.1.0-alpha` de Windows. |
| Meter musica | La subida por navegador de arriba. No hace falta ningun archivo del proyecto. |
| Oir mi musica dentro del juego antes de mandarla | El proyecto entero, clonado. No hay atajo. |

## Si quieres oirla en el juego antes de subirla

Hace falta Unity, y esta explicado en `docs/AUDIO.md`. Pero no es obligatorio: puedes
subir el archivo y que Munay te diga como suena.

# Subir musica sin instalar nada

Escrito para Diego. No hace falta git, ni clonar el repo, ni GitHub Desktop, ni abrir
Unity. Solo el navegador y tu cuenta de GitHub, que ya tiene permiso de escritura.

La musica es el unico trabajo del proyecto que no toca codigo: dejas el archivo con el
nombre correcto y suena solo.

## 1. Preparar el archivo

Tres cosas que importan, y ninguna es negociable:

- **Formato `.ogg`**, estereo, 44.1 kHz. Si exportas en `.wav` el archivo pesa diez veces
  mas y GitHub lo rechaza por tamano.
- **Menos de 25 MB.** Es el limite de la subida por navegador. Un tema de tres minutos en
  `.ogg` pesa entre 3 y 5 MB, asi que hay margen de sobra.
- **El nombre exacto**, en minusculas. El juego busca estos nombres y ninguno mas:

| Nombre | Donde suena |
|---|---|
| `combate.ogg` | En cualquier arena que no tenga la suya. **Empieza por este**: con un solo archivo el juego entero ya tiene musica. |
| `menu.ogg` | Menu principal y seleccion de personaje |
| `arena1.ogg` | Arena de Tupac Amaru II |
| `arena2.ogg` ... `arena9.ogg` | Las demas arenas, en el orden del roster. `arena9` es la de Socrates. |

## 2. Subirlo

1. Abre https://github.com/mpassaqui-droid/smash-the-past
2. Navega hasta la carpeta `SmashThePast/Assets/Audio/Resources/Musica/`. Se hace
   pinchando los nombres de carpeta uno detras de otro, como en el explorador.
3. Boton **Add file**, arriba a la derecha, y elige **Upload files**.
4. Arrastra tu `.ogg` a la zona grande.
5. Abajo aparecen dos opciones. Elige la **segunda**:
   *Create a new branch for this commit and start a pull request*.
   Deja el nombre que propone.
6. **Propose changes**, y en la pantalla siguiente **Create pull request**.

Ya esta. Munay lo ve, lo escucha en el juego y lo integra.

## Por que la segunda opcion y no la primera

La primera escribe directamente en `main`, que es la version buena del juego. La segunda
deja tu archivo en una copia aparte hasta que alguien lo mire. Si algo sale mal solo hay
que cerrar la propuesta, y `main` ni se entera.

## Lo que NO hay que hacer

- **No uses el boton "Download ZIP"** de GitHub para bajarte el proyecto. En este repo las
  imagenes y los sonidos estan en Git LFS, y ese ZIP trae archivos de texto de unos bytes
  en lugar de los de verdad. Unity no avisa claramente y parece que el proyecto esta roto.
- **No subas `.wav`.** Pasa del limite de 25 MB casi siempre.
- **No cambies el nombre** por algo mas bonito. `arena1.ogg` es `arena1.ogg`.

## Derechos

Solo musica tuya, CC0, o generada con un plan **de pago** que ceda derechos comerciales.
El plan gratuito de Suno y de Udio no los cede, y el juego se va a vender. Cada pista
necesita su nota en `Assets/Audio/Licenses/` diciendo de donde sale y con que licencia.
Si no sabes como, mandasela a Munay por escrito y el la anade.

## Si quieres oirla en el juego antes de subirla

Hace falta Unity, y esta explicado en `docs/AUDIO.md`. Pero no es obligatorio: puedes
subir el archivo y que Munay te diga como suena.

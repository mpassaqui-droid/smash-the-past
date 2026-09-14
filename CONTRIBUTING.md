# Como trabajamos aqui

Somos dos personas y un agente. Estas reglas existen para no pisarnos y para no
repetir errores que ya costaron caro.

## Bajarse el proyecto

**El boton "Download ZIP" de GitHub no sirve.** El repo guarda imagenes y sonidos en Git
LFS, y ese ZIP trae punteros de texto de unos bytes en lugar de los archivos: los retratos
y el audio llegan rotos y Unity no lo dice claramente. Hay que clonar.

En Windows, por linea de comandos:

```bash
winget install Git.Git          # si no esta ya
git lfs install                 # UNA vez por maquina, ANTES de clonar
git clone https://github.com/mpassaqui-droid/smash-the-past.git
```

Si prefieres interfaz grafica, **GitHub Desktop** hace lo mismo: Clone repository, pegar
la URL. Instala igualmente `git lfs install` antes, desde la terminal que trae.

Despues:

1. Unity Hub, e instalar **Unity 6000.0.83f1 exactamente**. Otra version reimporta el
   proyecto entero y mueve las escenas.
2. En Unity Hub, *Add project from disk* y elegir la carpeta **`SmashThePast/`** de dentro
   del repo, no la raiz.
3. La primera apertura tarda varios minutos: Unity importa todo y compila los shaders.

Comprobacion de que LFS funciono: `SmashThePast/Assets/UI/Portraits/TupacAmaruII.png` tiene
que pesar mas de un mega. Si pesa 130 bytes, es un puntero y falto `git lfs install`; se
arregla con `git lfs pull`.

## Antes de la primera linea de codigo

Leer `AGENTS.md`, `GDD.md` y `tasks/lessons.md` enteros. El ultimo es la lista de trampas
en las que ya cayo alguien aqui, y se lee en diez minutos.

## El flujo

- Rama propia y Pull Request. Nunca commitear directo en `main`.
- La CI corre las pruebas EditMode, construye el player, lo **lanza** y guarda una captura.
  Ojo: el runner es el Mac de Munay, asi que la CI solo avanza si su maquina esta encendida.
- Un PR se mergea con la CI en verde. Nada mas.

## Reglas duras

- **Cero simbolos nazis** en cualquier asset, animacion o escenario. El personaje de Hitler
  es una caricatura comica y esa linea no se discute: tambien es condicion para vender en
  Alemania.
- Una regla de gameplay ya establecida no se rompe sin avisar y sin actualizar la prueba
  que la vigila.
- Antes de dar algo por terminado, correr las pruebas y **reportar el numero real**
  ("88/88"), nunca "ya funciona".
- Un build que compila no demuestra que el juego arranque. Para lo visual y para el
  arranque existe la prueba de humo (`-humo captura.png`), usala.
- No editar la misma escena `.unity` o el mismo `.prefab` que otro este tocando esa semana.
  Avisar en `TASKS.md`.

## Quien es dueno de que

Escrito en `AGENTS.md`. Si necesitas tocar una carpeta que no es tuya, avisa antes.

## Mensajes de commit

En espanol, explicando **por que**, no solo que. Un mensaje bueno responde a la pregunta
que se hara quien lea ese diff dentro de seis meses. Sin `Co-Authored-By`.

## Cuando algo te muerde

Anadelo a `tasks/lessons.md` en cuanto lo resuelvas, con el porque. Ese archivo es la
memoria del proyecto y ha evitado ya varias horas perdidas.

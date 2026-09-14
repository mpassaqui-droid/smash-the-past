# Como trabajamos aqui

Somos dos personas y un agente. Estas reglas existen para no pisarnos y para no
repetir errores que ya costaron caro.

## Antes de la primera linea de codigo

1. Unity **6000.0.83f1 exactamente**. Otra version reimporta el proyecto y mueve las escenas.
2. `git lfs install` **antes** de clonar. Sin eso recibes archivos de texto de unos bytes
   en lugar de los retratos y el audio.
3. Leer `AGENTS.md`, `GDD.md` y `tasks/lessons.md` enteros. El ultimo es la lista de
   trampas en las que ya cayo alguien aqui.

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

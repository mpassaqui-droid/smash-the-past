# Smash the Past

Juego de pelea 1v1 local con personajes historicos. Nueve figuras que nunca se cruzaron
se reparten golpes en nueve arenas, cada una con su propio clima. Unity 6 + C#, 3D low
poly, sin multijugador en linea.

[![Unity tests](https://github.com/mpassaqui-droid/smash-the-past/actions/workflows/unity-tests.yml/badge.svg)](https://github.com/mpassaqui-droid/smash-the-past/actions/workflows/unity-tests.yml)

![Combate](docs/img/combate.png)

## Jugar ahora

La ultima version compilada esta en [Releases](https://github.com/mpassaqui-droid/smash-the-past/releases).
No hace falta instalar Unity ni compilar nada.

- **Windows**: descomprimir y doble click en `SmashThePast.exe`. Windows avisara de que no
  conoce al editor: *Mas informacion* y luego *Ejecutar de todas formas*. El ejecutable no
  esta firmado todavia.
- **macOS**: **click derecho sobre la app y Abrir**, no doble click. Luego *Abrir* otra vez
  en el aviso. Misma razon: sin firma de Apple.

## Controles

| | Jugador 1 | Jugador 2 | Mando |
|---|---|---|---|
| Moverse | `A` `D` | flechas | stick / cruceta |
| Saltar | `W` | flecha arriba | A (sur) |
| Golpe ligero | `J` | `1` | Y (norte) |
| Golpe fuerte | `K` | `2` | B (este) |
| Guardia | `L` | `3` | X (oeste) |
| Presa | `U` | `4` | gatillo izquierdo |
| Special | `I` | `0` | gatillo derecho |
| Pausa | `Esc` | `Esc` | Start |

`M` enciende y apaga a la maquina en el puesto del jugador 2. `N` cambia su dificultad
entre facil, normal y dificil. El jugador 2 tambien admite el teclado numerico.

## Como funciona el combate

Tres cosas que hacer y un coste de tiempo por cada error:

- **Golpear**. Ligero es rapido, fuerte pega mas y deja mas vendido.
- **Parar**. Bloquear reduce el dano a una rozadura, pero quien para pierde el turno 0,12 s
  y no puede moverse.
- **Agarrar**. La presa atraviesa la guardia, que es justo para lo que sirve: es la
  respuesta al que se queda tapado. Alcance corto, y fallarla deja vendido 0,85 s.

Encajar un golpe cuesta 0,25 s de turno, 0,38 s si es fuerte, 0,5 s en una presa. Esa
diferencia entre parar y encajar es lo que premia acertar.

Los combates son **al mejor de tres asaltos**. Cada personaje tiene un special propio
ligado a por que se le recuerda, y frases de victoria y derrota escritas para el.

![Asalto](docs/img/asalto.png)

## El roster

Tupac Amaru II, Genghis Khan, Miyamoto Musashi, Gandhi, Rasputin, Napoleon Bonaparte,
Cleopatra, Abraham Lincoln y Socrates. Hitler existe como DLC de pago aparte, con
verificacion de edad, caricatura comica y **cero simbolos nazis en ningun asset**.

![Seleccion](docs/img/seleccion.png)

## Las arenas

Nueve escenarios que comparten geometria pero no atmosfera: el sol de altura del patio
andino, la noche azul cerrada del palacio de Rasputin, el desierto dorado del de Cleopatra.
La luz es lo que las separa.

| | |
|---|---|
| ![Rasputin](docs/img/arena-rasputin.png) | ![Cleopatra](docs/img/arena-cleopatra.png) |

## Trabajar en el codigo

Hace falta **Unity 6000.0.83f1 exactamente**. Otra version reimporta el proyecto y mueve
las escenas.

```bash
git lfs install          # ANTES de clonar: las imagenes y el audio van en LFS
git clone https://github.com/mpassaqui-droid/smash-the-past.git
```

Abrir la carpeta `SmashThePast/` desde Unity Hub, no la raiz del repo.

> El boton **Download ZIP** de GitHub no sirve: no trae los archivos de LFS, solo punteros
> de texto, y los retratos y el audio llegan rotos. Hay que clonar. Los pasos completos,
> incluida la via con GitHub Desktop, estan en [`CONTRIBUTING.md`](CONTRIBUTING.md).

Para anadir musica o sonidos hay una guia aparte: [`docs/AUDIO.md`](docs/AUDIO.md).

**Antes de tocar nada**, leer en este orden: [`AGENTS.md`](AGENTS.md) (reglas de trabajo y
de quien es cada carpeta), [`GDD.md`](GDD.md) (el diseno cerrado), [`TASKS.md`](TASKS.md)
(el tablero), [`tasks/lessons.md`](tasks/lessons.md) (los errores ya cometidos aqui, leerlo
entero ahorra repetirlos) y [`PLAN.md`](PLAN.md) (a donde vamos).

### Pruebas

```bash
Unity -batchmode -projectPath SmashThePast -runTests -testPlatform EditMode \
      -testResults resultados.xml -logFile pruebas.log
```

Sin `-quit`: con esa bandera la corrida se salta y no escribe resultados.

### Construir

Desde el menu `Smash the Past` del editor, o en linea de comandos:

```bash
Unity -batchmode -quit -projectPath SmashThePast -logFile build.log \
      -executeMethod SmashThePast.Characters.Editor.PrototypeBuild.BuildMacOS
```

`BuildWindows` para el `.exe`. El build de Windows necesita el modulo *Windows Build
Support* instalado en el editor.

### Prueba de humo

Un build que compila no demuestra que el juego arranque. El player acepta banderas para
comprobarlo sin tocar el teclado:

```bash
SmashThePast.app/Contents/MacOS/SmashThePast -humo captura.png -pelea 12
```

Arranca, deja correr un combate doce segundos, guarda una captura de lo que se ve de
verdad y se cierra. `-escenario N` recorre las arenas, `-golpe 1` dispara el empujon de
camara. La CI lo ejecuta despues de cada build y falla si el juego no abre.

## Estado

Combate cerrado (golpes, guardia, presa, aturdimiento, specials, asaltos), nueve
personajes con datos y frases, nueve arenas, maquina con tres niveles, seleccion con
retratos, URP con contorno y post proceso. **88 pruebas** en verde en cada PR.

Falta menu principal, opciones, modelos 3D de verdad, musica y la tienda. El detalle y el
orden estan en [`PLAN.md`](PLAN.md).

## Quien lo hace

Munay Passaqui y Diego. Ver [`CONTRIBUTING.md`](CONTRIBUTING.md) para el flujo de trabajo.

---

Codigo bajo licencia MIT. La musica y los efectos de Diego quedan fuera de esa licencia: son suyos y se usan con su permiso. Ver [`LICENSE`](LICENSE) y [`ASSETS-LICENSE.md`](ASSETS-LICENSE.md).

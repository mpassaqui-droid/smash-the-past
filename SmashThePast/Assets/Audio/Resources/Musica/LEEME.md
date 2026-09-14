# Musica

Deja aqui los archivos y suenan solos, sin tocar codigo.

- `menu.ogg` suena en el menu y en la pantalla de seleccion.
- `arena1.ogg` ... `arena9.ogg` suenan en cada arena, en el mismo orden que el roster
  (arena1 es la de Tupac Amaru II, arena9 la de Socrates).
- `combate.ogg` es el respaldo: suena en cualquier arena que todavia no tenga la suya.

Formato: `.ogg`, estereo, 44.1 kHz. En el importador de Unity dejarlo en **Streaming** y
**Load in background**: son archivos largos y no tiene sentido cargarlos enteros en memoria.

Requisito legal: solo musica propia, CC0, o generada con un plan **de pago** que ceda
derechos comerciales (el plan gratuito de Suno y Udio NO los cede). Cada pista necesita su
nota en `Assets/Audio/Licenses/` diciendo de donde sale y bajo que licencia.

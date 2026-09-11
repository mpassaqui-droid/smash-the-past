using System;

namespace SmashThePast.Core
{
    /// <summary>
    /// Vida de un personaje y resolucion de golpes, incluyendo chip damage al bloquear.
    /// C# puro, sin dependencia de Unity, para poder testearlo sin abrir una escena.
    /// Regla fija del GDD: bloquear nunca anula el dano del todo (evita el turtling infinito).
    /// </summary>
    public class HealthSystem
    {
        /// <summary>El dano bloqueado se reduce a 1/N del original (division entera, sin floats
        /// para evitar imprecision: 20*0.1f da 2.0000002, cuyo Ceiling da 3 en vez de 2).</summary>
        public const int BlockedDamageDivisor = 10;

        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public HealthSystem(int maxHealth)
        {
            if (maxHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "La vida maxima debe ser mayor a 0.");

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        /// <summary>Aplica un golpe y devuelve el dano efectivo realmente descontado.</summary>
        public int ApplyHit(int rawDamage, bool defenderIsBlocking)
        {
            if (rawDamage < 0)
                throw new ArgumentOutOfRangeException(nameof(rawDamage), "El dano no puede ser negativo.");

            if (IsDead)
                return 0;

            int effectiveDamage = defenderIsBlocking
                ? CeilDiv(rawDamage, BlockedDamageDivisor)
                : rawDamage;

            CurrentHealth = Math.Max(0, CurrentHealth - effectiveDamage);
            return effectiveDamage;
        }

        private static int CeilDiv(int value, int divisor) => (value + divisor - 1) / divisor;

        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }
    }
}

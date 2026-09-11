using System;

namespace SmashThePast.Core
{
    /// <summary>
    /// Barra de special: se llena al dar o recibir dano (GDD: "Controles").
    /// Al llegar al maximo, el special esta disponible una vez.
    /// C# puro, testable sin escena.
    /// </summary>
    public class SpecialMeter
    {
        public const int MaxValue = 100;

        public int CurrentValue { get; private set; }
        public bool IsReady => CurrentValue >= MaxValue;

        public void Fill(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "No se puede llenar con un valor negativo.");

            CurrentValue = Math.Min(MaxValue, CurrentValue + amount);
        }

        /// <summary>Si esta lista, la consume y devuelve true. Si no, no hace nada y devuelve false.</summary>
        public bool TryConsume()
        {
            if (!IsReady)
                return false;

            CurrentValue = 0;
            return true;
        }
    }
}

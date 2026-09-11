using UnityEngine;

namespace SmashThePast.Core
{
    /// <summary>
    /// Datos de un personaje jugable. Diego crea una instancia de este asset por personaje
    /// en Assets/Characters/. La logica de combate (Fighter) es la MISMA para todos;
    /// lo unico que cambia entre personajes vive aqui (GDD: "Arquitectura de personajes").
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Smash the Past/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identidad")]
        public string characterName;
        [TextArea] public string bio;

        [Header("Stats base (kit universal, ver GDD: Moveset)")]
        public int maxHealth = 100;
        public float moveSpeed = 5f;
        public float jumpForce = 8f;
        public int lightAttackDamage = 5;
        public int heavyAttackDamage = 10;

        [Header("Special (unico por personaje, ver tabla del GDD)")]
        public string specialName;
        [TextArea] public string specialDescription;
        public GameObject specialEffectPrefab;

        [Header("Frases post-combate")]
        public string[] winQuotes;
        public string[] loseQuotes;

        [Header("Monetizacion")]
        [Tooltip("Solo true para Hitler: unlock de pago aparte con verificacion de edad.")]
        public bool isPaidDLC = false;
    }
}

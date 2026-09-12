using SmashThePast.Core;
using UnityEditor;
using UnityEngine;

namespace SmashThePast.Characters.Editor
{
    public static class DlcContentBuilder
    {
        private const string CharacterPath = "Assets/Characters/Hitler.asset";
        private const string OfferPath = "Assets/Characters/HitlerDlc.asset";

        [MenuItem("Smash the Past/Generar contenido DLC")]
        public static void Build()
        {
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterPath);
            if (character == null)
            {
                character = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(character, CharacterPath);
            }
            character.characterName = "Hitler";
            character.bio = "Caricatura satirica de un dictador derrotado. Entra a la arena solo para que el juego se ria de su berrinche.";
            character.specialName = "Berlin Tantrum";
            character.specialDescription = "Berrinche ridiculo que aturde alrededor y causa un golpe corto. No glorifica ni usa insignias o gestos nazis.";
            character.winQuotes = new[] { "Ni este berrinche arreglo Berlin.", "Una victoria pequena para un ego enorme." };
            character.loseQuotes = new[] { "La historia vuelve a cerrar la puerta.", "Otro discurso, cero aplausos." };
            character.isPaidDLC = true;
            EditorUtility.SetDirty(character);

            var offer = AssetDatabase.LoadAssetAtPath<DlcOffer>(OfferPath);
            if (offer == null)
            {
                offer = ScriptableObject.CreateInstance<DlcOffer>();
                AssetDatabase.CreateAsset(offer, OfferPath);
            }
            offer.offerId = "smash-the-past-hitler";
            offer.storeSku = "";
            offer.displayName = "Hitler DLC";
            offer.priceUsd = 0.99f;
            offer.minimumAge = 15;
            offer.character = character;
            EditorUtility.SetDirty(offer);
            BuildPrefabs(character);
            AssetDatabase.SaveAssets();
            Debug.Log("DLC_CONTENT: character, offer and 2 neutral prefabs generated.");
        }

        private static void BuildPrefabs(CharacterData character)
        {
            Material coat = PrototypePrefabBuilder.Material("Hitler", new Color(0.18f, 0.2f, 0.22f));
            Material hair = PrototypePrefabBuilder.Material("HitlerCabello", new Color(0.07f, 0.06f, 0.05f));
            for (int slot = 1; slot <= 2; slot++)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Characters/Prefabs/NapoleonBonaparte_P{slot}.prefab");
                var root = (GameObject)PrefabUtility.InstantiatePrefab(source);
                root.name = "Hitler_P" + slot;
                PrototypePrefabBuilder.SetObject(root.GetComponent<Fighter>(), "characterData", character);
                var combat = root.GetComponent<Combatant>();
                combat.Kind = SpecialKind.Hitler;
                combat.PlayerIndex = slot - 1;
                var model = root.transform.Find("Modelo");
                foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                    if (renderer.name != "Cabeza") renderer.sharedMaterial = coat;
                Remove(model, "Bicornio");
                Remove(model, "Escarapela");
                PrototypePrefabBuilder.Part(model, "Cabello", PrimitiveType.Sphere, new Vector3(-0.08f, 2.18f, 0), new Vector3(0.62f, 0.28f, 0.66f), hair);
                PrototypePrefabBuilder.Part(model, "Bigote", PrimitiveType.Cube, new Vector3(0.3f, 1.9f, 0), new Vector3(0.08f, 0.07f, 0.28f), hair);
                PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Characters/Prefabs/Hitler_P{slot}.prefab");
                Object.DestroyImmediate(root);
            }
        }

        private static void Remove(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }
    }
}

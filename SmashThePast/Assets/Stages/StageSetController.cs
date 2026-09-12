using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Stages
{
    public sealed class StageSetController : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private GameObject[] stages;
        private int activeIndex = -1;

        public GameObject[] Stages => stages;

        private void OnEnable()
        {
            if (match != null) match.Changed += Refresh;
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (match != null) match.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (match == null || match.PlayerOne == null || match.PlayerOne.Data == null || match.Roster == null) return;
            int selected = System.Array.IndexOf(match.Roster, match.PlayerOne.Data);
            if (selected < 0 || selected == activeIndex) return;
            activeIndex = Mathf.Clamp(selected, 0, stages.Length - 1);
            for (int index = 0; index < stages.Length; index++)
                if (stages[index] != null) stages[index].SetActive(index == activeIndex);
        }
    }
}

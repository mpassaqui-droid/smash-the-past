using UnityEngine;

namespace SmashThePast.Stages
{
    [RequireComponent(typeof(Camera))]
    public class ArenaCamera : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        private Camera view;
        private void Awake() => view = GetComponent<Camera>();
        private void LateUpdate()
        {
            if (match == null || match.PlayerOne == null || match.PlayerTwo == null) return;
            float center = Mathf.Clamp((match.PlayerOne.transform.position.x + match.PlayerTwo.transform.position.x) / 2, -2, 2);
            transform.position = Vector3.Lerp(transform.position, new Vector3(center, 5.3f, -12), Time.unscaledDeltaTime * 4);
            transform.rotation = Quaternion.Euler(15, 0, 0);
            float span = Mathf.Abs(match.PlayerOne.transform.position.x - match.PlayerTwo.transform.position.x) + 4.5f;
            float size = Mathf.Max(4.2f, span / (2 * Mathf.Max(0.5f, view.aspect)));
            view.orthographicSize = Mathf.Lerp(view.orthographicSize, size, Time.unscaledDeltaTime * 4);
        }
    }
}

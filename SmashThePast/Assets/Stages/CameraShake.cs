using UnityEngine;

namespace SmashThePast.Stages
{
    [DefaultExecutionOrder(1000)]
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float strength = 0.12f;
        private Vector3 previousOffset;
        private float trauma;

        public void AddShake(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        private void LateUpdate()
        {
            transform.position -= previousOffset;
            trauma = Mathf.MoveTowards(trauma, 0, Time.unscaledDeltaTime * 2.8f);
            previousOffset = Random.insideUnitSphere * (trauma * strength);
            transform.position += previousOffset;
        }
    }
}

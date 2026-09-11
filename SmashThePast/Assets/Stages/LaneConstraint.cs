using UnityEngine;

namespace SmashThePast.Stages
{
    [RequireComponent(typeof(CharacterController))]
    public class LaneConstraint : MonoBehaviour
    {
        private CharacterController controller;
        private void Awake() => controller = GetComponent<CharacterController>();
        private void LateUpdate()
        {
            if (!controller.enabled) return;
            float x = Mathf.Clamp(transform.position.x, -8, 8);
            controller.Move(new Vector3(x - transform.position.x, 0, -transform.position.z));
        }
    }
}

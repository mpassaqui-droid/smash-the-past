using UnityEngine;
using UnityEngine.InputSystem;

namespace SmashThePast.Core
{
    /// <summary>
    /// Traduce input crudo (teclado o mando) a llamadas sobre Fighter.
    /// Polling directo, sin .inputactions asset, para mantener el prototipo simple.
    /// Jugador 1: WASD + salto W, J puñete, K patada, L bloqueo, I special (o Gamepad 1).
    /// Jugador 2: flechas + salto arriba, Numpad1 puñete, Numpad2 patada, Numpad3 bloqueo, Numpad0 special (o Gamepad 2).
    /// </summary>
    [RequireComponent(typeof(Fighter))]
    public class PlayerInputController : MonoBehaviour
    {
        public enum PlayerSlot { Player1, Player2 }

        [SerializeField] private PlayerSlot playerSlot = PlayerSlot.Player1;

        private Fighter _fighter;

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
        }

        private void Update()
        {
            Gamepad pad = GetAssignedGamepad();

            float horizontal = pad != null ? pad.leftStick.x.ReadValue() : ReadKeyboardHorizontal();
            _fighter.Move(horizontal);

            bool jumpPressed = pad != null ? pad.buttonSouth.wasPressedThisFrame : ReadKeyboardJump();
            if (jumpPressed) _fighter.Jump();

            bool blocking = pad != null ? pad.buttonWest.isPressed : ReadKeyboardBlock();
            _fighter.SetBlocking(blocking);

            bool lightAttack = pad != null ? pad.buttonNorth.wasPressedThisFrame : ReadKeyboardLightAttack();
            if (lightAttack) _fighter.PerformLightAttack();

            bool heavyAttack = pad != null ? pad.buttonEast.wasPressedThisFrame : ReadKeyboardHeavyAttack();
            if (heavyAttack) _fighter.PerformHeavyAttack();

            bool specialPressed = pad != null ? pad.rightTrigger.wasPressedThisFrame : ReadKeyboardSpecial();
            if (specialPressed) _fighter.TryUseSpecial();
        }

        private Gamepad GetAssignedGamepad()
        {
            int index = playerSlot == PlayerSlot.Player1 ? 0 : 1;
            return Gamepad.all.Count > index ? Gamepad.all[index] : null;
        }

        private bool IsPlayer1 => playerSlot == PlayerSlot.Player1;

        private float ReadKeyboardHorizontal()
        {
            var kb = Keyboard.current;
            if (kb == null) return 0f;
            bool left = IsPlayer1 ? kb.aKey.isPressed : kb.leftArrowKey.isPressed;
            bool right = IsPlayer1 ? kb.dKey.isPressed : kb.rightArrowKey.isPressed;
            return (right ? 1f : 0f) - (left ? 1f : 0f);
        }

        private bool ReadKeyboardJump()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return IsPlayer1 ? kb.wKey.wasPressedThisFrame : kb.upArrowKey.wasPressedThisFrame;
        }

        private bool ReadKeyboardBlock()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return IsPlayer1 ? kb.lKey.isPressed : kb.numpad3Key.isPressed;
        }

        private bool ReadKeyboardLightAttack()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return IsPlayer1 ? kb.jKey.wasPressedThisFrame : kb.numpad1Key.wasPressedThisFrame;
        }

        private bool ReadKeyboardHeavyAttack()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return IsPlayer1 ? kb.kKey.wasPressedThisFrame : kb.numpad2Key.wasPressedThisFrame;
        }

        private bool ReadKeyboardSpecial()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return IsPlayer1 ? kb.iKey.wasPressedThisFrame : kb.numpad0Key.wasPressedThisFrame;
        }
    }
}

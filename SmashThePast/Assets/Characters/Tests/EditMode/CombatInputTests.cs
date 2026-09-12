using System.Collections;
using NUnit.Framework;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace SmashThePast.Characters.Tests
{
    public class CombatInputTests
    {
        private InputTestFixture devices;
        private Keyboard keyboard;
        private Gamepad firstPad, secondPad;
        private Combatant first, second;
        private GameObject floor;

        private static void Poll(Combatant combatant)
            => typeof(Combatant).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(combatant, null);

        private static void PollInput(Combatant combatant)
            => typeof(Combatant).GetMethod("ReadInput", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(combatant, null);

        private void Press(UnityEngine.InputSystem.Controls.ButtonControl key)
        {
            devices.Press(key);
            InputSystem.Update();
        }

        private void Release(UnityEngine.InputSystem.Controls.ButtonControl key)
        {
            devices.Release(key);
            InputSystem.Update();
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return new EnterPlayMode();
            devices = new InputTestFixture();
            devices.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            firstPad = InputSystem.AddDevice<Gamepad>();
            secondPad = InputSystem.AddDevice<Gamepad>();
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(30, 1, 10);
            first = Create(0, -1);
            second = Create(1, 1);
            first.Opponent = second;
            second.Opponent = first;
            yield return WaitFrames(0.2f);
        }

        private static Combatant Create(int player, float x)
        {
            var result = new GameObject("Input " + player).AddComponent<Combatant>();
            result.PlayerIndex = player;
            result.transform.position = new Vector3(x, 0.1f, 0);
            var controller = result.GetComponent<CharacterController>();
            controller.height = 2;
            controller.center = Vector3.up;
            result.gameObject.AddComponent<PlayerInputController>();
            return result;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            Object.Destroy(first.gameObject);
            Object.Destroy(second.gameObject);
            Object.Destroy(floor);
            devices.TearDown();
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator KeyboardAttacksAndBlockRemainUsableWithTwoIdlePads()
        {
            int lights = 0, heavies = 0;
            first.Fighter.OnLightAttackPerformed += () => lights++;
            first.Fighter.OnHeavyAttackPerformed += () => heavies++;
            Press(keyboard.jKey);
            Poll(first);
            Release(keyboard.jKey);
            Press(keyboard.kKey);
            Poll(first);
            Release(keyboard.kKey);
            Assert.AreEqual(1, lights);
            Assert.AreEqual(1, heavies);
            Press(keyboard.lKey);
            Press(keyboard.jKey);
            Poll(first);
            Assert.IsTrue(first.Fighter.IsBlocking);
            Assert.AreEqual(1, lights);
            Release(keyboard.jKey);
            Release(keyboard.lKey);
            Poll(first);
            Assert.IsFalse(first.Fighter.IsBlocking);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BothPlayerTwoKeyboardRowsExposeAttacksAndSpecial()
        {
            int lights = 0, heavies = 0, specials = 0;
            second.Fighter.OnLightAttackPerformed += () => lights++;
            second.Fighter.OnHeavyAttackPerformed += () => heavies++;
            second.SpecialActivated += _ => specials++;
            second.Kind = SpecialKind.Socrates;
            foreach (var key in new[] { keyboard.digit1Key, keyboard.numpad1Key, keyboard.digit2Key, keyboard.numpad2Key })
            {
                Press(key);
                PollInput(second);
                Release(key);
            }
            foreach (var key in new[] { keyboard.digit0Key, keyboard.numpad0Key })
            {
                second.Fighter.Special.Fill(100);
                Press(key);
                Poll(second);
                Release(key);
            }
            Assert.AreEqual(2, lights);
            Assert.AreEqual(2, heavies);
            Assert.AreEqual(2, specials);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EachPadControlsOnlyItsPlayerAndTriggerUsesSpecial()
        {
            int firstLight = 0, secondHeavy = 0, specials = 0;
            first.Fighter.OnLightAttackPerformed += () => firstLight++;
            second.Fighter.OnHeavyAttackPerformed += () => secondHeavy++;
            second.Kind = SpecialKind.Socrates;
            second.SpecialActivated += _ => specials++;
            second.Fighter.Special.Fill(100);
            devices.Press(firstPad.buttonNorth);
            devices.Press(secondPad.buttonEast);
            devices.Press(secondPad.rightTrigger);
            InputSystem.Update();
            Poll(first);
            Poll(second);
            Assert.AreEqual(1, firstLight);
            Assert.AreEqual(1, secondHeavy);
            Assert.AreEqual(1, specials);
            Release(firstPad.buttonNorth);
            Release(secondPad.buttonEast);
            Release(secondPad.rightTrigger);
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyboardMovesJumpsAndBlocksWithControllerConnected()
        {
            float before = first.transform.position.x;
            Press(keyboard.dKey);
            Poll(first);
            Assert.Greater(first.transform.position.x, before);
            Press(keyboard.lKey);
            before = first.transform.position.x;
            Poll(first);
            Assert.AreEqual(before, first.transform.position.x);
            Release(keyboard.lKey);
            Release(keyboard.dKey);
            yield return null;
            float height = first.transform.position.y;
            Press(keyboard.wKey);
            Poll(first);
            Release(keyboard.wKey);
            yield return WaitFrames(0.15f);
            Assert.Greater(first.transform.position.y, height + 0.001f);
        }

        [UnityTest]
        public IEnumerator SpecialKeyRequiresChargeAndPassiveDoesNotConsumeMeter()
        {
            int specials = 0;
            first.SpecialActivated += _ => specials++;
            Press(keyboard.iKey);
            Poll(first);
            Release(keyboard.iKey);
            Assert.AreEqual(0, specials);
            first.Fighter.Special.Fill(100);
            first.Kind = SpecialKind.Rasputin;
            Press(keyboard.iKey);
            Poll(first);
            Release(keyboard.iKey);
            Assert.AreEqual(100, first.Fighter.Special.CurrentValue);
            first.Kind = SpecialKind.Socrates;
            Press(keyboard.iKey);
            Poll(first);
            Release(keyboard.iKey);
            Assert.AreEqual(1, specials);
            Assert.AreEqual(0, first.Fighter.Special.CurrentValue);
            yield return null;
        }

        private static IEnumerator WaitFrames(float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}

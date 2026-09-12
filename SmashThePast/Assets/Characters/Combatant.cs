using System;
using System.Collections;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmashThePast.Characters
{
    [RequireComponent(typeof(Fighter))]
    [DefaultExecutionOrder(-100)]
    public class Combatant : MonoBehaviour
    {
        [SerializeField] private SpecialKind kind;
        [SerializeField, Range(0, 1)] private int playerIndex;
        private PlayerInputController input;
        private bool combatEnabled = true, survived, specialRunning;
        private float stunnedUntil, invulnerableUntil, invertedUntil, lockedUntil;
        private int generation;

        public SpecialKind Kind { get => kind; set => kind = value; }
        public int PlayerIndex { get => playerIndex; set => playerIndex = Mathf.Clamp(value, 0, 1); }
        public Fighter Fighter => GetComponent<Fighter>();
        public Combatant Opponent { get; set; }
        public bool CombatEnabled
        {
            get => combatEnabled;
            set
            {
                if (combatEnabled == value) return;
                combatEnabled = value;
                if (!value) CancelRuntime();
                UpdateInput();
            }
        }
        private bool Alive => isActiveAndEnabled && combatEnabled && Fighter.isActiveAndEnabled
            && Fighter.Health != null && !Fighter.Health.IsDead;
        public bool IsStunned => Alive && Time.time < stunnedUntil;
        public bool IsInvulnerable => Alive && Time.time < invulnerableUntil;
        public bool ControlsInverted => Alive && Time.time < invertedUntil;
        public bool SurvivalAvailable => Kind == SpecialKind.Rasputin && !survived;
        public bool CanAct => Alive && Time.timeScale > 0 && !IsStunned && Time.time >= lockedUntil;
        private bool TopRowBlock => PlayerIndex == 1
            && Keyboard.current != null && Keyboard.current.digit3Key.isPressed;
        public event Action<SpecialKind> SpecialActivated;

        private void OnEnable()
        {
            Fighter.OnSpecialUsed += UseSpecial;
            Fighter.OnKnockedOut += KnockedOut;
            UpdateInput();
        }

        private void OnDisable()
        {
            Fighter.OnSpecialUsed -= UseSpecial;
            Fighter.OnKnockedOut -= KnockedOut;
            CancelRuntime();
            UpdateInput();
        }

        private void KnockedOut()
        {
            CancelRuntime();
            UpdateInput();
        }

        private void CancelRuntime()
        {
            generation++;
            StopAllCoroutines();
            specialRunning = false;
            stunnedUntil = invulnerableUntil = invertedUntil = lockedUntil = 0;
            Fighter.SetBlocking(false);
        }

        private void UpdateInput()
        {
            if (input == null) input = GetComponent<PlayerInputController>();
            // La capa externa resuelve teclado y mando juntos, sin duplicar comandos de Core.
            if (input != null) input.enabled = false;
            if (!CanAct) Fighter.SetBlocking(false);
        }

        private void Update()
        {
            if (!Alive && specialRunning) CancelRuntime();
            UpdateInput();
            if (CanAct) ReadInput();
        }

        private void ReadInput()
        {
            var pad = Gamepad.all.Count > PlayerIndex ? Gamepad.all[PlayerIndex] : null;
            var kb = Keyboard.current;
            bool first = PlayerIndex == 0;
            bool block = (pad != null && pad.buttonWest.isPressed) || (kb != null &&
                (first ? kb.lKey.isPressed : kb.digit3Key.isPressed || kb.numpad3Key.isPressed));
            Fighter.SetBlocking(block);
            float horizontal = kb == null ? 0 :
                ((first ? kb.dKey : kb.rightArrowKey).isPressed ? 1 : 0)
                - ((first ? kb.aKey : kb.leftArrowKey).isPressed ? 1 : 0);
            if (pad != null)
            {
                float stick = pad.leftStick.x.ReadValue();
                horizontal += Mathf.Abs(stick) > 0.1f ? stick : pad.dpad.x.ReadValue();
            }
            if ((pad != null && pad.buttonSouth.wasPressedThisFrame) || (kb != null &&
                (first ? kb.wKey.wasPressedThisFrame : kb.upArrowKey.wasPressedThisFrame))) Fighter.Jump();
            Fighter.Move(Mathf.Clamp(horizontal, -1, 1) * (ControlsInverted ? -1 : 1));
            if ((pad != null && pad.buttonNorth.wasPressedThisFrame) || (kb != null &&
                (first ? kb.jKey.wasPressedThisFrame : kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)))
                Fighter.PerformLightAttack();
            if ((pad != null && pad.buttonEast.wasPressedThisFrame) || (kb != null &&
                (first ? kb.kKey.wasPressedThisFrame : kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)))
                Fighter.PerformHeavyAttack();
            if ((pad != null && pad.rightTrigger.wasPressedThisFrame) || (kb != null &&
                (first ? kb.iKey.wasPressedThisFrame : kb.digit0Key.wasPressedThisFrame || kb.numpad0Key.wasPressedThisFrame)))
                TrySpecial();
        }

        public bool TrySpecial()
        {
            if (!CanAct || specialRunning || Kind == SpecialKind.Rasputin || !Fighter.Special.IsReady) return false;
            Fighter.TryUseSpecial();
            return true;
        }

        public void ChargeFromDamage(int damage)
        {
            if (damage > 0) Fighter.Special.Fill((int)Math.Min((long)damage * 2, SpecialMeter.MaxValue));
        }

        public int ReceiveDamage(int rawDamage)
        {
            if (rawDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            if (!Alive || Time.timeScale <= 0 || IsInvulnerable) return 0;
            if (CanAct && TopRowBlock) Fighter.SetBlocking(true);
            int before = Fighter.Health.CurrentHealth;
            int divisor = Fighter.IsBlocking ? HealthSystem.BlockedDamageDivisor : 1;
            // Evita desbordar la suma que Core usa para redondear el bloqueo.
            rawDamage = Math.Min(rawDamage, int.MaxValue - (divisor - 1));
            long effective = ((long)rawDamage + divisor - 1) / divisor;
            bool survival = Kind == SpecialKind.Rasputin && !survived && effective >= before;
            if (survival)
            {
                survived = true;
                // Con 1 HP se envia cero, incluso bloqueando. Nunca se restaura vida.
                rawDamage = (int)Math.Min(rawDamage, (long)(before - 1) * divisor);
            }
            Fighter.ReceiveHit(rawDamage);
            int actual = before - Fighter.Health.CurrentHealth;
            if (actual > 0) Fighter.Special.Fill(actual);
            if (survival) SpecialActivated?.Invoke(SpecialKind.Rasputin);
            return actual;
        }

        private void UseSpecial()
        {
            if (!CanAct || specialRunning || Kind == SpecialKind.Rasputin)
            {
                Fighter.Special.Fill(SpecialMeter.MaxValue);
                return;
            }
            specialRunning = true;
            SpecialKind activated = Kind;
            StartCoroutine(RunSpecial(activated, Opponent, Opponent != null ? Opponent.generation : -1));
            SpecialActivated?.Invoke(activated);
        }

        private bool ValidTarget(Combatant target, int round, float range)
        {
            return Alive && !IsStunned && Time.timeScale > 0 && target != null && target != this
                && target == Opponent && target.Alive && target.generation == round
                && Vector3.Distance(transform.position, target.transform.position) <= range;
        }

        private int Hit(Combatant target, int round, int damage, float range)
        {
            if (!ValidTarget(target, round, range)) return 0;
            int actual = target.ReceiveDamage(damage);
            ChargeFromDamage(actual);
            return actual;
        }

        private IEnumerator RunSpecial(SpecialKind activated, Combatant target, int round)
        {
            switch (activated)
            {
                case SpecialKind.Tupac:
                    if (Hit(target, round, 12, 3) > 0 && target.Alive)
                    {
                        target.StopAllCoroutines();
                        target.specialRunning = false;
                        target.lockedUntil = 0;
                        target.stunnedUntil = Time.time + 0.8f;
                        target.UpdateInput();
                    }
                    break;
                case SpecialKind.Genghis:
                    for (int arrow = 0; arrow < 3; arrow++)
                    {
                        yield return new WaitForSeconds(0.15f);
                        Hit(target, round, 8, 16);
                    }
                    break;
                case SpecialKind.Musashi:
                    lockedUntil = Time.time + 0.6f;
                    UpdateInput();
                    var controller = GetComponent<CharacterController>();
                    float direction = target != null ? Mathf.Sign(target.transform.position.x - transform.position.x) : 1;
                    for (int slash = 0; slash < 3; slash++)
                    {
                        float end = Time.time + 0.12f;
                        while (Time.time < end)
                        {
                            if (!Alive || IsStunned) yield break;
                            if (controller != null && controller.enabled)
                                controller.Move(Vector3.right * (direction * 9 * Time.deltaTime));
                            yield return null;
                        }
                        Hit(target, round, 9, 2.5f);
                    }
                    lockedUntil = 0;
                    break;
                case SpecialKind.Gandhi:
                    invulnerableUntil = lockedUntil = Time.time + 1.5f;
                    UpdateInput();
                    yield return new WaitForSeconds(1.5f);
                    Hit(target, round, 30, 3);
                    break;
                case SpecialKind.Napoleon:
                    yield return new WaitForSeconds(0.65f);
                    Hit(target, round, 28, 18);
                    break;
                case SpecialKind.Cleopatra:
                    if (Hit(target, round, 4, 2.5f) <= 0) break;
                    for (int tick = 0; tick < 4; tick++)
                    {
                        yield return new WaitForSeconds(0.5f);
                        Hit(target, round, 5, float.PositiveInfinity);
                    }
                    break;
                case SpecialKind.Lincoln:
                    yield return new WaitForSeconds(0.2f);
                    Hit(target, round, 10, 12);
                    yield return new WaitForSeconds(0.45f);
                    Hit(target, round, 10, 12);
                    break;
                case SpecialKind.Socrates:
                    if (ValidTarget(target, round, 10) && !target.IsInvulnerable)
                        target.invertedUntil = Time.time + 2;
                    break;
                case SpecialKind.Hitler:
                    if (Hit(target, round, 10, 3) > 0 && target.Alive)
                    {
                        target.StopAllCoroutines();
                        target.specialRunning = false;
                        target.stunnedUntil = Time.time + 0.8f;
                        target.UpdateInput();
                    }
                    break;
            }
            specialRunning = false;
            UpdateInput();
        }
    }
}

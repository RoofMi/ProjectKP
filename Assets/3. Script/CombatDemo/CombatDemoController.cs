using AI;
using Character;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CombatDemo
{
    public sealed class CombatDemoController : MonoBehaviour
    {
        [Header("Combatants")]
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField] private HealthComponent _enemyHealth;
        [SerializeField] private StaminaComponent _playerStamina;
        [SerializeField] private StaminaComponent _enemyStamina;

        [Header("Player Controls")]
        [SerializeField] private PlayerInputHandler _playerInput;
        [SerializeField] private CharacterMovement _playerMovement;
        [SerializeField] private ActionController _playerActions;
        [SerializeField] private CombatDemoLockOn _lockOn;

        [Header("Enemy Controls")]
        [SerializeField] private AIBrain _enemyBrain;
        [SerializeField] private AINavigation _enemyNavigation;
        [SerializeField] private CharacterMovement _enemyMovement;
        [SerializeField] private ActionController _enemyActions;

        [Header("HUD")]
        [SerializeField] private Image _playerHealthFill;
        [SerializeField] private Image _enemyHealthFill;
        [SerializeField] private Image _playerStaminaFill;
        [SerializeField] private Image _enemyStaminaFill;
        [SerializeField] private TextMeshProUGUI _playerHealthText;
        [SerializeField] private TextMeshProUGUI _enemyHealthText;
        [SerializeField] private TextMeshProUGUI _playerStaminaText;
        [SerializeField] private TextMeshProUGUI _enemyStaminaText;
        [SerializeField] private TextMeshProUGUI _lockStateText;
        [SerializeField] private TextMeshProUGUI _resultText;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursorOnStart = true;

        private bool _matchComplete;

        private void Start()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            _playerHealth.OnDeath += HandlePlayerDefeated;
            _enemyHealth.OnDeath += HandleEnemyDefeated;
            RefreshHud();

            if (_lockCursorOnStart)
            {
                SetCursorLocked(true);
            }
        }

        private void OnDisable()
        {
            SetCursorLocked(false);

            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= HandlePlayerDefeated;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.OnDeath -= HandleEnemyDefeated;
            }
        }

        private void Update()
        {
            RefreshHud();

            if (_matchComplete && Keyboard.current?.rKey.wasPressedThisFrame == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }
            else if (!_matchComplete &&
                     Mouse.current?.leftButton.wasPressedThisFrame == true &&
                     Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _lockCursorOnStart && !_matchComplete)
            {
                SetCursorLocked(true);
            }
        }

        private bool HasRequiredReferences()
        {
            bool isValid =
                _playerHealth != null &&
                _enemyHealth != null &&
                _playerStamina != null &&
                _enemyStamina != null &&
                _playerInput != null &&
                _playerMovement != null &&
                _playerActions != null &&
                _lockOn != null &&
                _enemyBrain != null &&
                _enemyNavigation != null &&
                _enemyMovement != null &&
                _enemyActions != null &&
                _playerHealthFill != null &&
                _enemyHealthFill != null &&
                _playerStaminaFill != null &&
                _enemyStaminaFill != null &&
                _playerHealthText != null &&
                _enemyHealthText != null &&
                _playerStaminaText != null &&
                _enemyStaminaText != null &&
                _lockStateText != null &&
                _resultText != null;

            if (!isValid)
            {
                Debug.LogError("[CombatDemo] Inspector references are incomplete.", this);
            }

            return isValid;
        }

        private void RefreshHud()
        {
            if (_playerHealthFill == null)
            {
                return;
            }

            UpdateBar(
                _playerHealthFill,
                _playerHealthText,
                _playerHealth.CurrentHealth,
                _playerHealth.MaxHealth,
                false);
            UpdateBar(
                _enemyHealthFill,
                _enemyHealthText,
                _enemyHealth.CurrentHealth,
                _enemyHealth.MaxHealth,
                true);
            UpdateBar(
                _playerStaminaFill,
                _playerStaminaText,
                _playerStamina.CurrentStamina,
                _playerStamina.MaxStamina,
                false);
            UpdateBar(
                _enemyStaminaFill,
                _enemyStaminaText,
                _enemyStamina.CurrentStamina,
                _enemyStamina.MaxStamina,
                true);

            if (!_matchComplete)
            {
                _lockStateText.text = _lockOn.IsLockedOn
                    ? $"LOCK: {_lockOn.TargetName.ToUpperInvariant()}"
                    : "LOCK: OFF";
            }
        }

        private void HandlePlayerDefeated()
        {
            CompleteMatch("AI VICTORY");
        }

        private void HandleEnemyDefeated()
        {
            CompleteMatch("PLAYER VICTORY");
        }

        private void CompleteMatch(string result)
        {
            if (_matchComplete)
            {
                return;
            }

            _matchComplete = true;
            SetCursorLocked(false);
            DisableCombat();
            _resultText.text = $"{result}\n<size=20>PRESS R TO RESTART</size>";
            _lockStateText.text = "MATCH COMPLETE";
        }

        private void DisableCombat()
        {
            _playerInput.enabled = false;
            _playerMovement.enabled = false;
            _playerActions.enabled = false;
            _lockOn.enabled = false;

            _enemyBrain.enabled = false;
            _enemyMovement.enabled = false;
            _enemyActions.enabled = false;
            _enemyNavigation.Suspend();
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private static void UpdateBar(
            Image fill,
            TextMeshProUGUI text,
            float current,
            float maximum,
            bool fromRight)
        {
            float ratio = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
            RectTransform rect = fill.rectTransform;
            rect.anchorMin = fromRight ? new Vector2(1f - ratio, 0f) : Vector2.zero;
            rect.anchorMax = fromRight ? Vector2.one : new Vector2(ratio, 1f);
            text.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}";
        }
    }
}

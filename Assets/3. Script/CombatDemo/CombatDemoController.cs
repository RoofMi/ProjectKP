using AI;
using Character;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CombatDemo
{
    public sealed class CombatDemoController : MonoBehaviour
    {
        [Header("Combatants")]
        [SerializeField] private GameObject _player;
        [SerializeField] private GameObject _enemy;
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField] private HealthComponent _enemyHealth;
        [SerializeField] private StaminaComponent _playerStamina;
        [SerializeField] private StaminaComponent _enemyStamina;

        [Header("Presentation")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CombatDemoLockOn _lockOn;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursorOnStart = true;

        private Image _playerHealthFill;
        private Image _enemyHealthFill;
        private Image _playerStaminaFill;
        private Image _enemyStaminaFill;
        private TextMeshProUGUI _playerHealthText;
        private TextMeshProUGUI _enemyHealthText;
        private TextMeshProUGUI _playerStaminaText;
        private TextMeshProUGUI _enemyStaminaText;
        private TextMeshProUGUI _lockStateText;
        private TextMeshProUGUI _resultText;
        private bool _matchComplete;

        private void Start()
        {
            ResolveReferences();
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            BuildHud();
            _playerHealth.OnDeath += HandlePlayerDefeated;
            _enemyHealth.OnDeath += HandleEnemyDefeated;

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
            else if (!_matchComplete && Mouse.current?.leftButton.wasPressedThisFrame == true &&
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

        private void ResolveReferences()
        {
            if (_player == null)
            {
                _player = Object.FindFirstObjectByType<PlayerInputHandler>()?.gameObject;
            }

            if (_enemy == null)
            {
                _enemy = GameObject.FindGameObjectWithTag("Enemy");
                _enemy ??= Object.FindFirstObjectByType<AIBrain>()?.gameObject;
            }

            if (_player != null)
            {
                _playerHealth ??= _player.GetComponentInChildren<HealthComponent>();
                _playerStamina ??= _player.GetComponentInChildren<StaminaComponent>();
                _lockOn ??= _player.GetComponent<CombatDemoLockOn>();
            }

            if (_enemy != null)
            {
                _enemyHealth ??= _enemy.GetComponentInChildren<HealthComponent>();
                _enemyStamina ??= _enemy.GetComponentInChildren<StaminaComponent>();
            }

            _canvas ??= Object.FindFirstObjectByType<Canvas>();
            if (_canvas == null || _canvas.transform.localScale.sqrMagnitude < 0.0001f)
            {
                _canvas = CreateHudCanvas();
            }
        }

        private bool HasRequiredReferences()
        {
            bool isValid = _player != null &&
                           _enemy != null &&
                           _playerHealth != null &&
                           _enemyHealth != null &&
                           _playerStamina != null &&
                           _enemyStamina != null &&
                           _canvas != null;

            if (!isValid)
            {
                Debug.LogError($"[CombatDemo] Missing references - player: {_player != null}, enemy: {_enemy != null}, " +
                               $"player health: {_playerHealth != null}, enemy health: {_enemyHealth != null}, " +
                               $"player stamina: {_playerStamina != null}, enemy stamina: {_enemyStamina != null}, " +
                               $"canvas: {_canvas != null}.");
            }

            return isValid;
        }

        private void BuildHud()
        {
            Transform existingHud = _canvas.transform.Find("CombatDemoHUD");
            if (existingHud != null)
            {
                Destroy(existingHud.gameObject);
            }

            var hudRoot = CreateUiObject("CombatDemoHUD", _canvas.transform);
            Stretch(hudRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CreateText(hudRoot, "Title", "COMBAT DEMO", 30f, TextAlignmentOptions.Center,
                new Color(0.93f, 0.96f, 1f), new Vector2(0.35f, 0.94f), new Vector2(0.65f, 0.99f));
            CreateText(hudRoot, "Controls", "TAB  LOCK ON    ·    R  RESTART AFTER KO", 16f,
                TextAlignmentOptions.Center, new Color(0.75f, 0.8f, 0.9f), new Vector2(0.25f, 0.905f), new Vector2(0.75f, 0.935f));

            var playerBar = CreateBar(hudRoot, "PLAYER", false, new Color(0.18f, 0.76f, 1f),
                new Vector2(0.035f, 0.835f), new Vector2(0.34f, 0.875f));
            _playerHealthFill = playerBar.Fill;
            _playerHealthText = playerBar.Value;

            var enemyBar = CreateBar(hudRoot, "AI DUMMY", true, new Color(1f, 0.3f, 0.35f),
                new Vector2(0.66f, 0.835f), new Vector2(0.965f, 0.875f));
            _enemyHealthFill = enemyBar.Fill;
            _enemyHealthText = enemyBar.Value;

            var playerStamina = CreateBar(hudRoot, "STAMINA", false, new Color(0.95f, 0.78f, 0.22f),
                new Vector2(0.035f, 0.792f), new Vector2(0.27f, 0.812f), 13f);
            _playerStaminaFill = playerStamina.Fill;
            _playerStaminaText = playerStamina.Value;

            var enemyStamina = CreateBar(hudRoot, "STAMINA", true, new Color(0.95f, 0.78f, 0.22f),
                new Vector2(0.73f, 0.792f), new Vector2(0.965f, 0.812f), 13f);
            _enemyStaminaFill = enemyStamina.Fill;
            _enemyStaminaText = enemyStamina.Value;

            _lockStateText = CreateText(hudRoot, "LockState", "LOCK: OFF", 18f, TextAlignmentOptions.Center,
                new Color(0.6f, 0.85f, 1f), new Vector2(0.35f, 0.855f), new Vector2(0.65f, 0.89f));
            _resultText = CreateText(hudRoot, "Result", string.Empty, 44f, TextAlignmentOptions.Center,
                Color.white, new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.57f));
        }

        private void RefreshHud()
        {
            if (_playerHealthFill == null)
            {
                return;
            }

            UpdateBar(_playerHealthFill, _playerHealthText, _playerHealth.CurrentHealth, _playerHealth.MaxHealth, false);
            UpdateBar(_enemyHealthFill, _enemyHealthText, _enemyHealth.CurrentHealth, _enemyHealth.MaxHealth, true);
            UpdateBar(_playerStaminaFill, _playerStaminaText, _playerStamina.CurrentStamina, _playerStamina.MaxStamina, false);
            UpdateBar(_enemyStaminaFill, _enemyStaminaText, _enemyStamina.CurrentStamina, _enemyStamina.MaxStamina, true);

            if (!_matchComplete && _lockStateText != null)
            {
                _lockStateText.text = _lockOn != null && _lockOn.IsLockedOn
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
            SetCombatantEnabled(_player, false);
            SetCombatantEnabled(_enemy, false);
            _resultText.text = $"{result}\n<size=20>PRESS R TO RESTART</size>";
            _lockStateText.text = "MATCH COMPLETE";
        }

        private static void SetCombatantEnabled(GameObject combatant, bool enabled)
        {
            if (combatant == null)
            {
                return;
            }

            SetComponentEnabled<PlayerInputHandler>(combatant, enabled);
            SetComponentEnabled<CharacterMovement>(combatant, enabled);
            SetComponentEnabled<ActionController>(combatant, enabled);
            SetComponentEnabled<AIBrain>(combatant, enabled);
            SetComponentEnabled<CombatDemoLockOn>(combatant, enabled);

            var agent = combatant.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = !enabled;
                agent.enabled = enabled;
            }
        }

        private static void SetComponentEnabled<T>(GameObject owner, bool enabled) where T : Behaviour
        {
            var component = owner.GetComponent<T>();
            if (component != null)
            {
                component.enabled = enabled;
            }
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private static HudBar CreateBar(Transform parent, string title, bool fromRight, Color fillColor,
            Vector2 anchorMin, Vector2 anchorMax, float fontSize = 16f)
        {
            var root = CreateUiObject($"{title} Bar", parent);
            Stretch(root, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

            var background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.06f, 0.1f, 0.82f);

            var fill = CreateUiObject("Fill", root).gameObject.AddComponent<Image>();
            fill.color = fillColor;

            var fillRect = fill.rectTransform;
            fillRect.anchorMin = fromRight ? new Vector2(0f, 0f) : Vector2.zero;
            fillRect.anchorMax = fromRight ? Vector2.one : new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            var label = CreateText(root, "Label", title, fontSize, fromRight ? TextAlignmentOptions.Right : TextAlignmentOptions.Left,
                new Color(0.82f, 0.88f, 0.96f), new Vector2(0f, 1f), new Vector2(1f, 1f));
            label.rectTransform.anchoredPosition = new Vector2(0f, 18f);
            label.rectTransform.sizeDelta = new Vector2(0f, 20f);

            var value = CreateText(root, "Value", string.Empty, fontSize, TextAlignmentOptions.Center,
                Color.white, new Vector2(0f, 0f), new Vector2(1f, 1f));

            return new HudBar(fill, value);
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string value, float fontSize,
            TextAlignmentOptions alignment, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var text = CreateUiObject(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            Stretch(text.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            return text;
        }

        private static RectTransform CreateUiObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private static Canvas CreateHudCanvas()
        {
            var canvasObject = new GameObject("CombatDemoCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void UpdateBar(Image fill, TextMeshProUGUI text, float current, float maximum, bool fromRight)
        {
            float ratio = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
            var rect = fill.rectTransform;
            rect.anchorMin = fromRight ? new Vector2(1f - ratio, 0f) : Vector2.zero;
            rect.anchorMax = fromRight ? Vector2.one : new Vector2(ratio, 1f);
            text.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}";
        }

        private readonly struct HudBar
        {
            public readonly Image Fill;
            public readonly TextMeshProUGUI Value;

            public HudBar(Image fill, TextMeshProUGUI value)
            {
                Fill = fill;
                Value = value;
            }
        }
    }
}

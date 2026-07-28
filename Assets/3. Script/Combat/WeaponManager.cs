using UnityEngine;
using Combat.Weapons;

namespace Combat
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponBase _weaponToTest;
        [SerializeField] private Transform _weaponMount;
        [SerializeField] private LayerMask _targetLayers;

        private GameObject _currentWeaponInstance;
        private ComboManager _comboManager;
        private Hitbox[] _hitboxes;
        private WeaponBase _equippedWeapon;
        private bool _hasAttackData;

        private void Start()
        {
            _comboManager = GetComponent<ComboManager>();

            if (_weaponMount == null)
            {
                Debug.LogError("[WeaponManager] Weapon mount is not assigned.", this);
            }

            if (_weaponToTest != null)
            {
                EquipWeapon(_weaponToTest);
            }
        }

        public void EquipWeapon(WeaponBase weapon)
        {
            if (weapon == null)
            {
                Debug.LogError("[WeaponManager] Cannot equip a null weapon.", this);
                return;
            }

            ResetAttackState();

            if (_currentWeaponInstance != null)
            {
                Destroy(_currentWeaponInstance);
            }

            _equippedWeapon = weapon;

            if (weapon.WeaponPrefab == null || _weaponMount == null)
            {
                Debug.LogError(
                    $"[WeaponManager] Weapon prefab or mount is missing on {name}.",
                    this);
                return;
            }

            _currentWeaponInstance = Instantiate(weapon.WeaponPrefab, _weaponMount);
            _currentWeaponInstance.transform.localPosition = new Vector3(0.1f, -0.03f, 0.01f);
            _currentWeaponInstance.transform.localRotation = Quaternion.Euler(40f, -80f, -80f);
            SetLayerRecursively(_currentWeaponInstance, gameObject.layer);

            _hitboxes = _currentWeaponInstance.GetComponentsInChildren<Hitbox>(true);
            foreach (Hitbox hitbox in _hitboxes)
            {
                hitbox.ClearAttackData();
                hitbox.SetTargetLayers(_targetLayers);
                hitbox.DisableHitbox();
            }

            if (_comboManager != null && weapon.Combos != null && weapon.Combos.Length > 0)
            {
                _comboManager.SetWeaponCombos(weapon.Combos);
            }
            else
            {
                Debug.LogWarning($"[WeaponManager] No combos are configured on {weapon.name}.", this);
            }
        }

        public void EnableHitboxes()
        {
            if (!_hasAttackData || _hitboxes == null)
            {
                return;
            }

            foreach (Hitbox hitbox in _hitboxes)
            {
                hitbox.EnableHitbox();
            }
        }

        public void DisableHitboxes()
        {
            if (_hitboxes == null)
            {
                return;
            }

            foreach (Hitbox hitbox in _hitboxes)
            {
                hitbox.DisableHitbox();
            }
        }

        public bool BeginAttack(RuntimeComboNode comboNode)
        {
            ResetAttackState();

            if (comboNode == null ||
                _equippedWeapon == null ||
                _hitboxes == null ||
                _hitboxes.Length == 0)
            {
                Debug.LogWarning("[WeaponManager] Cannot configure attack data.", this);
                return false;
            }

            float damage = Mathf.Max(0f, _equippedWeapon.BaseDamage) * comboNode.DamageMultiplier;
            foreach (Hitbox hitbox in _hitboxes)
            {
                hitbox.ConfigureAttack(gameObject, damage, comboNode.HitReaction);
            }

            _hasAttackData = true;
            return true;
        }

        public void ResetAttackState()
        {
            DisableHitboxes();
            _hasAttackData = false;

            if (_hitboxes == null)
            {
                return;
            }

            foreach (Hitbox hitbox in _hitboxes)
            {
                hitbox.ClearAttackData();
            }
        }

        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}

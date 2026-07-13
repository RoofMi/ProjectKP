using UnityEngine;
using Combat.Weapons;

namespace Combat
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private WeaponBase _weaponToTest;
        [SerializeField] private Transform _weaponMount;
        
        private GameObject _currentWeaponInstance;
        private ComboManager _comboManager;
        private Hitbox[] _hitboxes;
        private WeaponBase _equippedWeapon;
        private LayerMask _targetLayers;
        private bool _hasTargetLayerOverride;
        private bool _hasAttackData;
        
        void Start()
        {
            _comboManager = GetComponent<ComboManager>();
            
            if (_weaponMount == null)
            {
                Debug.LogError("weaponMount not assigned.");
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
                Debug.LogError("[WeaponManager] Cannot equip null weapon!");
                return;
            }
            
            ResetAttackState();

            if (_currentWeaponInstance != null)
            {
                Destroy(_currentWeaponInstance);
            }

            _equippedWeapon = weapon;
            
            // SO에서 인스턴스 생성
            if (weapon.WeaponPrefab != null && _weaponMount != null)
            {
                _currentWeaponInstance = Instantiate(weapon.WeaponPrefab, _weaponMount);
                _currentWeaponInstance.transform.localPosition = new Vector3(0.1f, -0.03f, 0.01f);
                _currentWeaponInstance.transform.localRotation = Quaternion.Euler(40f, -80f, -80f);
                SetLayerRecursively(_currentWeaponInstance, gameObject.layer);
                
                
                _hitboxes = _currentWeaponInstance.GetComponentsInChildren<Hitbox>(true);
                
                // 히트박스 초기 설정
                foreach (var hitbox in _hitboxes)
                {
                    hitbox.ClearAttackData();
                    if (_hasTargetLayerOverride)
                    {
                        hitbox.SetTargetLayers(_targetLayers);
                    }

                    hitbox.DisableHitbox();
                }
            }
            else
            {
                Debug.LogError($"[WeaponManager] Failed - Prefab: {weapon.WeaponPrefab}, Mount: {_weaponMount}");
            }
            
            // SO에서 콤보 로드
            if (_comboManager != null && weapon.Combos != null && weapon.Combos.Length > 0)
            {
                _comboManager.SetWeaponCombos(weapon.Combos);
            }
            else
            {
                Debug.LogWarning($"No combos to load - ComboManager: {_comboManager != null}, Combos: {weapon.Combos?.Length ?? 0}");
            }
        }

        public void EnableHitboxes()
        {
            if (!_hasAttackData || _hitboxes == null)
            {
                return;
            }

            foreach (var hitbox in _hitboxes)
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

            foreach (var hitbox in _hitboxes)
            {
                hitbox.DisableHitbox();
            }
        }

        public bool BeginAttack(RuntimeComboNode comboNode)
        {
            ResetAttackState();

            if (comboNode == null || _equippedWeapon == null || _hitboxes == null || _hitboxes.Length == 0)
            {
                Debug.LogWarning("[WeaponManager] Cannot configure attack data.");
                return false;
            }

            float damage = Mathf.Max(0f, _equippedWeapon.BaseDamage) * comboNode.DamageMultiplier;
            foreach (var hitbox in _hitboxes)
            {
                hitbox.ConfigureAttack(
                    gameObject,
                    damage,
                    comboNode.ApplyKnockback,
                    comboNode.KnockbackHorizontalForce,
                    comboNode.KnockbackVerticalForce);
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

            foreach (var hitbox in _hitboxes)
            {
                hitbox.ClearAttackData();
            }
        }

        public void SetTargetLayers(LayerMask targetLayers)
        {
            _targetLayers = targetLayers;
            _hasTargetLayerOverride = true;

            if (_hitboxes == null)
            {
                return;
            }

            foreach (var hitbox in _hitboxes)
            {
                hitbox.SetTargetLayers(_targetLayers);
            }
        }

        public bool TargetsLayer(int layer)
        {
            return layer >= 0 && _hasTargetLayerOverride && (_targetLayers.value & (1 << layer)) != 0;
        }

        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        // 테스트용 Context Menu
        [ContextMenu("Test Equip Weapon")]
        void TestEquip()
        {
            if (_weaponToTest != null)
                EquipWeapon(_weaponToTest);
        }
    }
}

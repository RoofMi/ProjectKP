using UnityEngine;
using System.Linq;
using Combat;
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
            
            // 기존 무기 제거
            if (_currentWeaponInstance != null)
            {
                Destroy(_currentWeaponInstance);
            }
            
            // SO에서 인스턴스 생성
            if (weapon.WeaponPrefab != null && _weaponMount != null)
            {
                _currentWeaponInstance = Instantiate(weapon.WeaponPrefab, _weaponMount);
                _currentWeaponInstance.transform.localPosition = new Vector3(0.1f, -0.03f, 0.01f);
                _currentWeaponInstance.transform.localRotation = Quaternion.Euler(40f, -80f, -80f);
                
                
                _hitboxes = _currentWeaponInstance.GetComponentsInChildren<Hitbox>(true);
                
                // 히트박스 초기 설정
                foreach (var hitbox in _hitboxes)
                {
                    hitbox.Attacker = gameObject;
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
            foreach (var hitbox in _hitboxes)
            {
                hitbox.EnableHitbox();
            }
        }
        
        public void DisableHitboxes()
        {
            foreach (var hitbox in _hitboxes)
            {
                hitbox.DisableHitbox();
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
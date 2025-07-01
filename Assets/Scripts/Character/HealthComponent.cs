using System;
using UnityEngine;
using Combat;
using Combat.Interfaces;

namespace Character
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float> OnDamageTaken; // damage 양
        public event Action OnDeath;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0;
        
        public Transform Transform => transform;
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void Heal(float amount)
        {
            if (IsDead) return;
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            maxHealth = newMaxHealth;
            
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died!");
        }
        
        public void Revive(float healthPercentage = 1f)
        {
            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        // IDamageable 구현
        public void TakeDamage(HitInfo hitInfo)
        {
            if (IsDead) return;
            
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - hitInfo.damage);
            
            OnDamageTaken?.Invoke(hitInfo.damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0 && previousHealth > 0)
            {
                Die();
            }
            
            // 추가적인 히트 정보 활용 (히트 위치, 공격자 등)
            // 추후 넉백, 히트 이펙트 등에 사용
        }
    }
}
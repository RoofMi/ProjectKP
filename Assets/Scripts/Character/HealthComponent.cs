using System;
using UnityEngine;

namespace Character
{
    public class HealthComponent : MonoBehaviour
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
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0 && previousHealth > 0)
            {
                Die();
            }
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
    }
}
using System;
using Combat;
using Combat.Interfaces;
using UnityEngine;

namespace Character
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        public event Action OnDeath;
        public event Action<HitInfo> OnHitReceived;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0f;
        public Transform Transform => transform;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Heal(float amount)
        {
            if (IsDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
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
        }

        public void TakeDamage(float damage)
        {
            ApplyDamage(damage);
        }

        public void ReceiveHit(HitInfo hitInfo)
        {
            if (IsDead)
            {
                return;
            }

            ApplyDamage(hitInfo.damage);

            if (!IsDead)
            {
                OnHitReceived?.Invoke(hitInfo);
            }
        }

        public void Revive(float healthPercentage = 1f)
        {
            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
        }

        private void ApplyDamage(float damage)
        {
            if (IsDead)
            {
                return;
            }

            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, damage));
            float damageTaken = previousHealth - currentHealth;

            Debug.Log($"[HealthComponent] {gameObject.name} took {damageTaken} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0f && previousHealth > 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died!");
        }
    }
}

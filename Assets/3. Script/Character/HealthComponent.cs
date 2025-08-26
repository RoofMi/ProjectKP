using System;
using UnityEngine;
using Combat.Interfaces;
using System.Collections;

namespace Character
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        public event Action OnDeath;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public bool IsDead => currentHealth <= 0;
        
        public Transform Transform => transform;


        [Header("Knockback Settings")]
        [SerializeField] private float gravity = 25f;
        private CharacterController characterController;
        private Coroutine knockbackCoroutine;

        private void Awake()
        {
            currentHealth = maxHealth;

            characterController = GetComponent<CharacterController>();
        }
        
        public void Heal(float amount)
        {
            if (IsDead) return;
            
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
        
        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died!");
        }
        
        public void Revive(float healthPercentage = 1f)
        {
            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
        }
        
        // IDamageable 구현
        public void TakeDamage(float damage)
        {
            if (IsDead) 
                return;
            
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            Debug.Log($"[HealthComponent] {gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
            
            if (currentHealth <= 0 && previousHealth > 0)
            {
                Die();
            }
            
            // 추가적인 히트 정보 활용 (히트 위치, 공격자 등)
            // 추후 넉백, 히트 이펙트 등에 사용
        }

        public void TakeKnockback(Vector3 direction, float horizontalForce, float verticalForce)
        {
            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
            }
            knockbackCoroutine = StartCoroutine(KnockbackCoroutine(direction, horizontalForce, verticalForce));
        }

        private IEnumerator KnockbackCoroutine(Vector3 direction, float horizontalForce, float verticalForce)
        {
            direction.y = 0;

            Vector3 moveVector = direction.normalized * horizontalForce;
            moveVector.y = verticalForce;

            // 처음 한 프레임은 isGrounded가 true일 수 있으므로, 루프 시작 전에 잠시 대기
            yield return new WaitForFixedUpdate();

            while (true)
            {
                // 중력 적용
                moveVector.y -= gravity * Time.deltaTime;

                // 캐릭터 이동
                characterController.Move(moveVector * Time.deltaTime);

                // 캐릭터가 땅에 닿았고, 아래로 떨어지는 중이라면 넉백 종료
                if (characterController.isGrounded && moveVector.y < 0)
                    break;

                yield return null; // 다음 프레임까지 대기
            }

            knockbackCoroutine = null;
        }
    }
}
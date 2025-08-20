using System;
using UnityEngine;

namespace Character
{
    public class StaminaComponent : MonoBehaviour
    {
        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina;
        [SerializeField] private float regenRate = 10f; // 초당
        [SerializeField] private float regenDelay = 1f; // 리젠 재개까지의 딜레이
        
        [Header("Debug")]
        [SerializeField] private bool isRegenerating = true;
        [SerializeField] private float timeSinceLastUse = 0f;
        
        public event Action<float, float> OnStaminaChanged; // current, max
        public event Action OnStaminaDepleted;
        public event Action OnStaminaRecovered; // 스태미너가 regen될 때
        
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float StaminaPercentage => currentStamina / maxStamina;
        public bool HasStamina => currentStamina > 0;
        
        private void Awake()
        {
            currentStamina = maxStamina;
        }
        
        private void Start()
        {
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        private void Update()
        {
            HandleRegeneration();
        }
        
        private void HandleRegeneration()
        {
            if (currentStamina >= maxStamina)
            {
                isRegenerating = false;
                enabled = false; // Update 중지
                return;
            }
            
            timeSinceLastUse += Time.deltaTime;
            
            if (timeSinceLastUse >= regenDelay)
            {
                if (!isRegenerating)
                {
                    isRegenerating = true;
                    OnStaminaRecovered?.Invoke();
                }
                
                float regenAmount = regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina + regenAmount);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }
        
        public bool TryUseStamina(float amount)
        {
            if (currentStamina < amount)
            {
                return false;
            }
            
            UseStamina(amount);
            return true;
        }
        
        public void UseStamina(float amount)
        {
            float previousStamina = currentStamina;
            currentStamina = Mathf.Max(0, currentStamina - amount);
            
            timeSinceLastUse = 0f;
            isRegenerating = false;
            enabled = true; // Update 재개
            
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            if (currentStamina <= 0 && previousStamina > 0)
            {
                OnStaminaDepleted?.Invoke();
            }
        }
        
        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        public void SetMaxStamina(float newMaxStamina, bool restoreToFull = false)
        {
            maxStamina = newMaxStamina;
            
            if (restoreToFull)
            {
                currentStamina = maxStamina;
                enabled = false; // 최대치면 Update 중지
            }
            else
            {
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                enabled = currentStamina < maxStamina; // 필요시에만 Update
            }
            
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        public void SetRegenRate(float newRate)
        {
            regenRate = Mathf.Max(0, newRate);
        }
        
        public void SetRegenDelay(float newDelay)
        {
            regenDelay = Mathf.Max(0, newDelay);
        }
        
        public bool CanAfford(float staminaCost)
        {
            return currentStamina >= staminaCost;
        }
    }
}
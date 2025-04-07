using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class HealthUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image healthFill;   // 새로 사용될 이미지

        [SerializeField] private Button healthButton;
        [SerializeField] private Button damageButton;

        [Header("Linked Health Component")]
        [SerializeField] private Health health;

        private void Start()
        {
            // 버튼에 콜백 연결
            if (healthButton != null)
                healthButton.onClick.AddListener(() => Heal(10));
            
            if (damageButton != null)
                damageButton.onClick.AddListener(() => TakeDamage(10));
        }

        private void Update()
        {
            // Health의 값을 이용해 fillAmount 갱신
            if (healthFill != null && health != null)
            {
                float normalized = health.Current / health.MaxHealth;
                // 혹시 0 이하/1 이상이 될 수 있으니 Clamp 처리
                normalized = Mathf.Clamp01(normalized);
                healthFill.fillAmount = normalized;
            }
        }

        private void Heal(float value)
        {
            if (health != null)
            {
                health.Heal(value);
            }
        }

        private void TakeDamage(float damage)
        {
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
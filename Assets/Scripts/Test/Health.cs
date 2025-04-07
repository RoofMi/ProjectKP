using UnityEngine;

namespace Test
{
    public class Health : MonoBehaviour
    {
        public float MaxHealth = 100;
        public float Current;
        public float NormalizedHealth => Current / MaxHealth;

        void Start() 
        {
            Current = MaxHealth;
        }
        
        public void Heal(float value) 
        {
            Current += value;
        }

        public void TakeDamage(float damage) 
        {
            Current -= damage;
        }
    }
}

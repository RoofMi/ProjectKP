using UnityEngine;

namespace Combat.Weapons
{
    public enum WeaponType
    {
        Sword
        // , More weapons...
    }
    
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Combat/Weapon")]
    public class WeaponBase : ScriptableObject
    {
        public GameObject WeaponPrefab;
        public WeaponType WeaponType { get; private set; }
        
        public float BaseDamage = 10f;

        public ComboDefinition[] Combos;

        // More data...

        // 추후에 무기마다 독립적인 animator를 사용할 수도 있음, 그렇게 되면 animator 참조도 추가
    }
}

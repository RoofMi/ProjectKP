using UnityEngine;

namespace Combat.Weapons
{
    public enum WeaponType
    {
        Sword
        // , More weapons...
    }
    
    public abstract class WeaponBase : ScriptableObject
    {
        [field: SerializeField] public Mesh WeaponMesh { get; private set; }
        [field: SerializeField] public WeaponType WeaponType { get; private set; }
        // More data...
        
        // 추후에 무기마다 독립적인 animator를 사용할 수도 있음, 그렇게 되면 animator 참조도 추가
    }
}

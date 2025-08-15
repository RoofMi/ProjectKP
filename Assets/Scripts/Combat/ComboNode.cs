using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "ComboNode", menuName = "Scriptable Objects/ComboNode")]
    public class ComboNode : ScriptableObject
    {
        public AnimationClip AnimClip;
        // 추후 VFX, Sound 등 확장 가능
    }
}

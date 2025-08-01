using UnityEngine;

namespace Character
{
    public struct InputData
    {
        public string Key { get; }
        public InputType Type { get; }
        public float Timestamp { get; }
        public Vector2 Direction { get; }
        
        public InputData(string key, InputType type, float timestamp = 0, Vector2 direction = default)
        {
            Key = key;
            Type = type;
            Timestamp = timestamp > 0 ? timestamp : Time.time;
            Direction = direction;
        }
        
        // 이동 입력용 생성자
        public InputData(Vector2 direction) : this(string.Empty, InputType.Movement, Time.time, direction)
        {
        }
        
        // 공격/스킬 입력용 생성자
        public InputData(string key, InputType type) : this(key, type, Time.time, Vector2.zero)
        {
        }
    }
    
    public enum InputType
    {
        Movement,
        Attack,
        Skill,
        Jump,
        Dash
    }
}
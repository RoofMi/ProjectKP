using System.Collections.Generic;
using UnityEngine;

namespace AI.Debugging
{
    /// <summary>
    /// AI 의사결정 과정을 화면에 표시하는 디버그 컴포넌트
    /// F1 키로 표시 토글 가능
    /// </summary>
    [RequireComponent(typeof(AIBrain))]
    public class AIDebugDisplay : MonoBehaviour
    {
        private AIBrain _brain;
        private bool _showDebug = true;
        
        // GUI 스타일
        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _currentActionStyle;
        private GUIStyle _boxStyle;
        
        // 색상
        private readonly Color _highUtilityColor = Color.yellow;
        private readonly Color _mediumUtilityColor = Color.white;
        private readonly Color _lowUtilityColor = Color.gray;
        private readonly Color _currentActionColor = Color.green;
        
        private void Awake()
        {
            _brain = GetComponent<AIBrain>();
            InitializeStyles();
        }
        
        private void InitializeStyles()
        {
            // 스타일은 OnGUI에서 초기화
        }
        
        private void Update()
        {
            // F1 키로 디버그 표시 토글
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _showDebug = !_showDebug;
            }
        }
        
        private void OnGUI()
        {
            if (!_showDebug || _brain == null) return;
            
            // 스타일 초기화 (OnGUI 컨텍스트에서만 가능)
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
                
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = Color.white }
                };
                
                _valueStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = Color.cyan }
                };
                
                _currentActionStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = _currentActionColor }
                };
                
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f)) }
                };
            }
            
            // 메인 컨테이너
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // 왼쪽 패널 - Actions & Utility
            DrawActionsPanel(10, 10, 350, 300);
            
            // 오른쪽 패널 - Context Values
            DrawContextPanel(370, 10, 300, 300);
            
            // 하단 패널 - Decision History
            DrawHistoryPanel(10, 320, 660, 150);
            
            // 타이틀 및 토글 안내
            GUI.Label(new Rect(screenWidth - 200, 10, 190, 30), 
                "AI Debug (F1 to toggle)", _headerStyle);
        }
        
        private void DrawActionsPanel(float x, float y, float width, float height)
        {
            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);
            
            GUI.Label(new Rect(x + 10, y + 5, width - 20, 25), 
                "Actions & Utility", _headerStyle);
            
            var actions = _brain.GetAllActionsWithUtility();
            float yOffset = 35;
            
            foreach (var action in actions)
            {
                // 액션 이름
                GUIStyle nameStyle = action.isCurrentAction ? _currentActionStyle : _labelStyle;
                string prefix = action.isCurrentAction ? "> " : "  ";
                GUI.Label(new Rect(x + 10, y + yOffset, 200, 20), 
                    prefix + action.actionName, nameStyle);
                
                // Utility 막대 그래프
                DrawUtilityBar(x + 160, y + yOffset, 120, 18, action.utility);
                
                // Utility 값
                Color utilityColor = GetUtilityColor(action.utility);
                GUI.color = utilityColor;
                GUI.Label(new Rect(x + 285, y + yOffset, 60, 20), 
                    action.utility.ToString("F3"), _valueStyle);
                GUI.color = Color.white;
                
                yOffset += 22;
            }
            
            // 현재 액션 표시
            yOffset += 10;
            GUI.Label(new Rect(x + 10, y + yOffset, width - 20, 20),
                $"Current: {_brain.GetCurrentActionName()}", _currentActionStyle);
        }
        
        private void DrawContextPanel(float x, float y, float width, float height)
        {
            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);
            
            GUI.Label(new Rect(x + 10, y + 5, width - 20, 25), 
                "Context Values", _headerStyle);
            
            var context = _brain.GetContextDebugInfo();
            float yOffset = 35;
            
            foreach (var kvp in context)
            {
                // 키
                GUI.Label(new Rect(x + 10, y + yOffset, 150, 20), 
                    kvp.Key + ":", _labelStyle);
                
                // 값
                string valueStr = FormatContextValue(kvp.Value);
                Color valueColor = GetContextValueColor(kvp.Key, kvp.Value);
                GUI.color = valueColor;
                GUI.Label(new Rect(x + 160, y + yOffset, 130, 20), 
                    valueStr, _valueStyle);
                GUI.color = Color.white;
                
                yOffset += 20;
                
                if (yOffset > height - 30) break; // 오버플로우 방지
            }
        }
        
        private void DrawHistoryPanel(float x, float y, float width, float height)
        {
            GUI.Box(new Rect(x, y, width, height), "", _boxStyle);
            
            GUI.Label(new Rect(x + 10, y + 5, width - 20, 25), 
                "Decision History", _headerStyle);
            
            var history = _brain.GetDecisionHistory();
            float yOffset = 35;
            
            // 최신 순으로 표시 (역순)
            for (int i = history.Count - 1; i >= 0 && yOffset < height - 20; i--)
            {
                var record = history[i];
                string timeStr = $"[{record.timestamp:F2}]";
                string actionStr = $"{record.actionName} (Utility: {record.utility:F3})";
                
                GUI.Label(new Rect(x + 10, y + yOffset, 80, 20), 
                    timeStr, _labelStyle);
                GUI.Label(new Rect(x + 95, y + yOffset, width - 105, 20), 
                    actionStr, _labelStyle);
                
                yOffset += 20;
            }
            
            if (history.Count == 0)
            {
                GUI.Label(new Rect(x + 10, y + yOffset, width - 20, 20), 
                    "No decisions yet...", _labelStyle);
            }
        }
        
        private void DrawUtilityBar(float x, float y, float width, float height, float utility)
        {
            // 배경
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            
            // Utility 막대
            Color barColor = GetUtilityColor(utility);
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(x, y, width * utility, height), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
        
        private Color GetUtilityColor(float utility)
        {
            if (utility > 0.6f) return _highUtilityColor;
            if (utility > 0.3f) return _mediumUtilityColor;
            return _lowUtilityColor;
        }
        
        private Color GetContextValueColor(string key, object value)
        {
            // bool 값
            if (value is bool boolValue)
            {
                return boolValue ? Color.green : Color.red;
            }
            
            // Health/Stamina
            if (key.Contains("Health") || key.Contains("Stamina"))
            {
                float floatValue = (float)value;
                if (floatValue > 0.7f) return Color.green;
                if (floatValue > 0.3f) return Color.yellow;
                return Color.red;
            }
            
            // Distance
            if (key.Contains("Distance"))
            {
                return Color.cyan;
            }
            
            return Color.white;
        }
        
        private string FormatContextValue(object value)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "True" : "False";
            }
            
            if (value is float floatValue)
            {
                // 거리는 미터 단위로 표시
                if (value.ToString().Contains("Distance") && !value.ToString().Contains("Normalized"))
                {
                    return $"{floatValue:F1}m";
                }
                // 퍼센트는 % 표시
                if (floatValue <= 1.0f)
                {
                    return $"{(floatValue * 100):F0}%";
                }
                return floatValue.ToString("F2");
            }
            
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            
            return value?.ToString() ?? "null";
        }
        
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
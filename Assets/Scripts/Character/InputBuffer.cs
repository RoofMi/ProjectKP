using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class InputBuffer
    {
        // 상수로 명시적 선언
        private const float BUFFER_TIME_SECONDS = 0.5f;
        
        // 명확한 타입 정의
        private struct BufferedInput
        {
            public string InputName { get; }
            public float Timestamp { get; }
            
            public BufferedInput(string inputName, float timestamp)
            {
                InputName = inputName;
                Timestamp = timestamp;
            }
        }

        private readonly Queue<BufferedInput> _inputQueue = new Queue<BufferedInput>();

        public void AddInput(string inputName)
        {
            _inputQueue.Enqueue(new BufferedInput(inputName, Time.time));
        }

        public void UpdateBuffer()
        {
            float currentTime = Time.time;
            
            // 만료된 입력 제거
            while (_inputQueue.Count > 0)
            {
                var oldestInput = _inputQueue.Peek();
                if (IsInputExpired(oldestInput, currentTime))
                {
                    _inputQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }
        
        private bool IsInputExpired(BufferedInput input, float currentTime)
        {
            return currentTime - input.Timestamp > BUFFER_TIME_SECONDS;
        }

        public string GetNextInput()
        {
            if (_inputQueue.Count > 0)
            {
                var bufferedInput = _inputQueue.Dequeue();
                return bufferedInput.InputName;
            }

            return null;
        }

        public void ClearAllInputs()
        {
            _inputQueue.Clear();
        }

        public bool HasInput() => _inputQueue.Count > 0;
    }
}

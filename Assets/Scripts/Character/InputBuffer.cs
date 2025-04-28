using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public class InputBuffer
    {
        private const float _bufferTime = 0.3f;

        private Queue<(string inputName, float timeStamp)> _inputQueue = new Queue<(string, float)>();

        public void AddInput(string inputName)
        {
            _inputQueue.Enqueue((inputName, Time.time));
        }

        public void UpdateBuffer()
        {
            float currentTime = Time.time;
            while (_inputQueue.Count > 0)
            {
                var (inputName, timeStamp) = _inputQueue.Peek();
                if (currentTime - timeStamp > _bufferTime)
                {
                    _inputQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        public string GetNextInput()
        {
            if (_inputQueue.Count > 0)
            {
                var (inputName, _) = _inputQueue.Dequeue();
                return inputName;
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

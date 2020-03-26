using System;
using UnityEngine;

namespace HephaestusForge.EditorButton
{
    [Serializable]
    public sealed class MethodParameter 
    {
        [SerializeField]
        private string _type;

        [SerializeField]
        private string _jsonData;

        public string Type { get => _type; }
        public string JsonData { get => _jsonData; }

        public MethodParameter(string type, string jsonData)
        {
            _type = type;
            _jsonData = jsonData;
        }
    }
}
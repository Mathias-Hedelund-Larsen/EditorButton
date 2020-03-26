using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HephaestusForge.EditorButton
{
    [Serializable]
    public sealed class MethodParameters 
    {
        [SerializeField]
        private int _objectID;

        [SerializeField]
        private string _sceneGuid;

        [SerializeField]
        private string _methodName;

        [SerializeField]
        private MethodParameter[] _parameters;

        public int ObjectID { get => _objectID; }
        public string SceneGuid { get => _sceneGuid; }
        public string MethodName { get => _methodName; }
        public MethodParameter[] Parameters { get => _parameters; }

        public MethodParameters(int objectID, string sceneGuid, string methodName, MethodParameter[] parameters)
        {
            _objectID = objectID;
            _sceneGuid = sceneGuid;
            _methodName = methodName;
            _parameters = parameters;
        }
    }
}
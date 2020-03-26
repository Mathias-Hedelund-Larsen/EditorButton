using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HephaestusForge.EditorButton
{
    public sealed class MethodParametersContainer : ScriptableObject
    {
        [SerializeField]
        private List<MethodParameters> _methodsParameters = new List<MethodParameters>();

        /// <summary>
        /// Creating an instance of this scriptable object if none exists, this is called in the creation menu in the editor.
        /// </summary>
        [MenuItem("Assets/Create/HephaestusForge/Limited to one/MethodParametersContainer", false, 0)]
        private static void CreateInstance()
        {
            if (AssetDatabase.FindAssets("t:MethodParametersContainer").Length == 0)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);

                if (path.Length > 0)
                {
                    var obj = CreateInstance<MethodParametersContainer>();

                    if (Directory.Exists(path))
                    {
                        AssetDatabase.CreateAsset(obj, path + "/MethodParametersContainer.asset");

                        return;
                    }

                    var pathSplit = path.Split('/').ToList();
                    pathSplit.RemoveAt(pathSplit.Count - 1);
                    path = string.Join("/", pathSplit);

                    if (Directory.Exists(path))
                    {
                        AssetDatabase.CreateAsset(obj, path + "/MethodParametersContainer.asset");

                        return;
                    }
                }
                else
                {
                    Debug.LogWarning("An instance of MethodParametersContainer already exists");
                }
            }
        }

        public MethodParameters GetParameters(int objectID, string sceneGuid, string methodName, int parameterCount)
        {
            return _methodsParameters.Find(m => m.ObjectID == objectID && m.SceneGuid == sceneGuid && m.MethodName == methodName && m.Parameters.Length == parameterCount);
        }

        public void AddMethodParameters(MethodParameters methodParameters)
        {
            _methodsParameters.Add(methodParameters);

            EditorUtility.SetDirty(this);
        }
    }
}
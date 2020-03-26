using HephaestusForge.EditorFieldOnly;
using HephaestusForge.EditorFieldOnly.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HephaestusForge.EditorButton
{
    [CustomEditor(typeof(UnityEngine.Object), true, isFallback = true)]
    public class EditorButtonEditor : Editor
    {
        private int _objectID;
        private string _sceneGuid;
        private List<MethodInfo> _buttonMethods;
        private MethodParametersContainer _container;

        protected virtual void OnEnable()
        {
            target.GetSceneGuidAndObjectID(out _sceneGuid, out _objectID);
            _buttonMethods = new List<MethodInfo>();

            _buttonMethods.AddRange(target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).
                Where(m => m.IsDefined(typeof(EditorButtonAttribute))));

            var containerGuids = AssetDatabase.FindAssets("t:MethodParametersContainer");

            if (containerGuids.Length == 1) 
            {
                _container = AssetDatabase.LoadAssetAtPath<MethodParametersContainer>(AssetDatabase.GUIDToAssetPath(containerGuids[0]));
            }

            if (!_container)
            {
                Debug.LogError("Create an instance of the MethodParametersContainer");
            }
        }

        protected void DrawButtons()
        {          
            for (int i = 0; i < _buttonMethods.Count; i++)
            {
                if (GUILayout.Button(_buttonMethods[i].Name))
                {
                    object[] parameters = null;                    

                    if(_buttonMethods[i].GetParameters().Length > 0)
                    {
                        var param = _container.GetParameters(_objectID, _sceneGuid, _buttonMethods[i].Name, _buttonMethods[i].GetParameters().Length);

                        if (param != null)
                        {
                            parameters = new object[param.Parameters.Length];

                            for (int t = 0; t < parameters.Length; t++)
                            {
                                Type parameterType = Type.GetType(param.Parameters[t].Type);

                                if (parameterType.IsSubclassOf(typeof(EditorField)))
                                {
                                    var editorField = JsonUtility.FromJson(param.Parameters[t].JsonData, parameterType);
                                    var fieldValue = editorField.GetType().BaseType.GetField("_fieldValue", BindingFlags.Instance | BindingFlags.NonPublic);
                                    parameters[t] = fieldValue.GetValue(editorField);
                                }
                                else if (parameterType == typeof(UnityEngine.Object) || parameterType.IsSubclassOf(typeof(UnityEngine.Object)))
                                {
                                    StringCollectionField stringCollectionField = JsonUtility.FromJson<StringCollectionField>(param.Parameters[t].JsonData);

                                    string sceneGuid = stringCollectionField.FieldValue[0];
                                    int objectID = int.Parse(stringCollectionField.FieldValue[1]);

                                    parameters[t] = UnityEditorObjectExtensions.GetObjectByInstanceID(objectID, sceneGuid);

                                }
                                else if (parameterType.GetElementType() != null && (parameterType.GetElementType() == typeof(UnityEngine.Object) ||
                                    parameterType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object))))
                                {
                                    StringCollectionField stringCollectionField = JsonUtility.FromJson<StringCollectionField>(param.Parameters[t].JsonData);

                                    List<int> objectIDs = new List<int>();
                                    List<string> sceneGuids = new List<string>();
                                    IList objects = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(parameterType.GetElementType()));

                                    for (int c = 0; c < stringCollectionField.FieldValue.Length; c++)
                                    {
                                        if (c % 2 == 0)
                                        {
                                            sceneGuids.Add(stringCollectionField.FieldValue[c]);
                                        }
                                        else
                                        {
                                            objectIDs.Add(int.Parse(stringCollectionField.FieldValue[c]));
                                        }
                                    }

                                    for (int o = 0; o < sceneGuids.Count; o++)
                                    {
                                        objects.Add(UnityEditorObjectExtensions.GetObjectByInstanceID(objectIDs[o], sceneGuids[o]));
                                    }

                                    var castMethod = typeof(Enumerable).GetMethod("Cast", BindingFlags.Public | BindingFlags.Static).
                                        MakeGenericMethod(parameterType.GetElementType());

                                    var toArrayMethod = typeof(Enumerable).GetMethod("ToArray", BindingFlags.Public | BindingFlags.Static).
                                        MakeGenericMethod(parameterType.GetElementType());

                                    parameters[t] = toArrayMethod.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { objects })});
                                }
                                else
                                {
                                    parameters[t] = JsonUtility.FromJson(param.Parameters[t].JsonData, parameterType);
                                }
                            }
                        }
                        else
                        {
                            MakeParametersWindow.ShowWindow(_container, _objectID, _sceneGuid, _buttonMethods[i].Name, _buttonMethods[i].GetParameters(), target);
                            return;
                        }
                    }

                    _buttonMethods[i].Invoke(target, parameters);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawButtons();
        }
    }
}
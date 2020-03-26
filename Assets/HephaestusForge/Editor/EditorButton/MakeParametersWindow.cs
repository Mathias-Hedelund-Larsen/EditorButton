using HephaestusForge.EditorButton;
using HephaestusForge.EditorFieldOnly;
using HephaestusForge.EditorFieldOnly.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HephaestusForge.EditorButton 
{
    public class MakeParametersWindow : EditorWindow
    {
        private int _objectID;
        private string _sceneGuid;
        private string _methodName;
        private FieldInfo[] _fieldValues;
        private object[] _parameterValues;
        private UnityEngine.Object _target;
        private ParameterInfo[] _parameterInfos;
        private MethodParametersContainer _container;

        public static void ShowWindow(MethodParametersContainer container, int objectID, string sceneGuid, string name, ParameterInfo[] parameterInfo, UnityEngine.Object target)
        {
            var window = GetWindow<MakeParametersWindow>();

            window._container = container;
            window._objectID = objectID;
            window._sceneGuid = sceneGuid;
            window._methodName = name;
            window._parameterInfos = parameterInfo;
            window._parameterValues = new object[parameterInfo.Length];
            window._fieldValues = new FieldInfo[parameterInfo.Length];
            window._target = target;
        }

        private void OnGUI()
        {
            for (int i = 0; i < _parameterInfos.Length; i++)
            {
                if (_parameterInfos[i].ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    if(_fieldValues[i] == null)
                    {
                        _fieldValues[i] = typeof(StringCollectionField).BaseType.GetField("_fieldValue", BindingFlags.Instance | BindingFlags.NonPublic);
                    }

                    _parameterValues[i] = EditorGUILayout.ObjectField(new GUIContent(_parameterInfos[i].Name), (UnityEngine.Object)_parameterValues[i],
                        _parameterInfos[i].ParameterType, !_target.IsAsset());
                }
                else if (_parameterInfos[i].ParameterType.GetElementType() != null &&_parameterInfos[i].ParameterType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    if (_fieldValues[i] == null)
                    {
                        _fieldValues[i] = typeof(StringCollectionField).BaseType.GetField("_fieldValue", BindingFlags.Instance | BindingFlags.NonPublic);
                        _parameterValues[i] = new UnityEngine.Object[0];
                    }

                    var objArray = (UnityEngine.Object[])_parameterValues[i];

                    int size = EditorGUILayout.IntField($"{_parameterInfos[i].Name}Size", objArray.Length);

                    if (size != objArray.Length) 
                    {
                        Array.Resize(ref objArray, size);
                        _parameterValues[i] = objArray;
                    }

                    EditorGUI.indentLevel += 1;

                    for (int t = 0; t < objArray.Length; t++)
                    {
                        objArray[t] = EditorGUILayout.ObjectField(new GUIContent($"Element {t}"), objArray[t], _parameterInfos[i].ParameterType.GetElementType(),
                            !_target.IsAsset());
                    }

                    EditorGUI.indentLevel -= 1;
                }
                else if (_parameterInfos[i].ParameterType == typeof(int))
                {
                    if (_parameterValues[i] == null)
                    {
                        _parameterValues[i] = new IntField();
                        _fieldValues[i] = typeof(IntField).BaseType.GetField("_fieldValue", BindingFlags.Instance | BindingFlags.NonPublic);
                    }

                    _fieldValues[i].SetValue(_parameterValues[i], EditorGUILayout.IntField(_parameterInfos[i].Name, (int)_fieldValues[i].GetValue(_parameterValues[i])));
                }                
            }

            if (GUILayout.Button("Create"))
            {
                MethodParameter[] parameters = new MethodParameter[_parameterValues.Length];

                for (int i = 0; i < _parameterValues.Length; i++)
                {
                    if (_parameterValues[i].GetType() == typeof(UnityEngine.Object)|| _parameterValues[i].GetType().IsSubclassOf(typeof(UnityEngine.Object)))
                    {                        
                        (_parameterValues[i] as UnityEngine.Object).GetSceneGuidAndObjectID(out string sceneGuid, out int objectID);
                        _parameterValues[i] = new StringCollectionField();

                        string[] fieldValue = new string[] { sceneGuid, objectID.ToString() };

                        _fieldValues[i].SetValue(_parameterValues[i], fieldValue);

                        parameters[i] = new MethodParameter($"{_parameterInfos[i].ParameterType.AssemblyQualifiedName}",
                                JsonUtility.ToJson(_parameterValues[i]));
                    }
                    else if(_parameterValues[i].GetType().GetElementType() != null && (_parameterValues[i].GetType().GetElementType() == typeof(UnityEngine.Object) ||
                        _parameterValues[i].GetType().GetElementType().IsSubclassOf(typeof(UnityEngine.Object))))
                    {
                        var objArray = (UnityEngine.Object[])_parameterValues[i];
                        string[] sceneGuids = new string[objArray.Length];
                        int[] objectIDs = new int[objArray.Length];

                        for (int t = 0; t < objArray.Length; t++)
                        {
                            objArray[t].GetSceneGuidAndObjectID(out sceneGuids[t], out objectIDs[t]);
                        }

                        _parameterValues[i] = new StringCollectionField();
                        string[] fieldValue = new string[objArray.Length * 2];

                        for (int t = 0; t < sceneGuids.Length; t++)
                        {
                            fieldValue[t * 2] = sceneGuids[t];
                            fieldValue[t * 2 + 1] = objectIDs[t].ToString();
                        }

                        _fieldValues[i].SetValue(_parameterValues[i], fieldValue);

                        parameters[i] = new MethodParameter($"{_parameterInfos[i].ParameterType.AssemblyQualifiedName}",
                                JsonUtility.ToJson(_parameterValues[i]));
                    }
                    else
                    {
                        parameters[i] = new MethodParameter($"{_parameterValues[i].GetType().AssemblyQualifiedName}",
                                JsonUtility.ToJson(_parameterValues[i]));
                    }
                }

                _container.AddMethodParameters(new MethodParameters(_objectID, _sceneGuid, _methodName, parameters));

                Close();
            }
        }
    }
}
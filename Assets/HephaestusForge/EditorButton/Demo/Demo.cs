using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HephaestusForge.EditorButton
{
    public class Demo : MonoBehaviour
    {
        [EditorButton]
        private void Execute(int i, int t, Camera cam)
        {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Execute {i} + {t} = {i + t}, Cam:{cam.name}");
#endif

        }

        [EditorButton]
        private void Test(Transform[] trans)
        {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            for (int i = 0; i < trans.Length; i++)
            {
                Debug.Log($"{trans[i].position}");
            }
#endif
        }
    }
}
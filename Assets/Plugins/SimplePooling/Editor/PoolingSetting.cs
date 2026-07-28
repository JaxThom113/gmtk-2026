using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sezylrin.Pooling.Editor
{
    public class PoolerSettings : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset mainVisualTree = default;
        // Start is called before the first frame update
        [MenuItem("Tools/Sezylrin/PoolingOptions")]
        public static void CreateWindow()
        {
            PoolerSettings wnd = GetWindow<PoolerSettings>();
            wnd.titleContent = new GUIContent("Pooler Settings");
        }
        private Toggle disableWarningMessage;
        public void CreateGUI()
        {
            mainVisualTree.CloneTree(rootVisualElement);
            disableWarningMessage = rootVisualElement.Q<Toggle>("disableWarning");
            disableWarningMessage.RegisterValueChangedCallback((evt) =>
            {
                EditorPrefs.SetBool("SezylrinPoolingSettings", evt.newValue);
            });

        }


    }
}

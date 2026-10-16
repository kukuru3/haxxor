
using UnityEngine;

namespace Haxxor.View {
    [RequireComponent(typeof(HaxxorConsoleView))]
    class HaxxorConsoleTester : MonoBehaviour {
        #pragma warning disable 649
        [SerializeField] TextAsset textAsset;
        #pragma warning restore 649

        internal void InjectAsset() {
            var c = GetComponent<HaxxorConsoleView>().Console;
            c.cpu.Reset();
            if (textAsset != null) c.Print(textAsset.text);
        }
    }
}

#if UNITY_EDITOR
namespace Haxxor.Editors {
    using UnityEditor;
    using UnityEngine;
    using Haxxor.View;
    [CustomEditor(typeof(HaxxorConsoleTester))]
    class ConsoleTesterEditor: Editor {
        HaxxorConsoleTester Target => (HaxxorConsoleTester)target;
        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.OnInspectorGUI();
            if (Application.isPlaying) {
                if (GUILayout.Button("Print text")) {
                    Target.InjectAsset();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

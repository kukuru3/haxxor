using System.Collections.Generic;

using UnityEngine;

namespace Haxxor.View {
    public class HaxxorConsoleView : MonoBehaviour {
        
        #pragma warning disable 649
        [SerializeField] byte numLines;
        [SerializeField] byte charactersPerLine;
        [SerializeField] GameObject linePrefab;
        [SerializeField] [TextArea(3, 10)]string initialTextPrint;
        [Tooltip("If true, will use reflection on startup to locate all Haxxor commands defined in your project and register them. If this is unchecked, you must manually call Console.RegisterCommand for EVERY command and alias you intend to use in the project")]
        [SerializeField] bool autoRegisterCommands;
        [SerializeField] byte cycleSpeed;
        [SerializeField] byte lettersPerCycle;
        #pragma warning restore 649

        List<UnityEngine.UI.Text> listOfTexts;

        public HaxxorComputer Console { get; private set; }

        Vector2[] cachedPositions;

        private void Start() {    
            GenerateTexts();
            foreach (var text in listOfTexts) text.text = "";
            Console = new HaxxorComputer(numLines, charactersPerLine, 128);
            Console.bios.cyclesPerSecond = cycleSpeed;
            Console.bios.charsPerCycle = lettersPerCycle;
            
            Console.Events.MatrixShiftedUp += HandleMatrixShiftedUp;
            Console.Events.LineTextChanged += HandleLineTextChanged;
            if (autoRegisterCommands) 
                ConsoleUtility.AutoregisterInternalCommands(Console);
            Console.Print(initialTextPrint);
        }

        private void OnDestroy() {
            Console.Events.MatrixShiftedUp -= HandleMatrixShiftedUp;
            Console.Events.LineTextChanged -= HandleLineTextChanged;
            Console = null;
        }

        private void Update() {
            Console.ProcessFrame();
            Console.ProcessTimePassed(Time.deltaTime);
        }

        private void HandleLineTextChanged(int lineID, string newText) {
            // Debug.Log($"Reprinting line {lineID}");
            var key = GetKeyOfLine(lineID);
            listOfTexts[key].text = newText;
        }

        int shiftUpIndex;

        int GetKeyOfLine(int line) => (shiftUpIndex + line) % numLines;

        private void HandleMatrixShiftedUp() {
            shiftUpIndex++;

            for (var l = 0; l < numLines; l++) {
                listOfTexts[GetKeyOfLine(l)].GetComponent<RectTransform>().anchoredPosition = cachedPositions[l];
            }
            listOfTexts[GetKeyOfLine(numLines-1)].text = "";
        }

        private void GenerateTexts() {
            listOfTexts = new List<UnityEngine.UI.Text>();
            cachedPositions = new Vector2[numLines];
            var currentPos = Vector2.zero;
            for (var i = 0; i < numLines; i++) {
                var go = Instantiate(linePrefab);
                go.name = $"Console Line, Key = {i}";
                var rt = go.GetComponent<RectTransform>();
                var offset = new Vector2(0, -rt.sizeDelta.y);
                rt.SetParent(transform, false);
                cachedPositions[i] = currentPos;
                rt.anchoredPosition = cachedPositions[i];
                currentPos += offset;
                listOfTexts.Add(go.GetComponent<UnityEngine.UI.Text>());
            }
        }
    }
}

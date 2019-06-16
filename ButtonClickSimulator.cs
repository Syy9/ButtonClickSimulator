using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Syy.Tools
{
    public class ButtonClickSimulator : EditorWindow
    {
        [MenuItem("Window/ButtonClickSimulator")]
        public static void Open()
        {
            GetWindow<ButtonClickSimulator>(typeof(ButtonClickSimulator).Name);
        }

        List<DataSet> _targets = new List<DataSet>(16);

        void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Please Play");
                return;
            }

            if (GUILayout.Button("Search Buttons"))
            {
                Search();
            }

            if (_targets.Any())
            {
                var isSimualteTargetOption = GUILayout.Width(100);
                var priorityOption = GUILayout.Width(70);
                var buttonOption = GUILayout.ExpandWidth(true);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Is simulate target", isSimualteTargetOption);
                    EditorGUILayout.LabelField("Priority", priorityOption);
                    EditorGUILayout.LabelField("Button", buttonOption);
                }
                foreach (var dataset in _targets)
                {
                    if (dataset.Button == null) continue;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        dataset.IsSimulateTarget = EditorGUILayout.ToggleLeft("", dataset.IsSimulateTarget, isSimualteTargetOption);
                        using (new EditorGUI.DisabledScope(!dataset.IsSimulateTarget))
                        {
                            dataset.Priority = EditorGUILayout.IntField("", dataset.Priority, priorityOption);
                            EditorGUILayout.ObjectField(dataset.Button?.gameObject, typeof(GameObject), true, buttonOption);
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(_targets.All(value => !value.IsSimulateTarget)))
                {
                    if (GUILayout.Button("Simualte Click"))
                    {
                        foreach (var dataset in _targets.Where(value => value.IsSimulateTarget).OrderByDescending(value => value.Priority))
                        {
                            if (dataset.Button == null) continue;

                            ExecuteEvents.Execute
                            (
                                target: dataset.Button.gameObject,
                                eventData: new PointerEventData(EventSystem.current),
                                functor: ExecuteEvents.pointerClickHandler
                            );
                        }

                        _targets = _targets.Where(value => value.Button != null).ToList();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not found Buttons... Press [Search Buttons]");
            }
        }

        void Search()
        {
            var buttons = GameObject.FindObjectsOfType<Button>();
            var newTargets = new List<DataSet>(buttons.Length);
            foreach (var button in buttons)
            {
                var dataset = _targets.FirstOrDefault(value => value.Button != null && value.Button == button);
                if (dataset == null)
                {
                    dataset = new DataSet(false, button);
                }
                newTargets.Add(dataset);
            }
            _targets.Clear();
            _targets = newTargets;
        }

        class DataSet
        {
            public bool IsSimulateTarget;
            public int Priority;
            public Button Button;

            public DataSet(bool isSimulateTarget, Button button)
            {
                IsSimulateTarget = isSimulateTarget;
                Button = button;
            }
        }
    }
}

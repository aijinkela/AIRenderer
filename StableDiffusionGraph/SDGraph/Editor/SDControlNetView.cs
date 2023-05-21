using System.Collections.Generic;
using FernGraph;
using FernGraph.Editor;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDGraphCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDControlNet))]
    public class SDControlNetView : NodeView
    {
        private string[] modelNames;
        private string[] moduleNames;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            var controlNet = Target as SDControlNet;
            if(controlNet == null) return;
            extensionContainer.Clear();
            OnAsync();
            var button = new Button(OnAsync);
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.FlexEnd;
            button.style.bottom = 0;
            button.style.right = 0;
            titleButtonContainer.Add(button);
            
            RefreshExpandedState();
        }

        private void OnAsync()
        {
            SetModelNameSelector();
            SetModuleNameSelector();
        }

        private void SetModelNameSelector(){
            var controlNet = Target as SDControlNet;
            if(controlNet == null) return;
             modelNames = controlNet.modelNames;
            if (modelNames != null && modelNames.Length > 0)
            {
                extensionContainer.Clear();
                // Create a VisualElement with a popup field
                var listContainer = new VisualElement();
                listContainer.style.flexDirection = FlexDirection.Row;
                listContainer.style.alignItems = Align.Center;
                listContainer.style.justifyContent = Justify.Center;

                List<string> stringList = new List<string>();
                stringList.AddRange(controlNet.modelNames);
                var popup = new PopupField<string>(stringList, controlNet.currentIndex);

                // Add a callback to perform additional actions on value change
                popup.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log("Selected item: " + evt.newValue);
                    controlNet.ControlNet.model = evt.newValue;
                    controlNet.currentIndex = stringList.IndexOf(evt.newValue);
                });

                listContainer.Add(popup);

                extensionContainer.Add(listContainer);
                RefreshExpandedState();
            }
            else
            {
                EditorCoroutineUtility.StartCoroutine(controlNet.ListModelsAsync(), this);
            }
        }

        private void SetModuleNameSelector(){
            var controlNet = Target as SDControlNet;
            if(controlNet == null) return;
             moduleNames = controlNet.moduleNames;
            if (moduleNames != null && moduleNames.Length > 0)
            {
                // extensionContainer.Clear();
                // Create a VisualElement with a popup field
                var listContainer = new VisualElement();
                listContainer.style.flexDirection = FlexDirection.Row;
                listContainer.style.alignItems = Align.Center;
                listContainer.style.justifyContent = Justify.Center;

                List<string> stringList = new List<string>();
                stringList.AddRange(controlNet.moduleNames);
                var popup = new PopupField<string>(stringList, controlNet.currentIndex);

                // Add a callback to perform additional actions on value change
                popup.RegisterValueChangedCallback(evt =>
                {
                    Debug.Log("Selected item: " + evt.newValue);
                    controlNet.ControlNet.module = evt.newValue;
                    controlNet.currentIndex = stringList.IndexOf(evt.newValue);
                });

                listContainer.Add(popup);

                extensionContainer.Add(listContainer);
                RefreshExpandedState();
            }
            else
            {
                EditorCoroutineUtility.StartCoroutine(controlNet.ListModulesAsync(), this);
            }
        }
    }
}

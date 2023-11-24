using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logical.Editor
{
    public class GenerateNodeClassCustomMenu : CustomMenuElement
    {
        private const string CREATE_BUTTON = "create-button";
        private const string CLOSE_BUTTON = "close-button";
        private const string NODEVIEWDRAWER_CLASS_NAME = "class-name-textfield";
        private const string GRAPH_CLASS_NAME_IMGUI = "related-graph-class-imgui";
        private const string INCLUDE_COMMENTS_TOGGLE = "include-comments-toggle";
        private const string PREVIEW_AREA = "preview-area-textfield";
        private const string ASSET_PATH_LABEL = "asset-path-label";

        private List<string> m_graphClassPopupOptions = new List<string>();
        private HashSet<int> m_unselectablePopupOptions = new HashSet<int>();

        private Button m_closeButton = null;
        private Button m_createButton = null;
        private TextField m_className = null;
        private IMGUIContainer m_relatedGraphClassNameIMGUI = null;
        private Toggle m_includeComments = null;
        private TextField m_previewArea = null;
        private Label m_assetPathLabel = null;

        private TextAsset m_template = null;
        private string m_pendingGeneratedCode = "";
        private string m_pendingAssetPath = "";

        public GenerateNodeClassCustomMenu()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(ResourceAssetPaths.GenerateNodeClassCustomMenu_UXML);
            uxmlAsset.CloneTree(this);

            m_closeButton = this.Q<Button>(CLOSE_BUTTON);
            m_createButton = this.Q<Button>(CREATE_BUTTON);
            m_className = this.Q<TextField>(NODEVIEWDRAWER_CLASS_NAME);
            m_relatedGraphClassNameIMGUI = this.Q<IMGUIContainer>(GRAPH_CLASS_NAME_IMGUI);
            m_includeComments = this.Q<Toggle>(INCLUDE_COMMENTS_TOGGLE);
            m_previewArea = this.Q<TextField>(PREVIEW_AREA);
            m_assetPathLabel = this.Q<Label>(ASSET_PATH_LABEL);

            m_closeButton.clicked += OnCloseButtonPressed;
            m_createButton.clicked += OnCreateButtonPressed;

            m_includeComments.RegisterValueChangedCallback(ChangeTemplate);
            m_className.RegisterValueChangedCallback(OnGraphClassNameChanged);

            m_includeComments.value = true;
            ChangeTemplate(m_includeComments.value);

            UpdatePath();
            Selection.selectionChanged -= UpdatePath;
            Selection.selectionChanged += UpdatePath;
            m_relatedGraphClassNameIMGUI.onGUIHandler += OnRelatedNodeClassDraw;

            SetupPopupOptions();
        }

        private void SetupPopupOptions()
        {
            m_graphClassPopupOptions.Clear();
            m_unselectablePopupOptions.Clear();

            m_graphClassPopupOptions.Add("---");
            foreach (Type graphType in GraphTypeMetadata.GraphToNodes.Keys)
            {
                m_graphClassPopupOptions.Add(graphType.Name);
            }
        }

        private void ChangeTemplate(ChangeEvent<bool> changeEvent)
        {
            ChangeTemplate(changeEvent.newValue);
        }

        private void ChangeTemplate(bool includeComments)
        {
            m_template = Resources.Load<TextAsset>(includeComments
                ? ResourceAssetPaths.Node_Template
                : ResourceAssetPaths.Node_Template_Simple);
            OnGraphClassNameChanged(m_className.value);
        }

        private void OnGraphClassNameChanged(ChangeEvent<string> changeEvent)
        {
            OnGraphClassNameChanged(changeEvent.newValue);
        }

        private void OnGraphClassNameChanged(string className)
        {
            bool nameIsEmpty = string.IsNullOrEmpty(className);
            if (nameIsEmpty)
            {
                className = "***NO*NAME***";
            }

            m_createButton.SetEnabled(!nameIsEmpty);

            string graphClassName = "***NO*NAME***";
            if (m_relatedNodeSelectedIndex > 0)
            {
                graphClassName = m_graphClassPopupOptions[m_relatedNodeSelectedIndex];
            }

            m_pendingGeneratedCode = m_template.text.Replace("NODENAME", className);
            m_pendingGeneratedCode = m_pendingGeneratedCode.Replace("GRAPHNAME", graphClassName);
            m_previewArea.value = m_pendingGeneratedCode;

            UpdatePath();
        }

        private int m_relatedNodeSelectedIndex = 0;

        private void OnRelatedNodeClassDraw()
        {
            EditorGUI.BeginChangeCheck();
            m_relatedNodeSelectedIndex = EditorGUILayout.Popup(new GUIContent(" Related Graph Class"),
                m_relatedNodeSelectedIndex, m_graphClassPopupOptions.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                OnGraphClassNameChanged(m_className.value);
            }
        }

        private void UpdatePath()
        {
            m_pendingAssetPath = GetFullAssetPath(m_className.value);
            m_assetPathLabel.text = m_pendingAssetPath;
        }

        private string GetFullAssetPath(string name)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (File.Exists(assetPath))
                assetPath = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(assetPath)) assetPath = "Assets";

            return $"{assetPath}\\{name}.cs";
        }

        private void OnCloseButtonPressed()
        {
            OnCloseClicked?.Invoke();
        }

        private void OnCreateButtonPressed()
        {
            if (string.IsNullOrEmpty(m_className.value))
            {
                Debug.LogError("New NodeViewDrawer class name not set!!!");
                return;
            }
            else if (File.Exists(m_pendingAssetPath))
            {
                Debug.LogError($"File at file path {m_pendingAssetPath} already exists!");
                return;
            }
            else
            {
                StreamWriter sw = File.CreateText(m_pendingAssetPath);
                sw.Write(m_pendingGeneratedCode);
                sw.Close();

                AssetDatabase.Refresh();
            }
        }
    }
}
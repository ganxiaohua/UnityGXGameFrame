using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEditor.IMGUI.Controls;
#endif
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using FairyGUI;

namespace FairyGUIEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class PackagesWindow : EditorWindow
    {
        Vector2 scrollPos1;
        Vector2 scrollPos2;
        GUIStyle itemStyle;

        string selectedPackageName;
        string selectedComponentName;

        SearchField searchField;
        string searchText;

        List<PackageItem> packageItems = new List<PackageItem>();
        UIPackage selectedPkg;

        public PackagesWindow()
        {
            this.maxSize = new Vector2(550, 400);
            this.minSize = new Vector2(550, 400);
        }

        public void SetSelection(string packageName, string componentName)
        {
            selectedPackageName = packageName;
            selectedComponentName = componentName;
        }

        void OnGUI()
        {
            if (itemStyle == null)
                itemStyle = new GUIStyle(GUI.skin.GetStyle("Tag MenuItem"));

            if (searchField == null)
            {
                searchField = new SearchField();
            }

            searchText = searchField.OnToolbarGUI(searchText);

            EditorGUILayout.BeginHorizontal();

            //package list start------
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Packages", (GUIStyle)"OL Title", GUILayout.Width(300));
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);

            scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1, (GUIStyle)"CN Box", GUILayout.Height(300), GUILayout.Width(300));
            EditorToolSet.LoadPackages();
            List<UIPackage> pkgs = UIPackage.GetPackages();
            int cnt = pkgs.Count;
            packageItems.Clear();
            if (cnt == 0)
            {
                selectedPkg = null;
                selectedPackageName = null;
                selectedComponentName = null;
            }
            else
            {
                for (int i = 0; i < cnt; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (IsSearched(pkgs[i].name, searchText) && GUILayout.Toggle(selectedPackageName == pkgs[i].name, pkgs[i].name, itemStyle, GUILayout.ExpandWidth(true)))
                    {
                        selectedPkg = pkgs[i];
                        selectedPackageName = pkgs[i].name;
                    }
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var items = pkgs[i].GetItems();
                        for (int j = 0; j < items.Count; j++)
                        {
                            var pi = items[j];
                            if (pi.type == PackageItemType.Component && pi.exported && IsSearched(pi.name, searchText))
                            {
                                if (!packageItems.Contains(pi))
                                    packageItems.Add(pi);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            //package list end------

            //component list start------

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Components", (GUIStyle)"OL Title", GUILayout.Width(220));
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);

            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, (GUIStyle)"CN Box", GUILayout.Height(300), GUILayout.Width(220));
            if (string.IsNullOrEmpty(searchText) && selectedPkg != null)
            {
                foreach(var item in selectedPkg.GetItems())
                {
                    if (!packageItems.Contains(item))
                        packageItems.Add(item);
                }
            }
            foreach (PackageItem pi in packageItems)
            {
                if (pi.type == PackageItemType.Component && pi.exported)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(selectedComponentName == pi.name, pi.name, itemStyle, GUILayout.ExpandWidth(true)))
                    {
                        selectedPkg = pi.owner;
                        selectedPackageName = pi.owner.name;
                        selectedComponentName = pi.name;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            //component list end------

            GUILayout.Space(10);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            //buttons start---
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(180);

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                EditorToolSet.ReloadPackages();

            GUILayout.Space(20);
            if (GUILayout.Button("OK", GUILayout.Width(100)) && selectedPkg != null)
            {
                string tmp = selectedPkg.assetPath.ToLower();
                string packagePath;
                int pos = tmp.LastIndexOf("/resources/");
                if (pos != -1)
                    packagePath = selectedPkg.assetPath.Substring(pos + 11);
                else
                {
                    pos = tmp.IndexOf("resources/");
                    if (pos == 0)
                        packagePath = selectedPkg.assetPath.Substring(pos + 10);
                    else
                        packagePath = selectedPkg.assetPath;
                }
                if (Selection.activeGameObject != null)
                {
#if UNITY_2018_3_OR_NEWER
                    bool isPrefab = PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) != PrefabAssetType.NotAPrefab;
#else
                    bool isPrefab = PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab;
#endif
                    Selection.activeGameObject.SendMessage("OnUpdateSource",
                        new object[] { selectedPkg.name, packagePath, selectedComponentName, !isPrefab },
                        SendMessageOptions.DontRequireReceiver);
                }

#if UNITY_2018_3_OR_NEWER
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                else
                    ApplyChange();
#else
                ApplyChange();
#endif
                this.Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        void ApplyChange()
        {
#if UNITY_5_3_OR_NEWER
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#elif UNITY_5
            EditorApplication.MarkSceneDirty();
#else
            EditorUtility.SetDirty(Selection.activeGameObject);
#endif
        }

        static bool IsSearched(string source, string search)
        {
            if (string.IsNullOrEmpty(source))
                return true;
            return string.IsNullOrEmpty(search) || source.ToLower().Contains(search.ToLower());
        }
    }
}

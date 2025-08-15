using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class ConsoleCSharp : VirtualBaseDialog
    {
        private static readonly string HistoryConsoleScript = typeof(ConsoleCSharp).FullName;
        private static readonly string HistoryConsoleUsings = typeof(ConsoleCSharp).FullName + ".using";

        [MenuItem("GX框架工具/Console/CSharp", false, 102)]
        public static void Open()
        {
            VirtualDialogContainer.Open<ConsoleCSharp>((dialog) => dialog.OnAwake());
        }

        private ToolbarToggle collectToggle;
        private ToolbarMenu collectMenu;
        private TextField scriptField;
        private string script = "";

        private ToolbarToggle usingsToggle;
        private TextField usingsField;
        private string usings = "";

        private ToolbarToggle referencesToggle;
        private TextField referencesField;

        protected void OnAwake()
        {
            titleContent = new GUIContent("控制台(c#)");

            size = new Vector2(480, 240);
            minSize = new Vector2(480, 120);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            script = UnityEditor.EditorPrefs.GetString(HistoryConsoleScript);
            usings = UnityEditor.EditorPrefs.GetString(HistoryConsoleUsings);

            HistoryCollectScripts.Init();

            container.rootVisualElement.Clear();

            var root = new VisualElement();
            root.style.marginLeft = 4;
            root.style.marginRight = 4;
            root.style.marginTop = 4;
            root.style.marginBottom = 4;
            root.style.flexGrow = 1;
            container.rootVisualElement.Add(root);

            // toolbar 
            var toolbar = new Toolbar();
            toolbar.style.marginLeft = 32;
            BuildToolbar(toolbar);

            // body
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            var body = new VisualElement();
            body.style.flexGrow = 1;
            body.style.flexDirection = FlexDirection.Row;
            scrollView.Add(body);
            BuildBody(body);

            // buttons
            var buttons = new VisualElement();
            buttons.style.minHeight = 20;
            buttons.style.flexDirection = FlexDirection.RowReverse;
            BuildButtons(buttons);

            root.Add(toolbar);
            root.Add(scrollView);
            root.Add(buttons);
        }

        private void BuildToolbar(VisualElement toolbar)
        {
            collectToggle = new ToolbarToggle()
            {
                    style =
                    {
                            unityFontStyleAndWeight = FontStyle.Bold,
                    }
            };
            collectToggle.SetValueWithoutNotify(HistoryCollectScripts.Contain(script));
            collectToggle.RegisterValueChangedCallback(_ =>
            {
                if (HistoryCollectScripts.Contain(script))
                {
                    HistoryCollectScripts.Remove(script);
                }
                else
                {
                    HistoryCollectScripts.Add(script);
                }

                RefreshCollectToggle();
                RefreshCollectMenu();
            });
            collectMenu = new ToolbarMenu() {text = "收藏列表"};
            toolbar.Add(collectMenu);
            toolbar.Add(collectToggle);

            toolbar.Add(new VisualElement()
            {
                    style =
                    {
                            width = 100
                    }
            });

            usingsToggle = new ToolbarToggle()
            {
                    text = "头文件",
                    style =
                    {
                            unityFontStyleAndWeight = FontStyle.Bold,
                            color = Color.gray,
                    }
            };
            usingsToggle.RegisterValueChangedCallback(e =>
            {
                usingsToggle.style.color = e.newValue ? Color.white : Color.gray;
                usingsField.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            toolbar.Add(usingsToggle);

            referencesToggle = new ToolbarToggle()
            {
                    text = "库引用",
                    style =
                    {
                            unityFontStyleAndWeight = FontStyle.Bold,
                            color = Color.gray,
                    }
            };
            referencesToggle.RegisterValueChangedCallback(e =>
            {
                referencesToggle.style.color = e.newValue ? Color.white : Color.gray;
                referencesField.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            toolbar.Add(referencesToggle);

            RefreshCollectToggle();
            RefreshCollectMenu();
        }

        private void BuildBody(VisualElement body)
        {
            StringBuilder lineSB = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                lineSB.AppendLine($"{i + 1}");
            }

            var lineLabel = new TextField()
            {
                    multiline = true,
                    style =
                    {
                            width = 28,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            unityFont = EditorStylesHelper.FontMonospace,
                            marginLeft = 0,
                            marginRight = 0,
                    },
                    value = lineSB.ToString(),
            };
            var exactLineLabel = lineLabel.ElementAt(0);
            exactLineLabel.style.unityTextAlign = TextAnchor.UpperRight;
            lineLabel.SetEnabled(false);
            body.Add(lineLabel);

            usingsField = new TextField()
            {
                    multiline = true,
                    style =
                    {
                            flexGrow = 1,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            unityFont = EditorStylesHelper.FontMonospace,
                            maxWidth = 300,
                            display = DisplayStyle.None,
                    },
            };
            usingsField.SetValueWithoutNotify(usings);
            usingsField.RegisterValueChangedCallback((e) =>
            {
                usings = e.newValue;
                UnityEditor.EditorPrefs.SetString(HistoryConsoleUsings, usings);
            });
            body.Add(usingsField);

            referencesField = new TextField()
            {
                    multiline = true,
                    style =
                    {
                            flexGrow = 1,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            unityFont = EditorStylesHelper.FontMonospace,
                            display = DisplayStyle.None,
                    },
                    isReadOnly = true,
            };
            var references = "";
            foreach (var reference in CSharpRunner.GetReferences())
            {
                references += $"{reference}\n";
            }

            referencesField.SetValueWithoutNotify(references);
            body.Add(referencesField);

            scriptField = new TextField()
            {
                    multiline = true,
                    style =
                    {
                            flexGrow = 1,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            unityFont = EditorStylesHelper.FontMonospace,
                    },
            };
            scriptField.SetValueWithoutNotify(script);
            scriptField.RegisterValueChangedCallback((e) =>
            {
                EditUndo.Record(script);
                script = e.newValue;
                UnityEditor.EditorPrefs.SetString(HistoryConsoleScript, script);
                RefreshCollectToggle();
            });
            scriptField.RegisterCallback<KeyDownEvent>((e) =>
            {
                if (e.ctrlKey)
                {
                    if (e.keyCode == KeyCode.Z && EditUndo.HasUndo())
                    {
                        script = EditUndo.PerformUndo(script);
                        scriptField.SetValueWithoutNotify(script);
                        UnityEditor.EditorPrefs.SetString(HistoryConsoleScript, script);
                        RefreshCollectToggle();
                        e.StopImmediatePropagation();
                    }

                    if (e.keyCode == KeyCode.Y && EditUndo.HasRedo())
                    {
                        script = EditUndo.PerformRedo(script);
                        scriptField.SetValueWithoutNotify(script);
                        UnityEditor.EditorPrefs.SetString(HistoryConsoleScript, script);
                        RefreshCollectToggle();
                        e.StopImmediatePropagation();
                    }
                }
            });
            body.Add(scriptField);
        }

        private void BuildButtons(VisualElement buttons)
        {
            var btnExecute = new Button()
            {
                    text = "执行脚本",
                    style =
                    {
                            color = Color.green,
                            width = 80,
                            unityFontStyleAndWeight = FontStyle.Bold,
                    }
            };
            btnExecute.clicked += () => { ExecuteAsync(btnExecute).Forget(); };
            buttons.Add(btnExecute);
        }

        void RefreshCollectToggle()
        {
            collectToggle.text = HistoryCollectScripts.Contain(script) ? "移除" : "收藏";
            collectToggle.style.color = HistoryCollectScripts.Contain(script) ? new Color(1f, 0.5f, 0f) : Color.green;
            collectToggle.SetEnabled(!string.IsNullOrEmpty(script));
        }

        void RefreshCollectMenu()
        {
            collectMenu.menu.MenuItems().Clear();
            var menus = HistoryCollectScripts.GetMenus();
            for (int i = 0; i < menus.Length; i++)
            {
                var index = i;
                collectMenu.menu.AppendAction(menus[i], (_) => { scriptField.value = HistoryCollectScripts.GetCode(index); },
                        (_) => DropdownMenuAction.Status.Normal);
            }
        }

        private async UniTaskVoid ExecuteAsync(Button button)
        {
            button.SetEnabled(false);
            button.text = "编译中...";
            await CSharpRunner.Execute(usings, script);
            button.SetEnabled(true);
            button.text = "执行脚本";
        }

        protected class HistoryCollectScripts
        {
            private static readonly string Reg = typeof(HistoryCollectScripts).FullName;

            [Serializable]
            private class Data
            {
                public List<string> list = new List<string>();
            }

            private static Data data;

            public static void Init()
            {
                string str = UnityEditor.EditorPrefs.GetString(Reg);
                data = JsonUtility.FromJson<Data>(str);
                if (data == null)
                    data = new Data();
            }

            public static bool Contain(string code)
            {
                return data.list.Contains(code);
            }

            public static void Add(string code)
            {
                data.list.Add(code);
                UnityEditor.EditorPrefs.SetString(Reg, JsonUtility.ToJson(data));
            }

            public static void Remove(string code)
            {
                data.list.Remove(code);
                UnityEditor.EditorPrefs.SetString(Reg, JsonUtility.ToJson(data));
            }

            public static string GetCode(int index)
            {
                return data.list[index];
            }

            public static string[] GetMenus()
            {
                Dictionary<string, int> cache = new Dictionary<string, int>();
                string[] menus = new string[data.list.Count];
                for (int i = 0; i < data.list.Count; i++)
                {
                    string code = data.list[i];
                    int index = code.IndexOf('\n');
                    if (index != -1)
                        menus[i] = code.Substring(0, index).Replace(" ", "").TrimStart('/');
                    else
                        menus[i] = "unknown";
                    if (cache.TryGetValue(menus[i], out var j))
                    {
                        menus[j] += $"/{j}";
                        menus[i] += $"/{i}";
                    }

                    cache.Add(menus[i], i);
                }

                return menus;
            }
        }

        protected class EditUndo
        {
            private string text;
            private double lastEditTime;

            private static Stack<EditUndo> undos = new Stack<EditUndo>();
            private static Stack<EditUndo> redos = new Stack<EditUndo>();

            private const double GROUP_INTERVAL = 1f;

            public static void Clear()
            {
                undos.Clear();
                redos.Clear();
            }

            public static void Record(string text)
            {
                double curTime = EditorApplication.timeSinceStartup;
                if (undos.Count > 0 && (Math.Abs(curTime - undos.Peek().lastEditTime) <= GROUP_INTERVAL || undos.Peek().text == text))
                {
                    undos.Peek().lastEditTime = curTime;
                }
                else
                {
                    undos.Push(new EditUndo() {text = text, lastEditTime = curTime});
                }

                redos.Clear();
            }

            public static bool HasUndo() => undos.Count > 0;
            public static bool HasRedo() => redos.Count > 0;

            public static string PerformUndo(string text)
            {
                if (undos.Count > 0)
                {
                    double curTime = EditorApplication.timeSinceStartup;
                    redos.Push(new EditUndo() {text = text, lastEditTime = curTime});
                    return undos.Pop().text;
                }

                return "";
            }

            public static string PerformRedo(string text)
            {
                if (redos.Count > 0)
                {
                    double curTime = EditorApplication.timeSinceStartup;
                    undos.Push(new EditUndo() {text = text, lastEditTime = curTime});
                    return redos.Pop().text;
                }

                return "";
            }
        }
    }
}
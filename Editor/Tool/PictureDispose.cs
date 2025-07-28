using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public class TextureEditor
    {

        public class TextureEditorWindow : OdinEditorWindow
        {
            [HideInInspector] public string SavePath = "ToolsData/图片设置.json";
            [HideInInspector] public string[] Extensions = new[] {".jpg", ".png"};

            [Serializable]
            public class TextureData
            {
                public TextureData(string platformString)
                {
                    PlatformString = platformString;
                }

                public enum TextureSize
                {
                    T64 = 64,
                    T128 = 128,
                    T256 = 256,
                    T512 = 512,
                    T1024 = 1024,
                    T2048 = 2048,
                }


                public string Name;
                [ReadOnly] public string Path;

                public TextureSize TexSize = TextureSize.T1024;

                public TextureImporterType TextureType = TextureImporterType.Sprite;

                public TextureWrapMode WrapMode = TextureWrapMode.Repeat;

                [ReadOnly] public string PlatformString;

                [Button("应用")]
                public void Do()
                {
                    window.SetTextureData(this);
                    window.Save();
                }

                [Button("删除")]
                public void Remove()
                {
                    window.Remove(Path);
                }
            }


            private class TextureImporterData
            {
                public List<TextureImporter> TextureImporterList = new();
                public TextureData TextureData;
            }

            [Serializable]
            public class TextureDatas
            {
                public List<TextureData> TexWeb = new();

                public List<TextureData> TexIOS = new();

                public List<TextureData> TexAndroid = new();

                public List<TextureData> TexPc = new();
            }

            public enum Platform
            {
                PC,
                Android,
                IOS,
                Web,
            }

            private TextureDatas textureDatas;

            private List<string> existingPath = new();

            /// <summary>
            /// 当前平台下的数据
            /// </summary>
            [ListDrawerSettings(NumberOfItemsPerPage = 5, HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
            public List<TextureData> TexDataList;

            private TextureImporterData CurTextureImporterData;

            private static TextureEditorWindow window;


            [Title("需要添加的路径")] [HideLabel] [FolderPath(ParentFolder = "Assets/", AbsolutePath = true)]
            public string NeedPath;

            public static void OpenWindow()
            {
                if (window == null)
                {
                    window = GetWindow<TextureEditorWindow>();
                    window.CurTextureImporterData = new TextureImporterData();
                    window.name = "纹理设置";
                    if (File.Exists(window.SavePath))
                    {
                        window.textureDatas = JsonUtility.FromJson<TextureDatas>(File.ReadAllText(window.SavePath));
                        foreach (var item in window.textureDatas.TexPc)
                        {
                            window.existingPath.Add(item.Path);
                        }
                    }
                    else
                    {
                        window.textureDatas = new TextureDatas();
                    }

                    window.maxSize = new Vector2(400, 600);
                    window.minSize = new Vector2(400, 300);
                    window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 600);
                    window.TexDataList = window.textureDatas.TexPc;
                }
            }

            [Button("添加路径")]
            [PropertyOrder(2)]
            public void AddPicturePath()
            {
                if (string.IsNullOrEmpty(NeedPath))
                {
                    Debug.Log("请选择需要添加的路径!");
                    return;
                }
                string needAddPath = NeedPath.Replace(Application.dataPath, "Assets");
                if (existingPath.Contains(needAddPath))
                {
                    Debug.Log("这个路径已经加入过了!");
                    return;
                }

                void add(List<TextureData> picTureClass, string platform)
                {
                    TextureData picturepath = new TextureData(platform);
                    picturepath.Path = needAddPath;
                    picturepath.Name = Path.GetFileName(NeedPath);
                    picTureClass.Add(picturepath);
                }

                existingPath.Add(needAddPath);
                add(textureDatas.TexWeb, "WebGL");
                add(textureDatas.TexIOS, "iPhone");
                add(textureDatas.TexAndroid, "Android");
                add(textureDatas.TexPc, "Standalone");
            }

            [Button("应用选中平台")]
            [PropertyOrder(3)]
            public void Use()
            {
                SetCurTextureData();
                Save();
            }

            [Button("应用所有平台")]
            [PropertyOrder(4)]
            public void UseALL()
            {
                SetAllTextureData();
                Save();
            }

            [Button("保存设置")]
            [PropertyOrder(5)]
            public void Save()
            {
                if (!Directory.Exists("ToolsData"))
                {
                    Directory.CreateDirectory("ToolsData");
                }

                string json = JsonUtility.ToJson(textureDatas);
                Debug.Log(SavePath);
                File.WriteAllText(SavePath, json);
                Debug.Log("保存完毕");
            }

            private void Remove(string str)
            {
                void remove(List<TextureData> temp)
                {
                    foreach (var inet in temp)
                    {
                        if (inet.Path == str)
                        {
                            temp.Remove(inet);
                            return;
                        }
                    }
                }

                remove(textureDatas.TexPc);
                remove(textureDatas.TexIOS);
                remove(textureDatas.TexAndroid);
                remove(textureDatas.TexWeb);
            }

            [EnumToggleButtons] [PropertyOrder(6)] [Title("选择平台")] [OnValueChanged("PloatformChange")]
            public Platform PlatformEnmu;


            public void PloatformChange()
            {
                if (PlatformEnmu == Platform.Android)
                {
                    TexDataList = textureDatas.TexAndroid;
                }
                else if (PlatformEnmu == Platform.Web)
                {
                    TexDataList = textureDatas.TexWeb;
                }
                else if (PlatformEnmu == Platform.IOS)
                {
                    TexDataList = textureDatas.TexIOS;
                }
                else if (PlatformEnmu == Platform.PC)
                {
                    TexDataList = textureDatas.TexPc;
                }
            }


            private void GetTexture(TextureData textureData)
            {
                CurTextureImporterData.TextureData = textureData;
                CurTextureImporterData.TextureImporterList.Clear();
                string unityPath = textureData.Path.Replace("/", "\\");
                List<string> images = Directory
                    .EnumerateFiles(unityPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => Extensions.Any(file.ToLower().EndsWith))
                    .ToList();

                foreach (var path in images)
                {
                    string directoryName = Path.GetDirectoryName(path);
                    bool addPath = true;
                    //排除子文件夹中已经添加近修改列表的组合
                    if (unityPath != directoryName)
                    {
                        foreach (var picture in TexDataList)
                        {
                            string pictureDirectory = picture.Path.Replace("/", "\\");
                            if (pictureDirectory == directoryName)
                            {
                                addPath = false;
                                break;
                            }
                        }
                    }

                    if (addPath)
                    {
                        string upath = path.Replace(Application.dataPath, "Assets/");
                        var assetImporter = (TextureImporter) AssetImporter.GetAtPath(upath);
                        CurTextureImporterData.TextureImporterList.Add(assetImporter);
                    }
                }
            }

            /// <summary>
            /// 设置当前选中平台纹理
            /// </summary>
            private void SetCurTextureData()
            {
                for (int i = 0; i < TexDataList.Count; i++)
                {
                    SetTextureData(TexDataList[i]);
                }
            }

            /// <summary>
            /// 设置所有纹理
            /// </summary>
            private void SetAllTextureData()
            {
                void Impoter(List<TextureData> piturdatas)
                {
                    for (int i = 0; i < piturdatas.Count; i++)
                    {
                        SetTextureData(piturdatas[i]);
                    }
                }

                Impoter(textureDatas.TexPc);
                Impoter(textureDatas.TexWeb);
                Impoter(textureDatas.TexAndroid);
                Impoter(textureDatas.TexIOS);
            }

            private void SetTextureData(TextureData textureData)
            {
                GetTexture(textureData);

                foreach (TextureImporter textureImporter in CurTextureImporterData.TextureImporterList)
                {
                    var data = CurTextureImporterData.TextureData;
                    textureImporter.wrapMode = data.WrapMode;
                    textureImporter.textureType = data.TextureType;
                    textureImporter.mipmapEnabled = false;
                    TextureImporterSettings textureSettings = new TextureImporterSettings();
                    TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings(textureData.PlatformString);
                    platformSettings.maxTextureSize = (int) textureData.TexSize;
                    platformSettings.overridden = true;
                    platformSettings.format = SetTextFormat(textureData.PlatformString);
                    textureImporter.ReadTextureSettings(textureSettings);
                    textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                    textureImporter.SetPlatformTextureSettings(platformSettings);
                    textureImporter.SetTextureSettings(textureSettings);
                    textureImporter.SaveAndReimport();
                }
            }


            private TextureImporterFormat SetTextFormat(string str)
            {
                if (str.Equals("Standalone"))
                {
                    return TextureImporterFormat.RGBA32;
                }
                else if (str.Equals("Android"))
                {
                    return TextureImporterFormat.ASTC_4x4;
                }
                else if (str.Equals("iPhone"))
                {
                    return TextureImporterFormat.ASTC_4x4;
                }
                else if (str.Equals("WebGL"))
                {
                    return TextureImporterFormat.ETC2_RGB4;
                }
                
                return TextureImporterFormat.ASTC_4x4;
            }
        }
    }
}
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

namespace GameFrame.Editor
{
    public class PictureDispose
    {
        // [MenuItem("工具/图片整体修改")]
        public static void Picture()
        {
            PictureEditor.OpenWindow();
        }


        public class PictureEditor : OdinEditorWindow
        {
            [HideInInspector] public string SavePath = "ToolsData/图片设置.json";
            [HideInInspector] public string[] Extensions = new[] {".jpg", ".png"};

            [Serializable]
            public class PictureData
            {
                public PictureData(string platformString)
                {
                    PlatformString = platformString;
                }

                public enum Resolution
                {
                    T64 = 64,
                    T128 = 128,
                    T256 = 256,
                    T512 = 512,
                    T1024 = 1024,
                    T2048 = 2048,
                }

                public enum TextureType
                {
                    Retain,
                    Texture,
                    Sprite,
                }

                public enum WrapMode
                {
                    Repeat,
                    Clamp,
                }

                public string Name;
                [ReadOnly] public string Path;

                public Resolution CurResolution = Resolution.T512;

                public TextureType CurTextureType = TextureType.Sprite;

                public WrapMode CurWrapMode = WrapMode.Repeat;

                [ReadOnly] public string PlatformString;

                [Button("应用")]
                // [EnableIf("@PictureEditor.window.IsThisPloatform()")]
                public void Use()
                {
                    PictureEditor.window.SetImporter(this);
                    PictureEditor.window.Save();
                }

                [Button("删除")]
                public void Remove()
                {
                    PictureEditor.window.Remove(Path);
                }
            }

            public enum Platform
            {
                PC,
                Android,
                IOS,
                Web,
            }

            public class TextureImporterData
            {
                public List<TextureImporter> TextureImporterList = new();
                public PictureData PictureData;
            }

            [Serializable]
            public class JsonClass
            {
                public List<PictureData> PicTureWeb = new();

                public List<PictureData> pictureIos = new();

                public List<PictureData> pictureAndrod = new();

                public List<PictureData> picturePc = new();
            }

            private JsonClass PicTureClass;

            private List<string> ExistingPath = new();

            /// <summary>
            /// 当前平台下的数据
            /// </summary>
            [ListDrawerSettings(NumberOfItemsPerPage = 5, HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
            public List<PictureData> PlatformPictureData;

            [HideInInspector] public TextureImporterData CurTextureImporterData;

            public static PictureEditor window;

            public static void OpenWindow()
            {
                if (window == null)
                {
                    window = GetWindow<PictureEditor>();
                    window.CurTextureImporterData = new TextureImporterData();
                    // window.m_TextureImporterDictionary = new();
                    if (File.Exists(window.SavePath))
                    {
                        window.PicTureClass = JsonUtility.FromJson<JsonClass>(File.ReadAllText(window.SavePath));
                        foreach (var item in window.PicTureClass.picturePc)
                        {
                            window.ExistingPath.Add(item.Path);
                        }
                    }
                    else
                    {
                        window.PicTureClass = new JsonClass();
                    }

                    window.maxSize = new Vector2(400, 600);
                    window.minSize = new Vector2(400, 300);
                    window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 600);
                    window.PlatformPictureData = window.PicTureClass.picturePc;
                }

                InitPloatform(window.PlatformEnmu);
            }


            [Title("需要添加的路径")] [HideLabel] [FolderPath(ParentFolder = "Assets/", AbsolutePath = true)]
            public string NeedPath;

            [Button("添加路径")]
            [PropertyOrder(2)]
            public void AddPicturePath()
            {
                string needAddPath = NeedPath.Replace(Application.dataPath, "Assets");
                if (ExistingPath.Contains(needAddPath))
                {
                    Debug.Log("这个路径已经加入过了!");
                    return;
                }

                void add(List<PictureData> picTureClass, string platform)
                {
                    PictureData picturepath = new PictureData(platform);
                    picturepath.Path = needAddPath;
                    picturepath.Name = Path.GetFileName(NeedPath);
                    picTureClass.Add(picturepath);
                }

                ExistingPath.Add(needAddPath);
                add(PicTureClass.PicTureWeb, "WebGL");
                add(PicTureClass.pictureIos, "iPhone");
                add(PicTureClass.pictureAndrod, "Android");
                add(PicTureClass.picturePc, "Standalone");
            }

            [Button("应用当前平台")]
            [PropertyOrder(3)]
            // [EnableIf("@this.IsThisPloatform()")]
            public void Use()
            {
                SetCurImporter();
                Save();
            }

            [Button("应用所有平台")]
            [PropertyOrder(3)]
            // [EnableIf("@this.IsThisPloatform()")]
            public void UseALL()
            {
                SetAllImporter();
                Save();
            }

            [Button("保存设置")]
            [PropertyOrder(4)]
            public void Save()
            {
                if (!Directory.Exists("ToolsData"))
                {
                    Directory.CreateDirectory("ToolsData");
                }

                string json = JsonUtility.ToJson(PicTureClass);
                Debug.Log(SavePath);
                File.WriteAllText(SavePath, json);
                Debug.Log("保存完毕");
            }

            [EnumToggleButtons] [PropertyOrder(5)] [Title("平台")] [OnValueChanged("PloatformChange")]
            public Platform PlatformEnmu;


            public void PloatformChange()
            {
                if (PlatformEnmu == Platform.Android)
                {
                    PlatformPictureData = PicTureClass.pictureAndrod;
                }
                else if (PlatformEnmu == Platform.Web)
                {
                    PlatformPictureData = PicTureClass.PicTureWeb;
                }
                else if (PlatformEnmu == Platform.IOS)
                {
                    PlatformPictureData = PicTureClass.pictureIos;
                }
                else if (PlatformEnmu == Platform.PC)
                {
                    PlatformPictureData = PicTureClass.picturePc;
                }
            }

            public static void InitPloatform(Platform platformEnmu)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    platformEnmu = Platform.Android;
                }
                else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    platformEnmu = Platform.PC;
                }
                else if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    platformEnmu = Platform.Web;
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    platformEnmu = Platform.IOS;
                }
            }

            /// <summary>
            /// 判断是否是当前平台
            /// </summary>
            public bool IsThisPloatform()
            {
                if (PlatformEnmu == Platform.Android && Application.platform == RuntimePlatform.Android)
                {
                    return true;
                }
                else if (PlatformEnmu == Platform.PC &&
                         (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor))
                {
                    return true;
                }
                else if (PlatformEnmu == Platform.IOS && Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return true;
                }
                else if (PlatformEnmu == Platform.Web && Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    return true;
                }

                return false;
            }

            public void Remove(string str)
            {
                void remove(List<PictureData> temp)
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

                remove(PicTureClass.picturePc);
                remove(PicTureClass.pictureIos);
                remove(PicTureClass.pictureAndrod);
                remove(PicTureClass.PicTureWeb);
            }


            public void GetTexture(PictureData pictureData)
            {
                CurTextureImporterData.PictureData = pictureData;
                CurTextureImporterData.TextureImporterList.Clear();
                string unityPath = pictureData.Path.Replace("/", "\\");
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
                        foreach (var picture in PlatformPictureData)
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
            /// 设置所有图片
            /// </summary>
            public void SetCurImporter()
            {
                for (int i = 0; i < PicTureClass.pictureAndrod.Count; i++)
                {
                    SetImporter(PlatformPictureData[i]);
                }
            }

            /// <summary>
            /// 设置所有图片
            /// </summary>
            public void SetAllImporter()
            {
                void Impoter(List<PictureData> piturdatas)
                {
                    for (int i = 0; i < piturdatas.Count; i++)
                    {
                        SetImporter(piturdatas[i]);
                    }
                }

                Impoter(PicTureClass.picturePc);
                Impoter(PicTureClass.PicTureWeb);
                Impoter(PicTureClass.pictureAndrod);
                Impoter(PicTureClass.pictureIos);
            }

            public void SetImporter(PictureData pictureData)
            {
                GetTexture(pictureData);

                foreach (TextureImporter textureImporter in CurTextureImporterData.TextureImporterList)
                {
                    var data = CurTextureImporterData.PictureData;
                    if (data.CurWrapMode == PictureData.WrapMode.Repeat)
                    {
                        textureImporter.wrapMode = TextureWrapMode.Repeat;
                    }
                    else if (data.CurWrapMode == PictureData.WrapMode.Clamp)
                    {
                        textureImporter.wrapMode = TextureWrapMode.Clamp;
                    }

                    if (data.CurTextureType == PictureData.TextureType.Texture)
                    {
                        textureImporter.textureType = TextureImporterType.Default;
                    }
                    else if (data.CurTextureType == PictureData.TextureType.Sprite)
                    {
                        textureImporter.textureType = TextureImporterType.Sprite;
                    }

                    textureImporter.mipmapEnabled = false;
                    TextureImporterSettings textureSettings = new TextureImporterSettings();
                    TextureImporterPlatformSettings platformSettings = textureImporter.GetPlatformTextureSettings(pictureData.PlatformString);
                    platformSettings.maxTextureSize = (int) pictureData.CurResolution;
                    platformSettings.overridden = true;
                    platformSettings.format = SetTextFormat(pictureData.PlatformString);
                    textureImporter.ReadTextureSettings(textureSettings);
                    textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                    textureImporter.SetPlatformTextureSettings(platformSettings);
                    textureImporter.SetTextureSettings(textureSettings);
                    textureImporter.SaveAndReimport();
                }
            }


            public TextureImporterFormat SetTextFormat(string str)
            {
                if (str == "Standalone")
                {
                    return TextureImporterFormat.RGBA32;
                }
                else if (str == "Android")
                {
                    return TextureImporterFormat.ASTC_4x4;
                }
                else if (str == "iPhone")
                {
                    return TextureImporterFormat.ASTC_4x4;
                }
                else if (str == "WebGL")
                {
                    return TextureImporterFormat.ETC2_RGB4;
                }

                return TextureImporterFormat.ETC2_RGB4;
            }
        }
    }
}
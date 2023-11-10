using System;
using System.IO;
using Bright.Serialization;
// using cfg;
using SimpleJSON;
using UnityEngine;

namespace GameFrame
{
    public class Config : Singleton<Config>
    {
        // public Tables Tables;

        public void LoadTable()
        {
            // var tablesCtor = typeof(cfg.Tables).GetConstructors()[0];
            // var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            // // 根据cfg.Tables的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
            // System.Delegate loader = loaderReturnType == typeof(ByteBuf)
            //     ? new System.Func<string, ByteBuf>(LoadByteBuf)
            //     : (System.Delegate) new System.Func<string, JSONNode>(LoadJson);
            // Tables = (cfg.Tables) tablesCtor.Invoke(new object[] {loader});
        }

//         private static JSONNode LoadJson(string file)
//         {
// #if UNITY_EDITOR
//             return JSON.Parse(File.ReadAllText($"{Application.dataPath}/../GenerateDatas/json/{file}.json", System.Text.Encoding.UTF8));
// #else
//             throw new Exception("正式模式不要使用json");
//             return  "";
// #endif
//         }
//
//         private static ByteBuf LoadByteBuf(string file)
//         {
// #if UNITY_EDITOR
//             return new ByteBuf(File.ReadAllBytes($"{Application.dataPath}/../GenerateDatas/bytes/{file}.bytes"));
// #else
//             byte[] bytes = AssetManager.Instance.Load<TextAsset>($"Assets/GXGame/Config/ConfigData/{file}.bytes").bytes;
//             return new ByteBuf(bytes);
// #endif
//         }
    }
}
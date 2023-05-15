using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GameFrame.Editor
{
    public class JenkinsBuild
    {
        public static void BuildBundles()
        {
            var args = Environment.GetCommandLineArgs();
            bool.TryParse(GetParam(args, "isLocal=", "true"), out var isLocal);
            // BuildScript.ActiveProfile(isLocal);
            BuildScript.BuildBundles(true);
        }

        public static void BuildPlayer()
        {
            var args = Environment.GetCommandLineArgs();
            //buildParams=产品名@包名
            var buildParams = GetParamSplit(args, "buildParams=", "", '@');
            var version = GetParam(args, "version=", "");
            bool.TryParse(GetParam(args, "isFullRes=", "true"), out var isFullRes);
            bool.TryParse(GetParam(args, "IL2CPP=", "false"), out var IL2CPP);
            bool.TryParse(GetParam(args, "isLocal=", "true"), out var isLocal);

            // BuildScript.ActiveProfile(isLocal);
            BuildScript.BuildSettings(buildParams[0], buildParams[1], version, IL2CPP);
            BuildScript.BuildPlayer(isFullRes);
        }

        static string GetParam(string[] args, string param, string defaultValue)
        {
            string tmpValue = defaultValue;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].IndexOf(param) != -1)
                {
                    tmpValue = args[i].Split('=')[1].Trim();
                    break;
                }
            }
            return string.IsNullOrEmpty(tmpValue) ? defaultValue : tmpValue;
        }

        /// <summary>
        /// 返回带'#'分割注释的参数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static string[] GetParamSplit(string[] args, string param, string defaultValue, char sep = '#')
        {
            var str = GetParam(args, param, defaultValue);
            return str.Split(sep);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Xml;
using System.Text;

public partial class BuildTool
{

    // -----------------------------------------------------渠道宏----------------------------------------
    // 主线
    private const string USING_VR9 = "SVR_VR9";
    private const string USING_SVR = "SVR";
    private const string USING_SVR_LEGACY = "SVR_LEGACY";

    // 印度 AjnaXR
    private const string USING_INDIA_AJNAXR = "SVR_INDIA_AJNAXR";

    // 日本 Insta
    private const string USING_JAPAN_INSTA = "SVR_JAPAN_INSTA";

    // 海外通用版本
    private const string USING_OCCHINA = "SVR_OCCHINA";

    // ------------------------------------------------------------------------------------------------------

    // 渠道数组
    // 注意：每新建一个渠道宏，需要添加该宏到数组
    private static string[] MarcoArray = new string[] { 
        USING_VR9, 
        USING_SVR, 
        USING_SVR_LEGACY, 
        USING_INDIA_AJNAXR,
        USING_JAPAN_INSTA,
        USING_OCCHINA
    };

    /// <summary>
    /// Log宏
    /// </summary>
    private const string DebugKey = "ENABLE_LOG";

    private static string PlatformName = "";


    /// <summary>
    /// Jekins调用的打包接口
    /// </summary>
    static void PerformAndroidBuild()
    {
        bool SwitchActiveBuildTarget = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        bool SwitchActiveBuildTargetAsync = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
        //EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        Debug.Log("SwitchActiveBuildTarget:" + SwitchActiveBuildTarget);
        Debug.Log("SwitchActiveBuildTargetAsync:" + SwitchActiveBuildTargetAsync);
        Debug.Log("androidBuildSystem:" + EditorUserBuildSettings.androidBuildSystem);

        buildTarget(Environment.GetCommandLineArgs());
    }
    private static void buildTarget(params string[] systemparms)
    {
        string java_home = "";
        string ANDROID_SDK = "";
        string apkpath = "";
        string apkName = "";
        string platform = "901";
        int versionCode = 1;
        string commit = "";
        bool development = false;
        string branch = "";
        bool splash = false;
        bool gradle = false;
        string packageName = "";
        string extensionMacro = "";
        int allLength = systemparms.Length;

        string executeMethod = "BuildTool.PerformAndroidBuild";
        //EditorPrefs.SetString("AndroidSdkRoot", androidsdk);
        int startindex = 0;
        for (int i = 0; i < allLength; i++)
        {
            if (systemparms[i] == executeMethod)
            {
                startindex = i;
                break;
            }
        }
        if (startindex < allLength)
        {
            java_home = systemparms[startindex + 1];
            ANDROID_SDK = systemparms[startindex + 2];
            apkpath = systemparms[startindex + 3];
            apkName = systemparms[startindex + 4];
            platform = systemparms[startindex + 5];
            PlatformName = platform;
            versionCode = int.Parse(systemparms[startindex + 6]);
            commit = systemparms[startindex + 7];

            if (!bool.TryParse(systemparms[startindex + 8], out development)) 
            {
                Debug.LogErrorFormat("parms<{0}> not bool", systemparms[startindex + 8]);
            }
            if (startindex + 9 < systemparms.Length)
            {
                branch = systemparms[startindex + 9];
            }
            if (startindex + 10 < systemparms.Length)
            {
                if (!bool.TryParse(systemparms[startindex + 10], out splash))
                {
                    Debug.LogErrorFormat("parms<{0}> not bool", systemparms[startindex + 10]);
                }
            }
            if (startindex + 11 < systemparms.Length)
            {
                if (!bool.TryParse(systemparms[startindex + 11], out gradle))
                {
                    Debug.LogErrorFormat("parms<{0}> not bool", systemparms[startindex + 11]);
                }
            }

            if (startindex + 12 < systemparms.Length)
            {
                packageName = systemparms[startindex + 12];
            }

            if (startindex + 13 < systemparms.Length)
            {
                extensionMacro = systemparms[startindex + 13];
            }

            Debug.Log("usgradle:" + gradle);
            if (!string.IsNullOrEmpty(ANDROID_SDK))
                EditorPrefs.SetString("AndroidSdkRoot", ANDROID_SDK);
            if (!string.IsNullOrEmpty(java_home))
                EditorPrefs.SetString("JdkPath", java_home);
            Debug.Log("java_Home:" + java_home);
            Debug.Log("ANDROID_SDK:" + ANDROID_SDK);
            Debug.Log("apkpath:" + apkpath);
            Debug.Log("platform:" + platform);
            Debug.Log("commit:" + commit);
            Debug.Log("development:" + development);
            Debug.Log("splash:" + splash);
            Debug.Log("packageName:" + packageName);
            Debug.Log("extensionMacro:" + extensionMacro);
#if UNITY_2020_1_OR_NEWER
            EditorPrefs.SetBool("AndroidGradleStopDaemonsOnExit", false);
#else
            if (gradle) 
            {
                EditorPrefs.SetBool("AndroidGradleStopDaemonsOnExit", false);
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }
            else
            {
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
            }
#endif

        }
        //GvrBuildProcessor.batchmode = true;
        apkpath = apkpath + "/" + platform;
        if (!Directory.Exists(apkpath))
            Directory.CreateDirectory(apkpath);
        apkpath = apkpath + "/" + apkName + "_" + branch + "_" + commit + ".apk";
        if (File.Exists(apkpath))
            File.Delete(apkpath);

        /*switch (platform)
        {
            
               


        }*/
        PlayerSettings.Android.blitType = AndroidBlitType.Always;

        // 根据拓展字段设置拓展宏
        SetDefineBaseOnExtensionMacro(extensionMacro);

        // 设置PackageName
        if (!string.IsNullOrEmpty(packageName))
        {
            PlayerSettings.applicationIdentifier = packageName;
        }
        Debug.Log("PackageName:" + PlayerSettings.applicationIdentifier);


        var scenes = getScenes(platform);

        PlayerSettings.Android.useCustomKeystore = true;
        // if (PlayerSettings.applicationIdentifier.ToLower() == "com.ssnwt.newskyui")
        // {
        //     setKeystor(Application.dataPath + "/Plugins/Android/android.keystore", "android.keystore", "wubuandroid123");
        // }
        // else
        // {
        //     setKeystor(Application.dataPath + "/CommonLib/BuildTools/signkeystore/ssnwt.keystore", "ssnwt", "anndroid_ssnwt");
        // }
       /* setKeystore(Application.dataPath + "/CommonLib/BuildTools/signkeystore/ssnwt.keystore", "ssnwt", "anndroid_ssnwt");
        if (!string.IsNullOrEmpty(branch))
        {
            if (branch.ToLower().Contains("release"))
                DisableLog();
            else
                EnableLog();

            Debug.Log("LogUtile:" + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android));
        }*/
        buildApk(apkpath, versionCode, commit, development, scenes);
    }

    /// <summary>
    /// 根据拓展字段宏设置Unity的Define
    /// </summary>
    private static void SetDefineBaseOnExtensionMacro(string define)
    {
        if (!string.IsNullOrEmpty(define))
        {
            Debug.Log("添加拓展宏:"+ define);
            DefineSymbolUtil.Add(define);
        }
            
    }

    private static void setKeystore(string keyStorePath, string key, string password)
    {
        //string keysotrepath = path;// Application.dataPath + "/Plugins/Android/android.keystore";
        //string key =  "android.keystore";
        //string password =  "wubuandroid123";
        if (!File.Exists(keyStorePath))
        {
            throw new UnityException("The signature file does not exist in path = " + keyStorePath);
        }
        PlayerSettings.Android.keystoreName = keyStorePath;
        PlayerSettings.Android.keystorePass = password;
        PlayerSettings.Android.keyaliasName = key;
        PlayerSettings.Android.keyaliasPass = password;
    }
    private static void setSplash(bool isVR9,bool splash)
    {
        PlayerSettings.SplashScreen.show = splash;
        string configpath = Application.dataPath + "/Plugins/Android/assets/splash.cfg";
        if (!Directory.Exists(Application.dataPath + "/Plugins")) Directory.CreateDirectory(Application.dataPath + "/Plugins");
        if (!Directory.Exists(Application.dataPath + "/Plugins/Android")) Directory.CreateDirectory(Application.dataPath + "/Plugins/Android");
        if (!Directory.Exists(Application.dataPath + "/Plugins/Android/assets")) Directory.CreateDirectory(Application.dataPath + "/Plugins/Android/assets");
        if (File.Exists(configpath))
        {
            File.Delete(configpath);
        }
        StreamWriter sw = File.CreateText(configpath);
        if (PlayerSettings.SplashScreen.show)
        {
            sw.WriteLine("UNITY_USE_SPLASH=1");
        }
        else
        {
            sw.WriteLine("UNITY_USE_SPLASH=0");
        }
        if(splash)
            sw.WriteLine("USE_SVR_SPLASH=0");
        else
            sw.WriteLine("USE_SVR_SPLASH=1");
        sw.Close();
    }

    public static void SetMarco(params string[] defines)
    {
        DefineSymbolUtil.Remove(MarcoArray);
        DefineSymbolUtil.Add(defines);
    }

    public static void SetMarcoSVR()
    {
        DefineSymbolUtil.Remove(MarcoArray);
        DefineSymbolUtil.Add(USING_SVR);
    }

    public static string GetPlatformName()
    {
        return PlatformName;
    }

    private static string GetSourceLocalizationResource()
    {
        if (!string.IsNullOrEmpty(PlatformName))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Application.streamingAssetsPath);
            string rootPath = dirInfo.Parent.Parent.FullName;
            rootPath = Path.Combine(rootPath, "Localization_Channel");
            string sourcePath = Path.Combine(rootPath, PlatformName);
            sourcePath = Path.Combine(sourcePath, "Local");


            return sourcePath;
        } 
        else
        {
            Debug.Log("PlatformName为空");
            return "";
        }

    }


    static void EnableLog()
    {
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        bool found = false;
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

        string[] macrosList = macros.Split(';');
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] == DebugKey)
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.Log("EnableLog:" + macros + ";" + DebugKey);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, macros + ";" + DebugKey);
        }
    }
    static void DisableLog()
    {

        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        bool found = false;
        string macros = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] macrosList = macros.Split(';');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < macrosList.Length; i++)
        {
            if (macrosList[i] != DebugKey)
            {
                sb.Append(macrosList[i]);
                if (i < macrosList.Length - 1)
                    sb.Append(";");
            }
            else
            {
                found = true;
            }
        }
        if (found)
        {
            Debug.Log("DisableLog:" + sb.ToString());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, sb.ToString());

        }
    }
    static List<string> getScenes(string platform)
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            //if (scene.enabled)
            //{
            if (platform == "mobile")
            {
                if (scene.path.Contains("801") || scene.path.Contains("901"))
                {
                    if (scene.path.Contains("901"))
                        scenes.Add(scene.path);
                }
                else
                {
                    if (scene.enabled)
                        scenes.Add(scene.path);
                }
            }
            else if (platform == "India_AjnaXR" || platform == "Japan_Insta")
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            else
            {
                if (scene.path.Contains("801") || scene.path.Contains("901"))
                {
                    if (scene.path.Contains(platform))
                        scenes.Add(scene.path);
                }
                else
                {
                    if (scene.enabled)
                        scenes.Add(scene.path);
                }
            }
            //}

        }
        return scenes;
    }
    static void buildApk(string path, int versioncode, string versionName, bool development, List<string> scenes)
    {
        GC.Collect();
        if (scenes == null || scenes.Count == 0)
        {
            throw new UnityException("错误:未添加场景");
        }
        else
        {
            foreach (var item in scenes)
            {
                Debug.Log("Scenes:" + item);
            }
        }
        AssetDatabase.Refresh();

        BuildOptions buildOptions = BuildOptions.None;

        if (development)
        {
            buildOptions |= BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        }

        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.Android.bundleVersionCode = versioncode;
        PlayerSettings.bundleVersion = versionName;

        AssetDatabase.SaveAssets();
        Debug.Log("signapkDirectory:" + path);
        var result = BuildPipeline.BuildPlayer(scenes.ToArray(), path, BuildTarget.Android, buildOptions);

        Debug.Log("Build Result : " + result.summary.result);
        if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.Log("Errors " + result.summary.totalErrors);
            foreach (var item in result.steps)
            {
                Debug.LogFormat("{0}:{1}", item.name, item.messages);
                foreach (var item_msg in item.messages)
                {
                    if (item_msg.type == LogType.Warning || item_msg.type == LogType.Log) continue;
                    Debug.LogFormat("log:{0},content:{1}", item_msg.type, item_msg.content);
                }
            }
        }

    }

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;

            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    /// <summary>
    /// 此方法是从jienkins上接受  数据的 方法
    /// </summary>
    static void CommandLineBuild()
    {
        try
        {

            Debug.Log("Command line build\n------------------\n------------------");
            string[] scenes = GetBuildScenes();
            //string path = @"E:\Unity游戏包\Android\消消乐游戏";//这里的路径是打包的路径， 定义
            string path = GetJenkinsParameter("BuildPath");
            Debug.Log(path);
            for (int i = 0; i < scenes.Length; ++i)
            {
                Debug.Log(string.Format("Scene[{0}]: \"{1}\"", i, scenes[i]));
            }
            // ProjectPackageEditor.BuildByJenkins(GetJenkinsParameter("Platform"), GetJenkinsParameter("AppID"), GetJenkinsParameter("Version"), GetJenkinsParameter("IPAddress"));
            Debug.Log("Starting Build!");
            Debug.Log(GetJenkinsParameter("Platform"));

            string platform = GetJenkinsParameter("Platform");
            if (platform == "Android")
            {
                BuildPipeline.BuildPlayer(scenes, path + ".apk", BuildTarget.Android, BuildOptions.None);
            }
            else if (platform == "IOS")
            {
                //BuildPipeline.BuildPlayer(scenes, path, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);
            }
            else if (platform == "Window64")
            {
                BuildPipeline.BuildPlayer(scenes, path + ".exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            }
        }
        catch (Exception err)
        {
            Console.WriteLine("方法F中捕捉到：" + err.Message);
            throw;//重新抛出当前正在由catch块处理的异常err
        }
        finally
        {
            Debug.Log("---------->  I am copying!   <--------------");
        }
    }


    /// <summary>
    /// 获取jenkins传参
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    static string GetJenkinsParameter(string name)
    {
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            Debug.Log("获取参数:"+arg);
            if (arg.StartsWith(name))
            {
                return arg.Split("-"[0])[1];
            }
        }
        return null;
    }
}

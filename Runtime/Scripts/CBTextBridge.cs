using System;
using UnityEngine;

using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace ClashBuddy.Plugins.Native
{
    [Serializable]
    public class CBTextResponse
    {
        public string action;
        public string body;
    }


    public class CBTextBridge
    {
        private static AndroidJavaClass unityClass;
        private static AndroidJavaObject unityActivity;
        private static AndroidJavaObject pluginInstance;
        private static CBTextBridge _instance;


        // Singleton accessor
        public static CBTextBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CBTextBridge();
                    if (Application.platform == RuntimePlatform.Android)
                        _instance.Init();
                }
                return _instance;
            }
        }

        private CBTextBridge() { }



        public void Init()
        {
            try
            {
                unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                pluginInstance = new AndroidJavaObject("studio.clashbuddy.natives.cbtext.CBText");
                if (pluginInstance == null)
                    Debug.Log("Error: plugin is not available");
                pluginInstance.CallStatic("initActivity", unityActivity);
                Debug.Log("Done: everthing is ok");
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);

            }
        }



        public string ConvertIntoText(
            string message, int width, int fontSize, string color, bool isSingleLine = true,
            bool isEllipsize = false, int lineEllipse = 1, bool isCenter = false,
            string folderName = null, string fontName = "default",
            int stroke = 0, string strokeColor = "#000000",
            int shadowX = 0, int shadowY = 0, int shadowR = 0,
            string shadowColor = "#000000", float letterSpacing = 0f, float lineSpacing = 0f, Action<string> onError = null)
        {
            onError ??= (msg) => Debug.LogWarning("Default error: " + msg);
            if (Application.platform == RuntimePlatform.Android)
            {
                var response = pluginInstance?.CallStatic<string>(
                    "renderTextAsImage", message, width, fontSize, color, isSingleLine,
                    isEllipsize, lineEllipse, isCenter, fontName, folderName,
                    stroke, strokeColor, shadowX, shadowY, shadowR, shadowColor, letterSpacing, lineSpacing);
                var data = JsonUtility.FromJson<CBTextResponse>(response);
                if (data.action == "ERROR")
                {
                    Debug.LogError(data.body);
                    onError(data.body);


                    return null;
                }

                return data.body;
            }
            onError("Not supported");
            return null;
        }




    }
}
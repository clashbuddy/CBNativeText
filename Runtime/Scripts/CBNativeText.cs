using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ClashBuddy.Plugins.Native
{





    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CBNativeText : MonoBehaviour
    {
        private TextMeshProUGUI tmpText;

        private RawImage rawImage;



        public string fontName = "default";
        public bool useCBFontNative = false;
        public bool useSameFontTMP = false;


        [Header("Text Native Shadow")]
        public Color shadowColor = Color.black;
        public int shadowX = 0;
        public int shadowY = 0;
        public int shadowRadius = 1;


        [Header("Text Native Stroke")]
        public Color strokeColor = Color.black;
        public int strokeSize = 0;
        private string fontfolderName;


        void Awake()
        {


            tmpText = GetComponent<TextMeshProUGUI>();

            if (useCBFontNative)
                fontfolderName = "fonts";
            if (useSameFontTMP)
                fontName = tmpText.font.name.Split(" ")[0];

            Debug.Log(tmpText.text);
        }



        void Start()
        {

            CreateRawImageOverlay();
            SetText(tmpText.text);
        }

        private void ConvertIntoNative(string text)
        {

            if (!rawImage.IsActive()) return;
            int fontSize = (int)tmpText.fontSize;
            string textColor = ConvertUnityColorToAndroidHex(tmpText.color);
            string strokeColor = ConvertUnityColorToAndroidHex(this.strokeColor);
            string shadowColor = ConvertUnityColorToAndroidHex(this.shadowColor);

            bool isCenter = tmpText.horizontalAlignment == HorizontalAlignmentOptions.Center;
            bool isSingleLine = tmpText.textWrappingMode == TextWrappingModes.NoWrap;

            bool isEllipsize = tmpText.overflowMode == TextOverflowModes.Ellipsis;

            float lineSpacing = tmpText.lineSpacing;
            float letterSpacing = tmpText.characterSpacing;

            int shadowX = this.shadowX, shadowY = this.shadowY, shadowR = this.shadowRadius, strokeSize = this.strokeSize;
            int boxWidth = (int)rawImage.rectTransform.rect.width;
            if (Application.platform == RuntimePlatform.Android)
            {
                var imageText = CBTextBridge.Instance.ConvertIntoText(text, boxWidth, fontSize, textColor,
                isSingleLine, isEllipsize, 1, isCenter, fontfolderName, fontName,
                strokeSize, strokeColor, shadowX, shadowY, shadowR, shadowColor, letterSpacing, lineSpacing, (error) =>
                {
                    rawImage.gameObject.SetActive(false);
                    tmpText.gameObject.SetActive(true);
                    tmpText.color = Color.red;
                    tmpText.fontSize = 30;
                    tmpText.SetText(error);

                });
                OnTextImageReceived(imageText);
            }

        }

        public void OnTextImageReceived(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            Texture2D texture = new(2, 2);
            texture.LoadImage(imageBytes);
            rawImage.texture = texture;
            rawImage.color = Color.white;
            rawImage.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
        }

        public static string ConvertUnityColorToAndroidHex(Color color)
        {
            Color32 c = color;
            return $"#{c.a:X2}{c.r:X2}{c.g:X2}{c.b:X2}";
        }


        public void SetText(string text)
        {
            ConvertIntoNative(text);
        }

        private void CreateRawImageOverlay()
        {

            GameObject rawGO = new(tmpText.name);
            rawGO.transform.SetParent(transform.parent, false);

            rawImage = rawGO.AddComponent<RawImage>();

            // Match TMP transform
            RectTransform tmpRT = GetComponent<RectTransform>();
            RectTransform rawRT = rawGO.GetComponent<RectTransform>();

            rawRT.anchorMin = tmpRT.anchorMin;
            rawRT.anchorMax = tmpRT.anchorMax;
            rawRT.pivot = tmpRT.pivot;
            rawRT.anchoredPosition = tmpRT.anchoredPosition;
            rawRT.sizeDelta = tmpRT.sizeDelta;
            rawRT.localScale = tmpRT.localScale;


#if UNITY_ANDROID && !UNITY_EDITOR
                            tmpText.gameObject.SetActive(false);
                            rawImage.gameObject.SetActive(true);
#else
            rawImage.gameObject.SetActive(false);
#endif
        }


    }

}
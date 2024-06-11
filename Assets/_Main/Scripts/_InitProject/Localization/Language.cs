using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_WEBGL
using YG;
#endif

namespace Localization
{
    public class Language : MonoBehaviour
    {
        public enum Languages
        {
            En, Ru, Tr
        }

        private static string lang;
        public static string Lang => lang;


        private void Start()
        {
            lang = "en";

#if UNITY_WEBGL
        SetLanguageWebGL();
#endif

#if UNITY_ANDROID || UNITY_IOS
            SetLanguageMobile();
#endif
            Debug.Log("Localization set to: " + lang);
        }

#if UNITY_WEBGL
        private void SetLanguageWebGL()
        {
            lang = YandexGame.EnvironmentData.language;

        }
#endif


#if UNITY_ANDROID || UNITY_IOS
        private void SetLanguageMobile()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    lang = "en";
                    break;
                case SystemLanguage.Turkish:
                    lang = "tr";
                    break;
                case SystemLanguage.Russian:
                    lang = "ru";
                    break;
                case SystemLanguage.Belarusian:
                    lang = "ru";
                    break;
                default:
                    lang = "en";
                    break;
            }
        }
#endif
    }
}

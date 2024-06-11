using NaughtyAttributes;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core {
    public class AppStart : MonoBehaviour {
        [Foldout("Splash Screen"),
            SerializeField] private float _splashScreenDurationTime = 1f;
        [Foldout("Splash Screen"), SerializeField] private Image _logo;
        [Foldout("Splash Screen")]
        public bool SkipSplashScreenAnim = true;

        [Foldout("Scene to Load"),SerializeField] private string _sceneNameToLoad;
        [Foldout("Scene to Load"),SerializeField] private SceneTypes _sceneTypeToLoad = SceneTypes.MainMenu;

        private IEnumerator Start() {
#if !UNITY_EDITOR
            SkipSplashScreenAnim = false;
#endif

            BeforeSplashScreenAppear();

            if (SkipSplashScreenAnim) {
                LoadStartScene();
                yield break;
            }

            yield return null;

            _logo.DOFillAmount(1f, _splashScreenDurationTime * 0.95f);
            DOVirtual.DelayedCall(_splashScreenDurationTime, LoadStartScene);
        }

        private void LoadStartScene() {
            SceneSystem.Instance.LoadSceneSwitch(_sceneNameToLoad, _sceneTypeToLoad, LoadScreens.FadeInOut);
        }

        private void BeforeSplashScreenAppear() {
            VersionValidator versionValidator = new VersionValidator(SaveSystem.Instance.Data);
			VibrationsManager.Create();

            // Refresh vSync and target FPS to avoid how target FPS.
            // note that in project shouldn't be more vSync and target FPS setup,
            // or you may overrade two lines below
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;

#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            DOTween.logBehaviour = LogBehaviour.Default;
#else
            Debug.unityLogger.logEnabled = false;
            DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
#endif

#if UNITY_ANDROID
            // uncomment to use notifications
            //NotificationManager.Instance.Init();
#endif
        }
    }
}
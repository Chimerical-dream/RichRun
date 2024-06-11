using Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Game.Core {

    /// <summary>
    /// Main class to handle Scene switch/load operations
    /// </summary>
    public class SceneSystem : MonoBehaviour {
        #region Singleton
        private static SceneSystem _instance;
        public static SceneSystem Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<SceneSystem>();
                    if (_instance == null) {
                        GameObject go = new GameObject("SceneSystem");
                        _instance = go.AddComponent<SceneSystem>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        private SceneTypes _currentSceneType;
        /// <summary>
        /// Scene type that currently loaded
        /// </summary>
        public SceneTypes CurrentSceneType => _currentSceneType;

        /// <summary>
        /// Scene that is last loaded
        /// </summary>
        public static Scene ActiveScene;
        /// <summary>
        /// Last scene that was before <see cref="ActiveScene"/>
        /// </summary>
        public static Scene PreviousScene;

        #region Scene Contraints
        // you can add here scenes string names as in build settings
        // to pass in Load(AsyncLoad)SceneSwitch()
        public const string MAIN_MENU_SCENE = "MainMenu";
        public const string GAME_SCENE = "GameScene";
        public const string LOADER_SCENE = "_Loader";
        #endregion

        public bool VerboseLogs;

        /// <summary>
        /// UnityAction, invokes when scene is full ready to be used <br>
        /// You can use it to enable AI or activate play timer and etc.</br> <br>
        /// If loadScreen was - event executed on start of loadScreen disappear. </br> <br>
        /// In all, this event invokes only when <see cref="Initialization"/> completed (if no initialization running - instanly as soon as scene loads)</br>
        /// </summary>
        [HideInInspector]
        public static UnityEvent OnSceneFullReady = new UnityEvent();
        /// <summary>
        /// arg1 - scene
        /// </summary>
        public static UnityEvent<Scene> OnSceneUnloaded = new UnityEvent<Scene>();
        /// <summary>
        /// invokes before scene start loading
        /// </summary>
        public static UnityEvent OnSceneStartLoading = new UnityEvent();

        private Coroutine _sceneLoading;
        private Coroutine _scriptsInitialization;
        private List<AsyncOperation> _scenesLoad = new List<AsyncOperation>();

        private LoadScreen _hideLoadScreenTransition;

        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            SceneManager.sceneLoaded += OnManagerSceneLoaded;
            SceneManager.sceneUnloaded += (scene) => OnSceneUnloaded?.Invoke(scene);
			
			ActiveScene = SceneManager.GetActiveScene();

            DontDestroyOnLoad(gameObject);

            if (ActiveScene.name == LOADER_SCENE)
            {
                DG.Tweening.DOVirtual.DelayedCall(.5f, () => AsyncLoadSceneSwitch(GAME_SCENE, SceneTypes.LevelScene));
            }
        }

        private void Start() {
            if (ActiveScene.name != LOADER_SCENE) {
                CoroutineHelper.WaitForAction(InvokeSceneFullReadyEvent, 0.5f);
            }
        }

        /// <summary>
        /// Switch scene not-async immediately
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadSceneSwitch(string sceneName, SceneTypes sceneType) {
            StartSceneLoad(sceneName, sceneType, async: false);
        }

        /// <summary>
        /// Switch scene not-async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreen) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreen, () => { StartSceneLoad(sceneName, sceneType, async: false); });
            _hideLoadScreenTransition = screen;
        }

        /// <summary>
        /// Switch scene not-async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreenIn = LoadScreens.None, LoadScreens loadScreenOut = LoadScreens.None) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: false); });
            _hideLoadScreenTransition = LoadScreenManager.CreateLoadScreen(loadScreenOut);
        }

        /// <summary>
        /// Switch scene not-async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreenIn, LoadScreen loadScreenOut) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: false); });
            _hideLoadScreenTransition = loadScreenOut;
        }

        /// <summary>
        /// Switch scene not-async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreen loadScreenIn, LoadScreen loadScreenOut = null) {
            LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: false); });
            _hideLoadScreenTransition = loadScreenOut ? loadScreenOut : loadScreenIn;
        }

        /// <summary>
        /// Switch scene async immediately
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void AsyncLoadSceneSwitch(string sceneName, SceneTypes sceneType) {
            StartSceneLoad(sceneName, sceneType, async: true);
        }

        /// <summary>
        /// Switch scene async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void AsyncLoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreen) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreen, () => { StartSceneLoad(sceneName, sceneType, async: true); });
            _hideLoadScreenTransition = screen;
        }

        /// <summary>
        /// Switch scene async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void AsyncLoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreenIn, LoadScreens loadScreenOut) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: true); });
            _hideLoadScreenTransition = LoadScreenManager.CreateLoadScreen(loadScreenOut);
        }

        /// <summary>
        /// Switch scene async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void AsyncLoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreens loadScreenIn, LoadScreen loadScreenOut) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: true); });
            _hideLoadScreenTransition = loadScreenOut;
        }

        /// <summary>
        /// Switch scene async with LoadScreen
        /// </summary>
        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void AsyncLoadSceneSwitch(string sceneName, SceneTypes sceneType, LoadScreen loadScreenIn, LoadScreen loadScreenOut = null) {
            LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, sceneType, async: true); });
            _hideLoadScreenTransition = loadScreenOut ? loadScreenOut : loadScreenIn;
        }

        /// <summary>
        /// Reload active scene immediately
        /// </summary>
        public void ReloadActiveScene(bool async = true) {
            if (async) {
                AsyncLoadSceneSwitch(ActiveScene.name, _currentSceneType);
            }
            else {
                LoadSceneSwitch(ActiveScene.name, _currentSceneType);
            }
        }

        /// <summary>
        /// Reload active scene with LoadScreen
        /// </summary>
        public void ReloadActiveScene(LoadScreens loadScreenType, bool async = true) {
            if (async) {
                AsyncLoadSceneSwitch(ActiveScene.name, _currentSceneType, loadScreenType);
            }
            else {
                LoadSceneSwitch(ActiveScene.name, _currentSceneType, loadScreenType);
            }
        }

        /// <summary>
        /// when scene is ready to be used
        /// </summary>
        private void InvokeSceneFullReadyEvent() {
            OnSceneFullReady.Invoke();
        }

        private void OnManagerSceneLoaded(Scene loadedScene, LoadSceneMode loadMode) {
            PreviousScene = ActiveScene;
            ActiveScene = loadedScene;
        }

        private void StartSceneLoad(string sceneName, SceneTypes loadSceneType, bool async) {
            if (_sceneLoading != null || _scriptsInitialization != null) {
                if (VerboseLogs) {
                    Debug.LogError("Scene is already loading or scripts is not initialized yet!");
                }
                return;
            }

            OnSceneStartLoading.Invoke();
            if (loadSceneType == SceneTypes.LevelScene) {
                GlobalEventsManager.StartLevelSceneLoading();
            }

            if (_currentSceneType == SceneTypes.LevelScene) {
                if (loadSceneType == SceneTypes.MainMenu) {
                    GlobalEventsManager.ReturnMainManuFromLevel();
                }
            }

            if (VerboseLogs) print($"Начинаю загрузку {sceneName}!");

            if (async) {
                _scenesLoad.Add(SceneManager.LoadSceneAsync(sceneName));
            }
            else {
                SceneManager.LoadScene(sceneName);
            }

            _sceneLoading = StartCoroutine(SceneLoadProgress(loadSceneType));
        }

        /// <summary>
        /// progress [0..1]
        /// </summary>
        float _totalSceneLoadProgress;
        float _totalInitialisationProgress;
        private IEnumerator SceneLoadProgress(SceneTypes targetSceneType) {
            for (int i = 0; i < _scenesLoad.Count; i++) {
                while (!_scenesLoad[i].isDone) {
                    _totalSceneLoadProgress = 0f;

                    foreach (AsyncOperation operation in _scenesLoad) {
                        _totalSceneLoadProgress += operation.progress;
                    }

                    _totalSceneLoadProgress /= _scenesLoad.Count;
                    if (VerboseLogs) print($"прогресс сцены: {_totalSceneLoadProgress}!");
                    yield return null;
                }
            }

            _totalSceneLoadProgress = 1f;
            _currentSceneType = targetSceneType;

            // as soon as scene is loaded - start loading scripts
            _scriptsInitialization = StartCoroutine(GetTotalProgress());

            _scenesLoad.Clear();
            _sceneLoading = null;
        }

        private IEnumerator GetTotalProgress() {
            // wait 1 frame to ensure that Initialization object is created
            yield return null;

            float totalProgress;
            LoadScreen loadScreen = null;
            if (LoadScreenManager.IsActive) {
                loadScreen = LoadScreenManager.GetActiveScreen();
            }

            if (Initialization.current) {
                while (!Initialization.current.IsDone) {
                    _totalInitialisationProgress = Initialization.current.GetInitProgress();

                    // [0..1]
                    totalProgress = (_totalInitialisationProgress + _totalSceneLoadProgress) / 2f;
                    if (loadScreen != null) loadScreen.OnUpdateLoadProgress(totalProgress);
                    yield return null;
                }
            }

            totalProgress = 1f;
            if (loadScreen != null) {
                loadScreen.OnUpdateLoadProgress(totalProgress);
                LoadScreenManager.HideLoadScreen(_hideLoadScreenTransition);
            }
            else if (_hideLoadScreenTransition != null) {
                Destroy(_hideLoadScreenTransition);
                _hideLoadScreenTransition = null;
            }

            _scriptsInitialization = null;
            InvokeSceneFullReadyEvent();
        }
    }
}
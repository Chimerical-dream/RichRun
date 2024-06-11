using Game.Audio;
using Game.UI;
using UnityEngine;

namespace Game.Core {
    public class SceneSetupper : MonoBehaviour {
        private void Awake() {
#if UNITY_EDITOR
            VibroBtn.Editor_ClearEvents(); // avoid multiple events if "Editor Playmode options" enabled in Player Settings

            if (PoolManager.instance == null) {
                var poolManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("PoolSystem l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = Instantiate(poolManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            if (UIManager.instance == null) {
                var UIManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("UI l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = Instantiate(UIManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            if (AudioManager.IsCreated == false) {
                var audioManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("AudioManager l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = Instantiate(audioManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            if (LoadScreenManager.instance == null) {
                var loadScreenManager = EditorTools.EditorAssetManager.LoadAsset<GameObject>("LoaderScreens l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = Instantiate(loadScreenManager);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            var sceneSystem = SceneSystem.Instance; // will create scene system if not exists yet
#endif

            PoolManager.instance.OnSceneChanged();
            UIManager.instance.OnSceneChanged();

            OnSceneSetup();
        }

        protected T FindComponent<T>() where T : Object {
            int childCount = transform.childCount;
            T component;
            for (int i = 0; i < childCount; i++) {
                component = transform.GetChild(i).GetComponent<T>();
                if (component != null) {
                    return component;
                }
            }
            return null;
        }

        protected virtual void OnSceneSetup() {
            var sceneUI = FindComponent<SceneUIContainer>();
            if (sceneUI) {
                sceneUI.Setup();
            }
        }
    }
}
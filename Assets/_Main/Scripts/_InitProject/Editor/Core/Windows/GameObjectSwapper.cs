using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Game.EditorTools {
    public class GameObjectSwapper : EditorWindow {
        public GameObject[] _objectsToSwap;
        public GameObject _holder;
        public GameObject[] _swapToObjects;

        private List<GameObject> _tempNonPrefabClones = new List<GameObject>();

        public bool _swapChildren;

        SerializedObject so;
        SerializedProperty objectsToSwapProperty = null;
        SerializedProperty holderProperty = null;
        SerializedProperty toObjectsProperty = null;
        SerializedProperty swapSelectedAllChildren = null;

        [MenuItem("Tools/Windows/Game Objects Swapper")]
        public static void ShowWindow() {
            var window = GetWindow<GameObjectSwapper>();
            window.titleContent = new GUIContent("Game Objects Swapper");
            window.Init();
        }

        public void Init() {
            so = new SerializedObject(this);
            objectsToSwapProperty = so.FindProperty(nameof(_objectsToSwap));
            toObjectsProperty = so.FindProperty(nameof(_swapToObjects));
            swapSelectedAllChildren = so.FindProperty(nameof(_swapChildren));
            holderProperty = so.FindProperty(nameof(_holder));
        }

        public void OnGUI() {
            EditorGUILayout.BeginVertical();
            EditorUtils.Field(swapSelectedAllChildren, "Swap children");
            if (_swapChildren) {
                EditorUtils.Field(holderProperty, "Children holder object");
            }
            else {
                EditorUtils.ArrayField(objectsToSwapProperty, "Swap objects");
            }

            EditorUtils.ArrayField(toObjectsProperty, "To objects");

            // draw button
            if (GUILayout.Button(new GUIContent("Perform Swap"))) {
                Swap();
                so.Update();
            }
            EditorGUILayout.EndVertical();

            so.ApplyModifiedProperties(); // Remember to apply modified properties
        }

        public void Swap() {
            if (_swapToObjects == null || _swapToObjects.Length == 0) {
                Debug.LogError($"Error! swapToObject is null!");
                return;
            }

            foreach(var obj in _swapToObjects) {
                if (obj == null) {
                    Debug.LogError($"Error! some swapToObject values are null!");
                    return;
                }
            }

            if (_objectsToSwap != null && _objectsToSwap.Length > 0) {
                var objectsToSwapListCheck = new List<GameObject>();
                foreach (var obj in _objectsToSwap) {
                    if (obj != null) {
                        objectsToSwapListCheck.Add(obj);
                    }
                }
                _objectsToSwap = objectsToSwapListCheck.ToArray();
            }

            if (_swapChildren) {
                if (_holder == null) {
                    Debug.LogError("Children holder object is null!");
                    return;
                }
				
                SwapObjects(GetChildObjectsToSwap(_holder.transform));
            }
            else {
                SwapObjects(_objectsToSwap);
            }
        }

        private void SwapObjects(GameObject[] oldObjects) {

            // dublicate non prefab objects
            for (int i = 0; i < _swapToObjects.Length; i++) {
                if (IsPrefab(_swapToObjects[i]) == false) {
                    var clone = Instantiate(_swapToObjects[i]);
                    _tempNonPrefabClones.Add(clone);
                    _swapToObjects[i] = clone;
                }
            }

            // swap main logic
            int objsCount = oldObjects.Length;
            for (int i = 0; i < objsCount; i++) {
                GameObject newObject = _swapToObjects[Random.Range(0, _swapToObjects.Length)];
                GameObject instance;

                if (IsPrefab(newObject)) {
                    instance = PrefabUtility.InstantiatePrefab(newObject) as GameObject;
                }
                else {
                    instance = Instantiate(newObject);
                }
                instance.name = instance.name.Replace("(Clone)", "");

                GameObject objectToSwap = oldObjects[i];


                instance.transform.SetParent(objectToSwap.transform.parent);
                instance.transform.localPosition = objectToSwap.transform.localPosition;
                instance.transform.localRotation = objectToSwap.transform.localRotation;
                instance.transform.localScale = objectToSwap.transform.localScale;

                oldObjects[i] = instance;

                DestroyImmediate(objectToSwap);
            }

            // remove non prefab clones
            if (_tempNonPrefabClones.Count > 0) {
                for (int i = 0; i < _tempNonPrefabClones.Count; i++) {
                    DestroyImmediate(_tempNonPrefabClones[i]);
                }
                _tempNonPrefabClones.Clear();
            }

            // refresh swap objects list
            _objectsToSwap = oldObjects;


            // save changes
            if (_objectsToSwap.Length > 0) {
                var prefabIsolationState = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(_objectsToSwap[0]);
                if (prefabIsolationState != null) {
                    EditorUtility.SetDirty(prefabIsolationState.prefabContentsRoot);
                }
                else {
                    EditorUtility.SetDirty(_objectsToSwap[0]);
                }
            }

            // refresh to objects array (remove missing objects)
            List<GameObject> toObjectsNewList = new List<GameObject>(_swapToObjects.Length);
            for (int i = 0; i < _swapToObjects.Length; i++) {
                if (_swapToObjects[i] != null) {
                    toObjectsNewList.Add(_swapToObjects[i]);
                }
            }
            _swapToObjects = toObjectsNewList.ToArray();
        }

        private GameObject[] GetChildObjectsToSwap(Transform holder) {
            GameObject[] objectsToSwap = new GameObject[holder.childCount];
            for (int i = 0; i < holder.childCount; i++) {
                objectsToSwap[i] = holder.GetChild(i).gameObject;
            }
            return objectsToSwap;
        }

        private bool IsPrefab(GameObject go) {
            return go.scene.name == null;
        }
    }
}

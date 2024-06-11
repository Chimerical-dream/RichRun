using UnityEngine;
using System.IO;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Game.Core {
    // this system takes some parts from BayatGames.SaveGameFree package: 
    // https://assetstore.unity.com/packages/tools/input-management/save-game-free-gold-update-81519

    /// <summary>
    /// Main class for game save/load operations
    /// </summary>
    public class SaveSystem : MonoBehaviour {
        #region SINGLETON PATTERN    
        private static SaveSystem _instance;
        public static SaveSystem Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<SaveSystem>();

                    if (_instance == null) {
                        GameObject container = new GameObject("SaveSystem");
                        _instance = container.AddComponent<SaveSystem>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// arg1 - data before save, you can add or override information before it will be saved
        /// </summary>
        public static UnityEvent<SaveData> OnDataSave = new UnityEvent<SaveData>();

        [SerializeField]
        private bool _verboseLogging = false;

        private static ISaveSerializer _saveSerializer = new SaveBinarySerializer();

        [InfoBox("Don't override default variables in inspector,\n" +
            "you need to override them in script.\n" +
            "You can override values in inspector for test purposes runtime."),
            SerializeField]
        private SaveData _data;

        /// <summary>
        /// Core object of saveSystem that you will refer to.<br>
        /// By refering this property you can read/change data. <br>
        /// </br>Data will be saved automaticaly on OnApplicationFocus() where focus lost</br>
        /// </summary>
        public SaveData Data => _data;

        public static string PathToFiles => Application.persistentDataPath;
        public static readonly string SaveDataBackupFolderName = "backup gdata ";

        /// <summary>
        /// Save data file name in <see cref="PathToFiles"/>
        /// </summary>
        public static readonly string FileName = "gameData.sqw";

        /// <summary>
        /// temp path with save filename combined to load/save/delete data
        /// </summary>
        private string _savePath = null;

        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            LoadData();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// You should not do it manualy, only if you know what you do (game loads automaticaly as soon as <see cref="Data"/> called once)
        /// </summary>
        public void LoadData() {
            _savePath = Path.Combine(PathToFiles, FileName);

            if (_verboseLogging) {
                print("save path: " + _savePath);
            }

            if (!File.Exists(_savePath))   //means there is no saves -> new game
            {
                if (_verboseLogging) {
                    print("New save was created! No save file was found.");
                }

                FileStream file = File.Create(_savePath);
                _data = new SaveData() { GameVersion = string.Empty };
                _saveSerializer.Serialize(_data, file, null);
                file.Close();
            }
            else {
                if (_verboseLogging) {
                    print("Found save file -> read data.");
                }

                FileStream file = File.Open(_savePath, FileMode.Open);
                _data = _saveSerializer.Deserialize<SaveData>(file, null);
                file.Close();
            }
			
			if (_data == null) { // possible due to deserelization errors
                _data = new SaveData() { GameVersion = string.Empty };
            }
        }


        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        /// <param name="serializer">leave null to use binary</param>
        public static T LoadData<T>(string filePath, ISaveSerializer serializer = null) {
            if (serializer == null) {
                serializer = _saveSerializer;
            }

            filePath = Path.Combine(PathToFiles, filePath);

            if (File.Exists(filePath)) {
                FileStream file = File.Open(filePath, FileMode.Open);
                var data = serializer.Deserialize<T>(file, null);
                file.Close();
                return data;
            }
            else {
                return default;
            }
        }

#if UNITY_EDITOR
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Delete)) {
                DeleteSaveFile();
            }
        }
#endif

#if UNITY_EDITOR
        [Button("Log additional save data")]
        private void LogAdditionalSaveData() => _data.LogAdditionalData();
#endif

        /// <summary>
        /// You should not do it manualy, only if you know what you do (game saves automaticaly onFocus change event)
        /// </summary>
        [Button("Save Game")]
        public void SaveData() {
            if (!File.Exists(_savePath)) {
                Debug.LogError("Missing current Save path!");
				return;
            }

            OnDataSave?.Invoke(_data);

            FileStream file = File.Open(_savePath, FileMode.Open);
            _saveSerializer.Serialize(_data, file, null);
            file.Close();
        }

        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        /// <param name="serializer">leave null to use binary</param>
        public static void SaveData<T>(T data, string filePath, ISaveSerializer serializer = null) {
            if (serializer == null) {
                serializer = _saveSerializer;
            }

            filePath = Path.Combine(PathToFiles, filePath);

            FileStream file;
            if (!File.Exists(filePath)) {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath)) {
                    Directory.CreateDirectory(directoryPath);
                }
                file = File.Create(filePath);
                serializer.Serialize(data, file, null);
                file.Close();
                return;
            }

            file = File.Open(filePath, FileMode.Open);
            serializer.Serialize(data, file, null);
            file.Close();
        }

        public static void ClearFileContent(string pathToFile) {
            FileStream file;
            if (!File.Exists(pathToFile)) {
                return;
            }
            file = File.Open(pathToFile, FileMode.Open);
            file.SetLength(0);
            file.Close();
        }

        /// <summary>
        /// returns all files with .ftr extention in the Application persistent data path sorted descending
        /// </summary>
        public string[] GetSaveNames() {
            DirectoryInfo saveDir = new DirectoryInfo(PathToFiles);
            FileInfo[] saveFiles = saveDir.GetFiles("*.sqw"); // Getting files

            string[] fileNames = new string[saveFiles.Length];
            for (int i = 0; i < saveFiles.Length; i++) {
                fileNames[i] = saveFiles[saveFiles.Length - 1 - i].Name;
            }

            return fileNames;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Delete Save", priority = -999)]
#endif
        [Button("Delete Save")]
        private static void DeleteSaveFile() {
            var savePath = Path.Combine(PathToFiles, FileName);
            if (File.Exists(savePath)) {
                File.Delete(savePath);
                Debug.LogWarning("Save file was successfuly deleted");
            }
            else {
                Debug.LogError("There is no save file to delete");
            }
        }

        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        public static void DeleteSaveFile(string filePath) {
            var savePath = Path.Combine(PathToFiles, filePath);
            if (File.Exists(savePath)) {
                File.Delete(savePath);
                Debug.LogWarning("Save file was successfuly deleted");
            }
            else {
                Debug.LogError("There is no save file to delete");
            }
        }

        void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                if (_verboseLogging) {
                    print("data was saved");
                }
                SaveData();
            }
        }
    }
}
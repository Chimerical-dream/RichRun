using MoreMountains.NiceVibrations;
using Game.UI;

namespace Game.Core {

    /// <summary>
    /// Vibration manager x NiceVibrations
    /// </summary>
    public class VibrationsManager {
        /// <summary>
        /// This is cooldown between vibrations call, 0.2 seconds by default<br>
        /// Any vibration call before cooldown elapsed will be ignored</br>
        /// </summary>
        private CooldownUnscaled _hapticCooldown = new CooldownUnscaled(0.2f);

        public VibrationsManager() {
            _hapticEnabled = SaveSystem.Instance.Data.HapticEnabled;
            VibroBtn.OnClick += (vibroType) => {
                if (vibroType != HapticTypes.None) DoVibro(vibroType);
            };
        }

        #region SINGLETON PATTERN
        private static VibrationsManager _instance;
        public static VibrationsManager Instance {
            get {
                if (_instance == null) {
                    _instance = new VibrationsManager();
                }
                return _instance;
            }
        }
		public static void Create() => _instance = Instance;
        #endregion

        private bool _hapticEnabled;
        /// <summary>
        /// Is user settings vibro enabled
        /// </summary>
        public bool HapticEnabled {
            get => _hapticEnabled;
            set {
                _hapticEnabled = value;
                SaveSystem.Instance.Data.HapticEnabled = _hapticEnabled;
            }
        }

        /// <summary>
        /// Triggers phone vibration handler and plays <paramref name="hapticType"/>
        /// </summary>
        public void DoVibro(HapticTypes hapticType) {
            if (_hapticEnabled && _hapticCooldown.IsElapsed) {
                MMVibrationManager.Haptic(hapticType, defaultToRegularVibrate: true);
                _hapticCooldown.Reset();
            }
        }
		
		/// <summary>
        /// Android custom vibro
        /// </summary>
        /// <param name="duration">in milliseconds</param>
        /// <param name="intensity">intensity [0..255]</param>
        public void DoCustomVibroAndroid(long duration, int intensity) {
            if (_hapticEnabled && _hapticCooldown.IsElapsed) {
                MMNVAndroid.AndroidVibrate(duration, intensity);
            }
        }

        /// <summary>
        /// IOS custom vibro
        /// </summary>
        /// <param name="intensity">[0..1]</param>
        /// <param name="sharpness">[0..1]</param>
        public void DoCustomVibroIOS(float intensity, float sharpness) {
            if (_hapticEnabled && _hapticCooldown.IsElapsed) {
                MMVibrationManager.TransientHaptic(intensity, sharpness);
            }
        }
    }
}
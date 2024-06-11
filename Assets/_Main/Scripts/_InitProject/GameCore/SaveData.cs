using Game.Core;
using Game.JoystickUI;
using System.Collections.Generic;

/// <summary>
/// Main class that holds all savable data
/// </summary>
[System.Serializable]
public class SaveData {
    /// <summary>
    /// version check here <see cref="VersionValidator"/>
    /// </summary>
    public string GameVersion;

    /// <summary>
    /// Users currency, to manipulate use <see cref="CashManager"/> API
    /// Start values in Currency Settings config
    /// </summary>
    public Dictionary<Currencies, float> Currencies = null;

    /// <summary>
    /// Game haptic on/off, to manipulate use <see cref="VibrationsManager"/> API
    /// </summary>
    public bool HapticEnabled = true;

    /// <summary>
    /// Game sounds on/off, to manipulate use <see cref="Game.Audio.AudioManager"/> API
    /// </summary>
    public bool SoundsEnabled = true;
    /// <summary>
    /// In range [0..1] level of sfx channel
    /// </summary>
    public float SoundLevelNormalized = 1f;
    /// <summary>
    /// In range [0..1] level of music channel
    /// </summary>
    public float MusicLevelNormalized = 1f;

    /// <summary>
    /// Game joystick behaviour for <see cref="Joystick"/>.
    /// </summary>
    public JoystickBehaviours JoystickBehaviour = JoystickBehaviours.None; // leave None, so behaviour will be setup by config


    // add below any additional Serializable variables that will be in save file

#if UNITY_EDITOR
    public void LogAdditionalData() {
        System.Text.StringBuilder info = new System.Text.StringBuilder("Save additional data: \n");

        info.Append(Newtonsoft.Json.JsonConvert.SerializeObject(Currencies) + "\n");

        UnityEngine.Debug.Log(info.ToString());
    }
#endif
}
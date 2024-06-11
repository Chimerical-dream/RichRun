using UnityEngine;
using UnityEngine.UI;

namespace Game.Analyse {
	[AddComponentMenu("Common/Utils/FPS/Simple FPS")]
    public class Simple_FPS_Counter : MonoBehaviour {
        [SerializeField] private Text _fpsText;
        [SerializeField] private float _hudRefreshRate = 1f;

        private float _timer;
        int fps;

        private void Update() {
            if (Time.unscaledTime > _timer) {
                fps = (int)(1f / Time.unscaledDeltaTime);
                _fpsText.text = "FPS: " + fps;
                _timer = Time.unscaledTime + _hudRefreshRate;
            }
        }
    }
}
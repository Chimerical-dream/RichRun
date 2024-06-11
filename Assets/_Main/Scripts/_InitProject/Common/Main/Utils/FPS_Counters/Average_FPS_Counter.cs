using UnityEngine;
using UnityEngine.UI;

namespace Game.Analyse {
	[AddComponentMenu("Common/Utils/FPS/Average FPS")]
    public class Average_FPS_Counter : MonoBehaviour {
        [SerializeField] private Text _fpsText;
        [SerializeField, Range(1, 100)] private int _maxPassedFrames = 5;

        int _average_fps;

        float _passedFramesTime;
        int _passedFrames;

        private void Update() {
            _passedFrames++;
            _passedFramesTime += Time.unscaledDeltaTime;

            if (_passedFrames == _maxPassedFrames) {
                _average_fps = (int)(_passedFrames / _passedFramesTime);
                _fpsText.text = "FPS: " + _average_fps;

                _passedFrames = 0;
                _passedFramesTime = 0f;
            }
        }
    }
}
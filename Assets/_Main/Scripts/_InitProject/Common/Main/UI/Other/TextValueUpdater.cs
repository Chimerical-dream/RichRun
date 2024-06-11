using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {

    [AddComponentMenu("Common/UI/Text Value Updater")]
    public class TextValueUpdater : MonoBehaviour {
        public float CurrentValue => _currentValue;
        public float ChangeDuration => _changeDuration;
        /// <summary>
        /// Interface from what you can get Graphic and convert it to TMP or Text
        /// </summary>
        public IText TextBridge => _textBridge;

        [Tooltip("Text before value")] public string BeforeValueText = "";
        [Tooltip("Text after value")] public string AfterValueText = "";
        [Tooltip("For example '0' -> only int values. '0.0' -> float values with 1 digit after dot")]
        public string ValueFormat = "0";

        [SerializeField] private bool _updateInUnscaledTime = true;
        [SerializeField] private float _changeDuration = 2f;
        [SerializeField] private AnimationCurve _changeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float _currentValue = 0f;
        private Action _onComplete;
        private IText _textBridge;

        protected Coroutine _textUpdateCoroutine;

        protected virtual void Awake() {
            if (TryGetComponent(out Text legacyText)) _textBridge = new LegacyTextBridge(legacyText);
            else if (TryGetComponent(out TMP_Text textMeshPro)) _textBridge = new TMPTextBridge(textMeshPro);
            else Debug.LogError("[TextValueUpdater] The is no text component attached to gameObject or it's not supported", gameObject);
        }

        /// <summary>
        /// Updates text
        /// </summary>
        /// <param name="targetValue">future text value</param>
        /// <param name="duration">leave < 0f to use value from inspector</param>
        /// <param name="completeOldIfRunning">true -> set current value to target (if smooth fill)<br>
        /// </br>false -> keep smooth text change from current value</param>
        public TextValueUpdater UpdateTextSmooth(float targetValue, float duration = -1f, bool completeOldIfRunning = false) {
            StopUpdate(completeOldIfRunning);
            _onComplete = () => _currentValue = targetValue;
            _textUpdateCoroutine = StartCoroutine(UpdateSmooth(targetValue, duration >= 0 ? duration : _changeDuration));
            return this;
        }

        /// <summary>
        /// Instant test set (stops animation if was)
        /// </summary>
        public void UpdateTextInstant(float targetValue) {
            StopUpdate(false);
            _currentValue = targetValue;
            UpdateText();
        }

        /// <summary>
        /// Force stop updating text (smooth update)
        /// </summary>
        /// <param name="complete">should we complete coroutine (set target value to current)</param>
        public void StopUpdate(bool complete) {
            if (_textUpdateCoroutine != null) {
                StopCoroutine(_textUpdateCoroutine);
                _textUpdateCoroutine = null;

                if (complete) {
                    _onComplete?.Invoke();
                }
                _onComplete = null;
                UpdateText();
            }
        }

        /// <summary>
        /// Set ease curve of animation before it start
        /// </summary>
        public TextValueUpdater SetChangeCurve(AnimationCurve curve) {
            _changeCurve = curve;
            return this;
        }

        /// <summary>
        /// Set start value before animation start
        /// </summary>
        public TextValueUpdater SetStartValue(float value) {
            _currentValue = value;
            UpdateText();
            return this;
        }

        /// <summary>
        /// Add callback on text update finish
        /// </summary>
        public TextValueUpdater OnComplete(Action action) {
            _onComplete += action;
            return this;
        }

        IEnumerator UpdateSmooth(float targetValue, float duration) {
            float startValue = _currentValue;

            if (_updateInUnscaledTime) {
                for (float i = 0; i < 1f; i += Time.unscaledDeltaTime / duration) {
                    _currentValue = Mathf.Lerp(startValue, targetValue, _changeCurve.Evaluate(i));
                    UpdateText();
                    yield return null;
                }
            }
            else {
                for (float i = 0; i < 1f; i += Time.deltaTime / duration) {
                    _currentValue = Mathf.Lerp(startValue, targetValue, _changeCurve.Evaluate(i));
                    UpdateText();
                    yield return null;
                }
            }

            _currentValue = targetValue;
            UpdateText();
            _textUpdateCoroutine = null;
            _onComplete?.Invoke();
            _onComplete = null;
        }

        private void UpdateText() {
            _textBridge.text = BeforeValueText + _currentValue.ToString(ValueFormat) + AfterValueText;
        }
    }
}
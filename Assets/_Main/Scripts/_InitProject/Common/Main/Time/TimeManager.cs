using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {

    /// <summary>
    /// Main class to handle game time
    /// </summary>
    public class TimeManager : MonoSingleton<TimeManager> {
        private float _lastTimeScale;
        private float _defaultTimeScale;
        public float DefaultTimeScale => _defaultTimeScale;

        private float _defaultFixedDeltaTime;
        public float DefaultFixedDeltaTime => _defaultFixedDeltaTime;

        private Coroutine _motionCoroutine = null;
        private TimeEvent _timeEventData = new TimeEvent();

        protected override void Awake() {
            base.Awake();
            if (_awakeDestroyObj) {
                return;
            }
            _defaultTimeScale = Time.timeScale;
            _defaultFixedDeltaTime = Time.fixedDeltaTime;

            DontDestroyOnLoad(gameObject);
        }

        public static DateTime UnixTimeStampMillisecondsToDateTime(double unixTimeStamp) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)unixTimeStamp);
            return dateTimeOffset.LocalDateTime;
        }

        public static DateTime UnixTimeStampSecondsToDateTime(double unixTimeStamp) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp);
            return dateTimeOffset.LocalDateTime;
        }

        /// <summary>
        /// Resets time settings to default
        /// <code>
        ///    Time.timeScale = defaultTimeScale;
        ///    Time.fixedDeltaTime = defaultFixedDeltaTime;
        /// </code>
        /// </summary>
        public void ResetTimeToDefault() {
            _lastTimeScale = Time.timeScale;

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            Time.timeScale = _defaultTimeScale;
            Time.fixedDeltaTime = _defaultFixedDeltaTime;

            if (_lastTimeScale == 0f) {
                SendFreezeChangeEvent(false);
            }
        }

        /// <summary>
        /// Freezes time by scale
        /// </summary>
        public void FreezeTime() {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Time is already freezed and you tried to freeze it!", this);
#endif
				return;
            }

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            _lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            SendFreezeChangeEvent(true);
        }

        /// <summary>
        /// Unfreezes time by scale
        /// </summary>
        /// <param name="useDefault">Unfreeze and keep default timeScale
        /// <code>
        ///     Time.timeScale = defaultTimeScale;
        /// </code>
        /// </param>
        public void UnfreezeTime(bool useDefault = false) {
            if (Time.timeScale != 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Time was not freezed, but you tried to unfreeze it!", this);
#endif
				return;
            }

            if (useDefault) {
                ResetTimeToDefault();
            }
            else {
                Time.timeScale = _lastTimeScale;
                SendFreezeChangeEvent(false);
            }
        }
		
		public void SetTimeScale(float scale) {
            if (Time.timeScale > 0f && scale == 0f) {
                FreezeTime();
                return;
            }

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            _lastTimeScale = Time.timeScale;
            Time.timeScale = scale;

            if (Time.timeScale == 0f && scale > 0f) {
                _timeEventData.IsFreezed = false;
                GlobalEventsManager.ChangeTimeFreeze(_timeEventData);
            }
        }

        private void SendFreezeChangeEvent(bool isFreezed) {
            _timeEventData.IsFreezed = isFreezed;
            GlobalEventsManager.ChangeTimeFreeze(_timeEventData);
        }

        #region Time Motion Effect
        private TimeMotionEffectData _timeMotionData;
        private UnityAction _onTimeMotionEffectComplete;
        public void DoSlowMotion(TimeMotionEffectData data, UnityAction onEndCallback = null) {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Can't do time motion effect, time is freezed!", this);
#endif
				return;
            }

            if (_motionCoroutine != null) {
                StopCoroutine(_motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            //cache slow motion data
            _timeMotionData = data;
            _motionCoroutine = StartCoroutine(SlowMotion(onEndCallback));
        }

        public void DoSpeedUpMotion(TimeMotionEffectData data, UnityAction onEndCallback = null) {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Can't do time speed up motion effect, time is freezed!", this);
#endif
				return;
            }

            if (_motionCoroutine != null) {
                StopCoroutine(_motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            //cache slow motion data
            _timeMotionData = data;
            _motionCoroutine = StartCoroutine(SpeedUpMotion(onEndCallback));
        }
		
		public void StopTimeMotionEffect() {
            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }
        }

        IEnumerator SlowMotion(UnityAction onEndCallback) {
            float interpolationFactor;
            float startTimeScale = Time.timeScale;
            float startFixedDeltaTime = Time.fixedDeltaTime;
            WaitForSecondsRealtime realTimeFixedDelay = new WaitForSecondsRealtime(_defaultFixedDeltaTime);

            _onTimeMotionEffectComplete = () => {
                Time.timeScale = startTimeScale;
                Time.fixedDeltaTime = startFixedDeltaTime;
            };
            if (onEndCallback != null) _onTimeMotionEffectComplete += onEndCallback;

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeIn) {
                interpolationFactor = _timeMotionData.CurveIn.Evaluate(i);
                Time.timeScale = Mathf.Lerp(startTimeScale, _timeMotionData.Scale, interpolationFactor);
                Time.fixedDeltaTime = Mathf.Lerp(startFixedDeltaTime, _timeMotionData.Scale * startFixedDeltaTime, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            Time.timeScale = _timeMotionData.Scale;
            Time.fixedDeltaTime = _timeMotionData.Scale * startFixedDeltaTime;

            if (_timeMotionData.Duration < 0f) {
                onEndCallback?.Invoke();
                _onTimeMotionEffectComplete = null;
                yield break;
            }
            yield return new WaitForSecondsRealtime(_timeMotionData.Duration);

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeOut) {
                interpolationFactor = _timeMotionData.CurveOut.Evaluate(i);
                Time.timeScale = Mathf.Lerp(_timeMotionData.Scale, startTimeScale, interpolationFactor);
                Time.fixedDeltaTime = Mathf.Lerp(_timeMotionData.Scale * startFixedDeltaTime, startFixedDeltaTime, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            _motionCoroutine = null;
            _onTimeMotionEffectComplete.Invoke();
        }

        IEnumerator SpeedUpMotion(UnityAction onEndCallback) {
            float interpolationFactor;
            float startTimeScale = Time.timeScale;
            WaitForSecondsRealtime realTimeFixedDelay = new WaitForSecondsRealtime(_defaultFixedDeltaTime);

            _onTimeMotionEffectComplete = () => {
                Time.timeScale = startTimeScale;
            };
            if (onEndCallback != null) _onTimeMotionEffectComplete += onEndCallback;

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeIn) {
                interpolationFactor = _timeMotionData.CurveIn.Evaluate(i);
                Time.timeScale = Mathf.Lerp(startTimeScale, _timeMotionData.Scale, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            Time.timeScale = _timeMotionData.Scale;

            if (_timeMotionData.Duration < 0f) {
                onEndCallback?.Invoke();
                _onTimeMotionEffectComplete = null;
                yield break;
            }
            yield return new WaitForSecondsRealtime(_timeMotionData.Duration);

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeOut) {
                interpolationFactor = _timeMotionData.CurveOut.Evaluate(i);
                Time.timeScale = Mathf.Lerp(_timeMotionData.Scale, startTimeScale, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            _motionCoroutine = null;
            _onTimeMotionEffectComplete.Invoke();
        }
        #endregion
    }
}
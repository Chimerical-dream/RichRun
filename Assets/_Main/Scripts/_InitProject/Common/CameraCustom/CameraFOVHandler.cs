using UnityEngine;
using DG.Tweening;

namespace Game.Core {
    [RequireComponent(typeof(Camera))]
    public class CameraFOVHandler : MonoBehaviour {
        [SerializeField] private AnimationCurve _changeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <summary>
        /// Current field of view of camera
        /// </summary>
        public float CurrentFOV => _targetCamera.fieldOfView;

        /// <summary>
        /// Current OrthographicSize
        /// </summary>
        public float CurrentOrthographicSize => _targetCamera.orthographicSize;

        /// <summary>
        /// Camera that is handled by this script
        /// </summary>
        public Camera Camera => _targetCamera;

        private float _initFOV;
        private float _initOrthograpicSize;

        private Camera _targetCamera;
        private Sequence _contolSequence;
        private bool _inited = false;


        public void Awake() {
            if (_inited == false) {
                Init(GetComponent<Camera>());
            }
        }

        public void Init(Camera camera) {
            _targetCamera = camera;

            _initFOV = _targetCamera.fieldOfView;
            _initOrthograpicSize = _targetCamera.orthographicSize;

            _inited = true;
        }

        public Vector2 GetOrthographicBounds() {
            float cameraHeight = _targetCamera.orthographicSize * 2;
            return new Vector2(cameraHeight * _targetCamera.aspect, cameraHeight);
        }

        public Vector2 GetFrustrumBounds(float atDistance) {
            float frustumHeight = 2.0f * atDistance * Mathf.Tan(_targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * _targetCamera.aspect, frustumHeight);
        }

        /// <summary>
        /// Reset FOV to init default
        /// </summary>
        public void ResetFOV(float duration, AnimationCurve curve = null) {
            ChangeFOV(_initFOV, duration, curve);
        }

        /// <summary>
        /// Reset FOV to init default
        /// </summary>
        public void ResetFOV() {
            StopChangeFOV();
            ChangeFOV(_initFOV);
        }

        public void ChangeFOV(float target) {
            _targetCamera.fieldOfView = target;
        }

        public void ChangeFOV(float target, float changeTime, AnimationCurve curve = null) {
            StopChangeFOV();
            if (curve == null) { curve = _changeCurve; }
            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.fieldOfView, target, changeTime, ChangeFOV))
                           .SetEase(curve)
                           .OnComplete(() => _targetCamera.fieldOfView = target);
        }

        public void ChangeFOV(float target, float changeTime, Ease ease) {
            StopChangeFOV();
            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.fieldOfView, target, changeTime, ChangeFOV))
                           .SetEase(ease)
                           .OnComplete(() => _targetCamera.fieldOfView = target);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue) {
            ChangeFOV(_targetCamera.fieldOfView + fovAddValue);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue, float changeTime, AnimationCurve curve = null) {
            ChangeFOV(_targetCamera.fieldOfView + fovAddValue, changeTime, curve);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue, float changeTime, Ease ease) {
            ChangeFOV(_targetCamera.fieldOfView + fovAddValue, changeTime, ease);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoFOVBounce(float bounceDeltaFOV, float durationIn, float durationOut, float endTarget = -1f) {
            StopChangeFOV();

            float endValue = _targetCamera.fieldOfView;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget >= 0) {
                endValue = endTarget;
            }

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.fieldOfView, bounceFOV, durationIn, ChangeFOV))
                           .Append(DOVirtual.Float(bounceFOV, endValue, durationOut, ChangeFOV))
                           .SetEase(_changeCurve)
                           .OnComplete(() => _targetCamera.fieldOfView = endValue);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoFOVBounce(float bounceDeltaFOV, float durationIn, float durationOut, Ease easeIn, Ease easeOut, float endTarget = -1f) {
            StopChangeFOV();

            float endValue = _targetCamera.fieldOfView;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget >= 0) {
                endValue = endTarget;
            }

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.fieldOfView, bounceFOV, durationIn, ChangeFOV).SetEase(easeIn))
                           .Append(DOVirtual.Float(bounceFOV, endValue, durationOut, ChangeFOV).SetEase(easeOut))
                           .OnComplete(() => _targetCamera.fieldOfView = endValue);
        }


        /// <summary>
        /// Reset OrthographicSize to init default
        /// </summary>
        public void ResetOrthographicSize(float duration, AnimationCurve curve = null) {
            ChangeOrthographicSize(_initOrthograpicSize, duration, curve);
        }

        public void ResetOrthographicSize() {
            StopChangeFOV();
            ChangeOrthographicSize(_initOrthograpicSize);
        }

        public void ChangeOrthographicSize(float target) {
            _targetCamera.orthographicSize = target;
        }

        public void ChangeOrthographicSizeWorldX(float xViewWorldSize) {
            _targetCamera.orthographicSize = xViewWorldSize / _targetCamera.aspect * 0.5f;
        }

        public void ChangeOrthographicSizeWorldX(float xViewWorldSize, float changeTime, AnimationCurve curve = null) {
            ChangeOrthographicSize(xViewWorldSize / _targetCamera.aspect * 0.5f, changeTime, curve);
        }

        public void ChangeOrthographicSize(float target, float changeTime, AnimationCurve curve = null) {
            StopChangeFOV();
            if (curve == null) { curve = _changeCurve; }

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.orthographicSize, target, changeTime, ChangeOrthographicSize))
                           .SetEase(curve)
                           .OnComplete(() => _targetCamera.orthographicSize = target);
        }

        public void ChangeOrthographicSize(float target, float changeTime, Ease ease) {
            StopChangeFOV();

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.orthographicSize, target, changeTime, ChangeOrthographicSize))
                           .SetEase(ease)
                           .OnComplete(() => _targetCamera.orthographicSize = target);
        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue) {
            ChangeOrthographicSize(_targetCamera.orthographicSize + addValue);
        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue, float changeTime, AnimationCurve curve = null) {
            ChangeOrthographicSize(_targetCamera.orthographicSize + addValue, changeTime, curve);
        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue, float changeTime, Ease ease) {
            ChangeOrthographicSize(_targetCamera.orthographicSize + addValue, changeTime, ease);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoOrthographicSizeBounce(float bounceDeltaFOV, float durationIn, float durationOut, float endTarget = -1f) {
            StopChangeFOV();

            float endValue = _targetCamera.orthographicSize;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget > 0) {
                endValue = endTarget;
            }

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.orthographicSize, bounceFOV, durationIn, ChangeOrthographicSize))
                           .Append(DOVirtual.Float(bounceFOV, endValue, durationOut, ChangeOrthographicSize))
                           .SetEase(_changeCurve)
                           .OnComplete(() => _targetCamera.orthographicSize = endValue);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoOrthographicSizeBounce(float bounceDeltaFOV, float durationIn, float durationOut, Ease easeIn, Ease easeOut, float endTarget = -1f) {
            StopChangeFOV();

            float endValue = _targetCamera.orthographicSize;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget > 0) {
                endValue = endTarget;
            }

            _contolSequence = DOTween.Sequence();
            _contolSequence.Append(DOVirtual.Float(_targetCamera.orthographicSize, bounceFOV, durationIn, ChangeOrthographicSize).SetEase(easeIn))
                           .Append(DOVirtual.Float(bounceFOV, endValue, durationOut, ChangeOrthographicSize).SetEase(easeOut))
                           .OnComplete(() => _targetCamera.orthographicSize = endValue);
        }

        public void StopChangeFOV() {
            if (_contolSequence != null && _contolSequence.active) {
                _contolSequence.Kill(true);
                _contolSequence = null;
            }
        }

        private void OnDestroy() {
            StopChangeFOV();
        }
    }
}
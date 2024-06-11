using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {
    /// <summary>
    /// CameraPOVData is used to keep position and rotation of each POV
    /// </summary>
    [Serializable]
    public class CameraPOVData {
        public string Name;
        public Vector3 CameraLocalPosition;
        public Quaternion CameraLocalRotation;
        public float FOV = 60f;
    }

    [Serializable]
    public class CameraFocusPoint {
        public Transform Point;
        public bool CopyPointRotation = false;
        public float MoveDuration = 1f;
        public float OnReachedPointFocusDuration = 0.5f;
        public string POV_Name;
    }

    public class CameraFollower : MonoBehaviour {
        public static UnityEvent OnPOVChangeStart = new UnityEvent();
        public static UnityEvent OnPOVChangeEnd = new UnityEvent();

        public static UnityEvent OnFocusStart = new UnityEvent();
        public static UnityEvent<CameraFocusPoint> OnFocused = new UnityEvent<CameraFocusPoint>();
        public static UnityEvent OnFocusFinished = new UnityEvent();

        public static CameraFollower instance;

        /// <summary>
        /// Target that camera holder follows to
        /// </summary>
        public Transform FollowTarget {
            get => _followTarget;
            set {
                if (_coroutineChangeFollowTarget != null) {
                    StopCoroutine(_coroutineChangeFollowTarget);
                    _coroutineChangeFollowTarget = null;
                    _onChangeFollowTarget_Finished = null;
                }
                if (_focusingCoroutine != null) {
                    _lastFollowTarget = value;
                    _focusBeforeCoroutineStarted.Point = value;
                    return;
                }
                _followTarget = value;
            }
        }

        public bool IsFocusing => _focusingCoroutine != null;

        /// <summary>
        /// Shake system, that is used to make Camera shake effects
        /// </summary>
        public ObjectShake ShakeEffect => _cameraShake;

        /// <summary>
        /// FOV handler, that is used to make FOV effects (change field of view runtime)
        /// </summary>
        public CameraFOVHandler CameraFOVHandler => _cameraFOV;

        /// <summary>
        /// Camera transform in holder system
        /// </summary>
        public Transform CameraTransform => _cameraTransform;

        /// <summary>
        /// Camera component in holder system
        /// </summary>
        public Camera CameraComponent => _camera;

        /// <summary>
        /// System camera and other modules holder transform (move it to keep main camera offsets=POVs)
        /// </summary>
        public Transform HolderTransform => _systemHolder;

        /// <summary>
        /// Will camera copy <see cref="FollowTarget"/> rotation?
        /// </summary>
        [Space(10f)]
        public bool CopyFollowTargetRotation = false;
        /// <summary>
        /// How fast camera holder will follow target position
        /// </summary>
        public float FollowPositionLerp = 10f;
        /// <summary>
        /// How fast camera holder will follow target rotation
        /// </summary>
        public float FollowRotationLerp = 10f;

        /// <summary>
        /// Bunch of holder local points, that we can refer to change it's local position for camera and rotation
        /// </summary>
        public CameraPOVData[] CameraPOV_s;
        /// <summary>
        /// Active local point in holder that is used as position and rotation for MainCamera object
        /// </summary>
        public CameraPOVData ActivePov => _activePOV;

        [SerializeField] protected Transform _followTarget;

        [Space(10f)]
        [SerializeField] protected Transform _cameraTransform;
        [SerializeField] protected ObjectShake _cameraShake;
        [SerializeField] protected CameraFOVHandler _cameraFOV;

        /// <summary>
        /// Used to make interpolated curve while changing POV from one to another<br>
        /// You can change it immediately after <see cref="ChangePov(POV_Names, float)"/> call</br>
        /// </summary>
        [Space(10f)]
        [SerializeField] protected AnimationCurve _changePOV_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        /// <summary>
        /// Used to make interpolated curve while changing POV from one to another<br>
        /// You can change it immediately after <see cref="ChangePov(POV_Names, float)"/> call</br>
        /// </summary>
        [SerializeField] protected AnimationCurve _changePOV_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] protected AnimationCurve _focusPointMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Used to make interpolated curve while changing FollowTarget from one to another<br>
        /// You can change it immediately after <see cref="ChangeFollowTargetCoroutine(Transform, float, float)"/> call</br>
        /// </summary>
        [Space(10f)]
        [SerializeField] protected AnimationCurve _changeTarget_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        /// <summary>
        /// Used to make interpolated curve while changing FollowTarget from one to another<br>
        /// You can change it immediately after <see cref="ChangeFollowTargetCoroutine(Transform, float, float)"/> call</br>
        /// </summary>
        [SerializeField] protected AnimationCurve _changeTarget_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        protected Coroutine _coroutineChangePOV;
        protected Coroutine _coroutineChangeFollowTarget;
        protected Coroutine _focusingCoroutine;

        protected Transform _systemHolder;
        protected Transform _cameraHolder;
        protected CameraPOVData _activePOV;

        protected float _defaultFollowPositionLerp;
        protected float _defaultFollowRotationLerp;
        protected AnimationCurve _defaultChangePOV_PosCurve;
        protected AnimationCurve _defaultChangePOV_RotCurve;
        protected AnimationCurve _defaultChangeTargetPosCurve;
        protected AnimationCurve _defaultChangeTargetRotCurve;

        protected Camera _camera;
        protected Transform _lastFollowTarget;
        protected Transform _emptyPointFrom;

        protected List<CameraFocusPoint> _focusPoints = new List<CameraFocusPoint>(2);
        protected CameraFocusPoint _focusBeforeCoroutineStarted;

        protected Action _onPOV_Changed;
        protected Action _onChangeFollowTarget_Finished;

        protected virtual void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _systemHolder = transform;
            _cameraHolder = _cameraTransform.parent;
            _camera = _cameraTransform.GetComponent<Camera>();

            if (transform.parent != null) {
                transform.parent = null;
            }

            _emptyPointFrom = new GameObject("CameraFollower EmptyPointFrom").transform;
            _emptyPointFrom.parent = _systemHolder.parent;

            _defaultFollowPositionLerp = FollowPositionLerp;
            _defaultFollowRotationLerp = FollowRotationLerp;

            _defaultChangePOV_PosCurve = _changePOV_PositionCurve;
            _defaultChangePOV_RotCurve = _changePOV_RotationCurve;

            _defaultChangeTargetPosCurve = _changeTarget_PositionCurve;
            _defaultChangeTargetRotCurve = _changeTarget_RotationCurve;

            _cameraFOV.Init(_camera);
            ChangePov(CameraPOV_s[0], 0f);
        }

        protected virtual void LateUpdate() {
            if (_followTarget) {
                //move camera holder to target
                _systemHolder.position =
                    Vector3.Lerp(_systemHolder.position, _followTarget.position, FollowPositionLerp * Time.deltaTime);

                if (CopyFollowTargetRotation) {
                    _systemHolder.rotation =
                        Quaternion.Slerp(_systemHolder.rotation, _followTarget.rotation, FollowRotationLerp * Time.deltaTime);
                }
            }
        }

        public void AddFocusPoint(Transform point, float focusTime = 1f) {
            AddFocusPoint(new CameraFocusPoint() { Point = point, OnReachedPointFocusDuration = focusTime });
        }
        public void AddFocusPoint(CameraFocusPoint focusPoint) {
            _focusPoints.Add(focusPoint);

            if (_focusingCoroutine == null) {
                _lastFollowTarget = _followTarget;
                _followTarget = null;

                if (_focusBeforeCoroutineStarted == null) {
                    _focusBeforeCoroutineStarted = new CameraFocusPoint();
                }
                _focusBeforeCoroutineStarted.POV_Name = _activePOV.Name;
                _focusingCoroutine = StartCoroutine(FocusPoints());
            }
        }
        IEnumerator FocusPoints() {
            OnFocusStart.Invoke();

            CameraFocusPoint focusPoint;
            float easedValue;
            Quaternion initRotation = _systemHolder.rotation;

            while (_focusPoints.Count > 0) {
                focusPoint = _focusPoints[0];
                _emptyPointFrom.position = _systemHolder.position;
                _emptyPointFrom.rotation = _systemHolder.rotation;

                if (string.IsNullOrEmpty(focusPoint.POV_Name) == false) {
                    ChangePov(focusPoint.POV_Name, focusPoint.MoveDuration - Time.deltaTime);
                }

                for (float i = 0f; i < 1f; i += Time.deltaTime / focusPoint.MoveDuration) {
                    easedValue = _focusPointMoveCurve.Evaluate(i);
                    _systemHolder.position = Vector3.Lerp(
                                            _systemHolder.position,
                                            Vector3.Lerp(_emptyPointFrom.position, focusPoint.Point.position, easedValue),
                                            easedValue
                                        );

                    if (focusPoint.CopyPointRotation) {
                        _systemHolder.rotation = Quaternion.Slerp(
                            _systemHolder.rotation,
                            Quaternion.Slerp(_emptyPointFrom.rotation, focusPoint.Point.rotation, easedValue),
                                easedValue
                            );
                    }
                    yield return null;
                }

                _systemHolder.position = focusPoint.Point.position;
                if (focusPoint.CopyPointRotation) _systemHolder.rotation = focusPoint.Point.rotation;

                OnFocused.Invoke(focusPoint);
                _focusPoints.RemoveAt(0);
                yield return new WaitForSeconds(focusPoint.OnReachedPointFocusDuration);
            }

            if (_lastFollowTarget == null) {
                _focusingCoroutine = null;
                OnFocusFinished.Invoke();
                yield break;
            }

            // go back
            focusPoint = _focusBeforeCoroutineStarted;
            focusPoint.Point = _lastFollowTarget;
            _emptyPointFrom.position = _systemHolder.position;
            _emptyPointFrom.rotation = _systemHolder.rotation;

            if (_activePOV.Name.Equals(focusPoint.POV_Name) == false) {
                ChangePov(focusPoint.POV_Name, focusPoint.MoveDuration - Time.deltaTime);
            }

            for (float i = 0f; i < 1f; i += Time.deltaTime / focusPoint.MoveDuration) {
                easedValue = _focusPointMoveCurve.Evaluate(i);
                _systemHolder.position = Vector3.Lerp(
                                        _systemHolder.position,
                                        Vector3.Lerp(_emptyPointFrom.position, _lastFollowTarget.position, easedValue),
                                        easedValue
                                    );

                if (CopyFollowTargetRotation) {
                    _systemHolder.rotation = Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(_emptyPointFrom.rotation, _lastFollowTarget.rotation, easedValue),
                            easedValue
                        );
                }
                else if (_systemHolder.rotation != initRotation) {
                    _systemHolder.rotation = Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(_emptyPointFrom.rotation, initRotation, easedValue),
                            easedValue
                        );
                }
                yield return null;
            }

            _systemHolder.position = _lastFollowTarget.position;
            if (CopyFollowTargetRotation) {
                _systemHolder.rotation = _lastFollowTarget.rotation;
            }
            else {
                _systemHolder.rotation = initRotation;
            }

            if (_focusPoints.Count > 0) {
                StartCoroutine(FocusPoints());
                yield break;
            }

            _focusingCoroutine = null;
            FollowTarget = _lastFollowTarget;
            OnFocusFinished.Invoke();
        }

        public virtual void ResetFollowPositionLerp() {
            FollowPositionLerp = _defaultFollowPositionLerp;
        }
        public virtual void ResetFollowRotationLerp() {
            FollowRotationLerp = _defaultFollowRotationLerp;
        }

        /// <summary>
        /// Smoothly changes follow target by <paramref name="changeTime"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangeTarget(Transform newTarget, float changeTime = 0f, float changeDelay = 0f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (changeTime <= 0f) {
                FollowTarget = newTarget;
                _systemHolder.position = newTarget.position;
                return this;
            }

            if (_coroutineChangeFollowTarget != null) {
                StopCoroutine(_coroutineChangeFollowTarget);
                _onChangeFollowTarget_Finished = null;
            }

            if (positionCurve == null) {
                _changeTarget_PositionCurve = _defaultChangeTargetPosCurve;
            }
            else {
                _changeTarget_PositionCurve = positionCurve;
            }

            if (rotationCurve == null) {
                _changeTarget_RotationCurve = _defaultChangeTargetRotCurve;
            }
            else {
                _changeTarget_RotationCurve = rotationCurve;
            }

            _coroutineChangeFollowTarget = StartCoroutine(ChangeFollowTargetCoroutine(newTarget, changeTime, changeDelay));
            return this;
        }

        /// <summary>
        /// Appends callback to running Change follow target process. <br>
        /// If function ChangeTarget wasnt called, then callback will be invoked immediately </br>
        /// </summary>
        public void AppendOnChangeTarget_FinishedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangeFollowTarget == null) {
                callback.Invoke();
            }
            _onChangeFollowTarget_Finished += callback;
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(int povIndex, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (CameraPOV_s.Length - 1 < povIndex) {
                Debug.LogError($"POV number {povIndex + 1} doesn't exist");
                return this;
            }
            if (CameraPOV_s[povIndex].Name.Equals(_activePOV.Name)) {
                return this;
            }
            return ChangePov(CameraPOV_s[povIndex], time, positionCurve, rotationCurve);
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(string povName, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (povName.Equals(_activePOV.Name)) {
                return this;
            }
            int povIndex = GetPOV_Index(povName);
            return ChangePov(CameraPOV_s[povIndex], time, positionCurve, rotationCurve);
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(CameraPOVData cameraPOV, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (_activePOV != null && _activePOV.Name.Equals(cameraPOV.Name)) {
                return this;
            }

            _activePOV = cameraPOV;
            OnPOVChangeStart.Invoke();

            if (time <= 0f) {
                _cameraHolder.localPosition = _activePOV.CameraLocalPosition;
                _cameraHolder.localRotation = _activePOV.CameraLocalRotation;
                _cameraFOV.StopChangeFOV();
                _cameraFOV.ChangeFOV(cameraPOV.FOV);
                OnPOVChangeEnd.Invoke();
                return this;
            }

            if (positionCurve == null) {
                _changePOV_PositionCurve = _defaultChangePOV_PosCurve;
            }
            else {
                _changePOV_PositionCurve = positionCurve;
            }

            if (rotationCurve == null) {
                _changePOV_RotationCurve = _defaultChangePOV_RotCurve;
            }
            else {
                _changePOV_RotationCurve = rotationCurve;
            }

            if (_coroutineChangePOV != null) {
                StopCoroutine(_coroutineChangePOV);
                _onPOV_Changed = null;
            }
            _cameraShake.IgnoreShake = true;
            _cameraFOV.ChangeFOV(cameraPOV.FOV, time);
            _coroutineChangePOV = StartCoroutine(POVChanging(_activePOV, time));
            return this;
        }

        /// <summary>
        /// Set callback to running POV change process <br>
        /// This method will reset all callbacks and set argument <br></br>
        /// If no ChangePov was called, then callback will be invoked immediately </br>
        /// </summary>
        public void SetOnPOV_ChangedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangePOV == null) {
                callback.Invoke();
                return;
            }
            _onPOV_Changed = callback;
        }

        /// <summary>
        /// Appends callback to running POV change process. <br>
        /// If no ChangePov was called, then callback will be invoked immediately </br>
        /// </summary>
        public void AppendOnPOV_ChangedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangePOV == null) {
                callback.Invoke();
                return;
            }
            _onPOV_Changed += callback;
        }

        IEnumerator POVChanging(CameraPOVData cameraPOV, float time) {
            Vector3 startPos = _cameraHolder.localPosition;
            Quaternion startRot = _cameraHolder.localRotation;

            // if you want to change POV even if time freezed -> use Time.unscaledDeltaTime
            for (float i = 0f; i < 1f; i += Time.deltaTime / time) {
                _cameraHolder.localPosition =
                        Vector3.Lerp(startPos, cameraPOV.CameraLocalPosition, _changePOV_PositionCurve.Evaluate(i));
                _cameraHolder.localRotation =
                        Quaternion.Slerp(startRot, cameraPOV.CameraLocalRotation, _changePOV_RotationCurve.Evaluate(i));
                yield return null;
            }
            _cameraHolder.localPosition = cameraPOV.CameraLocalPosition;
            _cameraHolder.localRotation = cameraPOV.CameraLocalRotation;
            _coroutineChangePOV = null;
            _cameraShake.IgnoreShake = false;


            var callback = _onPOV_Changed;
            _onPOV_Changed = null;
            callback?.Invoke();

            OnPOVChangeEnd.Invoke();
        }

        IEnumerator ChangeFollowTargetCoroutine(Transform newTarget,
                                                float changeDuration, float changeDelay = 0f) {
            if (changeDelay > 0f) {
                yield return new WaitForSeconds(changeDelay);
            }

            _emptyPointFrom.SetPositionAndRotation(_systemHolder.position, _systemHolder.rotation);

            //to prevent LateUpdates;
            _followTarget = null;

            if (CopyFollowTargetRotation) {
                for (float i = 0; i < 1f; i += Time.deltaTime / changeDuration) {
                    //need to include follow lerp as well
                    _systemHolder.position = Vector3.Lerp(
                        _systemHolder.position,
                        Vector3.Lerp(_emptyPointFrom.position, newTarget.position, _changeTarget_PositionCurve.Evaluate(i)),
                        FollowPositionLerp
                    );

                    _systemHolder.rotation = Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(_emptyPointFrom.rotation, newTarget.rotation, _changeTarget_RotationCurve.Evaluate(i)),
                        FollowRotationLerp
                    );
                    yield return null;
                }
            }
            else {
                for (float i = 0; i < 1f; i += Time.deltaTime / changeDuration) {
                    //need to include follow lerp as well
                    _systemHolder.position = Vector3.Lerp(
                        _systemHolder.position,
                        Vector3.Lerp(_emptyPointFrom.position, newTarget.position, _changeTarget_PositionCurve.Evaluate(i)),
                        FollowPositionLerp
                    );
                    yield return null;
                }
            }


            _systemHolder.position = Vector3.Lerp(_systemHolder.position, newTarget.position, FollowPositionLerp);
            if (CopyFollowTargetRotation) {
                _systemHolder.rotation = Quaternion.Slerp(
                        _systemHolder.rotation, newTarget.rotation, FollowRotationLerp);
            }
            _coroutineChangeFollowTarget = null;

            _onChangeFollowTarget_Finished?.Invoke();
            _onChangeFollowTarget_Finished = null;

            FollowTarget = newTarget;
        }

        protected int GetPOV_Index(string name) {
            int index = 0;
            foreach (var pov in CameraPOV_s) {
                if (string.Equals(pov.Name, name, StringComparison.Ordinal)) {
                    return index;
                }
                index++;
            }
            Debug.LogError($"Didnt find POV with name {name}. Return 0 index = {CameraPOV_s[0].Name}.");
            return 0;
        }

        protected void OnValidate() {
            if (CameraPOV_s == null || CameraPOV_s.Length == 0 && _cameraHolder != null) {
                CameraPOV_s = new CameraPOVData[1] {
                    new CameraPOVData() {
                        Name = "Default",
                        CameraLocalPosition = _cameraHolder.localPosition,
                        CameraLocalRotation = _cameraHolder.rotation
                    }
                };
            }
        }
    }
}
using Game.Core;
using ChimeraGames;
using Game.UI;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.JoystickUI {
    public class Joystick : MonoBehaviour, IPointerDownHandler {
        public static Joystick Instance;
        public JoystickBehaviours BehaviourType => _behaviourType;

        /// <summary>
        /// Joystick move dir on plane XZ
        /// </summary>
        public Vector3 MoveDirection => _behaviour.MoveDirection;
        /// <summary>
        /// MoveCoef, [0..1]
        /// </summary>
        public float MoveCoef => _behaviour.MoveCoef;
        /// <summary>
        /// returns MoveDirection * MoveCoef
        /// </summary>
        public Vector3 Movement => _behaviour.Movement;
        /// <summary>
        /// returns radius scaled with current resolution in pixels
        /// </summary>
        public float JoystickScaledBackgroundRadius => _backgroundRadiusInPixels;

        /// <summary>
        /// Enables or Disables joystic object
        /// </summary>
        public bool IsEnabled {
            set {
                if (_isEnabled == value) {
                    return;
                }

                gameObject.SetActive(value);
                _isEnabled = value;

                if (_behaviour == null) {
                    SwitchBehaviourLocal(_behaviourType);
                }

                if (value) {
                    if (PlayerInput.IsOnTouch == false) {
                        IsVisible = false;
                    }
                    else {
                        var element = PlayerInput.GetCurrentTouchUIElement().gameObject;
                        if (element == _touchZone) {
                            var pointData = new PointerEventData(EventSystem.current);
                            pointData.position = PlayerInput.LastTouchPos;

                            OnPointerDown(pointData);
                        }
                    }
                }
                else if (!value) {
                    _behaviour.OnTouchRelease();
                    _isInProgress = false;
                }
            }
            get => _isEnabled;
        }

        /// <summary>
        /// Hides or Shows joystic object
        /// </summary>
        public bool IsVisible {
            set {
                if (_isVisible == value || _settings.RenderJoystickOnUI == false) {
                    return;
                }

                _isVisible = value;
                _stickBackgroundImage.enabled = value;
                _stickImage.enabled = value;
            }
            get => _isVisible;
        }

        private static UnityEvent<JoystickBehaviours> OnBehaviorSwitch = new();

        [SerializeField] private JoystickSettingsSO _settings;
        [SerializeField] private CanvasScaler _rootUI_CanvasScaler;
        [SerializeField] private RectTransform _stickBackground;
        [SerializeField] private RectTransform _stick;
        [SerializeField] private GameObject _touchZone;

        internal JoystickBehaviour Behaviour => _behaviour;

        private Image _stickBackgroundImage;
        private Image _stickImage;

        private JoystickBehaviour _behaviour;
        private Vector2 _resolutionScaleFactor;
        private JoystickBehaviours _behaviourType;
        private float _joystickParamsScaleFactor;
        private float _backgroundRadiusInPixels;
        private bool _isVisible = false;
        private bool _isEnabled = false;
        private bool _isInProgress = false;
        private float _aspectFactorRefToUser;

        private void Awake()
        {
            Instance = this;

            _stickBackgroundImage = _stickBackground.GetComponent<Image>();
            _stickImage = _stick.GetComponent<Image>();

            if (_rootUI_CanvasScaler == null) {
                _rootUI_CanvasScaler = GetComponentInParent<CanvasScaler>();
            }

            OnScreenSizeUpdated(SafeAreaManager.Instance.SafeAreaOffsets);

            if (_stickImage.enabled) {
                _isVisible = true; // set local to true so
                IsVisible = false; // property change will not be ignored
            }

            if (_settings.RenderJoystickOnUI == false) {
                _stickImage.gameObject.SetActive(false);
                _stickBackground.gameObject.SetActive(false);
            }

            var saveData = SaveSystem.Instance.Data;
            if (saveData.JoystickBehaviour != JoystickBehaviours.None) {
                _behaviourType = saveData.JoystickBehaviour;
            }
            else {
                _behaviourType = _settings.DefaultJoystickBehaviour;
            }

            IsEnabled = true; // enable by default

            PlayerInput.OnTouchMoved.AddListener(OnPointerMove);
            PlayerInput.OnTouchEnded.AddListener(OnPointerUp);

            OnBehaviorSwitch.AddListener(SwitchBehaviourLocal);
        }

        private void Start() {
            SafeAreaManager.Instance.OnSaveAreaUpdated.AddListener(OnScreenSizeUpdated);
        }

        private void OnScreenSizeUpdated(Vector4 safeAreas) {
            var refResolution = _rootUI_CanvasScaler.referenceResolution;
            _aspectFactorRefToUser = refResolution.x / refResolution.y / ((float)Screen.width / Screen.height);
            _joystickParamsScaleFactor = Screen.width / refResolution.x * _aspectFactorRefToUser;
            _resolutionScaleFactor = new Vector2(_joystickParamsScaleFactor, Screen.height / refResolution.y);
            _backgroundRadiusInPixels = _stickBackground.sizeDelta.x * 0.5f * _joystickParamsScaleFactor;

            if (_rootUI_CanvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand && _aspectFactorRefToUser > 1f) {
                _resolutionScaleFactor /= _aspectFactorRefToUser;
            }

            SetJoystickSize(_settings.JoystickRadius, _settings.StickSpriteRadius);
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (_isInProgress) {
                return;
            }


            if (Input.touchSupported && Input.touchCount > 0) {
                if (eventData.pointerId == 0) {
                    _behaviour.OnTouchBegan(eventData.position);
                    _isInProgress = true;
                }
            }
            else {
                if (eventData.pointerId == -1) {
                    _behaviour.OnTouchBegan(eventData.position);
                    _isInProgress = true;
                }
            }
        }

        public void OnPointerMove() {
            if (_isInProgress) {
                _behaviour.OnTouchMove(PlayerInput.LastTouchPos);
            }
        }

        public void OnPointerUp() {
            if (_isInProgress) {
                _behaviour.OnTouchRelease();
                _isInProgress = false;
            }
        }

        public static void SwitchBehaviour(JoystickBehaviours newBehaviourType) {
            OnBehaviorSwitch.Invoke(newBehaviourType);
        }

        private void SwitchBehaviourLocal(JoystickBehaviours newBehaviourType) {
            switch (newBehaviourType) {
                case JoystickBehaviours.Fixed:
                    _behaviour = new FixedJoysticBehaviour(this);
                    break;
                case JoystickBehaviours.Dynamic:
                    _behaviour = new DynamicJoysticBehaviour(this);
                    break;
                default:
                    Debug.LogError("Switch behaviour: target behaviour wasn't found! Did you forget to add it in switch case?");
                    return;
            }
            _behaviourType = newBehaviourType;
            SaveSystem.Instance.Data.JoystickBehaviour = _behaviourType;

            OnPointerMove();
        }

        /// <summary>
        /// Change parameters of joystic in reference resolution
        /// </summary>
        /// <param name="backgroundRadius">radius in pixels [use with reference resolution]</param>
        /// <param name="touchIconRadius">radius in pixels [use with reference resolution]</param>
        public void SetJoystickSize(float backgroundRadius, float touchIconRadius) {
            float canvasScalerCoef = 1f;
            if (_rootUI_CanvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand && _aspectFactorRefToUser > 1f) {
                canvasScalerCoef = _aspectFactorRefToUser;
            }

            _backgroundRadiusInPixels = backgroundRadius * _joystickParamsScaleFactor;

            //sizeDelta - width & height in pixels
            _stickBackground.sizeDelta = Vector2.one * (backgroundRadius * 2f) * canvasScalerCoef;
            _stick.sizeDelta = Vector2.one * (touchIconRadius * 2f) * canvasScalerCoef;
        }

        /// <summary>
        /// Takes current resolution positions and changes joystic position
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="touchPos"></param>
        internal void UpdatePositions(Vector2 circleCenter, Vector3 touchPos) {
            _stickBackground.anchoredPosition = circleCenter / _resolutionScaleFactor;
            _stick.anchoredPosition = touchPos / _resolutionScaleFactor;
        }

        private void OnApplicationFocus(bool focus) {
            if (focus == false && _isInProgress) {
                OnPointerUp();
            }
        }
    }
}
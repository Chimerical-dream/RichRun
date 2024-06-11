using Game.JoystickUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ChimeraGames
{
    public class PlayerInput : MonoBehaviour
    {
        private static EventSystem _eventSystem;

        public const int TotalTouchesSupported = 4;
        public static UnityEvent OnFirstTouch = new UnityEvent();

        public static UnityEvent OnTouchBegan = new UnityEvent();
        public static UnityEvent OnTouchEnded = new UnityEvent();
        public static UnityEvent OnTouchMoved = new UnityEvent();
        public static UnityEvent OnTouch = new UnityEvent();

        public static UnityEvent<int> On2AndMoreTouchBegan = new UnityEvent<int>();
        public static UnityEvent<int> On2AndMoreTouchEnded = new UnityEvent<int>();
        public static UnityEvent<int> On2AndMoreTouchMoved = new UnityEvent<int>();
        public static UnityEvent<int> On2AndMoreTouch = new UnityEvent<int>();

        public static bool IsOnTouch = false;
        public static bool[] IsOnMultiTouch = new bool[TotalTouchesSupported];
        public static bool IsMultiTouchActive;

        /// <summary>
        /// returns amount of pixels from start to current touch [user screen resolution]
        /// </summary>
        public static Vector3 MoveDelta;
        /// <summary>
        /// returns touch pos when it started [user screen resolution]
        /// </summary>
        public static Vector3 CenterTouchPos;
        /// <summary>
        /// returns current touch pos [user screen resolution]
        /// </summary>
        public static Vector3 LastTouchPos;
        /// <summary>
        /// returns frame-between touch move delta in pixels [user screen resolution]
        /// </summary>
        public static Vector3 TouchMoveDelta;

        /// <summary>
        /// returns amount of pixels from start to current touch [user screen resolution]
        /// </summary>
        public static Vector3[] MultiMoveDelta = new Vector3[TotalTouchesSupported];
        /// <summary>
        /// returns touch pos when it started [user screen resolution]
        /// </summary>
        public static Vector3[] MultiCenterTouchPos = new Vector3[TotalTouchesSupported];
        /// <summary>
        /// returns current touch pos [user screen resolution]
        /// </summary>
        public static Vector3[] MultiLastTouchPos = new Vector3[TotalTouchesSupported];
        /// <summary>
        /// returns frame-between touch move delta in pixels [user screen resolution]
        /// </summary>
        public static Vector3[] MultiTouchMoveDelta = new Vector3[TotalTouchesSupported];

        private static Touch _firstTouch;
        private static Touch _secondAndMoreTouch;

        private static bool _firstTouchWas = false;

        private static bool isMobile = false;   

        /// <summary>
        /// Reset to invoke OnFirstTouch again when player do next touch
        /// </summary>
        public static void ResetFirstTouch()
        {
            _firstTouchWas = false;
        }

        private void Awake()
        {
            GameManager.OnGameRestarted.AddListener(OnGameRestarted);
        }

        private void OnGameRestarted()
        {
            _firstTouchWas = false;
        }

        private static List<RaycastResult> _raycastResults;
        /// <summary>
        /// Returns true if touch pointed on UI element
        /// </summary>
        public static bool IsTouchOnUI
        {
            get
            {
                if (_eventSystem == null)
                {
                    _eventSystem = EventSystem.current;
                    _raycastResults = new List<RaycastResult>();
                    if (_eventSystem == null)
                    {
                        return false;
                    }
                }

                PointerEventData eventDataCurrentPosition = new PointerEventData(_eventSystem);
                eventDataCurrentPosition.position = LastTouchPos;
                _raycastResults.Clear();
                _eventSystem.RaycastAll(eventDataCurrentPosition, _raycastResults);
                return _raycastResults.Count > 0;
            }
        }

        public static RaycastResult GetCurrentTouchUIElement()
        {
            if (_eventSystem == null)
            {
                _eventSystem = EventSystem.current;
                _raycastResults = new List<RaycastResult>();
                if (_eventSystem == null)
                {
                    return default;
                }
            }

            PointerEventData eventDataCurrentPosition = new PointerEventData(_eventSystem);
            eventDataCurrentPosition.position = LastTouchPos;
            _raycastResults.Clear();
            _eventSystem.RaycastAll(eventDataCurrentPosition, _raycastResults);
            return _raycastResults.Count > 0 ? _raycastResults[0] : default;
        }

        public static bool IsMultiTouchOnUI(int touchIndex)
        {
            if (_eventSystem == null)
            {
                _eventSystem = EventSystem.current;
                _raycastResults = new List<RaycastResult>();
                if (_eventSystem == null)
                {
                    return false;
                }
            }

            PointerEventData eventDataCurrentPosition = new PointerEventData(_eventSystem);
            eventDataCurrentPosition.position = MultiLastTouchPos[touchIndex];
            _raycastResults.Clear();
            _eventSystem.RaycastAll(eventDataCurrentPosition, _raycastResults);
            return _raycastResults.Count > 0;
        }

        /// <summary>
        /// Resets <see cref="CenterTouchPos"/> to last touch (if there is no touch -> to zero) and <see cref="MoveDelta"/> to zero
        /// </summary>
        /// <param name="touchIndex">for touch [index 0 means first touch]</param>
        public static void ResetPositions(int touchIndex = 0)
        {
            if (touchIndex == 0)
            {
                if (Input.touchSupported)
                {
                    if (Input.touchCount > 0)
                    {
                        CenterTouchPos = Input.GetTouch(0).position;
                    }
                }
                else
                {
                    CenterTouchPos = Input.mousePosition;
                }

                MoveDelta = Vector2.zero;
            }
            else
            {
                if (Input.touchCount > touchIndex)
                {
                    MultiCenterTouchPos[touchIndex] = Input.GetTouch(touchIndex).position;
                    MultiMoveDelta[touchIndex] = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Resets <see cref="CenterTouchPos"/> to newCenter and <see cref="MoveDelta"/> to zero
        /// </summary>
        /// <param name="newCenter">where locate new touch center</param>
        /// <param name="touchIndex">for touch [index 0 means first touch]</param>
        public static void ResetPositions(Vector3 newCenter, int touchIndex = 0)
        {
            if (touchIndex == 0)
            {
                CenterTouchPos = newCenter;
                MoveDelta = Vector2.zero;
            }
            else
            {
                if (Input.touchCount > touchIndex)
                {
                    MultiCenterTouchPos[touchIndex] = newCenter;
                    MultiMoveDelta[touchIndex] = Vector3.zero;
                }
            }
        }

        private static void HandleTouches()
        {
            int touchesCount = Input.touchCount;
            if (touchesCount > 0)
            {
                _firstTouch = Input.GetTouch(0);
                Vector3 touchPos = _firstTouch.position;

                if (_firstTouch.phase == TouchPhase.Began)
                {
                    ResetPositions();
                    IsOnTouch = true;
                    LastTouchPos = touchPos;

                    if (!_firstTouchWas)
                    {
                        OnFirstTouch.Invoke();
                        _firstTouchWas = true;
                    }

                    OnTouchBegan.Invoke();
                }
                else if (_firstTouch.phase == TouchPhase.Ended)
                {

                    IsOnTouch = false;
                    OnTouchEnded.Invoke();
                    MoveDelta = Vector2.zero;
                    TouchMoveDelta = Vector2.zero;
                }

                TouchMoveDelta = touchPos - LastTouchPos;
                LastTouchPos = touchPos;
                MoveDelta = LastTouchPos - CenterTouchPos;

                if (_firstTouch.phase == TouchPhase.Moved)
                {
                    OnTouchMoved.Invoke();
                }

                OnTouch.Invoke();

                if (touchesCount > 1)
                {
                    for (int i = 1; i < touchesCount && i < TotalTouchesSupported; i++)
                    {
                        _secondAndMoreTouch = Input.GetTouch(i);
                        touchPos = _secondAndMoreTouch.position;
                        if (_secondAndMoreTouch.phase == TouchPhase.Began)
                        {
                            IsOnMultiTouch[i] = true;
                            ResetPositions(i);
                            MultiLastTouchPos[i] = touchPos;
                            On2AndMoreTouchBegan.Invoke(i);
                            IsMultiTouchActive = true;
                        }

                        else if (_secondAndMoreTouch.phase == TouchPhase.Ended)
                        {
                            IsOnMultiTouch[i] = false;
                            MultiMoveDelta[i] = Vector2.zero;
                            MultiTouchMoveDelta[i] = Vector2.zero;

                            if (touchesCount == 2)
                            {
                                IsMultiTouchActive = false;
                            }
                            On2AndMoreTouchEnded.Invoke(i);
                        }

                        MultiTouchMoveDelta[i] = touchPos - MultiLastTouchPos[i];
                        MultiLastTouchPos[i] = touchPos;
                        MultiMoveDelta[i] = MultiLastTouchPos[i] - MultiCenterTouchPos[i];

                        if (_secondAndMoreTouch.phase == TouchPhase.Moved)
                        {
                            On2AndMoreTouchMoved.Invoke(i);
                        }

                        On2AndMoreTouch.Invoke(i);
                    }
                }
                else if (IsMultiTouchActive)
                {
                    IsMultiTouchActive = false;
                    for (int i = 1; i < TotalTouchesSupported; i++)
                    {
                        if (IsOnMultiTouch[i])
                        {
                            IsOnMultiTouch[i] = false;
                            On2AndMoreTouchEnded.Invoke(i);
                        }
                    }
                }
            }
        }
        private static void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0)) //LMB
            {
                ResetPositions();
                IsOnTouch = true;
                LastTouchPos = Input.mousePosition;

                if (!_firstTouchWas)
                {
                    OnFirstTouch.Invoke();
                    _firstTouchWas = true;
                }

                OnTouchBegan.Invoke();
            }
            else if (Input.GetMouseButtonUp(0))
            {

                IsOnTouch = false;
                OnTouchEnded.Invoke();
                MoveDelta = Vector2.zero;
                TouchMoveDelta = Vector2.zero;
            }

            if (Input.GetMouseButton(0))
            {
                TouchMoveDelta = Input.mousePosition - LastTouchPos;
                LastTouchPos = Input.mousePosition;
                MoveDelta = LastTouchPos - CenterTouchPos;
                if (MoveDelta.x != 0 || MoveDelta.y != 0)
                {
                    OnTouchMoved.Invoke();
                }
                OnTouch.Invoke();
            }
        }

        private void Update()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouches();
            }
#if (!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR
            else
            {
                //#if UNITY_WEBGL
                //                // for webgl there is kind of bug (double touch up events)
                //                // Application.platform == RuntimePlatform.WebGLPlayer is both for pc and mobile
                //                // need to implement someday https://stackoverflow.com/questions/60806966/unity-webgl-check-if-mobile
                //                // and don't call HandleMouse() for mobile devices
                //#endif

                HandleMouse();
            }
#endif
        }
    }

}

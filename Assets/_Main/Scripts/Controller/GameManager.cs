using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

namespace ChimeraGames
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// true -> win
        /// </summary>
        public static UnityEvent<bool> OnGameOver = new UnityEvent<bool>();
        public static UnityEvent OnGameRestarted = new UnityEvent(), OnGamePlayStarted = new UnityEvent(),
            OnRevive = new UnityEvent();
        public static bool IsGameOver = false;
        public static bool GameStarted = false;
        public static PlayerController PlayerController;

        [SerializeField]
        private GlobalSettings settings;
        public static GlobalSettings Settings => instance.settings;

        private static GameManager instance;

        private void Awake()
        {
            IsGameOver = false;
            GameStarted = false;
            instance = this;

            VictoryWindow.OnScreenDismissed.AddListener(RestartGame); //TODO: load next level;

            LossWindow.OnScreenDismissed.AddListener(RestartGame);
            LossWindow.OnGetRevive.AddListener(Revive);

            PlayerInput.OnFirstTouch.AddListener(() => { OnGamePlayStarted.Invoke(); GameStarted = true; });

            PlayerController.FinishReached.AddListener(() => GameOver(true));
            MoneyManager.OnOutOfMoney.AddListener(() => GameOver(false));
        }

        private void GameOver(bool win)
        {
            OnGameOver.Invoke(win);
            IsGameOver = true;
        }

        private void Revive()
        {
            IsGameOver = false;
            OnRevive.Invoke();
        }

        private void RestartGame()
        {
            IsGameOver = false;
            OnGameRestarted.Invoke();
        }
    }
}
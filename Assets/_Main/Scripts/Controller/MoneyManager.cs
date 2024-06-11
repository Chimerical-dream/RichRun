using ChimeraGames;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;

public class MoneyManager : MonoBehaviour
{
    public static UnityEvent<int> OnMoneyUpdated = new UnityEvent<int>();
    public static UnityEvent<int> OnLevelCashUpdated = new UnityEvent<int>();
    public static UnityEvent OnOutOfMoney = new UnityEvent();

    public static MoneyManager instance;

    [SerializeField]
    private int startMoneyAmount = 0, startLevelCash = 40;

    [ShowNonSerializedField]
    private int levelCash, money;
    public int Money
    {
        get
        {
            return money;
        }
        set
        {
            money = value;
            OnMoneyUpdated.Invoke(money);
        }
    }

    public int LevelCash => levelCash;

    private void Awake()
    {
        OnGameRestarted();
        money = PlayerPrefs.GetInt("Money", startMoneyAmount);

        OnMoneyUpdated.AddListener((int x) => PlayerPrefs.SetInt("Money", money));

        instance = this;

        GameManager.OnGameOver.AddListener(OnGameOver);
        GameManager.OnGameRestarted.AddListener(OnGameRestarted);

        CollectablePositive.OnPositiveCollectablePickUp.AddListener(OnPositivePickUp);
        CollectableNegative.OnNegativeCollectablePickUp.AddListener(OnNegativePickUp);

        VictoryWindow.OnScreenDismissed.AddListener(OnDefaultRewardAccepted);
    }

    private void OnDefaultRewardAccepted()
    {
        Money += levelCash;
    }

    private void Start()
    {
        OnLevelCashUpdated.Invoke(levelCash);
        OnMoneyUpdated.Invoke(money);
    }

    private void OnGameRestarted()
    {
        levelCash = startLevelCash;
    }

    private void OnNegativePickUp(int v)
    {
        levelCash -= v;
        OnLevelCashUpdated.Invoke(levelCash);
        if(levelCash <= 0)
        {
            OnOutOfMoney.Invoke();
        }
    }

    private void OnPositivePickUp(int v)
    {
        levelCash += v;
        OnLevelCashUpdated.Invoke(levelCash);
    }

    private void OnGameOver(bool isWin)
    {
        if (isWin)
        {
            return;
        }
    }
}

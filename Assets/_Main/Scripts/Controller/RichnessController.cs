using ChimeraGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RichnessController : MonoBehaviour
{
/// <summary>
/// (from, to)
/// </summary>
    public static UnityEvent<RichLevels, RichLevels> OnLevelChange = new UnityEvent<RichLevels, RichLevels>();
    private RichLevels curLevel = RichLevels.Hobo;

    private void Awake()
    {
        MoneyManager.OnLevelCashUpdated.AddListener(OnMoneyUpdated);
    }

    private void Start()
    {
        OnLevelChange.Invoke(curLevel, curLevel);
    }

    private void OnMoneyUpdated(int money)
    {
        var newLevel = GameManager.Settings.RichLevelSettings.GetLevelByMoney(money);
        if (newLevel == curLevel)
        {
            return;
        }

        OnLevelChange.Invoke(curLevel, newLevel);

        curLevel = newLevel;
    }
}

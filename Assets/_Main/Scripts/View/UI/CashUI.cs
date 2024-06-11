using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI totalMoney, levelCash;
    [SerializeField]
    private string currency = "$";

    private void Awake()
    {
        MoneyManager.OnMoneyUpdated.AddListener(UpdateTotalMoney);
        MoneyManager.OnLevelCashUpdated.AddListener(UpdateLevelCash);
    }

    private void UpdateLevelCash(int v)
    {
        levelCash.text = v + currency;
    }

    private void UpdateTotalMoney(int v)
    {
        totalMoney.text = v.ToString();
    }
}

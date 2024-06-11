using ChimeraGames;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class RichLevelUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI text;
    [SerializeField]
    private Image fill;
    [SerializeField]
    private float punchScale = 1.5f;

    private void Awake()
    {
        RichnessController.OnLevelChange.AddListener(OnLevelChange);
        MoneyManager.OnLevelCashUpdated.AddListener(OnMoneyUpdated);
        GameManager.OnGameOver.AddListener(OnGameOver);
    }

    private void OnGameOver(bool arg0)
    {
        gameObject.SetActive(false);
    }

    private void OnMoneyUpdated(int m)
    {
        fill.fillAmount = ((float) m) / GameManager.Settings.RichLevelSettings.GetLevelSettings(GameManager.Settings.RichLevelSettings.maxLevel).money;
    }

    private void OnLevelChange(RichLevels oldL, RichLevels newL)
    {
        var settings = GameManager.Settings.RichLevelSettings.GetLevelSettings(newL);

        fill.DOColor(settings.nameColor, .1f);
        text.DOColor(settings.nameColor, .1f);
        text.text = settings.inGameName;

        text.transform.DOPunchScale(Vector3.one * punchScale, .5f, 1, 0);
    }
}

using ChimeraGames;
using DG.Tweening;
using Game.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStart : PopUp
{
    /// <summary>
    /// bool -> isLeft
    /// </summary>
    public static UnityEvent<bool> OnBtnPressed = new UnityEvent<bool>();

    private Cooldown btnCooldown = new Cooldown(.5f);

    private void Awake()
    {

        GameManager.OnGamePlayStarted.AddListener(Hide);
        GameManager.OnGameRestarted.AddListener(() =>
        {
            canvasGroup.alpha = 1;
            gameObject.SetActive(true);
        });
    }

    public void OnLeftBtnPressed()
    {
        if (!btnCooldown.IsElapsed)
        {
            return;
        }
        btnCooldown.Reset();

        OnBtnPressed.Invoke(true);
    }

    public void OnRightBtnPressed()
    {
        if (!btnCooldown.IsElapsed)
        {
            return;
        }
        btnCooldown.Reset();

        OnBtnPressed.Invoke(false);
    }
}
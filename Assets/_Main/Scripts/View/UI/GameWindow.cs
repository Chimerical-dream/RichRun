using ChimeraGames;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameWindow : PopUp
{

    private void Awake()
    {
        GameManager.OnGameOver.AddListener(OnGameOver);
    }

    private void OnGameOver(bool win)
    {
        Hide();
    }
}

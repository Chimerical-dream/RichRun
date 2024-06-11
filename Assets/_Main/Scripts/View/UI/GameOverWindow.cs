using ChimeraGames;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LossWindow : PopUp
{
    public static UnityEvent OnScreenShown = new UnityEvent(), OnScreenDismissed = new UnityEvent(),
        OnGetRevive = new UnityEvent();

    [SerializeField]
    private float screenDelay = 1.5f;
    [SerializeField]
    private GameObject dismissPrompt;

    private void Awake()
    {

        gameObject.SetActive(false);

        GameManager.OnGameOver.AddListener(OnGameOver);
    }

    private void OnGameOver(bool win)
    {
        if (win)
        {
            return;
        }
        Show();
    }

    public void OnRvClick()
    {
    }


    public override void Show()
    {
        base.Show();

        DOVirtual.DelayedCall(fadeIn, () =>
        {
            dismissPrompt.SetActive(true);

            OnScreenShown.Invoke();

        });
    }

    public void OnDismissClick()
    {
        Hide();
        SceneManager.LoadScene(1);
        DOVirtual.DelayedCall(fadeOut * .8f, () => OnScreenDismissed.Invoke());
    }
}

using DG.Tweening;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    [SerializeField]
    protected CanvasGroup canvasGroup;
    [SerializeField]
    protected float duration = 1, fadeIn = .2f, fadeOut = .2f;
    
    public virtual void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, fadeIn).OnComplete(() => 
        {
            if (duration > 0)
            {
                canvasGroup.DOFade(0f, fadeOut).SetDelay(duration).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
            }
        });
    }

    public virtual void Hide()
    {
        if(duration <= 0)
        {
            canvasGroup.DOFade(0f, fadeOut).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}

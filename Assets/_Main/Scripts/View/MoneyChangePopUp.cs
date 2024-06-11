using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

public class MoneyChangePopUp : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI positive, negative;
    private Vector3 startPosPositive, startPosNegative;
    private int curReward, curWithdraw;

    [SerializeField]
    private float bounceScale, flyUpTime, flyUpDist, fadeDelay;

    private void Awake()
    {
        positive.gameObject.SetActive(false);
        negative.gameObject.SetActive(false);
        startPosNegative = negative.transform.localPosition;
        startPosPositive = positive.transform.localPosition;

        CollectablePositive.OnPositiveCollectablePickUp.AddListener(OnPositivePickUp);
        CollectableNegative.OnNegativeCollectablePickUp.AddListener(OnNegativePickUp);
    }

    private void OnPositivePickUp(int v)
    {
        if (positive.gameObject.activeSelf)
        {
            curReward += v;
        }
        else
        {
            curReward = v;
        }
        positive.text = "+" + curReward.ToString() + " $";
        DoAnimWithCancelling(positive, startPosPositive);
    }

    private void OnNegativePickUp(int v)
    {
        if (negative.gameObject.activeSelf)
        {
            curWithdraw += v;
        }
        else
        {
            curWithdraw = v;
        }
        negative.text = "-" + curWithdraw.ToString() + " $";
        DoAnimWithCancelling(negative, startPosNegative);
    }

    private void DoAnimWithCancelling(TextMeshProUGUI text, Vector3 startPos)
    {
        if (text.gameObject.activeSelf)
        {
            text.DOKill();
            text.transform.DOKill();
            text.DOFade(1, .08f);
            text.transform.DOLocalMove(startPos, .08f).OnComplete(() => DoAnim(text, startPos));
            return;
        }
        text.gameObject.SetActive(true);
        text.transform.localScale = Vector3.zero * 0.1f;
        text.transform.DOScale(Vector3.one, .1f).OnComplete(() => DoAnim(text, startPos));
    }

    private void DoAnim(TextMeshProUGUI text, Vector3 startPos)
    {
        if (text.transform.localScale.x > bounceScale) text.transform.localScale = Vector3.one;

        text.transform.DOPunchScale(Vector3.one * bounceScale, .2f, 1, 0);
        text.transform.DOLocalMoveY(startPos.y + flyUpDist, flyUpTime).SetEase(Ease.OutSine);
        text.DOFade(0.05f, Mathf.Max(0.2f, flyUpTime - fadeDelay - .2f)).SetDelay(fadeDelay)
            .OnComplete(() => text.gameObject.SetActive(false));
    }
}

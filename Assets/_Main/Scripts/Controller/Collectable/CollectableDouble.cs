using DG.Tweening;
using UnityEngine;

public class CollectableDouble : CollectableBase
{
    [SerializeField]
    bool isPositive;
    [SerializeField]
    private int value;
    [SerializeField]
    private Transform parent;


    public override void Interact()
    {
        parent.DOScale(Vector3.zero, .1f).OnComplete(() => parent.gameObject.SetActive(false));

        if (isPositive)
        {
            CollectablePositive.OnPositiveCollectablePickUp.Invoke(value);
        }
        else
        {
            CollectableNegative.OnNegativeCollectablePickUp.Invoke(value);
        }
    }
}

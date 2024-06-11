using UnityEngine;
using UnityEngine.Events;

public class CollectableNegative : CollectableBase
{
    public static UnityEvent<int> OnNegativeCollectablePickUp = new UnityEvent<int>();
    [SerializeField]
    private Vector2Int minMaxWithdraw;

    public override void Interact()
    {
        if (didInteract)
        {
            return;
        }

        base.Interact();
        OnNegativeCollectablePickUp.Invoke(Random.Range(minMaxWithdraw.x, minMaxWithdraw.y + 1));
    }
}

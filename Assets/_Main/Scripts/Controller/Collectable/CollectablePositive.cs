using ChimeraGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectablePositive : CollectableBase
{
    public static UnityEvent<int> OnPositiveCollectablePickUp = new UnityEvent<int>();
    [SerializeField]
    private Vector2Int minMaxReward;

    public override void Interact()
    {
        if (didInteract)
        {
            return;
        }

        base.Interact();
        OnPositiveCollectablePickUp.Invoke(Random.Range(minMaxReward.x, minMaxReward.y + 1));
    }
}

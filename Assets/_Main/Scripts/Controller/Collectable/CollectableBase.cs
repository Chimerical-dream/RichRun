using DG.Tweening;
using UnityEngine;

public class CollectableBase : MonoBehaviour
{
    protected bool didInteract = false;

    public virtual void Interact()
    {
        if (didInteract)
        {
            return;
        }
        transform.DOMoveY(transform.position.y + .7f, .08f);
        transform.DOScale(Vector3.zero, .1f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        didInteract = true;
    }
}

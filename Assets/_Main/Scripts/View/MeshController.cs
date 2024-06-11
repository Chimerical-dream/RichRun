using ChimeraGames;
using DG.Tweening;
using System;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    [SerializeField]
    private Transform holder;
    [SerializeField]
    private AnimationScript animation;
    public AnimationScript Animation => animation;
    [SerializeField]
    RichnessLevelMesh[] meshes;

    private Transform t;
    public Transform T => t;

    private float offsetX;

    private void Awake()
    {
        t = transform;

        GameManager.OnGameOver.AddListener(OnGameOver);
        GameManager.OnGamePlayStarted.AddListener(OnGamePlayStarted);
        RichnessController.OnLevelChange.AddListener(OnRichLevelChange);

        CollectableNegative.OnNegativeCollectablePickUp.AddListener(OnNegativeCollectablePickUp);
    }

    private void OnGameOver(bool win)
    {
        animation.SetAnimationState(win ? AnimationStates.Victory : AnimationStates.Loss);
    }


    private void OnNegativeCollectablePickUp(int arg0)
    {
        holder.DOKill();
        animation.SetAnimationState(AnimationStates.Hit);
        DOVirtual.DelayedCall(.2f, () =>
        {
            if (GameManager.IsGameOver) animation.SetAnimationState(AnimationStates.Loss);
        });
    }

    private void OnRichLevelChange(RichLevels oldL, RichLevels newL)
    {
        if(newL > oldL && GameManager.GameStarted) //upgrade
        {
            animation.SetAnimationState(AnimationStates.Upgrade);
            float t = 0.8f;
            holder.DOLocalRotate(Vector3.up * 120f, t / 3f).SetEase(Ease.InSine).OnComplete(() => holder.DOLocalRotate(Vector3.up * 240f, t / 3f)
            .OnComplete(() => holder.DOLocalRotate(Vector3.up * 359.9f, t / 3f).SetEase(Ease.OutSine)));
            DOVirtual.DelayedCall(t * 0.8f, () => animation.SetAnimationState(AnimationStates.Walk01)).SetTarget(holder);
        }

        animation.Animator.SetFloat("Rich", ((float)newL) / (float)GameManager.Settings.RichLevelSettings.maxLevel);

        SetMesh(newL);
    }

    private void SetMesh(RichLevels lvl)
    {
        foreach(var m in meshes)
        {
            if(m.name == lvl)
            {
                m.mesh.SetActive(true);
                continue;
            }
            m.mesh.SetActive(false);
        }
    }


    private void OnGamePlayStarted()
    {
        animation.SetAnimationState(AnimationStates.Walk01);
    }

    private void Update()
    {
    }

}
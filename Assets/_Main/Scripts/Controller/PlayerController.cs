using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using ChimeraGames;
using Dreamteck.Splines;
using System;

public class PlayerController : MonoBehaviour
{
    public static UnityEvent FinishReached = new UnityEvent();

    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private MeshController meshController;

    [SerializeField]
    private SplineFollower splineFollower;
    [SerializeField]
    private SplineFollower camFollow;
    private float Speed
    {
        get
        {
            return splineFollower.followSpeed;
        }
        set
        {
            splineFollower.followSpeed = value;
            camFollow.followSpeed = value;
        }
    }

    private Vector2 Offset
    {
        get
        {
            return splineFollower.motion.offset;
        }
        set
        {
            splineFollower.motion.offset = value;
            camFollow.motion.offset = value;
        }
    }

    private Transform t;
    public Transform T => t;
    [Space(10)]
    [SerializeField]
    private float constantSpeed, sideSpeed;

    [SerializeField]
    private Quaternion tiltLeft, tiltRight;
    [SerializeField]
    private float maxOffsetX;
    [Space(10)]
    [SerializeField]
    private ParticleSystem positiveFx, negativeFx;

    private float touchStartOffset, normalizedOffset, oldNormalizedOffset;
    private float rotation = .5f; //0f -> left, 1f -> right

    private void Awake()
    {
        camFollow.transform.parent = null;
        camFollow.SetDistance(1f + (float)(splineFollower.GetPercent() * splineFollower.spline.CalculateLength()));
        GameManager.PlayerController = this;

        PlayerInput.OnTouchMoved.AddListener(OnTouchMoved);
        PlayerInput.OnTouchBegan.AddListener(OnTouchBegan);

        GameManager.OnGameOver.AddListener(OnGameOver);
        GameManager.OnGameRestarted.AddListener(OnGameRestarted);
        GameManager.OnRevive.AddListener(OnRevive);
        GameManager.OnGamePlayStarted.AddListener(OnGamePlayStarted);

        CollectablePositive.OnPositiveCollectablePickUp.AddListener(OnPositivePickUp);
        CollectableNegative.OnNegativeCollectablePickUp.AddListener(OnNegativePickUp);

        splineFollower.onEndReached += OnFinishReached;

        t = transform;
    }

    private void Start()
    {

        OnGameRestarted();
        this.enabled = false;
    }

    private void OnRevive()
    {
        this.enabled = true;

        t.rotation = Quaternion.identity;
    }


    private void OnGameRestarted()
    {
    }

    private void OnGamePlayStarted()
    {
        this.enabled = true;
        meshController.Animation.Animator.SetFloat("Speed", constantSpeed / 2f);
        DOVirtual.Float(0, constantSpeed, .3f, (float v) => Speed = v);
    }

    private void LateUpdate()
    {
        float dMove = -normalizedOffset + oldNormalizedOffset;
        rotation = Mathf.Clamp01(rotation + dMove); //increasing rotation
        meshController.T.localRotation = Quaternion.Lerp(meshController.T.localRotation, Quaternion.Lerp(tiltLeft, tiltRight, rotation), Time.deltaTime * 5f);

        rotation = Mathf.Lerp(rotation, 0.5f, Time.deltaTime * 4); // decaying rotation

        oldNormalizedOffset = normalizedOffset;
    }

    private void FixedUpdate()
    {
    }


    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.IsGameOver)
        {
            return;
        }

        if (other.CompareTag("Collectable"))
        {
            other.GetComponent<CollectableBase>().Interact();
        }
    }


    private void OnPositivePickUp(int v)
    {
        positiveFx.Play();
    }

    private void OnNegativePickUp(int v)
    {
        negativeFx.Play();
    }

    private void OnTouchBegan()
    {
        touchStartOffset = normalizedOffset;
    }

    private void OnTouchMoved()
    {
        if (!this.enabled)
        {
            return;
        }

        normalizedOffset = CustomMath.ClampPlusMinus1(touchStartOffset + PlayerInput.MoveDelta.x / Screen.width * 2);
        float offsetX = Mathf.Lerp(Offset.x, normalizedOffset * maxOffsetX, sideSpeed * Time.deltaTime * 10);
        Offset = new Vector2(offsetX, Offset.y);
    }


    private void OnFinishReached(double arg)
    {
        this.enabled = false;
        camFollow.enabled = false;
        camFollow.transform.DOLocalRotate(Vector3.up * 360f, 14f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental).SetRelative();
        FinishReached.Invoke();
    }


    private void OnGameOver(bool win)
    {
        this.enabled = false;
        Speed = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using ChimeraGames;
using System;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera startCamera, gameCamera;
    [SerializeField]
    private Camera mainCamera;
    public Camera Camera => mainCamera;
    [SerializeField]
    private Transform camFollow;
    private Transform playerT;

    private static CameraController instance;
    public static CameraController Instance => instance;

    private void Awake()
    {
        instance = this;

        GameManager.OnGameRestarted.AddListener(OnGameRestarted);
        GameManager.OnGamePlayStarted.AddListener(OnGamePlayStarted);
    }

    private void OnGamePlayStarted()
    {
        startCamera.Priority = 0;
        gameCamera.Priority = 10000;
    }

    private void Start()
    {
        playerT = GameManager.PlayerController.T;
    }

    private void OnGameRestarted()
    {
        startCamera.Priority = 10000;
        gameCamera.Priority = 0;
    }

    public static bool IsSphereVisibleOnCamera(Vector3 sphereCenter, float sphereRadius)
    {

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(instance.mainCamera);
        Bounds sphereBounds = new Bounds(sphereCenter, Vector3.one * sphereRadius * 2);

        return GeometryUtility.TestPlanesAABB(frustumPlanes, sphereBounds);

    }
}

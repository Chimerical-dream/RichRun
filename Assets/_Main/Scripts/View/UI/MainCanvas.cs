using ChimeraGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas instance;
    public static RectTransform t;
    [SerializeField]
    private CanvasScaler canvasScaler;

    private void Awake()
    {
        instance = this;
        t = (RectTransform)transform;
    }
}

using DG.Tweening;
using Game.Core;
using Game.UI;
using UnityEngine;

public class MainMenuCanvas : UICanvasController {
    [SerializeField] private float _disableCanvasDuration = 0.5f;

    // btn event
    public void OnPlayBtnClick() {
        UIManager.instance.DisableActiveCanvas(_disableCanvasDuration);

        DOVirtual.DelayedCall(_disableCanvasDuration,
            () => SceneSystem.Instance.LoadSceneSwitch(SceneSystem.GAME_SCENE, SceneTypes.LevelScene, LoadScreens.CutoutCube, LoadScreens.FadeInOut));
    }
}

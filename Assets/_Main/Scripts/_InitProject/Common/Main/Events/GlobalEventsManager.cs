using UnityEngine.Events;

/// <summary>
/// Contains all global events
/// </summary>
public static class GlobalEventsManager
{
    //!-!-!
    //UnityEvent subscription will be automaticaly deleted if obj destroyed (obj subscribed to event before)
    //so we use them to prevent possible issues
    //!-!-!

    public static UnityEvent<GameOverEvent> OnGameOver = new UnityEvent<GameOverEvent>();
    public static void GameOver(GameOverEvent data) {
        OnGameOver.Invoke(data);
    }

    public static UnityEvent<TimeEvent> OnTimeFreezeChanged = new UnityEvent<TimeEvent>();
    public static void ChangeTimeFreeze(TimeEvent data) {
        OnTimeFreezeChanged.Invoke(data);
    }

    /// <summary>
    /// invokes when you return to main menu from level
    /// </summary>
    public static UnityEvent OnMainMenuReturn = new UnityEvent();
    /// <summary>
    /// Call it when you return to main menu from level
    /// </summary>
    public static void ReturnMainManuFromLevel() {
        OnMainMenuReturn.Invoke();
    }

    /// <summary>
    /// invokes before level scene start loading
    /// </summary>
    public static UnityEvent OnLevelSceneStartLoading = new UnityEvent();
    public static void StartLevelSceneLoading() {
        OnLevelSceneStartLoading.Invoke();
    }

    /// <summary>
    /// invokes when level load is finished (it includes script initialization and loadScreen disappear)
    /// </summary>
    public static UnityEvent OnLevelLoadFinished = new UnityEvent();
    public static void FinishLevelLoad() {
        OnLevelLoadFinished.Invoke();
    }

    /// <summary>
    /// invokes when level load started (not scene)
    /// </summary>
    public static UnityEvent OnLevelLoadStarted = new UnityEvent();
    public static void StartLevelLoad() {
        OnLevelLoadStarted.Invoke();
    }

    /// <summary>
    /// invokes when player died
    /// </summary>
    public static UnityEvent OnPlayerDeath = new UnityEvent();
    public static void PlayerDeath() {
        OnPlayerDeath.Invoke();
    }

    /// <summary>
    /// invokes when player is revived
    /// </summary>
    public static UnityEvent OnPlayerRevive = new UnityEvent();

    public static void PlayerRevive() {
        OnPlayerRevive.Invoke();
    }
}

namespace Game.UI {
    public interface ICanvasTransition {
        public void Init(UICanvasController controller);
        public bool IsRunning { get; }
        public void StopTransition();
        public void DoFade(bool active, float fadeDuration);
    }
}
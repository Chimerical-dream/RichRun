namespace DG.Tweening {

    public static class DOTweenCustomExtentions {
        public static void SafeKill(this Tween tween, bool complete = false) {
            if (tween != null && tween.active) {
                tween.Kill(complete);
            }
        }
    }
}
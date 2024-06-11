using UnityEngine;

namespace Game.InputSystem {
    public class InputUpdater : MonoBehaviour {
        public static InputUpdater instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }
        }

        private void Update() {
            PlayerInput.Update();
        }
    }
}
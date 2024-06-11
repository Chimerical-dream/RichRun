using TMPro;
using UnityEngine.UI;

namespace Game.UI {
    public class TMPTextBridge : IText {
        private TMP_Text _component;

        public TMPTextBridge(TMP_Text component) {
            _component = component;
        }

        public MaskableGraphic Graphic => _component;

        public string text { 
            get => _component.text; 
            set => _component.text = value; 
        }

        public float preferredHeight => _component.preferredHeight;
        public float preferredWidth => _component.preferredWidth;
    }
}

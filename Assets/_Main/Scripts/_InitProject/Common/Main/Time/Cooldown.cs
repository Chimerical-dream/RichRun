using UnityEngine;

namespace Game.Core {

    /// @Author - Dzmitry Sudarau
    /// <summary>
    /// Is used to check delay
    /// </summary>
    [System.Serializable]
    public class Cooldown {
        public Cooldown(float time) {
            this._time = time;
        }

        public float Time => _time;
        public bool IsElapsed => UnityEngine.Time.time - _lastTimeStamp > _time;
        public float ElapsedSeconds => UnityEngine.Time.time - _lastTimeStamp;

        public void Reset() {
            _lastTimeStamp = UnityEngine.Time.time;
        }
		
		public void CompleteCooldown() {
            _lastTimeStamp = UnityEngine.Time.time - _time - 0.1f;
		}

        /// <summary>
        /// Sets new cooldown but doesn't reset it
        /// </summary>
        public void SetCooldown(float time) {
            _time = time;
        }

        [SerializeField] private float _time;
        private float _lastTimeStamp = 0;
    }
}
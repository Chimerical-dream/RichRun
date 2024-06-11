//using Alchemy.Inspector;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core {

    /// <summary>
    /// pool object for FXs with Particle System
    /// </summary>
	[AddComponentMenu("Common/Pool/Particle System Pool Object")]
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemPoolObject : PoolObject {
        [SerializeField]
        private ParticleSystem _baseFX;
        public ParticleSystem BaseFX {
            get {
                if (_baseFX == null) {
                    Debug.LogWarning($"[{gameObject.name}] No base FX serialized!");
                    return GetComponent<ParticleSystem>();
                }
                return _baseFX;
            }
        }

        public float Duration => BaseFX.main.duration;
        [Space, InfoBox("'Stop Action' value in main settings must be None or Callback" +
                                          "\nIf you use it in auto fill pool, or pool will always expand")]
        public bool ReturnToPoolOnPlayed = true;

        public override void OnObjectReuse() {
            base.OnObjectReuse();
            _baseFX.Play();
            if (ReturnToPoolOnPlayed) Destroy(Duration);
        }
    }
}
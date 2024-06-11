using DG.Tweening;
using Game.Core;
using UnityEngine;

namespace Game.Audio {
    public class Sound : PoolObject {
        [SerializeField] protected AudioSource _source;
        public bool IsPlaying => _source.isPlaying;
        public bool Loop { get => _source.loop; set => _source.loop = value; }
        public float Volume { get => _source.volume; set => _source.volume = Mathf.Clamp01(value); }
        public float Pitch { get => _source.pitch; set => _source.pitch = value; }
        /// <summary>
        /// Is sound 2d (value=true)<br>
        /// Set to false and it will be 3d</br>
        /// </summary>
        public bool Is2D { get => _source.spatialBlend < 0.01f; set => _source.spatialBlend = value ? 0f : 1f; }

        private float _soundInitVolume;

        private Tween _volumeChangeTween;
        private Tween _playbackEndCheckTween;
        private TweenCallback _onSoundPlaybackEnd;

        private const float CLIPPING_AVOID_ADD_DELAY = 0.05f;

        public void SetPosition(Vector3 worldPosition) {
            transform.position = worldPosition;
        }

        /// <summary>
        /// Add callback on sound played fully [Stop() will not execute this callback]
        /// </summary>
        public void AppendNonPersistantSoundEndCallback(TweenCallback callback) {
            _onSoundPlaybackEnd += callback;
        }

        /// <summary>
        /// Remove all callbacks on sound played fully [Stop() will not execute this callback]
        /// </summary>
        public void ClearAllSoundEndCallbacks() {
            _onSoundPlaybackEnd = null;
        }

        private void ExecuteSoundEndCallbacks() {
            _onSoundPlaybackEnd.Invoke();
            _onSoundPlaybackEnd = null;
        }

        public void Init(SoundAudioClip soundData, float pitch = 1f) {
            _source.clip = soundData.AudioClip;
            _soundInitVolume = soundData.Volume;

            if (soundData.Is2D) {
                _source.spatialBlend = 0f;
            }
            else {
                _source.spatialBlend = 1f;
                _source.dopplerLevel = soundData.DopplerLevel;
                _source.spread = soundData.Spread;
                _source.minDistance = soundData.MinDistance;
                _source.maxDistance = soundData.MaxDistance;
                _source.rolloffMode = soundData.VolumeRolloff;
            }

            _source.volume = soundData.Volume;
            _source.pitch = pitch;
            _source.outputAudioMixerGroup = soundData.MixerGroup;
        }

        public void Play() {
            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }
            if (_source.volume != _soundInitVolume) {
                _source.volume = _soundInitVolume;
            }

            _source.Play();

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                    _playbackEndCheckTween.Kill();
                }
                _playbackEndCheckTween = DOVirtual.DelayedCall(_source.clip.length / _source.pitch, ExecuteSoundEndCallbacks);
            }
        }

        public void Play(float fadeInDurationSeconds) {
            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }
            _source.volume = 0f;
            _source.Play();
            _volumeChangeTween = _source.DOFade(_soundInitVolume, fadeInDurationSeconds);

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                    _playbackEndCheckTween.Kill();
                }
                _playbackEndCheckTween = DOVirtual.DelayedCall(_source.clip.length / _source.pitch, ExecuteSoundEndCallbacks);
            }
        }

        public void PlayAndDestroy() {
            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }
            if (_source.volume != _soundInitVolume) {
                _source.volume = _soundInitVolume;
            }

            _source.Play();

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                    _playbackEndCheckTween.Kill();
                }
                _playbackEndCheckTween = DOVirtual.DelayedCall(_source.clip.length / _source.pitch, ExecuteSoundEndCallbacks);
            }

            Destroy(_source.clip.length / _source.pitch + CLIPPING_AVOID_ADD_DELAY);
        }

        public void Stop() {
            if (_source.isPlaying == false) {
                return;
            }

            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }

            if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                _playbackEndCheckTween.Kill();
            }

            _source.Stop();
        }

        public void Stop(float fadeOutDurationSeconds, TweenCallback callback, bool stopSourceOnFadeEnd = true) {
            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }

            if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                _playbackEndCheckTween.Kill();
            }

            if (!_source.isPlaying) {
                callback?.Invoke();
                return;
            }

            if (stopSourceOnFadeEnd) {
                callback += _source.Stop;
            }

            _volumeChangeTween = _source.DOFade(0f, fadeOutDurationSeconds)
                .OnComplete(callback);
        }

        public override void OnObjectReuse() {
            base.OnObjectReuse();
            if (_volumeChangeTween != null && _volumeChangeTween.active) {
                _volumeChangeTween.Kill();
            }

            if (_playbackEndCheckTween != null && _playbackEndCheckTween.active) {
                _playbackEndCheckTween.Kill();
            }

            if (_source.isPlaying) {
                _source.Stop();
            }
            Loop = false;
        }
    }
}

using UnityEngine;

namespace OneMoreRoulette.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _seSource;

        [Header("BGM")]
        [SerializeField] private AudioClip _gameBgm;

        [Header("SE - Buttons")]
        [SerializeField] private AudioClip _buttonClickSe;
        [SerializeField] private AudioClip _oneMoreSe;
        [SerializeField] private AudioClip _fireSe;

        [Header("SE - Results")]
        [SerializeField] private AudioClip _safeSe;
        [SerializeField] private AudioClip _deadSe;

        [Header("SE - Other")]
        [SerializeField] private AudioClip _bulletLoadSe;
        [SerializeField] private AudioClip _spinSe;
        [SerializeField] private AudioClip _rewardSe;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _seVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            PlayBgm();
        }

        public void PlayBgm()
        {
            if (_bgmSource == null || _gameBgm == null)
            {
                return;
            }

            _bgmSource.clip = _gameBgm;
            _bgmSource.volume = _bgmVolume;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBgm()
        {
            if (_bgmSource == null)
            {
                return;
            }

            _bgmSource.Stop();
        }

        public void PlayButtonClick()
        {
            PlaySe(_buttonClickSe);
        }

        public void PlayOneMore()
        {
            PlaySe(_oneMoreSe);
        }

        public void PlayFire()
        {
            PlaySe(_fireSe);
        }

        public void PlaySafe()
        {
            PlaySe(_safeSe);
        }

        public void PlayDead()
        {
            PlaySe(_deadSe);
        }

        public void PlayBulletLoad()
        {
            PlaySe(_bulletLoadSe);
        }

        public void PlaySpin()
        {
            PlaySe(_spinSe);
        }

        public void PlayReward()
        {
            PlaySe(_rewardSe);
        }

        private void PlaySe(AudioClip clip)
        {
            if (_seSource == null || clip == null)
            {
                return;
            }

            _seSource.PlayOneShot(clip, _seVolume);
        }

        public void SetBgmVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume;
            }
        }

        public void SetSeVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
        }
    }
}

using UnityEngine;
using DG.Tweening;

namespace OneMoreRoulette.UI
{
    /// <summary>
    /// オブジェクトをふわふわ浮遊させるアニメーション
    /// </summary>
    public class FloatingAnimation : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _floatHeight = 10f;
        [SerializeField] private float _floatDuration = 1.5f;
        [SerializeField] private Ease _floatEase = Ease.InOutSine;

        [Header("Rotation (Optional)")]
        [SerializeField] private bool _enableRotation;
        [SerializeField] private float _rotationAngle = 3f;
        [SerializeField] private float _rotationDuration = 2f;

        [Header("Scale Pulse (Optional)")]
        [SerializeField] private bool _enableScalePulse;
        [SerializeField] private float _scaleAmount = 0.05f;
        [SerializeField] private float _scaleDuration = 1.2f;

        [Header("Randomize")]
        [SerializeField] private bool _randomizeStartPhase = true;

        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private Vector3 _originalRotation;
        private Vector3 _originalScale;
        private Tween _floatTween;
        private Tween _rotationTween;
        private Tween _scaleTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalPosition = _rectTransform.anchoredPosition;
                _originalRotation = _rectTransform.localEulerAngles;
                _originalScale = _rectTransform.localScale;
            }
        }

        private void OnEnable()
        {
            StartAnimations();
        }

        private void OnDisable()
        {
            StopAnimations();
        }

        private void StartAnimations()
        {
            if (_rectTransform == null)
            {
                return;
            }

            // ランダムな開始位相
            var startDelay = _randomizeStartPhase ? Random.Range(0f, _floatDuration) : 0f;

            // 浮遊アニメーション
            _floatTween = _rectTransform
                .DOAnchorPosY(_originalPosition.y + _floatHeight, _floatDuration)
                .SetEase(_floatEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(startDelay);

            // 回転アニメーション
            if (_enableRotation)
            {
                _rotationTween = _rectTransform
                    .DOLocalRotate(
                        new Vector3(_originalRotation.x, _originalRotation.y, _originalRotation.z + _rotationAngle),
                        _rotationDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(startDelay * 0.5f);
            }

            // スケールパルス
            if (_enableScalePulse)
            {
                _scaleTween = _rectTransform
                    .DOScale(_originalScale * (1f + _scaleAmount), _scaleDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(startDelay * 0.7f);
            }
        }

        private void StopAnimations()
        {
            _floatTween?.Kill();
            _rotationTween?.Kill();
            _scaleTween?.Kill();

            // 元の位置に戻す
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPosition;
                _rectTransform.localEulerAngles = _originalRotation;
                _rectTransform.localScale = _originalScale;
            }
        }

        /// <summary>
        /// 外部からアニメーションをリセットして再開
        /// </summary>
        public void ResetAnimation()
        {
            StopAnimations();
            if (gameObject.activeInHierarchy)
            {
                StartAnimations();
            }
        }
    }
}

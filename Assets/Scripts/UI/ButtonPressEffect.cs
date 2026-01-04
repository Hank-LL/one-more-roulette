using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OneMoreRoulette.UI
{
    /// <summary>
    /// ボタンを押したときにスイッチが沈むような演出を行うコンポーネント
    /// </summary>
    public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Settings")]
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _pressDuration = 0.1f;
        [SerializeField] private float _releaseDuration = 0.15f;
        [SerializeField] private Ease _pressEase = Ease.OutQuad;
        [SerializeField] private Ease _releaseEase = Ease.OutBack;

        [Header("Optional Y Offset (3D押し込み感)")]
        [SerializeField] private bool _useYOffset = false;
        [SerializeField] private float _pressedYOffset = -5f;

        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        private Tween _scaleTween;
        private Tween _positionTween;
        private bool _isPressed;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;
        }

        private void OnDisable()
        {
            // 無効化時にTweenをキルしてリセット
            _scaleTween?.Kill();
            _positionTween?.Kill();
            transform.localScale = _originalScale;
            transform.localPosition = _originalPosition;
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed)
            {
                return;
            }

            _isPressed = true;

            // スケールを小さくする
            _scaleTween?.Kill();
            _scaleTween = transform
                .DOScale(_originalScale * _pressedScale, _pressDuration)
                .SetEase(_pressEase)
                .SetUpdate(true); // TimeScale影響を受けない

            // Y方向にオフセット（オプション）
            if (_useYOffset)
            {
                _positionTween?.Kill();
                var targetPosition = _originalPosition + new Vector3(0, _pressedYOffset, 0);
                _positionTween = transform
                    .DOLocalMove(targetPosition, _pressDuration)
                    .SetEase(_pressEase)
                    .SetUpdate(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed)
            {
                return;
            }

            _isPressed = false;

            // スケールを元に戻す
            _scaleTween?.Kill();
            _scaleTween = transform
                .DOScale(_originalScale, _releaseDuration)
                .SetEase(_releaseEase)
                .SetUpdate(true);

            // 位置を元に戻す
            if (_useYOffset)
            {
                _positionTween?.Kill();
                _positionTween = transform
                    .DOLocalMove(_originalPosition, _releaseDuration)
                    .SetEase(_releaseEase)
                    .SetUpdate(true);
            }
        }

        /// <summary>
        /// 外部からリセットを呼び出す場合
        /// </summary>
        public void ResetState()
        {
            _scaleTween?.Kill();
            _positionTween?.Kill();
            transform.localScale = _originalScale;
            transform.localPosition = _originalPosition;
            _isPressed = false;
        }
    }
}

using DG.Tweening;
using TMPro;
using UnityEngine;

namespace OneMoreRoulette.UI
{
    /// <summary>
    /// 数値のカウントアップ/ダウンアニメーションを行うコンポーネント
    /// </summary>
    public class CountUpText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [SerializeField] private string _format = "{0}";
        [SerializeField] private bool _useThousandsSeparator = true;
        [SerializeField] private bool _isInteger = true;

        [Header("Punch Effect")]
        [SerializeField] private bool _enablePunch = true;
        [SerializeField] private float _punchScale = 0.2f;
        [SerializeField] private float _punchDuration = 0.3f;

        private float _currentValue;
        private float _displayedValue;
        private Tween _countTween;
        private Tween _punchTween;

        private void Awake()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }
        }

        private void OnDisable()
        {
            _countTween?.Kill();
            _punchTween?.Kill();
        }

        /// <summary>
        /// 即座に値を設定（アニメーションなし）
        /// </summary>
        public void SetValueImmediate(float value)
        {
            _countTween?.Kill();
            _currentValue = value;
            _displayedValue = value;
            UpdateText(value);
        }

        /// <summary>
        /// 即座に値を設定（アニメーションなし）
        /// </summary>
        public void SetValueImmediate(int value)
        {
            SetValueImmediate((float)value);
        }

        /// <summary>
        /// カウントアップ/ダウンアニメーションで値を設定
        /// </summary>
        public void SetValue(float targetValue)
        {
            if (Mathf.Approximately(_currentValue, targetValue))
            {
                return;
            }

            _countTween?.Kill();
            _currentValue = targetValue;

            _countTween = DOTween.To(
                () => _displayedValue,
                x =>
                {
                    _displayedValue = x;
                    UpdateText(x);
                },
                targetValue,
                _duration
            ).SetEase(_ease);

            if (_enablePunch && _text != null)
            {
                _punchTween?.Kill();
                _punchTween = _text.transform
                    .DOPunchScale(Vector3.one * _punchScale, _punchDuration, 1, 0.5f);
            }
        }

        /// <summary>
        /// カウントアップ/ダウンアニメーションで値を設定
        /// </summary>
        public void SetValue(int targetValue)
        {
            SetValue((float)targetValue);
        }

        private void UpdateText(float value)
        {
            if (_text == null)
            {
                return;
            }

            if (_isInteger)
            {
                var intValue = Mathf.RoundToInt(value);
                var formattedNumber = _useThousandsSeparator
                    ? intValue.ToString("N0")
                    : intValue.ToString();
                _text.text = string.Format(_format, formattedNumber);
            }
            else
            {
                // float値をそのままフォーマットに渡す（小数点以下表示用）
                _text.text = string.Format(_format, value);
            }
        }
    }
}

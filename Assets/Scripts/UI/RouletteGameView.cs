using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace OneMoreRoulette.UI
{
    // ルーレットの弾倉と装填演出を担当する View（MVP の View 層）
    public sealed class RouletteGameView : GameView
    {
        private const int CylinderSize = 6;

        [SerializeField] private RectTransform _cylinderRoot;
        [SerializeField] private RectTransform _bulletSpawnPoint;
        [SerializeField] private RectTransform[] _slots = new RectTransform[CylinderSize];
        [SerializeField] private GameObject _bulletIconPrefab;
        [SerializeField] private float _loadDuration = 0.35f;
        [SerializeField] private float _loadInterval = 0.05f;
        [SerializeField] private float _spinDuration = 0.4f;
        [SerializeField] private Ease _loadEase = Ease.OutBack;
        [SerializeField] private Ease _spinEase = Ease.OutCubic;

        private readonly List<RectTransform> _bulletIcons = new(CylinderSize);
        private Sequence _loadSequence;
        private Tween _spinTween;
        private int _currentBulletCount;

        private void OnDestroy()
        {
            KillTweens();
            ClearIcons();
        }

        public override UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token)
        {
            KillTweens();
            ClearIcons();
            ResetCylinder();
            _currentBulletCount = 0;
            return UniTask.CompletedTask;
        }

        public override async UniTask PlayLoadBulletAsync(int bulletCount, CancellationToken token)
        {
            if (_bulletIconPrefab == null || _cylinderRoot == null)
            {
                return;
            }

            var slots = _slots ?? System.Array.Empty<RectTransform>();
            if (slots.Length == 0)
            {
                return;
            }

            KillTweens();

            var addCount = Mathf.Clamp(bulletCount - _currentBulletCount, 0, CylinderSize - _currentBulletCount);
            if (addCount == 0)
            {
                return;
            }

            _loadSequence = DOTween.Sequence();
            for (var i = 0; i < addCount; i++)
            {
                var slotIndex = (_currentBulletCount + i) % CylinderSize;
                if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null)
                {
                    continue;
                }

                var icon = CreateBulletIcon();
                var targetSlot = slots[slotIndex];
                icon.anchoredPosition = _bulletSpawnPoint != null ? _bulletSpawnPoint.anchoredPosition : Vector2.zero;
                icon.localScale = Vector3.zero;

                _loadSequence.Append(icon.DOScale(Vector3.one, _loadDuration).SetEase(_loadEase));
                _loadSequence.Join(icon.DOAnchorPos(targetSlot.anchoredPosition, _loadDuration).SetEase(_loadEase));
                _loadSequence.AppendInterval(_loadInterval);
            }

            using (token.Register(() => _loadSequence?.Kill()))
            {
                await _loadSequence.Play().AsyncWaitForCompletion().AttachExternalCancellation(token);
            }

            _currentBulletCount = bulletCount;
        }

        public override async UniTask PlaySpinAsync(CancellationToken token)
        {
            if (_cylinderRoot == null)
            {
                return;
            }

            KillSpin();
            _spinTween = _cylinderRoot
                .DORotate(new Vector3(0f, 0f, -360f), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(_spinEase);

            using (token.Register(() => _spinTween?.Kill()))
            {
                await _spinTween.AsyncWaitForCompletion().AttachExternalCancellation(token);
            }
        }

        private void ResetCylinder()
        {
            if (_cylinderRoot != null)
            {
                _cylinderRoot.localRotation = Quaternion.identity;
            }
        }

        private RectTransform CreateBulletIcon()
        {
            var instance = Instantiate(_bulletIconPrefab, _cylinderRoot);
            var rectTransform = instance.GetComponent<RectTransform>();
            _bulletIcons.Add(rectTransform);
            return rectTransform;
        }

        private void KillTweens()
        {
            KillLoad();
            KillSpin();
        }

        private void KillLoad()
        {
            if (_loadSequence != null && _loadSequence.IsActive())
            {
                _loadSequence.Kill();
            }

            _loadSequence = null;
        }

        private void KillSpin()
        {
            if (_spinTween != null && _spinTween.IsActive())
            {
                _spinTween.Kill();
            }

            _spinTween = null;
        }

        private void ClearIcons()
        {
            foreach (var icon in _bulletIcons)
            {
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
            }

            _bulletIcons.Clear();
        }
    }
}

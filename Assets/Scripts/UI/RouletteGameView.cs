using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OneMoreRoulette.Core;
using OneMoreRoulette.Model;
using UnityEngine;
using UnityEngine.UI;

namespace OneMoreRoulette.UI
{
    public sealed class RouletteGameView : GameView
    {
        [Header("Bullet Slots")]
        [SerializeField] private RectTransform _bulletSpawnPoint;
        [SerializeField] private RectTransform[] _bulletSlots;
        [SerializeField] private float _loadDuration = 0.35f;
        [SerializeField] private float _loadStagger = 0.08f;

        [Header("Result Popup")]
        [SerializeField] private RectTransform _resultPopup;
        [SerializeField] private Image _resultImage;
        [SerializeField] private CanvasGroup _resultCanvasGroup;
        [SerializeField] private Sprite _safeSmallSprite;
        [SerializeField] private Sprite _safeMediumSprite;
        [SerializeField] private Sprite _safeJackpotSprite;
        [SerializeField] private Sprite _deadSprite;
        [SerializeField] private float _popupDuration = 0.25f;
        [SerializeField] private float _popupDisplayTime = 0.8f;
        [SerializeField] private float _slideDistance = 50f;

        [Header("Coin Effect")]
        [SerializeField] private RectTransform _coinContainer;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private RectTransform _coinTarget;
        [SerializeField] private int _coinCountSmall = 5;
        [SerializeField] private int _coinCountMedium = 12;
        [SerializeField] private int _coinCountJackpot = 25;
        [SerializeField] private float _coinSpawnRadius = 100f;
        [SerializeField] private float _coinFlyDuration = 0.6f;
        [SerializeField] private float _coinSpawnStagger = 0.05f;

        private readonly List<Tween> _tweens = new();
        private Vector2[] _slotDefaultPositions = null!;
        private int _displayedBulletCount;

        private void Awake()
        {
            CacheSlotPositions();
            ResetSlots();
            HideResultPopup();
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public override UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token)
        {
            _displayedBulletCount = 0;
            ResetSlots();
            HideResultPopup();
            return UniTask.CompletedTask;
        }

        public override async UniTask PlayLoadBulletAsync(int bulletCount, CancellationToken token)
        {
            if (_bulletSlots == null || _bulletSlots.Length == 0)
            {
                _displayedBulletCount = bulletCount;
                return;
            }

            KillTweens();

            var clamped = Mathf.Clamp(bulletCount, 0, _bulletSlots.Length);
            var startIndex = _displayedBulletCount;
            _displayedBulletCount = clamped;

            if (clamped <= startIndex)
            {
                UpdateSlotVisibility();
                return;
            }

            var tasks = new List<UniTask>(_displayedBulletCount - startIndex);
            for (var i = startIndex; i < _displayedBulletCount; i++)
            {
                var slot = _bulletSlots[i];
                if (slot == null)
                {
                    continue;
                }

                var defaultPos = _slotDefaultPositions[i];
                var startPos = _bulletSpawnPoint != null ? _bulletSpawnPoint.anchoredPosition : defaultPos;

                slot.gameObject.SetActive(true);
                slot.localScale = Vector3.zero;
                slot.anchoredPosition = startPos;

                // 装填SE
                AudioManager.Instance?.PlayBulletLoad();

                var seq = DOTween.Sequence();
                seq.Append(slot.DOAnchorPos(defaultPos, _loadDuration).SetEase(Ease.OutQuad));
                seq.Join(slot.DOScale(1f, _loadDuration).SetEase(Ease.OutBack));
                seq.SetDelay(_loadStagger * (i - startIndex));
                seq.OnKill(() => slot.anchoredPosition = defaultPos);
                _tweens.Add(seq);
                tasks.Add(WaitForTweenAsync(seq, token));
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        public override UniTask PlaySpinAsync(CancellationToken token)
        {
            // ロジック側は回転中も弾数を変化させないため、既存の表示を維持したままにする
            return UniTask.CompletedTask;
        }

        public override async UniTask PlayRewardPopupAsync(RewardType type, int gained, float multiplier, CancellationToken token)
        {
            // Safe SE（空砲音）
            AudioManager.Instance?.PlaySafe();

            var sprite = type switch
            {
                RewardType.Small => _safeSmallSprite,
                RewardType.Medium => _safeMediumSprite,
                RewardType.Jackpot => _safeJackpotSprite,
                _ => _safeSmallSprite
            };

            var style = type switch
            {
                RewardType.Small => CutInStyle.FadeOnly,
                RewardType.Medium => CutInStyle.SlideFromBottom,
                RewardType.Jackpot => CutInStyle.DiagonalSlash,
                _ => CutInStyle.FadeOnly
            };

            var coinCount = type switch
            {
                RewardType.Small => _coinCountSmall,
                RewardType.Medium => _coinCountMedium,
                RewardType.Jackpot => _coinCountJackpot,
                _ => _coinCountSmall
            };

            // ポップアップとコイン演出を並行実行
            await UniTask.WhenAll(
                ShowResultPopupAsync(sprite, style, token),
                SpawnCoinsAsync(coinCount, token)
            );
        }

        public override async UniTask PlayDeadAsync(CancellationToken token)
        {
            // Dead SE（銃声）
            AudioManager.Instance?.PlayDead();

            await ShowResultPopupAsync(_deadSprite, CutInStyle.DiagonalReverse, token);
        }

        private async UniTask SpawnCoinsAsync(int count, CancellationToken token)
        {
            if (_coinPrefab == null || _coinContainer == null || _coinTarget == null)
            {
                return;
            }

            var tasks = new List<UniTask>(count);
            var spawnCenter = _resultPopup != null ? _resultPopup.anchoredPosition : Vector2.zero;

            for (var i = 0; i < count; i++)
            {
                var coin = Instantiate(_coinPrefab, _coinContainer);
                var coinRect = coin.GetComponent<RectTransform>();
                if (coinRect == null)
                {
                    Destroy(coin);
                    continue;
                }

                // ランダムな位置に出現
                var randomOffset = UnityEngine.Random.insideUnitCircle * _coinSpawnRadius;
                var startPos = spawnCenter + new Vector2(randomOffset.x, randomOffset.y);
                coinRect.anchoredPosition = startPos;
                coinRect.localScale = Vector3.zero;

                // ターゲット位置を取得
                var targetPos = _coinTarget.anchoredPosition;

                // 遅延付きでアニメーション開始
                var delay = i * _coinSpawnStagger;
                var coinTask = AnimateCoinAsync(coinRect, coin, startPos, targetPos, delay, token);
                tasks.Add(coinTask);
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        private async UniTask AnimateCoinAsync(RectTransform coinRect, GameObject coin, Vector2 startPos, Vector2 targetPos, float delay, CancellationToken token)
        {
            try
            {
                // 出現アニメーション
                var appearSeq = DOTween.Sequence();
                appearSeq.AppendInterval(delay);
                appearSeq.Append(coinRect.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
                _tweens.Add(appearSeq);
                await WaitForTweenAsync(appearSeq, token);

                // 少し浮遊
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f + UnityEngine.Random.Range(0f, 0.15f)), cancellationToken: token);

                // ターゲットへ飛んでいく
                var flySeq = DOTween.Sequence();
                flySeq.Append(coinRect.DOAnchorPos(targetPos, _coinFlyDuration).SetEase(Ease.InQuad));
                flySeq.Join(coinRect.DOScale(0.3f, _coinFlyDuration).SetEase(Ease.InQuad));
                _tweens.Add(flySeq);
                await WaitForTweenAsync(flySeq, token);

                // SE再生
                AudioManager.Instance?.PlayReward();
            }
            finally
            {
                // 必ずコインを破棄
                if (coin != null)
                {
                    Destroy(coin);
                }
            }
        }

        private enum CutInStyle
        {
            FadeOnly,
            SlideFromBottom,
            DiagonalSlash,
            DiagonalReverse
        }

        private async UniTask ShowResultPopupAsync(Sprite sprite, CutInStyle style, CancellationToken token)
        {
            if (_resultPopup == null || _resultImage == null)
            {
                return;
            }

            KillTweens();

            _resultImage.sprite = sprite;
            _resultPopup.gameObject.SetActive(true);

            // スタイルに応じた初期位置・回転・スケール
            var (startPos, startRot, startScale, inDuration, displayTime) = style switch
            {
                CutInStyle.FadeOnly => (
                    Vector2.zero,
                    0f,
                    Vector3.one,
                    _popupDuration * 0.8f,
                    _popupDisplayTime * 0.8f
                ),
                CutInStyle.SlideFromBottom => (
                    new Vector2(0, -_slideDistance),
                    0f,
                    Vector3.one,
                    _popupDuration,
                    _popupDisplayTime
                ),
                CutInStyle.DiagonalSlash => (
                    new Vector2(-_slideDistance * 4f, -_slideDistance * 2f),
                    -8f,
                    Vector3.one * 1.3f,
                    _popupDuration * 1.2f,
                    _popupDisplayTime * 1.3f
                ),
                CutInStyle.DiagonalReverse => (
                    new Vector2(_slideDistance * 4f, -_slideDistance * 2f),
                    8f,
                    Vector3.one * 1.2f,
                    _popupDuration * 1.1f,
                    _popupDisplayTime
                ),
                _ => (Vector2.zero, 0f, Vector3.one, _popupDuration, _popupDisplayTime)
            };

            _resultPopup.anchoredPosition = startPos;
            _resultPopup.localRotation = Quaternion.Euler(0, 0, startRot);
            _resultPopup.localScale = startScale;

            if (_resultCanvasGroup != null)
            {
                _resultCanvasGroup.alpha = 0f;
            }

            // カットイン
            var inSeq = DOTween.Sequence();

            switch (style)
            {
                case CutInStyle.FadeOnly:
                    if (_resultCanvasGroup != null)
                    {
                        inSeq.Append(_resultCanvasGroup.DOFade(1f, inDuration).SetEase(Ease.OutQuad));
                    }
                    break;

                case CutInStyle.SlideFromBottom:
                    inSeq.Append(_resultPopup.DOAnchorPos(Vector2.zero, inDuration).SetEase(Ease.OutExpo));
                    if (_resultCanvasGroup != null)
                    {
                        inSeq.Join(_resultCanvasGroup.DOFade(1f, inDuration * 0.6f).SetEase(Ease.OutQuad));
                    }
                    break;

                case CutInStyle.DiagonalSlash:
                    inSeq.Append(_resultPopup.DOAnchorPos(Vector2.zero, inDuration).SetEase(Ease.OutExpo));
                    inSeq.Join(_resultPopup.DORotate(Vector3.zero, inDuration).SetEase(Ease.OutExpo));
                    inSeq.Join(_resultPopup.DOScale(1f, inDuration).SetEase(Ease.OutExpo));
                    if (_resultCanvasGroup != null)
                    {
                        inSeq.Join(_resultCanvasGroup.DOFade(1f, inDuration * 0.4f).SetEase(Ease.OutQuad));
                    }
                    break;

                case CutInStyle.DiagonalReverse:
                    inSeq.Append(_resultPopup.DOAnchorPos(Vector2.zero, inDuration).SetEase(Ease.OutExpo));
                    inSeq.Join(_resultPopup.DORotate(Vector3.zero, inDuration).SetEase(Ease.OutExpo));
                    inSeq.Join(_resultPopup.DOScale(1f, inDuration).SetEase(Ease.OutExpo));
                    if (_resultCanvasGroup != null)
                    {
                        inSeq.Join(_resultCanvasGroup.DOFade(1f, inDuration * 0.4f).SetEase(Ease.OutQuad));
                    }
                    break;
            }

            _tweens.Add(inSeq);
            await WaitForTweenAsync(inSeq, token);

            // 表示時間
            await UniTask.Delay(TimeSpan.FromSeconds(displayTime), cancellationToken: token);

            // カットアウト
            var outSeq = DOTween.Sequence();
            var outDuration = inDuration * 0.6f;

            switch (style)
            {
                case CutInStyle.FadeOnly:
                    if (_resultCanvasGroup != null)
                    {
                        outSeq.Append(_resultCanvasGroup.DOFade(0f, outDuration).SetEase(Ease.InQuad));
                    }
                    break;

                case CutInStyle.SlideFromBottom:
                    outSeq.Append(_resultPopup.DOAnchorPosY(_slideDistance, outDuration).SetEase(Ease.InQuad));
                    if (_resultCanvasGroup != null)
                    {
                        outSeq.Join(_resultCanvasGroup.DOFade(0f, outDuration * 0.8f).SetEase(Ease.InQuad));
                    }
                    break;

                case CutInStyle.DiagonalSlash:
                    outSeq.Append(_resultPopup.DOAnchorPos(new Vector2(_slideDistance * 4f, _slideDistance * 2f), outDuration).SetEase(Ease.InExpo));
                    outSeq.Join(_resultPopup.DORotate(new Vector3(0, 0, 8f), outDuration).SetEase(Ease.InQuad));
                    if (_resultCanvasGroup != null)
                    {
                        outSeq.Join(_resultCanvasGroup.DOFade(0f, outDuration * 0.6f).SetEase(Ease.InQuad));
                    }
                    break;

                case CutInStyle.DiagonalReverse:
                    outSeq.Append(_resultPopup.DOAnchorPos(new Vector2(-_slideDistance * 4f, _slideDistance * 2f), outDuration).SetEase(Ease.InExpo));
                    outSeq.Join(_resultPopup.DORotate(new Vector3(0, 0, -8f), outDuration).SetEase(Ease.InQuad));
                    if (_resultCanvasGroup != null)
                    {
                        outSeq.Join(_resultCanvasGroup.DOFade(0f, outDuration * 0.6f).SetEase(Ease.InQuad));
                    }
                    break;
            }

            _tweens.Add(outSeq);
            await WaitForTweenAsync(outSeq, token);

            HideResultPopup();
        }

        private void HideResultPopup()
        {
            if (_resultPopup == null)
            {
                return;
            }

            _resultPopup.localScale = Vector3.one;
            _resultPopup.anchoredPosition = Vector2.zero;
            _resultPopup.localRotation = Quaternion.identity;
            if (_resultCanvasGroup != null)
            {
                _resultCanvasGroup.alpha = 0f;
            }
            _resultPopup.gameObject.SetActive(false);
        }

        private void CacheSlotPositions()
        {
            if (_bulletSlots == null)
            {
                _slotDefaultPositions = Array.Empty<Vector2>();
                return;
            }

            _slotDefaultPositions = new Vector2[_bulletSlots.Length];
            for (var i = 0; i < _bulletSlots.Length; i++)
            {
                if (_bulletSlots[i] == null)
                {
                    continue;
                }

                _slotDefaultPositions[i] = _bulletSlots[i].anchoredPosition;
            }
        }

        private void ResetSlots()
        {
            if (_bulletSlots == null)
            {
                return;
            }

            for (var i = 0; i < _bulletSlots.Length; i++)
            {
                var slot = _bulletSlots[i];
                if (slot == null)
                {
                    continue;
                }

                slot.localScale = Vector3.zero;
                slot.anchoredPosition = _slotDefaultPositions.Length > i ? _slotDefaultPositions[i] : slot.anchoredPosition;
                slot.gameObject.SetActive(false);
            }
        }

        private void UpdateSlotVisibility()
        {
            for (var i = 0; i < _bulletSlots.Length; i++)
            {
                var slot = _bulletSlots[i];
                if (slot == null)
                {
                    continue;
                }

                var active = i < _displayedBulletCount;
                slot.gameObject.SetActive(active);
                slot.localScale = active ? Vector3.one : Vector3.zero;
                slot.anchoredPosition = _slotDefaultPositions[i];
            }
        }

        private void KillTweens()
        {
            foreach (var tween in _tweens)
            {
                tween.Kill();
            }

            _tweens.Clear();
        }

        private static async UniTask WaitForTweenAsync(Tween tween, CancellationToken token)
        {
            if (tween == null || !tween.IsActive())
            {
                return;
            }

            var tcs = new UniTaskCompletionSource();

            tween.OnComplete(() => tcs.TrySetResult());
            tween.OnKill(() => tcs.TrySetResult());

            await using (token.Register(() =>
            {
                tween.Kill();
                tcs.TrySetCanceled(token);
            }))
            {
                await tcs.Task;
            }
        }
    }
}
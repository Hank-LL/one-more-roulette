using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace OneMoreRoulette.UI
{
    public sealed class RouletteGameView : GameView
    {
        [SerializeField] private RectTransform _bulletSpawnPoint;
        [SerializeField] private RectTransform[] _bulletSlots;
        [SerializeField] private float _loadDuration = 0.35f;
        [SerializeField] private float _loadStagger = 0.08f;

        private readonly List<Tween> _tweens = new();
        private Vector2[] _slotDefaultPositions = null!;
        private int _displayedBulletCount;

        private void Awake()
        {
            CacheSlotPositions();
            ResetSlots();
        }

        private void OnDisable()
        {
            KillTweens();
        }

        public override UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token)
        {
            _displayedBulletCount = 0;
            ResetSlots();
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

                var seq = DOTween.Sequence();
                seq.Append(slot.DOAnchorPos(defaultPos, _loadDuration).SetEase(Ease.OutQuad));
                seq.Join(slot.DOScale(1f, _loadDuration).SetEase(Ease.OutBack));
                seq.SetDelay(_loadStagger * (i - startIndex));
                seq.OnKill(() => slot.anchoredPosition = defaultPos);
                _tweens.Add(seq);
                tasks.Add(seq.ToUniTask(true, token: token));
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
    }
}

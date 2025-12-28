using System;
using System.Collections.Generic;
namespace OneMoreRoulette.Model
{
    // 報酬の種類とベース値（倍率適用前）
    public readonly struct Reward
    {
        public Reward(RewardType type, int baseReward)
        {
            Type = type;
            BaseReward = baseReward;
        }

        public RewardType Type { get; }
        public int BaseReward { get; }
    }

    // 発砲結果（死んだかどうか + 現在の弾数）
    public readonly struct FireResult
    {
        public FireResult(bool isDead, int bulletCount)
        {
            IsDead = isDead;
            BulletCount = bulletCount;
        }

        public bool IsDead { get; }
        public int BulletCount { get; }
    }

    public sealed class GameSettings
    {
        public GameSettings(
            int maxRounds,
            int deadLimit,
            int cylinderSize,
            float carryRate,
            int rankCap,
            IReadOnlyList<float> rankToMultiplier,
            IReadOnlyList<RewardBand> rewardBands)
        {
            MaxRounds = maxRounds;
            DeadLimit = deadLimit;
            CylinderSize = cylinderSize;
            CarryRate = carryRate;
            RankCap = rankCap;
            RankToMultiplier = rankToMultiplier;
            RewardBands = rewardBands;
        }

        public int MaxRounds { get; }
        public int DeadLimit { get; }
        public int CylinderSize { get; }
        public float CarryRate { get; }
        public int RankCap { get; }
        public IReadOnlyList<float> RankToMultiplier { get; }
        public IReadOnlyList<RewardBand> RewardBands { get; }
    }

    // 純粋 C# のゲームロジック。Unity 依存を避けてテストしやすくする
    public sealed class RouletteModel
    {
        private Random _random = new();
        private GameSettings _settings = null!;
        private int _roundIndex;

        public int BulletCount { get; private set; }
        public int RoundScore { get; private set; }
        public int TotalScore { get; private set; }
        public int Rank { get; private set; }
        public int DeadCount { get; private set; }
        public int CurrentRound => _roundIndex;

        // ランの初期化（スコア・死亡数・ラウンドインデックスをリセット）
        public void StartRun(GameSettings settings, int? seed = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (seed.HasValue)
            {
                _random = new Random(seed.Value);
            }

            _roundIndex = 0;
            DeadCount = 0;
            TotalScore = 0;
            ResetRoundValues(0);
        }

        // ラウンド開始時の初期化
        public void StartRound(int roundIndex, int startRank)
        {
            _roundIndex = roundIndex;
            ResetRoundValues(startRank);
        }

        // ONE MORE ボタンによるランク＆弾数増加
        public void ApplyOneMore()
        {
            Rank = Math.Min(Rank + 1, _settings.RankCap);
            BulletCount = Math.Min(BulletCount + 1, _settings.CylinderSize);
        }

        // シリンダー発砲。弾数が多いほど死ぬ確率が上がる
        public FireResult Fire()
        {
            var isDead = BulletCount >= _settings.CylinderSize || _random.Next(_settings.CylinderSize) < BulletCount;
            return new FireResult(isDead, BulletCount);
        }

        // セーフ時の報酬テーブル抽選
        public Reward RollSafeReward(int k)
        {
            var band = GetRewardBand(k);
            if (band.Entries == null || band.Entries.Length == 0)
            {
                throw new InvalidOperationException($"Reward band for k={k} has no entries.");
            }

            var roll = _random.NextDouble() * GetBandTotalWeight(band.Entries);
            var cumulative = 0.0;
            foreach (var entry in band.Entries)
            {
                cumulative += entry.Weight;
                if (roll <= cumulative)
                {
                    return new Reward(entry.Type, entry.BaseReward);
                }
            }

            var last = band.Entries[band.Entries.Length - 1];
            return new Reward(last.Type, last.BaseReward);
        }

        // 現在倍率をベース報酬に乗算して四捨五入
        public int CalcGained(int baseReward)
        {
            var gained = (int)Math.Round(baseReward * GetCurrentMultiplier(), MidpointRounding.AwayFromZero);
            return gained;
        }

        // ラウンドスコアへ加算
        public void ApplySafeGain(int gained)
        {
            RoundScore += gained;
        }

        // 死亡時のリセット処理
        public void ApplyDead()
        {
            RoundScore = 0;
            Rank = 0;
            DeadCount += 1;
        }

        // STOP 押下: トータルへ加算し、キャリーランクを計算して返す
        public int Cashout()
        {
            TotalScore += RoundScore;
            RoundScore = 0;
            var nextRank = (int)Math.Floor(Rank * _settings.CarryRate);
            Rank = nextRank;
            return nextRank;
        }

        // 現在ランクの倍率を取得
        public float GetCurrentMultiplier()
        {
            var index = Math.Clamp(Rank, 0, _settings.RankCap);
            return _settings.RankToMultiplier[index];
        }

        // 任意ランクの倍率（CarryNext の表示用）
        public float GetMultiplierForRank(int rank)
        {
            var index = Math.Clamp(rank, 0, _settings.RankCap);
            return _settings.RankToMultiplier[index];
        }

        private RewardBand GetRewardBand(int k)
        {
            foreach (var band in _settings.RewardBands)
            {
                if (band.K == k)
                {
                    return band;
                }
            }

            throw new InvalidOperationException($"No reward band registered for k={k}");
        }

        private static float GetBandTotalWeight(IEnumerable<RewardEntry> entries)
        {
            var total = 0f;
            foreach (var entry in entries)
            {
                total += entry.Weight;
            }

            return total;
        }

        private void ResetRoundValues(int startRank)
        {
            BulletCount = 0;
            RoundScore = 0;
            Rank = Math.Clamp(startRank, 0, _settings.RankCap);
        }
    }
}

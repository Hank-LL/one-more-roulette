using System;
using System.Collections.Generic;
namespace OneMoreRoulette.Model
{
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

        public void StartRound(int roundIndex, int startRank)
        {
            _roundIndex = roundIndex;
            ResetRoundValues(startRank);
        }

        public void ApplyOneMore()
        {
            Rank = Math.Min(Rank + 1, _settings.RankCap);
            BulletCount = Math.Min(BulletCount + 1, _settings.CylinderSize);
        }

        public FireResult Fire()
        {
            var isDead = BulletCount >= _settings.CylinderSize || _random.Next(_settings.CylinderSize) < BulletCount;
            return new FireResult(isDead, BulletCount);
        }

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

        public int CalcGained(int baseReward)
        {
            var gained = (int)Math.Round(baseReward * GetCurrentMultiplier(), MidpointRounding.AwayFromZero);
            return gained;
        }

        public void ApplySafeGain(int gained)
        {
            RoundScore += gained;
        }

        public void ApplyDead()
        {
            RoundScore = 0;
            Rank = 0;
            DeadCount += 1;
        }

        public int Cashout()
        {
            TotalScore += RoundScore;
            RoundScore = 0;
            var nextRank = (int)Math.Floor(Rank * _settings.CarryRate);
            Rank = nextRank;
            return nextRank;
        }

        public float GetCurrentMultiplier()
        {
            var index = Math.Clamp(Rank, 0, _settings.RankCap);
            return _settings.RankToMultiplier[index];
        }

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

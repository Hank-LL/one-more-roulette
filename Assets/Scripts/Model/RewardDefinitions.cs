using System;

namespace OneMoreRoulette.Model
{
    public enum RewardType
    {
        Small,
        Medium,
        Jackpot
    }

    [Serializable]
    public struct RewardEntry
    {
        public RewardType Type;
        public int BaseReward;
        public float Weight;
    }

    [Serializable]
    public struct RewardBand
    {
        public int K;
        public RewardEntry[] Entries;
    }
}

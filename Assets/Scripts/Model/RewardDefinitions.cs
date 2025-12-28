using System;

namespace OneMoreRoulette.Model
{
    // 抽選結果の種類（UI 表示・演出用）
    public enum RewardType
    {
        Small,
        Medium,
        Jackpot
    }

    // 危険度別テーブルに並ぶ 1 エントリー
    [Serializable]
    public struct RewardEntry
    {
        public RewardType Type;
        public int BaseReward;
        public float Weight;
    }

    // 弾数 k に対応する報酬バンド
    [Serializable]
    public struct RewardBand
    {
        public int K;
        public RewardEntry[] Entries;
    }
}

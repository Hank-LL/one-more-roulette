using System;
using UnityEngine;
using OneMoreRoulette.Model;

namespace OneMoreRoulette.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "OneMoreRoulette/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        // ラン全体のルール設定
        [Header("Run Settings")]
        [SerializeField] private int maxRounds = 5;
        [SerializeField] private int deadLimit = 2;
        [SerializeField] private int cylinderSize = 6;
        [SerializeField] private float carryRate = 0.5f;
        [SerializeField] private int rankCap = 10;

        // ランクに応じた倍率テーブル（インデックス = ランク）
        [Header("Rank -> Multiplier")]
        [SerializeField] private float[] rankToMultiplier =
        {
            1.0f, 1.15f, 1.35f, 1.60f, 1.90f, 2.25f,
            2.70f, 3.25f, 3.95f, 4.80f, 5.80f
        };

        [Header("Reward Bands (k=1..5)")]
        [SerializeField] private RewardBand[] rewardBandsByK =
        {
            new RewardBand
            {
                K = 1,
                Entries = new[]
                {
                    new RewardEntry { Type = RewardType.Small, BaseReward = 20, Weight = 65f },
                    new RewardEntry { Type = RewardType.Medium, BaseReward = 50, Weight = 30f },
                    new RewardEntry { Type = RewardType.Jackpot, BaseReward = 120, Weight = 5f },
                }
            },
            new RewardBand
            {
                K = 2,
                Entries = new[]
                {
                    new RewardEntry { Type = RewardType.Small, BaseReward = 40, Weight = 60f },
                    new RewardEntry { Type = RewardType.Medium, BaseReward = 90, Weight = 32f },
                    new RewardEntry { Type = RewardType.Jackpot, BaseReward = 200, Weight = 8f },
                }
            },
            new RewardBand
            {
                K = 3,
                Entries = new[]
                {
                    new RewardEntry { Type = RewardType.Small, BaseReward = 60, Weight = 55f },
                    new RewardEntry { Type = RewardType.Medium, BaseReward = 140, Weight = 35f },
                    new RewardEntry { Type = RewardType.Jackpot, BaseReward = 320, Weight = 10f },
                }
            },
            new RewardBand
            {
                K = 4,
                Entries = new[]
                {
                    new RewardEntry { Type = RewardType.Small, BaseReward = 90, Weight = 50f },
                    new RewardEntry { Type = RewardType.Medium, BaseReward = 200, Weight = 37f },
                    new RewardEntry { Type = RewardType.Jackpot, BaseReward = 480, Weight = 13f },
                }
            },
            new RewardBand
            {
                K = 5,
                Entries = new[]
                {
                    new RewardEntry { Type = RewardType.Small, BaseReward = 120, Weight = 45f },
                    new RewardEntry { Type = RewardType.Medium, BaseReward = 280, Weight = 40f },
                    new RewardEntry { Type = RewardType.Jackpot, BaseReward = 650, Weight = 15f },
                }
            }
        };

        // ScriptableObject を純粋な設定クラスへ変換
        public GameSettings ToSettings()
        {
            return new GameSettings(
                maxRounds,
                deadLimit,
                cylinderSize,
                carryRate,
                rankCap,
                rankToMultiplier,
                rewardBandsByK);
        }

        private void OnValidate()
        {
            if (rankToMultiplier == null || rankToMultiplier.Length != rankCap + 1)
            {
                Array.Resize(ref rankToMultiplier, rankCap + 1);
            }

            if (rankToMultiplier.Length >= 11)
            {
                rankToMultiplier[0] = 1.0f;
                rankToMultiplier[1] = 1.15f;
                rankToMultiplier[2] = 1.35f;
                rankToMultiplier[3] = 1.60f;
                rankToMultiplier[4] = 1.90f;
                rankToMultiplier[5] = 2.25f;
                rankToMultiplier[6] = 2.70f;
                rankToMultiplier[7] = 3.25f;
                rankToMultiplier[8] = 3.95f;
                rankToMultiplier[9] = 4.80f;
                rankToMultiplier[10] = 5.80f;
            }
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using unityroom.Api;

namespace OneMoreRoulette.Core
{
    public class RankingService : MonoBehaviour
    {
        public static RankingService Instance { get; private set; }

        [SerializeField] private int _boardNo = 1;

        // HMAC認証用キー
        private const string HmacKey = "J1DNWxbp6htuIDx4lETDMpr2DfvrU1vxP4l9yWPLIGDqMtFbqWpnu72BmBF5NGBWHNdghlnByTVzqYIlGsem8w==";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// トータルスコアをunityroomランキングに送信
        /// </summary>
        public void SendScore(int totalScore)
        {
            if (totalScore <= 0)
            {
                Debug.Log("[Ranking] スコアが0以下のため送信をスキップ");
                return;
            }

            try
            {
                // HMAC認証付きでスコア送信
                var score = (float)totalScore;
                UnityroomApiClient.Instance.SendScore(_boardNo, score, ScoreboardWriteMode.HighScoreDesc);
                Debug.Log($"[Ranking] スコア送信成功: {totalScore}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Ranking] スコア送信失敗: {e.Message}");
            }
        }

        /// <summary>
        /// 非同期でスコア送信（UIからの呼び出し用）
        /// </summary>
        public async UniTask SendScoreAsync(int totalScore)
        {
            await UniTask.SwitchToMainThread();
            SendScore(totalScore);
        }
    }
}

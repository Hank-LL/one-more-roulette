using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using OneMoreRoulette.Config;

namespace OneMoreRoulette.UI
{
    public interface IGameView
    {
        UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token);
        UniTask PlayLoadBulletAsync(int bulletCount, CancellationToken token);
        UniTask PlaySpinAsync(CancellationToken token);
        UniTask PlayFireAsync(bool isDead, CancellationToken token);
        UniTask PlayRewardPopupAsync(RewardType type, int gained, float multiplier, CancellationToken token);
        UniTask PlayCashoutAsync(int addScore, int prevRank, int nextRank, CancellationToken token);
        UniTask PlayDeadAsync(CancellationToken token);
        UniTask PlayGameOverAsync(bool isDeadLimit, CancellationToken token);
    }

    public class GameView : MonoBehaviour, IGameView
    {
        public virtual UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayLoadBulletAsync(int bulletCount, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlaySpinAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayFireAsync(bool isDead, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayRewardPopupAsync(RewardType type, int gained, float multiplier, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayCashoutAsync(int addScore, int prevRank, int nextRank, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayDeadAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask PlayGameOverAsync(bool isDeadLimit, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}

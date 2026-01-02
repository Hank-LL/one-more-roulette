using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using OneMoreRoulette.Config;
using OneMoreRoulette.Model;

namespace OneMoreRoulette.UI
{
    // アニメーション用のインターフェース。ロジックは RunController 側にのみ存在する
    //
    // 想定している演出フローの例
    // - PlayRoundStartAsync: ラウンド番号や倍率の更新をフェードインさせ、UI を初期化する。
    // - PlayLoadBulletAsync: One More 直後に呼ばれる。追加された弾の数だけストレージを埋める、
    //   シリンダー中央に弾が吸い込まれる、といった装填演出を行う。
    // - PlaySpinAsync: One More 後は「次の弾を込めたままシリンダーを空回しする」演出、
    //   Stop 後は発砲直前の最終回転演出として呼ばれる。弾が発射されるのは次の PlayFireAsync。
    // - PlayFireAsync: トリガーを引いた結果の演出。マズルフラッシュ、煙、被弾エフェクト、
    //   セーフなら空撃ちモーションなどを実装できる。
    // - PlayRewardPopupAsync: セーフ時のリワードポップアップ表示。
    // - PlayCashoutAsync: セーフ分をスコアに加算し、ランクが上がる場合はそのアニメーションを行う。
    // - PlayDeadAsync: 被弾時の死亡演出。残機があればラウンド終了へ、尽きれば PlayGameOverAsync へ。
    // - PlayGameOverAsync: 死亡上限に達した/最大ラウンドを走破した際のゲーム終了演出。
    public interface IGameView
    {
        UniTask PlayRoundStartAsync(int roundIndex, CancellationToken token);
        // One More 後に装填された弾数が渡されるので、バレルやストレージの UI があればここで装填演出を再生できる
        UniTask PlayLoadBulletAsync(int bulletCount, CancellationToken token);
        // シリンダー回転のみの演出。Stop 側で発砲可否を判定するため、ここではまだ弾が出ない
        UniTask PlaySpinAsync(CancellationToken token);
        UniTask PlayFireAsync(bool isDead, CancellationToken token);
        UniTask PlayRewardPopupAsync(RewardType type, int gained, float multiplier, CancellationToken token);
        UniTask PlayCashoutAsync(int addScore, int prevRank, int nextRank, CancellationToken token);
        UniTask PlayDeadAsync(CancellationToken token);
        UniTask PlayGameOverAsync(bool isDeadLimit, CancellationToken token);
    }

    // DOTween などの演出を継承クラスで実装できるようにしたベース View
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

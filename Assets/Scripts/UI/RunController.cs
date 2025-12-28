using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using OneMoreRoulette.Config;
using OneMoreRoulette.Core;
using OneMoreRoulette.Model;

namespace OneMoreRoulette.UI
{
    // Presenter。ステート管理と Model 呼び出し、ViewModel 更新、View 演出を直列化する
    public class RunController : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private GameView _view;
        [SerializeField] private UiBinder _uiBinder;

        private readonly RouletteModel _model = new();
        private readonly GameStateMachine _stateMachine = new();
        private GameViewModel _viewModel;
        private GameSettings _settings;
        private CancellationTokenSource _runCts;

        private void Awake()
        {
            // プレゼンター側で ViewModel を生成し、Binder に渡す
            _viewModel = new GameViewModel();
            _uiBinder?.Initialize(_viewModel, this);
        }

        private void OnDestroy()
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _uiBinder?.Dispose();
        }

        private void Start()
        {
            StartRunAsync().Forget();
        }

        // ONE MORE ボタン入力（外部からバインド）
        public UniTask OnOneMoreAsync()
        {
            // Decision ステート以外では入力を無視して安全に抜ける
            if (_stateMachine.CurrentState != GameState.Decision)
            {
                return UniTask.CompletedTask;
            }

            return HandleOneMoreAsync(_runCts?.Token ?? CancellationToken.None);
        }

        public UniTask OnStopAsync()
        {
            // Decision ステート以外では入力を無視して安全に抜ける
            if (_stateMachine.CurrentState != GameState.Decision)
            {
                return UniTask.CompletedTask;
            }

            return HandleStopAsync(_runCts?.Token ?? CancellationToken.None);
        }

        // ラン全体の開始。設定を読み込み、最初のラウンドに入る
        public async UniTask StartRunAsync(int? seed = null)
        {
            // ラン中断用のトークンをリセット
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();

            _stateMachine.SetState(GameState.Run_Init);
            UpdateViewModelState();

            if (_config == null)
            {
                throw new InvalidOperationException("GameConfig is not assigned.");
            }

            _settings = _config.ToSettings();
            _model.StartRun(_settings, seed);
            await StartRoundAsync(1, 0, _runCts.Token);
        }

        private async UniTask StartRoundAsync(int roundIndex, int startRank, CancellationToken token)
        {
            _model.StartRound(roundIndex, startRank);
            _stateMachine.SetState(GameState.Round_Start);
            UpdateViewModel();
            UpdateViewModelState();
            await _view.PlayRoundStartAsync(roundIndex, token);

            _stateMachine.SetState(GameState.Decision);
            UpdateViewModelState();
        }

        private async UniTask HandleOneMoreAsync(CancellationToken token)
        {
            _stateMachine.SetState(GameState.Spin);
            _model.ApplyOneMore();
            UpdateViewModel();
            UpdateViewModelState();

            await _view.PlayLoadBulletAsync(_model.BulletCount, token);
            await _view.PlaySpinAsync(token);
            var fireResult = _model.Fire();
            await _view.PlayFireAsync(fireResult.IsDead, token);

            if (fireResult.IsDead)
            {
                await ResolveDeadAsync(token);
                return;
            }

            await ResolveSafeAsync(fireResult.BulletCount, token);
        }

        private async UniTask ResolveSafeAsync(int bulletCount, CancellationToken token)
        {
            _stateMachine.SetState(GameState.Resolve_Safe);
            var reward = _model.RollSafeReward(bulletCount);
            var gained = _model.CalcGained(reward.BaseReward);
            _model.ApplySafeGain(gained);
            UpdateViewModel();
            UpdateViewModelState();
            await _view.PlayRewardPopupAsync(reward.Type, gained, _model.GetCurrentMultiplier(), token);

            _stateMachine.SetState(GameState.Decision);
            UpdateViewModelState();
        }

        private async UniTask ResolveDeadAsync(CancellationToken token)
        {
            _stateMachine.SetState(GameState.Resolve_Dead);
            _model.ApplyDead();
            UpdateViewModel();
            UpdateViewModelState();
            await _view.PlayDeadAsync(token);

            if (_model.DeadCount >= _settings.DeadLimit)
            {
                await FinishGameAsync(true, token);
                return;
            }

            await EndRoundAsync(token);
        }

        private async UniTask HandleStopAsync(CancellationToken token)
        {
            _stateMachine.SetState(GameState.Cashout);
            var previousRank = _model.Rank;
            var addedScore = _model.RoundScore;
            var nextRank = _model.Cashout();
            UpdateViewModel();
            UpdateViewModelState();
            await _view.PlayCashoutAsync(addedScore, previousRank, nextRank, token);

            await EndRoundAsync(token);
        }

        private async UniTask EndRoundAsync(CancellationToken token)
        {
            _stateMachine.SetState(GameState.Round_End);
            UpdateViewModelState();

            var nextRoundIndex = _model.CurrentRound + 1;
            if (nextRoundIndex > _settings.MaxRounds)
            {
                await FinishGameAsync(false, token);
                return;
            }

            var startRank = _model.Rank;
            await StartRoundAsync(nextRoundIndex, startRank, token);
        }

        private async UniTask FinishGameAsync(bool isDeadLimit, CancellationToken token)
        {
            _stateMachine.SetState(GameState.GameOver);
            UpdateViewModelState();
            await _view.PlayGameOverAsync(isDeadLimit, token);
            _stateMachine.SetState(GameState.Result);
            UpdateViewModelState();
        }

        private void UpdateViewModel()
        {
            _viewModel.RoundIndex.Value = _model.CurrentRound;
            _viewModel.DeadCount.Value = _model.DeadCount;
            _viewModel.BulletCount.Value = _model.BulletCount;
            _viewModel.Rank.Value = _model.Rank;
            _viewModel.Multiplier.Value = _model.GetCurrentMultiplier();
            _viewModel.RoundScore.Value = _model.RoundScore;
            _viewModel.TotalScore.Value = _model.TotalScore;
            var nextRank = (int)Math.Floor(_model.Rank * _settings.CarryRate);
            _viewModel.CarryNextMultiplier.Value = _model.GetMultiplierForRank(nextRank);
        }

        private void UpdateViewModelState()
        {
            _viewModel.State.Value = _stateMachine.CurrentState;
        }
    }
}

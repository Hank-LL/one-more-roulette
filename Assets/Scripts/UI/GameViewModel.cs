using R3;

namespace OneMoreRoulette.UI
{
    // R3 で UI を双方向バインディングするための値コンテナ（Presenter だけが書き込む）
    public sealed class GameViewModel
    {
        public ReactiveProperty<int> RoundIndex { get; } = new(0);
        public ReactiveProperty<int> DeadCount { get; } = new(0);
        public ReactiveProperty<int> BulletCount { get; } = new(0);
        public ReactiveProperty<int> Rank { get; } = new(0);
        public ReactiveProperty<float> Multiplier { get; } = new(1f);
        public ReactiveProperty<int> RoundScore { get; } = new(0);
        public ReactiveProperty<int> TotalScore { get; } = new(0);
        public ReactiveProperty<float> CarryNextMultiplier { get; } = new(1f);
        public ReactiveProperty<Core.GameState> State { get; } = new(Core.GameState.Title);
    }
}

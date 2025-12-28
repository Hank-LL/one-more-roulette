namespace OneMoreRoulette.Core
{
    // ゲーム進行を表すシンプルなステート列挙
    public enum GameState
    {
        Title,
        HowTo,
        Run_Init,
        Round_Start,
        Decision,
        Spin,
        Resolve_Safe,
        Resolve_Dead,
        Cashout,
        Round_End,
        GameOver,
        Result
    }

    // 現在ステートを保持するだけの軽量マシン（遷移ロジックは RunController 側）
    public sealed class GameStateMachine
    {
        public GameState CurrentState { get; private set; } = GameState.Title;

        public void SetState(GameState next)
        {
            CurrentState = next;
        }
    }
}

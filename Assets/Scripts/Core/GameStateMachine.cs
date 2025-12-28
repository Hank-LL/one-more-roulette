namespace OneMoreRoulette.Core
{
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

    public sealed class GameStateMachine
    {
        public GameState CurrentState { get; private set; } = GameState.Title;

        public void SetState(GameState next)
        {
            CurrentState = next;
        }
    }
}

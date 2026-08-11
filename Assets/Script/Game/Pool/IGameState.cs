namespace Assets.Script.Game.Pool {
    /// <summary>Read-only match data for input, UI, aiming, and presentation code.</summary>
    public interface IGameState {
        GameState CurrentGameState { get; }
        Turn CurrentTurn { get; }
        bool ShotInProgress { get; }
        bool GroupsDecided { get; }
        BallType Player1BallType { get; }
        BallType Player2BallType { get; }
        BallType GetBallTypeFor(Turn player);
    }

    /// <summary>Commands that only the rules layer should issue to the match state.</summary>
    public interface IGameStateWriter {
        bool TryBeginShot();
        void EndShot();
        void OpenTable();
        void AssignBallGroups(BallType currentPlayerGroup);
        void SwitchTurn();
        void SetPlayerBallType(Turn player, BallType ballType);
    }
}

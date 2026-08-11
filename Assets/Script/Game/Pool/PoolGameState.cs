using System;
using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// The single source of truth for persistent pool-match state.
    /// Only PoolRules should hold this as an IGameStateWriter.
    /// </summary>
    public sealed class PoolGameState : MonoBehaviour, IGameState, IGameStateWriter {
        [SerializeField] GameState currentGameState = GameState.Breaking;
        [SerializeField] Turn currentTurn = Turn.player1Turn;
        [SerializeField] bool shotInProgress;
        [SerializeField] bool groupsDecided;
        [SerializeField] BallType player1BallType = BallType.Solid;
        [SerializeField] BallType player2BallType = BallType.Strip;

        public GameState CurrentGameState => currentGameState;
        public Turn CurrentTurn => currentTurn;
        public bool ShotInProgress => shotInProgress;
        public bool GroupsDecided => groupsDecided;
        public BallType Player1BallType => player1BallType;
        public BallType Player2BallType => player2BallType;

        public event Action StateChanged;
        public event Action<BallType, BallType> BallGroupsDecided;
        public event Action<Turn, BallType> PlayerBallGroupChanged;
        public event Action<Turn> TurnChanged;

        public BallType GetBallTypeFor( Turn player ) {
            return player == Turn.player1Turn ? player1BallType : player2BallType;
        }

        public bool TryBeginShot() {
            if ( shotInProgress )
                return false;

            shotInProgress = true;
            NotifyStateChanged();
            return true;
        }

        public void EndShot() {
            if ( !shotInProgress )
                return;

            shotInProgress = false;
            NotifyStateChanged();
        }

        public void OpenTable() {
            if ( currentGameState == GameState.Breaking ) {
                currentGameState = GameState.DecideBallGroup;
                NotifyStateChanged();
            }
        }

        public void AssignBallGroups( BallType currentPlayerGroup ) {
            if ( groupsDecided || (currentPlayerGroup != BallType.Solid && currentPlayerGroup != BallType.Strip) )
                return;

            BallType otherGroup = currentPlayerGroup == BallType.Solid ? BallType.Strip : BallType.Solid;
            if ( currentTurn == Turn.player1Turn ) {
                player1BallType = currentPlayerGroup;
                player2BallType = otherGroup;
            }
            else {
                player1BallType = otherGroup;
                player2BallType = currentPlayerGroup;
            }

            groupsDecided = true;
            currentGameState = GameState.Playing;
            BallGroupsDecided?.Invoke(player1BallType, player2BallType);
            NotifyStateChanged();
        }

        public void SwitchTurn() {
            currentTurn = currentTurn == Turn.player1Turn ? Turn.player2Turn : Turn.player1Turn;
            TurnChanged?.Invoke(currentTurn);
            NotifyStateChanged();
        }

        public void SetPlayerBallType( Turn player, BallType ballType ) {
            if ( player == Turn.player1Turn ) {
                if ( player1BallType == ballType )
                    return;

                player1BallType = ballType;
            }
            else {
                if ( player2BallType == ballType )
                    return;

                player2BallType = ballType;
            }

            PlayerBallGroupChanged?.Invoke(player, ballType);
            NotifyStateChanged();
        }

        void NotifyStateChanged() {
            StateChanged?.Invoke();
        }
    }
}

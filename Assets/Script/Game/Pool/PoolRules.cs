using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>Converts one settled ShotResult into turn and ball-group state changes.</summary>
    public sealed class PoolRules : MonoBehaviour {
        [SerializeField] PoolGameState gameState;
        [SerializeField] CueBall cueBall;
        [SerializeField] BallMotionTracker motionTracker;

        ShotResult activeShot;

        void Awake() {
            gameState ??= FindFirstObjectByType<PoolGameState>();
            cueBall ??= FindFirstObjectByType<CueBall>();
            motionTracker ??= FindFirstObjectByType<BallMotionTracker>();
        }

        void OnEnable() {
            if ( cueBall != null ) {
                cueBall.ShotFired += BeginShot;
                cueBall.ObjectBallHit += RecordObjectBallHit;
            }

            if ( motionTracker != null )
                motionTracker.BallsStopped += ResolveShot;

            PoolPocket.BallPocketed += RecordPocketedBall;
        }

        void OnDisable() {
            if ( cueBall != null ) {
                cueBall.ShotFired -= BeginShot;
                cueBall.ObjectBallHit -= RecordObjectBallHit;
            }

            if ( motionTracker != null )
                motionTracker.BallsStopped -= ResolveShot;

            PoolPocket.BallPocketed -= RecordPocketedBall;
        }

        void BeginShot() {
            if ( gameState == null || !gameState.TryBeginShot() )
                return;

            activeShot = new ShotResult(gameState.CurrentGameState == GameState.Breaking);
        }

        void RecordObjectBallHit( PoolBall ball ) {
            activeShot?.RecordCueHitObjectBall(ball);
        }

        void RecordPocketedBall( PoolBall ball ) {
            activeShot?.RecordPocketedBall(ball);
        }

        void ResolveShot() {
            if ( gameState == null || activeShot == null || !gameState.ShotInProgress )
                return;

            if ( activeShot.IsBreak )
                ResolveBreak(activeShot);
            else
                ResolveNormalShot(activeShot);

            PromoteClearedGroupsToEightBall();
            gameState.EndShot();
            activeShot = null;
        }

        void ResolveBreak( ShotResult shot ) {
            if ( !shot.CueHitObjectBall ) {
                Debug.LogWarning("Break finished without an object-ball contact.", this);
                gameState.SwitchTurn();
                return;
            }

            // A legal break opens the table. A pocketed ball only keeps the turn.
            gameState.OpenTable();
            if ( !shot.PocketedObjectBall )
                gameState.SwitchTurn();
        }

        void ResolveNormalShot( ShotResult shot ) {
            if ( gameState.CurrentGameState == GameState.DecideBallGroup
                && shot.FirstPocketedGroup.HasValue ) {
                gameState.AssignBallGroups(shot.FirstPocketedGroup.Value);
            }

            if ( !shot.PocketedObjectBall )
                gameState.SwitchTurn();
        }

        void PromoteClearedGroupsToEightBall() {
            if ( !gameState.GroupsDecided )
                return;

            PromoteIfCleared(Turn.player1Turn);
            PromoteIfCleared(Turn.player2Turn);
        }

        void PromoteIfCleared( Turn player ) {
            BallType group = gameState.GetBallTypeFor(player);
            if ( group == BallType.Ball8 || HasActiveBallOfType(group) )
                return;

            gameState.SetPlayerBallType(player, BallType.Ball8);
        }

        static bool HasActiveBallOfType( BallType group ) {
            foreach ( PoolBall ball in FindObjectsByType<PoolBall>(FindObjectsSortMode.None) ) {
                if ( ball.Type == group )
                    return true;
            }

            return false;
        }
    }
}

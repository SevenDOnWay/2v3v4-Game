using System.Collections.Generic;

namespace Assets.Script.Game.Pool {
    /// <summary>Facts gathered during one shot; PoolRules resolves them after the table settles.</summary>
    public sealed class ShotResult {
        readonly List<PoolBall> pocketedBalls = new();

        public bool IsBreak { get; }
        public bool CueHitObjectBall { get; private set; }
        public IReadOnlyList<PoolBall> PocketedBalls => pocketedBalls;
        public bool PocketedObjectBall => pocketedBalls.Count > 0;

        public BallType? FirstPocketedGroup {
            get {
                foreach (PoolBall ball in pocketedBalls) {
                    if (ball.Type == BallType.Solid || ball.Type == BallType.Strip)
                        return ball.Type;
                }
                return null;
            }
        }

        public ShotResult(bool isBreak) { IsBreak = isBreak; }

        public void RecordCueHitObjectBall(PoolBall ball) {
            if (ball != null) CueHitObjectBall = true;
        }

        public void RecordPocketedBall(PoolBall ball) {
            if (ball != null && !pocketedBalls.Contains(ball)) pocketedBalls.Add(ball);
        }
    }
}

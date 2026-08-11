using System;
using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// Backward-compatible event bridge for scenes that already contain BallManager.
    /// New gameplay code should use BallMotionTracker directly.
    /// </summary>
    [Obsolete("Use BallMotionTracker instead.")]
    public sealed class BallManager : MonoBehaviour {
        [SerializeField] BallMotionTracker motionTracker;

        public event Action BallsStopped;

        void Awake() {
            motionTracker ??= FindFirstObjectByType<BallMotionTracker>();
        }

        void OnEnable() {
            if ( motionTracker != null )
                motionTracker.BallsStopped += ForwardBallsStopped;
        }

        void OnDisable() {
            if ( motionTracker != null )
                motionTracker.BallsStopped -= ForwardBallsStopped;
        }

        void ForwardBallsStopped() {
            BallsStopped?.Invoke();
        }
    }
}

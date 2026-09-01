using System;
using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// Owns the "all balls have stopped" signal for a single shot.
    /// </summary>
    public sealed class BallMotionTracker : MonoBehaviour {
        [SerializeField, Min(0.001f)] private float restSpeed = 0.025f;
        [SerializeField] private CueBall cueBall;

        private bool trackingShot;
        private int fixedStepsSinceShot;

        public event Action BallsStopped;

        private void Awake() {
            if (cueBall == null) {
                cueBall = FindFirstObjectByType<CueBall>();
            }
        }

        private void OnEnable() {
            if (cueBall != null) {
                cueBall.ShotFired += BeginTracking;
            }
        }

        private void OnDisable() {
            if (cueBall != null) {
                cueBall.ShotFired -= BeginTracking;
            }
        }

        public bool AllPoolBallsStopped() {
            foreach (Rigidbody body in FindObjectsByType<Rigidbody>(
                         FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
                if (!IsPoolBall(body)) {
                    continue;
                }

                if (body.linearVelocity.sqrMagnitude > restSpeed * restSpeed) {
                    return false;
                }
            }

            return true;
        }

        private void BeginTracking() {
            trackingShot = true;
            fixedStepsSinceShot = 0;
        }

        private void FixedUpdate() {
            if (!trackingShot) {
                return;
            }

            fixedStepsSinceShot++;
            if (fixedStepsSinceShot < 2 || !AllPoolBallsStopped()) {
                return;
            }

            trackingShot = false;
            BallsStopped?.Invoke();
        }

        private bool IsPoolBall(Rigidbody body) {
            return body != null &&
                   (body.GetComponent<CueBall>() != null || body.GetComponent<PoolBall>() != null);
        }
    }
}
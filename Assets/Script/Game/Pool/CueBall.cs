using System;
using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>Applies the cue impulse and reports physics facts; it contains no pool-rule decisions.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public sealed class CueBall : MonoBehaviour {
        [SerializeField, Min(0.01f)] float shotImpulse = 0.65f;
        [SerializeField] ForceSlider forceSlider;
        [SerializeField] SpinUi spinUi;
        [SerializeField] PoolController poolController;
        [SerializeField] PoolGameState gameState;

        Rigidbody cueRigidbody;
        Collider cueCollider;

        public event Action ShotFired;
        public event Action<PoolBall> ObjectBallHit;

        void Awake() {
            cueRigidbody = GetComponent<Rigidbody>();
            cueCollider = GetComponent<Collider>();
            forceSlider ??= FindFirstObjectByType<ForceSlider>();
            spinUi ??= FindFirstObjectByType<SpinUi>();
            poolController ??= FindFirstObjectByType<PoolController>();
            gameState ??= FindFirstObjectByType<PoolGameState>();
        }

        void OnEnable() {
            if ( forceSlider != null )
                forceSlider.OnRelease += Shoot;
        }

        void OnDisable() {
            if ( forceSlider != null )
                forceSlider.OnRelease -= Shoot;
        }

        void Shoot( float force ) {
            if ( gameState != null && gameState.ShotInProgress )
                return;

            if ( cueRigidbody == null || cueCollider == null || poolController == null
                || !poolController.TryGetAimDirection(out Vector3 direction) )
                return;

            float impulse = Mathf.Clamp01(force) * shotImpulse;
            if ( impulse <= 0f )
                return;

            cueRigidbody.AddForceAtPosition(direction * impulse, GetCueContactPoint(direction), ForceMode.Impulse);
            ShotFired?.Invoke();
        }

        Vector3 GetCueContactPoint( Vector3 shotDirection ) {
            Vector3 center = cueCollider.bounds.center;
            float radius = Mathf.Max(cueCollider.bounds.extents.x, cueCollider.bounds.extents.z);
            Vector3 right = Vector3.Cross(Vector3.up, shotDirection).normalized;
            Vector3 tipOffset = spinUi != null
                ? spinUi.GetCueTipOffset(radius, right, Vector3.up)
                : Vector3.zero;
            float depth = Mathf.Sqrt(Mathf.Max(0f, radius * radius - tipOffset.sqrMagnitude));

            return center + tipOffset - shotDirection * depth;
        }

        void OnCollisionEnter( Collision collision ) {
            PoolBall objectBall = collision.gameObject.GetComponent<PoolBall>();
            if ( objectBall != null )
                ObjectBallHit?.Invoke(objectBall);
        }
    }
}

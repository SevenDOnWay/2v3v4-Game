using System;
using UnityEngine;

namespace Assets.Script.Game.Pool {
    [RequireComponent(typeof(Collider))]
    public sealed class PoolPocket : MonoBehaviour {
        public static event Action<PoolBall> BallPocketed;

        void OnTriggerEnter( Collider other ) {
            PoolBall poolBall = other.GetComponent<PoolBall>();
            if ( poolBall == null )
                return;

            Rigidbody ball = other.attachedRigidbody;
            if ( ball == null )
                return;

            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
            BallPocketed?.Invoke(poolBall);
            ball.gameObject.SetActive(false);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Script.Game.Pool {
    /// <summary>Owns pointer input, aiming, and optional cue-ball placement.</summary>
    public sealed class PoolController : MonoBehaviour {
        [Header("References")]
        [SerializeField] GameInput gameInput;
        [SerializeField] PoolGameState gameState;
        [SerializeField] Camera tableCamera;
        [SerializeField] CueBall cueBall;

        [Header("Cue ball placement")]
        [SerializeField] bool canPickUpBall;
        [SerializeField] LayerMask collisionMask = 1 << 10;
        [SerializeField] LayerMask tableMask = 1 << 11;

        InputAction aimPositionAction;
        InputAction aimPressAction;
        Rigidbody cueRigidbody;
        Collider cueCollider;
        bool isDraggingCueBall;
        bool cueBallWasKinematic;
        Vector3 currentAimPosition;

        public event Action AimUpdated;
        public event Action AimEnded;
        public Vector3 CurrentAimPosition => currentAimPosition;

        void Awake() {
            gameInput ??= FindFirstObjectByType<GameInput>();
            gameState ??= FindFirstObjectByType<PoolGameState>();
            tableCamera ??= Camera.main;
            cueBall ??= FindFirstObjectByType<CueBall>();

            if ( cueBall != null ) {
                cueRigidbody = cueBall.GetComponent<Rigidbody>();
                cueCollider = cueBall.GetComponent<Collider>();
            }
        }

        void OnEnable() {
            if ( gameInput == null || gameInput.AimPosition == null || gameInput.AimPress == null ) {
                Debug.LogError("PoolController needs GameInput.AimPosition and GameInput.AimPress.", this);
                return;
            }

            aimPositionAction = gameInput.AimPosition;
            aimPressAction = gameInput.AimPress;
            aimPositionAction.performed += HandleAimPosition;
            aimPressAction.started += HandleAimPressStarted;
            aimPressAction.canceled += HandleAimPressCanceled;
            aimPositionAction.Enable();
            aimPressAction.Enable();
        }

        void OnDisable() {
            if ( aimPositionAction != null ) {
                aimPositionAction.performed -= HandleAimPosition;
                aimPositionAction.Disable();
                aimPositionAction = null;
            }

            if ( aimPressAction != null ) {
                aimPressAction.started -= HandleAimPressStarted;
                aimPressAction.canceled -= HandleAimPressCanceled;
                aimPressAction.Disable();
                aimPressAction = null;
            }
        }

        void HandleAimPosition( InputAction.CallbackContext context ) {
            if ( aimPressAction == null || !aimPressAction.IsPressed() || IsShotRunning() )
                return;

            Vector2 screenPosition = context.ReadValue<Vector2>();
            if ( isDraggingCueBall ) {
                TryMoveCueBall(screenPosition);
                return;
            }

            UpdateAimFromScreen(screenPosition);
        }

        void HandleAimPressStarted( InputAction.CallbackContext context ) {
            if ( !canPickUpBall || IsShotRunning() || cueCollider == null || tableCamera == null )
                return;

            Vector2 screenPosition = aimPositionAction.ReadValue<Vector2>();
            Ray ray = tableCamera.ScreenPointToRay(screenPosition);
            if ( !Physics.Raycast(ray, out RaycastHit hit) || hit.collider != cueCollider )
                return;

            isDraggingCueBall = true;
            cueBallWasKinematic = cueRigidbody != null && cueRigidbody.isKinematic;
            if ( cueRigidbody == null )
                return;

            cueRigidbody.linearVelocity = Vector3.zero;
            cueRigidbody.angularVelocity = Vector3.zero;
            cueRigidbody.isKinematic = true;
        }

        void HandleAimPressCanceled( InputAction.CallbackContext context ) {
            if ( !isDraggingCueBall ) {
                AimEnded?.Invoke();
                return;
            }

            isDraggingCueBall = false;
            if ( cueRigidbody != null )
                cueRigidbody.isKinematic = cueBallWasKinematic;
            AimEnded?.Invoke();
        }

        void UpdateAimFromScreen( Vector2 screenPosition ) {
            if ( tableCamera == null )
                return;

            Ray ray = tableCamera.ScreenPointToRay(screenPosition);
            if ( !Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tableMask, QueryTriggerInteraction.Ignore) )
                return;

            currentAimPosition = hit.point;
            AimUpdated?.Invoke();
        }

        bool TryMoveCueBall( Vector2 screenPosition ) {
            if ( tableCamera == null || cueBall == null || cueCollider == null )
                return false;

            Ray ray = tableCamera.ScreenPointToRay(screenPosition);
            if ( !Physics.Raycast(ray, out RaycastHit tableHit, Mathf.Infinity, tableMask, QueryTriggerInteraction.Ignore) )
                return false;

            Vector3 candidate = tableHit.point;
            candidate.y = cueBall.transform.position.y;
            float radius = Mathf.Max(cueCollider.bounds.extents.x, cueCollider.bounds.extents.z);
            foreach ( Collider overlap in Physics.OverlapSphere(candidate, radius, collisionMask, QueryTriggerInteraction.Ignore) ) {
                if ( overlap != cueCollider )
                    return false;
            }

            cueRigidbody.position = candidate;
            return true;
        }

        public bool TryGetAimDirection( out Vector3 direction ) {
            direction = Vector3.zero;
            if ( cueBall == null )
                return false;

            direction = Vector3.ProjectOnPlane(currentAimPosition - cueBall.transform.position, Vector3.up);
            if ( direction.sqrMagnitude < 0.0001f )
                return false;

            direction.Normalize();
            return true;
        }

        bool IsShotRunning() {
            return gameState != null && gameState.ShotInProgress;
        }
    }
}

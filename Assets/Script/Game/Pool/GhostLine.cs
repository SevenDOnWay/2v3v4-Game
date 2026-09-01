using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>Draws the predicted cue and object-ball paths for the current aim.</summary>
    public sealed class GhostLine : MonoBehaviour {
        [Header("References")]
        [SerializeField] PoolController controller;
        [SerializeField] PoolGameState gameState;
        [SerializeField] CueBall cueBall;

        [Header("Guide renderers")]
        [SerializeField] LineRenderer cueBallGuide;
        [SerializeField] LineRenderer objectBallGuide;
        [SerializeField] LineRenderer cueBallDeflectionGuide;
        [SerializeField] LineRenderer contactPointGuide;

        [Header("Physics")]
        [SerializeField] LayerMask collisionMask = 1 << 10;
        [SerializeField, Min(0.1f)] float cueBallGuideLength = 3f;
        [SerializeField, Min(0.1f)] float objectBallGuideLength = 0.6f;
        [SerializeField, Min(0.1f)] float cueBallDeflectionGuideLength = 0.6f;
        [SerializeField, Min(0f)] float guideHeight = 0.01f;
        [SerializeField, Range(8, 64)] int contactCircleSegments = 24;
        [SerializeField, Min(0.1f)] float contactCircleRadiusScale = 1f;
        [SerializeField] Color legalContactColor = new(1f, 0.55f, 0.1f, 1f);
        [SerializeField] Color illegalContactColor = new(1f, 0.12f, 0.12f, 1f);

        Collider cueBallCollider;

        void Awake() {
            controller ??= FindFirstObjectByType<PoolController>();
            gameState ??= FindFirstObjectByType<PoolGameState>();
            cueBall ??= FindFirstObjectByType<CueBall>();
            cueBallCollider = cueBall != null ? cueBall.GetComponent<Collider>() : null;
            SetGuideVisible(false);
        }

        public void Configure( PoolController inputController, PoolGameState state, CueBall ball,
            LineRenderer cueGuide, LineRenderer objectGuide, LineRenderer deflectionGuide,
            LineRenderer contactGuide, LayerMask mask, float mainGuideLength, float objectGuideLength,
            float deflectionGuideLength, float height, int circleSegments, float circleRadiusScale ) {
            controller ??= inputController;
            gameState ??= state;
            cueBall ??= ball;
            cueBallCollider ??= cueBall != null ? cueBall.GetComponent<Collider>() : null;
            cueBallGuide ??= cueGuide;
            objectBallGuide ??= objectGuide;
            cueBallDeflectionGuide ??= deflectionGuide;
            contactPointGuide ??= contactGuide;
            collisionMask = mask;
            cueBallGuideLength = mainGuideLength;
            objectBallGuideLength = objectGuideLength;
            cueBallDeflectionGuideLength = deflectionGuideLength;
            guideHeight = height;
            contactCircleSegments = circleSegments;
            contactCircleRadiusScale = circleRadiusScale;
        }

        void OnEnable() {
            if ( controller == null )
                return;

            controller.AimUpdated += DrawGhostLine;
            controller.AimEnded += HideGhostLine;
        }

        void OnDisable() {
            if ( controller == null )
                return;

            controller.AimUpdated -= DrawGhostLine;
            controller.AimEnded -= HideGhostLine;
        }

        void DrawGhostLine() {
            if ( cueBall == null || cueBallCollider == null || controller == null
                || (gameState != null && gameState.ShotInProgress)
                || !controller.TryGetAimDirection(out Vector3 direction) ) {
                SetGuideVisible(false);
                return;
            }

            Vector3 cueStart = cueBall.transform.position;
            float cueRadius = GetRadius(cueBallCollider);
            TryGetFirstHit(cueStart, cueRadius, direction, cueBallCollider, null, cueBallGuideLength, out RaycastHit firstHit);
            Vector3 cueEnd = firstHit.collider == null
                ? cueStart + direction * cueBallGuideLength
                : cueStart + direction * firstHit.distance;
            SetLine(cueBallGuide, cueStart, cueEnd);

            if ( firstHit.collider == null || !IsObjectBall(firstHit.collider) ) {
                SetGuideVisible(cueBallGuide != null);
                return;
            }

            bool legalTarget = IsLegalTarget(firstHit.collider);
            SetContactPointColor(legalTarget);
            DrawContactCircle(firstHit.point, cueRadius * contactCircleRadiusScale);
            if ( !legalTarget ) {
                SetGuideVisible(cueBallGuide != null, false, contactPointGuide != null, false);
                return;
            }

            Vector3 normal = Vector3.ProjectOnPlane(firstHit.normal, Vector3.up).normalized;
            Vector3 objectDirection = Vector3.Project(direction, -normal).normalized;
            if ( objectDirection.sqrMagnitude < 0.0001f ) {
                SetGuideVisible(cueBallGuide != null, false, contactPointGuide != null, false);
                return;
            }

            Collider objectCollider = firstHit.collider;
            Vector3 objectStart = objectCollider.bounds.center;
            objectStart.y = cueStart.y;
            float objectRadius = GetRadius(objectCollider);
            TryGetFirstHit(objectStart + objectDirection * 0.001f, objectRadius, objectDirection,
                objectCollider, cueBallCollider, objectBallGuideLength, out RaycastHit objectHit);
            Vector3 objectEnd = objectHit.collider == null
                ? objectStart + objectDirection * objectBallGuideLength
                : objectStart + objectDirection * objectHit.distance;
            SetLine(objectBallGuide, objectStart, objectEnd);

            Vector3 cueDeflectionDirection = Vector3.ProjectOnPlane(direction, -normal);
            bool hasCueDeflection = cueDeflectionDirection.sqrMagnitude > 0.0001f;
            if ( hasCueDeflection ) {
                cueDeflectionDirection.Normalize();
                TryGetFirstHit(cueEnd + cueDeflectionDirection * 0.001f, cueRadius, cueDeflectionDirection,
                    cueBallCollider, objectCollider, cueBallDeflectionGuideLength, out RaycastHit deflectionHit);
                Vector3 deflectionEnd = deflectionHit.collider == null
                    ? cueEnd + cueDeflectionDirection * cueBallDeflectionGuideLength
                    : cueEnd + cueDeflectionDirection * deflectionHit.distance;
                SetLine(cueBallDeflectionGuide, cueEnd, deflectionEnd);
            }

            SetGuideVisible(cueBallGuide != null, objectBallGuide != null,
                contactPointGuide != null, hasCueDeflection && cueBallDeflectionGuide != null);
        }

        void HideGhostLine() => SetGuideVisible(false);

        bool IsLegalTarget( Collider collider ) {
            PoolBall ball = collider.GetComponent<PoolBall>();
            return ball != null && (gameState == null || !gameState.GroupsDecided
                || ball.Type == gameState.GetBallTypeFor(gameState.CurrentTurn));
        }

        bool TryGetFirstHit( Vector3 origin, float radius, Vector3 direction, Collider ignoredA,
            Collider ignoredB, float maxDistance, out RaycastHit firstHit ) {
            firstHit = default;
            float closestDistance = float.PositiveInfinity;
            foreach ( RaycastHit hit in Physics.SphereCastAll(origin, radius, direction, maxDistance,
                         collisionMask, QueryTriggerInteraction.Ignore) ) {
                if ( hit.collider == null || hit.collider == ignoredA || hit.collider == ignoredB
                    || hit.distance >= closestDistance )
                    continue;

                closestDistance = hit.distance;
                firstHit = hit;
            }

            return firstHit.collider != null;
        }

        static bool IsObjectBall( Collider collider ) => collider != null && collider.GetComponent<PoolBall>() != null;

        static float GetRadius( Collider collider ) {
            Vector3 extents = collider.bounds.extents;
            return Mathf.Max(extents.x, extents.z);
        }

        void SetContactPointColor( bool isLegal ) {
            if ( contactPointGuide == null )
                return;

            Color color = isLegal ? legalContactColor : illegalContactColor;
            contactPointGuide.startColor = color;
            contactPointGuide.endColor = color;
        }

        void DrawContactCircle( Vector3 contactPoint, float radius ) {
            if ( contactPointGuide == null )
                return;

            contactPointGuide.positionCount = contactCircleSegments + 1;
            for ( int index = 0; index <= contactCircleSegments; index++ ) {
                float angle = index * Mathf.PI * 2f / contactCircleSegments;
                Vector3 point = contactPoint + new Vector3(Mathf.Cos(angle), guideHeight, Mathf.Sin(angle)) * radius;
                point.y = contactPoint.y + guideHeight;
                contactPointGuide.SetPosition(index, point);
            }
        }

        void SetLine( LineRenderer line, Vector3 start, Vector3 end ) {
            if ( line == null )
                return;

            start.y += guideHeight;
            end.y += guideHeight;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        void SetGuideVisible( bool cueVisible, bool objectVisible = false, bool contactVisible = false,
            bool cueDeflectionVisible = false ) {
            if ( cueBallGuide != null ) cueBallGuide.enabled = cueVisible;
            if ( objectBallGuide != null ) objectBallGuide.enabled = objectVisible;
            if ( contactPointGuide != null ) contactPointGuide.enabled = contactVisible;
            if ( cueBallDeflectionGuide != null ) cueBallDeflectionGuide.enabled = cueDeflectionVisible;
        }
    }
}

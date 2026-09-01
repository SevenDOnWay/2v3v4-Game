using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// Scene composition root retained for existing scenes. It creates the modular
    /// components but owns no shot, pocket, turn, or ball-group rules.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour {
        [Header("Extracted components")]
        [SerializeField] PoolGameState gameState;
        [SerializeField] PoolController poolController;
        [SerializeField] BallMotionTracker ballMotionTracker;
        [SerializeField] PoolRules poolRules;
        [SerializeField] GhostLine ghostLine;
        [SerializeField] CueBall cueBall;

        // These fields keep the existing GameManager scene assignments alive while
        // the visual guide moves into GhostLine.
        [Header("Legacy guide references")]
        [SerializeField] LineRenderer cueBallGuide;
        [SerializeField] LineRenderer objectBallGuide;
        [SerializeField] LineRenderer cueBallDeflectionGuide;
        [SerializeField] LineRenderer contactPointGuide;
        [SerializeField] LayerMask collisionMask = 1 << 10;
        [SerializeField, Min(0.1f)] float guideLength = 3f;
        [SerializeField, Min(0.1f)] float objectBallGuideLength = 0.6f;
        [SerializeField, Min(0.1f)] float cueBallDeflectionGuideLength = 0.6f;
        [SerializeField, Min(0f)] float guideHeight = 0.01f;
        [SerializeField, Range(8, 64)] int contactCircleSegments = 24;
        [SerializeField, Min(0.1f)] float contactCircleRadiusScale = 1f;

        public IGameState State => gameState;

        void Awake() {
            gameState ??= EnsureComponent<PoolGameState>();
            poolController ??= EnsureComponent<PoolController>();
            ballMotionTracker ??= EnsureComponent<BallMotionTracker>();
            poolRules ??= EnsureComponent<PoolRules>();
            ghostLine ??= EnsureComponent<GhostLine>();
            cueBall ??= FindFirstObjectByType<CueBall>();

            ghostLine.Configure(poolController, gameState, cueBall, cueBallGuide,
                objectBallGuide, cueBallDeflectionGuide, contactPointGuide, collisionMask,
                guideLength, objectBallGuideLength, cueBallDeflectionGuideLength, guideHeight,
                contactCircleSegments, contactCircleRadiusScale);
            EnsureBallGroupUi();
        }

        T EnsureComponent<T>() where T : Component {
            T existing = FindFirstObjectByType<T>();
            return existing != null ? existing : gameObject.AddComponent<T>();
        }

        void EnsureBallGroupUi() {
            if ( FindFirstObjectByType<BallGroupUIManager>() != null )
                return;

            GameObject hud = GameObject.Find("Ball Group HUD Prototype");
            Canvas canvas = hud != null ? hud.GetComponentInParent<Canvas>() : null;
            if ( canvas != null )
                canvas.gameObject.AddComponent<BallGroupUIManager>();
        }
    }
}

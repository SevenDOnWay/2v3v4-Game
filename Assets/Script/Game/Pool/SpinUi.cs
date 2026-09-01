using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// Opens and controls a cue-ball spin selector. Drag the marker around the ball:
    /// horizontal movement selects left/right English and vertical movement selects follow/draw.
    /// </summary>
    public class SpinUi : MonoBehaviour, IPointerDownHandler, IDragHandler {

        [Header("Sprites")]
        [SerializeField] Sprite icon;
        [SerializeField] Sprite dot;

        [Header("UI")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image cueBallImage;
        [SerializeField] Image contactDot;
        [SerializeField] Color contactDotColor = new(0.9f, 0.08f, 0.08f, 1f);
        [SerializeField, Range(0.1f, 1f)] float maxContactRadius = 0.7f;
        [SerializeField] float contactDotSize = 24f;

        [Header("Events")]
        [SerializeField] UnityEvent<Vector2> spinChanged;

        Button btn;

        /// <summary>Normalized cue-tip contact point: X = side spin, Y = follow/draw.</summary>
        public Vector2 Spin { get; private set; }
        public float SideSpin => Spin.x;
        public float FollowDraw => Spin.y;
        public UnityEvent<Vector2> SpinChanged => spinChanged;

        void Awake() {
            EnsureVisuals();
            SetSpin(Spin, false);
        }

        void OnEnable() {
            btn ??= GetComponent<Button>();
            if ( btn != null )
                btn.onClick.AddListener(TurnOnUIForSpin);
        }

        void OnDisable() {
            if ( btn != null )
                btn.onClick.RemoveListener(TurnOnUIForSpin);
        }

        void OnValidate() {
            maxContactRadius = Mathf.Clamp(maxContactRadius, 0.1f, 1f);
            contactDotSize = Mathf.Max(1f, contactDotSize);

            if ( Application.isPlaying ) SetSpin(Spin, false);
        }

        public void OnPointerDown( PointerEventData eventData ) {
            UpdateSpinFromPointer(eventData);
        }

        public void OnDrag( PointerEventData eventData ) {
            UpdateSpinFromPointer(eventData);
        }

        /// <summary>Sets the selector with values in the -1..1 range.</summary>
        public void SetSpin( Vector2 normalizedSpin ) {
            SetSpin(normalizedSpin, true);
        }

        public void ResetSpin() {
            SetSpin(Vector2.zero);
        }

        public Vector3 GetCueTipOffset( float ballRadius, Vector3 cueRight, Vector3 tableUp ) {
            float offset = ballRadius * maxContactRadius;
            return cueRight.normalized * (SideSpin * offset) + tableUp.normalized * (FollowDraw * offset);
        }

        public void TurnOnUIForSpin() {
            if ( canvasGroup == null )
                return;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void TurnOffUIForSpin() {
            if ( canvasGroup == null )
                return;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        void UpdateSpinFromPointer( PointerEventData eventData ) {
            if ( cueBallImage == null )
                return;

            RectTransform rect = cueBallImage.rectTransform;
            if ( !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint) )
                return;

            Vector2 halfSize = rect.rect.size * 0.5f;
            if ( halfSize.x <= 0f || halfSize.y <= 0f )
                return;

            Vector2 pointOnBall = new(localPoint.x / halfSize.x, localPoint.y / halfSize.y);
            SetSpin(pointOnBall / maxContactRadius, true);
        }

        void SetSpin( Vector2 normalizedSpin, bool notify ) {
            Spin = Vector2.ClampMagnitude(normalizedSpin, 1f);
            UpdateContactDotPosition();

            if ( notify ) spinChanged?.Invoke(Spin);
        }

        void UpdateContactDotPosition() {
            if ( cueBallImage == null || contactDot == null )
                return;

            Rect rect = cueBallImage.rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f * maxContactRadius;
            contactDot.rectTransform.anchoredPosition = Spin * radius;
        }

        void EnsureVisuals() {
            cueBallImage ??= GetComponent<Image>();
            if ( cueBallImage == null )
                return;

            if ( icon != null )
                cueBallImage.sprite = icon;

            cueBallImage.preserveAspect = true;
            cueBallImage.raycastTarget = true;

            if ( contactDot == null && dot != null )
                contactDot = CreateContactDot();

            if ( contactDot == null )
                return;

            contactDot.sprite = dot != null ? dot : contactDot.sprite;
            contactDot.color = contactDotColor;
            contactDot.raycastTarget = false;
            contactDot.preserveAspect = true;
        }

        Image CreateContactDot() {
            GameObject marker = new("Contact Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            marker.transform.SetParent(cueBallImage.transform, false);

            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.one * contactDotSize;

            return marker.GetComponent<Image>();
        }

    }
}

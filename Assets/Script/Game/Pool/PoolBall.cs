using UnityEngine;

namespace Assets.Script.Game.Pool {
    /// <summary>
    /// Static identity data for one numbered object ball. This belongs on the
    /// ball prefab/GameObject; it is not per-shot gameplay logic.
    /// </summary>
    public sealed class PoolBall : MonoBehaviour {
        [SerializeField, Range(1, 15)] int number;
        [SerializeField] BallType ballType;

        public int Number {
            get {
                if ( number > 0 )
                    return number;

                // Compatibility fallback for balls already placed in the
                // scene before the number field was added.
                string objectName = gameObject.name;
                string numberText = objectName.StartsWith("Ball ")
                    ? objectName.Substring(5)
                    : objectName.StartsWith("Ball")
                        ? objectName.Substring(4)
                        : string.Empty;

                return int.TryParse(numberText, out int parsedNumber)
                    ? parsedNumber
                    : 0;
            }
        }
        public BallType Type => ballType;
    }
}

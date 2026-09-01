using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Script.Game.Pool {
    public class GameInput : MonoBehaviour {
        [SerializeField] InputActionReference aimPosition;
        [SerializeField] InputActionReference aimPress;

        public InputAction AimPosition => aimPosition.action;
        public InputAction AimPress => aimPress.action;
    }
}

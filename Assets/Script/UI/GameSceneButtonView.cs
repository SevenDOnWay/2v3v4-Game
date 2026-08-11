using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.UI {
    [RequireComponent(typeof(Button))]
    public class GameSceneButtonView : MonoBehaviour {

        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text sceneNameText;

        private void Reset() {
            button = GetComponent<Button>();
        }

        public void Bind(GameScene gameScene, Action<GameScene> onSelected) {
            if ( button == null )
                button = GetComponent<Button>();

            if ( iconImage != null ) {
                iconImage.sprite = gameScene.Icon;
                iconImage.enabled = gameScene.Icon != null;
            }

            if ( sceneNameText != null )
                sceneNameText.text = gameScene.SceneName;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(gameScene));
        }
    }
}

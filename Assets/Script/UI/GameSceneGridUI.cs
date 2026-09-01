using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace Assets.Script.UI {
    [Serializable]
    public class GameSceneSelectedEvent : UnityEvent<GameScene> { }

    /// <summary>
    /// Builds a UGUI button grid from the data cached by GameSceneLoader.
    /// GridLayoutGroup controls all button positioning.
    /// </summary>
    public class GameSceneGridUI : MonoBehaviour {

        [SerializeField] private GameSceneLoader gameSceneLoader;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private GameSceneButtonView buttonPrefab;
        [SerializeField] private GameSceneSelectedEvent onGameSceneSelected;

        public event Action<GameScene> GameSceneSelected;

        [Inject]
        public void Construct(GameSceneLoader loader) {
            gameSceneLoader = loader;
        }

        private async void Start() {
            if ( gameSceneLoader == null ) {
                Debug.LogError("[GameSceneGridUI] GameSceneLoader was not injected. Register this component in a LifetimeScope.", this);
                return;
            }

            if ( gridLayout == null || buttonPrefab == null ) {
                Debug.LogError("[GameSceneGridUI] Assign a GridLayoutGroup and button prefab.", this);
                return;
            }

            try {
                await RebuildAsync();
            }
            catch ( Exception exception ) {
                Debug.LogException(exception, this);
            }
        }

        public async System.Threading.Tasks.Task RebuildAsync() {
            IReadOnlyList<GameScene> gameScenes = await gameSceneLoader.GetGameScenes();

            ClearButtons();

            foreach ( GameScene gameScene in gameScenes ) {
                GameSceneButtonView buttonView = Instantiate(buttonPrefab, gridLayout.transform);
                buttonView.Bind(gameScene, SelectGameScene);
            }
        }

        private void SelectGameScene(GameScene gameScene) {
            GameSceneSelected?.Invoke(gameScene);
            onGameSceneSelected?.Invoke(gameScene);
        }

        private void ClearButtons() {
            for ( int index = gridLayout.transform.childCount - 1; index >= 0; index-- )
                Destroy(gridLayout.transform.GetChild(index).gameObject);
        }
    }
}

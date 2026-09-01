using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;

namespace Assets.Script {
    /// <summary>
    /// VContainer-managed cache for GameScene ScriptableObject data.
    /// This service intentionally does not load Unity scenes.
    /// </summary>
    public sealed class GameSceneLoader : IStartable, IDisposable {

        private const string GameSceneLabel = "GameScene";

        private readonly List<GameScene> gameScenes = new();
        private AsyncOperationHandle<IList<GameScene>> gameScenesHandle;
        private Task<IReadOnlyList<GameScene>> loadingTask;
        private bool hasLoadedGameScenes;

        public void Start() {
            _ = PreloadAsync();
        }

        private async Task PreloadAsync() {
            try {
                await GetGameScenes();
            }
            catch ( Exception exception ) {
                Debug.LogException(exception);
            }
        }

        public async Task<IReadOnlyList<GameScene>> GetGameScenes() {
            if ( hasLoadedGameScenes )
                return gameScenes;

            loadingTask ??= LoadGameScenesAsync();

            try {
                return await loadingTask;
            }
            catch {
                loadingTask = null;
                throw;
            }
        }

        private async Task<IReadOnlyList<GameScene>> LoadGameScenesAsync() {
            gameScenesHandle = Addressables.LoadAssetsAsync<GameScene>(GameSceneLabel, null);
            IList<GameScene> loadedGameScenes = await gameScenesHandle.Task;

            if ( gameScenesHandle.Status != AsyncOperationStatus.Succeeded ) {
                Addressables.Release(gameScenesHandle);
                throw new Exception($"[GameSceneLoader] Failed to load GameScene data with label '{GameSceneLabel}'.");
            }

            gameScenes.Clear();
            gameScenes.AddRange(loadedGameScenes);
            gameScenes.Sort(( left, right ) => left.Id.CompareTo(right.Id));
            hasLoadedGameScenes = true;

            Debug.Log($"[GameSceneLoader] Loaded {gameScenes.Count} GameScene data assets.");
            return gameScenes;
        }

        public async Task<GameScene> GetGameSceneById( int id ) {
            IReadOnlyList<GameScene> loadedGameScenes = await GetGameScenes();

            foreach ( GameScene gameScene in loadedGameScenes ) {
                if ( gameScene.Id == id )
                    return gameScene;
            }

            Debug.LogWarning($"[GameSceneLoader] GameScene with ID {id} was not found.");
            return null;
        }

        public void Dispose() {
            if ( gameScenesHandle.IsValid() )
                Addressables.Release(gameScenesHandle);
        }
    }
}

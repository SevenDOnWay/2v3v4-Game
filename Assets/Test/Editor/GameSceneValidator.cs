using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Test.Editor {
    public static class GameSceneValidator {

        private const int FirstSceneId = 1;

        [MenuItem("Tools/Game Scenes/Validate IDs")]
        public static void ValidateIds() {
            string[] guids = AssetDatabase.FindAssets("t:GameScene");

            List<GameScene> gameScenes = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameScene>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(gameScene => gameScene != null)
            .OrderBy(gameScene => gameScene.Id)
            .ToList();

            if ( gameScenes.Count == 0 ) {
                Debug.LogWarning("[GameSceneValidator] No GameScene assets found.");
                return;
            }

            List<string> errors = new();

            // IDs must start at 1 or higher.
            foreach ( GameScene gameScene in gameScenes.Where(x => x.Id < FirstSceneId) ) {
                errors.Add(
                    $"'{gameScene.name}' has invalid ID {gameScene.Id}. " +
                    $"IDs must start at {FirstSceneId}.");
            }

            // Duplicate IDs.
            foreach ( IGrouping<int, GameScene> group in gameScenes.GroupBy(x => x.Id) ) {
                if ( group.Count() > 1 ) {
                    errors.Add(
                        $"Duplicate ID {group.Key}: " +
                        string.Join(", ", group.Select(x => x.name)));
                }
            }

            // Missing IDs, e.g. 1, 2, 4 means ID 3 is missing.
            HashSet<int> ids = gameScenes.Select(x => x.Id).ToHashSet();
            int highestId = gameScenes.Max(x => x.Id);

            for ( int id = FirstSceneId; id <= highestId; id++ ) {
                if ( !ids.Contains(id) )
                    errors.Add($"Missing GameScene ID: {id}");
            }

            if ( errors.Count == 0 ) {
                Debug.Log(
                    $"[GameSceneValidator] Valid: {gameScenes.Count} GameScene assets, " +
                    $"IDs {FirstSceneId}–{highestId}.");
                return;
            }

            Debug.LogError(
                "[GameSceneValidator] GameScene ID validation failed:\n- " +
                string.Join("\n- ", errors));
        }
    }
}
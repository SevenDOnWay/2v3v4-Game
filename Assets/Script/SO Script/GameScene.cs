using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GameScene", menuName = "Scriptable Objects/GameScene")]

public class GameScene : ScriptableObject {
    [SerializeField,Min(1)] int id;
    [SerializeField] string sceneName;
    [SerializeField] Sprite icon;
    [SerializeField] AssetReference scene;
    public int Id => id;
    public string SceneName => sceneName;
    public Sprite Icon => icon;
    public AssetReference Scene => scene;


    private void OnValidate() {
        if ( scene == null || !scene.RuntimeKeyIsValid() ) Debug.LogWarning($"[GameScene] Scene is not assigned in {name}.", this);

    }
}

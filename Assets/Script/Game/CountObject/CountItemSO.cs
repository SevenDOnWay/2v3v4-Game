using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Script.Game.CountObject {
    [CreateAssetMenu(fileName = "CountItemSO", menuName = "SO/CountItemSO")]
    public class CountItemSO : ScriptableObject {

        [SerializeField, Min(0)] public int id;
        [SerializeField] public new string name;
        [SerializeField] public GameObject prefab;
    }
}
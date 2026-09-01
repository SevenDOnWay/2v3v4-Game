using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Script.Game.CountObject {
    [CreateAssetMenu(fileName = "AnimalSO", menuName = "SO/AnimalSO")]
    public class AnimalSO : ScriptableObject {

        [SerializeField, Min(0)] public int id;
        [SerializeField] public new string name;
        [SerializeField] public GameObject prefab;
    }
}
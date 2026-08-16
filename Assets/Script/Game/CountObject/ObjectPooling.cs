using Codice.CM.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Assets.Script.Game.CountObject {
    public class ObjectPooling : MonoBehaviour {
        [Header("Inject")]
        AddressableLoader addressableLoader;

        [SerializeField] Transform poolRoot;

        private readonly Dictionary<CountItemSO, Stack<GameObject>> poolDictionary = new();
        private readonly Dictionary<CountItemSO, Transform> rootDictionary = new();

        //Commnet this if not using with addressable.
        IReadOnlyList<CountItemSO> loadedSO;

        [Inject]
        void Construct( AddressableLoader addressableLoader ) {
            this.addressableLoader = addressableLoader;
        }


        async void Start() {
            if ( poolRoot == null ) poolRoot = gameObject.transform;
            loadedSO = await addressableLoader.LoadSOAsync();
            //Debug.Log($"loadSO count {loadedSO.Count}");

            InitializePools();
        }


        private void InitializePools() {
            foreach ( CountItemSO item in loadedSO ) {
                poolDictionary[item] = new Stack<GameObject>();

                GameObject root = new GameObject($"{item.name}_Pool");
                root.transform.SetParent(poolRoot);

                rootDictionary[item] = root.transform;
            }
        }


        public List<GameObject> Get( CountItemSO countItemSO, int number = 1 ) {
            Transform speciesRoot = GetOrCreateSpeciesRoot(countItemSO);
            Stack<GameObject> stack = GetOrCreateStack(countItemSO);

            List<GameObject> instances = new(number);

            for ( int i = 0; i < number; i++ ) {
                GameObject instance = null;

                while ( stack.Count > 0 ) {
                    instance = stack.Pop();

                    if ( instance != null )
                        break;
                }

                if ( instance == null ) instance = Instantiate(countItemSO.prefab, speciesRoot);

                instance.SetActive(true);
                instances.Add(instance);
            }

            return instances;
        }

        public void Release( GameObject instance, CountItemSO CountItemSO ) {
            if ( instance == null ) return;

            Transform speciesRoot = GetOrCreateSpeciesRoot(CountItemSO);
            Stack<GameObject> stack = GetOrCreateStack(CountItemSO);

            instance.SetActive(false);
            instance.transform.SetParent(speciesRoot, false);
            stack.Push(instance);
        }

        private Stack<GameObject> GetOrCreateStack( CountItemSO CountItemSO ) {
            if ( !poolDictionary.TryGetValue(CountItemSO, out Stack<GameObject> stack) ) {
                stack = new Stack<GameObject>();
                poolDictionary[CountItemSO] = stack;
            }
            return stack;
        }

        private Transform GetOrCreateSpeciesRoot( CountItemSO CountItemSO ) {
            if ( !rootDictionary.TryGetValue(CountItemSO, out Transform speciesRoot) || speciesRoot == null ) {
                GameObject groupObj = new GameObject($"{CountItemSO}_Pool");
                groupObj.transform.position = new Vector3(100, 100, 100); //set pool to out of the screen
                groupObj.transform.SetParent(poolRoot, true);
                speciesRoot = groupObj.transform;
                rootDictionary[CountItemSO] = speciesRoot;
            }
            return speciesRoot;
        }
    }
}
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Script.Game.CountObject {
    public class AddressableLoader : MonoBehaviour {
        [SerializeField]
        private AssetLabelReference countItemLabel;

        private AsyncOperationHandle<IList<AnimalSO>> loadHandle;
        private bool hasLoadHandle;

        public async Task<IReadOnlyList<AnimalSO>> LoadSOAsync() {
            // Already requested before.
            if ( hasLoadHandle ) {
                if ( !loadHandle.IsDone ) await loadHandle.Task;

                return (IReadOnlyList<AnimalSO>)(loadHandle.Status == AsyncOperationStatus.Succeeded ? loadHandle.Result : new List<AnimalSO>());
            }

            loadHandle =
                Addressables.LoadAssetsAsync<AnimalSO>(
                    countItemLabel.RuntimeKey,
                    null
                );

            hasLoadHandle = true;

            await loadHandle.Task;

            if ( loadHandle.Status != AsyncOperationStatus.Succeeded ) {
                Debug.LogError("Failed to load AnimalSO assets.");
                return new List<AnimalSO>();
            }

            return (IReadOnlyList<AnimalSO>)loadHandle.Result;
        }

        private void OnDestroy() {
            if ( hasLoadHandle && loadHandle.IsValid() ) {
                Addressables.Release(loadHandle);
            }
        }
    }
}
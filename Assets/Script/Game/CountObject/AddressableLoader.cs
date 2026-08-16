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

        private AsyncOperationHandle<IList<CountItemSO>> loadHandle;
        private bool hasLoadHandle;

        public async Task<IReadOnlyList<CountItemSO>> LoadSOAsync() {
            // Already requested before.
            if ( hasLoadHandle ) {
                if ( !loadHandle.IsDone ) await loadHandle.Task;

                return (IReadOnlyList<CountItemSO>)(loadHandle.Status == AsyncOperationStatus.Succeeded ? loadHandle.Result : new List<CountItemSO>());
            }

            loadHandle =
                Addressables.LoadAssetsAsync<CountItemSO>(
                    countItemLabel.RuntimeKey,
                    null
                );

            hasLoadHandle = true;

            await loadHandle.Task;

            if ( loadHandle.Status != AsyncOperationStatus.Succeeded ) {
                Debug.LogError("Failed to load CountItemSO assets.");
                return new List<CountItemSO>();
            }

            return (IReadOnlyList<CountItemSO>)loadHandle.Result;
        }

        private void OnDestroy() {
            if ( hasLoadHandle && loadHandle.IsValid() ) {
                Addressables.Release(loadHandle);
            }
        }
    }
}
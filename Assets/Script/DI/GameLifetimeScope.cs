using UnityEngine;
using VContainer;
using VContainer.Unity;
using Assets.Script.UI;

namespace Assets.Script.DI {
    /// <summary>
    /// Place one instance in the first scene. It persists for the app lifetime.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope {

        [SerializeField] private GameSceneGridUI gameSceneGridUI;

        protected override void Awake() {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder) {
            builder.Register<GameSceneLoader>(Lifetime.Singleton)
                .AsSelf()
                .As<IStartable>();

            if ( gameSceneGridUI != null )
                builder.RegisterComponent(gameSceneGridUI);
        }
    }
}

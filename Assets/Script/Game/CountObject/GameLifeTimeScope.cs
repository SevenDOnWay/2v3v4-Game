using Assets.Script.Game.CountObject.UI;
using System.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Assets.Script.Game.CountObject {
    public class GameLifeTimeScope : LifetimeScope{

        protected override void Configure(IContainerBuilder builder) {
            builder.Register<PlayerContract>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<AddressableLoader>();
            builder.RegisterComponentInHierarchy<ObjectPooling>();
            builder.RegisterComponentInHierarchy<GridScript>();
            builder.RegisterComponentInHierarchy<UIManager>();
            builder.RegisterComponentInHierarchy<GameState>();

            builder.RegisterComponentInHierarchy<ClockUI>();

            }


    }
}
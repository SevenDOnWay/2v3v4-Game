using Codice.Utils.Buffers;
using System.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Assets.Script.Game.Pool {
    public class PoolGameLifeTimeScope : LifetimeScope {

        protected override void Configure( IContainerBuilder builder ) {
            builder.RegisterComponentInHierarchy<GameInput>();
            builder.RegisterComponentInHierarchy<ForceSlider>();
            builder.RegisterComponentInHierarchy<SpinUi>();

            builder.RegisterComponentInHierarchy<PoolController>();



        }


    }
}
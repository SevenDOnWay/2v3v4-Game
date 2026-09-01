using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.Game.CountObject {
    public class PlayerContract {

        List<PlayerInfo> playerInfos;
        
        public void AddRange(List<PlayerInfo> playerInfos) {
            this.playerInfos = new List<PlayerInfo>(playerInfos);
        }
        public int GetPlayerCount() => playerInfos.Count;
    
    }
}
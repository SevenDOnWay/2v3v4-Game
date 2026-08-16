using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Game.CountObject {
    public class PlayerInfo {
        //TODO: change into img catalog, for better memory.
        public Image img;
        public new string name;

        public PlayerInfo( Image img, string name ) { 
            this.img = img;
            this.name = name;
        }
    }
}
using Assets.Script.Game.CountObject.Effect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.Game.CountObject {
    public class ThisTurnContract : MonoBehaviour {

        

        Dictionary<Vector2Int, int> grid = new();

        List<CountItemSO> itemThisTurn = new(); //SO type of this , TODO: rename
        Dictionary<int, List<GameObject>> gameObjectThisTurn = new(); //list of gameobject use for relase to object pool, TODO: rename

        
        public void ApplyEffct() {
            for(int i  =  0; i < 5; i++ ) {
                for(int j = 0; j < 5; j++ ) {

                    

                }
            }
        }
        


    }

    [Serializable]
    public class CellData {
        public bool isOccupied = false;

        public int id;
        public GameObject gameObject;
        public CountItemSO countItemSO;
        public CorrectEffect correctEffect;

    }

}
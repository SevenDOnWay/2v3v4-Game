using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Game.CountObject.UI {
    public class PlayerSelectionManager : MonoBehaviour {

        [SerializeField] GameObject playerUIPrefab;
        [SerializeField] Button PlayBtn;

        List<PlayerSelection> playerSelectionList = new(); //FUTURE: might use this for wining scene (pedal scene top1,2,3,...)

        public event Action<List<PlayerInfo>> OnPlay;

        public void OnEnable() {
            PlayBtn?.onClick.AddListener(Play);
        }

        public void OnDisable() {
            PlayBtn?.onClick?.RemoveListener(Play);
        }

        public void Start() {
            AddNewPlayerSection();
        }

        void AddNewPlayerSection() {
            var temp = Instantiate(playerUIPrefab, this.gameObject.transform);
            var script = temp.GetComponent<PlayerSelection>();

            script.SetState(false);
            playerSelectionList.Add(script);
            if ( playerSelectionList.Count < 4 ) {
                AddNewPlayerSection();
            }

        }

        public void Play() {
            List<PlayerInfo> temp = new();

            foreach ( PlayerSelection selection in playerSelectionList ) {
                if ( selection.isPlay == true ) {
                    selection.UpdateOnPlay();
                    temp.Add(new PlayerInfo(selection.img, selection.name));
                }
            }

            if ( temp.Count <= 0 ) return;

            OnPlay?.Invoke(temp);

        }

    }
}
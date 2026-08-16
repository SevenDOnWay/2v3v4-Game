using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Script.Game.CountObject.UI {
    /// <summary>
    /// This class is responsible for handling player selection in the game.
    /// EX: how many players are playing, which player is selected, etc.
    /// </summary>
    public class PlayerSelection : MonoBehaviour {

        [SerializeField] Button addPlayerBtn;

        [SerializeField] public Image img;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Button removePlayerBtn;

        public new string name;


        public bool isPlay;


        public void OnEnable() {
            addPlayerBtn?.onClick.AddListener(AddPlayer);
            removePlayerBtn?.onClick.AddListener(RemovePlayer);
        }

        public void OnDisable() {
            addPlayerBtn?.onClick.RemoveListener(AddPlayer);
            removePlayerBtn?.onClick.RemoveListener(RemovePlayer);
        }

        /// <summary>
        /// Decide this panel will have player or 
        /// Ui asking for add user
        /// </summary>
        /// <param name="isPlay"></param>
        public void SetState(bool isPlay) {
            this.isPlay = isPlay;

            addPlayerBtn.gameObject.SetActive(!isPlay);

            img.gameObject.SetActive(isPlay);
            inputField.gameObject.SetActive(isPlay);
            removePlayerBtn.gameObject.SetActive(isPlay);
        }
        
        void AddPlayer() {
            isPlay = true;
            SetState(isPlay);
        }

        void RemovePlayer() { 
            isPlay = false;
            SetState(isPlay);
        }

        public void UpdateOnPlay() {
            //TDDO: make avatar support changable, and use that avatar to show character as the game end.

            name = inputField.text;
        }


    }
}
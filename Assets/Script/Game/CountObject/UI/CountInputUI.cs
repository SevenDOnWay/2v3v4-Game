using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Game.CountObject.UI {
    public class CountInputUI : MonoBehaviour {

        [SerializeField] Button increaseBtn;
        [SerializeField] Button decreaseBtn;
        [SerializeField] TextMeshProUGUI text;

        public int count = 0;

        void OnEnable() {
            increaseBtn?.onClick.AddListener(Increase);
            decreaseBtn?.onClick.AddListener(Decrease);
        }

        void OnDisable() {
            increaseBtn?.onClick.RemoveListener(Increase);
            decreaseBtn?.onClick.RemoveListener(Decrease);
        }

        void Increase() {
            count++;
            UpdateText();
        }

        void Decrease() {
            if ( count <= 0 ) return;

            count--;
            UpdateText();
        }


        public void UpdateText() {
            text.text = count.ToString();
        }
    }
}
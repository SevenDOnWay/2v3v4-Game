using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Game.CountObject.UI {
    public class ClockUI : MonoBehaviour {

        [SerializeField] private Image clockFillImage;

        private float totalDuration;
        private float timeRemaining;
        private bool isRunning;

        public void StartTimer( float duration ) {
            Debug.Log($"Start Timer {duration}");

            totalDuration = duration;
            timeRemaining = duration;
            clockFillImage.fillAmount = 1f;
            isRunning = true;
        }

        private void Update() {
            if ( !isRunning ) return;

            if ( timeRemaining > 0f ) {
                timeRemaining -= Time.deltaTime;
                clockFillImage.fillAmount = Mathf.Clamp01(timeRemaining / totalDuration);
            }
            else {
                clockFillImage.fillAmount = 0f;
                isRunning = false;
                Debug.Log("Time up!");
            }
        }



    }
}
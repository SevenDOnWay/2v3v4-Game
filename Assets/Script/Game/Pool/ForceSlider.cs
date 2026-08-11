using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Script.Game.Pool {
    public class ForceSlider : MonoBehaviour, IPointerUpHandler{

        Slider slider;

        public event Action<float> OnRelease;


        private void OnEnable() {
            slider = GetComponent<Slider>();

            if(slider == null ) {
                Debug.LogError("ForceSlider requires a Slider component.");
            }

        }

        public void OnPointerUp( PointerEventData eventData ) {
            if ( slider == null )
                return;

            Debug.Log("ForceSlider released. Value: " + slider.value);
            OnRelease?.Invoke(slider.value);

            StopAllCoroutines();
            StartCoroutine(ResetSlider());
        }

        IEnumerator ResetSlider() {
            while ( slider.value > 0f ) {
                slider.value -= Time.deltaTime * 2f;
                yield return null;
            }

            slider.value = 0f;
        }


    }
}

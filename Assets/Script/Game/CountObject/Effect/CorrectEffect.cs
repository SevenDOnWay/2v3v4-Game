using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script.Game.CountObject.Effect {
    public class CorrectEffect : MonoBehaviour {

        Renderer[] renderers;
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        void Start() {
            renderers = GetComponentsInChildren<Renderer>();
        }

        public async Task PlayCorrectEffect() {

            Color green = Color.green;
            Sequence sequence = DOTween.Sequence();
            Vector3 originalScale = transform.localScale;

            var tcs = new TaskCompletionSource<bool>();

            foreach ( Renderer renderer in renderers ) {
                Material mat = renderer.material;
                mat.SetColor(EmissionColor, Color.black);

                sequence.Join(
                    mat.DOColor(green * 3f, EmissionColor, 0.15f)
                );
            }

            // Stay glowing.
            sequence.AppendInterval(0.2f);

            // Scale up.
            sequence.Append(
                transform.DOScale(originalScale * 1.15f, 0.15f).SetEase(Ease.OutBack)
            );

            // Scale back down.
            sequence.Append(
                transform.DOScale(originalScale, 0.3f).SetEase(Ease.InOutSine)
            );

            // Remove glow from all renderers.
            foreach ( Renderer renderer in renderers ) {
                Material mat = renderer.material;

                sequence.Join(
                    mat.DOColor(Color.black,EmissionColor,0.3f)
                );
            }

            sequence.OnComplete(() =>
            {
                tcs.SetResult(true);
            });

            await tcs.Task;
        }
    }
}
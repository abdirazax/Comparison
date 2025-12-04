using System;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Animations;
using UnityEngine;

namespace TCG_Card_System.Scripts.Effects
{
    public class ScaleEffect : MonoBehaviourExtended<GameObject>
    {
        public Vector3 startScale = Vector3.zero;
        public Vector3 endScale = Vector3.one;

        public void SetScale(Vector3 scale) => transform.localScale = scale;

        public UniTask ScaleUp(float duration = 0.5f) =>
            AnimationStart(gameObject, () => AnimationDefinition(true, duration));

        public UniTask ScaleDown(float duration = 0.5f) =>
            AnimationStart(gameObject, () => AnimationDefinition(false, duration));
        
        public async UniTask ScaleUpAndDown(float duration, float middleBreakSeconds)
        {
            await ScaleUp(duration);
            await UniTask.Delay(TimeSpan.FromSeconds(middleBreakSeconds));
            await ScaleDown(duration);
        }

        private async UniTask AnimationDefinition(bool scaleUp, float duration)
        {
            var currentTime = 0f;
            while (currentTime < duration)
            {
                var t = currentTime / duration;
                transform.localScale = Vector3.Lerp(scaleUp ? startScale : endScale, scaleUp ? endScale : startScale, t);
                currentTime += Time.deltaTime;
                await UniTask.Yield();
            }

            // Ensure the final values are set
            transform.localScale = scaleUp ? endScale : startScale;
        }
    }
}
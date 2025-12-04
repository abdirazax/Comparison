using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace TCG_Card_System.Scripts.Animations
{
    public class CardAttackRangedAnimation : MonoBehaviourExtended<string>
    {
        [SerializeField]
        private float duration = 0.3f;
        
        [SerializeField]
        private AnimationCurve curve;

        private readonly Dictionary<string, GameObject> _gameObjects = new();
        
        public async UniTask Animate
        (
            string objectId,
            GameObject prefab,
            Vector3 startPosition,
            Vector3 endPosition
        )
        {
            var cancellationTokenSource = Animations.GetValueOrDefault(objectId, null);
            if (cancellationTokenSource != null)
            {
                if (_gameObjects.ContainsKey(objectId))
                {
                    Destroy(_gameObjects[objectId]);
                    _gameObjects.Remove(objectId);
                }
            }

            _gameObjects[objectId] = Instantiate(prefab, startPosition, Quaternion.identity);
            await AnimationStart
            (
                objectId,
                () => AnimationDefinition
                (
                    objectId,
                    startPosition,
                    endPosition
                )
            );
        }
        
        private async UniTask AnimationDefinition
        (
            string objectId,
            Vector3 startPosition,
            Vector3 endPosition
        )
        {
            var currentGameObject = _gameObjects[objectId];
            var currentTransform = currentGameObject.transform;
            
            // Calculate the direction from the start point to the end point
            var direction = (endPosition - startPosition).normalized;

            // Calculate the length of the original line
            var length = Vector3.Distance(startPosition, endPosition);

            // Calculate the new end point by moving from the start point towards the original end point
            // by the distance of the new length
            var newEndPosition = startPosition + direction * length;
            
            // Rotate projectile
            currentTransform.rotation = Quaternion.LookRotation(endPosition - startPosition);
            
            // Animate to the target
            for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                currentTransform.position = Vector3.LerpUnclamped(startPosition, newEndPosition, curve.Evaluate(t));
                await UniTask.Yield();
            }
            
            currentTransform.position = endPosition;
            
            var accessor = currentTransform.GetComponent<CardAttackRangedAnimationAccessor>();
            accessor.projectileParticleSystem.Stop();
            accessor.projectileParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            accessor.hitParticleSystem.Play();
            
            if (accessor.hitParticleSystem)
                Destroy(currentGameObject, accessor.hitParticleSystem.main.duration);
            else
                Destroy(currentGameObject, 1);
            
            _gameObjects.Remove(objectId);
        }
    }
}
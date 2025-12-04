using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TCG_Card_System.Scripts.Animations
{
    public class CardAttackMeleeAnimation : MonoBehaviourExtended<string>
    {
        [SerializeField]
        private float duration = 0.3f;
        
        [SerializeField]
        private float distanceShortenedBy = 3f;
        
        [SerializeField]
        private AnimationCurve curve;

        private readonly Dictionary<string, GameObject> _playerGameObjects = new();
        private readonly Dictionary<string, GameObject> _opponentGameObjects = new();

        public async UniTask Animate
        (
            string objectId,
            GameObject playerGameObject,
            GameObject opponentGameObject,
            GameObject playerAttackPrefab,
            GameObject opponentAttackPrefab,
            Vector3 startPosition,
            Vector3 endPosition,
            Func<UniTask> middle = null
        )
        {
            var cancellationTokenSource = Animations.GetValueOrDefault(objectId, null);
            if (cancellationTokenSource != null)
            {
                if (_playerGameObjects.ContainsKey(objectId))
                {
                    Destroy(_playerGameObjects[objectId]);
                    _playerGameObjects.Remove(objectId);
                }
                
                if (_opponentGameObjects.ContainsKey(objectId))
                {
                    Destroy(_opponentGameObjects[objectId]);
                    _opponentGameObjects.Remove(objectId);
                }
            }

            await AnimationStart
            (
                objectId,
                () => AnimationDefinition
                (
                    objectId,
                    playerGameObject,
                    opponentGameObject,
                    playerAttackPrefab,
                    opponentAttackPrefab,
                    startPosition,
                    endPosition,
                    middle
                )
            );
        }

        private async UniTask AnimationDefinition
        (
            string objectId,
            GameObject playerGameObject,
            GameObject opponentGameObject,
            GameObject playerAttackPrefab,
            GameObject opponentAttackPrefab,
            Vector3 startPosition,
            Vector3 endPosition,
            Func<UniTask> middle = null
        )
        {
            // Calculate the direction from the start point to the end point
            var direction = (endPosition - startPosition).normalized;

            // Calculate the length of the original line
            var length = Vector3.Distance(startPosition, endPosition);

            // Ensure we're not trying to shorten the line more than its original length
            if (distanceShortenedBy > length)
            {
                // Handle this situation: perhaps set shortenAmount to length, or alert the user
                distanceShortenedBy = length; // This would effectively make the line disappear
            }

            // Calculate the new length of the line
            var newLength = length - distanceShortenedBy;

            // Calculate the new end point by moving from the start point towards the original end point
            // by the distance of the new length
            var newEndPosition = startPosition + direction * newLength;

            // Animate to the target
            for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                playerGameObject.transform.position =
                    Vector3.LerpUnclamped(startPosition, newEndPosition, curve.Evaluate(t));
                await UniTask.Yield();
            }

            // Player
            _playerGameObjects[objectId] = Instantiate
            (
                playerAttackPrefab,
                playerGameObject.transform.position,
                Quaternion.LookRotation(endPosition - playerGameObject.transform.position),
                opponentGameObject.transform
            );

            var playerAccessor = _playerGameObjects[objectId].GetComponent<CardAttackMeleeAnimationAccessor>();
            playerAccessor.hitParticleSystem.Play();

            Destroy(_playerGameObjects[objectId], playerAccessor.hitParticleSystem.main.duration);
            _playerGameObjects.Remove(objectId);

            if (opponentAttackPrefab)
            {
                // Opponent
                _opponentGameObjects[objectId] = Instantiate
                (
                    opponentAttackPrefab,
                    endPosition,
                    Quaternion.LookRotation(playerGameObject.transform.position - endPosition),
                    playerGameObject.transform
                );

                var opponentAnimationDuration = 0f;

                var opponentMeleeAccessor =
                    _opponentGameObjects[objectId].GetComponent<CardAttackMeleeAnimationAccessor>();
                if (opponentMeleeAccessor)
                {
                    opponentAnimationDuration = opponentMeleeAccessor.hitParticleSystem.main.duration;
                    opponentMeleeAccessor.hitParticleSystem.Play();
                }

                Destroy(_opponentGameObjects[objectId], opponentAnimationDuration);
                _opponentGameObjects.Remove(objectId);
            }

            if (middle != null)
                await middle();

            await UniTask.Delay(TimeSpan.FromSeconds(playerAccessor.hitParticleSystem.main.duration / 4));

            // Animate back to the start
            for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                playerGameObject.transform.position =
                    Vector3.LerpUnclamped(newEndPosition, startPosition, curve.Evaluate(t));
                await UniTask.Yield();
            }

            playerGameObject.transform.position = startPosition;
        }
    }
}
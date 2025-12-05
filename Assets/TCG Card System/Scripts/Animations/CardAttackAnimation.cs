using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TCG_Card_System.Scripts.Animations
{
    public class CardAttackAnimation : MonoBehaviourExtended<string>
    {
        // [SerializeField]
        // private float duration = 0.05f;
        
        [SerializeField]
        private float distanceShortenedBy = 0f;
        
        [SerializeField]
        private AnimationCurve curve;

        private readonly Dictionary<string, GameObject> _playerGameObjects = new();
        private readonly Dictionary<string, GameObject> _opponentGameObjects = new();

        public async UniTask Animate
        (string objectId,
            Card card,
            CardAttackDisplay cardAttackDisplay,
            GameObject attackingCardGameObject,
            TeamDisplay enemyTeamDisplay,
            Vector3 startPosition,
            Vector3 endPosition,
            float cardMoveDuration,
            float cardAttackDuration)
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
            Debug.Log("Duration: " + cardMoveDuration);

            await AnimationStart
            (
                objectId,
                () => AnimationDefinition(
                    card,
                    cardAttackDisplay,
                    attackingCardGameObject,
                    enemyTeamDisplay,
                    startPosition,
                    endPosition,
                    cardMoveDuration,
                    cardAttackDuration
                    )
            );
        }

        private async UniTask AnimationDefinition(
            Card card,
            CardAttackDisplay cardAttackDisplay, 
            GameObject attackingCardGameObject,
            TeamDisplay enemyTeamDisplay,
            Vector3 startPosition,
            Vector3 endPosition,
            float cardMoveDuration,
            float cardAttackDuration)
        {
            // Animate to the target
            for (var t = 0f; t < 1f; t += Time.deltaTime / cardMoveDuration)
            {
                attackingCardGameObject.transform.position =
                    Vector3.LerpUnclamped(startPosition, endPosition, curve.Evaluate(t));
                await UniTask.Yield();
            }

            HandleOnCardAnimation(
                card,
                cardAttackDisplay,
                cardAttackDuration);
            
            HandleOnEnemyAnimation(
                card,
                cardAttackDisplay, 
                enemyTeamDisplay,
                cardAttackDuration);


            // Animate back to the start
            for (var t = 0f; t < 1f; t += Time.deltaTime / cardMoveDuration)
            {
                attackingCardGameObject.transform.position =
                    Vector3.LerpUnclamped(endPosition, startPosition, curve.Evaluate(t));
                await UniTask.Yield();
            }

            attackingCardGameObject.transform.position = startPosition;
        }

        private async UniTask HandleOnEnemyAnimation(
            Card card,
            CardAttackDisplay cardAttackDisplay, 
            TeamDisplay enemyTeamDisplay,
            float cardAttackDuration
            )
        {
            var attackTemplate = card.Template.attackTemplate;
            var attackIndexInCombo = card.AttackIndexInCombo;
            // X is the duration factor for the attack start
            var attackStartDuration = cardAttackDuration * 
                                      attackTemplate.onEnemyEffectDurationFactor[attackIndexInCombo].x;
            // Y is the duration factor for the attack
            var attackDuration = cardAttackDuration * 
                                 attackTemplate.onEnemyEffectDurationFactor[attackIndexInCombo].y;
            // Z is the duration factor for the attack end
            var attackEndDuration = cardAttackDuration * 
                                    attackTemplate.onEnemyEffectDurationFactor[attackIndexInCombo].z;
            // W is the duration factor for the delay
            var delay = cardAttackDuration * 
                        attackTemplate.onEnemyEffectDurationFactor[attackIndexInCombo].w;
            Debug.Log("cardAttackDuration:" + cardAttackDuration + "Delay: " + delay);
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            Debug.Log("Attack Start Duration: " + attackStartDuration);
            Vector2 randomSeedPerAttack = 
                new Vector2(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f));
            for (var t = 0f; t < 1f; t += Time.deltaTime / attackStartDuration)
            {
                cardAttackDisplay.AnimateAttackOnEnemy(
                    t, 
                    attackIndexInCombo, 
                    t,
                    enemyTeamDisplay,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnEnemy(
                1, 
                attackIndexInCombo, 
                1,
                enemyTeamDisplay,
                randomSeedPerAttack);
            for (var t = 1f; t < 2f; t += Time.deltaTime / attackDuration)
            {
                cardAttackDisplay.AnimateAttackOnEnemy(
                    t, 
                    attackIndexInCombo, 
                    1,
                    enemyTeamDisplay,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnEnemy(
                2, 
                attackIndexInCombo, 
                1,
                enemyTeamDisplay,
                randomSeedPerAttack);
            for (var t = 2f; t < 3f; t += Time.deltaTime / attackEndDuration)
            {
                cardAttackDisplay.AnimateAttackOnEnemy(
                    t, 
                    attackIndexInCombo, 
                    3 - t,
                    enemyTeamDisplay,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnEnemy(
                3, 
                attackIndexInCombo, 
                0,
                enemyTeamDisplay,
                randomSeedPerAttack);
        }
        
        private async UniTask HandleOnCardAnimation(
            Card card,
            CardAttackDisplay cardAttackDisplay,
            float cardAttackDuration)
        {
            
            var attackTemplate = card.Template.attackTemplate;
            var attackIndexInCombo = card.AttackIndexInCombo;
            // X is the duration factor for the attack start
            var attackStartDuration = cardAttackDuration * 
                                      attackTemplate.onCardEffectDurationFactor[attackIndexInCombo].x;
            // Y is the duration factor for the attack
            var attackDuration = cardAttackDuration * 
                                 attackTemplate.onCardEffectDurationFactor[attackIndexInCombo].y;
            // Z is the duration factor for the attack end
            var attackEndDuration = cardAttackDuration * 
                                    attackTemplate.onCardEffectDurationFactor[attackIndexInCombo].z;
            // W is the duration factor for the delay
            var delay = cardAttackDuration * 
                        attackTemplate.onCardEffectDurationFactor[attackIndexInCombo].w;
            Vector2 randomSeedPerAttack = 
                new Vector2(UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f));
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            for (var t = 0f; t < 1f; t += Time.deltaTime / attackStartDuration)
            {
                cardAttackDisplay.AnimateAttackOnCard(
                    t, 
                    attackIndexInCombo, 
                    t,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnCard(
                1, 
                attackIndexInCombo, 
                1,
                randomSeedPerAttack);
            for (var t = 1f; t < 2f; t += Time.deltaTime / attackDuration)
            {
                cardAttackDisplay.AnimateAttackOnCard(
                    t, 
                    attackIndexInCombo, 
                    1,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnCard(
                2, 
                attackIndexInCombo, 
                1,
                randomSeedPerAttack);
            for (var t = 2f; t < 3f; t += Time.deltaTime / attackEndDuration)
            {
                cardAttackDisplay.AnimateAttackOnCard(
                    t, 
                    attackIndexInCombo, 
                    3 - t,
                    randomSeedPerAttack);
                await UniTask.Yield();
            }
            cardAttackDisplay.AnimateAttackOnCard(
                3, 
                attackIndexInCombo, 
                0,
                randomSeedPerAttack);
        }
    }
}
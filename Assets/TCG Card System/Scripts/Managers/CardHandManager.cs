using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardHandManager : CardBaseManager
    {
        [SerializeField]
        private GameObject cardPrefab;
        [SerializeField]
        protected Vector3 cardScale = new(5f, 5f, 1f);
        [SerializeField]
        protected float cardSpreadWidth = 6;
        [SerializeField]
        protected float cardSpreadWidthIntensity = 0.8f;
        [SerializeField]
        protected float cardSpreadAngle = 15f;
        [SerializeField]
        protected float cardSpreadAngleIntensity = 0.8f;
        [SerializeField]
        protected float cardZAxisPeak = -0.5f;
        [SerializeField]
        protected float cardZAxisIntensity = 0.05f;
        [SerializeField]
        protected bool mirror;
        
        protected int VirtualIndex;

        public async UniTask DrawInitialCards(IEnumerable<Card> initialCards)
        {
            var cards = initialCards.ToList();
            var cardTransforms = GetCardTransforms(cards.Count);

            SetUnbreakableAnimation(cards, true);

            for (var index = 0; index < cards.Count; index++)
            {
                var card = cards[index];
                var cardTransform = cardTransforms[index];
                
                AddCardToHand(card);
                
                await RepositionCard
                (
                    card,
                    cardTransform.Position,
                    cardTransform.Rotation,
                    index,
                    true,
                    2f
                );
            }
            
            SetUnbreakableAnimation(cards, false);
        }

        public async UniTask DrawCard(Card card, int? index = null)
        {
            SetUnbreakableAnimation(card, true);
            
            AddCardToHand(card, index);
            await RepositionCards(card.GameObject);
            
            SetUnbreakableAnimation(card, false);
        }
        
        private void AddCardToHand(Card card, int? index = null)
        {
            card.GameObject.transform.SetParent(transform);

            if (index.HasValue)
                Cards.Insert(index.Value, card);
            else
                Cards.Add(card);
        }

        private async UniTask RepositionCards
        (
            GameObject ignoreUnbreakableAnimationForGameObject = null,
            float animationSpeed = 3f
        )
        {
            var tasks = new List<UniTask>();
            var cardTransforms = GetCardTransforms(Cards.Count);
            for (var index = 0; index < Cards.Count; index++)
            {
                var cardTransform = cardTransforms[index];
                var card = Cards[index];

                tasks.Add(RepositionCard
                (
                    card,
                    cardTransform.Position,
                    cardTransform.Rotation,
                    index,
                    card.GameObject == ignoreUnbreakableAnimationForGameObject,
                    animationSpeed
                ));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask RepositionCard
        (
            Card card, 
            Vector3 position, 
            Quaternion rotation,
            int index,
            bool ignoreUnbreakableAnimation = false,
            float animationSpeed = 3f
        )
        {
            card.CachePosition = position;
            card.CacheRotation = rotation;
            
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.CardSortingGroup.sortingOrder = index;
            cardAccessor.FrameSortingGroup.sortingOrder = 0;

            CardAnimateBetweenFrame(card, false).Forget();
            
            await CardAnimationStart
            (
                card,
                position,
                rotation,
                cardScale,
                ignoreUnbreakableAnimation,
                animationSpeed
            );
        }
        
        protected IList<(Vector3 Position, Quaternion Rotation)> GetCardTransforms(int count, int focusedIndex = -1)
        {
            var dynamicProperties = CalculateDynamicProperties(count);
            var output = new List<(Vector3 Position, Quaternion Rotation)>();
    
            for (var i = 0; i < count; i++)
            {
                var zOffset = CalculateZOffset(count, i, dynamicProperties.dynamicZAxisPeak);
                var positionOffset = CalculatePositionOffset(count, i, dynamicProperties, zOffset, focusedIndex);
                var rotation = CalculateRotation(i, dynamicProperties.startAngle, dynamicProperties.angleStep, focusedIndex);
        
                if (mirror)
                {
                    var euler = rotation.eulerAngles;
                    euler.x += 180;
                    rotation = Quaternion.Euler(euler);
                }
        
                var position = transform.position + positionOffset;
                output.Add((position, rotation));
            }
    
            return output;
        }

        private (float dynamicSpreadWidth, float dynamicSpreadAngle, float dynamicZAxisPeak, float angleStep, float startAngle) CalculateDynamicProperties(int count)
        {
            var dynamicSpreadWidth = cardSpreadWidth * Mathf.Max(1, cardSpreadWidthIntensity * Mathf.Log(count));
            var dynamicSpreadAngle = cardSpreadAngle * Mathf.Max(1, cardSpreadAngleIntensity * Mathf.Log(count));
            var dynamicZAxisPeak = cardZAxisPeak * Mathf.Max(1, cardZAxisIntensity * count);
            var angleStep = count > 1 ? dynamicSpreadAngle / (count - 1) : 0;
            var startAngle = dynamicSpreadAngle / 2;
            
            return (dynamicSpreadWidth, dynamicSpreadAngle, dynamicZAxisPeak, angleStep, startAngle);
        }

        private float CalculateZOffset(int count, int index, float dynamicZAxisPeak)
        {
            var zOffsetIncrement = dynamicZAxisPeak * 2 / Mathf.Max(count - 1, 1);
            var zOffset = -Mathf.Abs(dynamicZAxisPeak - index * zOffsetIncrement);
            if (mirror)
            {
                zOffset *= -1; // Reverse the direction for Z offset when outwards is true
            }
            return zOffset;
        }

        private Quaternion CalculateRotation(int index, float startAngle, float angleStep, int focusedIndex)
        {
            var angle = startAngle - angleStep * index;
            if (mirror)
            {
                angle *= -1; // Invert angle when outwards is true to mirror the fan
            }
            Quaternion rotation;
            if (focusedIndex != -1 && index == focusedIndex)
            {
                rotation = Quaternion.Euler(90f, 0f, 0);
            }
            else
            {
                rotation = Quaternion.Euler(90f, 0f, angle);
            }
            return rotation;
        }

        private Vector3 CalculatePositionOffset
        (
            int count,
            int index,
            (float dynamicSpreadWidth, float dynamicSpreadAngle, float dynamicZAxisPeak, float angleStep, float startAngle) props, 
            float zOffset,
            int focusedIndex
        )
        {
            var additionalOffsetForAdjacentCards = props.dynamicSpreadWidth * 0.25f;
            var normalizedPosition = (float)index / Mathf.Max(count - 1, 1);
            var centeredPosition = normalizedPosition - 0.5f;
            var positionOffset = new Vector3(centeredPosition * props.dynamicSpreadWidth, 0f, zOffset);

            if (focusedIndex == -1)
                return positionOffset;
            
            if (index == focusedIndex)
            {
                zOffset = 0; // Adjusted within this method for simplicity
                positionOffset.z = zOffset;
            }
            else
            {
                float direction = index < focusedIndex ? -1 : 1;
                if (index == focusedIndex - 1 || index == focusedIndex + 1)
                {
                    positionOffset.x += additionalOffsetForAdjacentCards * direction;
                }
                else
                {
                    var adjustedPosition = centeredPosition + direction * additionalOffsetForAdjacentCards / props.dynamicSpreadWidth;
                    positionOffset.x = adjustedPosition * props.dynamicSpreadWidth;
                }
            }

            return positionOffset;
        }
    }
}
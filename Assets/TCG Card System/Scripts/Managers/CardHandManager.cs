using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.States;
using UnityEngine;
using UnityEngine.Serialization;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardHandManager : CardBaseManager
    {
        [SerializeField]
        private GameObject cardPrefab;
        [SerializeField]
        protected Vector3 cardScale = new(5f, 5f, 1f);
        protected Vector3Int CurrentGridOffset = Vector3Int.zero;
        [SerializeField]
        protected Vector3Int gridOffsetWhenPreparing = Vector3Int.zero;
        [SerializeField]
        protected Vector3Int gridOffsetAtBattle = Vector3Int.zero;

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
        
        [SerializeField]
        protected BattleStateManager battleStateManager;
        
        protected int VirtualIndex;

        private void OnEnable()
        {
            battleStateManager.OnBattleStateChanged += OnBattleStateChanged;
        }

        private void OnDisable()
        {
            battleStateManager.OnBattleStateChanged -= OnBattleStateChanged;
        }

        public void ResetHand()
        {
            foreach (var card in Cards)
            {
                Destroy(card.GameObject);
            }
            Cards.Clear();
            VirtualIndex = 0;
        }
        
        private void OnBattleStateChanged(object sender, BattleStateEventArgs e)
        {
            if (e.BattleState is BattleGoingOnState)
            {
                CurrentGridOffset = gridOffsetAtBattle;
            }
            else if (e.BattleState is BattlePreparingState)
            {
                CurrentGridOffset = gridOffsetWhenPreparing;
            }
            RepositionCards();
        }

        public async UniTask DrawInitialCards(IEnumerable<Card> initialCards)
        {
            var cards = initialCards.ToList();
            var cardTransforms = GetCardTransforms(Cards);

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
            
            if (!HasEnoughSlotsFor(card?.Template.slotSize ?? 1))
            {
                Debug.LogWarning($"Not enough slots to draw card {card?.Data.Id}. Current free slots: {GetFreeCardSlotsCount()}, Required slots: {card?.Template.slotSize}");
                return;
            }
            
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

        protected async UniTask RepositionCards
        (
            GameObject ignoreUnbreakableAnimationForGameObject = null,
            float animationSpeed = 3f
        )
        {
            var tasks = new List<UniTask>();
            var cardTransforms = GetCardTransforms(Cards);
            for (var index = 0; index < Cards.Count; index++)
            {
                var cardTransform = cardTransforms[index];
                var card = Cards[index];
                CancelAllCardAnimations();
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
        protected IList<(Vector3 Position, Quaternion Rotation)> GetCardTransforms(List<Card> cards, int focusedIndex = -1)
        {
            var output = new List<(Vector3 Position, Quaternion Rotation)>();
            for (int i = 0; i < cards.Count; i++)
            {
                Vector3Int offset = CurrentGridOffset;
                if ((focusedIndex > -1) && (i != focusedIndex))
                {
                    offset += new Vector3Int( 1 * (i < focusedIndex ? -1 : 1), 0, 0);
                }
                Vector3 position = gridManager.GetXCenteredPosition(GetCardSlotIndex(i), GetCardSlotsCount(),
                    cards[i].Template.slotSize, offset);
                output.Add(
                    (position, Quaternion.Euler(90f, 0f, 0)));
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

        
    }
}
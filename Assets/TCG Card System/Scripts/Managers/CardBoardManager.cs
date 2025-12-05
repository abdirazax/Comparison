using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TCG_Card_System.Scripts.Animations;
using TCG_Card_System.Scripts.Enums;
using TCG_Card_System.Scripts.EventArgs;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardBoardManager : CardBaseManager
    {
        public event EventHandler<CardPlayEventArgs> CardPlayed;
        public event EventHandler<Card> CardDestroyed;
        
        public event EventHandler<CardAttackEventArgs> OnCardAttackedBoardStarted; 
        public event EventHandler<CardAttackEventArgs> OnCardAttackedBoardEnded;

        public virtual Team Team { get; } = new Team();
        public TeamDisplay teamDisplay;
        
        protected virtual string LayerName => null;

        [SerializeField]
        protected GameObject cardPrefab;
        
        [SerializeField]
        protected Quaternion cardRotation = Quaternion.Euler(90, 0, 0);
        
        [SerializeField]
        protected Vector3 cardScale = new(10f, 10f, 1f);
        [SerializeField]
        protected Vector3Int gridOffset = Vector3Int.zero;

        [SerializeField]
        private float cardSpreadWidth = 5;
        
        private CardAttackAnimation _attackAnimation;
        private CardAttackRangedAnimation _attackRangedAnimation;

        private IList<Vector3> _cardPositions;

        protected virtual void Awake()
        {
            _attackAnimation = GetComponent<CardAttackAnimation>();
            _attackRangedAnimation = GetComponent<CardAttackRangedAnimation>();
        }

        public void AddCard(Card card, int? index = null)
        {
            index ??= FindVirtualIndex(card);
            
            Cards.Insert(index.Value, card);
            card.GameObject.transform.SetParent(transform);
            
            RepositionCards();
        }
        
        public void ResetBoard()
        {
            foreach (var card in Cards)
            {
                Destroy(card.GameObject);
            }
            Cards.Clear();
            RepositionCards();
        }


        public void PlayCard(Card card, int? index = null)
        {
            index ??= FindVirtualIndex(card);

            Cards.Insert(index.Value, card);
            card.GameObject.transform.SetParent(transform);
            
            CardPlayed?.Invoke(this, new CardPlayEventArgs
            {
                CardData = card.Data,
                BoardIndex = index.Value
            });
            
            RepositionCards();
        }
        
        public Card CardSpawn(CardData cardData, Vector3? position = null, int sortingOrder = 0)
        {
            position ??= transform.position;
            
            var layer = LayerMask.NameToLayer(LayerName);
            var card = new Card
            {
                GameObject = Instantiate
                (
                    cardPrefab, 
                    position.Value,
                    cardRotation,
                    transform
                )
            };
            card.GameObject.layer = layer;
            card.CachePosition = position.Value;
            card.CacheRotation = cardRotation;
            card.SetData(cardData);
            
            foreach (Transform cardChilds in card.GameObject.transform)
                cardChilds.localScale = cardScale;
                
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.cardLayout.layer = layer;
            cardAccessor.attackLayout.layer = layer;
                
            cardAccessor.Card = card;
            cardAccessor.CardSortingGroup.sortingOrder = sortingOrder;
            
            cardAccessor.CardDisplay.UpdateUI(card);

            return card;
        }

        public async UniTask CardAttackOpponentBoard
        (
            Card card,
            CardBoardManager defenderBoard,
            Func<UniTask> middle = null
        )
        {
            
            OnCardAttackedBoardStarted?.Invoke
            (
                this,
                new CardAttackEventArgs
                {
                    CardId = card.Data.Id,
                    TargetBoard = defenderBoard
                }
            );
            defenderBoard.Team.TakeDamage(card.Data.Attack[card.AttackIndexInCombo]);
            

            var cardIndex = GetCardIndex(card.Data.Id);
            
            await _attackAnimation.Animate
            (
                card.Data.Id,
                card,
                card.GameObject.GetComponent<CardAccessor>().CardAttackDisplay,
                card.GameObject,
                defenderBoard.teamDisplay,
                _cardPositions[cardIndex],
                _cardPositions[cardIndex] + new Vector3(0, 0, _cardPositions[cardIndex].z) * .2f,
                0.1f,
                card.Data.AutoAttackInterval / card.Data.AttacksPerInterval
            );
            
        }

        public async UniTask CardAttackOpponentCardBoardShowDamage
        (
            Card card,
            CardBoardManager targetCardBoardManager
        )
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            var cardAttackDisplay = cardAccessor.CardAttackDisplay;
            
            // var opponentCardAccessor = targetCard.GameObject.GetComponent<CardAccessor>();
            // var opponentCardFrameDisplay = opponentCardAccessor.FrameDisplay;
            //
            // var tasks = new List<UniTask> 
            // {
            //     // Damage to Target
            //     opponentCardFrameDisplay.ShowDamageTaken(card.Data.Attack) 
            // };
            //
            // await UniTask.WhenAll(tasks);
        }

        public async UniTask RaiseCard(Card card)
        {
            await CardAnimationStart
            (
                card,
                new Vector3
                (
                    card.GameObject.transform.position.x,
                    card.GameObject.transform.position.y + 5f,
                    card.GameObject.transform.position.z
                ),
                cardRotation,
                cardScale
            );
        }
        
        public async UniTask LowerCard(Card card)
        {
            await CardAnimationStart
            (
                card,
                new Vector3
                (
                    card.CachePosition.x,
                    card.CachePosition.y,
                    card.CachePosition.z
                ),
                cardRotation,
                cardScale
            );
        }

        public void RepositionCards(Card virtualCard = null, int? virtualIndex = null)
        {
            _cardPositions = GetCardPositions(virtualCard, virtualIndex);
            for (var i = 0; i < Cards.Count; i++)
            {
                var cardPosition = _cardPositions[i];
                var card = Cards[i];
                card.CachePosition = cardPosition;
                card.CacheRotation = Quaternion.Euler(90, 0, 0);
                
                var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
                cardAccessor.CardSortingGroup.sortingOrder = 0;
                cardAccessor.FrameSortingGroup.sortingOrder = 0;

                CardAnimateBetweenFrame(card, true).Forget();
                CardAnimationStart(card, cardPosition, cardRotation, cardScale).Forget();
            }
        }

        public int FindVirtualIndex(Card card)
        {
            if (Cards.Count == 0)
                return 0;
            
            var cardPosition = card.GameObject.transform.position;
            
            // Check if the dragged object is to the left of all cards
            if (cardPosition.x < Cards[0].GameObject.transform.position.x)
            {
                // Debug.Log("Dragging on the leftmost side");
                return 0;
            }

            // Check if the dragged object is to the right of all cards
            if (cardPosition.x > Cards[^1].GameObject.transform.position.x)
            {
                // Debug.Log("Dragging on the rightmost side");
                return Cards.Count;
            }

            for (var i = 0; i < Cards.Count - 1; i++)
            {
                var cardLeft = Cards[i];
                var cardRight = Cards[i + 1];
                var leftTransform = cardLeft.GameObject.transform;
                var rightTransform = cardRight.GameObject.transform;

                if (cardPosition.x > leftTransform.position.x && cardPosition.x < rightTransform.position.x)
                {
                    var leftIndex = Cards.FindIndex(x => x.Data.Id == cardLeft.Data.Id);
                    var rightIndex = Cards.FindIndex(x => x.Data.Id == cardRight.Data.Id);
                    
                    // Debug.Log($"Dragging over: {leftIndex} and {rightIndex}");
                    return rightIndex; 
                }
            }

            return Cards.Count - 1;
        }
        
        public void CardUpdate(CardData cardData)
        {
            var card = GetCardById(cardData.Id);
            if (card == null) 
                return;
            
            card.SetData(cardData);
            
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
        }
        
        public async UniTask CardDestroy(string cardId)
        {
            var card = GetCardById(cardId);
            if (card is not { State: EBattleCardState.Death }) 
                return;
            
            Cards.Remove(card);
            CardDestroyed?.Invoke(this, card);
            
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            
            Destroy(card.GameObject);
            RepositionCards();
        }
        
        private IList<Vector3> GetCardPositions(Card virtualCard = null, int? virtualIndex = null)
        {
            var output = new List<Vector3>();

            for (int i = 0; i < Cards.Count; i++)
            {
                
                Vector3Int offset = gridOffset;
                if (virtualIndex.HasValue && virtualCard != null)
                {
                    offset += new Vector3Int((virtualCard.Template.slotSize + 1) * (i < virtualIndex ? -1 : 1), 0, 0);
                }
                Vector3 position = gridManager.GetXCenteredPosition(GetCardSlotIndex(i), GetCardSlotsCount(),
                    Cards[i].Template.slotSize, offset);
                output.Add(position);
            }   
            return output;
        }
        
        protected override async UniTask CardAnimationDefinition
        (
            Card card,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float animationSpeed = 3f
        )
        {
            //TODO
            // Skip if melee attack animation is running
            // if (_attackMeleeAnimation.IsAnimationRunning(card.Data.Id))
            //     return;
            //
            // // Skip if ranged attack animation is running
            // if (_attackRangedAnimation.IsAnimationRunning(card.Data.Id))
            //     return;
            //
            
            
            float time = 0;
            var startPosition = card.GameObject.transform.position;
            var startRotation = card.GameObject.transform.rotation;
            
            var startScales = card.GameObject.transform
                .Cast<Transform>()
                .Select(x => x.localScale)
                .Take(2)
                .ToList();
            
            while (time < 1)
            {
                card.GameObject.transform.SetPositionAndRotation
                (
                    Vector3.Lerp(startPosition, position, time),
                    Quaternion.Lerp(startRotation, rotation, time)
                );

                for (var i = 0; i < 2; i++)
                {
                    card.GameObject.transform.GetChild(i).localScale = Vector3.Lerp(startScales[i], scale, time);
                }

                time += Time.deltaTime * animationSpeed; // Adjust time increment for speed
                await UniTask.Yield();
            }

            card.GameObject.transform.SetPositionAndRotation
            (
                position,
                rotation
            );
            
            for (var i = 0; i < 2; i++)
                card.GameObject.transform.GetChild(i).localScale = scale;
            
            
        }
    }
}
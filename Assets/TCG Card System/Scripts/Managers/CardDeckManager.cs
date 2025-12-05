using System;
using System.Linq;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardDeckManager : CardBaseManager
    {
        protected virtual string LayerName => null;
        
        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField] 
        protected int numberOfCards = 20;
        
        [SerializeField]
        protected Vector3 cardScale = new(8f, 8f, 1f);
        
        [SerializeField]
        private float cardOffset = 0.05f;
        
        protected virtual void Start()
        {
            SpawnDeck(numberOfCards);
        }

        public void SpawnDeck(int count)
        {
            for (var i = 0; i < count; i++)
                Cards.Add(CardSpawn(i));
        }

        public Card CardSpawn(int index = 0)
        {
            var layer = LayerMask.NameToLayer(LayerName);
            var cardPosition = transform.position + new Vector3(0, 0 + cardOffset * index, 0);
            var cardRotation = Quaternion.Euler(-90, 90, 90);
                
            var card = new Card
            {
                GameObject = Instantiate
                (
                    cardPrefab, 
                    cardPosition,
                    cardRotation,
                    transform
                )
            };
            card.GameObject.layer = layer;
            card.CachePosition = cardPosition;
            card.CacheRotation = cardRotation;
            
            foreach (Transform cardChilds in card.GameObject.transform)
                cardChilds.localScale = cardScale;
                
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.cardLayout.layer = layer;
            cardAccessor.frameLayout.layer = layer;
                
            cardAccessor.Card = card;
            cardAccessor.CardSortingGroup.sortingOrder = -(numberOfCards - index);
                
            cardAccessor.CardDissolveEffect.dissolveTime = 0.4f;
            cardAccessor.CardDissolveEffect.SetVisibility(true);
        
            cardAccessor.FrameDissolveEffect.dissolveTime = 0.6f;
            cardAccessor.FrameDissolveEffect.SetVisibility(false);

            return card;
        }

        public Card CardPrepareForDraw(CardData cardData)
        {
            if (Cards.Count == 0)
                return null;

            var card = Cards.Last();
            
            if (cardData != null)
            {
                card.SetData(cardData);

                var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
                cardAccessor.CardDisplay.UpdateUI(card);
                cardAccessor.FrameDisplay.UpdateUI(card);
            }

            Cards.Remove(card);

            return card;
        }
        
        public void DestroyLastCards(int count)
        {
            count = Math.Max(0, Cards.Count - count);

            for (var i = 0; i < count; i++)
            {
                var card = Cards.First();
                Cards.Remove(card);
                Destroy(card.GameObject);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardBaseManager : MonoBehaviour
    {
        [SerializeField]
        protected GridManager gridManager;
        [SerializeField]
        protected int maxCardSlots = -1; // -1 means no limit
        
        public readonly List<Card> Cards = new();
        
        protected readonly Dictionary<GameObject, CancellationTokenSource> _cardAnimations = new();
        
        public bool Exists(string id) =>
            Cards.Any(x => x.Data.Id == id);
        
        public int GetCardIndex(string id) =>
            Cards.FindIndex(x => x.Data.Id == id);
        
        public Card GetCardById(string id) => 
            Cards.Find(x => x.Data.Id == id);
        
        public Card GetCardByIndex(int index) => Cards[index];
        
        
        protected int GetCardSlotIndex(string id) => 
            GetCardSlotIndex(GetCardIndex(id));

        protected int GetCardSlotIndex(int index)
        {
            var slotIndex = 0;
            for (var i = 0; i < index; i++)
            {
                slotIndex+= Cards[i].Template.slotSize;
            }
            return slotIndex;
        }

        
        public void ShowCardHighlight(Card card, float thickness = 8, Color? color = null)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            
        }
        
        public void HideCardHighlight(Card card)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
        }
        
        public void ShowFrameHighlight(Card card, float thickness = 8, Color? color = null)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            
        }
        
        public void HideFrameHighlight(Card card)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
        }
        
        public async UniTask CardAnimateBetweenFrame(Card card, bool isOnTop)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();

            // if (isOnTop)
            // {
            //     await
            //     (
            //         cardAccessor.CardDissolveEffect.Disappear(),
            //         cardAccessor.FrameDissolveEffect.Appear()
            //     );
            // }
            // else
            // {
            //     await
            //     (
            //         cardAccessor.CardDissolveEffect.Appear(),
            //         cardAccessor.FrameDissolveEffect.Disappear()
            //     );
            // }
        }
        
        protected void SetUnbreakableAnimation(IEnumerable<Card> cards, bool value)
        {
            foreach (var card in cards)
                SetUnbreakableAnimation(card, value);
        }
        
        protected void SetUnbreakableAnimation(Card card, bool value) => 
            card.UnbreakableAnimation = value;
        
        protected async UniTask CardAnimationStart
        (
            Card card,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            bool ignoreUnbreakableAnimation = false,
            float animationSpeed = 3f
        )
        {
            if (card.UnbreakableAnimation && !ignoreUnbreakableAnimation)
                return;
            
            // Cancel the existing animation if there is one
            if (_cardAnimations.ContainsKey(card.GameObject))
            {
                CardAnimationStop(card);
            }

            var animationCtSource = new CancellationTokenSource();
            _cardAnimations.Add(card.GameObject, animationCtSource);

            try
            {
                await CardAnimationDefinition(card, position, rotation, scale, animationSpeed)
                    .AttachExternalCancellation(animationCtSource.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                _cardAnimations.Remove(card.GameObject);
            }
        }
        protected async UniTask CardAnimationStart
        (
            Card card,
            Vector3Int gridPosition,
            Quaternion rotation,
            Vector3 scale,
            bool ignoreUnbreakableAnimation = false,
            float animationSpeed = 3f
        )
        {
            await CardAnimationStart(card, gridManager.GetWorldPosition(gridPosition), rotation, card.GameObject.transform.localScale, false, animationSpeed);
        }
        
        protected void CardAnimationStop(Card card)
        {
            if (card.UnbreakableAnimation)
                return;
            
            var cancellationTokenSource = _cardAnimations.GetValueOrDefault(card.GameObject, null);
            if (cancellationTokenSource == null)
                return;
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
                
            _cardAnimations.Remove(card.GameObject);
        }

        protected virtual async UniTask CardAnimationDefinition
        (
            Card card,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float animationSpeed = 3f
        )
        {
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
                    card.GameObject.transform.GetChild(i).localScale = Vector3.Lerp(startScales[i], scale, time);
                
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
        
        protected void CancelAllCardAnimations()
        {
            foreach (var kv in _cardAnimations.ToList())
            {
                kv.Value.Cancel();
                kv.Value.Dispose();
                _cardAnimations.Remove(kv.Key);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var cardAnimation in _cardAnimations)
            {
                cardAnimation.Value.Cancel();
                cardAnimation.Value.Dispose();
                
                _cardAnimations.Remove(cardAnimation.Key);
            }
        }
        
        protected int GetCardSlotsCount()
        {
            return Cards.Sum(card => card.Template.slotSize);
        }
        protected int GetFreeCardSlotsCount()
        {
            if (maxCardSlots < 0)
                return int.MaxValue;
            return maxCardSlots - GetCardSlotsCount();
        }
        public bool HasEnoughSlotsFor(Card card)
        {
            if (GetFreeCardSlotsCount() > card.Template.slotSize)
                return true;
            return false;
        }

        public bool HasEnoughSlotsFor(int slotSize)
        {
            if (GetFreeCardSlotsCount() > slotSize)
                return true;
            return false;
                
        }

    }
}
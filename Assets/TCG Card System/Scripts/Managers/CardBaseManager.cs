using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public abstract class CardBaseManager : MonoBehaviour
    {
        public readonly List<Card> Cards = new();
        
        private readonly Dictionary<GameObject, CancellationTokenSource> _cardAnimations = new();
        
        public bool Exists(string id) =>
            Cards.Any(x => x.Data.Id == id);
        
        public int GetCardIndex(string id) =>
            Cards.FindIndex(x => x.Data.Id == id);
        
        public Card GetCardById(string id) => 
            Cards.Find(x => x.Data.Id == id);
        
        public Card GetCardByIndex(int index) => Cards[index];
        
        public void ShowCardHighlight(Card card, float thickness = 8, Color? color = null)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.CardHighlightEffect
                .Show(thickness, color, 3.5f)
                .Forget();
        }
        
        public void HideCardHighlight(Card card)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.CardHighlightEffect.Hide().Forget();
        }
        
        public void ShowFrameHighlight(Card card, float thickness = 8, Color? color = null)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.FrameHighlightEffect
                .Show(thickness, color, 3.5f)
                .Forget();
        }
        
        public void HideFrameHighlight(Card card)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.FrameHighlightEffect.Hide().Forget();
        }
        
        public async UniTask CardAnimateBetweenFrame(Card card, bool isOnTop)
        {
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();

            if (isOnTop)
            {
                await
                (
                    cardAccessor.CardDissolveEffect.Disappear(),
                    cardAccessor.FrameDissolveEffect.Appear()
                );
            }
            else
            {
                await
                (
                    cardAccessor.CardDissolveEffect.Appear(),
                    cardAccessor.FrameDissolveEffect.Disappear()
                );
            }
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

        protected virtual void OnDestroy()
        {
            foreach (var cardAnimation in _cardAnimations)
            {
                cardAnimation.Value.Cancel();
                cardAnimation.Value.Dispose();
                
                _cardAnimations.Remove(cardAnimation.Key);
            }
        }
    }
}
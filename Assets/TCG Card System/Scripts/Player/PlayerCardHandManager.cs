 using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts.Player
{
    public class PlayerCardHandManager : CardHandManager
    {
        public static PlayerCardHandManager Instance { get; private set; }

        public event EventHandler<int> CardIndexFocused;
        public event EventHandler<int> CardIndexUnfocused;

        [SerializeField]
        private GameObject droppableGameObject;

        private Card _currentlyDraggedCard;

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            CardEvents.Instance.CardFocused += OnCardFocused;
            CardEvents.Instance.CardUnfocused += OnCardUnfocused;
            CardEvents.Instance.CardDraggingStarted += OnCardDraggingStarted;
            CardEvents.Instance.CardDragging += OnCardDragging;
            CardEvents.Instance.CardDraggingEnded += OnCardDraggingEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            CardEvents.Instance.CardFocused -= OnCardFocused;
            CardEvents.Instance.CardUnfocused -= OnCardUnfocused;
            CardEvents.Instance.CardDraggingStarted -= OnCardDraggingStarted;
            CardEvents.Instance.CardDragging -= OnCardDragging;
            CardEvents.Instance.CardDraggingEnded -= OnCardDraggingEnded;
        }

        public void ReturnDraggedCardBackToHand()
        {
            if (_currentlyDraggedCard == null)
                return;
            
            CardAnimationStart
            (
                _currentlyDraggedCard, 
                _currentlyDraggedCard.CachePosition, 
                _currentlyDraggedCard.CacheRotation,
                cardScale,
                animationSpeed: 5
            ).Forget();
            
            CardAnimateBetweenFrame(_currentlyDraggedCard, false).Forget();
            
            _currentlyDraggedCard = null;
        }
        
        private bool IsCardOnTopOfDroppableArea(Vector3 position)
        {
            var droppableAreaPosition = droppableGameObject.transform.position;
            var droppableBounds = droppableGameObject.GetComponent<Collider>().bounds;
            
            return droppableBounds.Contains(new Vector3(position.x, droppableAreaPosition.y, position.z));
        }

        private void OnCardFocused(object sender, Card card) 
        {
            var focusedIndex = Cards.FindIndex(x => x.Data.Id == card.Data.Id);
            CardIndexFocused?.Invoke(this, focusedIndex);
            
            var cardTransforms = GetCardTransforms(Cards, focusedIndex);

            for (var i = 0; i < Cards.Count; i++)
            {
                var cardByIndex = Cards[i];
                var cardTransform = cardTransforms[i];

                if (i == focusedIndex)
                {
                    var cardAccessor = cardByIndex.GameObject.GetComponent<CardAccessor>();
                    cardAccessor.CardSortingGroup.sortingOrder = 100;
                    cardAccessor.FrameSortingGroup.sortingOrder = 100;
                    CardAnimationStart
                    (
                        cardByIndex,
                        new Vector3(
                            cardTransform.Position.x,
                            cardByIndex.GameObject.transform.parent.transform.position.y,
                            cardByIndex.GameObject.transform.parent.transform.position.z 
                        ),
                        cardTransform.Rotation,
                        cardScale * 1.3f,
                        animationSpeed: 8
                    ).Forget();
                }
                else
                {
                    var cardAccessor = cardByIndex.GameObject.GetComponent<CardAccessor>();
                    cardAccessor.CardSortingGroup.sortingOrder = i;
                    
                    CardAnimationStart
                    (
                        cardByIndex,
                        cardTransform.Position,
                        cardTransform.Rotation, 
                        cardScale,
                        animationSpeed: 8
                    ).Forget();
                }
            }
        }

        private void OnCardUnfocused(object sender, Card card)
        {
            var focusedIndex = Cards.FindIndex(x => x.Data.Id == card.Data.Id);
            CardIndexUnfocused?.Invoke(this, focusedIndex);
            
            var cardTransforms = GetCardTransforms(Cards);

            for (var i = 0; i < Cards.Count; i++)
            {
                var cardByIndex = Cards[i];
                var cardTransform = cardTransforms[i];

                var cardAccessor = cardByIndex.GameObject.GetComponent<CardAccessor>();
                cardAccessor.CardSortingGroup.sortingOrder = i;
                cardAccessor.FrameSortingGroup.sortingOrder = 0;

                CardAnimationStart
                (
                    cardByIndex,
                    cardTransform.Position,
                    cardTransform.Rotation,
                    cardScale,
                    animationSpeed: 8
                ).Forget();
            }
        }

        private void OnCardDraggingStarted(object sender, CardDragEventArgs args)
        {
            if (Cards.All(x => x.Data.Id != args.Card.Data.Id))
                return;
            
            Debug.Log($"DRAG_START: {args.Card.Template.name}");

            _currentlyDraggedCard = args.Card;
            args.Card.GameObject.transform.rotation = Quaternion.Euler(args.Card.CacheRotation.eulerAngles.x, 0, 0);
        }

        private void OnCardDragging(object sender, CardDragEventArgs args)
        {
            if (Cards.All(x => x.Data.Id != args.Card.Data.Id))
                return;
            
            var cardPosition = new Vector3(args.Position.x, args.Card.GameObject.transform.position.y, args.Position.z);
            args.Card.GameObject.transform.position = cardPosition;

            var isOnTop = IsCardOnTopOfDroppableArea(cardPosition);
            CardAnimateBetweenFrame(args.Card, isOnTop).Forget();
            if (!isOnTop)
                return;
            
            var newVirtualIndex = PlayerCardBoardManager.Instance.FindVirtualIndex(args.Card);
            
            if (VirtualIndex == newVirtualIndex) 
                return;
            
            VirtualIndex = newVirtualIndex;
            PlayerCardBoardManager.Instance.RepositionCards(args.Card, VirtualIndex);
        }

        private void OnCardDraggingEnded(object sender, CardDragEventArgs args)
        {
            if (Cards.All(x => x.Data.Id != args.Card.Data.Id))
                return;
            
            Debug.Log($"DRAG_END: {args.Card.Template.name}");

            _currentlyDraggedCard = null;
            var cardPosition = new Vector3(args.Position.x, args.Card.GameObject.transform.position.y, args.Position.z);
            
            var isOnTop = IsCardOnTopOfDroppableArea(cardPosition);
            if (isOnTop && PlayerCardBoardManager.Instance.HasEnoughSlotsFor(args.Card))
            {
                Cards.Remove(args.Card);
                PlayerCardBoardManager.Instance.PlayCard(args.Card);
            }
            else
            {
                ReturnDraggedCardBackToHand();
                PlayerCardBoardManager.Instance.RepositionCards(args.Card);
            }
        }
    }
}
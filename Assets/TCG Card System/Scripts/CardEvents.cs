using System;
using System.Linq;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class CardEvents : MonoBehaviour
    {
        public static CardEvents Instance { get; private set; }

        public event EventHandler<Card> CardFocused;
        public event EventHandler<Card> CardUnfocused; 
        
        public event EventHandler<CardDragEventArgs> CardDraggingStarted;
        public event EventHandler<CardDragEventArgs> CardDragging;
        public event EventHandler<CardDragEventArgs> CardDraggingEnded;
        
        public event EventHandler<Card> FrameFocused;
        public event EventHandler<Card> FrameUnfocused; 
        
        public event EventHandler<CardDragEventArgs> FrameDraggingStarted;
        public event EventHandler<CardDragEventArgs> FrameDragging;
        public event EventHandler<CardDragEventArgs> FrameDraggingEnded;


        public Func<Card, bool> CanDragCard = _ => true; 
        public Func<Card, bool> CanDragFrame = _ => true;
        
        private static Camera MainCamera => Camera.main;

        private Card _focusedCard;
        private string _focusedTag;
        private bool _isFocused;

        private bool _isDraggingEnabled;
        private bool _isDragging;
        
        private bool _isOverDroppableArea;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void Update()
        {
            if (!MainCamera)
                return;
            
            if (!_isDragging)
            {
                DetectFocus();
            }
            
            if (!_isDraggingEnabled)
                return;

            // Start dragging
            if (_isFocused && Input.GetMouseButtonDown(0))
            {
                StartDragging();
            }

            // While dragging
            if (_isDragging)
            {
                OnDragging();
            }

            // Stop dragging
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }

        public void SetDraggingEnabled()
        {
            _isDraggingEnabled = true;
            _isDragging = false;
        }
        
        public void SetDraggingDisabled()
        {
            _isDraggingEnabled = false;
            _isDragging = false;
        }

        private void DetectFocus()
        {
            var foundHoverTarget = false;
            var layerMask = LayerMask.GetMask("Player");
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics
                .RaycastAll(ray, 100, layerMask)
                .Where(x => x.transform.CompareTag("Card") || x.transform.CompareTag("Frame"));
            
            var highestOrder = int.MinValue;
            Card card = null;
            string cardTag = null;
            
            foreach (var hit in hits)
            {
                var hitCardAccessor = hit.transform.parent.parent.GetComponent<CardAccessor>();
                if (hitCardAccessor == null || hitCardAccessor.CardSortingGroup.sortingOrder <= highestOrder)
                    continue;

                foundHoverTarget = true;
                highestOrder = hitCardAccessor.CardSortingGroup.sortingOrder;
                card = hitCardAccessor.Card;
                cardTag = hit.transform.tag;
            }

            switch (_isFocused)
            {
                // Refocus on other card
                case true when foundHoverTarget && _focusedCard?.Data?.Id != card?.Data?.Id:
                    Unfocus(_focusedCard);
                    Focus(card, cardTag);
                    break;
                // UnFocus
                case true when !foundHoverTarget:
                    Unfocus(_focusedCard);
                    break;
                // Focus card
                case false when foundHoverTarget:
                    Focus(card, cardTag);
                    break;
            }
        }

        private void Focus(Card card, string cardTag)
        {
            if (card.Template == null || card.Data == null)
                return;
            
            _focusedCard = card;
            _focusedTag = cardTag;
            _isFocused = true;
            
            // Debug.Log($"FOCUS: {CardTag} - {Card.Template.name}");
            
            switch (cardTag)
            {
                case "Card":
                    CardFocused?.Invoke(this, card);
                    break;
                case "Frame":
                    FrameFocused?.Invoke(this, card);
                    break;
            }
        }
        
        private void Unfocus(Card card)
        {
            if (card.Template == null || card.Data == null)
                return;
            
            // Debug.Log($"UNFOCUS: {_focusedTag} - {Card.Template.name}");
            
            switch (_focusedTag)
            {
                case "Card":
                    CardUnfocused?.Invoke(this, card);
                    break;
                case "Frame":
                    FrameUnfocused?.Invoke(this, card);
                    break;
            }
            
            _focusedCard = null;
            _focusedTag = null;
            _isFocused = false;
        }

        private Vector3 _dragOffset;
        private void StartDragging()
        {
            switch (_focusedTag)
            {
                case "Card" when !CanDragCard(_focusedCard):
                case "Frame" when !CanDragFrame(_focusedCard):
                    return;
            }

            _isDragging = true;

            // Convert the mouse position to a world point at the depth of the card.
            var startMousePositionWorld = CurrentDragPosition();

            // Calculate the offset between the card's position and the mouse position in the world.
            _dragOffset = _focusedCard.GameObject.transform.position - startMousePositionWorld;

            switch (_focusedTag)
            {
                case "Card":
                    CardDraggingStarted?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = startMousePositionWorld
                    });
                    break;
                case "Frame":
                    FrameDraggingStarted?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = startMousePositionWorld
                    });
                    break;
            }

        }

        private void OnDragging()
        {
            var currentDragPosition = CurrentDragPosition();

            // Slowly reduce the _dragOffset towards zero to make the drag more 'natural'
            // The third parameter controls the rate at which the offset is reduced
            _dragOffset = Vector3.Lerp(_dragOffset, Vector3.zero, Time.deltaTime * 8f);

            // Update the position of the dragged object by applying the adjusted _dragOffset
            var newPosition = currentDragPosition + _dragOffset;

            switch (_focusedTag)
            {
                case "Card":
                    CardDragging?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = newPosition
                    });
                    break;
                case "Frame":
                    FrameDragging?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = newPosition
                    });
                    break;
            }
        }

        private void StopDragging()
        {
            var currentDragPosition = CurrentDragPosition();

            _isDragging = false;
            
            switch (_focusedTag)
            {
                case "Card":
                    CardDraggingEnded?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = currentDragPosition
                    });
                    break;
                case "Frame":
                    FrameDraggingEnded?.Invoke(this, new CardDragEventArgs
                    {
                        Card = _focusedCard,
                        Position = currentDragPosition
                    });
                    break;
            }
            
            Unfocus(_focusedCard);
        }

        private Vector3 CurrentDragPosition()
        {
            var cardPosition = _focusedCard.GameObject.transform.position;

            // Recalculate the card's depth from the camera every frame in case of camera movement.
            var cardScreenDepth = MainCamera.WorldToScreenPoint(cardPosition).z;

            // Convert the current mouse position to a world point at the card's depth.
            return MainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cardScreenDepth));
        }
        
    }
}
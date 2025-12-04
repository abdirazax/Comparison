using System.Linq;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Enums;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.Opponent;
using UnityEngine;

namespace TCG_Card_System.Scripts.Player
{
    public class PlayerCardBoardManager : CardBoardManager
    {
        public override Team Team { get; } = new Team("Player Team", 100);
        
        public static PlayerCardBoardManager Instance { get; private set; }
        
        protected override string LayerName => "Player";

        [SerializeField]
        private GameObject pointerHeadPrefab;
        
        private Camera _camera;
        private LineRenderer _lineRenderer;
        private GameObject _pointerHead;

        private Card _draggingCard;

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            
            base.Awake();
            
            _camera = Camera.main;
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            CardEvents.Instance.FrameFocused += OnFrameFocused;
            CardEvents.Instance.FrameUnfocused += OnFrameUnfocused;
            CardEvents.Instance.FrameDraggingStarted += OnFrameDraggingStarted;
            CardEvents.Instance.FrameDragging += OnFrameDragging;
            CardEvents.Instance.FrameDraggingEnded += OnFrameDraggingEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            CardEvents.Instance.FrameFocused -= OnFrameFocused;
            CardEvents.Instance.FrameUnfocused -= OnFrameUnfocused;
            CardEvents.Instance.FrameDraggingStarted -= OnFrameDraggingStarted;
            CardEvents.Instance.FrameDragging -= OnFrameDragging;
            CardEvents.Instance.FrameDraggingEnded -= OnFrameDraggingEnded;
        }

        public void TargetingStart(Vector3 position)
        {
            CursorManager.Instance.Hide();
            
            _pointerHead = Instantiate(pointerHeadPrefab, transform, true);
            _pointerHead.transform.position = position;
            
            _lineRenderer.positionCount = 2;
        }
        
        public void TargetingStop()
        {
            if (_pointerHead == null)
                return;
            
            Destroy(_pointerHead);
            _pointerHead = null;
            
            CursorManager.Instance.Show();
            
            _lineRenderer.positionCount = 0;
        }

        public async UniTaskVoid ReturnDraggedFrameBackToBoard()
        {
            if (_draggingCard == null)
                return;
            
            var cardAccessor = _draggingCard.GameObject.GetComponent<CardAccessor>();
            cardAccessor.FrameSortingGroup.sortingOrder = 0;
            
            SetUnbreakableAnimation(_draggingCard, true);

            await CardAnimationStart
            (
                _draggingCard,
                _draggingCard.CachePosition,
                _draggingCard.CacheRotation,
                _draggingCard.GameObject.transform.localScale,
                true
            );
            
            SetUnbreakableAnimation(_draggingCard, false);
        }
        
        private async UniTask LocateTarget(Card card)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var playerLayerMask = LayerMask.GetMask("Player");
            var opponentLayerMask = LayerMask.GetMask("Opponent");
            
            var playerFrameHits = Physics
                .RaycastAll(ray, 100, playerLayerMask)
                .Where(x => x.transform.CompareTag("Frame"))
                .ToList();
            
            var opponentFrameHits = Physics
                .RaycastAll(ray, 100, opponentLayerMask)
                .Where(x => x.transform.CompareTag("Frame"))
                .ToList();

            Card targetPlayerCard = null;
            Card targetOpponentCard = null;

            // Only if one exact frame is selected
            if (playerFrameHits.Count + opponentFrameHits.Count != 1)
            {
                return;
            }

            if (playerFrameHits.Any())
            {
                var targetTransform = playerFrameHits.First().transform.parent.parent;
                var targetPlayerCardAccessor = targetTransform.GetComponent<CardAccessor>();
                targetPlayerCard = targetPlayerCardAccessor.Card;    
            }
            else if (opponentFrameHits.Any())
            {
                var targetTransform = opponentFrameHits.First().transform.parent.parent;
                var targetOpponentCardAccessor = targetTransform.GetComponent<CardAccessor>();
                targetOpponentCard = targetOpponentCardAccessor.Card;
            }
            
            // TODO: Multiple actions according to card
            if (targetPlayerCard != null)
            {
            }
            // TODO: Multiple actions according to card
            else if (targetOpponentCard != null)
            {
                if (targetOpponentCard.State == EBattleCardState.Death)
                    return;
                
                await CardAttackOpponentBoard
                (
                    card, 
                    OpponentCardBoardManager.Instance
                    // () =>
                    // {
                    //     CardUpdateResolve(card.Data.Id);
                    //     OpponentBattleBoardManager.Instance.CardUpdateResolve(targetOpponentCard.Data.Id);
                    //
                    //     finished?.Invoke();
                    // }
                );
            }
        }

        private void OnFrameFocused(object sender, Card card)
        {
            
        }

        private void OnFrameUnfocused(object sender, Card card)
        {
            
        }

        private async void OnFrameDraggingStarted(object sender, CardDragEventArgs args)
        {
            if (!CanDrag(args.Card))
                return;

            _draggingCard = args.Card;

            TargetingStart(args.Position);
            
            var cardAccessor = args.Card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.FrameSortingGroup.sortingOrder = 100;
            
            await RaiseCard(args.Card);
        }

        private void OnFrameDragging(object sender, CardDragEventArgs args)
        {
            if (!CanDrag(args.Card))
                return;
            
            _pointerHead.transform.position = args.Position;
            
            var startPoint = args.Card.GameObject.transform.position;
            var endPoint = args.Position;
            var shortenAmount = 1f;
            
            _lineRenderer.SetPosition(0, startPoint);
            
            // Calculate the direction from the start point to the end point
            var direction = (endPoint - startPoint).normalized;

            // Calculate the length of the original line
            var length = Vector3.Distance(startPoint, endPoint);

            // Ensure we're not trying to shorten the line more than its original length
            if (shortenAmount > length) {
                // Handle this situation: perhaps set shortenAmount to length, or alert the user
                shortenAmount = length; // This would effectively make the line disappear
            }

            // Calculate the new length of the line
            var newLength = length - shortenAmount;

            // Calculate the new end point by moving from the start point towards the original end point
            // by the distance of the new length
            var newEndPoint = startPoint + direction * newLength;
            
            _lineRenderer.SetPosition(1, newEndPoint);
        }

        private async void OnFrameDraggingEnded(object sender, CardDragEventArgs args)
        {
            if (!CanDrag(args.Card))
                return;
            
            TargetingStop();
            
            await LocateTarget(args.Card);
            
            var cardAccessor = args.Card.GameObject.GetComponent<CardAccessor>();
            cardAccessor.FrameSortingGroup.sortingOrder = 0;

            Debug.Log(cardScale);
            
            await CardAnimationStart
            (
                args.Card,
                args.Card.CachePosition,
                args.Card.CacheRotation,
                cardScale
            );
            
            _draggingCard = null;
        }

        private bool CanDrag(Card card) =>
            Cards.Any(x => x.Data.Id == card.Data.Id) &&
            card.Data.CanAttack &&
            card.State != EBattleCardState.Death;
    }
}
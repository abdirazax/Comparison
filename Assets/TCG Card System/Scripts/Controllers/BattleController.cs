using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Enums;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.Opponent;
using TCG_Card_System.Scripts.Player;
using TCG_Card_System.Scripts.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace TCG_Card_System.Scripts.Controllers
{
    [RequireComponent(typeof(BattleStateManager))]
    public class BattleController : MonoBehaviour
    {
        BattleStateManager _battleStateManager;
        [SerializeField] private Button _playerButton1, _playerButton2, _opponentButton1, _opponentButton2;
        private TMP_Text _playerButton1Text, _playerButton2Text, _opponentButton1Text, _opponentButton2Text;
        private void Awake()
        {
            _battleStateManager = GetComponent<BattleStateManager>();
            _playerButton1Text = _playerButton1.GetComponentInChildren<TMP_Text>();
            _playerButton2Text = _playerButton2.GetComponentInChildren<TMP_Text>();
            _opponentButton1Text = _opponentButton1.GetComponentInChildren<TMP_Text>();
            _opponentButton2Text = _opponentButton2.GetComponentInChildren<TMP_Text>();
        }

        private void OnEnable()
        {
            _battleStateManager.OnBattleStateChanged += OnBattleStateChanged;

        }

        private void OnBattleStateChanged(object sender, BattleStateEventArgs e)
        {
            if (e.BattleState is BattleGoingOnState)
            {
                _playerButton1.gameObject.SetActive(true);
                _playerButton2.gameObject.SetActive(false);
                _playerButton1Text.text = "End Game";
                _playerButton1.onClick.RemoveAllListeners();
                _playerButton1.onClick.AddListener(() =>
                {
                    _battleStateManager.EndBattle();
                });
                _opponentButton1.gameObject.SetActive(false);
                _opponentButton2.gameObject.SetActive(false);
            }
            else if (e.BattleState is BattlePreparingState)
            {
                _playerButton1.gameObject.SetActive(true);
                _playerButton2.gameObject.SetActive(true);
                _playerButton1Text.text = "Serve Card";
                _playerButton2Text.text = "Start Game";
                _playerButton1.onClick.RemoveAllListeners();
                _playerButton2.onClick.RemoveAllListeners();
                _playerButton1.onClick.AddListener(() => {
                    PlayerServeCardToHand();
                });
                _playerButton2.onClick.AddListener(() => {
                    _battleStateManager.StartBattle();
                });
                _opponentButton1.gameObject.SetActive(true);
                _opponentButton2.gameObject.SetActive(true);
                _opponentButton1Text.text = "Serve Card";
                _opponentButton2Text.text = "Play Card";
                _opponentButton1.onClick.RemoveAllListeners();
                _opponentButton2.onClick.RemoveAllListeners();
                _opponentButton1.onClick.AddListener(() => {
                    OpponentServeCardToHand();
                });
                _opponentButton2.onClick.AddListener(() => {
                    OpponentPlayCardFromHand();
                });
            }
            else if (e.BattleState is BattleEndState)
            {
                _playerButton1.gameObject.SetActive(true);
                _playerButton2.gameObject.SetActive(false);
                _playerButton1Text.text = "Restart Game";
                _playerButton1.onClick.RemoveAllListeners();
                _playerButton1.onClick.AddListener(() => {
                    _battleStateManager.ResetBattle();
                });
                _opponentButton1.gameObject.SetActive(false);
                _opponentButton2.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _battleStateManager.OnBattleStateChanged -= OnBattleStateChanged;
            PlayerCardBoardManager.Instance.OnCardAttackedBoardStarted -= LocalPlayerOnCardAttackedBoardStarted;
            PlayerCardBoardManager.Instance.OnCardAttackedBoardEnded -= LocalPlayerOnCardAttackedBoardEnded;
        }
        

        #region Player
        
        private async void PlayerServeCardToHand()
        {
            ServeTemplateCardForHand(CardCollectionManager.Instance.playerCardTemplates, PlayerCardHandManager.Instance, PlayerCardDeckManager.Instance);
        }

        private void PlayerHandTurnStarted()
        {
            var cards = PlayerCardHandManager.Instance.Cards;

            foreach (var card in cards)
            {
                PlayerCardHandManager.Instance.ShowCardHighlight
                (
                    card,
                    4,
                    new Color(0, 191, 67, 255)
                );
            }
                
            CardEvents.Instance.SetDraggingEnabled();
        }
        
        
        
        private void LocalPlayerOnCardAttackedBoardStarted(object sender, CardAttackEventArgs args)
        {
            
            var card = PlayerCardBoardManager.Instance.GetCardById(args.CardId);
            card.Data.CanAttack = false;
            
            PlayerCardBoardManager.Instance.HideFrameHighlight(card);
        }
        
        private async void LocalPlayerOnCardAttackedBoardEnded(object sender, CardAttackEventArgs args)
        {
            var card = PlayerCardBoardManager.Instance.GetCardById(args.CardId);
            var targetBoard = OpponentCardBoardManager.Instance;

            var tasks = new List<UniTask>
            {
                PlayerCardBoardManager.Instance.CardAttackOpponentCardBoardShowDamage(card, args.TargetBoard),
            };
            
            
            await UniTask.WhenAll(tasks);
            
        }
        #endregion
        
        /// <summary>
        /// This section contains methods for the opponent's actions during the battle.
        /// </summary>
        #region Opponent
        private void OpponentServeCardToHand()
        {
            ServeTemplateCardForHand(CardCollectionManager.Instance.opponentCardTemplates, OpponentCardHandManager.Instance, OpponentCardDeckManager.Instance);
        }

        async void ServeTemplateCardForHand(List<CardTemplate> cardTemplates, CardHandManager cardHandManager, CardDeckManager cardDeckManager)
        {
            var random = new Random();
            var chosenCard = cardTemplates[random.Next(0, cardTemplates.Count)];
            
            var autoAttackInterval = UnityEngine.Random.Range
                (chosenCard.autoAttackInterval - 1, 
                    chosenCard.autoAttackInterval + 3);
            if(!cardHandManager.HasEnoughSlotsFor(chosenCard.slotSize))
                return;                
            var card = cardDeckManager.CardPrepareForDraw(new CardData
            {
                Id = Guid.NewGuid().ToString("N"),
                TotalMana = chosenCard.mana,
                Mana = chosenCard.mana,
                TotalHealth = chosenCard.health,
                Health = chosenCard.health,
                TotalAttack = chosenCard.attack,
                Attack = chosenCard.attack,
                TemplateId = chosenCard.id,
                AutoAttackInterval = autoAttackInterval,
                AttacksPerInterval = chosenCard.attackPerInterval
            });
            
            await cardHandManager.DrawCard(card);
        }
        
        async void ServeRandomCardForHand(List<CardTemplate> cardTemplates, CardHandManager cardHandManager, CardDeckManager cardDeckManager)
        {
            var random = new Random();
            var mana = random.Next(1, 9);
            var health = random.Next(1, 9);
            var autoAttackInterval = UnityEngine.Random.Range(2f, 8f);
            // var autoAttackInterval = 2f;
            var multipleAttacks = random.Next(0, 2) == 0; // 50% chance for multiple attacks
            var attacksPerInterval = multipleAttacks ? random.Next(1, 5) : 1;
            // var attacksPerInterval = 4;
            
            List<int> attack = new List<int>();
            for (int i = 0; i < attacksPerInterval; i++)
            {
                attack.Add(random.Next(1, 9));
            }

            
            Debug.Log("Attack per interval: " + attacksPerInterval);
            
            var templateIndex = random.Next(0, cardTemplates.Count);
            if(!cardHandManager.HasEnoughSlotsFor(cardTemplates[templateIndex].slotSize))
                return;                
            var card = cardDeckManager.CardPrepareForDraw(new CardData
            {
                Id = Guid.NewGuid().ToString("N"),
                TotalMana = mana,
                Mana = mana,
                TotalHealth = health,
                Health = health,
                TotalAttack = attack,
                Attack = attack,
                TemplateId = cardTemplates[templateIndex].id,
                AutoAttackInterval = autoAttackInterval,
                AttacksPerInterval = attacksPerInterval
            });
            
            await cardHandManager.DrawCard(card);
        }

        private async void OpponentPlayCardFromHand()
        {
            if (OpponentCardHandManager.Instance.Cards.Count == 0)
                return;
            var random = new Random();
            var cardIndex = random.Next(0, OpponentCardHandManager.Instance.Cards.Count);

            if (cardIndex < 0 )
                return;
            
            var card = OpponentCardHandManager.Instance.GetCardByIndex(cardIndex);
            
            if(!OpponentCardBoardManager.Instance.HasEnoughSlotsFor(card))
            return;
            
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            
            cardAccessor.CardDisplay.UpdateUI(card);
            
            await OpponentCardHandManager.Instance.PlayCard(card, 0);
        }
        
        private async void OpponentAttackPlayerCard()
        {
            var random = new Random();
            
            var cardIndex = random.Next(0, OpponentCardHandManager.Instance.Cards.Count);
            var targetCardIndex = random.Next(0, PlayerCardBoardManager.Instance.Cards.Count);
            var card = OpponentCardBoardManager.Instance.GetCardById(OpponentCardBoardManager.Instance.Cards[cardIndex].Data.Id);
            var targetCard = PlayerCardBoardManager.Instance.GetCardById(PlayerCardBoardManager.Instance.Cards[targetCardIndex].Data.Id);

            var showDamageTask = UniTask.CompletedTask;
            
            await OpponentCardBoardManager.Instance.RaiseCard(card);
            await OpponentCardBoardManager.Instance.CardAttackOpponentBoard
            (
                card,
                PlayerCardBoardManager.Instance,
                () =>
                {
                    OpponentCardBoardManager.Instance.CardUpdate(card.Data);
                    PlayerCardBoardManager.Instance.CardUpdate(targetCard.Data);
                    
                    showDamageTask = OpponentCardBoardManager.Instance.CardAttackOpponentCardBoardShowDamage(card, PlayerCardBoardManager.Instance);
                    return UniTask.CompletedTask;
                }
            );
            
            await OpponentCardBoardManager.Instance.LowerCard(card);

            showDamageTask.ContinueWith(() =>
            {
                OpponentCardBoardManager.Instance.CardDestroy(card.Data.Id).Forget();
                PlayerCardBoardManager.Instance.CardDestroy(targetCard.Data.Id).Forget();
            });
        }
        #endregion
    }
}
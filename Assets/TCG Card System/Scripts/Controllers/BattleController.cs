using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Enums;
using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.Opponent;
using TCG_Card_System.Scripts.Player;
using UnityEngine;
using Random = System.Random;

namespace TCG_Card_System.Scripts.Controllers
{
    public class BattleController : MonoBehaviour
    {
        private void OnGUI()
        {
            PlayerCardBoardManager.Instance.CardAttackedCardStarted += Local_PlayerCardAttackedCardStarted;
            PlayerCardBoardManager.Instance.CardAttackedCardEnded += Local_PlayerCardAttackedCardEnded;
            
            if (GUI.Button(new Rect(10, 5, 150, 50), "[Player] Serve Card"))
            {
                PlayerServeCardToHand();
            }
            if (GUI.Button(new Rect(160, 5, 150, 50), "[Player] Start Turn"))
            {
                PlayerHandTurnStarted();
                PlayerBoardTurnStarted();
            }
            if (GUI.Button(new Rect(160, 55, 150, 50), "[Player] End Turn"))
            {
               PlayerHandTurnEnded();
               PlayerBoardTurnEnded();
            }
            
            if (GUI.Button(new Rect(410, 5, 150, 50), "[Opponent] Serve Card"))
            {
                OpponentServeCardToHand();
            }
            if (GUI.Button(new Rect(560, 5, 150, 50), "[Opponent] Play Card"))
            {
                OpponentPlayCardFromHand();
            }
            if (GUI.Button(new Rect(710, 5, 160, 50), "[Opponent] Attack Player"))
            {
                OpponentAttackPlayerCard();
            }
        }

        #region Player
        
        private async void PlayerServeCardToHand()
        {
            var random = new Random();
            var mana = random.Next(1, 9);
            var health = random.Next(1, 9);
            var attack = random.Next(1, 9);

            var templateIndex = random.Next(0, CardCollectionManager.Instance.cardTemplates.Count);
                
            var card = PlayerCardDeckManager.Instance.CardPrepareForDraw(new CardData
            {
                Id = Guid.NewGuid().ToString("N"),
                TotalMana = mana,
                Mana = mana,
                TotalHealth = health,
                Health = health,
                TotalAttack = attack,
                Attack = attack,
                TemplateId = CardCollectionManager.Instance.cardTemplates[templateIndex].id,
            });
            await PlayerCardDeckManager.Instance.DrawCardToMidPoint(card);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
                
            await PlayerCardHandManager.Instance.DrawCard(card);
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
        
        private void PlayerHandTurnEnded()
        {
            var cards = PlayerCardHandManager.Instance.Cards;

            foreach (var card in cards)
            {
                PlayerCardHandManager.Instance.HideCardHighlight(card);
            }
                
            CardEvents.Instance.SetDraggingDisabled();
        }

        private void PlayerBoardTurnStarted()
        {
            var cards = PlayerCardBoardManager.Instance.Cards;

            foreach (var card in cards)
            {
                card.Data.CanAttack = true;
                
                PlayerCardBoardManager.Instance.ShowFrameHighlight
                (
                    card,
                    4,
                    new Color(0, 191, 67, 255)
                );    
            }
        }

        public void PlayerBoardTurnEnded()
        {
            foreach (var card in PlayerCardBoardManager.Instance.Cards)
                PlayerCardBoardManager.Instance.HideFrameHighlight(card);
        }
        
        
        private void Local_PlayerCardAttackedCardStarted(object sender, (string CardId, string TargetCardId) args)
        {
            var (cardId, targetCardId) = args;
            
            var card = PlayerCardBoardManager.Instance.GetCardById(cardId);
            card.Data.CanAttack = false;
            
            PlayerCardBoardManager.Instance.HideFrameHighlight(card);
        }
        
        private async void Local_PlayerCardAttackedCardEnded(object sender, (string CardId, string TargetCardId) args)
        {
            var (cardId, targetCardId) = args;
            var card = PlayerCardBoardManager.Instance.GetCardById(cardId);
            var targetCard = OpponentCardBoardManager.Instance.GetCardById(targetCardId);

            var tasks = new List<UniTask>
            {
                PlayerCardBoardManager.Instance.CardAttackOpponentCardShowDamage(card, targetCard),
            };

            if (card.Template.attackType.type == ECardAttack.Melee)
            {
                tasks.Add(OpponentCardBoardManager.Instance.CardAttackOpponentCardShowDamage(targetCard, card));
            }
            
            await UniTask.WhenAll(tasks);
            
            PlayerCardBoardManager.Instance.CardDestroy(cardId).Forget();      
            OpponentCardBoardManager.Instance.CardDestroy(targetCardId).Forget();
        }
        #endregion
        
        
        #region Opponent
        private async void OpponentServeCardToHand()
        {
            var card = OpponentCardDeckManager.Instance.CardPrepareForDraw(null);
            
            await OpponentCardHandManager.Instance.DrawCard(card);
        }

        private async void OpponentPlayCardFromHand()
        {
            var random = new Random();
            var cardIndex = random.Next(0, OpponentCardHandManager.Instance.Cards.Count);

            if (cardIndex < 0)
                return;
            
            var card = OpponentCardHandManager.Instance.GetCardByIndex(cardIndex);
            var cardAccessor = card.GameObject.GetComponent<CardAccessor>();
            
            var mana = random.Next(1, 9);
            var health = random.Next(1, 9);
            var attack = random.Next(1, 9);

            var templateIndex = random.Next(0, CardCollectionManager.Instance.cardTemplates.Count);
            
            card.SetData(new CardData
            {
                Id = Guid.NewGuid().ToString("N"),
                TotalMana = mana,
                Mana = mana,
                TotalHealth = health,
                Health = health,
                TotalAttack = attack,
                Attack = attack,
                TemplateId = CardCollectionManager.Instance.cardTemplates[templateIndex].id,
            });
            cardAccessor.CardDisplay.UpdateUI(card);
            cardAccessor.FrameDisplay.UpdateUI(card);
            
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
            await OpponentCardBoardManager.Instance.CardAttackOpponentCard
            (
                card,
                targetCard,
                () =>
                {
                    OpponentCardBoardManager.Instance.CardUpdate(card.Data);
                    PlayerCardBoardManager.Instance.CardUpdate(targetCard.Data);
                    
                    showDamageTask = OpponentCardBoardManager.Instance.CardAttackOpponentCardShowDamage(card, targetCard);
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
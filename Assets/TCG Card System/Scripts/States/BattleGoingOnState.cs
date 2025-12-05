using TCG_Card_System.Scripts;
using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.States;
using UnityEngine;

public class BattleGoingOnState : BattleBaseState
{
    
    public override void OnEnter(BattleStateManager battle)
    {
        foreach (Card card in battle.playerCardBoardManager.Cards)
        {
            card.AutoAttackTimer = 0f;
            card.AttackIndexInCombo = 0;
            card.InBattle = true;
            card.GameObject.GetComponent<CardAccessor>().UpdateUI();
        }
        foreach (Card card in battle.opponentCardBoardManager.Cards)
        {
            card.AutoAttackTimer = 0f;
            card.AttackIndexInCombo = 0;
            card.InBattle = true;
            card.GameObject.GetComponent<CardAccessor>().UpdateUI();
        }
        
        CardEvents.Instance.SetDraggingDisabled();
    }

    public override void OnUpdate(BattleStateManager battle)
    {
        HandleTeam(battle.playerCardBoardManager, battle.opponentCardBoardManager);
        HandleTeam(battle.opponentCardBoardManager, battle.playerCardBoardManager);
    }

    private void HandleTeam(CardBoardManager attackerBoardManager, CardBoardManager defenderBoardManager)
    {
        foreach (Card card in attackerBoardManager.Cards)
        {
            card.AutoAttackTimer+= Time.deltaTime;
            
            if (card.AutoAttackTimer >= 
                (card.AttackIndexInCombo + 1) * 
                card.Data.AutoAttackInterval / 
                card.Data.AttacksPerInterval)
            {
                if (card.AutoAttackTimer >= card.Data.AutoAttackInterval)
                {
                    card.AutoAttackTimer %= card.Data.AutoAttackInterval;
                    card.AttackIndexInCombo = 0;
                }
                else
                {
                    card.AttackIndexInCombo++;
                }
                attackerBoardManager.CardAttackOpponentBoard(card, defenderBoardManager);
                if (defenderBoardManager.Team.IsDead)
                {
                    BattleStateManager.Instance.EndBattle();
                }
            }
            card.GameObject.GetComponent<CardAccessor>().UpdateUI(card);
        }
    }

    public override void OnExit(BattleStateManager battle)
    {
        foreach (Card card in battle.playerCardBoardManager.Cards)
        {
            card.InBattle = false;
            card.GameObject.GetComponent<CardAccessor>().UpdateUI(card);
        }
        foreach (Card card in battle.opponentCardBoardManager.Cards)
        {
            card.InBattle = false;
            card.GameObject.GetComponent<CardAccessor>().UpdateUI(card);
        }
        
        CardEvents.Instance.SetDraggingEnabled();
    }
}

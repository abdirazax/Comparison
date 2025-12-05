using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.States;
using UnityEngine;

public class BattleEndState : BattleBaseState
{
    public override void OnEnter(BattleStateManager battle)
    {
        
    }

    public override void OnUpdate(BattleStateManager battle)
    {
    }

    public override void OnExit(BattleStateManager battle)
    {
        battle.playerCardDeckManager.ResetDeck();
        battle.opponentCardDeckManager.ResetDeck();
        battle.playerCardBoardManager.ResetBoard();
        battle.opponentCardBoardManager.ResetBoard();
        battle.playerCardHandManager.ResetHand();
        battle.opponentCardHandManager.ResetHand();
    }
}

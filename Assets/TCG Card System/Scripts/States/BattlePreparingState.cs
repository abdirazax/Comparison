using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts.States
{
    public class BattlePreparingState : BattleBaseState
    {
        public override void OnEnter(BattleStateManager battle)
        {
            Debug.Log("Entering Battle Preparing State");
            InitializeBoard(battle.playerCardBoardManager);
            InitializeBoard(battle.opponentCardBoardManager);
            CardEvents.Instance.SetDraggingEnabled();
        }

        public override void OnUpdate(BattleStateManager battle)
        {
        }

        public override void OnExit(BattleStateManager battle)
        {
            CardEvents.Instance.SetDraggingDisabled();
        }
    
        private void InitializeBoard
            (CardBoardManager cardBoard)
        {
            foreach (var card in cardBoard.Cards)
            {
                card.Initialize();
                card.GameObject.GetComponent<CardAccessor>().UpdateUI();
            }
            cardBoard.Team.Initialize();
            cardBoard.RepositionCards();
        }
    }
}

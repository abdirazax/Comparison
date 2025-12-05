using System;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Opponent;
using TCG_Card_System.Scripts.Player;
using TCG_Card_System.Scripts.States;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class BattleStateManager : MonoBehaviour
    {
        public PlayerCardBoardManager playerCardBoardManager;
        public OpponentCardBoardManager opponentCardBoardManager;
        
        public PlayerCardDeckManager playerCardDeckManager;
        public OpponentCardDeckManager opponentCardDeckManager;
        
        public PlayerCardHandManager playerCardHandManager;
        public OpponentCardHandManager opponentCardHandManager;
    
        public event EventHandler<BattleStateEventArgs> OnBattleStateChanged; 
        
        BattleBaseState _battleCurrentState;
        BattlePreparingState _battlePreparingState = new BattlePreparingState();
        BattleGoingOnState _battleGoingOnState = new BattleGoingOnState();
        BattleEndState _battleEndState = new BattleEndState();
        
        public static BattleStateManager Instance { get; private set; }
        
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
            playerCardBoardManager = PlayerCardBoardManager.Instance;
            if (playerCardBoardManager == null)
            {
                Debug.LogError("PlayerCardBoardManager component not found.");
                return;
            }
            opponentCardBoardManager = OpponentCardBoardManager.Instance;
            if (opponentCardBoardManager == null)
            {
                Debug.LogError("OpponentCardBoardManager component not found.");
                return;
            }
            ChangeState(_battlePreparingState);
        }

        private void ChangeState(BattleBaseState newState)
        {
            if(!IsStateChangeAllowed(newState))
                return;
            _battleCurrentState?.OnExit(this);
            _battleCurrentState = newState;
            _battleCurrentState.OnEnter(this);
            Debug.Log($"Battle state changed to: {_battleCurrentState.GetType().Name}");
            OnBattleStateChanged?.Invoke(this, new BattleStateEventArgs()
            {
                BattleState = _battleCurrentState
            });
        }

        private bool IsStateChangeAllowed(BattleBaseState newState)
        {
            if (_battleCurrentState == null)
                return true;
            if (_battleCurrentState is BattlePreparingState && newState is BattleGoingOnState)
                return true;
            if (_battleCurrentState is BattleGoingOnState && newState is BattleEndState)
                return true;
            if (_battleCurrentState is BattleEndState && newState is BattlePreparingState)
                return true;
            return false;
        }
        
        private void Update()
        {
            _battleCurrentState.OnUpdate(this);
        }
        
        
        public void StartBattle()
        {
            ChangeState(_battleGoingOnState);
        }
        public void EndBattle()
        {
            ChangeState(_battleEndState);
        }

        public void ResetBattle()
        {
            ChangeState(_battlePreparingState);
        }
    }
}

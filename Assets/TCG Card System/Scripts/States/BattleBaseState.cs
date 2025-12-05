using TCG_Card_System.Scripts.Managers;

namespace TCG_Card_System.Scripts.States
{
    public abstract class BattleBaseState
    {
        public abstract void OnEnter(BattleStateManager battle);
    
        public abstract void OnUpdate(BattleStateManager battle);
    
        public abstract void OnExit(BattleStateManager battle);
    }
}

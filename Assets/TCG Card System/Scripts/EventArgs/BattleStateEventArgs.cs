using TCG_Card_System.Scripts.States;

namespace TCG_Card_System.Scripts.EventArgs
{
    public class BattleStateEventArgs : System.EventArgs
    {
        public BattleBaseState BattleState { get; set; }
    }
}
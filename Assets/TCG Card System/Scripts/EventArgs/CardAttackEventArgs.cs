using TCG_Card_System.Scripts.Managers;

namespace TCG_Card_System.Scripts.EventArgs
{
    public class CardAttackEventArgs : System.EventArgs
    {
        public CardBoardManager TargetBoard { get; set; }
        
        public string CardId { get; set; }
    }
}
using TCG_Card_System.Scripts.Managers;

namespace TCG_Card_System.Scripts.EventArgs
{
    public class TeamGotAttackedEventArgs : System.EventArgs
    {
        public Team Team { get; set; }
        
        public float Damage { get; set; }
    }
}
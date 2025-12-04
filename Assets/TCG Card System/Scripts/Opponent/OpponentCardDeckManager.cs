using TCG_Card_System.Scripts.Managers;

namespace TCG_Card_System.Scripts.Opponent
{
    public class OpponentCardDeckManager : CardDeckManager
    {
        public static OpponentCardDeckManager Instance { get; private set; }
        
        protected override string LayerName => "Opponent";
        
        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
    }
}
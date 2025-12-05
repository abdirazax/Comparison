using TCG_Card_System.Scripts.Managers;

namespace TCG_Card_System.Scripts.Opponent
{
    public class OpponentCardBoardManager : CardBoardManager
    {
        public static OpponentCardBoardManager Instance { get; private set; }
        
        protected override string LayerName => "Opponent";
        
        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            
            base.Awake();
        }
    }
}
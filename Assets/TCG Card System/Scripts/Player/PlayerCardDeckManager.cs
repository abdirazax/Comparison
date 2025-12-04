using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts.Player
{
    public class PlayerCardDeckManager : CardDeckManager
    {
        public static PlayerCardDeckManager Instance { get; private set; }
        
        protected override string LayerName => "Player";
        
        [SerializeField]
        private Transform midPointTransform;
        
        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
        
        public async UniTask DrawCardToMidPoint(Card card)
        {
            
            card.GameObject.transform.SetParent(midPointTransform);

            await CardAnimationStart
            (
                card,
                midPointTransform.position,
                Quaternion.Euler(90, -90, -90), 
                cardScale * 2
            );
        }
    }
}
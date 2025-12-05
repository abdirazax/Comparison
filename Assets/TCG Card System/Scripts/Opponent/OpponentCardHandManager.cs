using System;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts.Opponent
{
    public class OpponentCardHandManager : CardHandManager
    {
        public static OpponentCardHandManager Instance { get; private set; }
        
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

        public async UniTask PlayCard(Card card, int boardIndex)
        {
            Cards.Remove(card);
            RepositionCards();
            OpponentCardBoardManager.Instance.PlayCard(card, boardIndex);
            
        }
    }
}
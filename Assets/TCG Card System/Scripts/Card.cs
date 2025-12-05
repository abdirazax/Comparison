using System;
using TCG_Card_System.Scripts.Enums;
using TCG_Card_System.Scripts.Managers;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class Card
    {
        public CardTemplate Template { get; private set; }
        public CardData Data { get; private set; }
        public event EventHandler DataChanged;
        
        public EBattleCardState State { get; set; }
        public bool UnbreakableAnimation { get; set; }
        
        public GameObject GameObject;
        public Vector3 CachePosition;
        public Quaternion CacheRotation;
        
        public Card() {}
        public Card(CardData cardData) => SetData(cardData);

        public void SetData(CardData cardData)
        {
            Template = CardCollectionManager.Instance.cardTemplates.Find(x => x.id == cardData.TemplateId);
            Data = cardData;
            
            if (Data.Health <= 0)
                State = EBattleCardState.Death;
            
            DataChanged?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace TCG_Card_System.Scripts
{
    public class CardData : IEquatable<CardData>
    {
        public string Id;
        public string TemplateId;
        
        public bool CanAttack;
        
        public int TotalMana;
        public List<int> TotalAttack;
        public int TotalHealth;
        
        public int Mana;
        public List<int> Attack;
        public int Health;

        public float AutoAttackInterval;
        public int AttacksPerInterval;
        
        [UsedImplicitly]
        public CardData() {}
        public CardData(CardTemplate template)
        {
            Id = Guid.NewGuid().ToString("N");
            TemplateId = template.id;

            TotalMana = template.mana;
            TotalAttack = template.attack;
            TotalHealth = template.health;

            Mana = template.mana;
            Attack = template.attack;
            Health = template.health;
            
            AutoAttackInterval = template.autoAttackInterval;
            AttacksPerInterval = template.attackPerInterval;
        }

        public bool Equals(CardData other) =>
            Id == other?.Id;
    }
}
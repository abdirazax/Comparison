using System;
using JetBrains.Annotations;

namespace TCG_Card_System.Scripts
{
    public class CardData : IEquatable<CardData>
    {
        public string Id;
        public string TemplateId;
        
        public bool CanAttack;
        
        public int TotalMana;
        public int TotalAttack;
        public int TotalHealth;
        
        public int Mana;
        public int Attack;
        public int Health;

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
        }

        public bool Equals(CardData other) =>
            Id == other?.Id;
    }
}
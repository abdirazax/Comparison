using System;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Interfaces;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class Team
    {
        public string teamName;
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        
        public event EventHandler<TeamGotAttackedEventArgs> OnTeamGotAttacked;
        
        public Team(string name = "Team ", int maxHealth = 30)
        {
            teamName = name;
            MaxHealth = maxHealth;
            Initialize();
        }
        
        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsDead = true;
            }
            OnTeamGotAttacked?.Invoke(this, new TeamGotAttackedEventArgs()
            {
                Team = this,
                Damage = damage,
            });
        }

        public void Initialize()
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
        }
    }
}

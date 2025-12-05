using System.Collections.Generic;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class CardSkinsManager : MonoBehaviour
    {
        public static CardSkinsManager Instance { get; private set; }

        [SerializeField]
        public List<CardSkinTemplate> cardSkinTemplates = new();
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
        
        public CardSkinTemplate GetCardSkinTemplate(string templateId) =>
            cardSkinTemplates.Find(x => x.id == templateId);
    }
}
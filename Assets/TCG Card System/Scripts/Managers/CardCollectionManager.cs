using System.Collections.Generic;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class CardCollectionManager : MonoBehaviour
    {
        public static CardCollectionManager Instance { get; private set; }

        [SerializeField]
        public List<CardTemplate> cardTemplates = new();
        
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
        
        public CardTemplate GetCardTemplate(string templateId) =>
            cardTemplates.Find(x => x.id == templateId);
    }
}
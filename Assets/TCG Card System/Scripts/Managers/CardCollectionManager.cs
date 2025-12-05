using System.Collections.Generic;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class CardCollectionManager : MonoBehaviour
    {
        public static CardCollectionManager Instance { get; private set; }

        [SerializeField]
        public List<CardTemplate> playerCardTemplates = new();
        [SerializeField]
        public List<CardTemplate> opponentCardTemplates = new();
        
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
    }
}
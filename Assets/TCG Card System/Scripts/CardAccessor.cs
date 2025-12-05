    using TCG_Card_System.Scripts.Effects;
    using TCG_Card_System.Scripts.Managers;
    using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace TCG_Card_System.Scripts
{
    public class CardAccessor : MonoBehaviour
    {
        #region Card
        
        public Card Card;
        
        public GameObject cardLayout;

        public CardDisplay CardDisplay => cardLayout.GetComponent<CardDisplay>();
        public SortingGroup CardSortingGroup => cardLayout.GetComponent<SortingGroup>();
        
        #endregion

        #region Attack
        
        public GameObject attackLayout;

        public CardAttackDisplay CardAttackDisplay => attackLayout.GetComponent<CardAttackDisplay>();
        public SortingGroup FrameSortingGroup => attackLayout.GetComponent<SortingGroup>();
     
        #endregion

        public void UpdateUI(Card card)
        {
            CardDisplay.UpdateUI(card);
            CardAttackDisplay.UpdateUI(card);
        }

        public void UpdateUI()
        {
            CardDisplay.UpdateUI(Card);
            CardAttackDisplay.UpdateUI(Card);
        }
    }
}
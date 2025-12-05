using TCG_Card_System.Scripts.Effects;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCG_Card_System.Scripts
{
    public class CardAccessor : MonoBehaviour
    {
        public Card Card;
        
        #region Card
        public GameObject cardLayout;

        public CardDisplay CardDisplay => cardLayout.GetComponent<CardDisplay>();
        public SortingGroup CardSortingGroup => cardLayout.GetComponent<SortingGroup>();
        
        public DissolveEffect CardDissolveEffect => cardLayout.GetComponent<DissolveEffect>();
        public HighlightEffect CardHighlightEffect => cardLayout.GetComponent<HighlightEffect>();
        
        #endregion

        #region Frame
        
        public GameObject frameLayout;

        public FrameDisplay FrameDisplay => frameLayout.GetComponent<FrameDisplay>();
        public SortingGroup FrameSortingGroup => frameLayout.GetComponent<SortingGroup>();
        
        public DissolveEffect FrameDissolveEffect => frameLayout.GetComponent<DissolveEffect>();
        public HighlightEffect FrameHighlightEffect => frameLayout.GetComponent<HighlightEffect>();
     
        #endregion
    }
}
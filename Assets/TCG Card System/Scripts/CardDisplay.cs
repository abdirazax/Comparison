using TMPro;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class CardDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro nameText;
        
        [SerializeField]
        private TextMeshPro descriptionText;
        
        [SerializeField]
        private TextMeshPro manaText;
        
        [SerializeField]
        private TextMeshPro attackText;
        
        [SerializeField]
        private TextMeshPro heathText;
        
        [SerializeField]
        private GameObject uiHelpItem;
        
        private static readonly int ShaderBackgroundImageId = Shader.PropertyToID("_BackgroundImage");
        private static readonly int ShaderCharacterImageId = Shader.PropertyToID("_CharacterImage");

        private bool _initialized;
        private Coroutine _coroutineShowTooltip;

        public void UpdateUI(Card card)
        {
            manaText.text = card.Data.Mana.ToString();
            attackText.text = card.Data.Attack.ToString();
            heathText.text = card.Data.Health.ToString();
            
            if (_initialized)
                return;

            _initialized = true;
            
            var material = GetComponent<MeshRenderer>().material;
            material.SetTexture(ShaderBackgroundImageId, card.Template.backgroundImage);
            material.SetTexture(ShaderCharacterImageId, card.Template.characterImage);
            
            nameText.text = card.Template.name;
            descriptionText.text = card.Template.description;
        }
    }
}
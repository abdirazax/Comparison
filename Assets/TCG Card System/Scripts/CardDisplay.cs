using System;
using TMPro;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class CardDisplay : MonoBehaviour
    {
        MaterialPropertyBlock _materialPropertyBlock;
        
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
        
        SpriteRenderer _spriteRenderer;
        
        private static readonly int ShaderBackgroundImageId = Shader.PropertyToID("_BackgroundImage");
        private static readonly int ShaderFrontSideImageId = Shader.PropertyToID("_FrontSideImage");
        private static readonly int ShaderFrontSideMaskId = Shader.PropertyToID("_FrontSideMask");
        private static readonly int ShaderBackSideImageId = Shader.PropertyToID("_BackSideImage");
        private static readonly int ShaderSlotSizeId = Shader.PropertyToID("_SlotSize");
        private static readonly int ShaderFillBar = Shader.PropertyToID("_FillBar");
        private static readonly int ShaderInBattle = Shader.PropertyToID("_InBattle");
        private static readonly int ShaderAttacksPerInterval = Shader.PropertyToID("_AttacksPerInterval");
        
        
        
        
        private bool _initialized;
        private Coroutine _coroutineShowTooltip;

        public void UpdateUI(Card card)
        {
            if (!_initialized)
                InitializeUI(card);
            
            _materialPropertyBlock.SetFloat(ShaderFillBar, card.AutoAttackTimer/card.Data.AutoAttackInterval);
            
            
            _materialPropertyBlock.SetInt(ShaderInBattle, card.InBattle ? 1 : 0);
            
            _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void InitializeUI(Card card)
        {
            _initialized = true;
            
            _materialPropertyBlock = new MaterialPropertyBlock();
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            
            _spriteRenderer.size= new Vector2(card.Template.slotSize * .5f, 1);
            _materialPropertyBlock.SetTexture(ShaderBackgroundImageId, card.Template.backgroundImage);
            _spriteRenderer.sprite = card.Template.characterImage;
            _materialPropertyBlock.SetTexture(ShaderFrontSideImageId, card.Template.cardSkin.frontSprite[card.Template.slotSize - 1]);
            _materialPropertyBlock.SetTexture(ShaderFrontSideMaskId, card.Template.cardSkin.frontMaskSprite[card.Template.slotSize - 1]);
            _materialPropertyBlock.SetTexture(ShaderBackSideImageId, card.Template.cardSkin.backSprite[card.Template.slotSize - 1]);
            _materialPropertyBlock.SetFloat(ShaderSlotSizeId, card.Template.slotSize);
            _materialPropertyBlock.SetFloat(ShaderAttacksPerInterval, card.Data.AttacksPerInterval);
            
        }
    }
}
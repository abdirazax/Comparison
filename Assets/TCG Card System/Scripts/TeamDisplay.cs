using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TCG_Card_System.Scripts.EventArgs;
using TCG_Card_System.Scripts.Managers;
using TCG_Card_System.Scripts.States;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TCG_Card_System.Scripts
{
    public class TeamDisplay : MonoBehaviour
    {
        [SerializeField] public GridManager gridManager;
        [SerializeField] public Vector3Int portraitBattlePosition = new Vector3Int(0, -2, 0);
        [SerializeField] public Vector3Int portraitPreparingPosition = new Vector3Int(-10, -2, 0);
        [SerializeField] public Transform healthBarBattlePosition;
        [SerializeField] public Transform healthBarPreparingPosition;
        [SerializeField] public SpriteRenderer portraitRenderer;
        [SerializeField] public Canvas healthCanvas;
        [SerializeField] public CardBoardManager cardBoardManager;
        
        [SerializeField] public BattleStateManager battleStateManager;
        [SerializeField] public Image fillBar, fillBarTrail;
        
        
        protected readonly Dictionary<GameObject, CancellationTokenSource> _portraitAnimations = new();
        protected readonly Dictionary<Image, CancellationTokenSource> _fillBarAnimations = new();
        

        private void OnEnable()
        {
            battleStateManager.OnBattleStateChanged += OnBattleStateChanged;
            cardBoardManager.Team.OnTeamGotAttacked += OnTeamGotAttacked;
        }

        private void OnTeamGotAttacked(object sender, TeamGotAttackedEventArgs e)
        {
            SetFillBar((float) e.Team.CurrentHealth / e.Team.MaxHealth);
        }

        private void OnDisable()
        {
            battleStateManager.OnBattleStateChanged -= OnBattleStateChanged;
        }

        private void OnBattleStateChanged(object sender, BattleStateEventArgs e)
        {
            Reposition(e.BattleState);
            if (e.BattleState is BattlePreparingState)
            {
                SetFillBar(1f);
            }
        }

        private void SetFillBar(float fillAmount)
        {
            FillBarAnimationStart(fillBar, fillAmount, 5f);
            FillBarAnimationStart(fillBarTrail, fillAmount, .15f);
        }
        
        protected async UniTask FillBarAnimationStart
        (
            Image animationImage,
            float newFillAmount,
            float animationSpeed = 3f,
            float delay = 0f
        )
        {
            
            // Cancel the existing animation if there is one
            if (_fillBarAnimations.ContainsKey(animationImage))
            {
                FillBarAnimationStop(animationImage);
            }

            var animationCtSource = new CancellationTokenSource();
            _fillBarAnimations.Add(animationImage, animationCtSource);
            
            // Wait for the specified delay before starting the animation
            if (delay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: animationCtSource.Token);
            }

            try
            {
                await FillBarAnimationDefinition(animationImage, newFillAmount, animationSpeed)
                    .AttachExternalCancellation(animationCtSource.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                _fillBarAnimations.Remove(animationImage);
            }
        }
        
        protected void FillBarAnimationStop(Image animationObject)
        {
            var cancellationTokenSource = _fillBarAnimations.GetValueOrDefault(animationObject, null);
            if (cancellationTokenSource == null)
                return;
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
                
            _fillBarAnimations.Remove(animationObject);
        }
        
        protected virtual async UniTask FillBarAnimationDefinition
        (
            Image animationImage,
            float newFillAmount,
            float animationSpeed = 3f
        )
        {
            float time = 0;
            var startPosition = animationImage.transform.position;
            var startRotation = animationImage.transform.rotation;

            var startScale = animationImage.transform.localScale;

            while (time < 1)
            {
                animationImage.fillAmount = Mathf.Lerp
                (
                    animationImage.fillAmount,
                    newFillAmount,
                    time
                );
                time += Time.deltaTime * animationSpeed; // Adjust time increment for speed
                await UniTask.Yield();
            }
            animationImage.fillAmount = newFillAmount;
        }

        public void Reposition(BattleBaseState battleState)
        {
            if (battleState is BattleGoingOnState)
            {
                var portrait = portraitRenderer.GameObject();
                ObjectAnimationStart(portrait,
                    gridManager.GetWorldPosition(portraitBattlePosition),
                    portrait.transform.rotation,
                    portrait.transform.localScale);
                
                
                ObjectAnimationStart(healthCanvas.gameObject,
                    healthBarBattlePosition.position,
                    healthCanvas.gameObject.transform.rotation,
                    healthCanvas.gameObject.transform.localScale);
            }
            else if (battleState is BattlePreparingState)
            {
                var portrait = portraitRenderer.GameObject();
                ObjectAnimationStart(portraitRenderer.GameObject(),
                    gridManager.GetWorldPosition(portraitPreparingPosition),
                    portrait.transform.rotation,
                    portrait.transform.localScale);
                
                ObjectAnimationStart(healthCanvas.gameObject,
                    healthBarPreparingPosition.position,
                    healthCanvas.gameObject.transform.rotation,
                    healthCanvas.gameObject.transform.localScale);
            }
        }
        
        protected async UniTask ObjectAnimationStart
        (
            GameObject animationObject,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float animationSpeed = 3f
        )
        {
            
            // Cancel the existing animation if there is one
            if (_portraitAnimations.ContainsKey(animationObject))
            {
                ObjectAnimationStop(animationObject);
            }

            var animationCtSource = new CancellationTokenSource();
            _portraitAnimations.Add(animationObject, animationCtSource);

            try
            {
                await ObjectAnimationDefinition(animationObject, position, rotation, scale, animationSpeed)
                    .AttachExternalCancellation(animationCtSource.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                _portraitAnimations.Remove(animationObject);
            }
        }
        
        protected void ObjectAnimationStop(GameObject animationObject)
        {
            var cancellationTokenSource = _portraitAnimations.GetValueOrDefault(animationObject, null);
            if (cancellationTokenSource == null)
                return;
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
                
            _portraitAnimations.Remove(animationObject);
        }
        
        protected virtual async UniTask ObjectAnimationDefinition
        (
            GameObject animationObject,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float animationSpeed = 3f
        )
        {
            float time = 0;
            var startPosition = animationObject.transform.position;
            var startRotation = animationObject.transform.rotation;

            var startScale = animationObject.transform.localScale;

            while (time < 1)
            {
                animationObject.transform.SetPositionAndRotation
                (
                    Vector3.Lerp(startPosition, position, time),
                    Quaternion.Lerp(startRotation, rotation, time)
                );
                animationObject.transform.localScale = Vector3.Lerp(startScale, scale, time);
                time += Time.deltaTime * animationSpeed; // Adjust time increment for speed
                await UniTask.Yield();
            }

            animationObject.transform.SetPositionAndRotation
            (
                position,
                rotation
            );
            animationObject.transform.localScale = scale;
        }
    }
}
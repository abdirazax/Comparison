using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public abstract class MonoBehaviourExtended<T> : MonoBehaviour
    {
        protected readonly Dictionary<T, CancellationTokenSource> Animations = new();
        
        protected async UniTask AnimationStart
        (
            T @object,
            Func<UniTask> animationTask
        )
        {
            AnimationStop(@object);

            var animationCtSource = new CancellationTokenSource();
            Animations.Add(@object, animationCtSource);

            try
            {
                await animationTask()
                    .AttachExternalCancellation(animationCtSource.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                Animations.Remove(@object);
            }
        }

        protected void AnimationStop(T @object)
        {
            var cancellationTokenSource = Animations.GetValueOrDefault(@object, null);
            if (cancellationTokenSource == null)
                return;
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
                
            Animations.Remove(@object);
        }
        
        protected virtual void OnDestroy()
        {
            foreach (var animationPair in Animations)
                AnimationStop(animationPair.Key);
        }
    }
}
using System.Collections;
using TCG_Card_System.Scripts.Interfaces;
using UnityEngine;

namespace TCG_Card_System.Scripts
{
    public class ResizeTrigger : MonoBehaviour
    {
        private int _lastWidth;
        private int _lastHeight;
        [SerializeField]
        float intervalWhenIdle=0.2f, intervalWhenResize=0.01f;

        private float _checkInterval = 0.2f;

        private IResizeOnScreenChange[] _resizables;

        private void Awake()
        {
            _resizables = GetComponentsInChildren<IResizeOnScreenChange>(false);
        }
    

        private void Start()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            StartCoroutine(CheckForResize());
        }

        private IEnumerator CheckForResize()
        {
            while (true)
            {
                yield return new WaitForSeconds(_checkInterval);
                if (Screen.width != _lastWidth || Screen.height != _lastHeight)
                {
                    _lastWidth = Screen.width;
                    _lastHeight = Screen.height;
                    _checkInterval = intervalWhenResize;
                    OnResize();
                }
                else
                {
                    _checkInterval = intervalWhenIdle;
                }
            }
        }

        private void OnResize()
        {
            if (_resizables.Length == 0)
                return;
            foreach (var resizable in _resizables)
            {
                if (resizable != null)
                {
                    resizable.Resize();
                }
            }
        }
    }
}
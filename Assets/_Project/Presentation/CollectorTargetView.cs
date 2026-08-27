using UnityEngine;

namespace IncrementalGame.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CollectorTargetView : MonoBehaviour
    {
        private Renderer _renderer;
        private Collider2D _collider;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider2D>();
        }

        public void SetAvailable(bool available)
        {
            if (_renderer != null)
            {
                _renderer.enabled = available;
            }

            if (_collider != null)
            {
                _collider.enabled = available;
            }
        }
    }
}

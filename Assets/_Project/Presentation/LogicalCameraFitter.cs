using UnityEngine;

namespace IncrementalGame.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class LogicalCameraFitter : MonoBehaviour
    {
        private const float TargetAspect = 16f / 9f;
        private Camera _camera;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = LogicalSpace.Height / (2f * LogicalSpace.UnitsPerWorldUnit);
            ApplyViewport();
        }

        private void Update()
        {
            if (_lastWidth != Screen.width || _lastHeight != Screen.height)
            {
                ApplyViewport();
            }
        }

        public void ApplyViewport()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _camera.rect = CalculateViewport(_lastWidth, _lastHeight);
        }

        public static Rect CalculateViewport(int width, int height)
        {
            var windowAspect = Mathf.Max(1, width) / (float)Mathf.Max(1, height);

            if (windowAspect > TargetAspect)
            {
                var viewportWidth = TargetAspect / windowAspect;
                return new Rect((1f - viewportWidth) * 0.5f, 0f, viewportWidth, 1f);
            }

            var viewportHeight = windowAspect / TargetAspect;
            return new Rect(0f, (1f - viewportHeight) * 0.5f, 1f, viewportHeight);
        }
    }
}

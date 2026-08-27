using UnityEngine;

namespace IncrementalGame.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PrototypeShapeView : MonoBehaviour
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Vector2 _size = Vector2.one;

        private static Sprite _sharedSprite;

        private void Awake()
        {
            Apply();
        }

        public void Configure(Color color, Vector2 size)
        {
            _color = color;
            _size = size;
            Apply();
        }

        private void Apply()
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = _color;
            transform.localScale = _size;
        }

        private static Sprite GetSharedSprite()
        {
            if (_sharedSprite != null)
            {
                return _sharedSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Prototype White Pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);
            _sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            _sharedSprite.name = "Prototype Square";
            return _sharedSprite;
        }
    }
}

using UnityEngine;

namespace IncrementalGame.Presentation
{
    // Board coordinates stay 1600x900. Text is rasterized at the actual screen's pixel size.
    public static class RecipeUiText
    {
        public const int BodySize = 16, TitleSize = 21, SmallSize = 13, ButtonSize = 16, GoldSize = 38;
        public static Rect PixelRect(Rect logical, Matrix4x4 matrix)
        {
            var start = matrix.MultiplyPoint3x4(new Vector3(logical.xMin, logical.yMin));
            var end = matrix.MultiplyPoint3x4(new Vector3(logical.xMax, logical.yMax));
            return Rect.MinMaxRect(Mathf.Round(start.x), Mathf.Round(start.y), Mathf.Round(end.x), Mathf.Round(end.y));
        }
        public static GUIStyle PixelStyle(GUIStyle source, float scale)
        {
            var style = new GUIStyle(source) { fontSize = Mathf.RoundToInt(source.fontSize * scale) };
            style.padding = new RectOffset(Mathf.RoundToInt(source.padding.left * scale), Mathf.RoundToInt(source.padding.right * scale),
                Mathf.RoundToInt(source.padding.top * scale), Mathf.RoundToInt(source.padding.bottom * scale));
            style.contentOffset = source.contentOffset * scale;
            return style;
        }
    }
}

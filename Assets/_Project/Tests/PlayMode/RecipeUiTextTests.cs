using IncrementalGame.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace IncrementalGame.Tests.PlayMode
{
    public sealed class RecipeUiTextTests
    {
        [TestCase(1f)]
        [TestCase(1.2f)]
        [TestCase(1.525f)]
        public void JapaneseHeaderAndFooterFitAtNativePixelSizes(float scale)
        {
            var font = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "Arial" }, 16);
            try
            {
                Check(font, scale, RecipeUiText.SmallSize, "NOW", 114, 24);
                Check(font, scale, RecipeUiText.TitleSize, "通常", 114, 34);
                Check(font, scale, RecipeUiText.SmallSize, "5 / Primer 2", 114, 22);
                Check(font, scale, RecipeUiText.SmallSize, "0.3.1-recipe / 0123456789ab\nSeed 20260828", 610, 52);
                Check(font, scale, RecipeUiText.BodySize, "Primer 2\n子弾 4 発 / 開き30°", 223, 65);
            }
            finally { Object.DestroyImmediate(font); }
        }
        private static void Check(Font font, float scale, int size, string text, float width, float height)
        {
            var source = new GUIStyle { font = font, fontSize = size, wordWrap = true, padding = new RectOffset() };
            var style = RecipeUiText.PixelStyle(source, scale);
            var rect = RecipeUiText.PixelRect(new Rect(10, 10, width, height), Matrix4x4.Scale(Vector3.one * scale));
            Assert.That(style.CalcHeight(new GUIContent(text), rect.width), Is.LessThanOrEqualTo(rect.height), text);
            Assert.That(rect.x, Is.EqualTo(Mathf.Round(rect.x)));
            Assert.That(style.fontSize, Is.EqualTo(Mathf.RoundToInt(size * scale)));
        }
    }
}

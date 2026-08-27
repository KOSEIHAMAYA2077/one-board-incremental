using UnityEngine;

namespace IncrementalGame.Presentation
{
    public static class BuildMetadata
    {
        private const string DevelopmentCommit = "development";

        public static string CommitHash
        {
            get
            {
                var asset = Resources.Load<TextAsset>("build-info");
                return asset == null || string.IsNullOrWhiteSpace(asset.text)
                    ? DevelopmentCommit
                    : asset.text.Trim();
            }
        }
    }
}

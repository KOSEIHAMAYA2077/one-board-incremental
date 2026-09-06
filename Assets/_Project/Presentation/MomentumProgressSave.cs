using System;
using System.IO;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumLabController
    {
        private string _savedProgress;
        private bool _saveBlocked;
        public string ProgressWarning { get; private set; }
        public string ProgressPath => Path.Combine(Application.persistentDataPath, "challenge-arsenal-v1.json");
        private MomentumProgress LoadProgress()
        {
            if (!PersistenceEnabled) return new MomentumProgress();
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                var text = PlayerPrefs.GetString("Momentum.Web.ProgressV1", "");
                if (string.IsNullOrEmpty(text)) return new MomentumProgress();
                var progress = JsonUtility.FromJson<MomentumProgress>(text);
                if (progress == null || !progress.Valid()) throw new InvalidDataException("Invalid browser progress");
                _savedProgress = JsonUtility.ToJson(progress, true);
                return progress;
            }
            catch (Exception e)
            {
                _saveBlocked = true; ProgressWarning = "ブラウザの保存を読めません。一時Play中。";
                Debug.LogWarning("[Momentum] Browser save preserved: " + e.Message);
                return new MomentumProgress();
            }
#else
            var source=File.Exists(ProgressPath)?ProgressPath:Path.Combine(Application.persistentDataPath,"challenge-v1.json");
            if(!File.Exists(source)) return new MomentumProgress();
            try
            {
                var text = File.ReadAllText(source);
                var progress = JsonUtility.FromJson<MomentumProgress>(text);
                if (progress == null || !progress.Valid()) throw new InvalidDataException("Unsupported or invalid progress");
                _savedProgress = source==ProgressPath?JsonUtility.ToJson(progress, true):null; return progress;
            }
            catch (Exception e)
            {
                _saveBlocked = true; ProgressWarning = "保存を読めません。元Fileを保持し、一時Play中。";
                Debug.LogWarning("[Momentum] Progress preserved: " + e.Message); return new MomentumProgress();
            }
#endif
        }
        private void SaveProgress()
        {
            if (!PersistenceEnabled || _saveBlocked || Simulation == null) return;
            var json = JsonUtility.ToJson(Simulation.Progress, true);
            if (json == _savedProgress) return;
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                PlayerPrefs.SetString("Momentum.Web.ProgressV1", json);
                PlayerPrefs.Save();
#else
                var history = Path.Combine(Application.persistentDataPath, "challenge-arsenal-history"); Directory.CreateDirectory(history);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-" + Guid.NewGuid().ToString("N");
                var temp = Path.Combine(history, stamp + "-pending.json");
                File.WriteAllText(temp, json);
                if (File.Exists(ProgressPath)) File.Replace(temp, ProgressPath, Path.Combine(history, stamp + "-previous.json"));
                else File.Move(temp, ProgressPath);
#endif
                _savedProgress = json; ProgressWarning = null;
            }
            catch (Exception e)
            {
                _saveBlocked = true; ProgressWarning = "保存失敗。このSessionの進行は未保存です。";
                Debug.LogWarning("[Momentum] Progress save failed: " + e.Message);
            }
        }
        public bool RecallVolley()
        {
            var ok = Simulation.Recall();
            if (ok) { _blockedFrame = Time.frameCount + 1; SyncViews(); SaveProgress(); }
            return ok;
        }
        public bool BeginChallenge(int stage)
        {
            if (!Simulation.StartChallenge(stage)) return false;
            Simulation.SetEditing(false); _blockedFrame = Time.frameCount + 1;
            _combatFeed.Clear(); _popups.Clear(); SyncViews(); SaveProgress(); return true;
        }
    }
}

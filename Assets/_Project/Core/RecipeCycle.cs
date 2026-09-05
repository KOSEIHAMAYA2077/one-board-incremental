using System;
using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public enum RecipeBullet { Normal, Pierce, Split }

    [Serializable]
    public sealed class RecipeSnapshot
    {
        public int schemaVersion = 1;
        public RecipeBullet[] active, pending;
        public int slot, cycle;
        public bool fired;
    }

    public sealed class RecipeCycle
    {
        public const int Capacity = 4;
        private RecipeBullet[] _active = { RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Normal, RecipeBullet.Split };
        private RecipeBullet[] _pending;
        private readonly ReloadState _reload = new ReloadState(0.65);
        public IReadOnlyList<RecipeBullet> Active => Array.AsReadOnly(_active);
        public IReadOnlyList<RecipeBullet> Pending => _pending == null ? null : Array.AsReadOnly(_pending);
        public int Slot { get; private set; }
        public int CycleId { get; private set; }
        public bool HasFired { get; private set; }
        public bool Ready => _reload.IsReady;
        public double Remaining => _reload.RemainingSeconds;
        public RecipeBullet Current => _active[Slot];
        public int Primer => PrimerAt(_active, Slot);
        public static int Cost(RecipeBullet type) => type == RecipeBullet.Normal ? 0 : type == RecipeBullet.Pierce ? 2 : 3;
        public static int TotalCost(IReadOnlyList<RecipeBullet> slots)
        { var cost = 0; if (slots == null) return int.MaxValue; foreach (var type in slots) cost += Cost(type); return cost; }
        public static bool IsValid(IReadOnlyList<RecipeBullet> slots)
        {
            if (slots == null || slots.Count != 5) return false;
            foreach (var type in slots) if (type < RecipeBullet.Normal || type > RecipeBullet.Split) return false;
            return TotalCost(slots) <= Capacity;
        }
        public static int PrimerAt(IReadOnlyList<RecipeBullet> slots, int index)
        {
            if (slots[index] == RecipeBullet.Normal) return 0;
            var count = 0;
            for (var n = 1; n <= 2 && slots[(index - n + 5) % 5] == RecipeBullet.Normal; n++) count++;
            return count;
        }
        public static RecipeBullet[] Copy(IReadOnlyList<RecipeBullet> slots)
        { var result = new RecipeBullet[slots.Count]; for (var i = 0; i < result.Length; i++) result[i] = slots[i]; return result; }
        public bool Apply(IReadOnlyList<RecipeBullet> slots)
        {
            if (!IsValid(slots)) return false;
            if (!HasFired) _active = Copy(slots); else _pending = Copy(slots);
            return true;
        }
        public bool TryFire()
        { if (!_reload.TryFire()) return false; HasFired = true; return true; }
        public bool Tick(double seconds)
        {
            if (!_reload.Tick(seconds)) return false;
            Slot = (Slot + 1) % 5;
            if (Slot == 0)
            { CycleId++; if (_pending != null) { _active = _pending; _pending = null; } }
            return true;
        }
        // Save after a shot stores the next ready chamber, never resurrects a fired chamber.
        public int SavedSlot => Ready ? Slot : (Slot + 1) % 5;
        public RecipeSnapshot Capture()
        {
            var boundary = !Ready && Slot == 4;
            return new RecipeSnapshot
            {
                active = Copy(boundary && _pending != null ? _pending : _active),
                pending = boundary || _pending == null ? null : Copy(_pending),
                slot = SavedSlot, cycle = CycleId + (boundary ? 1 : 0), fired = HasFired
            };
        }
        public bool Restore(RecipeSnapshot snapshot) => snapshot != null && snapshot.schemaVersion == 1 &&
            Restore(snapshot.active, snapshot.pending, snapshot.slot, snapshot.cycle, snapshot.fired);
        public bool Restore(IReadOnlyList<RecipeBullet> active, IReadOnlyList<RecipeBullet> pending, int slot, int cycle, bool fired)
        {
            if (pending != null && pending.Count == 0) pending = null; // Unity JSON may encode null arrays as [].
            if (!IsValid(active) || (pending != null && !IsValid(pending)) || slot < 0 || slot > 4 || cycle < 0) return false;
            _active = Copy(active); _pending = pending == null ? null : Copy(pending);
            Slot = slot; CycleId = cycle; HasFired = fired; return true;
        }
    }
}

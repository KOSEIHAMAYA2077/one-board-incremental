using System;

namespace IncrementalGame.Core
{
    [Flags] public enum MomentumMod { None = 0, Power = 1, Speed = 2, Pierce = 4, Split = 8, Golden = 16, Blast = 32, Overcharge = 64 }
    public enum MomentumGun { Revolver, Uzi, Shotgun, Sniper }
    public enum MomentumChallengeState { Active, Cleared, Failed }

    // Only persistent ownership/configuration. No in-flight state or Unity dependency.
    [Serializable] public sealed class MomentumProgress
    {
        public int schemaVersion = 1, gold, highestStage, masteryMask, magazineLevel, powerLevel;
        public bool uziUnlocked;
        public int unlockedMods = 6, equippedMods = 6, gun;
        public int[] presetGuns = { 0, 0, 0 }, presetMods = { 6, 6, 6 };
        public int MagazineLimit => 3 + magazineLevel;
        public int Capacity => 20;
        public int MagazinePrice => 30 * (magazineLevel + 1);
        public int PowerPrice => 15 * (powerLevel + 1);
        public static readonly MomentumMod[] Mods = { MomentumMod.Power, MomentumMod.Speed, MomentumMod.Pierce, MomentumMod.Split, MomentumMod.Golden, MomentumMod.Blast, MomentumMod.Overcharge };
        public static int Cost(MomentumMod mod) => mod == MomentumMod.Split || mod == MomentumMod.Blast ? 6 : mod == MomentumMod.Golden ? 2 : 4;
        public static int Used(int mask) { var sum = 0; foreach (var mod in Mods) if ((mask & (int)mod) != 0) sum += Cost(mod); return sum; }
        public bool Has(MomentumMod mod) => (equippedMods & (int)mod) != 0;
        public bool Owns(MomentumMod mod) => (unlockedMods & (int)mod) != 0;
        public bool Valid()
        {
            if (schemaVersion != 1 || gold < 0 || highestStage < 0 || highestStage > 2 || masteryMask < 0 || masteryMask > 7 ||
                magazineLevel < 0 || magazineLevel > 2 || powerLevel < 0 || powerLevel > 5 || !ValidBuild(gun, equippedMods) ||
                (unlockedMods & ~127) != 0 || (unlockedMods & 6) != 6 || presetGuns == null || presetMods == null || presetGuns.Length != 3 || presetMods.Length != 3) return false;
            if (((masteryMask & 1) != 0) != Owns(MomentumMod.Golden) || ((masteryMask & 2) != 0) != Owns(MomentumMod.Blast) || ((masteryMask & 4) != 0) != Owns(MomentumMod.Overcharge)) return false;
            for (var i = 0; i < 3; i++) if (!ValidBuild(presetGuns[i], presetMods[i])) return false;
            return true;
        }
        public bool OwnsGun(int weapon) => weapon >= 0 && weapon <= 3 && (weapon != 1 || uziUnlocked);
        private bool ValidBuild(int weapon, int mask) => OwnsGun(weapon) &&
            mask >= 0 && (mask & ~unlockedMods) == 0 && Used(mask) <= Capacity;
        public bool Toggle(MomentumMod mod)
        {
            if (!Array.Exists(Mods, m => m == mod) || !Owns(mod)) return false;
            var mask = equippedMods ^ (int)mod;
            if (!ValidBuild(gun, mask)) return false;
            equippedMods = mask; return true;
        }
        public bool SelectGun(int value) { if (!ValidBuild(value, equippedMods)) return false; gun = value; return true; }
        public bool BuyMod(MomentumMod mod)
        {
            var price = mod == MomentumMod.Power ? 10 : mod == MomentumMod.Split ? 20 : -1;
            if (price < 0 || Owns(mod) || gold < price) return false;
            gold -= price; unlockedMods |= (int)mod; return true;
        }
        public bool BuyGun() { if (uziUnlocked || gold < 40) return false; gold -= 40; uziUnlocked = true; return true; }
        public bool BuyMagazine() { if (magazineLevel >= 2 || gold < MagazinePrice) return false; gold -= MagazinePrice; magazineLevel++; return true; }
        public bool BuyPower() { if (powerLevel >= 5 || gold < PowerPrice) return false; gold -= PowerPrice; powerLevel++; return true; }
        public bool StorePreset(int slot) { if (slot < 0 || slot >= 3) return false; presetGuns[slot] = gun; presetMods[slot] = equippedMods; return true; }
        public bool LoadPreset(int slot) { if (slot < 0 || slot >= 3 || !ValidBuild(presetGuns[slot], presetMods[slot])) return false; gun = presetGuns[slot]; equippedMods = presetMods[slot]; return true; }
        public void Complete(int stage, int magazines)
        {
            highestStage = Math.Max(highestStage, Math.Min(2, stage + 1));
            if (magazines != 1) return;
            masteryMask |= 1 << stage;
            unlockedMods |= (int)(stage == 0 ? MomentumMod.Golden : stage == 1 ? MomentumMod.Blast : MomentumMod.Overcharge);
        }
    }
}

using BepInEx.Configuration;
using UnityEngine;

namespace EndlessBounty
{
    /// <summary>All persisted config for EndlessBounty.</summary>
    internal sealed class BountySettings
    {
        public readonly ConfigEntry<bool> Enabled;
        public readonly ConfigEntry<KeyCode> ToggleKey;
        public readonly ConfigEntry<float> MinutesPerChest;
        public readonly ConfigEntry<float> MinutesPerCustomDrop;
        public readonly ConfigEntry<float> MinutesPerBuildingUpgrade;
        public readonly ConfigEntry<bool> ShowHud;

        public BountySettings(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Master switch: grant occasional bonus chests/items for surviving the endless post-boss final wave.");

            ToggleKey = config.Bind("General", "ToggleKey", KeyCode.F8,
                "Key to toggle EndlessBounty on/off in-match.");

            // Kill counts scale into the hundreds per minute during the final
            // wave (enemy kill rate, not player skill, dominates any
            // kills-based cadence), so rewards are paced by TIME spent
            // surviving the final wave instead - this tracks the same
            // difficulty curve the game itself uses (enemy health growth
            // over time) without spiraling at high kill rates. The game
            // already grants coins per kill on its own; this mod no longer
            // duplicates that - only occasional bonus chests/items remain.
            MinutesPerChest = config.Bind("Rewards", "MinutesPerChest", 2f,
                "A free bonus chest spawns after this many minutes survived in the final wave (timer only runs while the final wave is active). 0 disables this.");

            MinutesPerCustomDrop = config.Bind("Rewards", "MinutesPerCustomDrop", 1.5f,
                "A random item from the game's own elite drop table (e.g. the Magnet pickup) spawns after this many minutes survived in the final wave. 0 disables this.");

            MinutesPerBuildingUpgrade = config.Bind("Rewards", "MinutesPerBuildingUpgrade", 4f,
                "A free building-upgrade pickup spawns after this many minutes survived in the final wave. 0 disables this.");

            ShowHud = config.Bind("Display", "ShowHud", true,
                "Show a small HUD readout of time survived and time until the next reward, docked above the objective banner.");
        }
    }
}

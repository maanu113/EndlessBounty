using SineusModding.Api;
using UnityEngine;

namespace EndlessBounty
{
    /// <summary>
    /// Core reward logic: while the post-boss phase is active (tracked via
    /// BountyMobs' kill feed and, since Patch 1.1, MatchObjective.
    /// IsFinalBossSiege for the ~2 minute Siege Wave that now precedes the
    /// endless final wave), periodically spawns a free chest / elite-table
    /// item / building-upgrade pickup, paced by TIME survived rather than
    /// kill count - kill rate in the final wave scales into the hundreds per
    /// minute, so any kills-based cadence explodes regardless of tuning. The
    /// game already grants coins per kill on its own; this mod does not
    /// duplicate that anymore.
    /// Reward-granting itself only runs on the server/host - PlayerCoinCounter/
    /// GoldChestsManager/CustomItemDropService are server-authoritative, so
    /// non-host clients would no-op anyway. State is tracked locally on every
    /// client so the HUD stays responsive.
    /// </summary>
    internal sealed class BountyTracker
    {
        private readonly BountySettings _settings;

        public int SessionKills { get; private set; }
        public float TimeInFinalWave { get; private set; }
        public float LastKillTime { get; private set; } = -999f;
        public Vector3 LastKillPosition { get; private set; }

        private float _timeUntilNextChest;
        private float _timeUntilNextCustomDrop;
        private float _timeUntilNextBuildingUpgrade;

        public float TimeUntilNextChest => _timeUntilNextChest;
        public float TimeUntilNextCustomDrop => _timeUntilNextCustomDrop;
        public float TimeUntilNextBuildingUpgrade => _timeUntilNextBuildingUpgrade;

        public BountyTracker(BountySettings settings)
        {
            _settings = settings;
            ResetTimers();
            BountyMobs.EnsureSubscribed();
            BountyMobs.BountyUnitKilled += OnBountyUnitKilled;
        }

        private void ResetTimers()
        {
            _timeUntilNextChest = Mathf.Max(0f, _settings.MinutesPerChest.Value) * 60f;
            _timeUntilNextCustomDrop = Mathf.Max(0f, _settings.MinutesPerCustomDrop.Value) * 60f;
            _timeUntilNextBuildingUpgrade = Mathf.Max(0f, _settings.MinutesPerBuildingUpgrade.Value) * 60f;
        }

        /// <summary>
        /// Advances the survival timers while the post-boss phase is active.
        /// Since Patch 1.1, defeating the boss starts a ~2 minute Siege Wave
        /// (MatchObjective.IsFinalBossSiege) BEFORE the endless final wave
        /// begins - kills during the siege don't feed BountyMobs (that only
        /// tracks the endless-night horde), so the siege alone would never
        /// set a recent LastKillTime. Treat the siege as active too, so
        /// rewards/HUD don't go dark for those two minutes.
        /// </summary>
        public void Update()
        {
            if (!_settings.Enabled.Value)
            {
                return;
            }

            bool finalWaveActive = BountyMobs.TrackedAliveCount > 0
                || Time.time - LastKillTime < 5f
                || MatchObjective.IsFinalBossSiege;
            if (!finalWaveActive)
            {
                return;
            }

            TimeInFinalWave += Time.deltaTime;

            if (_settings.MinutesPerChest.Value > 0f)
            {
                _timeUntilNextChest -= Time.deltaTime;
                if (_timeUntilNextChest <= 0f)
                {
                    _timeUntilNextChest = _settings.MinutesPerChest.Value * 60f;
                    BountyRewards.SpawnFreeChestAt(LastKillPosition);
                }
            }

            if (_settings.MinutesPerCustomDrop.Value > 0f)
            {
                _timeUntilNextCustomDrop -= Time.deltaTime;
                if (_timeUntilNextCustomDrop <= 0f)
                {
                    _timeUntilNextCustomDrop = _settings.MinutesPerCustomDrop.Value * 60f;
                    BountyRewards.SpawnRandomCustomDropAt(LastKillPosition);
                }
            }

            if (_settings.MinutesPerBuildingUpgrade.Value > 0f)
            {
                _timeUntilNextBuildingUpgrade -= Time.deltaTime;
                if (_timeUntilNextBuildingUpgrade <= 0f)
                {
                    _timeUntilNextBuildingUpgrade = _settings.MinutesPerBuildingUpgrade.Value * 60f;
                    BountyRewards.SpawnBuildingUpgradeAt(LastKillPosition);
                }
            }
        }

        private void OnBountyUnitKilled(BountyKillInfo kill)
        {
            if (!_settings.Enabled.Value)
            {
                return;
            }

            LastKillTime = Time.time;
            LastKillPosition = kill.Position;
            SessionKills++;
        }
    }
}

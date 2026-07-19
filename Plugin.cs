using BepInEx;
using SineusModding.Api;
using UnityEngine;

namespace EndlessBounty
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.community.sineusarena.endlessbounty";
        public const string PluginName = "Endless Bounty";
        public const string PluginVersion = "1.0.0";

        private const string GithubOwner = "maanu113";
        private const string GithubRepo = "EndlessBounty";

        private BountySettings _settings;
        private BountyTracker _tracker;
        private BountyHud _hud;
        private UpdateNotice _updateNotice;

        private void Awake()
        {
            _settings = new BountySettings(Config);
            _tracker = new BountyTracker(_settings);
            _hud = new BountyHud(_settings, _tracker);
            _updateNotice = new UpdateNotice(PluginName);
            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded. Occasional bonus chests/items during the endless final wave. Press {_settings.ToggleKey.Value} to toggle.");

            UpdateChecker.CheckAsync(this, GithubOwner, GithubRepo, PluginVersion, result =>
            {
                if (result.Status == UpdateCheckStatus.UpdateAvailable)
                {
                    _updateNotice.Show(result.LatestVersion, result.ReleaseUrl);
                }
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(_settings.ToggleKey.Value))
            {
                _settings.Enabled.Value = !_settings.Enabled.Value;
                Logger.LogInfo($"Endless Bounty {(_settings.Enabled.Value ? "ENABLED" : "DISABLED")} (toggled with {_settings.ToggleKey.Value}).");
                _hud.NotifyToggled();
            }

            _tracker.Update();
        }

        private void OnGUI()
        {
            _hud.Draw();
            _updateNotice.Draw();
        }
    }
}

using SineusModding.Api;
using UnityEngine;

namespace EndlessBounty
{
    /// <summary>
    /// Small IMGUI readout of time survived in the final wave and time until
    /// the next bonus reward, docked directly above the "Find the boss's
    /// lair" objective banner (same anchoring approach as LiveStatsOverlay).
    /// Only drawn once the tracker has registered at least one kill this
    /// session, so it stays invisible until it's actually relevant.
    /// </summary>
    internal sealed class BountyHud
    {
        private const int WindowId = 0xB047B;
        private const int ToastWindowId = 0xB047C;

        private readonly BountySettings _settings;
        private readonly BountyTracker _tracker;

        private Rect _rect = new Rect(0f, 0f, 260f, 10f);
        private GUIStyle _windowStyle;
        private GUIStyle _textStyle;
        private bool _stylesReady;
        private Texture2D _panelTexture;
        private Font _uiFont;

        private float _toggleToastUntil;

        public BountyHud(BountySettings settings, BountyTracker tracker)
        {
            _settings = settings;
            _tracker = tracker;
        }

        /// <summary>Briefly shows an ON/OFF confirmation after the toggle key is pressed, so silently flipping it off (e.g. by an accidental keypress) is never invisible.</summary>
        public void NotifyToggled()
        {
            _toggleToastUntil = Time.unscaledTime + 2.5f;
        }

        public void Draw()
        {
            // Deliberately NOT gated on MatchObjective.IsMatchActive - see
            // MatchObjective.IsInMatch docs: the final wave continues well
            // past LevelQuestManager reaching QuestStep.Completed.
            if (Time.unscaledTime < _toggleToastUntil)
            {
                EnsureStyles();
                DrawToggleToast();
            }

            // SessionKills alone would keep the HUD hidden for the whole
            // Patch 1.1 Siege Wave (siege kills don't feed BountyMobs), so
            // also show once the siege itself is confirmed active.
            bool everRelevant = _tracker.SessionKills > 0 || MatchObjective.IsFinalBossSiege || _tracker.TimeInFinalWave > 0f;
            if (!_settings.Enabled.Value || !_settings.ShowHud.Value || !everRelevant)
            {
                return;
            }

            EnsureStyles();

            if (MatchObjective.TryGetObjectiveBannerScreenRect(out Rect bannerRect))
            {
                float width = Mathf.Max(200f, bannerRect.width);
                _rect = new Rect(bannerRect.xMax - width, bannerRect.y - _rect.height - 8f, width, _rect.height);
            }
            else
            {
                _rect = new Rect(Screen.width - _rect.width - 20f, 20f, _rect.width, _rect.height);
            }
            _rect = ClampToScreen(_rect);

            string chestPart = _settings.MinutesPerChest.Value > 0f
                ? $"      <color=#9FB4BA>Chest:</color> <color=#FFFFFF>{FormatSeconds(_tracker.TimeUntilNextChest)}</color>"
                : string.Empty;
            string dropPart = _settings.MinutesPerCustomDrop.Value > 0f
                ? $"      <color=#9FB4BA>Item:</color> <color=#FFFFFF>{FormatSeconds(_tracker.TimeUntilNextCustomDrop)}</color>"
                : string.Empty;
            string upgradePart = _settings.MinutesPerBuildingUpgrade.Value > 0f
                ? $"      <color=#9FB4BA>Upgrade:</color> <color=#FFFFFF>{FormatSeconds(_tracker.TimeUntilNextBuildingUpgrade)}</color>"
                : string.Empty;

            string text =
                $"<color=#E6C478><b>Endless Bounty</b></color>\n" +
                $"<color=#9FB4BA>Final wave time:</color> <color=#F0D060>{FormatSeconds(_tracker.TimeInFinalWave)}</color>      " +
                $"<color=#9FB4BA>Kills:</color> <color=#FFFFFF>{_tracker.SessionKills}</color>\n" +
                $"<color=#9FB4BA>Next -</color>{chestPart}{dropPart}{upgradePart}";

            _rect = GUILayout.Window(WindowId, _rect, _ => GUILayout.Label(text, _textStyle), string.Empty, _windowStyle, GUILayout.Width(_rect.width));
        }

        private static string FormatSeconds(float seconds)
        {
            int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
            int minutes = total / 60;
            int secs = total % 60;
            return $"{minutes}:{secs:00}";
        }

        /// <summary>
        /// A brief top-center confirmation whenever the toggle key is pressed,
        /// so turning the mod off (deliberately or by an accidental keypress)
        /// is never silent.
        /// </summary>
        private void DrawToggleToast()
        {
            string state = _settings.Enabled.Value ? "<color=#7CE07C>ENABLED</color>" : "<color=#FF6B57>DISABLED</color>";
            string text = $"<color=#E6C478><b>Endless Bounty</b></color> {state}";

            const float width = 220f;
            var rect = new Rect((Screen.width - width) / 2f, 60f, width, 10f);
            GUILayout.Window(ToastWindowId, rect, _ => GUILayout.Label(text, _textStyle), string.Empty, _windowStyle, GUILayout.Width(width));
        }

        private static Rect ClampToScreen(Rect rect)
        {
            float x = Mathf.Clamp(rect.x, 0f, Mathf.Max(0f, Screen.width - rect.width));
            float y = Mathf.Clamp(rect.y, 0f, Mathf.Max(0f, Screen.height - rect.height));
            return new Rect(x, y, rect.width, rect.height);
        }

        private void EnsureStyles()
        {
            if (_stylesReady && _panelTexture != null && _uiFont != null)
            {
                return;
            }

            _uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Segoe UI Semibold", "Arial" }, 13);
            if (_uiFont != null)
            {
                _uiFont.hideFlags = HideFlags.HideAndDontSave;
            }

            Color panelBg = new Color32(10, 26, 34, 235);
            Color panelBorder = new Color32(198, 158, 74, 255);
            Color bodyText = new Color32(224, 230, 232, 255);

            _panelTexture = MakeBorderedTexture(panelBg, panelBorder);

            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                font = _uiFont,
                border = new RectOffset(2, 2, 2, 2),
                overflow = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                normal = { background = _panelTexture },
                onNormal = { background = _panelTexture },
                hover = { background = _panelTexture },
                onHover = { background = _panelTexture },
                active = { background = _panelTexture },
                onActive = { background = _panelTexture },
                focused = { background = _panelTexture },
                onFocused = { background = _panelTexture },
                padding = new RectOffset(10, 10, 8, 8)
            };

            _textStyle = new GUIStyle(GUI.skin.label)
            {
                font = _uiFont,
                fontSize = 13,
                richText = true,
                wordWrap = true,
                normal = { textColor = bodyText }
            };

            _stylesReady = true;
        }

        private static Texture2D MakeBorderedTexture(Color fill, Color border)
        {
            const int size = 12;
            const int borderWidth = 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < borderWidth || y < borderWidth || x >= size - borderWidth || y >= size - borderWidth;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}

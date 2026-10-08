using System;
using System.IO;
using System.Collections.Generic;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildFixFont
{
    public class OuterWildFixFont : ModBehaviour
    {
        private static Font _translateFont;
        private static Font _translateFontDynamic;
        private static byte[] _fontContainer;
        private static int _shipLogFontSize = 20;
        private static readonly Dictionary<Text, ShipLogTextFont> _shipLogTextFonts =
            new Dictionary<Text, ShipLogTextFont>();
        private static readonly HashSet<Text> _shipDisplayTexts = new HashSet<Text>();
        internal static bool IsChinese { get; private set; }
        internal static int ShipLogFontSize => _shipLogFontSize;
        private static OuterWildFixFont Instance;
        private float _nextShipTextSearch;
        private readonly List<Text> _shipTexts = new List<Text>();

        public void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (!LoadFonts()) return;
            IsChinese = TextTranslation.Get().GetLanguage() == TextTranslation.Language.CHINESE_SIMPLE;
            ModHelper.HarmonyHelper.AddPostfix<UIStyleManager>("GetShipLogFont", typeof(OuterWildFixFont),
                nameof(GetShipLogFont));
            ModHelper.HarmonyHelper.AddPostfix<Text>("get_pixelsPerUnit", typeof(OuterWildFixFont),
                nameof(GetShipLogPixelsPerUnit));
            ModHelper.HarmonyHelper.AddPostfix<ShipLogFactListItem>("Start", typeof(OuterWildFixFont),
                nameof(InitializeShipLogFactFont));
            ModHelper.Console.WriteLine($"{nameof(OuterWildFixFont)} is loaded!", MessageType.Success);

            // ...这是？我忘记了！！！
            ModHelper.HarmonyHelper.AddPrefix<TextTranslation>("GetFont", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.GetFont));

            // 语言字体也走游戏原本的排版逻辑
            ModHelper.HarmonyHelper.AddPrefix<TextTranslation>("GetLanguageFont", typeof(OuterWildFixFont),
                nameof(GetLanguageFont));

            // 1.1.14 new
            ModHelper.HarmonyHelper.AddPostfix<ShipLogEntryListItem>("Setup", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.InitSetup));

            // 条目切换时缓存完整文字，渐显过程中不再重复估算贴图
            ModHelper.HarmonyHelper.AddPostfix<ShipLogFactListItem>("DisplayFact", typeof(OuterWildFixFont),
                nameof(InitializeShipLogFactFont));
            ModHelper.HarmonyHelper.AddPostfix<ShipLogFactListItem>("DisplayText", typeof(OuterWildFixFont),
                nameof(InitializeShipLogFactFont));

            // 死亡
            ModHelper.HarmonyHelper.AddPostfix<GameOverController>("SetupGameOverScreen", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.SetGameOverScreenFont));

            //飞船信号镜
            ModHelper.HarmonyHelper.AddPostfix<SignalscopeUI>("Activate", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.Activate));

            // 控制台的测试文本、模板和消息池使用同一字体与布局参数
            ModHelper.HarmonyHelper.AddPostfix<ShipNotificationDisplay>("Start", typeof(OuterWildFixFont),
                nameof(InitializeShipConsoleFont));
            ModHelper.HarmonyHelper.AddPostfix<ShipNotificationDisplay>("ExpandPool", typeof(OuterWildFixFont),
                nameof(InitializeShipConsoleFont));
        }

        private void Update()
        {
            IsChinese = TextTranslation.Get().GetLanguage() == TextTranslation.Language.CHINESE_SIMPLE;
            if (!_translateFontDynamic || !IsChinese) return;

            // 定期重新发现对象，兼容恒星际穿越后的飞船和 UI 重建
            if (Time.unscaledTime >= _nextShipTextSearch)
            {
                _nextShipTextSearch = Time.unscaledTime + 1f;
                _shipTexts.Clear();
                _shipDisplayTexts.RemoveWhere(text => !text);
                foreach (var display in Resources.FindObjectsOfTypeAll<ShipNotificationDisplay>())
                {
                    if (!display.gameObject.scene.IsValid() || !display.gameObject.scene.isLoaded) continue;
                    foreach (var text in display.GetComponentsInChildren<Text>(true))
                        if (!_shipTexts.Contains(text)) _shipTexts.Add(text);
                }
                foreach (var scope in Resources.FindObjectsOfTypeAll<SignalscopeUI>())
                {
                    if (!scope.gameObject.scene.IsValid() || !scope.gameObject.scene.isLoaded) continue;
                    if (scope._signalscopeLabel && !_shipTexts.Contains(scope._signalscopeLabel))
                        _shipTexts.Add(scope._signalscopeLabel);
                    if (scope._distanceLabel && !_shipTexts.Contains(scope._distanceLabel))
                        _shipTexts.Add(scope._distanceLabel);
                }
            }

            // 每帧只检查已缓存文本，游戏或其他 MOD 重置字体后再次应用
            for (int i = _shipTexts.Count - 1; i >= 0; i--)
            {
                if (!_shipTexts[i]) _shipTexts.RemoveAt(i);
                else
                {
                    var text = _shipTexts[i];
                    ApplyShipDisplayFont(text);

                }
            }
        }

        private static void ApplyShipDisplayFont(Text text)
        {
            if (!text || !_translateFontDynamic) return;
            bool samplingChanged = _shipDisplayTexts.Add(text);
            if (text.font != _translateFontDynamic) text.font = _translateFontDynamic;
            // Text.mainTexture 会提供动态字形贴图，UI 默认材质同时支持遮罩和裁剪
            if (text.material != Graphic.defaultGraphicMaterial)
                text.material = null;
            var style = text.GetComponent<TextStyleApplier>();
            if (style)
            {
                if (style.font != text.font) style.font = text.font;
                if (style.fixedWidth != text.fontSize) style.fixedWidth = text.fontSize;
            }
            if (samplingChanged) text.SetAllDirty();
        }

        private static void InitializeShipConsoleFont(ShipNotificationDisplay __instance)
        {
            if (!_translateFontDynamic || TextTranslation.Get().GetLanguage() != TextTranslation.Language.CHINESE_SIMPLE)
                return;

            foreach (var text in __instance.GetComponentsInChildren<Text>(true))
                ApplyShipDisplayFont(text);
        }

        public override void Configure(IModConfig config)
        {
            _shipLogFontSize = Mathf.Clamp(config.GetSettingsValue<int>("ShipLogFontSize"), 10, 20);
        }

        private bool LoadFonts()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(OuterWildFixFont).Assembly.Location),
                "Fonts", "GameFont.ttf");
            try
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Font file was not found.", path);

                var data = RuntimeFontData.BuildBundle(File.ReadAllBytes(path));
                _translateFont = LoadFileFont(data);
                _translateFontDynamic = LoadFileFont(data);
                _fontContainer = data;
                return true;
            }
            catch (Exception exception)
            {
                if (_translateFont) Destroy(_translateFont);
                if (_translateFontDynamic) Destroy(_translateFontDynamic);
                _translateFont = _translateFontDynamic = null;
                ModHelper.Console.WriteLine($"Failed to load font '{path}': {exception.Message}", MessageType.Error);
                return false;
            }
        }

        private static Font LoadFileFont(byte[] data, bool validate = true)
        {
            var bundle = AssetBundle.LoadFromMemory(data);
            if (!bundle) throw new InvalidDataException("Unity rejected the generated font container.");
            Font font = null;
            try
            {
                font = bundle.LoadAsset<Font>("assets/gamefont.ttf");
                if (!font || !font.dynamic || !font.HasCharacter('A') || !font.HasCharacter('\u4E2D') ||
                    !font.HasCharacter('\u6587'))
                    throw new InvalidDataException("The embedded TTF did not produce a dynamic font with A, 中 and 文.");
                if (validate)
                {
                    font.RequestCharactersInTexture("A\u4E2D\u6587", 48, FontStyle.Normal);
                    CharacterInfo glyph;
                    if (!font.GetCharacterInfo('\u4E2D', out glyph, 48) || !font.material || !font.material.mainTexture)
                        throw new InvalidDataException("Unity could not rasterize the embedded font's Chinese glyph.");
                }
                DontDestroyOnLoad(font);
                return font;
            }
            catch
            {
                if (font) Destroy(font);
                throw;
            }
            finally
            {
                // Release the container so a second independent atlas can be loaded.
                bundle.Unload(false);
            }
        }

        private static bool GetFont(
            bool dynamicFont,
            ref Font __result)
        {
            if (TextTranslation.Get().GetLanguage() != TextTranslation.Language.CHINESE_SIMPLE)
            {
                return true;
            }

            if (dynamicFont)
            {
                __result = _translateFontDynamic;
            }
            else
            {
                __result = _translateFont;
            }

            return false;
        }

        private static bool GetLanguageFont(ref Font __result)
        {
            return GetFont(true, ref __result);
        }

        private static void GetShipLogFont(ref Font __result)
        {
            if (_translateFont && TextTranslation.Get().GetLanguage() == TextTranslation.Language.CHINESE_SIMPLE)
                __result = _translateFont;
        }

        internal static Font CreateShipLogTextFont() => LoadFileFont(_fontContainer, false);

        internal static void RegisterShipLogTextFont(Text text, ShipLogTextFont owner)
        {
            _shipLogTextFonts[text] = owner;
        }

        internal static void UnregisterShipLogTextFont(Text text, ShipLogTextFont owner)
        {
            if (ReferenceEquals(text, null)) return;
            if (_shipLogTextFonts.TryGetValue(text, out var current) && current == owner)
                _shipLogTextFonts.Remove(text);
        }

        private static void ApplyShipLogTextFont(Text text, bool useConfiguredSize, string completeText)
        {
            if (!text) return;
            if (!_shipLogTextFonts.TryGetValue(text, out var owner) || !owner)
            {
                owner = text.GetComponent<ShipLogTextFont>();
                if (!owner) owner = text.gameObject.AddComponent<ShipLogTextFont>();
                owner.Initialize(text, useConfiguredSize);
            }
            owner.SetText(completeText);
            owner.Apply();
        }

        private static void InitializeShipLogFactFont(ShipLogFactListItem __instance)
        {
            var completeText = __instance._fact != null ? __instance._fact.GetText() : __instance._text.text;
            ApplyShipLogTextFont(__instance._text, true, completeText);
        }

        private static void GetShipLogPixelsPerUnit(Text __instance, ref float __result)
        {
            // 世界空间 Canvas 的倍率可能高达数百，动态字体会撞上 Unity 字号上限
            if (_shipDisplayTexts.Contains(__instance) && __instance.font == _translateFontDynamic)
            {
                __result = Mathf.Min(__result, 64f / Mathf.Max(1, __instance.fontSize));
                return;
            }
            if (_shipLogTextFonts.TryGetValue(__instance, out var owner) && owner && owner.Owns(__instance.font))
                __result = owner.SamplingDensity(__result);
        }

        private static void SetGameOverScreenFont(ref Text ____deathText)
        {
            ____deathText.font = TextTranslation.GetFont(false);
        }

        private static void Activate(SignalscopeUI __instance)
        {
            if (TextTranslation.Get().GetLanguage() != TextTranslation.Language.CHINESE_SIMPLE) return;
            ApplyShipDisplayFont(__instance._signalscopeLabel);
            ApplyShipDisplayFont(__instance._distanceLabel);
        }

        // 保留游戏原本的字号、布局和动画初始化，只在最后替换字体
        private static void InitSetup(ShipLogEntryListItem __instance)
        {
            ApplyShipLogTextFont(__instance._nameField, false, __instance._nameField.text);
        }
    }
}

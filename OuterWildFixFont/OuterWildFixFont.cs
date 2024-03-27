using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildFixFont
{
    public class OuterWildFixFont : ModBehaviour
    {
        private static Font _translateFont;
        private static Font _originFont;
        private static int _shipLogFontSize = 15;
        private static bool _isFontSizeSet = false;
        private GameObject ConsoleDisplay = null;
        private static OuterWildFixFont Instance;
        private bool _isConsoleTextSet = false;
        private static Font _hudFont;

        public void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadPingFang();
            ModHelper.Console.WriteLine($"My mod {nameof(OuterWildFixFont)} is loaded!", MessageType.Success);
            ModHelper.HarmonyHelper.AddPrefix<TextTranslation>("GetFont", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.GetFont));
            ModHelper.HarmonyHelper.AddPrefix<NomaiTranslatorProp>("InitializeFont", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.InitTranslatorFont));
            ModHelper.HarmonyHelper.AddPrefix<DialogueBoxVer2>("InitializeFont", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.InitTranslatorFontDialogue));
            ModHelper.HarmonyHelper.AddPrefix<FontAndLanguageController>("InitializeFont", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.InitializeFont));
            ModHelper.HarmonyHelper.AddPrefix<ShipLogEntryListItem>("Setup", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.InitSetup));
            ModHelper.HarmonyHelper.AddPrefix<ShipLogEntryDescriptionField>("Update", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.ShipLogEntryDescriptionFieldUpdate));
            ModHelper.HarmonyHelper.AddPostfix<GameOverController>("SetupGameOverScreen", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.SetGameOverScreenFont));
            ModHelper.HarmonyHelper.AddPostfix<SignalscopeUI>("Activate", typeof(OuterWildFixFont),
                nameof(OuterWildFixFont.Activate));
        }

        public override void Configure(IModConfig config)
        {
            _shipLogFontSize = config.GetSettingsValue<int>("ShipLogFontSize");
            _isFontSizeSet = false;
        }

        private void Update()
        {
            var onEnterShip = PlayerState._insideShip;

            if (!_isConsoleTextSet && onEnterShip)
            {
                ConsoleDisplay =
                    GameObject.Find(
                        "Ship_Body/Module_Cockpit/Systems_Cockpit/ShipCockpitUI/CockpitCanvases/ShipWorldSpaceUI/ConsoleDisplay/Mask/LayoutGroup");
                if (ConsoleDisplay)
                {
                    Transform consoleTextTransform = ConsoleDisplay.transform.Find("TextTemplate");
                    if (consoleTextTransform && consoleTextTransform.gameObject.activeSelf)
                    {
                        Text consoleText = consoleTextTransform.GetComponent<Text>();
                        if (consoleText)
                        {
                            consoleText.fontSize = 48;
                            consoleText.font = _originFont;
                            _isConsoleTextSet = true;
                            Instance.ModHelper.Console.WriteLine("Set ConsoleDisplay TextTemplate font size to 48",
                                MessageType.Info);
                        }
                    }

                    foreach (Transform child in ConsoleDisplay.transform)
                    {
                        if (child.name == "TextTemplate(Clone)")
                        {
                            Text consoleText = child.GetComponent<Text>();
                            if (consoleText)
                            {
                                consoleText.fontSize = 48;
                                consoleText.font = _originFont;
                                _isConsoleTextSet = true;
                                Instance.ModHelper.Console.WriteLine(
                                    "Set ConsoleDisplay TextTemplate(Clone) font size to 48", MessageType.Info);
                            }
                        }
                    }
                }
            }
        }

        private void LoadPingFang()
        {
            var path = $"{ModHelper.OwmlConfig.ModsPath}/{ModHelper.Manifest.UniqueName}/Font/pingfang";
            var ab = AssetBundle.LoadFromFile(path);
            _translateFont = ab.LoadAsset<Font>("PingFang");
            _hudFont = Resources.Load<Font>(@"fonts/chinese/urw global - nimbussanschs medium_dynamic");
            _originFont = Resources.Load<Font>(@"fonts/chinese/urw global - nimbussanschs medium");
            //URW Global - NimbusSansCHS Medium
            ab.Unload(false);
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
                __result = _translateFont;
            }
            else
            {
                __result = _translateFont;
            }

            return false;
        }

        private static bool InitTranslatorFont(
            ref Font ____fontInUse,
            ref Font ____dynamicFontInUse,
            ref float ____fontSpacingInUse,
            ref Text ____textField)
        {
            if (TextTranslation.Get().GetLanguage() != TextTranslation.Language.CHINESE_SIMPLE)
            {
                return true;
            }

            ____fontInUse = _translateFont;
            ____dynamicFontInUse = _translateFont;
            ____fontSpacingInUse = TextTranslation.GetDefaultFontSpacing();
            ____textField.font = ____fontInUse;
            ____textField.lineSpacing = ____fontSpacingInUse;
            return false;
        }

        private static bool InitTranslatorFontDialogue(DialogueBoxVer2 __instance)
        {
            if (TextTranslation.Get().IsLanguageLatin())
            {
                __instance._fontInUse = __instance._defaultDialogueFont;
                __instance._dynamicFontInUse = __instance._defaultDialogueFontDynamic;
            }
            else
            {
                __instance._fontInUse = TextTranslation.GetFont(false);
                __instance._dynamicFontInUse = TextTranslation.GetFont(true);
            }

            __instance._mainTextField.font = __instance._fontInUse;
            __instance._nameTextField.font = __instance._fontInUse;
            __instance._optionBox.GetRequiredComponent<DialogueOptionUI>().textElement.font = __instance._fontInUse;

            return false;
        }

        private static void SetGameOverScreenFont(ref Text ____deathText)
        {
            ____deathText.font = TextTranslation.GetFont(false);
        }

        private static void Activate(SignalscopeUI __instance)
        {
            __instance._signalscopeLabel.font = _originFont;
            __instance._signalscopeLabel.fontSize = 48;
            __instance._distanceLabel.font = _originFont;
            __instance._distanceLabel.fontSize = 48;
        }

        // ShipLogEntryDescriptionFieldUpdate
        private static bool ShipLogEntryDescriptionFieldUpdate(ShipLogEntryDescriptionField __instance)
        {
            if (__instance._usingGamepad != OWInput.UsingGamepad())
            {
                __instance._usingGamepad = !__instance._usingGamepad;
            }

            float num = __instance._listRoot.anchoredPosition.y;
            float num2;
            if (__instance._usingGamepad)
            {
                num2 = OWInput.GetValue(InputLibrary.scrollLogText, InputMode.All) * Time.unscaledDeltaTime * 300f;
            }
            else
            {
                num2 = OWInput.GetValue(InputLibrary.toolOptionY, InputMode.All) * Time.unscaledDeltaTime * 300f;
            }

            num -= num2;
            __instance.SetListYPos(num);
            if (!__instance._hasScrolledView)
            {
                bool flag = -__instance._listRoot.anchoredPosition.y >
                            __instance.GetListBottomPos() + __instance._thisRectTransform.rect.height;
                bool flag2 = Mathf.Abs(__instance._listRoot.anchoredPosition.y - __instance._origYPos) > 0.1f;
                __instance._scrollPromptRoot.SetActive(flag && !flag2);
                __instance.SetScrollPromptVisibility(__instance._scrollPromptRoot.activeSelf);
                if (flag2)
                {
                    __instance._hasScrolledView = true;
                }
            }

            bool flag3 = false;
            for (int i = 0; i < __instance._factListItems.Length; i++)
            {
                if (!_isFontSizeSet)
                {
                    __instance._factListItems[i]._text.fontSize = _shipLogFontSize;
                    Instance.ModHelper.Console.WriteLine($"Set _factListItems[{i}] font size to {_shipLogFontSize}",
                        MessageType.Info);
                }

                if (__instance._factListItems[i].UpdateTextReveal())
                {
                    flag3 = true;
                }
            }

            _isFontSizeSet = true;

            if (flag3 && !__instance._audioSource.isPlaying)
            {
                __instance._audioSource.Play();
            }

            if (!flag3 && __instance._audioSource.isPlaying)
            {
                __instance._audioSource.Stop();
            }

            return false;
        }


        // 1.1.14 new
        private static bool InitSetup(ShipLogEntryListItem __instance, ShipLogEntry entry, float appearDelay)
        {
            __instance._entry = entry;
            __instance.UpdateNameField();
            __instance._unreadIcon.gameObject.SetActive(false);
            __instance._hudMarkerIcon.gameObject.SetActive(false);
            __instance._moreToExploreIcon.gameObject.SetActive(false);
            __instance.gameObject.SetActive(true);
            __instance._animRoot.anchoredPosition = new Vector2(-10f, 0.0f);
            __instance._animAlpha = 0.0f;
            __instance._hasFocus = false;
            __instance._focusAlpha = 0.2f;
            __instance.UpdateAlpha();
            // __instance._uiSizeSetter.MarkReadyForInitialization();
            // __instance._uiSizeSetter.DoResizeAction(PlayerData.GetTextSize());
            __instance.AnimateTo(1f, __instance.GetEntryIndentation(), 0.05f, appearDelay);
            return false;
        }

        private static bool InitializeFont(FontAndLanguageController __instance)
        {
            Font languageFont = _translateFont;
            bool flag = TextTranslation.Get().IsLanguageLatin();
            for (int i = 0; i < __instance._textContainerList.Count; i++)
            {
                TextStyleApplier component =
                    __instance._textContainerList[i].textElement.GetComponent<TextStyleApplier>();
                if (__instance._textContainerList[i].isLanguageFont)
                {
                    if (__instance._textContainerList[i].originalFont == languageFont)
                    {
                        __instance._textContainerList[i].textElement.font = languageFont;
                        __instance._textContainerList[i].textElement.lineSpacing =
                            __instance._textContainerList[i].originalSpacing;
                        __instance._textContainerList[i].textElement.fontSize =
                            TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
                        __instance._textContainerList[i].textElement.rectTransform.localScale =
                            __instance._textContainerList[i].originalScale;
                    }
                    else
                    {
                        int modifiedFontSize = TextTranslation.GetModifiedFontSize(languageFont.fontSize);
                        __instance._textContainerList[i].textElement.font = languageFont;
                        __instance._textContainerList[i].textElement.lineSpacing =
                            TextTranslation.GetDefaultFontSpacing();
                        if (__instance._textContainerList[i].shouldScale)
                        {
                            __instance._textContainerList[i].textElement.fontSize = modifiedFontSize;
                            Vector3 vector = __instance._textContainerList[i].originalScale *
                                             ((float)__instance._textContainerList[i].originalFontSize /
                                              (float)modifiedFontSize);
                            __instance._textContainerList[i].textElement.rectTransform.localScale = vector;
                        }
                        else
                        {
                            __instance._textContainerList[i].textElement.fontSize =
                                TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
                        }

                        if (__instance._textContainerList[i].useDefaultLineSpacing)
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                TextTranslation.GetDefaultFontSpacing();
                        }
                        else
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                __instance._textContainerList[i].originalSpacing;
                        }
                    }
                }
                else if (flag)
                {
                    __instance._textContainerList[i].textElement.font = __instance._textContainerList[i].originalFont;
                    __instance._textContainerList[i].textElement.lineSpacing =
                        __instance._textContainerList[i].originalSpacing;
                    __instance._textContainerList[i].textElement.fontSize =
                        __instance._textContainerList[i].originalFontSize;
                    __instance._textContainerList[i].textElement.rectTransform.localScale =
                        __instance._textContainerList[i].originalScale;
                }
                else
                {
                    Font font = TextTranslation.GetFont(__instance._textContainerList[i].originalFont.dynamic);
                    if (__instance._textContainerList[i].originalFont == font)
                    {
                        __instance._textContainerList[i].textElement.font = _hudFont;
                        __instance._textContainerList[i].textElement.lineSpacing =
                            __instance._textContainerList[i].originalSpacing;
                        __instance._textContainerList[i].textElement.fontSize =
                            // 10;
                            TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
                        __instance._textContainerList[i].textElement.rectTransform.localScale =
                            __instance._textContainerList[i].originalScale;
                    }
                    else if (font.dynamic)
                    {
                        __instance._textContainerList[i].textElement.fontSize =
                            TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
                        __instance._textContainerList[i].textElement.rectTransform.localScale =
                            __instance._textContainerList[i].originalScale;
                        __instance._textContainerList[i].textElement.font = languageFont;
                        if (__instance._textContainerList[i].useDefaultLineSpacing)
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                TextTranslation.GetDefaultFontSpacing();
                        }
                        else
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                __instance._textContainerList[i].originalSpacing;
                        }
                    }
                    else
                    {
                        int modifiedFontSize2 = TextTranslation.GetModifiedFontSize(font.fontSize);
                        __instance._textContainerList[i].textElement.font = font;
                        __instance._textContainerList[i].textElement.lineSpacing =
                            TextTranslation.GetDefaultFontSpacing();
                        if (__instance._textContainerList[i].shouldScale)
                        {
                            __instance._textContainerList[i].textElement.fontSize = modifiedFontSize2;
                            Vector3 vector2 = __instance._textContainerList[i].originalScale *
                                              ((float)__instance._textContainerList[i].originalFontSize /
                                               (float)modifiedFontSize2);
                            __instance._textContainerList[i].textElement.rectTransform.localScale = vector2;
                        }
                        else
                        {
                            __instance._textContainerList[i].textElement.fontSize =
                                TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
                        }

                        if (__instance._textContainerList[i].useDefaultLineSpacing)
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                TextTranslation.GetDefaultFontSpacing();
                        }
                        else
                        {
                            __instance._textContainerList[i].textElement.lineSpacing =
                                __instance._textContainerList[i].originalSpacing;
                        }
                    }
                }

                if (component != null)
                {
                    component.font = __instance._textContainerList[i].textElement.font;
                    if (!TextTranslation.Get().IsLanguageLatin() &&
                        TextTranslation.Get().GetLanguage() != TextTranslation.Language.RUSSIAN &&
                        TextTranslation.Get().GetLanguage() != TextTranslation.Language.POLISH &&
                        TextTranslation.Get().GetLanguage() != TextTranslation.Language.TURKISH)
                    {
                        component.fixedWidth = (float)__instance._textContainerList[i].textElement.font.fontSize;
                    }
                    else
                    {
                        component.fixedWidth = 0f;
                    }
                }

                __instance._textContainerList[i].textElement.SetAllDirty();
            }

            return false;
        }
    }
}
using OWML.Common;
using OWML.ModHelper;
using System;
using TMPro;
using UnityEngine;
using System.IO;
using HarmonyLib;
using UnityEngine.UI;

namespace OuterWildFixFont
{
    public class OuterWildFixFont : ModBehaviour
    {
        private static OuterWildFixFont Instance;
        private static Font _hudFont;
        private static Font TranslateFont;
        private static TMP_FontAsset TMPTranslateFont;
        static string chineseTxt = null;
        private static UnityEngine.Font baseFont;

        public void LoadFont(string fontName)
        {
            try
            {
                string path = $"{ModHelper.OwmlConfig.ModsPath}/{ModHelper.Manifest.UniqueName}/Font/{fontName}";
                if (File.Exists(path))
                {
                    var ab = AssetBundle.LoadFromFile(path);
                    TranslateFont = ab.LoadAsset<Font>(fontName);
                    TMPTranslateFont = ab.LoadAsset<TMP_FontAsset>($"{fontName} SDF");
                    if (TranslateFont != null && TMPTranslateFont != null)
                    {
                    }
                    else
                    {
                        ModHelper.Console.WriteLine("$The font file is damaged. Please check the file.",
                            MessageType.Error);
                    }

                    ab.Unload(false);
                }
                else
                {
                    ModHelper.Console.WriteLine($"Font {fontName} not found, Please check the path: {path}",
                        MessageType.Error);
                }
            }
            catch (Exception e)
            {
                ModHelper.Console.WriteLine($"Load font exception:{e.Message}\n{e.StackTrace}", MessageType.Error);
            }
        }

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
            Instance = this;
        }

        private void Start()
        {
            ModHelper.Console.WriteLine($"My mod {nameof(OuterWildFixFont)} is loaded!", MessageType.Success);
            _hudFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular_Dynamic");
            LoadFont("pingfang");
            Harmony.CreateAndPatchAll(typeof(OuterWildFixFont));
            FixBrokenWord();
        }

        private void FixBrokenWord()
        {
            if (chineseTxt == null)
            {
                try
                {
                    AssetBundle words = ModHelper.Assets.LoadBundle("words");
                    TextAsset textAsset = words.LoadAsset<TextAsset>("chineseWords.txt");
                    if (textAsset != null)
                    {
                        chineseTxt = textAsset.text;
                        ModHelper.Console.WriteLine("chineseWords.txt loaded");

                        if (TranslateFont != null)
                        {
                            Texture texture = TranslateFont.material.mainTexture;
                            ModHelper.Console.WriteLine(string.Format("Old TranslateFont texture:{0}   {1}",
                                texture.width, texture.height));
                            TranslateFont.RequestCharactersInTexture(chineseTxt, 54);

                            ModHelper.Console.WriteLine(string.Format("New TranslateFont texture:{0}   {1}",
                                texture.width, texture.height));
                        }

                        if (baseFont != null)
                        {
                            Texture texture = baseFont.material.mainTexture;
                            ModHelper.Console.WriteLine(string.Format("Old baseFont texture:{0}   {1}",
                                texture.width, texture.height));
                            baseFont.RequestCharactersInTexture(chineseTxt, 54);
                            ModHelper.Console.WriteLine(string.Format("New baseFont texture:{0}   {1}",
                                texture.width, texture.height));
                        }
                    }
                    else
                        ModHelper.Console.WriteLine("chineseWords.txt not found", MessageType.Error);
                }
                catch (Exception e)
                {
                    ModHelper.Console.WriteLine(e.ToString(), MessageType.Success);
                    throw;
                }
            }
        }


        //人物对话框
        [HarmonyPrefix, HarmonyPatch(typeof(DialogueBoxVer2), "InitializeFont")]
        public static bool InitializeFontPatch(DialogueBoxVer2 __instance)
        {
            DialogueOptionUI requiredComponent =
                __instance._optionBox.GetRequiredComponent<DialogueOptionUI>();
            __instance._fontInUse = TranslateFont;
            if (TextTranslation.Get().IsLanguageLatin())
            {
                __instance._dynamicFontInUse = __instance._defaultDialogueFontDynamic;
                __instance._fontSpacingInUse = __instance._defaultFontSpacing;
            }
            else
            {
                __instance._dynamicFontInUse = TextTranslation.GetFont(true);
                __instance._fontSpacingInUse = TextTranslation.GetDefaultFontSpacing();
            }

            __instance._mainTextField.font = __instance._fontInUse;
            __instance._mainTextField.lineSpacing = __instance._fontSpacingInUse;
            __instance._nameTextField.font = __instance._fontInUse;
            __instance._nameTextField.lineSpacing = __instance._fontSpacingInUse;
            requiredComponent.textElement.font = __instance._fontInUse;
            requiredComponent._optionText.font = __instance._fontInUse;
            requiredComponent.textElement.lineSpacing = __instance._fontSpacingInUse;

            return false;
        }

        //挪麦文字翻译器
        [HarmonyPrefix, HarmonyPatch(typeof(NomaiTranslatorProp), "InitializeFont")]
        public static bool InitializeFontPatch(NomaiTranslatorProp __instance)
        {
            __instance._fontInUse = TranslateFont;
            if (TextTranslation.Get().IsLanguageLatin())
            {
                __instance._dynamicFontInUse = __instance._defaultPropFontDynamic;
                __instance._fontSpacingInUse = __instance._defaultFontSpacing;
            }
            else
            {
                __instance._dynamicFontInUse = TextTranslation.GetFont(true);
                __instance._fontSpacingInUse = TextTranslation.GetDefaultFontSpacing();
            }

            __instance._textField.font = __instance._fontInUse;
            __instance._textField.lineSpacing = __instance._fontSpacingInUse;
            return false;
        }


        // FontAndLanguageController
        [HarmonyPrefix, HarmonyPatch(typeof(FontAndLanguageController), "InitializeFont")]
        public static bool InitializeFontPatch(FontAndLanguageController __instance)
        {
            Font languageFont = _hudFont;
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
                        __instance._textContainerList[i].textElement.font = languageFont;
                        __instance._textContainerList[i].textElement.lineSpacing =
                            __instance._textContainerList[i].originalSpacing;
                        __instance._textContainerList[i].textElement.fontSize =
                            12;
                        // TextTranslation.GetModifiedFontSize(__instance._textContainerList[i].originalFontSize);
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
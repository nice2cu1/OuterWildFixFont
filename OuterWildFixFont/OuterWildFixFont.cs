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
        public static OuterWildFixFont Instance;
        private static Font TranslateFont;
        private static TMP_FontAsset TMPTranslateFont;

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
            LoadFont("pingfang");
            Harmony.CreateAndPatchAll(typeof(OuterWildFixFont));
        }

        //人物对话框
        [HarmonyPrefix, HarmonyPatch(typeof(DialogueBoxVer2), "InitializeFont")]
        public static bool InitializeFontPatch(DialogueBoxVer2 __instance)
        {
            DialogueOptionUI requiredComponent = __instance._optionBox.GetRequiredComponent<DialogueOptionUI>();
            // OuterWildFixFont.Instance.ModHelper.Console.WriteLine("####\n字体初始化被调用了:\n"+
            //                                                       "\ntext:"+ requiredComponent.textElement.text+
            //                                                       "\nname:"+requiredComponent.textElement.name+
            //                                                       "\nfont:"+requiredComponent.textElement.font+
            //                                                       "\nlineSpacing:"+requiredComponent.textElement.lineSpacing);
            //requiredComponent.textElement.font = TranslateFont;
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

         //FontAndLanguageController
         [HarmonyPrefix, HarmonyPatch(typeof(FontAndLanguageController), "InitializeFont")]
         public static bool InitializeFontPatch(FontAndLanguageController __instance)
         {
             bool flag = TextTranslation.Get().IsLanguageLatin();
             Font languageFont = TranslateFont;
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
                     __instance._textContainerList[i].textElement.font = languageFont;
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
                         __instance._textContainerList[i].textElement.font = languageFont;
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
                     component.font = languageFont;
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


        // [HarmonyPostfix, HarmonyPatch(typeof(Text), "OnEnable")]
        // public static void FontPatch(Text __instance)
        // {
        //     if (__instance != null)
        //     {
        //         if (__instance.font.name != TranslateFont.name)
        //         {
        //             OuterWildFixFont.Instance.ModHelper.Console.WriteLine("Old Font Name:" + __instance.font.name);
        //             __instance.font = TranslateFont;
        //             OuterWildFixFont.Instance.ModHelper.Console.WriteLine("Font Patch");
        //         }
        //     }
        // }
        // [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
        // public static void TMPFontPatch(TextMeshProUGUI __instance)
        // {
        //   if(__instance!=null)
        //   {
        //     if (__instance.font.name != TMPTranslateFont.name)
        //     {
        //         OuterWildFixFont.Instance.ModHelper.Console.WriteLine("Old TMP Font Name:" + __instance.font.name);
        //         __instance.font = TMPTranslateFont;
        //         OuterWildFixFont.Instance.ModHelper.Console.WriteLine("TMP Font Patch");
        //     }
        //   }
        // }
        // [HarmonyPostfix, HarmonyPatch(typeof(TextMeshProUGUI), "InternalUpdate")]
        // public static void TMPFontPatch2(TextMeshProUGUI __instance)
        // {
        //     if (__instance.font == TMPTranslateFont)
        //     {
        //         if (__instance.overflowMode != TextOverflowModes.Overflow)
        //         {
        //             if (__instance.preferredWidth > 1 && __instance.bounds.extents == Vector3.zero)
        //             {
        //                 __instance.overflowMode = TextOverflowModes.Overflow;
        //                 OuterWildFixFont.Instance.ModHelper.Console.WriteLine("TMP Font Patch2");
        //             }
        //         }
        //     }
        // }
    }
}
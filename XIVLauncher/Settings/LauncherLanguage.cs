﻿using System.Collections.Generic;

namespace XIVLauncher.Settings
{
    public enum LauncherLanguage
    {
        //Japanese,
        //English,
        //German,
        //French,
        //Italian,
        //Spanish,
        //Portuguese,
        ChineseSimplified
    }

    public static class LauncherLanguageExtensions
    {
        private static Dictionary<LauncherLanguage, string> GetLangCodes()
        {
            // MUST match LauncherLanguage enum
            return new Dictionary<LauncherLanguage, string>
            {
                //{ LauncherLanguage.Japanese, "ja" },
                //{ LauncherLanguage.English, "en" },
                //{ LauncherLanguage.German, "de" },
                //{ LauncherLanguage.French, "fr" },
                //{ LauncherLanguage.Italian, "it" },
                //{ LauncherLanguage.Spanish, "es" },
                //{ LauncherLanguage.Portuguese, "pt" },
                { LauncherLanguage.ChineseSimplified, "zh" }
            };
        }

        public static string GetLocalizationCode(this LauncherLanguage? language)
        {

            return GetLangCodes()[language ?? LauncherLanguage.ChineseSimplified]; // Default localization language
        }

        public static bool IsDefault(this LauncherLanguage? language)
        {
            return language == null || language == LauncherLanguage.ChineseSimplified;
        }

        public static LauncherLanguage GetLangFromTwoLetterISO(this LauncherLanguage? language, string code)
        {
            foreach (var langCode in GetLangCodes())
            {
                if (langCode.Value == code)
                {
                    return langCode.Key;
                }
            }
            return LauncherLanguage.ChineseSimplified; // Default language
        }
    }
}

// Code copied from GitHub: https://github.com/musiye14/Duckov_ShowDisassemblyInfo

using SodaCraft.Localizations;
using System.Collections.Generic;
using UnityEngine;

namespace WeightViewer
{
    public static class ModLocalization
    {
        public const string ConfigShowMainKey = "MOD_WEIGHTVIEWER_CONFIG_SHOW_MAIN";
        public const string ConfigShowInventoryKey = "MOD_WEIGHTVIEWER_CONFIG_SHOW_INVENTORY";

        private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> Translations = new Dictionary<SystemLanguage, Dictionary<string, string>>()
        {
            // --- 简体中文 ---
            { SystemLanguage.ChineseSimplified, new Dictionary<string, string>() {
                { ConfigShowMainKey, "在主屏幕上显示" },
                { ConfigShowInventoryKey, "在物品栏中显示" }
            }},
            // --- 繁体中文 ---
            { SystemLanguage.ChineseTraditional, new Dictionary<string, string>() {
                { ConfigShowMainKey, "在主畫面上顯示" },
                { ConfigShowInventoryKey, "在物品欄中顯示" }
            }},
            // --- 日语 ---
            { SystemLanguage.Japanese, new Dictionary<string, string>() {
                { ConfigShowMainKey, "メイン画面に表示" },
                { ConfigShowInventoryKey, "インベントリに表示" }
            }},
            // --- 韩语 ---
            { SystemLanguage.Korean, new Dictionary<string, string>() {
                { ConfigShowMainKey, "기본 화면에서 표시" },
                { ConfigShowInventoryKey, "인벤토리 화면에서 표시" }
            }},
            // --- 法语 ---
            { SystemLanguage.French, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Afficher sur l'écran principal" },
                { ConfigShowInventoryKey, "Afficher dans l'inventaire" }
            }},
            // --- 德语 ---
            { SystemLanguage.German, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Im Hauptbildschirm anzeigen" },
                { ConfigShowInventoryKey, "Im Inventar anzeigen" }
            }},
            // --- 俄语 ---
            { SystemLanguage.Russian, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Показать на главном экране" },
                { ConfigShowInventoryKey, "Показать в инвентаре" }
            }},
            // --- 西班牙语 ---
            { SystemLanguage.Spanish, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Mostrar en la pantalla principal" },
                { ConfigShowInventoryKey, "Mostrar en el inventario" }
            }},
            // --- 意大利语 ---
            { SystemLanguage.Italian, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Mostra nella schermata principale" },
                { ConfigShowInventoryKey, "Mostra nell'inventario" }
            }},
            // --- 英语 (默认) ---
            { SystemLanguage.English, new Dictionary<string, string>() {
                { ConfigShowMainKey, "Show on Main Screen" },
                { ConfigShowInventoryKey, "Show in Inventory" }
            }}
        };

        public static string GetTranslation(string key)
        {
            #region GetTranslation 
            SystemLanguage currentLanguage = SystemLanguage.English;
            try { currentLanguage = LocalizationManager.CurrentLanguage; }
            catch { Debug.LogError($"..."); currentLanguage = SystemLanguage.English; }

            if (Translations.TryGetValue(currentLanguage, out var langDict))
            {
                if (langDict.TryGetValue(key, out var translation)) return translation;
                Debug.LogWarning($"...");
            }
            else { Debug.LogWarning($"..."); }

            if (Translations.TryGetValue(SystemLanguage.English, out var englishDict) && englishDict.TryGetValue(key, out var englishTranslation))
            {
                return englishTranslation;
            }
            Debug.LogError($"...");
            return key;
            #endregion
        }
    }
}
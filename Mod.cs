using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Colossal;
using System.Linq;

namespace Ukrainian_localization_CSII
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(Ukrainian_localization_CSII)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private LocalizationManager _localizationManager;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad) + " called in phase " + updateSystem.currentPhase);

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            _localizationManager = GameManager.instance.localizationManager;

            LogManagerLocales();
            LogDbLocales();

            var ukrainianLocAsset = LoadUkrainianLocAsset(asset);

            _localizationManager.AddLocale(ukrainianLocAsset);
            _localizationManager.AddSource(ukrainianLocAsset.localeId, ukrainianLocAsset);
            
            log.Info($"Current active locale {_localizationManager.activeLocaleId}");

            _localizationManager.SetActiveLocale(ukrainianLocAsset.localeId);
            _localizationManager.ReloadActiveLocale();

            LogManagerLocales();
            LogDbLocales();
        }

        private LocaleAsset LoadUkrainianLocAsset(ExecutableAsset asset)
        {
            var ukrainianLocAsset = AssetDatabase.global.GetAssets<LocaleAsset>().FirstOrDefault(f => f.localeId == "uk-UA");

            if (ukrainianLocAsset == null)
            {
                string directoryPath = Path.GetDirectoryName(asset.path);
                string localizedPath = Path.Combine(directoryPath, "localization", "uk-UA.loc");

                ukrainianLocAsset = new LocaleAsset();
                FirstLoad(ukrainianLocAsset, localizedPath);

                log.Info(
                    $"ukrainianLocAsset data - localeId: {ukrainianLocAsset.localeId}, systemLanguage: {ukrainianLocAsset.systemLanguage}, localizedName: {ukrainianLocAsset.localizedName}");
            }

            return ukrainianLocAsset;
        }

        public void LogDbLocales()
        {
            log.Info("Existing locales in global db:");
            foreach (LocaleAsset localeAsset in AssetDatabase.global.GetAssets<LocaleAsset>())
            {
                log.Info($"{localeAsset.localeId} {localeAsset.state} {localeAsset.transient} {localeAsset.path} {localeAsset.subPath}" +
                         $"{localeAsset.guid} {localeAsset.identifier} isDirty:{localeAsset.isDirty} isDummy:{localeAsset.isDummy} isValid:{localeAsset.isValid}");
            }

        }

        private void LogManagerLocales()
        {
            var locs = _localizationManager.GetSupportedLocales();
            log.Info("Supported locales by localizationManager: " + string.Join(", ", locs));
        }

        private void FirstLoad(LocaleAsset localeAsset, string filePath)
        {
            using (var input = File.OpenRead(filePath))
            using (var binaryReader = new BinaryReader(input))
            {
                binaryReader.ReadUInt16();
                Enum.TryParse<SystemLanguage>(binaryReader.ReadString(), out var m_SystemLanguage);
                //log.Info($"SystemLang {m_SystemLanguage}");
                var language = "Ukrainian";
                Enum.TryParse(language, out m_SystemLanguage);
                string text = binaryReader.ReadString();
                var localizedName = binaryReader.ReadString();
                //log.Info($"localizedName {localizedName}");
                int num = binaryReader.ReadInt32();
                //log.Info($"num {num}");
                Dictionary<string, string> dictionary = new Dictionary<string, string>(num);
                for (int i = 0; i < num; i++)
                {
                    string key = binaryReader.ReadString();
                    string value = binaryReader.ReadString();
                    dictionary[key] = value;
                    //log.Info($"{key} {value}");
                }

                num = binaryReader.ReadInt32();
                //log.Info($"num {num}");
                Dictionary<string, int> dictionary2 = new Dictionary<string, int>(num);
                for (int j = 0; j < num; j++)
                {
                    string key2 = binaryReader.ReadString();
                    int value2 = binaryReader.ReadInt32();
                    dictionary2[key2] = value2;
                    //log.Info($"{key2} {value2}");
                }

                LocaleData data = new LocaleData(text, dictionary, dictionary2);

                localeAsset.SetData(data, m_SystemLanguage, localizedName);
                localeAsset.database = AssetDatabase.user;
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}

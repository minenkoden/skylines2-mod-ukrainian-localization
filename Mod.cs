using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System;
using System.Drawing.Printing;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Unity.Entities;
using Colossal;
using Game.UI.Localization;

namespace Ukrainian_localization_CSII
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(Ukrainian_localization_CSII)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            //log.Info($"Current mod asset sub path at {asset.subPath}");
            // Get the directory of the assetPath
            string directoryPath = Path.GetDirectoryName(asset.path);
            //log.Info($"directoryPath {directoryPath}");
            string localizedPath = Path.Combine(directoryPath, "localization", "uk-UA.loc");
            //log.Info($"localizedPath {localizedPath}");

            //GameManager.instance.localizationManager.AddLocale(ukrainianLocAsset);
            var ukrainianLocAsset = new LocaleAsset();
            Load(ukrainianLocAsset, localizedPath);
            //log.Info($"{ukrainianLocAsset.localeId}, {ukrainianLocAsset.systemLanguage}, {ukrainianLocAsset.localizedName}");
            GameManager.instance.localizationManager.AddLocale(ukrainianLocAsset);
            GameManager.instance.localizationManager.AddSource(ukrainianLocAsset.localeId, (IDictionarySource)ukrainianLocAsset);

            if (GameManager.instance.localizationManager.activeLocaleId == ukrainianLocAsset.localeId)
            {
                GameManager.instance.localizationManager.SetActiveLocale(ukrainianLocAsset.localeId);
                
                GameManager.instance.localizationManager.ReloadActiveLocale();
            }

        }


        private void Load(LocaleAsset localeAsset, string filePath)
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

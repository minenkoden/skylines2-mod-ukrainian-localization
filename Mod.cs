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
using System.Linq;
using Hash128 = Colossal.Hash128;

namespace Ukrainian_localization_CSII
{
    public class Mod : IMod
    {
        const string LOC_FILE = "Locale.cok";
        const string CURRENT_LOCALIZATION = "uk-UA";
        const string CITIES2_DATA = "Cities2_Data";


        public static ILog log = LogManager.GetLogger($"{nameof(Ukrainian_localization_CSII)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        
        
        private LocalizationManager _localizationManager;

        public void OnLoad(UpdateSystem updateSystem)
        {
            _localizationManager = GameManager.instance.localizationManager;
            
            log.Info(nameof(OnLoad) + " called in phase " + updateSystem.currentPhase + " at " + DateTime.Now);
            log.Info("Localization version: " + Colossal.Localization.Version.current.fullVersion);
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
            log.Info($"Current active locale {_localizationManager.activeLocaleId}");

            LogManagerLocales();
            LogDbLocales();

            LoadLocAsset(asset);

            
            LogManagerLocales();
            LogDbLocales();
        }

        private void LoadLocAsset(ExecutableAsset asset)
        {

            var filePaths = OverrideLocFile(asset);

            var supportedLocales = _localizationManager.GetSupportedLocales();
            if (supportedLocales.Contains(CURRENT_LOCALIZATION))
            {
                // Reload in case the last version was replaced
                _localizationManager.ReloadActiveLocale();
            }
            else
            {
                var ukrainianLocAsset = new LocaleAsset();
                FirstLoad(ukrainianLocAsset, filePaths.NewLocalizationPath);

                log.Info(
                    $"ukrainianLocAsset data - localeId: {ukrainianLocAsset.localeId}, systemLanguage: {ukrainianLocAsset.systemLanguage}, localizedName: {ukrainianLocAsset.localizedName}");

                //MakeReserveDBCopy(filePaths.LocalizationFolderPath, filePaths.StreamingAssetPath);

                var hash = AddFileToDB(filePaths.NewLocalizationPath);
                ukrainianLocAsset.id = hash;

                ukrainianLocAsset.Save();

                _localizationManager.AddLocale(ukrainianLocAsset);
                _localizationManager.AddSource(ukrainianLocAsset.localeId, ukrainianLocAsset);

                _localizationManager.SetActiveLocale(ukrainianLocAsset.localeId);
                _localizationManager.ReloadActiveLocale();

                log.Info($"Force set new locale {_localizationManager.activeLocaleId}");
            }
        }

        private void MakeReserveDBCopy(string localizationFolderPath, string backupFolderPath)
        {
            string currentDbPath = localizationFolderPath + "cache.db";
            log.Info($"Backup DB folder: {backupFolderPath}");
            string backupDbPath = backupFolderPath + $"cache_backup_UAmod_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.db";
            log.Info($"Created DB backup file: {backupDbPath}");

            File.Copy(currentDbPath, backupDbPath, true);
        }

        private FilePaths OverrideLocFile(ExecutableAsset asset)
        {
            string directoryPath = Path.GetDirectoryName(asset.path);
            string modLocalizedPath = Path.Combine(directoryPath, "localization", CURRENT_LOCALIZATION + ".loc");

            var defaultLocAsset = AssetDatabase.global.GetAssets<LocaleAsset>().FirstOrDefault(f => f.localeId == _localizationManager.fallbackLocaleId && f.path.Contains(LOC_FILE));

            log.Info($"defaultLocAsset.path {defaultLocAsset.path}, defaultLocAsset.path.IndexOf({LOC_FILE}) {defaultLocAsset.path.IndexOf(LOC_FILE)}");

            var locFolderPath = defaultLocAsset.path.Substring(0, defaultLocAsset.path.IndexOf(LOC_FILE));
            log.Info($"locFolderPath {locFolderPath}");

            var streamingAssetsPath = locFolderPath.Substring(0, locFolderPath.IndexOf(CITIES2_DATA) + CITIES2_DATA.Length) + "/StreamingAssets/";
            log.Info($"streamingAssetsPath {streamingAssetsPath}");
            Directory.CreateDirectory(streamingAssetsPath);

            string newLocalizedPath = streamingAssetsPath + CURRENT_LOCALIZATION +".loc";
            log.Info($"newLocalizedPath {newLocalizedPath}");

            File.Copy(modLocalizedPath, newLocalizedPath, true);
            return new FilePaths()
            {
                NewLocalizationPath = newLocalizedPath,
                LocalizationFolderPath = locFolderPath,
                StreamingAssetPath = streamingAssetsPath
            };
        }

        public void LogDbLocales()
        {
            log.Info("Existing locales in global db:");
            foreach (LocaleAsset localeAsset in AssetDatabase.global.GetAssets<LocaleAsset>())
            {
                log.Info($"{localeAsset.localeId} {localeAsset.state} {localeAsset.transient} {localeAsset.path} {localeAsset.subPath} " +
                         $"{localeAsset.id} {localeAsset.identifier} isDirty:{localeAsset.isDirty} isDummy:{localeAsset.isDummy} isValid:{localeAsset.isValid} {localeAsset.systemLanguage}");
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

        private Identifier AddFileToDB(string path)
        {
            log.Info("Adding file " + path);
            System.Type type;
            var assetFactory = DefaultAssetFactory.instance;
            if (!assetFactory.GetAssetType(Path.GetExtension(path), out type))
            {
                log.Info("Adding file not happens");
                return new Identifier();
            }

            log.Info($"Adding file happened! type: {type.Name}");
            var hash = AssetDatabase.user.dataSource.AddEntryFromDatabase(AssetDataPath.Create(path, EscapeStrategy.None), type, new Colossal.Hash128());
            assetFactory.CreateAndRegisterAsset<LocaleAsset>(type, hash, AssetDatabase.user);

            log.Info($"Saving DB with entry hash: {hash}");

            //AssetDatabase.game.SaveCache();
            //log.Info("Saved");
            return hash;
        }
    }

    internal class FilePaths
    {
        public string NewLocalizationPath { get; set; }
        public string StreamingAssetPath { get; set; }
        public string LocalizationFolderPath { get; set; }
    }
}

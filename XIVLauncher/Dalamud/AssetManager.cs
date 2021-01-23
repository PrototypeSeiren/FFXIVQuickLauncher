﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using XIVLauncher.Windows;

namespace XIVLauncher.Dalamud
{
    internal class AssetManager
    {
        private const string AssetStoreUrl = "https://bluefissure.gitee.io/dalamudassets/";

        private static readonly Dictionary<string, string> AssetDictionary = new()
        {
            { AssetStoreUrl + "UIRes/serveropcode.json", "UIRes/serveropcode.json" },
            { AssetStoreUrl + "UIRes/clientopcode.json", "UIRes/clientopcode.json" },
            { AssetStoreUrl + "UIRes/NotoSansCJKsc-Medium.otf", "UIRes/NotoSansCJKsc-Medium.otf" },
            { AssetStoreUrl + "UIRes/FontAwesome5FreeSolid.otf", "UIRes/FontAwesome5FreeSolid.otf" },
            { AssetStoreUrl + "UIRes/logo.png", "UIRes/logo.png" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_de.json", "UIRes/loc/dalamud/dalamud_de.json" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_es.json", "UIRes/loc/dalamud/dalamud_es.json" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_fr.json", "UIRes/loc/dalamud/dalamud_fr.json" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_it.json", "UIRes/loc/dalamud/dalamud_it.json" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_ja.json", "UIRes/loc/dalamud/dalamud_ja.json" },
            { AssetStoreUrl + "UIRes/loc/dalamud/dalamud_zh.json", "UIRes/loc/dalamud/dalamud_zh.json" },
            { "https://img.finalfantasyxiv.com/lds/pc/global/fonts/FFXIV_Lodestone_SSF.ttf", "UIRes/gamesym.ttf"}
        };

        public static bool EnsureAssets(string baseDir, DalamudLoadingOverlay overlay)
        {
            using var client = new WebClient();

            Log.Verbose("[ASSET] Starting asset download");

            var versionRes = CheckAssetRefreshNeeded(baseDir);

            foreach (var entry in AssetDictionary)
            {
                var filePath = Path.Combine(baseDir, entry.Value);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                if (!File.Exists(filePath) || versionRes.isRefreshNeeded)
                {
                    overlay.Dispatcher.Invoke(() => overlay.SetProgress(DalamudLoadingOverlay.DalamudLoadingProgress.Assets));

                    Log.Verbose("[ASSET] Downloading {0} to {1}...", entry.Key, entry.Value);
                    try
                    {
                        File.WriteAllBytes(filePath, client.DownloadData(entry.Key));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[ASSET] Could not download asset.");
                        return false;
                    }
                }
            }

            if (versionRes.isRefreshNeeded)
                SetLocalAssetVer(baseDir, versionRes.version);

            Log.Verbose("[ASSET] Assets OK");

            return true;
        }

        private static string GetAssetVerPath(string baseDir)
        {
            return Path.Combine(baseDir, "asset.ver");
        }


        /// <summary>
        ///     Check if an asset update is needed. When this fails, just return false - the route to github
        ///     might be bad, don't wanna just bail out in that case
        /// </summary>
        /// <param name="baseDir">Base directory for assets</param>
        /// <returns>Update state</returns>
        private static (bool isRefreshNeeded, int version) CheckAssetRefreshNeeded(string baseDir)
        {
            using var client = new WebClient();

            try
            {
                var localVerFile = GetAssetVerPath(baseDir);
                var localVer = 0;

                if (File.Exists(localVerFile))
                    localVer = int.Parse(File.ReadAllText(localVerFile));

                var remoteVer = int.Parse(client.DownloadString(AssetStoreUrl + "asset.ver"));

                Log.Verbose("[ASSET] Ver check - local:{0} remote:{1}", localVer, remoteVer);

                return remoteVer > localVer ? (true, remoteVer) : (false, localVer);
            }
            catch (Exception e)
            {
                Log.Error(e, "[ASSET] Could not check asset version");
                return (false, 0);
            }
        }

        private static void SetLocalAssetVer(string baseDir, int version)
        {
            try
            {
                var localVerFile = GetAssetVerPath(baseDir);
                File.WriteAllText(localVerFile, version.ToString());
            }
            catch (Exception e)
            {
                Log.Error(e, "[ASSET] Could not write local asset version");
            }
        }
    }
}

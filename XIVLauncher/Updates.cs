﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CheapLoc;
using Serilog;
using Squirrel;

namespace XIVLauncher
{
    class Updates
    {
#if !XL_NOAUTOUPDATE
        public EventHandler OnUpdateCheckFinished;
#endif

        public async Task Run(bool downloadPrerelease = false)
        {

            // GitHub requires TLS 1.2, we need to hardcode this for Windows 7
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                var stgPath = downloadPrerelease ? "stg/" : string.Empty;
                var path = $"https://goaaats.github.io/ffxiv/tools/launcher/xlupdate/{stgPath}";

                var updateManager = new UpdateManager(path, "XIVLauncher");

                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: v => updateManager.CreateShortcutForThisExe(),
                    onAppUpdate: v => updateManager.CreateShortcutForThisExe(),
                    onAppUninstall: v => updateManager.RemoveShortcutForThisExe());

                var downloadedRelease = await updateManager.UpdateApp();

                if (downloadedRelease != null)
                {
                    MessageBox.Show(Loc.Localize("UpdateNotice", "An update for XIVLauncher is available and will now be installed."),
                        "XIVLauncher Update", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    UpdateManager.RestartApp();
                }
#if !XL_NOAUTOUPDATE
                    else
                        OnUpdateCheckFinished?.Invoke(this, null);
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Update failed");
                MessageBox.Show(Loc.Localize("updatefailureerror", "XIVLauncher failed to check for updates. This may be caused by connectivity issues to GitHub. Wait a few minutes and try again.\nIf it continues to fail after several minutes, please join the discord linked on GitHub for support."),
                                "XIVLauncher",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Error);
                System.Environment.Exit(1);
            }



            // Reset security protocol after updating
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }
    }
}

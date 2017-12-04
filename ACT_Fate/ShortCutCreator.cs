using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_FATE_ACT_Plugin
{
    static class ShortCutCreator
    {
        // In order to display toasts, a desktop application must have
        // a shortcut on the Start menu.
        // Also, an AppUserModelID must be set on that shortcut.
        // The shortcut should be created as part of the installer.
        // The following code shows how to create
        // a shortcut and assign an AppUserModelID using Windows APIs.
        // You must download and include the Windows API Code Pack
        // for Microsoft .NET Framework for this code to function

        internal static bool TryCreateShortcut(string appId, string appName)
        {
            String shortcutPath = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData) +
                "\\Microsoft\\Windows\\Start Menu\\Programs\\" + appName + ".lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(appId, shortcutPath);
                return true;
            }
            return false;
        }

        static void InstallShortcut(string appId, string shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            VerifySucceeded(newShortcut.SetPath(exePath));
            VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant applicationId = new PropVariant(appId))
            {
                VerifySucceeded(newShortcutProperties.SetValue(
                    SystemProperties.System.AppUserModel.ID, applicationId));
                VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        static void VerifySucceeded(UInt32 hresult)
        {
            if (hresult <= 1)
                return;

            throw new Exception("Failed with HRESULT: " + hresult.ToString("X"));
        }
    }
}

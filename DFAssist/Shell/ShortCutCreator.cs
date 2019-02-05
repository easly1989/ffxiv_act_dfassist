using System;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;

namespace DFAssist.Shell
{
    /// <summary>
    /// In order to display toasts, a desktop application must have
    /// a shortcut on the Start menu.
    /// Also, an AppUserModelID must be set on that shortcut.
    /// The shortcut should be created as part of the installer.
    /// The following code shows how to create
    /// a shortcut and assign an AppUserModelID using Windows APIs.
    /// You must download and include the Windows API Code Pack
    /// for Microsoft .NET Framework for this code to function
    /// </summary>
    public static class ShortCutCreator
    {
        public static bool TryCreateShortcut(string appId, string appName)
        {
            var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\" + appName + ".lnk";
            if (File.Exists(shortcutPath))
                return false;

            InstallShortcut(appId, shortcutPath);
            return true;
        }

        private static void InstallShortcut(string appId, string shortcutPath)
        {
            // Find the path to the current executable
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            // ReSharper disable SuspiciousTypeConversion.Global
            if (!(new CShellLink() is IShellLinkW newShortcut))
                return;
            // ReSharper restore SuspiciousTypeConversion.Global

            // Create a shortcut to the exe
            VerifySucceeded(newShortcut.SetPath(exePath));
            VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            // ReSharper disable SuspiciousTypeConversion.Global
            var newShortcutProperties = newShortcut as IPropertyStore;
            // ReSharper restore SuspiciousTypeConversion.Global

            using (var applicationId = new PropVariant(appId))
            {
                if (newShortcutProperties != null)
                {
                    VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, applicationId));
                    VerifySucceeded(newShortcutProperties.Commit());
                }
            }

            // Commit the shortcut to disk
            // ReSharper disable SuspiciousTypeConversion.Global
            if (newShortcut is IPersistFile newShortcutSave)
                VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        private static void VerifySucceeded(UInt32 hresult)
        {
            if (hresult <= 1)
                return;

            Logger.Error("Failed with HRESULT: " + hresult.ToString("X"));
        }
    }
}

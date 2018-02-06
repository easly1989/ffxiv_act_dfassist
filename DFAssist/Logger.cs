using System;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist
{
    public static class Logger
    {
        public static RichTextBox RichTextBox;

        public static void LogSuccess(string key, params object[] args)
        {
            Log(key);
        }
    
        public static void LogInfo(string key, params object[] args)
        {
            Log(key);
        }

        public static void LogError(string key, params object[] args)
        {
            Log(key);
        }

        public static void LogException(Exception ex, string key, params object[] args)
        {
            Log(key + ":" + ex.ToString());
        }

        private static void Log(string text)
        {
            if (RichTextBox != null)
                ThreadInvokes.RichTextBoxAppendText(ActGlobals.oFormActMain, RichTextBox, DateTime.Now.ToString("O") + "|" + text + "\n");
        }
    }
}

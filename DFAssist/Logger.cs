using System;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist
{
    public static class Logger
    {
        private static RichTextBox _richTextBox;

        public static void SetLoggerTextBox(RichTextBox textBox)
        {
            _richTextBox = textBox;
        }

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
            Log(key + ":" + ex);
        }

        private static void Log(string text)
        {
            if(_richTextBox != null)
                ThreadInvokes.RichTextBoxAppendText(ActGlobals.oFormActMain, _richTextBox, DateTime.Now.ToString("O") + "|" + text + "\n");
        }
    }
}

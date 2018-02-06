using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist
{
    public static class Logger
    {
        // todo: check, never used
        //private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");

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

        // todo: check, never used
        //public static void D(object format, params object[] args)
        //{
        //    // used to Write...
        //}

        //public static void B(byte[] buffer)
        //{
        //    // used to call D...
        //}

        public static void LogException(Exception ex, string key, params object[] args)
        {
            Log(key + ":" + ex.ToString());
        }

        private static void Log(string text)
        {
            if (RichTextBox != null)
                ThreadInvokes.RichTextBoxAppendText(ActGlobals.oFormActMain, RichTextBox, DateTime.Now.ToString("O") + "|" + text + "\n");
        }

        // todo: check, never used
        //private static string Escape(string line)
        //{
        //    return EscapePattern.Replace(line, "{{$1}}");
        //}
    }
}

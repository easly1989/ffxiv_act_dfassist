using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist
{
    public static class Logger
    {
        private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");

        private static RichTextBox _richTextBox;

        public static void SetTextBox(RichTextBox box)
        {
            _richTextBox = box;
        }

        private static void Write(Color color, object format, params object[] args)
        {
            if (_richTextBox == null)
                return;

            var formatted = format ?? "(null)";
            try
            {
                formatted = string.Format(formatted.ToString(), args);
            }
            catch (FormatException)
            {
                // do nothing
            }

            var datetime = DateTime.Now.ToString("HH:mm:ss");
            var message = $"[{datetime}] {formatted}{Environment.NewLine}";

            ActGlobals.oFormActMain.Invoke(new Action(() =>
            {
                _richTextBox.SelectionStart = _richTextBox.TextLength;
                _richTextBox.SelectionLength = 0;
                _richTextBox.SelectionColor = color;
                _richTextBox.AppendText(message);
                _richTextBox.SelectionColor = _richTextBox.ForeColor;
            }));
        }

        public static void Success(string key, params object[] args)
        {
            Write(Color.Green, Localization.GetText(key, args));
        }

        public static void Info(string key, params object[] args)
        {
            Write(Color.Black, Localization.GetText(key, args));
        }

        public static void Error(string key, params object[] args)
        {
            Write(Color.Red, Localization.GetText(key, args));
        }

        public static void Exception(Exception ex, string key, params object[] args)
        {
#if DEBUG
            throw ex;
#else
            var format = Localization.GetText(key);
            var message = ex.Message;

            message = Escape(message);
            Error($"{format}: {message}", args);
#endif
        }

        public static void Debug(object format, params object[] args)
        {
#if DEBUG
            Write(Color.Gray, format, args);
#endif
        }

        public static void Buffer(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();

            for (var i = 0; i < buffer.Length; i++)
            {
                if (i != 0)
                {
                    if (i % 16 == 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    else if (i % 8 == 0)
                    {
                        stringBuilder.Append(' ', 2);
                    }
                    else
                    {
                        stringBuilder.Append(' ');
                    }
                }

                stringBuilder.Append(buffer[i].ToString("X2"));
            }

            Debug(stringBuilder.ToString());
        }

        private static string Escape(string line)
        {
            return EscapePattern.Replace(line, "{{$1}}");
        }
    }
}

using Advanced_Combat_Tracker;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

internal static class Log
{
    private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");

    static public RichTextBox richTextBox1;

    private static void Write(object format, params object[] args)
    {
        /*
        var formatted = format ?? "(null)";
        try
        {
            formatted = string.Format(formatted.ToString(), args);
        }
        catch (FormatException) { }

        var datetime = DateTime.Now.ToString("HH:mm:ss");
        var message = $"[{datetime}] {formatted}{Environment.NewLine}";

        Form.Invoke(() =>
        {
            Form.richTextBox_Log.SelectionStart = Form.richTextBox_Log.TextLength;
            Form.richTextBox_Log.SelectionLength = 0;

            Form.richTextBox_Log.SelectionColor = color;
            Form.richTextBox_Log.AppendText(message);
            Form.richTextBox_Log.SelectionColor = Form.richTextBox_Log.ForeColor;
        });
        */
    }

    internal static void S(string key, params object[] args)
    {
        //Write(Localization.GetText(key, args));
        logOut(key);
    }
    
    internal static void I(string key, params object[] args)
    {
        //Write(Localization.GetText(key, args));
        logOut(key);
    }

    internal static void E(string key, params object[] args)
    {
        //Write(Localization.GetText(key, args));
        
        logOut(key);
    }

    internal static void Ex(Exception ex, string key, params object[] args)

    {
        /*
#if DEBUG
            throw ex;
#else
        var format = Localization.GetText(key);

        var message = ex.Message;

        message = Escape(message);
        E($"{format}: {message}", args);

        Sentry.ReportAsync(ex, new { LogMessage = string.Format(format.ToString(), args) });
#endif
*/
        logOut(key + ":" + ex.ToString());
    }

    internal static void logOut(string text)
    {
        if (richTextBox1 != null) ThreadInvokes.RichTextBoxAppendText(ActGlobals.oFormActMain, richTextBox1, DateTime.Now.ToString("O") + "|" + text + "\n");
        //ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text + "|");
    }

    internal static void D(object format, params object[] args)
    {
#if DEBUG
            Write(System.Drawing.Color.Gray, format, args);
#endif
    }

    internal static void B(byte[] buffer)
    {
        var sb = new StringBuilder();
        sb.AppendLine();

        for (var i = 0; i < buffer.Length; i++)
        {
            if (i != 0)
            {
                if (i % 16 == 0)
                {
                    sb.AppendLine();
                }
                else if (i % 8 == 0)
                {
                    sb.Append(' ', 2);
                }
                else
                {
                    sb.Append(' ');
                }
            }

            sb.Append(buffer[i].ToString("X2"));
        }

        D(sb.ToString());
    }

    private static string Escape(string line)
    {
        return EscapePattern.Replace(line, "{{$1}}");
    }
}

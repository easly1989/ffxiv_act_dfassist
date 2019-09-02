using System;
using System.Windows.Forms;
using Splat;

namespace DFAssist
{
    public interface IActLogger : ILogger, IDisposable
    {
        void SetLoggingLevel(LogLevel level);
        void SetTextBox(RichTextBox box);
    }
}
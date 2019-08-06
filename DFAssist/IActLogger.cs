using System.Windows.Forms;
using Splat;

namespace DFAssist
{
    public interface IActLogger : ILogger
    {
        void SetLoggingLevel(LogLevel level);
        void SetTextBox(RichTextBox box);
    }
}
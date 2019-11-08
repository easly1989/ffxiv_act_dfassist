using System.Runtime.InteropServices;
using DFAssist.Core.Toast.Base;
using Splat;

namespace DFAssist.Core.Toast
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("67d5ccfb-c77f-4a77-a37d-ecce57279150"), ComVisible(true)]
    public class ToastNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            var logger = Locator.Current.GetService<ILogger>();
            logger.Write("UI: Toast Activated...", LogLevel.Debug);
        }
    }
}
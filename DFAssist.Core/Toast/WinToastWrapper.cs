using System.Runtime.InteropServices;

namespace DFAssist.Core.Toast
{
    public enum Duration
    {
        System = 0, 
        Short, 
        Long
    };

    public enum AudioOption
    {
        Default = 0, 
        Silent, 
        Loop
    };

    public enum AudioSystemFile {
        DefaultSound = 0,
        IM, 
        Mail,
        Reminder, 
        SMS, 
        Alarm,
        Alarm2,
        Alarm3,
        Alarm4,
        Alarm5,
        Alarm6,
        Alarm7,
        Alarm8,
        Alarm9,
        Alarm10,
        Call,
        Call1,
        Call2,
        Call3,
        Call4,
        Call5,
        Call6,
        Call7,
        Call8,
        Call9,
        Call10,
    };

    public static class WinToastWrapper
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ToastEventCallback(int messageCode);

        [DllImport("DFAssist.WinToast.dll")]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            [MarshalAs(UnmanagedType.I4)]Duration duration = 0,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = 0,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = 0);

    }
}

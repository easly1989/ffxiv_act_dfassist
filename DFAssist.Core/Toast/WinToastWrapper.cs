using System.Runtime.InteropServices;

namespace DFAssist.Core.Toast
{
    public enum Duration
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
        System = 0, 
        Short, 
        Long
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    };

    public enum AudioOption
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
        Default = 0, 
        Silent, 
        Loop
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    };

    public enum AudioSystemFile 
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Global
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
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
    };

    public static class WinToastWrapper
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ToastEventCallback(int messageCode);

        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_Text01", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);

        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_Text02", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastTitle,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            bool wrapFirstLine = true,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);

        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_Text03", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastTitle,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.LPWStr)]string toastAdditionalMessage,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);
        
        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_ImageAndText01", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.LPWStr)]string toastImagePath,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);

        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_ImageAndText02", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastTitle,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.LPWStr)]string toastImagePath,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            bool wrapFirstLine = true,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);

        [DllImport("libs/DFAssist.WinToast.dll", EntryPoint = "CreateToast_ImageAndText03", ExactSpelling = true)]
        public static extern void CreateToast(
            [MarshalAs(UnmanagedType.LPWStr)]string appName,
            [MarshalAs(UnmanagedType.LPWStr)]string appUserModelId,
            [MarshalAs(UnmanagedType.LPWStr)]string toastTitle,
            [MarshalAs(UnmanagedType.LPWStr)]string toastMessage,
            [MarshalAs(UnmanagedType.LPWStr)]string toastAdditionalMessage,
            [MarshalAs(UnmanagedType.LPWStr)]string toastImagePath,
            [MarshalAs(UnmanagedType.FunctionPtr)]ToastEventCallback eventCallback,
            [MarshalAs(UnmanagedType.LPWStr)]string attribution = null,
            [MarshalAs(UnmanagedType.I4)]Duration duration = Duration.System,
            [MarshalAs(UnmanagedType.I4)]AudioSystemFile audioFile = AudioSystemFile.DefaultSound,
            [MarshalAs(UnmanagedType.I4)]AudioOption audioOption = AudioOption.Default);

    }
}

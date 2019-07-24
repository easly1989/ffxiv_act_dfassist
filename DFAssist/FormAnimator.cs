using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DFAssist
{
    public sealed class FormAnimator
    {
        [Flags]
        public enum AnimationDirection
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedMember.Global
            Right = 1,
            Left = 2,
            Down = 4,
            Up = 8
            // ReSharper restore UnusedMember.Global
            // ReSharper restore InconsistentNaming
        }

        [Flags]
        public enum AnimationMethod
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedMember.Global
            Roll = 0,
            Center = 16, // 0x00000010
            Slide = 262144, // 0x00040000
            Fade = 524288 // 0x00080000
            // ReSharper restore UnusedMember.Global
            // ReSharper restore InconsistentNaming
        }

        // ReSharper disable UnusedMember.Local
        private const int AwHide = 65536;
        private const int AwActivate = 131072;
        private const int DefaultDuration = 250;
        // ReSharper restore UnusedMember.Local

        public FormAnimator(Form form)
        {
            Form = form;
            Form.Load += Form_Load;
            Form.VisibleChanged += Form_VisibleChanged;
            Form.Closing += Form_Closing;
            Duration = 250;
        }

        public FormAnimator(Form form, AnimationMethod method, int duration)
            : this(form)
        {
            Method = method;
            Duration = duration;
        }

        public FormAnimator(
            Form form,
            AnimationMethod method,
            AnimationDirection direction,
            int duration)
            : this(form, method, duration)
        {
            Direction = direction;
        }

        public AnimationMethod Method { get; set; }
        public AnimationDirection Direction { get; set; }
        public int Duration { get; set; }
        public Form Form { get; }

        private void Form_Load(object sender, EventArgs e)
        {
            if (Form.MdiParent != null && Method == AnimationMethod.Fade)
                return;
            
            NativeMethods.AnimateWindow(Form.Handle, Duration, (int)((AnimationMethod)131072 | Method | (AnimationMethod)Direction));
        }

        private void Form_VisibleChanged(object sender, EventArgs e)
        {
            if (Form.MdiParent != null)
                return;
            
            var num = (int)(Method | (AnimationMethod)Direction);
            NativeMethods.AnimateWindow(Form.Handle, Duration, !Form.Visible ? num | 65536 : num | 131072);
        }

        private void Form_Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel || Form.MdiParent != null && Method == AnimationMethod.Fade)
                return;

            NativeMethods.AnimateWindow(Form.Handle, Duration,(int)((AnimationMethod)65536 | Method | (AnimationMethod)Direction));
        }
    }
}
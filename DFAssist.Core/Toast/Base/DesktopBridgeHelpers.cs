// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DFAssist.Core.Toast.Base
{
    /// <summary>
    /// Code from https://github.com/qmatteoq/DesktopBridgeHelpers/edit/master/DesktopBridge.Helpers/Helpers.cs
    /// </summary>
    public static class DesktopBridgeHelpers
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable ArrangeTypeMemberModifiers
        const long APPMODEL_ERROR_NO_PACKAGE = 15700L;
        // ReSharper restore ArrangeTypeMemberModifiers
        // ReSharper restore InconsistentNaming

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

        private static bool? _isRunningAsUwp;
        public static bool IsRunningAsUwp()
        {
            if (_isRunningAsUwp != null) 
                return _isRunningAsUwp.Value;

            if (IsWindows7OrLower)
            {
                _isRunningAsUwp = false;
            }
            else
            {
                var length = 0;
                var sb = new StringBuilder(0);
                // ReSharper disable RedundantAssignment
                var result = GetCurrentPackageFullName(ref length, sb);
                // ReSharper restore RedundantAssignment
                sb = new StringBuilder(length);
                result = GetCurrentPackageFullName(ref length, sb);

                _isRunningAsUwp = result != APPMODEL_ERROR_NO_PACKAGE;
            }

            return _isRunningAsUwp.Value;
        }

        private static bool IsWindows7OrLower
        {
            get
            {
                var versionMajor = Environment.OSVersion.Version.Major;
                var versionMinor = Environment.OSVersion.Version.Minor;
                var version = versionMajor + (double)versionMinor / 10;
                return version <= 6.1;
            }
        }
    }
}
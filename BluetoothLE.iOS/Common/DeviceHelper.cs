using System;
using System.Runtime.InteropServices;
using UIKit;

namespace BluetoothLE.iOS.Common
{
    public class DeviceHelper
    {
        private const string HardwareProperty = "hw.machine";

        [DllImport(ObjCRuntime.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static DeviceInfo GetDeviceInfo()
        {
            var pLen = Marshal.AllocHGlobal(sizeof(int));
            sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

            var length = Marshal.ReadInt32(pLen);

            var pStr = Marshal.AllocHGlobal(length);
            sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

            var hardwareStr = Marshal.PtrToStringAnsi(pStr);

            var ret = DeviceModelTypes.Unknown;
            switch (hardwareStr)
            {
                case "iPhone1,1":
                    ret = DeviceModelTypes.iPhone;
                    break;
                case "iPhone1,2":
                    ret = DeviceModelTypes.iPhone3G;
                    break;
                case "iPhone2,1":
                    ret = DeviceModelTypes.iPhone3GS;
                    break;
                case "iPhone3,1":
                    ret = DeviceModelTypes.iPhone4;
                    break;
                case "iPhone3,3":
                    ret = DeviceModelTypes.VerizoniPhone4;
                    break;
                case "iPhone4,1":
                    ret = DeviceModelTypes.iPhone4S;
                    break;
                case "iPhone 5,1":
                case "iPhone 5,2":
                    ret = DeviceModelTypes.iPhone5;
                    break;
                case "iPhone5,3":
                case "iPhone5,4":
                    ret = DeviceModelTypes.iPhone5C;
                    break;
                case "iPhone6,1":
                case "iPhone6,2":
                    ret = DeviceModelTypes.iPhone5S;
                    break;
                case "iPhone7,2":
                    ret = DeviceModelTypes.iPhone6;
                    break;
                case "iPhone7,1":
                    ret = DeviceModelTypes.iPhone6Plus;
                    break;
                case "iPhone8,1":
                    ret = DeviceModelTypes.iPhone6S;
                    break;
                case "iPhone8,2":
                    ret = DeviceModelTypes.iPhone6SPlus;
                    break;
                case "iPad1,1":
                case "iPad1,2":
                    ret = DeviceModelTypes.iPad;
                    break;
                case "iPad2,1":
                case "iPad2,2":
                case "iPad2,3":
                case "iPad2,4":
                    ret = DeviceModelTypes.iPad2;
                    break;
                case "iPad3,1":
                case "iPad3,2":
                case "iPad3,3":
                    ret = DeviceModelTypes.iPad3;
                    break;
                case "iPad2,5":
                case "iPad2,6":
                case "iPad2,7":
                    ret = DeviceModelTypes.iPadMini;
                    break;
                case "iPad3,4":
                case "iPad3,5":
                case "iPad3,6":
                    ret = DeviceModelTypes.iPad4;
                    break;
                case "iPad4,1":
                case "iPad4,2":
                    ret = DeviceModelTypes.iPadAir;
                    break;
                case "iPad4,4":
                case "iPad4,5":
                case "iPad4,6":
                    ret = DeviceModelTypes.iPadMini2;
                    break;
                case "iPad4,7":
                case "iPad4,8":
                case "iPad4,9":
                    ret = DeviceModelTypes.iPadMini3;
                    break;
                case "iPad5,1":
                case "iPad5,2":
                    ret = DeviceModelTypes.iPadMini4;
                    break;
                case "iPad5,3":
                case "iPad5,4":
                    ret = DeviceModelTypes.iPadAir2;
                    break;
                case "iPad6,3":
                case "iPad6,4":
                    ret = DeviceModelTypes.iPadPro9_7;
                    break;
                case "iPad6,7":
                case "iPad6,8":
                    ret = DeviceModelTypes.iPadPro12_9;
                    break;
                case "iPod1,1":
                    ret = DeviceModelTypes.iPod1G;
                    break;
                case "iPod2,1":
                    ret = DeviceModelTypes.iPod2G;
                    break;
                case "iPod3,1":
                    ret = DeviceModelTypes.iPod3G;
                    break;
                case "iPod4,1":
                    ret = DeviceModelTypes.iPod4G;
                    break;
                case "iPod5,1":
                    ret = DeviceModelTypes.iPod5G;
                    break;
                case "AppleTV2,1":
                    ret = DeviceModelTypes.AppleTv2;
                    break;
                case "AppleTV3,1":
                case "AppleTV3,2":
                    ret = DeviceModelTypes.AppleTv3;
                    break;
                case "i386":
                case "x86_64":
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                    {
                        ret = UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale == 960 || UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale == 960 ? DeviceModelTypes.iPhone4Simulator : DeviceModelTypes.iPhoneSimulator;
                    }
                    else
                    {
                        ret = DeviceModelTypes.iPadSimulator;
                    }
                    break;
            }

            return new DeviceInfo()
            {
                RawModelString = hardwareStr,
                Model = ret
            };
        }
    }

    public class DeviceInfo
    {
        public DeviceModelTypes Model { get; set; }
        public string RawModelString { get; set; }
    }

    public enum DeviceModelTypes
    {
        iPhone,
        iPhone3G,
        iPhone3GS,
        iPhone4,
        VerizoniPhone4,
        iPhone4S,
        iPhone5,
        iPhone5C,
        iPhone5S,
        iPhone6,
        iPhone6Plus,
        iPhone6S,
        iPhone6SPlus,
        iPod1G,
        iPod2G,
        iPod3G,
        iPod4G,
        iPod5G,
        iPad,
        iPad2,
        iPadMini,
        iPad3,
        iPad4,
        iPadAir,
        iPadMini2,
        iPadMini3,
        iPadMini4,
        iPadAir2,
        iPadPro9_7,
        iPadPro12_9,
        AppleTv2,
        AppleTv3,
        iPhoneSimulator,
        iPhone4Simulator,
        iPadSimulator,
        Unknown
    }
}
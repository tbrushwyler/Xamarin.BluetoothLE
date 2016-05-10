using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace BLE.Dev {
	[StructLayout(LayoutKind.Sequential)]
	public struct AdvertisePacket {
		public byte[] Data;

		public AdvertisePacket(int gatewayState, long timeStamp) {
			Data = new byte[] {
				0xB0, 0x0B,
				0xB0, 0x0B,
				0xB0, 0x0B,
				0xB0, 0x0B
			};
		}

		public byte[] ToBytes() {
			byte[] arr = null;
			IntPtr ptr = IntPtr.Zero;
			try {
				int size = Marshal.SizeOf(this);
				arr = new byte[size];
				ptr = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(this, ptr, true);
				Marshal.Copy(ptr, arr, 0, size);
			} catch (Exception e) {
				throw new Exception("Error converting to bytes", e);
			} finally {
				Marshal.FreeHGlobal(ptr);
			}
			if (arr.Length > 29) {
				throw new Exception("Data cannot exceed 29 bytes");
			}
			return arr;
		}
	}
}

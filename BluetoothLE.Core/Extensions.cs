using System;
using System.Linq;

namespace BluetoothLE.Core
{
	/// <summary>
	/// Common Bluetooth extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Write the specified data to a device.
		/// </summary>
		/// <param name="device">The device to write the data to.</param>
		/// <param name="serviceUuid">The service to write the data to.</param>
		/// <param name="characteristicUuid">The characteristic to write the data to.</param>
		/// <param name="data">The data to write to the device.</param>
		public static void Write(this IDevice device, string serviceUuid, string characteristicUuid, byte[] data)
		{
			if (device.Services == null || !device.Services.Any())
				throw new Exception("Device does not have any services");

			var serviceGuid = serviceUuid.ToGuid();
			var service = device.Services.FirstOrDefault(x => x.Id == serviceGuid);
			if (service == null)
				throw new Exception("Device does not have specified service");

			if (service.Characteristics == null || !service.Characteristics.Any())
				throw new Exception("Specified service does not have any characteristics");

			var characteristicGuid = characteristicUuid.ToGuid();
			var characteristic = service.Characteristics.FirstOrDefault(x => x.Id == characteristicGuid);
			if (characteristic == null)
				throw new Exception("Specified service does not have the specified characteristic");

			characteristic.Write(data);
		}

		private const string IdFormat = "{0}{1}-0000-1000-8000-00805f9b34fb";

		/// <summary>
		/// Convert a UUID to a Guid by using it as the first part of the Guid {0}-0000-1000-8000-00805f9b34fb
		/// </summary>
		/// <returns>The Guid representing the supplied UUID.</returns>
		/// <param name="uuid">The UUID to convert.
		/// If the string is 4 characters, "0000" will be appended before creating the Guid.
		/// If the string is 8 characters, nothing is appended before creating the Guid.
		/// If the string is not 4 characters or 8 characters, the exact Guid is parsed from the input.
		/// </param>
		public static Guid ToGuid(this string uuid)
		{
			if (uuid.Length == 4) {
				// 4 character prefix
				uuid = string.Format(IdFormat, "0000", uuid);
			} else if (uuid.Length == 8) {
				// no prefix required
				uuid = string.Format(IdFormat, uuid, "");
			}
			return Guid.ParseExact (uuid, "d");
		}
	}
}


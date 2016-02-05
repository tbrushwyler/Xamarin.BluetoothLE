using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Device discovered event arguments.
	/// </summary>
	public class DeviceDiscoveredEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>The device.</value>
		public IDevice Device { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Core.Events.DeviceDiscoveredEventArgs"/> class.
		/// </summary>
		/// <param name="device">The device that was discovered.</param>
		public DeviceDiscoveredEventArgs(IDevice device)
		{
			Device = device;
		}
	}
}


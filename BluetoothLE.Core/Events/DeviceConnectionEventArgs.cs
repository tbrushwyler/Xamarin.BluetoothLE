using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Device connection event arguments.
	/// </summary>
	public class DeviceConnectionEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the device
		/// </summary>
		/// <value>The device.</value>
		public IDevice Device { get; set; }

		/// <summary>
		/// Gets or sets the error message
		/// </summary>
		/// <value>The error message, or null if there was no error</value>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Core.Events.DeviceConnectionEventArgs"/> class.
		/// </summary>
		/// <param name="device">The device that was connected/disconnected.</param>
		public DeviceConnectionEventArgs(IDevice device)
		{
			Device = device;
		}
	}
}


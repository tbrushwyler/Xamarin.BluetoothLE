using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Service discovered event arguments.
	/// </summary>
	public class ServiceDiscoveredEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the service.
		/// </summary>
		/// <value>The service.</value>
		public IService Service { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Core.Events.ServiceDiscoveredEventArgs"/> class.
		/// </summary>
		/// <param name="service">The service that was discovered.</param>
		public ServiceDiscoveredEventArgs(IService service)
		{
			Service = service;
		}
	}
}


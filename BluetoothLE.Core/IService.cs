using System;
using System.Collections.Generic;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Core
{
	/// <summary>
	/// The service interface.
	/// </summary>
	public interface IService
	{
		/// <summary>
		/// Occurs when characteristics discovered.
		/// </summary>
		event EventHandler<CharacteristicDiscoveredEventArgs> CharacteristicDiscovered;

		/// <summary>
		/// Gets the service's unique identifier.
		/// </summary>
		/// <value>The identifier.</value>
		Guid Id { get; }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		string Uuid { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is primary.
		/// </summary>
		/// <value><c>true</c> if this instance is primary; otherwise, <c>false</c>.</value>
		bool IsPrimary { get; }

		/// <summary>
		/// Gets the service's characteristics.
		/// </summary>
		/// <value>The characteristics.</value>
		IList<ICharacteristic> Characteristics { get; }

		/// <summary>
		/// Discovers the characteristics for the services.
		/// </summary>
		void DiscoverCharacteristics();
	}
}


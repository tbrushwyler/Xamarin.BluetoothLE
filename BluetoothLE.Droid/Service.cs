using System;
using BluetoothLE.Core;
using Android.Bluetooth;
using Java.Util;
using System.Collections.Generic;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.IService" /> interface
	/// </summary>
	public class Service : IService
	{
		private readonly BluetoothGattService _nativeService;
		private readonly BluetoothGatt _gatt;
		private readonly GattCallback _callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Service"/> class.
		/// </summary>
		/// <param name="nativeService">Native service.</param>
		/// <param name="gatt">Native Gatt.</param>
		/// <param name="callback">Callback.</param>
		public Service(BluetoothGattService nativeService, BluetoothGatt gatt, GattCallback callback)
		{
			_nativeService = nativeService;
			_gatt = gatt;
			_callback = callback;

			_id = ServiceIdFromUuid(_nativeService.Uuid);

			Characteristics = new List<ICharacteristic>();
		}

		/// <summary>
		/// Gets a service identifier from a UUID
		/// </summary>
		/// <returns>The service identifier.</returns>
		/// <param name="uuid">The service UUID.</param>
		public static Guid ServiceIdFromUuid(UUID uuid)
		{
			return Guid.ParseExact(uuid.ToString(), "d");
		}

		#region IService implementation

		/// <summary>
		/// Occurs when characteristics discovered.
		/// </summary>
		public event EventHandler<CharacteristicDiscoveredEventArgs> CharacteristicDiscovered = delegate {};

		/// <summary>
		/// Discovers the characteristics for the services.
		/// </summary>
		public void DiscoverCharacteristics()
		{
			// do nothing
			foreach (var c in _nativeService.Characteristics)
			{
				var characteristic = new Characteristic(c, _gatt, _callback);
				Characteristics.Add(characteristic);

				CharacteristicDiscovered(this, new CharacteristicDiscoveredEventArgs(characteristic));
			}
		}

		private readonly Guid _id;
		/// <summary>
		/// Gets the service's unique identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public Guid Id { get { return _id; } }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		public string Uuid { get { return _nativeService.Uuid.ToString(); }}

		/// <summary>
		/// Gets a value indicating whether this instance is primary.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsPrimary { get { return _nativeService.Type == GattServiceType.Primary; } }

		/// <summary>
		/// Gets the service's characteristics.
		/// </summary>
		/// <value>The characteristics.</value>
		public IList<ICharacteristic> Characteristics { get; set; }

		#endregion
	}
}


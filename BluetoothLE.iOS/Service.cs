using System;
using BluetoothLE.Core;
using CoreBluetooth;
using System.Collections.Generic;
using System.Linq;
using BluetoothLE.Core.Events;

namespace BluetoothLE.iOS
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.IService" /> interface
	/// </summary>
	public class Service : IService, IDisposable
	{
		private readonly CBPeripheral _peripheral;
		private readonly CBService _nativeService;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Service"/> class.
		/// </summary>
		/// <param name="peripheral">The native peripheral.</param>
		/// <param name="service">The native service.</param>
		public Service(CBPeripheral peripheral, CBService service)
		{
			_peripheral = peripheral;
			_nativeService = service;

			_id = service.UUID.ToString().ToGuid();

			_peripheral.DiscoveredCharacteristic += DiscoveredCharacteristic;

			Characteristics = new List<ICharacteristic>();
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
			_peripheral.DiscoverCharacteristics(_nativeService);
		}

		private Guid _id;
		/// <summary>
		/// Gets the service's unique identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public Guid Id { get { return _id; } }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		public string Uuid { get { return _nativeService.UUID.ToString(); }}

		/// <summary>
		/// Gets a value indicating whether this instance is primary.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsPrimary { get { return _nativeService.Primary; } }

		/// <summary>
		/// Gets the service's characteristics.
		/// </summary>
		/// <value>The characteristics.</value>
		public IList<ICharacteristic> Characteristics { get; set; }

		#endregion

		#region CBPeripheral delegate methods

		private void DiscoveredCharacteristic(object sender, CBServiceEventArgs args)
		{
			if (args.Service.UUID != _nativeService.UUID)
				return;
			
			foreach (var c in args.Service.Characteristics)
			{
				var charId = c.UUID.ToString().ToGuid();
				if (Characteristics.All(x => x.Id != charId))
				{
					var characteristic = new Characteristic(_peripheral, c);
					Characteristics.Add(characteristic);

					CharacteristicDiscovered(this, new CharacteristicDiscoveredEventArgs(characteristic));
				}
			}
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="BluetoothLE.iOS.Service"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="BluetoothLE.iOS.Service"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="BluetoothLE.iOS.Service"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="BluetoothLE.iOS.Service"/> so the garbage
		/// collector can reclaim the memory that the <see cref="BluetoothLE.iOS.Service"/> was occupying.</remarks>
		public void Dispose()
		{
			_peripheral.DiscoveredCharacteristic -= DiscoveredCharacteristic;
		}

		#endregion
	}
}


using System;
using BluetoothLE.Core;
using Android.Bluetooth;
using Java.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Android.OS;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.IService" /> interface
	/// </summary>
	public class Service : IService, IDisposable
	{
		internal readonly BluetoothGattService NativeService;
		private readonly BluetoothGatt _gatt;
		private readonly GattCallback _callback;
		
		public Service(Guid uuid, bool isPrimary) {
			NativeService = new BluetoothGattService(UUID.FromString(uuid.ToString()), isPrimary ? GattServiceType.Primary : GattServiceType.Secondary);
			var characteristics = new ObservableCollection<ICharacteristic>();
			characteristics.CollectionChanged += CharacteristicsOnCollectionChanged;
			Characteristics = characteristics;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Service"/> class.
		/// </summary>
		/// <param name="nativeService">Native service.</param>
		/// <param name="gatt">Native Gatt.</param>
		/// <param name="callback">Callback.</param>
		public Service(BluetoothGattService nativeService, BluetoothGatt gatt, GattCallback callback) {
			NativeService = nativeService;
			_gatt = gatt;
			_callback = callback;

			_id = ServiceIdFromUuid(NativeService.Uuid);

			Characteristics = new ObservableCollection<ICharacteristic>();
		}

		/// <summary>
		/// Gets a service identifier from a UUID
		/// </summary>
		/// <returns>The service identifier.</returns>
		/// <param name="uuid">The service UUID.</param>
		public static Guid ServiceIdFromUuid(UUID uuid) {
			return Guid.ParseExact(uuid.ToString(), "d");
		}

		#region IService implementation

		/// <summary>
		/// Occurs when characteristics discovered.
		/// </summary>
		public event EventHandler<CharacteristicDiscoveredEventArgs> CharacteristicDiscovered = delegate { };

		/// <summary>
		/// Discovers the characteristics for the services.
		/// </summary>
		public void DiscoverCharacteristics() {
			Characteristics.Clear();
			// do nothing
			foreach (var c in NativeService.Characteristics) {
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
		public Guid Id {
			get { return _id; }
		}

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		public string Uuid {
			get { return NativeService.Uuid.ToString(); }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is primary.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool IsPrimary {
			get { return NativeService.Type == GattServiceType.Primary; }
		}

		/// <summary>
		/// Gets the service's characteristics.
		/// </summary>
		/// <value>The characteristics.</value>
		public IList<ICharacteristic> Characteristics { get; set; }

		#endregion

		private void CharacteristicsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
			foreach (ICharacteristic newItem in notifyCollectionChangedEventArgs.NewItems) {
				switch (notifyCollectionChangedEventArgs.Action) {
					case NotifyCollectionChangedAction.Add:
						NativeService.AddCharacteristic((BluetoothGattCharacteristic) newItem.NativeCharacteristic);
						break;
					case NotifyCollectionChangedAction.Remove:
						// remove characteristic
						break;
					case NotifyCollectionChangedAction.Replace:
						// create & remove
						break;
					case NotifyCollectionChangedAction.Reset:
						// Remove all
						break;
					case NotifyCollectionChangedAction.Move:
					default:
						break;
				}
			}
		}

		public void Dispose() {
			_gatt.Close();
		}
	}
}


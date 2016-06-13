﻿using System;
using BluetoothLE.Core;
using CoreBluetooth;
using System.Collections.Generic;
using System.Text;
using Foundation;
using BluetoothLE.Core.Events;

namespace BluetoothLE.iOS {
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.ICharacteristic" /> interface
	/// </summary>
	public class Characteristic : ICharacteristic, IDisposable {
		private readonly CBPeripheral _peripheral;
		private readonly CBCharacteristic _nativeCharacteristic;
		private bool _isUpdating;

		public Characteristic(Guid uuid, CharacterisiticPermissionType permissions, CharacteristicPropertyType properties) {
			CBAttributePermissions nativePermissions = 0;
			nativePermissions = GetNativePermissions(permissions);
			_nativeCharacteristic = new CBMutableCharacteristic(CBUUID.FromString(uuid.ToString()), (CBCharacteristicProperties)properties, null, nativePermissions);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Characteristic"/> class.
		/// </summary>
		/// <param name="peripheral">The native peripheral.</param>
		/// <param name="nativeCharacteristic">The native characteristic.</param>
		public Characteristic(CBPeripheral peripheral, CBCharacteristic nativeCharacteristic) {
			_peripheral = peripheral;
			_nativeCharacteristic = nativeCharacteristic;
			_peripheral.UpdatedCharacterteristicValue += UpdatedCharacteristicValue;
			_peripheral.UpdatedNotificationState += PeripheralOnUpdatedNotificationState;
			_peripheral.WroteCharacteristicValue += UpdatedCharacteristicValue;
			_id = _nativeCharacteristic.UUID.ToString().ToGuid();
		}

		private void PeripheralOnUpdatedNotificationState(object sender, CBCharacteristicEventArgs cbCharacteristicEventArgs) {
			NotificationStateChanged?.Invoke(this, new CharacteristicNotificationStateEventArgs(this));
		}

		#region ICharacteristic implementation

		/// <summary>
		/// Occurs when value updated.
		/// </summary>
		public event EventHandler<CharacteristicUpdateEventArgs> ValueUpdated;

		/// <summary>
		/// Occurs when the subscription state changed
		/// </summary>
		public event EventHandler<CharacteristicNotificationStateEventArgs> NotificationStateChanged;

		/// <summary>
		/// Is the charactersitic subscribed to
		/// </summary>
		public bool Updating => _isUpdating;


		/// <summary>
		/// Subscribe to the characteristic
		/// </summary>
		public void StartUpdates() {
			if (!CanUpdate)
				throw new InvalidOperationException("Characteristic does not support UPDATE");

			_peripheral.SetNotifyValue(true, _nativeCharacteristic);
			_isUpdating = true;
		}

		/// <summary>
		/// Unsubscribe from the characteristic
		/// </summary>
		public void StopUpdates() {
			if (CanUpdate) {
				_peripheral.UpdatedCharacterteristicValue -= UpdatedCharacteristicValue;
				_peripheral.SetNotifyValue(false, _nativeCharacteristic);
			}

			_isUpdating = false;
		}

		/// <summary>
		/// Read the characteristic's value
		/// </summary>
		public void Read() {
			if (!CanRead)
				throw new InvalidOperationException("Characteristic does not support READ");

			_peripheral.ReadValue(_nativeCharacteristic);
		}

		/// <summary>
		/// Write the specified data to the characteristic
		/// </summary>
		/// <param name="data">Data.</param>
		public void Write(byte[] data) {
			if (!CanWrite) {
				throw new InvalidOperationException("Characteristic does not support WRITE");
			}

			var nsData = NSData.FromArray(data);
			var writeType = ((Properties & CharacteristicPropertyType.WriteWithoutResponse) > 0) ?
				CBCharacteristicWriteType.WithoutResponse :
				CBCharacteristicWriteType.WithResponse;

			_peripheral.WriteValue(nsData, _nativeCharacteristic, writeType);
		}

		private Guid _id;

		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <value>The unique identifier.</value>
		public Guid Id { get { return _id; } }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		public string Uuid { get { return _nativeCharacteristic.UUID.ToString(); } }

		/// <summary>
		/// Gets the characteristic's value.
		/// </summary>
		/// <value>The characteristic's value.</value>
		public byte[] Value {
			get {
				return _nativeCharacteristic.Value?.ToArray();
			}
			set {
				_nativeCharacteristic.Value = NSData.FromArray(value);
			}
		}

		/// <summary>
		/// Gets the characteristic's value as a string.
		/// </summary>
		/// <value>The characteristic's value, interpreted as a string.</value>
		public string StringValue {
			get {
				if (Value == null)
					return string.Empty;

				return Encoding.UTF8.GetString(Value);
			}
		}

		/// <summary>
		/// Gets the native characteristic. Should be cast to the appropriate type.
		/// </summary>
		/// <value>The native characteristic.</value>
		public object NativeCharacteristic { get { return _nativeCharacteristic; } }

		/// <summary>
		/// Gets the characteristic's properties
		/// </summary>
		/// <value>The characteristic's properties.</value>
		public CharacteristicPropertyType Properties {
			get { return (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties; }
			set {

			}
		}

		public CharacterisiticPermissionType Permissions {
			//TODO: Figure out how to get these
			get { return 0; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance can read.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool CanRead { get { return (Properties & CharacteristicPropertyType.Read) > 0; } }

		/// <summary>
		/// Gets a value indicating whether this instance can update.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool CanUpdate { get { return (Properties & CharacteristicPropertyType.Notify) > 0; } }

		/// <summary>
		/// Gets a value indicating whether this instance can write.
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public bool CanWrite { get { return (Properties & CharacteristicPropertyType.WriteWithoutResponse) > 0 || (Properties & CharacteristicPropertyType.Write) > 0; } }

		#endregion

		#region CBPeripheral delegate methods

		private void UpdatedCharacteristicValue(object sender, CBCharacteristicEventArgs e) {
			var c = e.Characteristic;
			var cId = c.UUID;
			var tId = _nativeCharacteristic.UUID;
			if (cId == tId) {
				ValueUpdated?.Invoke(this, new CharacteristicUpdateEventArgs(this));
			}
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="BluetoothLE.iOS.Characteristic"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="BluetoothLE.iOS.Characteristic"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="BluetoothLE.iOS.Characteristic"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="BluetoothLE.iOS.Characteristic"/>
		/// so the garbage collector can reclaim the memory that the <see cref="BluetoothLE.iOS.Characteristic"/> was occupying.</remarks>
		public void Dispose() {
			_peripheral.UpdatedCharacterteristicValue -= UpdatedCharacteristicValue;
		}
		#endregion

		private CBAttributePermissions GetNativePermissions(CharacterisiticPermissionType permissions) {
			CBAttributePermissions nativePermissions = 0;
			foreach (CharacterisiticPermissionType value in Enum.GetValues(typeof(CharacterisiticPermissionType))) {
				if (permissions.HasFlag(value)) {
					switch (value) {
						case CharacterisiticPermissionType.Read:
							nativePermissions |= CBAttributePermissions.Readable;
							break;
						case CharacterisiticPermissionType.Write:
							nativePermissions |= CBAttributePermissions.Writeable;
							break;
						case CharacterisiticPermissionType.ReadEncrypted:
							nativePermissions |= CBAttributePermissions.ReadEncryptionRequired;
							break;
						case CharacterisiticPermissionType.WriteEncrypted:
							nativePermissions |= CBAttributePermissions.WriteEncryptionRequired;
							break;
					}
				}
			}
			return nativePermissions;
		}
	}
}




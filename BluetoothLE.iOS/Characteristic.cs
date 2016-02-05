using System;
using BluetoothLE.Core;
using CoreBluetooth;
using System.Collections.Generic;
using System.Text;
using Foundation;
using BluetoothLE.Core.Events;

namespace BluetoothLE.iOS
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.ICharacteristic" /> interface
	/// </summary>
	public class Characteristic : ICharacteristic, IDisposable
	{
		private readonly CBPeripheral _peripheral;
		private readonly CBCharacteristic _nativeCharacteristic;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.iOS.Characteristic"/> class.
		/// </summary>
		/// <param name="peripheral">The native peripheral.</param>
		/// <param name="nativeCharacteristic">The native characteristic.</param>
		public Characteristic(CBPeripheral peripheral, CBCharacteristic nativeCharacteristic)
		{
			_peripheral = peripheral;
			_nativeCharacteristic = nativeCharacteristic;

			_id = _nativeCharacteristic.UUID.ToString().ToGuid();
		}

		#region ICharacteristic implementation

		/// <summary>
		/// Occurs when value updated.
		/// </summary>
		public event EventHandler<CharacteristicReadEventArgs> ValueUpdated;

		private bool _isUpdating;
		/// <summary>
		/// Subscribe to the characteristic
		/// </summary>
		public void StartUpdates()
		{
			if (!CanUpdate)
				throw new InvalidOperationException("Characteristic does not support UPDATE");

			_peripheral.UpdatedCharacterteristicValue += UpdatedCharacteristicValue;
			_peripheral.SetNotifyValue(true, _nativeCharacteristic);
			_isUpdating = true;
		}

		/// <summary>
		/// Unsubscribe from the characteristic
		/// </summary>
		public void StopUpdates()
		{
			if (CanUpdate)
			{
				_peripheral.UpdatedCharacterteristicValue -= UpdatedCharacteristicValue;
				_peripheral.SetNotifyValue(false, _nativeCharacteristic);
			}

			_isUpdating = false;
		}

		/// <summary>
		/// Read the characteristic's value
		/// </summary>
		public void Read()
		{
			if (!CanRead)
				throw new InvalidOperationException("Characteristic does not support READ");
			
			_peripheral.ReadValue(_nativeCharacteristic);
		}

		/// <summary>
		/// Write the specified data to the characteristic
		/// </summary>
		/// <param name="data">Data.</param>
		public void Write(byte[] data)
		{
			if (!CanWrite)
				throw new InvalidOperationException("Characteristic does not support WRITE");

			var nsData = NSData.FromArray(data);
			var writeType = ((Properties & CharacteristicPropertyType.AppleWriteWithoutResponse) > 0) ?
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
		public byte[] Value { get { return _nativeCharacteristic.Value.ToArray(); } }

		/// <summary>
		/// Gets the characteristic's value as a string.
		/// </summary>
		/// <value>The characteristic's value, interpreted as a string.</value>
		public string StringValue
		{
			get
			{
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
		public CharacteristicPropertyType Properties { get { return (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties; } }

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
		public bool CanWrite { get { return (Properties & (CharacteristicPropertyType.WriteWithoutResponse | CharacteristicPropertyType.AppleWriteWithoutResponse)) > 0; } }

		#endregion

		#region CBPeripheral delegate methods

		private void UpdatedCharacteristicValue(object sender, CBCharacteristicEventArgs e)
		{
			ValueUpdated(this, new CharacteristicReadEventArgs(this));
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
		public void Dispose()
		{
			if (_isUpdating)
			{
				_peripheral.UpdatedCharacterteristicValue -= UpdatedCharacteristicValue;
			}
		}

		#endregion
	}
}


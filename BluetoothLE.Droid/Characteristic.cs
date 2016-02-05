using System;
using BluetoothLE.Core;
using Android.Bluetooth;
using System.Linq;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Droid
{
	/// <summary>
	/// Concrete implmentation of <see cref="BluetoothLE.Core.ICharacteristic" /> interface
	/// </summary>
	public class Characteristic : ICharacteristic
	{
		private readonly BluetoothGattCharacteristic _nativeCharacteristic;
		private readonly BluetoothGatt _gatt;
		private readonly GattCallback _callback;

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Droid.Characteristic"/> class.
		/// </summary>
		/// <param name="characteristic">Characteristic.</param>
		/// <param name="gatt">Gatt.</param>
		/// <param name="gattCallback">Gatt callback.</param>
		public Characteristic(BluetoothGattCharacteristic characteristic, BluetoothGatt gatt, GattCallback gattCallback)
		{
			_nativeCharacteristic = characteristic;
			_gatt = gatt;
			_callback = gattCallback;

			if (_callback != null)
			{
				_callback.CharacteristicValueUpdated += CharacteristicValueUpdated;
			}
		}

		#region ICharacteristic implementation

		/// <summary>
		/// Occurs when value updated.
		/// </summary>
		public event EventHandler<CharacteristicReadEventArgs> ValueUpdated = delegate {};

		/// <summary>
		/// Subscribe to the characteristic
		/// </summary>
		public void StartUpdates()
		{
			if (!CanUpdate)
				throw new InvalidOperationException("Characteristic does not support UPDATE");

			SetUpdateValue(true);
		}

		/// <summary>
		/// Unsubscribe from the characteristic
		/// </summary>
		public void StopUpdates()
		{
			if (CanUpdate)
				SetUpdateValue(false);
		}

		/// <summary>
		/// Read the characteristic's value
		/// </summary>
		public void Read()
		{
			if (!CanRead)
				throw new InvalidOperationException("Characteristic does not support READ");
			
			_gatt.ReadCharacteristic(_nativeCharacteristic);
		}

		/// <summary>
		/// Write the specified data to the characteristic
		/// </summary>
		/// <param name="data">Data.</param>
		public void Write(byte[] data)
		{
			if (!CanWrite)
				throw new InvalidOperationException("Characteristic does not support WRITE");

			_nativeCharacteristic.SetValue(data);
			_nativeCharacteristic.WriteType = GattWriteType.NoResponse;

			_gatt.WriteCharacteristic(_nativeCharacteristic);
		}

		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <value>The unique identifier.</value>
		public Guid Id { get { return Guid.Parse(_nativeCharacteristic.Uuid.ToString()); } }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		public string Uuid { get { return _nativeCharacteristic.Uuid.ToString(); } }

		/// <summary>
		/// Gets the characteristic's value.
		/// </summary>
		/// <value>The characteristic's value.</value>
		public byte[] Value { get { return _nativeCharacteristic.GetValue(); } }

		/// <summary>
		/// Gets the characteristic's value as a string.
		/// </summary>
		/// <value>The characteristic's value, interpreted as a string.</value>
		public string StringValue
		{
			get
			{
				var val = Value;
				if (val == null)
					return string.Empty;

				return System.Text.Encoding.UTF8.GetString(val);
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

		#region GattCallback delegate methods

		private void CharacteristicValueUpdated (object sender, CharacteristicReadEventArgs e)
		{
			if (e.Characteristic.Id == this.Id)
			{
				ValueUpdated(this, e);
			}
		}

		#endregion

		private void SetUpdateValue(bool enable)
		{
			if (!_gatt.SetCharacteristicNotification(_nativeCharacteristic, enable))
				throw new Exception("Unable to set the notification value on the characteristic");

			// hackity-hack-hack
			System.Threading.Thread.Sleep(100);

			if (_nativeCharacteristic.Descriptors.Count > 0)
			{
				const string descriptorId = "00002902-0000-1000-8000-00805f9b34fb";
				var value = enable ? BluetoothGattDescriptor.EnableNotificationValue : BluetoothGattDescriptor.DisableNotificationValue;
				var descriptor = _nativeCharacteristic.Descriptors.FirstOrDefault(x => x.Uuid.ToString() == descriptorId);
				if (descriptor != null && !descriptor.SetValue(value.ToArray()))
					throw new Exception("Unable to set the notification value on the descriptor");
			}
		}
	}
}


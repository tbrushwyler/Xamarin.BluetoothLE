using System;
using Android.Bluetooth;
using BluetoothLE.Core;
using BluetoothLE.Core.Events;
using Android.Util;
using System.Diagnostics;

namespace BluetoothLE.Droid
{
	/// <summary>
	/// Gatt callback to handle Gatt events.
	/// </summary>
	public class GattCallback : BluetoothGattCallback
	{
		/// <summary>
		/// Occurs when device connected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate {};

		/// <summary>
		/// Occurs when device disconnected.
		/// </summary>
		public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate {};

		/// <summary>
		/// Occurs when services discovered.
		/// </summary>
		public event EventHandler ServicesDiscovered = delegate {};

		/// <summary>
		/// Occurs when characteristic value updated.
		/// </summary>
		public event EventHandler<CharacteristicUpdateEventArgs> CharacteristicValueUpdated = delegate {};

		public event EventHandler<CharacteristicUpdateEventArgs> CharacteristicWriteComplete = delegate {};
		public event EventHandler<CharacteristicUpdateEventArgs> CharacteristicWriteFailed = delegate { };

		/// <summary>
		/// Occurs when the RSSI is updated
		/// </summary>
		public event EventHandler<RssiUpdateEventArgs> RssiValueUpdated = delegate { };

		/// <Docs>GATT client</Docs>
		/// <summary>
		/// Raises the connection state change event.
		/// </summary>
		/// <param name="gatt">Gatt.</param>
		/// <param name="status">Status.</param>
		/// <param name="newState">New state.</param>
		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
		{
			base.OnConnectionStateChange(gatt, status, newState);

			if (status != GattStatus.Success)
				return;
            
			var device = new Device(gatt.Device, gatt, this, 0);
			switch (newState)
			{
				case ProfileState.Disconnected:
                    device.State = DeviceState.Disconnected;

                    try
                    {
                        gatt.Close();
                        gatt = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Unable to close connection to gatt. Exception: {0}", ex.Message);
                    }
                    finally
                    {
                        DeviceDisconnected(this, new DeviceConnectionEventArgs(device));
                    }

					break;
				case ProfileState.Connected:
					device.State = DeviceState.Connected;
					
                    DeviceConnected(this, new DeviceConnectionEventArgs(device));   
					break;
			}
		}

		/// <summary>
		/// Raises the services discovered event.
		/// </summary>
		/// <param name="gatt">Gatt.</param>
		/// <param name="status">Status.</param>
		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
		{
			base.OnServicesDiscovered(gatt, status);

			if (status != GattStatus.Success)
				return;

			ServicesDiscovered(this, EventArgs.Empty);
		}

		/// <summary>
		/// Raises the characteristic read event.
		/// </summary>
		/// <param name="gatt">Gatt.</param>
		/// <param name="characteristic">Characteristic.</param>
		/// <param name="status">Status.</param>
		public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
		{
			base.OnCharacteristicRead(gatt, characteristic, status);

			if (status != GattStatus.Success)
				return;

			var iChar = new Characteristic(characteristic, gatt, this);
			CharacteristicValueUpdated(this, new CharacteristicUpdateEventArgs(iChar));
		}

		/// <Docs>GATT client the characteristic is associated with</Docs>
		/// <summary>
		/// Callback triggered as a result of a remote characteristic notification.
		/// </summary>
		/// <para tool="javadoc-to-mdoc">Callback triggered as a result of a remote characteristic notification.</para>
		/// <format type="text/html">[Android Documentation]</format>
		/// <since version="Added in API level 18"></since>
		/// <param name="gatt">Gatt.</param>
		/// <param name="characteristic">Characteristic.</param>
		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
		{
			base.OnCharacteristicChanged(gatt, characteristic);

			var iChar = new Characteristic(characteristic, gatt, this);
			CharacteristicValueUpdated(this, new CharacteristicUpdateEventArgs(iChar));
		}

		public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status) {
			base.OnCharacteristicWrite(gatt, characteristic, status);
			var iChar = new Characteristic(characteristic, gatt, this);
			if (status == GattStatus.Success) {
				CharacteristicWriteComplete(this, new CharacteristicUpdateEventArgs(iChar));
			} else {
				CharacteristicWriteFailed(this, new CharacteristicUpdateEventArgs(iChar));
			}
		}

		public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, GattStatus status) {
			base.OnReadRemoteRssi(gatt, rssi, status);
			RssiValueUpdated(this, new RssiUpdateEventArgs(rssi));
		}
	}
}


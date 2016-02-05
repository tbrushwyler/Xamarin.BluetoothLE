using System;
using System.Collections.Generic;
using BluetoothLE.Core.Events;

namespace BluetoothLE.Core
{
	/// <summary>
	/// Characteristic interface.
	/// </summary>
	public interface ICharacteristic
	{
		/// <summary>
		/// Occurs when value updated.
		/// </summary>
		event EventHandler<CharacteristicReadEventArgs> ValueUpdated;

		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <value>The unique identifier.</value>
		Guid Id { get; }

		/// <summary>
		/// Gets the UUID.
		/// </summary>
		/// <value>The UUID.</value>
		string Uuid { get; }

		/// <summary>
		/// Gets the characteristic's value.
		/// </summary>
		/// <value>The characteristic's value.</value>
		byte[] Value { get; }

		/// <summary>
		/// Gets the characteristic's value as a string.
		/// </summary>
		/// <value>The characteristic's value, interpreted as a string.</value>
		string StringValue { get; }

		/// <summary>
		/// Gets the native characteristic. Should be cast to the appropriate type.
		/// </summary>
		/// <value>The native characteristic.</value>
		object NativeCharacteristic { get; }

		/// <summary>
		/// Gets the characteristic's properties
		/// </summary>
		/// <value>The characteristic's properties.</value>
		CharacteristicPropertyType Properties { get; }

		/// <summary>
		/// Gets a value indicating whether this instance can read.
		/// </summary>
		/// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
		bool CanRead { get; }

		/// <summary>
		/// Gets a value indicating whether this instance can update.
		/// </summary>
		/// <value><c>true</c> if this instance can update; otherwise, <c>false</c>.</value>
		bool CanUpdate { get; }

		/// <summary>
		/// Gets a value indicating whether this instance can write.
		/// </summary>
		/// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
		bool CanWrite { get; }

		/// <summary>
		/// Subscribe to the characteristic
		/// </summary>
		void StartUpdates();

		/// <summary>
		/// Unsubscribe from the characteristic
		/// </summary>
		void StopUpdates();

		/// <summary>
		/// Read the characteristic's value
		/// </summary>
		void Read();

		/// <summary>
		/// Write the specified data to the characteristic
		/// </summary>
		/// <param name="data">Data.</param>
		void Write(byte[] data);
	}
}


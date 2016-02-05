using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Characteristic read event arguments.
	/// </summary>
	public class CharacteristicReadEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the characteristic that was read
		/// </summary>
		/// <value>The characteristic</value>
		public ICharacteristic Characteristic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Core.Events.CharacteristicReadEventArgs"/> class.
		/// </summary>
		/// <param name="characteristic">The characteristic that was read.</param>
		public CharacteristicReadEventArgs(ICharacteristic characteristic)
		{
			Characteristic = characteristic;
		}
	}
}


using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Characteristic read event arguments.
	/// </summary>
	public class CharacteristicUpdateEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the characteristic that was read
		/// </summary>
		/// <value>The characteristic</value>
		public ICharacteristic Characteristic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CharacteristicUpdateEventArgs"/> class.
		/// </summary>
		/// <param name="characteristic">The characteristic that was read.</param>
		public CharacteristicUpdateEventArgs(ICharacteristic characteristic)
		{
			Characteristic = characteristic;
		}
	}
}


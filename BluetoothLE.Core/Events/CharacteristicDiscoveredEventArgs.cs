using System;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Characteristic discovered event arguments.
	/// </summary>
	public class CharacteristicDiscoveredEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the characteristic that was discovered.
		/// </summary>
		/// <value>The characteristic.</value>
		public ICharacteristic Characteristic { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BluetoothLE.Core.Events.CharacteristicDiscoveredEventArgs"/> class.
		/// </summary>
		/// <param name="characteristic">The characteristic that was discovered.</param>
		public CharacteristicDiscoveredEventArgs(ICharacteristic characteristic)
		{
			Characteristic = characteristic;
		}
	}
}


using System;
using System.Collections.Generic;

namespace BluetoothLE.Core.Events
{
	/// <summary>
	/// Characteristic discovered event arguments.
	/// </summary>
	public class CharacteristicsDiscoveredEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the characteristic that was discovered.
		/// </summary>
		/// <value>The characteristic.</value>
		public IList<ICharacteristic> Characteristics { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CharacteristicsDiscoveredEventArgs"/> class.
		/// </summary>
		/// <param name="characteristic">The characteristic that was discovered.</param>
		public CharacteristicsDiscoveredEventArgs(IList<ICharacteristic> characteristics)
		{
			Characteristics = characteristics;
		}
	}
}


using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;

namespace BluetoothLE.Example.Models
{
	public class Grouping<K, T> : ObservableCollection<T>
	{
		public Grouping(K key) : base()
		{
			Key = key;
		}

		public Grouping(K key, IEnumerable<T> items) : base(items)
		{
			Key = key;
		}

		public K Key { get; private set; }
	}
}


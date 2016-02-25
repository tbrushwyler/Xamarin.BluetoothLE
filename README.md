# Xamarin.BluetoothLE

To get started, register the IAdapter implementation in AppDelegate.cs and MainActivity.cs:

```
DependencyService.Register<IAdapter, Adapter>
```

Then, to retrieve the implementation from your cross-platform code:

```
DependencyService.Get<IAdapter>();
```

**Note:** You should only ever instantiate one instance of `IAdapter`. I have used the following code to use a single instance throughout my code:

```
private static readonly IAdapter _bluetoothAdapter;
public static IAdapter BluetoothAdapter { get { return _bluetoothAdapter; } }

static App() {
	_bluetoothAdapter = DependencyService.Get<IAdapter>();

	_bluetoothAdapter.ScanTimeout = TimeSpan.FromSeconds(10);
	_bluetoothAdapter.ConnectionTimeout = TimeSpan.FromSeconds(10);
}
```

Then, the `IAdapter` can be retrieved with:

```
App.BluetoothAdapter
```

Once you have an `IAdapter`, see the documentation [here](https://github.com/tbrushwyler/Xamarin.BluetoothLE/wiki/N_BluetoothLE_Core) for usage.

using System;
using System.Reactive.Subjects;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Plugin.CurrentActivity;
using ReactiveBluetooth.Android.Extensions;
using ReactiveBluetooth.Core;

namespace ReactiveBluetooth.Android.Common
{
    public class BroadcastListener : BroadcastReceiver
    {
        private readonly Activity _activity;

        public BroadcastListener()
        {
            _activity = CrossCurrentActivity.Current.Activity;
            _activity.RegisterReceiver(this, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            StateUpdatedSubject = new BehaviorSubject<ManagerState>(BluetoothAdapter.DefaultAdapter?.State.ToManagerState() ?? ManagerState.Unsupported);
        }
                                                                                                               
        public BehaviorSubject<ManagerState> StateUpdatedSubject { get; }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            if (action == BluetoothAdapter.ActionStateChanged)
            {
                State state = (State) intent.GetIntExtra(BluetoothAdapter.ExtraState, BluetoothAdapter.Error);
                StateUpdatedSubject?.OnNext(state.ToManagerState());
            }
        }

        protected override void Dispose(bool disposing)
        {
            _activity.UnregisterReceiver(this);
            base.Dispose(disposing);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ReactiveBluetooth.Core;

namespace ReactiveBluetooth.Android.Extensions
{
    public static class StateExtentions
    {
        public static ManagerState ToManagerState(this State state)
        {
            switch (state)
            {
                case global::Android.Bluetooth.State.Connected:
                case global::Android.Bluetooth.State.Connecting:
                case global::Android.Bluetooth.State.Disconnected:
                case global::Android.Bluetooth.State.Disconnecting:
                case global::Android.Bluetooth.State.On:
                    return ManagerState.PoweredOn;
                case global::Android.Bluetooth.State.Off:
                    return ManagerState.PoweredOff;
                case global::Android.Bluetooth.State.TurningOff:
                case global::Android.Bluetooth.State.TurningOn:
                    return ManagerState.Resetting;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
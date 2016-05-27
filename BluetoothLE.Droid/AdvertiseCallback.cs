using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothLE.Core.Events;
using AdvertiseFailure = Android.Bluetooth.LE.AdvertiseFailure;

namespace BluetoothLE.Droid {
    class AdvertiseCallback : Android.Bluetooth.LE.AdvertiseCallback {
        /// <summary>
        /// Occurs when advertising start fails
        /// </summary>
        public EventHandler<AdvertiseStartEventArgs> AdvertiseStartFailed = delegate { };

        /// <summary>
        /// Occurs when advertising start succeeds
        /// </summary>
        public EventHandler<AdvertiseStartEventArgs> AdvertiseStartSuccess = delegate { }; 

        public override void OnStartFailure(AdvertiseFailure errorCode) {
            base.OnStartFailure(errorCode);
            var error = AdvertiseStatus.None;
            switch (errorCode) {
                case AdvertiseFailure.FeatureUnsupported:
                    error = AdvertiseStatus.Unsupported;
                    break;
                case AdvertiseFailure.AlreadyStarted:
                case AdvertiseFailure.DataTooLarge:
                case AdvertiseFailure.InternalError:
                case AdvertiseFailure.TooManyAdvertisers:
                    error = AdvertiseStatus.InternalError;
                    break;
            }
            AdvertiseStartFailed?.Invoke(this, new AdvertiseStartEventArgs(error));
        }

        public override void OnStartSuccess(AdvertiseSettings settingsInEffect) {
            base.OnStartSuccess(settingsInEffect);
            AdvertiseStartSuccess?.Invoke(this, new AdvertiseStartEventArgs(AdvertiseStatus.None));
        }
    }
}
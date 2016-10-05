namespace ReactiveBluetooth.Core
{
    /// <summary>
    /// States of <see cref="ICentralManager"/> and <see cref="IPeripheralManager"/>
    /// </summary>
    public enum ManagerState
    {
        /// <summary>
        /// Unknown state
        /// </summary>
        Unknown,
        /// <summary>
        /// Ble is resetting
        /// </summary>
        Resetting,
        /// <summary>
        /// The requested mode is not supported
        /// </summary>
        Unsupported,
        /// <summary>
        /// Not authorized to use bluetooth on the device
        /// </summary>
        Unauthorized,
        /// <summary>
        /// Bluetooth is not enabled on the device
        /// </summary>
        PoweredOff,
        /// <summary>
        /// Bluetooth is enabled on the device
        /// </summary>
        PoweredOn,
        /// <summary>
        /// The mode has limitations 
        /// </summary>
        PartialSupport,
    }
}
namespace Linux.Bluetooth
{
  /// <summary>Wrapper for Adapter1Properties.</summary>
  public class AdapterProperties
  {
    public string Address { get; set; } = default(string);

    /// <summary>
    ///   The Bluetooth  Address Type. For dual-mode and BR/EDR
		///   only adapter this defaults to "public". Single mode LE
		///   adapters may have either value. With privacy enabled
		///   this contains type of Identity Address and not type of
		///   address used for connection.
    ///
		///   Possible values:
		///   * "public" - Public address
		///   * "random" - Random address
    /// </summary>
    /// <remarks>Read-only</remarks>
    public string AddressType { get; set; } = default(string);

    /// <summary>
    ///   The Bluetooth system name (pretty hostname).
    ///
    ///   This property is either a static system default
    ///   or controlled by an external daemon providing
    ///   access to the pretty hostname configuration.
    /// </summary>
    /// <remarks>Read-only.</remarks>
    public string Name { get; set; } = default(string);

    /// <summary>
    ///   The Bluetooth friendly name. This value can be changed.
    ///
    ///   In case no alias is set, it will return the system
    ///   provided name. Setting an empty string as alias will
    ///   convert it back to the system provided name.
    ///
    ///   When resetting the alias with an empty string, the
    ///   property will default back to system name.
    ///
    ///   On a well configured system, this property never
    ///   needs to be changed since it defaults to the system
    ///   name and provides the pretty hostname. Only if the
    ///   local name needs to be different from the pretty
    ///   hostname, this property should be used as last
    ///   resort.
    /// </summary>
    /// <remarks>Read-write.</remarks>
    public string Alias { get; set; } = default(string);

    /// <summary>
    ///   The Bluetooth class of device.
    ///
    ///   This property represents the value that is either
    ///   automatically configured by DMI/ACPI information
    ///   or provided as static configuration.
    /// </summary>
    /// <remarks>Read-only.</remarks>
    public uint Class { get; set; } = default(uint);

    /// <summary>
    ///   Switch an adapter on or off.This will also set the
    ///   appropriate connectable state of the controller.
    ///
    ///   The value of this property is not persistent. After
    ///   restart or unplugging of the adapter it will reset
    ///   back to false.
    /// </summary>
    /// <remarks>Read-write.</remarks>
    public bool Powered { get; set; } = default(bool);

    /// <summary>
    ///   Switch an adapter to discoverable or non-discoverable
    ///   to either make it visible or hide it.This is a global
    ///
    ///   setting and should only be used by the settings
    ///   application.
    ///
    ///   If the DiscoverableTimeout is set to a non-zero
    ///   value then the system will set this value back to
    ///   false after the timer expired.
    ///
    ///   In case the adapter is switched off, setting this
    ///   value will fail.
    ///
    ///   When changing the Powered property the new state of
    ///   this property will be updated via a PropertiesChanged
    ///   signal.
    ///
    ///   For any new adapter this settings defaults to false.
    /// </summary>
    /// <remarks>Read-write.</remarks>
    public bool Discoverable { get; set; } = default(bool);

    /// <summary>
    ///   The discoverable timeout in seconds.A value of zero
    ///   means that the timeout is disabled and it will stay in
    ///   discoverable/limited mode forever.
    ///   
    ///   The default value for the discoverable timeout should
    ///   be 180 seconds(3 minutes).
    /// </summary>
    /// <remarks>Read-only.</remarks>
    public uint DiscoverableTimeout { get; set; } = default(uint);

    /// <summary>
    /// Indicates that a device discovery procedure is active.
    /// </summary>
    /// <remarks>Read-only.</remarks>
    public bool Discovering { get; set; } = default(bool);

    /// <summary>
    ///   Switch an adapter to pairable or non-pairable.This is
    ///   a global setting and should only be used by the
    ///   settings application.
    ///
    ///   Note that this property only affects incoming pairing
    ///   requests.
    ///
    ///   For any new adapter this settings defaults to true.
    /// </summary>
    /// <remarks>Read-write.</remarks>
    public bool Pairable { get; set; } = default(bool);

    /// <summary>
    ///   The pairable timeout in seconds.A value of zero
    ///   means that the timeout is disabled and it will stay in
    ///   pairable mode forever.
    ///
    ///   The default value for pairable timeout should be
    ///   disabled(value 0).
    /// </summary>
    /// <remarks>Read-write.</remarks>
    public uint PairableTimeout { get; set; } = default(uint);

    /// <summary>List of 128-bit UUIDs that represents the available local services.</summary>
    /// <remarks>Read-only.</remarks>
    public string[] UUIDs { get; set; } = default(string[]);

    /// <summary>Local device ID information in mod alias format used by the kernal and udev.</summary>
    /// <remarks>Readonly, optional.</remarks>
    public string Modalias { get; set; } = default(string);

    // Optional:
    /////// <summary>
    ///////   List of supported roles.Possible values:
    ///////   * "central": Supports the central role.
    ///////   * "peripheral": Supports the peripheral role.
    ///////   * "central-peripheral": Supports both roles concurrently.
    /////// </summary>
    //// public string[] Roles { get; set; }
    ////
    /////// <summary>
    ///////   List of 128-bit UUIDs that represents the experimental features currently enabled.
    /////// </summary>
    /////// <remarks>readonly, optional</remarks>
    //// public string[] ExperimentalFeatures { get; set; }
  }
}

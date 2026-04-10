# Linux.Bluetooth 6.0

## New Additions

* LEAdvertisement
* `DBusBase` class

### Server Example

Although Linux.Bluetooth already supports creating a Server; make a simple example.

## Design Considerations

### DBus Base Class

Base class `DBusBase` will be used to inherit common DBus method functionality.

* `GetAllAsync`
* `GetAsync`
* `SetAsync`
* `WatchPropertiesAsync`

### Refactoring Namespaces

Refactored namespaces to match Bluetooth hierarchy

1. Place GATT classes under `Linux.Bluetooth.Gatt`.
   * i.e. `Characteristics`, `Service`, `Descriptor`.
2. Place generic Advertisements under `Linux.Bluetooth.Advertisements`.
3. Split up `DBusInterfaces` to respective hierarchy
   * i.e. `GattInterfaces`, `CoreInterfaces`

### Extensions

Eliminate the `Linux.Bluetooth.Extensions` namespace, and place in the same namespace as the class which it extends.

## Under Consideration

* Support BT Classic

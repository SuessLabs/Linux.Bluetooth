# Generating with Tmds.DBus

## Install the Tool

There's got to be a better way than making a dummy project and performing `dotnet restore`

## Listing Services

To list services normally you would execute

```bash
dotnet dbus list service
```

However, BlueZ is a system service, which is referenced in `/usr/share/dbus-1/system-service` as `org.bluez.service`. To uncover all other system services you can execute one of the following:

```bash
dotnet dbus list services --bus system
```

## BlueZ Service

To get moving on listing the objects and generating code for BlueZ over DBus, perform the following:

```bash
$ dotnet dbus list services --bus system | grep bluez
org.bluez
```

In the results you will find, `org.bluez` if you have it installed (_Raspberr PI 4B does_).

### Get the Objects

To obtain the objects of `org.bluez` input the following and you should get similar results.

```bash
$ dotnet dbus list objects --bus system --service org.bluez
/ : org.freedesktop.DBus.ObjectManager
/org/bluez : org.bluez.AgentManager1 org.bluez.HealthManager1 org.bluez.ProfileManager1
/org/bluez/hci0 : org.bluez.Adapter1 org.bluez.GattManager1 org.bluez.LEAdvertisingManager1 org.bluez.Media1 org.bluez.NetworkServer1
```

### Generating Code

```bash
$ dotnet dbus codegen --bus system --service org.bluez
Generated: /home/.../bluez.DBus.cs
```

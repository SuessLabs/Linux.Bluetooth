# Generating with Tmds.DBus

Generating interfaces with Tmds.DBus v0.10.1.

Skip ahead to the **Generate Interfaces** section, if you already have it installed.

## Install Tmds.DBus.Tool

### .NET Prerequisites

The following actions were tested against [Ubuntu v20.04 LTS](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-). Please note, NET Core 3.1 or NET 6 is required for this step.

Verify that .NET 3.1 or 6.0 is installed.

```sh
dotnet --list-sdk
```

If the required SDK it's not installed, continue. Referenced by [Install the SDK - Ubuntu v20.04 LTS](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-)

```sh
sudo apt-get update; \
sudo apt-get install -y apt-transport-https && \
sudo apt-get update && \
sudo apt-get install -y dotnet-sdk-3.1
```

After installation, validate it again.

```sh
dotnet --list-sdk
```

### Download Tmds.DBus.Tool

Open a new console application (_create project space_)

```sh
dotnet new console -o GenBluez
cd GenBluez
```

Add a reference to `Tmds.DBus` in the `GenBluez.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Tmds.DBus" Version="0.10.1" />
  </ItemGroup>
</Project>
```

Restore dependencies

```sh
dotnet restore
dotnet tool install -g Tmds.DBus.Tool
```

## Validation

### Listing Services

To list services normally you would execute. Note, if you get an `Unhandled exception`, continue to the next step.

```sh
dotnet dbus list service
```

However, BlueZ is a system service, which is referenced in `/usr/share/dbus-1/system-service` as `org.bluez.service`. To uncover all other system services you can execute one of the following:

```bash
dotnet dbus list services --bus system
```

### Validate Existance of BlueZ Service

To get moving on listing the objects and generating code for BlueZ over DBus, perform the following:

```bash
dotnet dbus list services --bus system | grep bluez

OUTPUT:
org.bluez
```

In the results you will find, `org.bluez` if you have it installed (_Raspberr PI 4B does_).

## Generate Interfaces

### Get the Objects

To obtain the objects of `org.bluez` input the following and you should get similar results.

```bash
dotnet dbus list objects --bus system --service org.bluez
```

**OUTPUT:**

```txt
/ : org.freedesktop.DBus.ObjectManager
/org/bluez : org.bluez.AgentManager1 org.bluez.HealthManager1 org.bluez.ProfileManager1
/org/bluez/hci0 : org.bluez.Adapter1 org.bluez.GattManager1 org.bluez.LEAdvertisingManager1 org.bluez.Media1 org.bluez.NetworkServer1
```

### Generating Code

```bash
dotnet dbus codegen --bus system --service org.bluez
Generated: /home/.../bluez.DBus.cs
```

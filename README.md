# A .NET client for the Android Debug Bridge

[![Build status](https://ci.appveyor.com/api/projects/status/o60gc1bdp5y4tbxd?svg=true)](https://ci.appveyor.com/project/SAPTeamDEV/androctrl-protocols-androiddebugbridge)
[![NuGet Status](http://img.shields.io/nuget/v/AndroCtrl.Protocols.AndroidDebugBridge.svg?style=flat)](https://www.nuget.org/packages/AndroCtrl.Protocols.AndroidDebugBridge/)

This library allows .NET applications to communicate with Android devices. 
It provides a .NET implementation of the `adb` protocol, giving more flexibility to the developer than launching an 
`adb.exe` process and parsing the console output.

## Installation
To install this library to your Project, install the [AndroCtrl.Protocols.AndroidDebugBridge NuGetPackage](https://www.nuget.org/packages/AndroCtrl.Protocols.AndroidDebugBridge). If you're
using Visual Studio, you can run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):

```
PM> Install-Package AndroCtrl.Protocols.AndroidDebugBridge
```

## Getting Started

All of the adb functionality is exposed through the `AdbClient` class. You can create an instance of that class and use it.

This class provides various methods that allow you to interact with Android devices.

### Starting the `adb` server
We don't communicate directly with your Android devices, this class uses the `adb.exe` server process as an intermediate. Before you can connect to your Android device, you must first start the `adb.exe` server.

You can do so by either running `adb.exe` yourself (it comes as a part of the ADK, the Android Development Kit), or you can use the `AdbServer.StartServer` method like this:

```
AdbServer server = new AdbServer();
var result = server.StartServer(@"C:\Program Files (x86)\android-sdk\platform-tools\adb.exe", restartServerIfNewer: false);
```

### List all Android devices currently connected
To list all Android devices that are connected to your PC, you can use the following code:

```
var client = new AdbClient();
var devices = client.GetDevices();

foreach(var device in devices)
{
    Console.WriteLine(device.Name);
}
```

### Subscribe for events when devices connect/disconnect
To receive notifications when devices connect to or disconnect from your PC, you can use the `DeviceMonitor` class:

```
void Test()
{
    var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
    monitor.DeviceConnected += this.OnDeviceConnected;
    monitor.Start();
}

void OnDeviceConnected(object sender, DeviceDataEventArgs e)
{
    Console.WriteLine($"The device {e.Device.Name} has connected to this PC");
}
```

### Manage applications
To install or uninstall applications, you can use the `PackageManager` class:

```
void InstallApplication()
{
    var client = new AdbClient();
    var device = client.GetDevices().First();
    PackageManager manager = new PackageManager(device);
    manager.InstallPackage(@"C:\Users\me\Documents\mypackage.apk", reinstall: false);
}
```

### Send or receive files
To send files to or receive files from your Android device, you can use the `SyncService` class. When uploading a file, you need to specify
the permissions of the file. These are standard Unix file permissions. For example, `444` will give everyone read permissions and `666` will
give everyone write permissions. You also need to specify the date at which the file was last modified. A good default there is `DateTime.Now`.

```
void DownloadFile()
{
    var client = new AdbClient();
    var device = client.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
    using (Stream stream = File.OpenWrite(@"C:\MyFile.txt"))
    {
        service.Pull("/data/local/tmp/MyFile.txt", stream, null, CancellationToken.None);
    }
}

void UploadFile()
{
    var client = new AdbClient();
    var device = client.GetDevices().First();
    
    using (SyncService service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device))
    using (Stream stream = File.OpenRead(@"C:\MyFile.txt"))
    {
        service.Push(stream, "/data/local/tmp/MyFile.txt", 444, DateTime.Now, null, CancellationToken.None);
    }
}
```

### Run shell commands
To run shell commands on an Android device, you can use the `AdbClient.ExecuteRemoteCommand` method.

You need to pass a `DeviceData` object which specifies the device on which you want to run your command. You
can get a `DeviceData` object by calling `AdbClient.GetDevices()`, which will run one `DeviceData`
object for each device Android connected to your PC.

You'll also need to pass an `IOutputReceiver` object. Output receivers are classes that receive and parse the data
the device sends back. In this example, we'll use the standard `ConsoleOutputReceiver`, which reads all console
output and allows you to retrieve it as a single string. You can also use other output receivers or create your own.

```
void EchoTest()
{
    var client = new AdbClient();
    var device = client.GetDevices().First();
    var receiver = new ConsoleOutputReceiver();

    client.ExecuteRemoteCommand("echo Hello, World", device, receiver);

    Console.WriteLine("The device responded:");
    Console.WriteLine(receiver.ToString());
}
```

### Pair a Wi-Fi Device
Google in Android 11 added a new feature called `Wireless Debugging`. With this feature you can connect to a Device that in the same network as your machine.

Before you can connect to your device, You must pair it. Before pair, you must see ip address, port and pair code in your device.

```
void PairDevice()
{
    var client = new AdbClient();
    client.Pair(new("192.168.1.1", 12345), 123456);
}
```

### Run an Interactive Shell
You can now start a shell session and Communicate with your device by Using a `ShellSocket` class instance.

This class has various methods for communicating with your device, The simplest way is to use `ShellSocket.Interact` method that Sends a command and wait for Receiving data from your device.

```
void ListDirs()
{
    var client = new AdbClient();
    var device = client.GetDevices().First();
    using (ShellSocket shell = client.StartShell(device))
    {
        Console.WriteLine(shell.Interact("ls"));
        Console.WriteLine(shell.Interact("ls /data/"));
    }
}
```

## History
This library continues development of [SharpAdbClient](https://github.com/quamotion/madb) which is a fork of [madb](https://github.com/camalot/madb); which in itself is a .NET port of the 
[ddmlib Java Library](https://android.googlesource.com/platform/tools/base/+/master/ddmlib/). Credits for porting 
this library go to [Ryan Conrad](https://github.com/camalot).


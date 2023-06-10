// <copyright file="NamespaceDoc.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>


using System.Runtime.CompilerServices;

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// <para>
    ///     SAPTeam.AndroCtrl.Adb is a .NET library that allows.NET applications to communicate with Android devices.
    ///     It provides a.NET implementation of the <c>adb</c> protocol, giving more flexibility to the developer than launching an
    ///     <c>adb.exe</c> process and parsing the console output.
    /// </para>
    ///
    /// <para>
    ///     Most of the adb functionality is exposed through the <see cref="AdbClient"/> class.
    ///     You can create an instance of that class to use adb commands.
    /// </para>
    ///
    /// <para>
    ///     To send and receive files to and from Android devices, you can use the <see cref="SyncService"/> class.
    /// </para>
    ///
    /// <para>
    ///     To be notified when Android devices connect to or disconnect from your PC, you can use the <see cref="DeviceMonitor"/>
    ///     class.
    /// </para>
    /// </summary>
    ///
    /// <example>
    /// <para>
    ///     To list all Android devices that are connected to your PC, you can use the following code:
    /// </para>
    ///
    /// <code>
    /// var devices = devices = AdbClient.Instance.GetDevices();
    ///
    /// foreach(var device in devices)
    /// {
    ///     Console.WriteLine(device.Name
    /// }
    /// </code>
    /// </example>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}
﻿// <copyright file="Factories.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>


using System;
using System.Net;

using SAPTeam.AndroCtrl.Adb.Interfaces;

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// Provides factory methods used by the various SAPTeam.AndroCtrl.Adb classes.
    /// </summary>
    public static class Factories
    {
        static Factories()
        {
            Reset();
        }

        /// <summary>
        /// Gets or sets a delegate which creates a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdbSocket"/> class.
        /// </returns>
        public static Func<EndPoint, IAdbSocket> AdbSocketFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a delegate that creates a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdbClient"/> class.
        /// </returns>
        public static Func<EndPoint, IAdbClient> AdbClientFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="IAdbCommandLineClient"/> interface, that can be used to interact with the
        /// <c>adb.exe</c> command line client.
        /// </summary>
        public static Func<string, IAdbCommandLineClient> AdbCommandLineClientFactory
        { get; set; }

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="ISyncService"/> interface, that can be used to transfer files to and from
        /// a given device.
        /// </summary>
        public static Func<IAdbClient, DeviceData, ISyncService> SyncServiceFactory
        { get; set; }

        /// <summary>
        /// Resets all factories to their default values.
        /// </summary>
        public static void Reset()
        {
            AdbSocketFactory = (endPoint) => new AdbSocket(endPoint);
            AdbClientFactory = (endPoint) => new AdbClient(endPoint, AdbSocketFactory);
            AdbCommandLineClientFactory = (path) => new AdbCommandLineClient(path);
            SyncServiceFactory = (client, device) => new SyncService(client, device);
        }
    }
}
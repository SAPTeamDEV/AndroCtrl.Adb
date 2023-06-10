﻿// <copyright file="DeviceDataEventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>


using System;

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    public class DeviceDataEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDataEventArgs"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DeviceDataEventArgs(DeviceData device)
        {
            Device = device;
        }

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <value>The device.</value>
        public DeviceData Device { get; private set; }
    }
}
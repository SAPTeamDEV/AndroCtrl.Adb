﻿// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>


using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using SAPTeam.AndroCtrl.Adb.Exceptions;
using SAPTeam.AndroCtrl.Adb.Interfaces;

namespace SAPTeam.AndroCtrl.Adb.DeviceCommands
{
    /// <summary>
    /// Allows you to get information about packages that are installed on a device.
    /// </summary>
    public class PackageManager
    {
        /// <summary>
        /// The path to a temporary directory to use when pushing files to the device.
        /// </summary>
        public const string TempInstallationDirectory = "/data/local/tmp/";

        /// <summary>
        /// The command that list all packages installed on the device.
        /// </summary>
        private const string ListFull = "pm list packages -f";

        /// <summary>
        /// The command that list all third party packages installed on the device.
        /// </summary>
        private const string ListThirdPartyOnly = "pm list packages -f -3";

        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<PackageManager> logger;

        /// <summary>
        /// The <see cref="IAdbClient"/> to use when communicating with the device.
        /// </summary>
        private readonly IAdbClient client;

        /// <summary>
        /// A function which returns a new instance of a class that implements the
        /// <see cref="ISyncService"/> interface, that can be used to transfer files to and from
        /// a given device.
        /// </summary>
        private readonly Func<IAdbClient, DeviceData, ISyncService> syncServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager"/> class.
        /// </summary>
        /// <param name="client">
        /// The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.
        /// </param>
        /// <param name="device">
        /// The device on which to look for packages.
        /// </param>
        /// <param name="thirdPartyOnly">
        /// <see langword="true"/> to only indicate third party applications;
        /// <see langword="false"/> to also include built-in applications.
        /// </param>
        /// <param name="syncServiceFactory">
        /// A function which returns a new instance of a class that implements the
        /// <see cref="ISyncService"/> interface, that can be used to transfer files to and from
        /// a given device.
        /// </param>
        /// <param name="skipInit">
        /// A value indicating whether to skip the initial refresh of the package list or not. Used mainly by unit tests.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public PackageManager(IAdbClient client, DeviceData device, bool thirdPartyOnly = false, Func<IAdbClient, DeviceData, ISyncService> syncServiceFactory = null, bool skipInit = false, ILogger<PackageManager> logger = null)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            Packages = new Dictionary<string, string>();
            ThirdPartyOnly = thirdPartyOnly;
            this.client = client ?? throw new ArgumentNullException(nameof(client));

            this.syncServiceFactory = syncServiceFactory ?? Factories.SyncServiceFactory;

            if (!skipInit)
            {
                RefreshPackages();
            }

            this.logger = logger ?? NullLogger<PackageManager>.Instance;
        }

        /// <summary>
        /// Gets a value indicating whether this package manager only lists third party
        /// applications, or also includes built-in applications.
        /// </summary>
        public bool ThirdPartyOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of packages currently installed on the device. They key is the name of the
        /// package; the value the package path.
        /// </summary>
        public Dictionary<string, string> Packages { get; private set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        public void RefreshPackages()
        {
            ValidateDevice();

            PackageManagerReceiver pmr = new PackageManagerReceiver(Device, this);

            if (ThirdPartyOnly)
            {
                client.ExecuteShellCommand(Device, ListThirdPartyOnly, pmr);
            }
            else
            {
                client.ExecuteShellCommand(Device, ListFull, pmr);
            }
        }

        /// <summary>
        /// Installs an Android application on device.
        /// </summary>
        /// <param name="packageFilePath">
        /// The absolute file system path to file on local host to install.
        /// </param>
        /// <param name="reinstall">
        /// <see langword="true"/>if re-install of app should be performed; otherwise,
        /// <see langword="false"/>.
        /// </param>
        public void InstallPackage(string packageFilePath, bool reinstall)
        {
            ValidateDevice();

            string remoteFilePath = SyncPackageToDevice(packageFilePath);
            InstallRemotePackage(remoteFilePath, reinstall);
            RemoveRemotePackage(remoteFilePath);
        }

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device</param>
        /// <param name="reinstall">set to <see langword="true"/> if re-install of app should be performed</param>
        public void InstallRemotePackage(string remoteFilePath, bool reinstall)
        {
            ValidateDevice();

            InstallReceiver receiver = new InstallReceiver();
            string reinstallSwitch = reinstall ? "-r " : string.Empty;

            string cmd = $"pm install {reinstallSwitch}{remoteFilePath}";
            client.ExecuteShellCommand(Device, cmd, receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="packageName">
        /// The name of the package to uninstall.
        /// </param>
        public void UninstallPackage(string packageName)
        {
            ValidateDevice();

            InstallReceiver receiver = new InstallReceiver();
            client.ExecuteShellCommand(Device, $"pm uninstall {packageName}", receiver);
            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="packageName">
        /// The name of the package from which to get the application version.
        /// </param>
        public VersionInfo GetVersionInfo(string packageName)
        {
            ValidateDevice();

            VersionInfoReceiver receiver = new VersionInfoReceiver();
            client.ExecuteShellCommand(Device, $"dumpsys package {packageName}", receiver);
            return receiver.VersionInfo;
        }

        private void ValidateDevice()
        {
            if (Device.State != DeviceState.Online)
            {
                throw new AdbException("Device is offline");
            }
        }

        /// <summary>
        /// Pushes a file to device
        /// </summary>
        /// <param name="localFilePath">the absolute path to file on local host</param>
        /// <returns>destination path on device for file</returns>
        /// <exception cref="IOException">if fatal error occurred when pushing file</exception>
        private string SyncPackageToDevice(string localFilePath)
        {
            ValidateDevice();

            try
            {
                string packageFileName = Path.GetFileName(localFilePath);

                // only root has access to /data/local/tmp/... not sure how adb does it then...
                // workitem: 16823
                // workitem: 19711
                string remoteFilePath = LinuxPath.Combine(TempInstallationDirectory, packageFileName);

                logger.LogDebug(packageFileName, $"Uploading {packageFileName} onto device '{Device.Serial}'");

                using (ISyncService sync = syncServiceFactory(client, Device))
                using (Stream stream = File.OpenRead(localFilePath))
                {
                    logger.LogDebug($"Uploading file onto device '{Device.Serial}'");

                    // As C# can't use octals, the octal literal 666 (rw-Permission) is here converted to decimal (438)
                    sync.Push(stream, remoteFilePath, 438, File.GetLastWriteTime(localFilePath), null, CancellationToken.None);
                }

                return remoteFilePath;
            }
            catch (IOException e)
            {
                logger.LogError(e, $"Unable to open sync connection! reason: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Remove a file from device
        /// </summary>
        /// <param name="remoteFilePath">path on device of file to remove</param>
        /// <exception cref="IOException">if file removal failed</exception>
        private void RemoveRemotePackage(string remoteFilePath)
        {
            // now we delete the app we sync'ed
            try
            {
                client.ExecuteShellCommand(Device, "rm " + remoteFilePath, null);
            }
            catch (IOException e)
            {
                logger.LogError(e, $"Failed to delete temporary package: {e.Message}");
                throw;
            }
        }
    }
}
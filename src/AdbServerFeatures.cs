// <copyright file="AdbServerFeatures.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// Lists features which an Android Debug Bridge can support.
    /// </summary>
    public class AdbServerFeatures
    {
        /// <summary>
        /// The server supports the shell protocol.
        /// </summary>
        public const string Shell2 = "shell_v2";

        /// <summary>
        /// The server supports the <c>cmd</c> command.
        /// </summary>
        public const string Cmd = "cmd";

        /// <summary>
        /// The server supports the stat2 protocol.
        /// </summary>
        public const string Stat2 = "stat_v2";

        /// <summary>
        /// The server supports libusb
        /// </summary>
        public const string Libusb = "libusb";

        /// <summary>
        /// The server supports <c>push --sync</c>
        /// </summary>
        public const string PushSync = "push_sync";
    }
}
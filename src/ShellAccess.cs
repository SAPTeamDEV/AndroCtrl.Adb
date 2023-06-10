// <copyright file="ShellAccess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// Contains linux console user access level.
    /// </summary>
    public enum ShellAccess
    {
        /// <summary>
        /// Represents that current shell session has adb uid.
        /// </summary>
        Adb,

        /// <summary>
        /// Represents that current shell session has root 0 uid.
        /// </summary>
        Root
    }
}

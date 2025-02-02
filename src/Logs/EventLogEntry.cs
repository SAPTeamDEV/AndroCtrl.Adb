﻿// <copyright file="EventLogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>


using System.Collections.ObjectModel;

namespace SAPTeam.AndroCtrl.Adb.Logs
{
    /// <summary>
    /// Represents an entry in event buffer of the the Android log.
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/core/+/master/include/log/log.h#482"/>
    public class EventLogEntry : LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <seealso cref="EventLogEntry"/> class.
        /// </summary>
        public EventLogEntry()
        {
            Values = new Collection<object>();
        }

        /// <summary>
        /// Gets or sets the 4 bytes integer key from "/system/etc/event-log-tags" file.
        /// </summary>
        public int Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the values of this event log entry.
        /// </summary>
        public Collection<object> Values
        {
            get;
            set;
        }
    }
}
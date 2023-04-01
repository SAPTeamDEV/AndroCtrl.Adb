using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroCtrl.Protocols.AndroidDebugBridge
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

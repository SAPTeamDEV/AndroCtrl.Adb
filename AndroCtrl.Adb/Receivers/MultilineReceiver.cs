// <copyright file="MultilineReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>


using System.Collections.Generic;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Receivers
{
    /// <summary>
    ///
    /// </summary>
    public abstract class MultiLineReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineReceiver"/> class.
        /// </summary>
        public MultiLineReceiver()
        {
            Lines = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        public bool TrimLines { get; set; }

        /// <inheritdoc/>
        public virtual bool ParsesErrors { get; protected set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        protected ICollection<string> Lines { get; set; }

        /// <summary>
        /// Adds a line to the output.
        /// </summary>
        /// <param name="line">
        /// The line to add to the output.
        /// </param>
        public void AddOutput(string line)
        {
            Lines.Add(line);
        }

        /// <summary>
        /// Flushes the output.
        /// </summary>
        public void Flush()
        {
            if (Lines.Count > 0)
            {
                // send it for final processing
                ProcessNewLines(Lines);
                Lines.Clear();
            }

            Done();
        }

        /// <summary>
        /// Finishes the receiver
        /// </summary>
        protected virtual void Done()
        {
            // Do nothing
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected abstract void ProcessNewLines(IEnumerable<string> lines);
    }
}
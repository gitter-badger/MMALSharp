﻿// <copyright file="MMALDownstreamHandlerComponent.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using MMALSharp.Handlers;

namespace MMALSharp.Components
{
    /// <summary>
    /// Base class for all downstream components which support capture handlers.
    /// </summary>
    public abstract class MMALDownstreamHandlerComponent : MMALDownstreamComponent
    {
        /// <summary>
        /// Creates a new instance of <see cref="MMALDownstreamHandlerComponent"/>.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="handlers">The handlers to associate with each port.</param>
        protected MMALDownstreamHandlerComponent(string name, params ICaptureHandler[] handlers)
            : base(name)
        {
            if (handlers != null)
            {
                for (var i = 0; i < this.Outputs.Count; i++)
                {
                    this.Outputs[i].Handler = handlers[i];
                }    
            }
        }
    }
}

﻿// <copyright file="MMALComponentBase.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MMALSharp.Native;
using MMALSharp.Ports;
using MMALSharp.Ports.Clocks;
using MMALSharp.Ports.Controls;
using MMALSharp.Ports.Inputs;
using MMALSharp.Ports.Outputs;
using static MMALSharp.MMALNativeExceptionHelper;

namespace MMALSharp
{
    /// <summary>
    /// Base class for all components.
    /// </summary>
    public abstract unsafe class MMALComponentBase : MMALObject
    {
        /// <summary>
        /// Reference to the Control port of this component.
        /// </summary>
        public ControlPortBase Control { get; }

        /// <summary>
        /// Reference to all input ports associated with this component.
        /// </summary>
        public List<InputPortBase> Inputs { get; }

        /// <summary>
        /// Reference to all output ports associated with this component.
        /// </summary>
        public List<OutputPortBase> Outputs { get; }

        /// <summary>
        /// Reference to all clock ports associated with this component.
        /// </summary>
        public List<PortBase> Clocks { get; }

        /// <summary>
        /// Reference to all ports associated with this component.
        /// </summary>
        public List<PortBase> Ports { get; }
        
        /// <summary>
        /// Name of the component
        /// </summary>
        public string Name => Marshal.PtrToStringAnsi((IntPtr)this.Ptr->Name);

        /// <summary>
        /// Indicates whether this component is enabled.
        /// </summary>
        public bool Enabled => this.Ptr->IsEnabled == 1;

        internal bool ForceStopProcessing { get; set; }
        
        /// <summary>
        /// Native pointer to the component this object represents.
        /// </summary>
        private MMAL_COMPONENT_T* Ptr { get; }
        
        /// <summary>
        /// Creates the MMAL Component by the given name.
        /// </summary>
        /// <param name="name">The native MMAL name of the component you want to create.</param>
        protected MMALComponentBase(string name)
        {
            this.Ptr = CreateComponent(name);

            this.Inputs = new List<InputPortBase>();
            this.Outputs = new List<OutputPortBase>();
            this.Clocks = new List<PortBase>();
            this.Ports = new List<PortBase>();

            this.Control = new ControlPort(this.Ptr->Control, this, PortType.Control, Guid.NewGuid());

            for (int i = 0; i < this.Ptr->InputNum; i++)
            {
                this.Inputs.Add(new InputPort(&(*this.Ptr->Input[i]), this, PortType.Input, Guid.NewGuid()));
            }

            for (int i = 0; i < this.Ptr->OutputNum; i++)
            {
                this.Outputs.Add(new OutputPort(&(*this.Ptr->Output[i]), this, PortType.Output, Guid.NewGuid()));
            }

            for (int i = 0; i < this.Ptr->ClockNum; i++)
            {
                this.Clocks.Add(new ClockPort(&(*this.Ptr->Clock[i]), this, PortType.Clock, Guid.NewGuid()));
            }

            for (int i = 0; i < this.Ptr->PortNum; i++)
            {
                this.Ports.Add(new GenericPort(&(*this.Ptr->Port[i]), this, PortType.Generic, Guid.NewGuid()));
            }
        }
        
        /// <summary>
        /// Enables any connections associated with this component, traversing down the pipeline to enable those connections
        /// also.
        /// </summary>
        public void EnableConnections()
        {
            foreach (OutputPortBase port in this.Outputs)
            {
                if (port.ConnectedReference != null)
                {
                    // This component has an output port connected to another component.
                    port.ConnectedReference.DownstreamComponent.EnableConnections();

                    // Enable the connection
                    port.ConnectedReference.Enable();
                }
            }
        }

        /// <summary>
        /// Disables any connections associated with this component, traversing down the pipeline to disable those connections
        /// also.
        /// </summary>
        public void DisableConnections()
        {
            foreach (OutputPortBase port in this.Outputs)
            {
                if (port.ConnectedReference != null)
                {
                    // This component has an output port connected to another component.
                    port.ConnectedReference.DownstreamComponent.DisableConnections();

                    // Disable the connection
                    port.ConnectedReference.Disable();
                }
            }
        }

        /// <summary>
        /// Prints a summary of the ports associated with this component to the console.
        /// </summary>
        public virtual void PrintComponent()
        {
            MMALLog.Logger.Info($"Component: {this.Name}");

            for (var i = 0; i < this.Inputs.Count; i++)
            {
                if (this.Inputs[i].EncodingType != null)
                {
                    MMALLog.Logger.Info($"    Port {i} Input encoding: {this.Inputs[i].EncodingType.EncodingName}.");
                }
            }

            for (var i = 0; i < this.Outputs.Count; i++)
            {
                if (this.Outputs[i].EncodingType != null)
                {
                    MMALLog.Logger.Info($"    Port {i} Output encoding: {this.Outputs[i].EncodingType.EncodingName}");
                }
            }
        }

        /// <summary>
        /// Disposes of the current component, and frees any native resources still in use by it.
        /// </summary>
        public override void Dispose()
        {
            MMALLog.Logger.Debug($"Disposing component {this.Name}.");

            // See if any pools need disposing before destroying component.
            foreach (var port in this.Inputs)
            {
                if (port.BufferPool != null)
                {
                    MMALLog.Logger.Debug("Destroying port pool");

                    port.DestroyPortPool();
                }
                
                // Remove any unmanaged resources held by the capture handler.
                port.Handler?.Dispose();
            }

            foreach (var port in this.Outputs)
            {
                if (port.BufferPool != null)
                {
                    MMALLog.Logger.Debug("Destroying port pool");

                    port.DestroyPortPool();
                }
                
                // Remove any unmanaged resources held by the capture handler.
                port.Handler?.Dispose();
            }

            this.DisableComponent();
            this.DestroyComponent();

            MMALLog.Logger.Debug("Completed disposal...");

            base.Dispose();
        }
        
        /// <summary>
        /// Acquire a reference on a component. Acquiring a reference on a component will prevent a component from being destroyed until the 
        /// acquired reference is released (by a call to mmal_component_destroy). References are internally counted so all acquired references 
        /// need a matching call to release them.
        /// </summary>
        internal void AcquireComponent()
        {
            MMALComponent.mmal_component_acquire(this.Ptr);
        }

        /// <summary>
        /// Release a reference on a component Release an acquired reference on a component. Triggers the destruction of the component 
        /// when the last reference is being released.
        /// </summary>
        internal void ReleaseComponent()
        {
            MMALCheck(MMALComponent.mmal_component_release(this.Ptr), "Unable to release component");
        }

        /// <summary>
        /// Destroy a previously created component Release an acquired reference on a component. 
        /// Only actually destroys the component when the last reference is being released.
        /// </summary>
        internal void DestroyComponent()
        {
            MMALCheck(MMALComponent.mmal_component_destroy(this.Ptr), "Unable to destroy component");
        }

        /// <summary>
        /// Enable processing on a component.
        /// </summary>
        internal void EnableComponent()
        {
            if (!this.Enabled)
            {
                MMALCheck(MMALComponent.mmal_component_enable(this.Ptr), "Unable to enable component");
            }
        }

        /// <summary>
        /// Disable processing on a component.
        /// </summary>
        internal void DisableComponent()
        {
            if (this.Enabled)
            {
                MMALCheck(MMALComponent.mmal_component_disable(this.Ptr), "Unable to disable component");
            }
        }

        /// <summary>
        /// Helper method to destroy any port pools still in action. Failure to do this will cause MMAL to block indefinitely.
        /// </summary>
        internal void CleanPortPools()
        {
            // See if any pools need disposing before destroying component.
            foreach (var port in this.Inputs)
            {
                if (port.BufferPool != null)
                {
                    MMALLog.Logger.Debug("Destroying input port pool.");
                    
                    port.DisablePort();
                    port.DestroyPortPool();
                    port.BufferPool = null;
                }
            }

            foreach (var port in this.Outputs)
            {
                if (port.BufferPool != null)
                {
                    MMALLog.Logger.Debug("Destroying output port pool.");
                    
                    port.DisablePort();
                    port.DestroyPortPool();
                    port.BufferPool = null;
                }
            }
        }

        /// <summary>
        /// Provides a facility to create a component with a given name.
        /// </summary>
        /// <param name="name">The name of the component to create.</param>
        /// <returns>A pointer to the new component struct.</returns>
        private static MMAL_COMPONENT_T* CreateComponent(string name)
        {
            IntPtr ptr = IntPtr.Zero;
            MMALCheck(MMALComponent.mmal_component_create(name, &ptr), "Unable to create component");

            var compPtr = (MMAL_COMPONENT_T*)ptr.ToPointer();

            return compPtr;
        }
    }
}

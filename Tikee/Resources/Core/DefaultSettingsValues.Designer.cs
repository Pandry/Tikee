﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tikee.Resources.Core {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DefaultSettingsValues {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DefaultSettingsValues() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Tikee.Resources.Core.DefaultSettingsValues", typeof(DefaultSettingsValues).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to false.
        /// </summary>
        internal static string AddictionMode {
            get {
                return ResourceManager.GetString("AddictionMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # This is the default configuration file generated by Tikee
        ///# Author: Pandry &lt;https://github.com/Pandry&gt;
        ///# URL: https://github.com/Pandry/Tikee
        ///
        ///# The Tresholds section refers to the value of the treshold idle counter and the pause lenght after wich the timer will reset itself
        ///[Tresholds]
        ///# DefaultTimerDuration is the default timer lenght
        ///#  The format is supposed to be HH:mm:ss
        ///#  Default: 01:00:00
        ///DefaultTimerDuration = 01:00:00
        ///
        ///# PauseTreshold is the pause lenght. Medics recommend 15 minutes  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ConfigFileContent {
            get {
                return ResourceManager.GetString("ConfigFileContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tikee.ini.
        /// </summary>
        internal static string ConfigFileName {
            get {
                return ResourceManager.GetString("ConfigFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 00:00:01.
        /// </summary>
        internal static string DefaultPauseString {
            get {
                return ResourceManager.GetString("DefaultPauseString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 01:00:00.
        /// </summary>
        internal static string DefaultTimeString {
            get {
                return ResourceManager.GetString("DefaultTimeString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 00:00:03.
        /// </summary>
        internal static string IdleDisplayTresholdString {
            get {
                return ResourceManager.GetString("IdleDisplayTresholdString", resourceCulture);
            }
        }
    }
}
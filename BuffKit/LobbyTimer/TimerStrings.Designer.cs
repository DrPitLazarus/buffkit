﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BuffKit {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TimerStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TimerStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BuffKit.LobbyTimer.TimerStrings", typeof(TimerStrings).Assembly);
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
        ///   Looks up a localized string similar to TIMER: Setup time is over, the referee will either start the game or add overtime now.
        /// </summary>
        internal static string LoadoutSetupEnd {
            get {
                return ResourceManager.GetString("LoadoutSetupEnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Ships are locked, you have {0} seconds to set up your loadouts or request overtime.
        /// </summary>
        internal static string LoadoutSetupStart {
            get {
                return ResourceManager.GetString("LoadoutSetupStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Lobby time is over, the referee can force start the game now, please ready up.
        /// </summary>
        internal static string OvertimeLoadoutSetupEnd {
            get {
                return ResourceManager.GetString("OvertimeLoadoutSetupEnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Ships are locked, you have {0} seconds to set up your loadouts.
        /// </summary>
        internal static string OvertimeLoadoutSetupStart {
            get {
                return ResourceManager.GetString("OvertimeLoadoutSetupStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Overtime starting, {0} remaining, you will have {1} seconds to set up your loadouts after the timer ends.
        /// </summary>
        internal static string OvertimeStart {
            get {
                return ResourceManager.GetString("OvertimeStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Last change times: RED - {0}, BLUE - {1}. {2} can request overtime.
        /// </summary>
        internal static string OvertimeTeamAnnouncement {
            get {
                return ResourceManager.GetString("OvertimeTeamAnnouncement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PAUSE: {0} remaining.
        /// </summary>
        internal static string PauseAnnouncement {
            get {
                return ResourceManager.GetString("PauseAnnouncement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PAUSE: Pause extended, {0} remaining.
        /// </summary>
        internal static string PauseExtended {
            get {
                return ResourceManager.GetString("PauseExtended", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PAUSE: Timer paused, {0} remaining, starting a new {1} countdown to unpause.
        /// </summary>
        internal static string PauseStart {
            get {
                return ResourceManager.GetString("PauseStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: {0} remaining, ship lock after the timer runs out.
        /// </summary>
        internal static string PreLockAnnouncement {
            get {
                return ResourceManager.GetString("PreLockAnnouncement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PAUSE: The ref has stopped the timer indefinitely.
        /// </summary>
        internal static string RefPauseStart {
            get {
                return ResourceManager.GetString("RefPauseStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Timer starting, {0} remaining, you will have {1} seconds to set up your loadouts or request overtime after the timer ends
        ///        .
        /// </summary>
        internal static string Startup {
            get {
                return ResourceManager.GetString("Startup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: {0} until lock.
        /// </summary>
        internal static string TimerAnnouncement {
            get {
                return ResourceManager.GetString("TimerAnnouncement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Timer has been reset and is not running anymore.
        /// </summary>
        internal static string TimerReset {
            get {
                return ResourceManager.GetString("TimerReset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TIMER: Timer resumed, {0} remaining.
        /// </summary>
        internal static string TimerResumed {
            get {
                return ResourceManager.GetString("TimerResumed", resourceCulture);
            }
        }
    }
}

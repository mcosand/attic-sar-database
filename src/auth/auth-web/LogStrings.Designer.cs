﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sar.Auth {
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
    internal class LogStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LogStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Sar.Auth.LogStrings", typeof(LogStrings).Assembly);
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
        ///   Looks up a localized string similar to Account {username} has no first or last name set.
        /// </summary>
        internal static string AccountHasNoName {
            get {
                return ResourceManager.GetString("AccountHasNoName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {provider} login {providerId} already registered to account {account} ({first} {last}).
        /// </summary>
        internal static string AlreadyRegistered {
            get {
                return ResourceManager.GetString("AlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Associating login {provider}:{providerId} with {name}&apos;s account.
        /// </summary>
        internal static string AssociatingExternalLogin {
            get {
                return ResourceManager.GetString("AssociatingExternalLogin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {email} does not exist in the database.
        /// </summary>
        internal static string EmailNotFound {
            get {
                return ResourceManager.GetString("EmailNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account {@account} has linked member, but member not in database.
        /// </summary>
        internal static string LinkedMemberNotFound {
            get {
                return ResourceManager.GetString("LinkedMemberNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Login attempt for locked account {@account}.
        /// </summary>
        internal static string LockedAccountAttempt {
            get {
                return ResourceManager.GetString("LockedAccountAttempt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member not found in database.
        /// </summary>
        internal static string MemberNotFound {
            get {
                return ResourceManager.GetString("MemberNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {email} exists for multiple accounts: {@accounts}.
        /// </summary>
        internal static string MultipleAccountsForEmail {
            get {
                return ResourceManager.GetString("MultipleAccountsForEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {email} exists for multiple members: {@members}.
        /// </summary>
        internal static string MultipleMembersForEmail {
            get {
                return ResourceManager.GetString("MultipleMembersForEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sending verification code to {email} for login {provider}:{providerId}.
        /// </summary>
        internal static string SendingVerifyCode {
            get {
                return ResourceManager.GetString("SendingVerifyCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Don&apos;t know how to handle verification result {0}.
        /// </summary>
        internal static string UnknownProcessVerification {
            get {
                return ResourceManager.GetString("UnknownProcessVerification", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Verification code {verifyCode} for {email} is not correct.
        /// </summary>
        internal static string VerificationCodeNotCorrect {
            get {
                return ResourceManager.GetString("VerificationCodeNotCorrect", resourceCulture);
            }
        }
    }
}

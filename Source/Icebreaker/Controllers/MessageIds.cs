﻿//----------------------------------------------------------------------------------------------
// <copyright file="MessageIds.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Icebreaker.Controllers
{
    /// <summary>
    /// The accepted messages for the bot
    /// </summary>
    public static class MessageIds
    {
        /// <summary>
        /// Opt in to being paired up with someone
        /// </summary>
        public const string OptIn = "optin";

        /// <summary>
        /// Opt out to being paired up with someone
        /// </summary>
        public const string OptOut = "optout";

        /// <summary>
        /// Edit user profile
        /// </summary>
        public const string EditProfile = "editprofile";

        /// <summary>
        /// Edit user enrollment status
        /// </summary>
        public const string EditEnrollment = "editenrollment";

        /// <summary>
        /// Generate pairs from opted in users in the team
        /// </summary>
        public const string AdminMakePairs = "makepairs";

        /// <summary>
        /// Notify each person in the pairing of who they are paired up with
        /// </summary>
        public const string AdminNotifyPairs = "notifypairs";

        /// <summary>
        /// Edit team settings
        /// </summary>
        public const string AdminEditTeamSettings = "editteamsettings";

        /// <summary>
        /// Edit user info which includes profile and opt in status
        /// </summary>
        public const string AdminEditUser = "edituser";

        /// <summary>
        /// Welcome team with an invite to chat with the bot
        /// </summary>
        public const string AdminWelcomeTeam = "welcometeam";

        /// <summary>
        /// Debug the notify user card
        /// </summary>
        public const string DebugNotifyUser = "debugnotifyme";

        /// <summary>
        /// Debug the welcome card for a normal user
        /// </summary>
        public const string DebugWelcomeUser = "debugwelcomeme";

        /// <summary>
        /// Debug the welcome card for an admin user
        /// </summary>
        public const string DebugWelcomeUserAdmin = "debugwelcomemeadmin";

        /// <summary>
        /// Debug the welcome card for a team
        /// </summary>
        public const string DebugWelcomeTeam = "debugwelcometeam";
    }
}
// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2022 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

namespace Steamworks
{
    public static class SteamGameServerStats
    {
        /// <summary>
        /// <para> downloads stats for the user</para>
        /// <para> returns a GSStatsReceived_t callback when completed</para>
        /// <para> if the user has no stats, GSStatsReceived_t.m_eResult will be set to k_EResultFail</para>
        /// <para> these stats will only be auto-updated for clients playing on the server. For other</para>
        /// <para> users you'll need to call RequestUserStats() again to refresh any data</para>
        /// </summary>
        public static SteamAPICall_t RequestUserStats(CSteamID steamIDUser)
        {
            InteropHelp.TestIfAvailableGameServer();
            return (SteamAPICall_t)NativeMethods.ISteamGameServerStats_RequestUserStats(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser);
        }

        /// <summary>
        /// <para> requests stat information for a user, usable after a successful call to RequestUserStats()</para>
        /// </summary>
        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out int pData)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_GetUserStatInt32(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, out pData);
            }
        }

        public static bool GetUserStat(CSteamID steamIDUser, string pchName, out float pData)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_GetUserStatFloat(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, out pData);
            }
        }

        public static bool GetUserAchievement(CSteamID steamIDUser, string pchName, out bool pbAchieved)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_GetUserAchievement(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, out pbAchieved);
            }
        }

        /// <summary>
        /// <para> Set / update stats and achievements.</para>
        /// <para> Note: These updates will work only on stats game servers are allowed to edit and only for</para>
        /// <para> game servers that have been declared as officially controlled by the game creators.</para>
        /// <para> Set the IP range of your official servers on the Steamworks page</para>
        /// </summary>
        public static bool SetUserStat(CSteamID steamIDUser, string pchName, int nData)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_SetUserStatInt32(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, nData);
            }
        }

        public static bool SetUserStat(CSteamID steamIDUser, string pchName, float fData)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_SetUserStatFloat(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, fData);
            }
        }

        public static bool UpdateUserAvgRateStat(CSteamID steamIDUser, string pchName, float flCountThisSession, double dSessionLength)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_UpdateUserAvgRateStat(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2, flCountThisSession, dSessionLength);
            }
        }

        public static bool SetUserAchievement(CSteamID steamIDUser, string pchName)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_SetUserAchievement(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2);
            }
        }

        public static bool ClearUserAchievement(CSteamID steamIDUser, string pchName)
        {
            InteropHelp.TestIfAvailableGameServer();
            using (var pchName2 = new InteropHelp.UTF8StringHandle(pchName))
            {
                return NativeMethods.ISteamGameServerStats_ClearUserAchievement(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser, pchName2);
            }
        }

        /// <summary>
        /// <para> Store the current data on the server, will get a GSStatsStored_t callback when set.</para>
        /// <para> If the callback has a result of k_EResultInvalidParam, one or more stats</para>
        /// <para> uploaded has been rejected, either because they broke constraints</para>
        /// <para> or were out of date. In this case the server sends back updated values.</para>
        /// <para> The stats should be re-iterated to keep in sync.</para>
        /// </summary>
        public static SteamAPICall_t StoreUserStats(CSteamID steamIDUser)
        {
            InteropHelp.TestIfAvailableGameServer();
            return (SteamAPICall_t)NativeMethods.ISteamGameServerStats_StoreUserStats(CSteamGameServerAPIContext.GetSteamGameServerStats(), steamIDUser);
        }
    }
}

#endif // !DISABLESTEAMWORKS

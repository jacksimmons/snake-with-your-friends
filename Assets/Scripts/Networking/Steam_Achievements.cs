using Steamworks;
using UnityEngine;

public partial class Steam
{
    protected Callback<UserStatsReceived_t> userStatsReceived;


    private void Awake_Achievements()
    {
        // Requests achievement and friend data, with potential callbacks
        userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        bool loggedIn = SteamUserStats.RequestCurrentStats();
        if (!loggedIn)
            Debug.LogError("User not logged in - so can't get achievements!");
    }


    private void OnUserStatsReceived(UserStatsReceived_t result)
    {
        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Couldn't retrieve user stats.");
            return;
        }
    }


    public int GetStat(string name)
    {
        SteamUserStats.GetStat(name, out int stat);
        return stat;
    }


    public void SetStat(string name, float val)
    {
        SteamUserStats.GetAchievementProgressLimits(name, out int _, out int max);
        SteamUserStats.SetStat(name, val);
        SteamUserStats.IndicateAchievementProgress(name, (uint)val, (uint)max);
    }


    public void GiveAchievement(string name)
    {
        SteamUserStats.SetAchievement(name);
    }
}
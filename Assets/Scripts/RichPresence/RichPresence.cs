using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Discord;
using Steamworks;

public class RichPresence : MonoBehaviour
{
    const long ClientId = 917277682226593812;

    static Discord.Discord discordClient;
    static ActivityManager activityManager;

    static bool shuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Init()
    {
        discordClient = null;
        activityManager = null;

        DontDestroyOnLoad(new GameObject("RPC Manager").AddComponent<RichPresence>());

        SceneManager.sceneLoaded += UpdateAllRPC;
    }

    static void UpdateAllRPC(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        UpdateSteamRPC();
        UpdateDiscordRPC();
    }

    static bool IsPlaying()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        return sceneName is not "Title" or "Init";
    }

    static void UpdateSteamRPC()
    {
        if (!SteamManager.Initialized) return;

        SteamFriends.ClearRichPresence();
        var status = IsPlaying() ? "Playing" : "MainMenu";
        SteamFriends.SetRichPresence("steam_display", $"#Status_{status}");
    }

    static void UpdateDiscordRPC()
    {
        if (!InitDiscordRPCIfNeeded()) return;

        var playing = IsPlaying();

        var status = playing ? "Playing" : "In Main Menu";

        var activity = new Activity
        {
            State = status,
            Timestamps =
            {
                Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            Assets =
            {
                LargeImage = "icon_large",
                LargeText = "RECOUNTER"
            },
            Instance = playing
        };

        activityManager.UpdateActivity(activity, ActivityCallback);
    }

    static bool InitDiscordRPCIfNeeded()
    {
        if (discordClient != null) return true;

        try
        {
            discordClient = new Discord.Discord(ClientId, (ulong)CreateFlags.NoRequireDiscord);
            activityManager = discordClient.GetActivityManager();

            Debug.Log("Initialized Discord client.");
        }
        catch (ResultException)
        {
            Debug.Log("No Discord client detected.");
        }

        return discordClient != null;
    }

    static void ActivityCallback(Result r)
    {
        if (shuttingDown) return;

        if (r != Result.Ok)
            Debug.LogError("(Discord RPC) Failed to update activity: " + r);
    }

    void Update() => discordClient?.RunCallbacks();

    void OnDestroy()
    {
        shuttingDown = true;
        discordClient?.Dispose();
    }
}
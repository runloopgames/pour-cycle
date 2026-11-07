#if UNITY_ANDROID || UNITY_EDITOR
using UnityEngine;

namespace PourCycle
{
    public enum PlayTrialStatus
    {
        Unknown = 0,
        UnderTrial = 1,
        NotUnderTrial = 2
    }

    public interface IPlayTrialService
    {
        PlayTrialStatus CheckStatus();
        bool TryStopTrial();
    }

    public sealed class PlayTrialService : IPlayTrialService
    {
        private const string PlayTrialsClassName = "com.google.android.play.trials.PlayTrials";

        public PlayTrialStatus CheckStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass playTrials = new AndroidJavaClass(PlayTrialsClassName))
                {
                    int rawStatus = playTrials.CallStatic<int>("checkTrialStatus", currentActivity);
                    PlayTrialStatus status = ToStatus(rawStatus);
                    Log("[PlayTrial] Status: " + StatusName(status));
                    return status;
                }
            }
            catch (System.Exception exception)
            {
                Log("[PlayTrial] Exception: " + exception);
                Log("[PlayTrial] Status: UNKNOWN");
                return PlayTrialStatus.Unknown;
            }
#else
            Log("[PlayTrial] Status: UNKNOWN");
            return PlayTrialStatus.Unknown;
#endif
        }

        public bool TryStopTrial()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass playTrials = new AndroidJavaClass(PlayTrialsClassName))
                {
                    playTrials.CallStatic("stopTrial", currentActivity);
                    return true;
                }
            }
            catch (System.Exception exception)
            {
                Log("[PlayTrial] Exception: " + exception);
                return false;
            }
#else
            return false;
#endif
        }

        private static PlayTrialStatus ToStatus(int rawStatus)
        {
            if (rawStatus == (int)PlayTrialStatus.UnderTrial)
            {
                return PlayTrialStatus.UnderTrial;
            }

            if (rawStatus == (int)PlayTrialStatus.NotUnderTrial)
            {
                return PlayTrialStatus.NotUnderTrial;
            }

            return PlayTrialStatus.Unknown;
        }

        private static string StatusName(PlayTrialStatus status)
        {
            switch (status)
            {
                case PlayTrialStatus.UnderTrial:
                    return "UNDER_TRIAL";
                case PlayTrialStatus.NotUnderTrial:
                    return "NOT_UNDER_TRIAL";
                default:
                    return "UNKNOWN";
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }
    }
}
#endif

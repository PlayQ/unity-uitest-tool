using System.Collections.Generic;

namespace PlayQ.UITestTools
{
    public static class PermittedErrors
    {
        private static readonly List<string> permanent = new List<string>()
        {
            "Stuck in the Cinematic loop !reachedTarget. Exit from loop",
            "[FB] Failed to link:",
            "PlayQ.TG.Core.Exceptions.UnhandledHttpException: Offline mode",
            "Missing config value: features.rewarded_video.placements.level_pass_offer.enabled",
            "Missing config value: features.rewarded_video.placements.level_pass_offer.cooldown_minutes",
            "Missing config value: features.rewarded_video.placements.map_offer.enabled",
            "evel_unlock Missing features.rewarded_video.placements.map_offer.level_unlock. defaulting to 18",
            "[TG] FileStorage:: File",
        };

        private static readonly List<string> variable = new List<string>();

        public static void AddPermittedError(string errorText)
        {
            variable.Add(errorText);
        }

        public static void Clear()
        {
            variable.Clear();
        }
        
        public static bool IsPermitted(string condition)
        {
            foreach (var error in permanent)
            {
                if (condition.Contains(error))
                {
                    return true;
                }
            }
            foreach (var error in variable)
            {
                if (condition.Contains(error))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
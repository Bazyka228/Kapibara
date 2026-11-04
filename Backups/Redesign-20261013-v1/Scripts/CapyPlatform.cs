using UnityEngine;
#if PLUGIN_YG_2
using YG;
#endif

namespace Capybara67
{
    public sealed partial class CapyGame
    {
        bool platformPaused;
        void BindPlatform()
        {
#if PLUGIN_YG_2
            YG2.onPauseGame += PlatformPause;
            YG2.onGetSDKData += PlatformReady;
            if(YG2.isSDKEnabled) PlatformReady();
#endif
        }
        void PlatformReady()
        {
#if PLUGIN_YG_2
            YG2.GameReadyAPI();
            PlatformPlaying(round.Running && !paused && !platformPaused);
#endif
        }
        void PlatformPlaying(bool playing)
        {
#if PLUGIN_YG_2
            if(!YG2.isSDKEnabled) return;
            if(playing) YG2.GameplayStart(); else YG2.GameplayStop();
#endif
        }
        void PlatformPause(bool value)
        {
            platformPaused=value;
            if(value && audioSource) audioSource.Stop();
            if(!value) PlatformPlaying(round.Running && !paused);
        }
        void UnbindPlatform()
        {
#if PLUGIN_YG_2
            YG2.onPauseGame -= PlatformPause;
            YG2.onGetSDKData -= PlatformReady;
            if(YG2.isSDKEnabled) YG2.GameplayStop();
#endif
        }
    }
}

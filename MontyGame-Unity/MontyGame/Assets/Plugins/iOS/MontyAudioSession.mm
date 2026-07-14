#import <AVFoundation/AVFoundation.h>

// Unity's default iOS audio session category is SoloAmbient, which is SILENCED by
// the mute switch / Control Center mute. For a game whose entire soundtrack and
// every effect is generated at runtime, that reads as "the sound is broken".
//
// Playback is the category for apps whose audio is the point: it plays even when
// the device is muted, and keeps playing over the ringer switch.
extern "C" void MontyConfigureAudioSession()
{
    AVAudioSession *session = [AVAudioSession sharedInstance];
    NSError *err = nil;

    [session setCategory:AVAudioSessionCategoryPlayback
                   error:&err];
    if (err) { NSLog(@"[MontyGame] audio category failed: %@", err); err = nil; }

    [session setActive:YES error:&err];
    if (err) { NSLog(@"[MontyGame] audio session activate failed: %@", err); }
}

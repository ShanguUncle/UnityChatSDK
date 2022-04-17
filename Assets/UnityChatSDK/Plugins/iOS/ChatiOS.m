
#import "ChatiOSSet.h"
#import <AVFoundation/AVFoundation.h>

void _forceToSpeaker() {

    if (_headsetConnected()) {
        return;
    }
    
    UInt32 audioRouteOverride = kAudioSessionOverrideAudioRoute_Speaker;
    AudioSessionSetProperty(kAudioSessionProperty_OverrideAudioRoute,
                                     sizeof(audioRouteOverride),
                                     &audioRouteOverride);
}

void _forceToHeadset() {

    if (_headsetConnected()) {
        return;
    }
    
    UInt32 audioRouteOverride = kAudioSessionOverrideAudioRoute_None;
    AudioSessionSetProperty(kAudioSessionProperty_OverrideAudioRoute,
                                     sizeof(audioRouteOverride),
                                     &audioRouteOverride);
}


bool _headsetConnected() {
    
    UInt32 routeSize = sizeof(CFStringRef);
    CFStringRef route = NULL;
    OSStatus error = AudioSessionGetProperty(kAudioSessionProperty_AudioRoute, &routeSize, &route);
    
    if (!error &&
        (route != NULL)&&
        ([(__bridge NSString*)route rangeOfString:@"Head"].location != NSNotFound))
    {
        return true;
    }  
    return false;
}

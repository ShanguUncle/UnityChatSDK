var MicrophoneLibrary = {

    $CallbacksMap:{},

    devices: function (callback) {
        var callbackName = "devices";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.devices(callbackName);
    },

    end: function(deviceId, callback) {
        var callbackName = "end";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.end(document.getStringFromPtr(deviceId), callbackName);
    },

    getDeviceCaps: function(deviceId) {
        return document.UnityMicrophone.getDeviceCaps(document.getStringFromPtr(deviceId));
    },

    isRecording: function(deviceId) {
        return document.UnityMicrophone.isRecording(document.getStringFromPtr(deviceId));
    },

    start: function(deviceId, frequency, callback) {
        var callbackName = "start";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.start(document.getStringFromPtr(deviceId), frequency, callbackName);
    },

    requestPermission: function(callback) {
        var callbackName = "requestPermission";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.requestPermission(callbackName);
    },

    isPermissionGranted: function(callback) {
        var callbackName = "isPermissionGranted";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.isPermissionGranted(callbackName);
    },

    isSupported: function() {
        return document.UnityMicrophone.isSupported();
    },
    
    setRecordingBufferCallback: function(callback) {
        var callbackName = "setRecordingBufferCallback";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.setRecordingBufferCallback(callbackName);
    },

    getRecordingBuffer: function(callback) {
        var callbackName = "getRecordingBuffer";
        CallbacksMap[callbackName] = callback;
        document.UnityMicrophone.getRecordingBuffer(callbackName);
    },

    init: function(version, worklet) {
        if(document.UnityMicrophone != undefined)
            return 0;
        document.UnityMicrophone = new UnityMicrophone(version, worklet);

        function getStringFromPtr(ptr) {
            if(document.UnityMicrophone.unityVersion >= 2021.0){
                return UTF8ToString(ptr);
            } else{
                return Pointer_stringify(ptr);
            }
        }

        function getPtrFromString(str){
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(str, buffer, bufferSize);
            return buffer;
        }

        function callUnityCallback(callback, object){
            if(callback == null)
                return;

            var ptrFunc = CallbacksMap[callback];

            if(callback == "setRecordingBufferCallback"){
                var samples = new Float32Array(object.data);
                var buffer = _malloc(samples.length * 4)
                HEAPF32.set(object.data, buffer >> 2)

                if(document.UnityMicrophone.unityVersion >= 2021.0){
                    Module['dynCall_vii'](ptrFunc, buffer, object.length);
                } else{
                    Runtime.dynCall('vii', ptrFunc, [buffer, object.length]);
                }

                _free(buffer);
            } else {        
                var json = UnityWebGLTools.objectToJSON(object);
                var buffer = getPtrFromString(json);

                if(document.UnityMicrophone.unityVersion >= 2021.0){
                    Module['dynCall_vi'](ptrFunc, buffer);
                } else{
                    Runtime.dynCall('vi', ptrFunc, [buffer]);
                }
                
                _free(buffer);
            }
        }

        document.getPtrFromString = getPtrFromString;
        document.getStringFromPtr = getStringFromPtr;
        document.callUnityCallback = callUnityCallback;

        return 1;
    }
};

autoAddDeps(MicrophoneLibrary, '$CallbacksMap');
mergeInto(LibraryManager.library, MicrophoneLibrary);
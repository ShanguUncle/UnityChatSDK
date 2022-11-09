class UnityMicrophone {

    static DEFAULT_FREQUENCY = 44100;

    static INSTANCE = null;

    static AUDIO_WORKLET = false;

    constructor(version, worklet) {
        UnityMicrophone.INSTANCE = this;

        UnityMicrophone.AUDIO_WORKLET = worklet === 1 ? true : false;
        this.unityVersion = version;
        this.recording = false;
        this.frequency = UnityMicrophone.DEFAULT_FREQUENCY;
        this.devicesList = [];
        this.audioContext = new (window.AudioContext || window.webKitAudioContext)();
        this.recordingDevice = null;
        this.recordingSource = null;
        this.recordingBuffer = null;
        this.scriptProcessorNode = null;
        this.recordingBufferCallback = null;
        this.recordingEndedCallback = null;
        this.recordingStartedCallback = null;

        if (UnityMicrophone.AUDIO_WORKLET === true) {
            this.audioContext.audioWorklet.addModule('./Native/mic-worklet-processor.js')
                .then(() => {
                    console.log("worklet module registered");
                }).catch((err) => {
                    console.log(err);
                });
        }

        setInterval(() => {
            if (this.audioContext.state === "suspended" || this.audioContext.state === "interrupted") {
                console.log("resuming audioContext. state: " + this.audioContext.state);
                this.audioContext.resume();
            }
        }, 1000);
    }

    devices(callback) {
        var unityCallback = callback;

        this.refreshDevicesList((status, error) => {
            if (status === true) {
                UnityWebGLTools.callUnityCallback(unityCallback, { "status": true, "type": "devices", "data": UnityWebGLTools.objectToJSON({ "array": UnityMicrophone.INSTANCE.devicesList }) });
            } else {
                UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "devices", "data": error });
            }
        });
    }

    end(deviceId, callback) {
        // doesnt matter which device id
        if (!this.recording)
            return;

        this.recordingEndedCallback = callback;

        if (UnityMicrophone.AUDIO_WORKLET === true) {
            const isRecording = this.scriptProcessorNode.parameters.get('isRecording');
            isRecording.setValueAtTime(0, this.audioContext.currentTime);
        } else {
            this.recordEnded();
        }
    }

    getDeviceCaps(deviceId) {
        // doesnt matter which device id
        var array = [(UnityWebGLTools.isSafari() ? 44100 : 16000), 48000];
        return UnityWebGLTools.getPtrFromString(UnityWebGLTools.objectToJSON({ "array": array }))
    }

    isRecording(deviceId) {
        // doesnt matter which device id
        return this.recording ? 1 : 0;
    }

    start(deviceId, frequency, callback) {
        if (this.recording)
            return;

        this.frequency = frequency;
        this.recordingDevice = this.devicesList.find(item => item.deviceId === deviceId);
        this.recordingBuffer = [];

        this.recordingStartedCallback = callback;

        if (navigator.mediaDevices.getUserMedia) {
            var constraints = null;

            if (deviceId === null || !navigator.mediaDevices.getSupportedConstraints().deviceId) {
                constraints = {
                    audio: true,
                };
            } else {
                constraints = {
                    audio: {
                        deviceId: {
                            exact: deviceId
                        },
                        echoCancellation: true
                    }
                };
            }

            navigator.mediaDevices.getUserMedia(constraints).then((stream) => this.getUserMediaSuccessForRecording(stream)).catch((error) => this.getUserMediaFailedForRecording(error));
        }
    }

    requestPermission(callback) {
        // doesnt matter which device
        var unityCallback = callback;

        if (this.isPermissionGranted(null)) {
            UnityWebGLTools.callUnityCallback(unityCallback, { "status": true, "type": "requestPermission", "data": "granted" });
            return;
        }

        if (this.isSupported()) {
            navigator.mediaDevices.getUserMedia({ audio: true }).then(getUserMediaSuccess).catch(getUserMediaFailed);

            function getUserMediaSuccess(stream) {
                UnityWebGLTools.callUnityCallback(unityCallback, { "status": true, "type": "requestPermission", "data": "granted" });
            }

            function getUserMediaFailed(error) {
                UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "requestPermission", "data": error.message });
            }
        } else {
            UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "requestPermission", "data": "mediaDevices.getUserMedia isn't supported" });
        }
    }

    isPermissionGranted(callback) {
        var unityCallback = callback;

        this.refreshDevicesList((status, error) => {
            if (status === true) {
                if (this.devicesList.length > 0) {
                    if (!this.devicesList[0].isGrantedAccess) {
                        UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "isPermissionGranted", "data": "denied" });
                    } else {
                        UnityWebGLTools.callUnityCallback(unityCallback, { "status": true, "type": "isPermissionGranted", "data": "granted" });
                    }
                } else {
                    UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "isPermissionGranted", "data": "no devices connected" });
                }
            } else {
                UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "isPermissionGranted", "data": error });
            }
        });
    }

    recordEnded(){
        if(!this.recording)
            return;

        //this.download("recordedAudio.txt", JSON.stringify(this.recordingBuffer));

        if (this.recordingSource.mediaStream) {
            this.recordingSource.mediaStream
              .getTracks()
              .forEach((track) => track.stop());
        }

        this.recordingSource.disconnect(this.scriptProcessorNode);
        this.scriptProcessorNode.disconnect();
        this.scriptProcessorNode = null;
        this.recordingSource = null;
        this.recordingBuffer = null;
        this.recordingDevice = null;
        this.recording = false;

        UnityWebGLTools.callUnityCallback(UnityMicrophone.INSTANCE.recordingEndedCallback, { "status": true, "type": "end", "data": "recording ended" });
    }

    download(filename, text) {
        var element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', filename);
      
        element.style.display = 'none';
        document.body.appendChild(element);
      
        element.click();
      
        document.body.removeChild(element);
    }

    isSupported() {
        return (!!(navigator.mediaDevices.getUserMedia)) ? 1 : 0;
    }

    getUserMediaSuccessForRecording(stream) {
        this.recordingBuffer = [];

        this.recordingSource = this.audioContext.createMediaStreamSource(stream);

        if (UnityMicrophone.AUDIO_WORKLET === true) {
            this.scriptProcessorNode = new window.AudioWorkletNode(this.audioContext, 'microphone-worklet');
        } else {
            this.scriptProcessorNode = this.audioContext.createScriptProcessor(4096, 1, 1);
        }

        this.recordingSource.connect(this.scriptProcessorNode);
        this.scriptProcessorNode.connect(this.audioContext.destination);
        
        if (UnityMicrophone.AUDIO_WORKLET === true) {
            this.scriptProcessorNode.port.onmessage = (e) => this.audioNodeWorkletEventHandler(e);

            const isRecording = this.scriptProcessorNode.parameters.get('isRecording');
            isRecording.setValueAtTime(1, this.audioContext.currentTime);
        } else {
            this.scriptProcessorNode.onaudioprocess = (e) => this.audioNodeEventHandler(e);

            UnityWebGLTools.callUnityCallback(this.recordingStartedCallback, { "status": true, "type": "start", "data": "recording started" });
        }

        this.recording = true;
    }

    getUserMediaFailedForRecording(error) {
        UnityWebGLTools.callUnityCallback(this.recordingStartedCallback, { "status": false, "type": "start", "data": error });
    }

    // throws callback when resampling is complete
    changeBitrate(inputAudioBuffer, targetFrequency, callback) {
        
        if (inputAudioBuffer === null) {
            callback(false, null);
            return;
        }

        if (inputAudioBuffer.sampleRate === targetFrequency) {
            callback(true, inputAudioBuffer.getChannelData(0));
            return;
        }

        if (inputAudioBuffer.length < 64 || inputAudioBuffer.length > targetFrequency * 4) {
            callback(false, null);
            return;
        }

        if (this.targetFrequency < inputAudioBuffer.sampleRate) {
            this.downsampleBitrate(Object.values(inputAudioBuffer.getChannelData(0)), inputAudioBuffer.sampleRate, this.targetFrequency, callback);
            return;
        }

        try{
            var OfflineAudioContext = (window.OfflineAudioContext || window.webkitOfflineAudioContext);
            var offlineCtx = new OfflineAudioContext(inputAudioBuffer.numberOfChannels, inputAudioBuffer.duration * inputAudioBuffer.numberOfChannels * targetFrequency, targetFrequency);
            var buffer = offlineCtx.createBuffer(inputAudioBuffer.numberOfChannels, inputAudioBuffer.length, inputAudioBuffer.sampleRate);
            // copy the source data into the offline AudioBuffer
            for (var channel = 0; channel < inputAudioBuffer.numberOfChannels; channel++) {
                buffer.copyToChannel(inputAudioBuffer.getChannelData(channel), channel);
            }
            // resample it from the beginning.
            var source = offlineCtx.createBufferSource();
            source.buffer = inputAudioBuffer;
            source.connect(offlineCtx.destination);
            source.start(0);
            offlineCtx.oncomplete = function (e) {
                callback(true, e.renderedBuffer.getChannelData(0));
            }
            offlineCtx.startRendering();
        } catch(error){
            console.error(error);
            callback(false, null);
        }
    }

    downsampleBitrate(samples, sourceFrequency, targetFrequency, callback){
        if (samples === null) {
            callback(false, samples);
            return;
        }

        if (sourceFrequency === targetFrequency) {
            callback(true, samples);
            return;
        }

        var sampleRateRatio = sourceFrequency / targetFrequency;
        var newLength = Math.round(samples.length / sampleRateRatio);
        var result = new Float32Array(newLength);
        var offsetResult = 0;
        var offsetBuffer = 0;
        while (offsetResult < result.length) {
            var nextOffsetBuffer = Math.round((offsetResult + 1) * sampleRateRatio);
            var accum = 0,
            count = 0;
            for (var i = offsetBuffer; i < nextOffsetBuffer && i < samples.length; i++) {
                accum += samples[i];
                count++;
            }
            result[offsetResult] = accum / count;
            offsetResult++;
            offsetBuffer = nextOffsetBuffer;
        }

        callback(true, result);
    }

    // returns block of samples from recorded buffer
    getRecordingBuffer(callback) {
        if (!this.isRecording()) {
            UnityWebGLTools.callUnityCallback(unityCallback, { "status": false, "type": "getRecordingBuffer", "data": "recording isnt started" });
            return;
        }

        var unityCallback = callback;
        UnityWebGLTools.callUnityCallback(unityCallback, { "status": true, "type": "getRecordingBuffer", "data": UnityWebGLTools.objectToJSON({ "array": this.recordingBuffer }) });
    }

    setRecordingBufferCallback(callback) {
        this.recordingBufferCallback = callback;
    }

    // handling media stream and filling buffer
    audioNodeEventHandler(e) { 
        if(!this.recording)
            return;

        this.changeBitrate(e.inputBuffer, this.frequency, (status, channelData) => {
            if (status === true) {
                if(!this.recording)
                    return;

                this.recordingBuffer = this.recordingBuffer.concat(Object.values(channelData));
                
                UnityWebGLTools.callUnityCallback(this.recordingBufferCallback, {
                    data: channelData,
                    length: channelData.length
                });
            } else {
                console.log("resampling cannot be done");
            }
        });
    }

    audioNodeWorkletEventHandler(e) {
        switch (e.data.eventType) {
            case "data":
                {
                    const audioData = e.data.audioBuffer;
                    const inputBuffer = this.audioContext.createBuffer(1, audioData.length, this.audioContext.sampleRate);
                    const nowBuffering = inputBuffer.getChannelData(0);
                    for (var i = 0; i < audioData.length; i++) {
                        nowBuffering[i] = audioData[i];
                    }

                    this.changeBitrate(inputBuffer, this.frequency, (status, channelData) => {
                        if (status === true) {

                            if (!this.recording)
                                return;

                            this.recordingBuffer = this.recordingBuffer.concat(channelData);

                            UnityWebGLTools.callUnityCallback(this.recordingBufferCallback, {
                                data: channelData,
                                length: channelData.length
                            });
                        } else {
                            console.log("resampling cannot be done");
                        }
                    });
                }
                break;
            case "stop":
                {
                    this.recordEnded();
                }
                break;
            case "start":
                {
                    UnityWebGLTools.callUnityCallback(this.recordingStartedCallback, { "status": true, "type": "start", "data": "recording started" });
                }
                break;
        }
    }

    // refreshes devices list
    refreshDevicesList(callback) {
        if (!navigator.mediaDevices.enumerateDevices) {
            callback(false, "enumerateDevices() not supported");
            return;
        }

        navigator.mediaDevices.enumerateDevices()
            .then(function (devices) {
                var outputDevicesArr = [];

                for (var i = 0; i < devices.length; i++) {
                    if (devices[i].kind === "audioinput") {
                        var deviceInfo = {
                            deviceId: devices[i].deviceId,
                            kind: devices[i].kind,
                            label: devices[i].label,
                            groupId: devices[i].groupId,
                            isGrantedAccess: true
                        };

                        if (deviceInfo.label === undefined || deviceInfo.label === null || deviceInfo.label.length === 0) {
                            deviceInfo.label = "Microphone " + (outputDevicesArr.length + 1);
                            deviceInfo.isGrantedAccess = false;
                        }

                        outputDevicesArr.push(deviceInfo);
                    }
                }

                UnityMicrophone.INSTANCE.devicesList = outputDevicesArr;

                callback(true, null);
            })
            .catch(function (error) {
                callback(false, ("get devices exception: " + error.name + ": " + error.message + "; " + error.stack));
            });
    }
}

class MicrophoneWorkletProcessor extends AudioWorkletProcessor {
    static get parameterDescriptors() {
        return [{
          name: 'isRecording',
          defaultValue: 0
        }];
      }
    
      constructor() {
        super();
        this._bufferSize = 4096;
        this._buffer = new Float32Array(this._bufferSize);
        this._initBuffer();

        this.recording = false;
      }
    
      _initBuffer() {
        this._bytesWritten = 0;
      }
    
      _isBufferEmpty() {
        return this._bytesWritten === 0;
      }
    
      _isBufferFull() {
        return this._bytesWritten === this._bufferSize;
      }
    
      _appendToBuffer(value) {
        if (this._isBufferFull()) {
          this._flush();
        }
    
        this._buffer[this._bytesWritten] = value;
        this._bytesWritten += 1;
      }
    
      _flush() {
        let buffer = this._buffer;
        if (this._bytesWritten < this._bufferSize) {
          buffer = buffer.slice(0, this._bytesWritten);
        }
    
        this.port.postMessage({
          eventType: 'data',
          audioBuffer: buffer
        });
    
        this._initBuffer();
      }
    
      _recordingStopped() {
        this.port.postMessage({
          eventType: 'stop'
        });
        this.recording = false;
      }

      _recordingStarted() {
        this.port.postMessage({
          eventType: 'start'
        });
        this.recording = true;
      }
    
      process(inputs, outputs, parameters) {
        const isRecordingValues = parameters.isRecording;           

        for (let i = 0; i < isRecordingValues.length; i++) {
          const shouldRecord = isRecordingValues[i] === 1;     

          if(shouldRecord && !this.recording){
            this._recordingStarted();
          }

          if (!shouldRecord && !this._isBufferEmpty()) {
            this._flush();  
          }

          if(!shouldRecord && this.recording){
            this._recordingStopped();
          }
      
          if (this.recording) {        
            if(inputs.length > 0){
                if(inputs[0].length > 0){
                  this._appendToBuffer(inputs[0][0][i]);
                }
            }
          }
        }
  
        return true;
      }
    }
    
    registerProcessor('microphone-worklet', MicrophoneWorkletProcessor);
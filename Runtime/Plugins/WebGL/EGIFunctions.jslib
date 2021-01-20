var EGIFunctions = {
  $EGIState: {
    isPlumbingSetUp: false,
    ensurePlumbing: function () {
      if (this.isPlumbingSetUp) return;
      var self = this;

      PlayEGI.onSignal(function (signal) {
        if (self.onSignal != null) {
          Runtime.dynCall('vi', self.onSignal, [self._stringToPtr(JSON.stringify(signal))]);
        }
      });

      this.isPlumbingSetUp = true;
    },
    onSignal: null,
    _stringToPtr: function (str) {
      var len = lengthBytesUTF8(str) + 1;
      var buffer = _malloc(len);
      stringToUTF8(str, buffer, len);
      return buffer;
    }
  },
  Register: function(onStep, onRelease, onSensoState) {
    EGIState.ensurePlumbing();
  },
  RegisterPlumbing: function(onSignal) {
    EGIState.onSignal = onSignal;

    EGIState.ensurePlumbing();
  },
  Ready: function() {
    PlayEGI.ready();
  },
  Pong: function() {
    PlayEGI.pong();
  },
  UnmarshalFinish: function(strPtrMetrics, strPtrMemory) {
    PlayEGI.finish(JSON.parse(Pointer_stringify(strPtrMetrics)), JSON.parse(Pointer_stringify(strPtrMemory)));
  },
  SendMotorPreset: function(keywordPtr) {
    PlayEGI.motor(Pointer_stringify(keywordPtr));
  }
};

autoAddDeps(EGIFunctions, '$EGIState');
mergeInto(LibraryManager.library, EGIFunctions);

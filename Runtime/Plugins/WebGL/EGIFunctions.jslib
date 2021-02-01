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
  Register: function(onSignal) {
    EGIState.onSignal = onSignal;

    EGIState.ensurePlumbing();
  },
  Command: function(strPtrCommand) {
    PlayEGI.send(JSON.parse(Pointer_stringify(strPtrCommand)));
  }
};

autoAddDeps(EGIFunctions, '$EGIState');
mergeInto(LibraryManager.library, EGIFunctions);

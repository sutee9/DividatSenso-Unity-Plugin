var EGIFunctions = {
  $EGIState: {
    isPlumbingSetUp: false,
    ensurePlumbing: function () {
      if (this.isPlumbingSetUp) return;
      var self = this;

      var directionMap = {
        "Center": 0, "Up": 1, "Right": 2, "Down": 3, "Left": 4
      };

      PlayEGI.onSignal(function (signal) {
        if (signal.type === 'Hello' && self.onHello != null) {
          Runtime.dynCall('vi', self.onHello, [self._stringToPtr(JSON.stringify(signal.settings))]);
        } else if (signal.type === 'Ping' && self.onPing != null) {
          Runtime.dynCall('v', self.onPing, []);
        } else if (signal.type === 'Suspend' && self.onSuspend != null) {
          Runtime.dynCall('v', self.onSuspend, []);
        } else if (signal.type === 'Resume' && self.onResume != null) {
          Runtime.dynCall('v', self.onResume, []);
        } else if (signal.type === 'Step' && self.onStep != null) {
          Runtime.dynCall('vi', self.onStep, [directionMap[signal.direction]]);
        } else if (signal.type === 'Release' && self.onRelease != null) {
          Runtime.dynCall('vi', self.onRelease, [directionMap[signal.direction]]);
        } else if (signal.type === 'SensoState' && self.onSensoState != null) {
          Runtime.dynCall('vifff', self.onSensoState, [directionMap["Center"], signal.state.center.x, signal.state.center.y, signal.state.center.f]);
          Runtime.dynCall('vifff', self.onSensoState, [directionMap["Up"], signal.state.up.x, signal.state.up.y, signal.state.up.f]);
          Runtime.dynCall('vifff', self.onSensoState, [directionMap["Right"], signal.state.right.x, signal.state.right.y, signal.state.right.f]);
          Runtime.dynCall('vifff', self.onSensoState, [directionMap["Down"], signal.state.down.x, signal.state.down.y, signal.state.down.f]);
          Runtime.dynCall('vifff', self.onSensoState, [directionMap["Left"], signal.state.left.x, signal.state.left.y, signal.state.left.f]);
        }
      });

      this.isPlumbingSetUp = true;
    },
    onHello: null,
    onPing: null,
    onSuspend: null,
    onResume: null,
    onStep: null,
    onRelease: null,
    onSensoState: null,
    _stringToPtr: function (str) {
      var len = lengthBytesUTF8(str) + 1;
      var buffer = _malloc(len);
      stringToUTF8(str, buffer, len);
      return buffer;
    }
  },
  Register: function(onStep, onRelease, onSensoState) {
    EGIState.onStep = onStep;
    EGIState.onRelease = onRelease;
    EGIState.onSensoState = onSensoState;

    EGIState.ensurePlumbing();
  },
  RegisterPlumbing: function(onHello, onPing, onSuspend, onResume) {
    EGIState.onHello = onHello;
    EGIState.onPing = onPing;
    EGIState.onSuspend = onSuspend;
    EGIState.onResume = onResume;

    EGIState.ensurePlumbing();
  },
  Ready: function() {
    PlayEGI.ready();
  },
  Pong: function() {
    PlayEGI.pong();
  },
  UnmarshalFinish: function(strPtr) {
    PlayEGI.finish(JSON.parse(Pointer_stringify(strPtr)));
  },
  SendMotorPreset: function(keywordPtr) {
    PlayEGI.motor(Pointer_stringify(keywordPtr));
  }
};

autoAddDeps(EGIFunctions, '$EGIState');
mergeInto(LibraryManager.library, EGIFunctions);

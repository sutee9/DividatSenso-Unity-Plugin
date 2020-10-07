// Dividat Play EGI
;(function() {
    var play = null

    // Helper to send commands
    function sendCommand(cmd) {
        if (play) {
            play.postMessage(cmd, '*')
        }
    }

    var signalHandler = null

    // Setup listener for messages from Play
    window.addEventListener('message', function(event) {
        var signal = event.data
        play = event.source

        switch (signal.type) {
            case 'SetupEGI':
                // This signal is only to setup the interface it is not forwarded to consumers
                break

            default:
                if (signalHandler) {
                    signalHandler(signal)
                }
                break
        }
    })

    // Add an error handler
    window.onerror = function(message, source, lineno, colno, error) {
        sendCommand({
            type: 'Error',
            error: {
                message: message,
                source: source,
                lineno: lineno,
                colno: colno,
                error: error.toString(),
                stack:
                    typeof error.stack === 'string'
                        ? error.stack.split('\n')
                        : null
            }
        })
    }

    window.PlayEGI = {
        ready: () => {
            sendCommand({ type: 'Ready' })
        },
        pong: () => {
            sendCommand({ type: 'Pong' })
        },
        finish: (metrics, memory) => {
            metrics = metrics || {}
            sendCommand({ type: 'Finish', metrics: metrics, memory: memory })
        },
        onSignal: cb => {
            signalHandler = cb
        },
        led: settings => {
            sendCommand({ type: 'Led', settings: settings })
        },
        motor: presetOrSettings => {
            switch (typeof presetOrSettings) {
                case 'string':
                    sendCommand({ type: 'Motor', preset: presetOrSettings })
                    break
                default:
                    sendCommand({ type: 'Motor', settings: presetOrSettings })
            }
        }
    }
})()

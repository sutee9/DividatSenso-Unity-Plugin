using System.IO.Ports;
using System.Threading;
using System;

namespace Dividat.TouchRail {
    public class ArduinoSerial
    {
        string port;
        SerialPort serial;

        public string LastLine {
            get {
                return _lastLine;
            }
        }
        private string _lastLine;
        private bool _connected;
        
        public event ArduinoNotification OnLineReceived;
        public delegate void ArduinoNotification();

        //Main Thread Variables
        private Thread readThread;
        private bool setup_ok;

        public ArduinoSerial(string serialPort, int baud=9200, bool autoConnect = true){
            #if UNITY_EDITOR || !UNITY_WEBGL

            port = serialPort;
            serial = new SerialPort(serialPort, baud);
            serial.ReadTimeout = 500;
            setup_ok = true;
            if (autoConnect){
                Connect();
            }
                
            #else
            Console.WriteLine("ArduinoSerial is not supported on this platform");
            #endif
        }

        public void Connect(){
            #if UNITY_EDITOR || !UNITY_WEBGL
            if (setup_ok){
                if (!serial.IsOpen)
                    serial.Open ();
                _connected = true;
                readThread = new Thread(ReadLine);
                readThread.IsBackground = true;
                readThread.Start();
            }
            #endif
        }

       private void ReadLine(){
            while (_connected){
                try {
                    _lastLine = serial.ReadLine();
                    OnLineReceived?.Invoke();
                }
                catch (TimeoutException) { }
            }      
        }

        public void Close(){
            #if UNITY_EDITOR || !UNITY_WEBGL
            if (setup_ok){
                _connected = false;
                if (readThread != null){
                    readThread.Join();
                }
                serial.Close();
            }
            #endif
        }
    }
}

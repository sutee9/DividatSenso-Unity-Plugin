using System.IO.Ports;
using System.Threading;
using System;

namespace Dividat.TouchRail {
    public class ArduinoConnection
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


        public ArduinoConnection(string serialPort, int baud=9200, bool autoConnect = true){
            port = serialPort;
            serial = new SerialPort(serialPort, baud);
            serial.ReadTimeout = 500;
            if (autoConnect){
                Connect();
            }
        }

        public void Connect(){
            if (!serial.IsOpen)
				serial.Open ();
            _connected = true;
            readThread = new Thread(ReadLine);
            readThread.IsBackground = true;
            readThread.Start();
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
            _connected = false;
            readThread.Join();
            serial.Close();
        }
    }
}

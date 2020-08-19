using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR || !UNITY_WEBGL
using System.IO.Ports;
#endif

namespace Dividat.TouchRail {
    public class TouchRailVisualizerArduino : MonoBehaviour
    {
        [Header("Touch Rail Behaviour and Connection")]
        public string comPort = "/dev/tty.usbmodem14101";

        [Range(1f, 300f)]
        public float thresholdTouched = 30f;

        [Range(60f, 600f)]
        public float thresholdPressed = 200f;

        [Header("Component Setup")]
        public GameObject[] touchZones;
        
        //Private Status Vars
        private ArduinoSerial _arduino;
        private int _lastUpdatedFrame = -1;

        // Start is called before the first frame update
        void Start()
        {
            try {
                _arduino = new ArduinoSerial(comPort, 115200);
            }
            catch (Exception e){
                Debug.LogWarning("[TouchRail] Failed to establish connection to TouchRail: " + e.Message);
            }
        }

        void Update(){
            if (_arduino != null){
                string railRaw = _arduino.LastLine;
                if (railRaw != null && railRaw != ""){
                    string[] values = railRaw.Split(',');
                    if (values.Length != 6){
                        Debug.Log(values.Length);
                    }
                    else {
                        _lastUpdatedFrame = Time.frameCount;

                        for (int i=0; i < values.Length; i++){
                            float pressureValue = float.Parse(values[i]);
                            if (pressureValue > thresholdPressed){
                                touchZones[i-2].transform.localScale = new Vector3(0.20f, 0.75f, 0.20f);
                                Debug.Log("active=" + i + " value= " + values[i]);
                            }
                            else if (pressureValue > thresholdTouched){
                                touchZones[i-2].transform.localScale = new Vector3(
                                    0.4f - 0.2f*(pressureValue-thresholdTouched)/(thresholdPressed-thresholdTouched), 
                                    0.75f, 
                                    0.4f - 0.2f*(pressureValue-thresholdTouched)/(thresholdPressed-thresholdTouched)
                                );
                                Debug.Log("active=" + i + " value= " + values[i]);
                            }
                            else {
                                touchZones[i-2].transform.localScale = new Vector3(0.4f, 0.75f, 0.4f);
                            }
                        }
                    }
                }
            }
        }

        [ContextMenu("List Ports")]
        public void ListPorts(){
            #if UNITY_EDITOR || !UNITY_WEBGL
            Debug.Log("Available Port Names");
            foreach (string s in SerialPort.GetPortNames())
            {
                Debug.Log(s);
            }
            #else
            Debug.Log("List Ports is not supported on this platform");
            #endif
        }

        public void OnDestroy(){
            if (_arduino != null){
                _arduino.Close();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

namespace Dividat.TouchRail {
    public class TouchRail : MonoBehaviour
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
        private ArduinoConnection _arduino;
        private int _lastUpdatedFrame = -1;

        // Start is called before the first frame update
        void Start()
        {
            ListPorts();
            _arduino = new ArduinoConnection(comPort, 115200);
            // _arduino.OnLineReceived+= UpdateTouchRailStatus;
        }

        // void UpdateTouchRailStatus(){
        //     Debug.Log("Update in frame" + _lastUpdatedFrame);
        // }

        void Update(){
            string railRaw = _arduino.LastLine;
            if (railRaw != null && railRaw != ""){
                string[] values = railRaw.Split(',');
                if (values.Length != 8){
                    Debug.Log(values.Length);
                }
                else {
                    _lastUpdatedFrame = Time.frameCount;

                    for (int i=2; i < values.Length; i++){
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

        [ContextMenu("List Ports")]
        public void ListPorts(){
            Debug.Log("Available Port Names");
            foreach (string s in SerialPort.GetPortNames())
            {
                Debug.Log(s);
            }
        }

        public void OnDestroy(){
            if (_arduino != null){
                _arduino.Close();
            }
        }
    }
}

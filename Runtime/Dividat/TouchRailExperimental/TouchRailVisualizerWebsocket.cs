using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

[RequireComponent(typeof(TouchRailConnectionWebsocket))]
public class TouchRailVisualizerWebsocket : MonoBehaviour
{
    public enum Side {left, right}
    [Header("Configuration")]
    public Side side = Side.left;
    
    [Header("Component Setup")]
    public GameObject[] touchZones;
    public float thresholdPressed = 0.5f;
    [Range(0f, 1f)]
    public float thresholdTouched = 0.2f;

    private TouchRailConnectionWebsocket _input;

    // Start is called before the first frame update
    void Awake()
    {
        _input = GetComponent<TouchRailConnectionWebsocket>();
    }

    void Update(){
        float[] inputValues = null;
        if (side == Side.left){
            inputValues = _input.LeftValues;
        }
        else {
            inputValues = _input.RightValues;
        }
        for (int i=0; i < inputValues.Length; i++){
            float pressureValue = inputValues[i];
            if (pressureValue > thresholdPressed){
              touchZones[i].transform.localScale = new Vector3(0.20f, 0.75f, 0.20f);
            }
            else if (pressureValue > thresholdTouched){
              touchZones[i].transform.localScale = new Vector3(
                  0.4f - 0.2f*(pressureValue), 
                  0.75f, 
                  0.4f - 0.2f*(pressureValue)
              );
            }
            else {
              touchZones[i].transform.localScale = new Vector3(0.4f, 0.75f, 0.4f);
            }
        }
    }
}

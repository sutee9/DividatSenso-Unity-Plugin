using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class HandlaufWS : MonoBehaviour
{
  WebSocket _websocket;
  int _lastUpdatedFrame = 0;
  [Header("Network Configuration")]
  public string serverURL = "ws://echo.websocket.org";
  
  [Header("Component Setup")]
  public GameObject[] touchZones;
  [Range(0f, 1f)]
  public float thresholdPressed = 0.5f;
  [Range(0f, 1f)]
  public float thresholdTouched = 0.2f;


  // Start is called before the first frame update
  async void Start()
  {
      Connect();
  }

  async void Connect(){
    Connect(serverURL);
  }

  async void Connect(string url){
    _websocket = new WebSocket(url);

    _websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
    };

    _websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    _websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
    };

    _websocket.OnMessage += (bytes) =>
    {
      // Reading a plain text message
      var message = System.Text.Encoding.UTF8.GetString(bytes);
      //Debug.Log("OnMessage! " + message);
      string msg = message.Remove(0, 1);
      msg = msg.Remove(msg.Length-1, 1);
      string[] values = msg.Split(',');
      
      if (values != null && values.Length == 6){
        if (Time.frameCount != _lastUpdatedFrame){
          _lastUpdatedFrame = Time.frameCount;

          for (int i=0; i < values.Length; i++){

            float pressureValue = float.Parse(values[i]);
            if (pressureValue > thresholdPressed){
              touchZones[i].transform.localScale = new Vector3(0.20f, 0.75f, 0.20f);
              
            }
            else if (pressureValue > thresholdTouched){
              touchZones[i].transform.localScale = new Vector3(
                  0.4f - 0.2f*(pressureValue-thresholdTouched)/(thresholdPressed-thresholdTouched), 
                  0.75f, 
                  0.4f - 0.2f*(pressureValue-thresholdTouched)/(thresholdPressed-thresholdTouched)
              );
            }
            else {
              touchZones[i].transform.localScale = new Vector3(0.4f, 0.75f, 0.4f);
            }
          }
        }
      }
      else {
        Debug.LogWarning("Received Invalid Message. Should contain 6 values. Message="+message);
      }
    };
    await _websocket.Connect();
  }

  void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
    #endif
  }

  async void SendWebSocketTextMessage(string message)
  {
    if (_websocket.State == WebSocketState.Open)
    {
      await _websocket.SendText(message);
    }
  }

  async void SendWebSocketBinaryMessage(byte[] binaryMessage)
  {
    if (_websocket.State == WebSocketState.Open)
    {
        // Sending bytes
      await _websocket.Send(binaryMessage);
      }
  }

  private async void OnApplicationQuit()
  {
      await _websocket.Close();
  }
}

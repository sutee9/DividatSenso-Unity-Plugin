using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class TouchRailConnectionWebsocket : MonoBehaviour
{
    WebSocket _websocket;
    int _lastUpdatedFrame = 0;
    [Header("Network Configuration")]
    public string serverURL = "ws://localhost:54535";
    public bool connectAtStartup;
    public bool automaticReconnect = true;
    [Range(0.2f, 10f)]
    public float retryEvery = 2f;

    [Header("Visualization")]
    [SerializeField]
    private float[] _leftValues;
    [SerializeField]
    private float[] _rightValues;
    public float[] LeftValues {
        get { return _leftValues; }
    }
    public float[] RightValues {
        get { return _rightValues; }
    }

    public bool Connected {
        get { return _connected;} 
    }
    private bool _connected = false;
    private float _retryTimer = 0f;

    void Awake(){
        _leftValues = new float[6];
        _rightValues = new float[6];
    }

    // Start is called before the first frame update
    void Start()
    {
        if (connectAtStartup){
            Connect();
        }
    }

    ///Note that the connection request is asynchronous. You can not expect values right away, but only after connected is true.
    public void Connect(){
        ConnectSocket();
    }
    void ConnectSocket(){
        Connect(serverURL);
    }

    async void Connect(string url){
        _websocket = new WebSocket(url);

        _websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            _connected = true;

        };

        _websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            _connected = false;
            _retryTimer = 0f;
        };

        _websocket.OnMessage += (bytes) =>
        {
            if (Time.frameCount != _lastUpdatedFrame){
                
                _lastUpdatedFrame = Time.frameCount;
                // Reading a plain text message
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log(message);
                string msg = message.Remove(0, 1);
                msg = msg.Remove(msg.Length-1, 1);
                string[] values = msg.Split(',');
                
                if (values != null && values.Length == 6){
                    for (int i=0; i < values.Length; i++){
                        float pressureValue = float.Parse(values[i]);
                        _leftValues[i] = pressureValue;
                    } 
                }
                else if (values != null && values.Length == 12){
                    for (int i=0; i < values.Length; i++){
                        float pressureValue = float.Parse(values[i]);
                        if (i < 6){
                            _leftValues[i] = pressureValue;
                        }
                        else {
                            _rightValues[i-6] = pressureValue;
                        }
                    } 
                }
                else {
                    Debug.LogWarning("Received Invalid Message. Should contain 6 or 12 values. Message="+message);
                }
            }
            _retryTimer = 0f;
        };
        await _websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            _websocket.DispatchMessageQueue();
        #endif

        if (automaticReconnect && !_connected){
            //Debug.Log("Not Connected " + _retryTimer);
            _retryTimer += Time.deltaTime;
            if (_retryTimer >= retryEvery){
                Debug.Log("[TouchRailConnectionWebsocket] Trying Reconnect");
                _retryTimer=0f;
                _websocket.Connect();
            }
        }
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

    private async void OnDestroy()
    {
        await _websocket.Close();
    }
}
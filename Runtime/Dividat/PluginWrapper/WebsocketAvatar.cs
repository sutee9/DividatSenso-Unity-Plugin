using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Dividat;

using NativeWebSocket;
using UnityEditor;

namespace Dividat {
#if UNITY_EDITOR
    /**
     * While in the Editor, the Websocket Avatar, aka "Senso Editor Companion", automatically starts and tries to establish
     * a connection to Senso.
     * 
     * In order for this connection to work, the user must complete the following.
     * 
     * 1. Install the Dividat Senso Driver from Dividat
     * 2. Connect and test the connection using google chrome.
     * 3. Start the "Companion" from the unity editor: Dividat > Senso Editor Companion. Make sure the editor runs in Chrome!
     */
    public class WebsocketAvatar : MonoBehaviour
    {
        WebSocket _websocket;
        [Header("Network Configuration")]
        public string serverURL = "wss://rooms.dividat.com/rooms";
        public string roomName = "";
        public string joinCommand = "join";
        public bool automaticReconnect = true;
        [Range(0.2f, 10f)]
        public float retryEvery = 5f;

        public bool Connected
        {
            get { return _connected; }
        }
        private bool _connected = false;
        private float _retryTimer = 0f;

        public static WebsocketAvatar Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WebsocketAvatar>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "Senso-LiveConnection";
                        _instance = obj.AddComponent<WebsocketAvatar>();
                    }
                }
                return _instance;
            }
        }
        private static WebsocketAvatar _instance;

        public void Register()
        {
        }

        protected void Awake()
        {
            if (!EditorPrefs.HasKey(SensoEditorSettings.COMP_ROOM_KEY)) { 
                EditorPrefs.SetString(SensoEditorSettings.COMP_ROOM_KEY, "unity-avatar-" + UnityEngine.Random.Range(1000000, 10000000));
            }
            roomName = EditorPrefs.GetString(SensoEditorSettings.COMP_ROOM_KEY);

            //Check the singleton is unique
            if (_instance == null)
            {
                _instance = WebsocketAvatar.Instance;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("[Senso Editor Companion] Starting EGI");
            Connect();
        }

        ///Note that the connection request is asynchronous. You can not expect values right away, but only after connected is true.
        public void Connect()
        {
            ConnectSocket();
        }
        void ConnectSocket()
        {
            Connect(serverURL + "/"+ roomName + "/" + joinCommand);
        }

        async void Connect(string url)
        {
            _websocket = new WebSocket(url);

            _websocket.OnOpen += () =>
            {
                Debug.Log("[Senso Editor Companion] Avatar connection open!");
                _connected = true;

            };

            _websocket.OnError += (e) =>
            {
                Debug.Log("Error in EGI WS avatar: " + e);
            };

            _websocket.OnClose += (e) =>
            {
                //Debug.Log("EGI WS avatar connection closed!");
                _connected = false;
                _retryTimer = 0f;
            };

            _websocket.OnMessage += (bytes) =>
            {
                // Reading a plain text message
                var message = System.Text.Encoding.UTF8.GetString(bytes);

                Play.ProcessSignal(message);

                _retryTimer = 0f;
            };
            await _websocket.Connect();
        }

        void Update()
        {
            if (_websocket != null)
            {
    #if !UNITY_WEBGL || UNITY_EDITOR
                _websocket.DispatchMessageQueue();
    #endif

                if (automaticReconnect && !_connected)
                {
                    _retryTimer += Time.deltaTime;
                    if (_retryTimer >= retryEvery)
                    {
                        _retryTimer = 0f;
                         #pragma warning disable CS4014 //Do not wait in Update method, else unity main thread will block.
                        _websocket.Connect();
                        #pragma warning restore CS4014
                    }
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
#endif
}

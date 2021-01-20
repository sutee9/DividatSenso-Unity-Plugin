using AOT;
using System;
using System.Runtime.InteropServices;

using UnityEngine;
using SimpleJSON;
using Dividat;


namespace Dividat
{
    public static class Play
    {

        private static IPlayBehaviour gameController;

        public static void Init(IPlayBehaviour gameController)
        {
            if (Play.gameController == null)
            {
                Play.gameController = gameController;
                Play.Wire();
                Debug.Log("Dividat.Play is now wired");
            }
            else
            {
                throw new Exception("Dividat Play already initialized.");
            }
        }

        [DllImport("__Internal")]
        public static extern void Ready();

        [DllImport("__Internal")]
        public static extern void Pong();

        [DllImport("__Internal")]
        public static extern void UnmarshalFinish(string jsonString, string memoryString);

        public static void Finish(Metrics metrics)
        {
            Debug.Log("Finish with Memory null");
            Finish(metrics, "null");
        }

        public static void Finish(Metrics metrics, string memory)
        {
            Debug.Log("Finish with Memory " + memory);
            #if !UNITY_EDITOR
            UnmarshalFinish(metrics.toJSONString(), memory);
            #endif
        }

        private static void Wire()
        {
            if (gameController == null) return;

            Dividat.Hardware.Wire();

            #if UNITY_WEBGL && !UNITY_EDITOR
            RegisterPlumbing(OnSignal);
            #endif
        }

        [DllImport("__Internal")]
        private static extern void RegisterPlumbing(SignalCallback onSignal);

        public delegate void SignalCallback(System.IntPtr signalJsonPtr);

        [MonoPInvokeCallback(typeof(SignalCallback))]
        private static void OnSignal(System.IntPtr signalJsonPtr)
        {
            if (gameController != null)
            {
                string jsonSignal = Marshal.PtrToStringAuto(signalJsonPtr);
                Debug.Log("Play->On Signal: Received Signal\nJSON Raw:\n" + jsonSignal);
                ProcessSignal(jsonSignal);
            }
            else {
                Debug.Log("No Game Controller was set up");
            }
        }

        public static void ProcessSignal(string jsonString)
        {
            JSONNode json = JSON.Parse(jsonString);

            switch (json["type"].Value)
            {
                case "Hello":
                    gameController.OnHello(Settings.FromString(json["settings"].Value), json["memory"].Value);
                    break;
                case "Ping":
                    gameController.OnPing();
                    break;
                case "Suspend":
                    gameController.OnSuspend();
                    break;
                case "Resume":
                    gameController.OnResume();
                    break;
                case "Step":
                    Hardware.OnStep(directionToString(json["direction"].Value));
                    break;
                case "Release":
                    Hardware.OnRelease(directionToString(json["direction"].Value));
                    break;
                case "SensoState":
                    foreach (var dir in json["state"].Keys)
                    {
                        JSONNode plate = json["state"][dir];
                        Hardware.OnSensoState(directionToString(dir), plate["x"].AsFloat, plate["y"].AsFloat, plate["f"].AsFloat);
                    }
                    break;
            }
        }

        private static Direction directionToString(string dir)
        {
            return (Direction)Enum.Parse(typeof(Direction), dir, true);
        }

    }
}

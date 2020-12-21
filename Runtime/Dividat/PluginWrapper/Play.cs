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
            Finish(metrics, "null");
        }

        public static void Finish(Metrics metrics, string memory)
        {
            #if !UNITY_EDITOR
            UnmarshalFinish(metrics.toJSONString(), memory);
            #endif
        }

        private static void Wire()
        {
            if (gameController == null) return;

            Dividat.Hardware.Wire();

#if UNITY_WEBGL && !UNITY_EDITOR
            RegisterPlumbing(OnHello, OnPing, OnSuspend, OnResume);
#endif
        }

        [DllImport("__Internal")]
        private static extern void RegisterPlumbing(HelloCallback onHello, PingCallback onPing, SuspendCallback onSuspend, ResumeCallback onResume);

        public delegate void HelloCallback(System.IntPtr settingsPtr, System.IntPtr memoryPtr);
        public delegate void PingCallback();
        public delegate void SuspendCallback();
        public delegate void ResumeCallback();

        [MonoPInvokeCallback(typeof(HelloCallback))]
        private static void OnHello(System.IntPtr settingsPtr, System.IntPtr memoryPtr)
        {
            if (gameController != null)
            {
                string settings = Marshal.PtrToStringAuto(settingsPtr);
                string memory = Marshal.PtrToStringAuto(memoryPtr);
                Debug.Log("play settings");
                gameController.OnHello(Settings.FromString(settings), JSON.Parse(memory));
            }
            else {
                Debug.Log("No Game Controller was set up");
            }
        }

        [MonoPInvokeCallback(typeof(PingCallback))]
        private static void OnPing()
        {
            if (gameController != null)
            {
                gameController.OnPing();
            }

        }

        [MonoPInvokeCallback(typeof(SuspendCallback))]
        private static void OnSuspend()
        {
            if (gameController != null)
            {
                gameController.OnSuspend();
            }

        }

        [MonoPInvokeCallback(typeof(ResumeCallback))]
        private static void OnResume()
        {
            if (gameController != null)
            {
                gameController.OnResume();
            }

        }

    }
}

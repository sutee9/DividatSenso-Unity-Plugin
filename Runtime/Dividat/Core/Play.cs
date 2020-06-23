using AOT;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;


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
        public static extern void UnmarshalFinish(string jsonString);

        public static void Finish(Metrics metrics)
        {
            #if !UNITY_EDITOR
            UnmarshalFinish(metrics.toJSONString());
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

        public delegate void HelloCallback(System.IntPtr ptr);
        public delegate void PingCallback();
        public delegate void SuspendCallback();
        public delegate void ResumeCallback();

        [MonoPInvokeCallback(typeof(HelloCallback))]
        private static void OnHello(System.IntPtr ptr)
        {
            if (gameController != null)
            {
                string settings = Marshal.PtrToStringAuto(ptr);
                gameController.OnHello(Settings.FromString(settings));
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

    public abstract class Setting
    {
        public class Time : Setting
        {
            public Time(float t) { value = t; }
            public readonly float value;
        }

        public class Float : Setting
        {
            public Float(float f) { value = f; }
            public readonly float value;
        }

        public class Percentage : Setting
        {
            public Percentage(float f) { value = f; }
            public readonly float value;
        }


        public class String : Setting
        {
            public String(string s) { value = s; }
            public readonly string value;
        }

        public class Json : Setting
        {
            public Json(string t, JSONNode j) { type = t; value = j; }
            public readonly string type;
            public readonly JSONNode value;
        }

        public static Setting FromJSON(JSONNode json)
        {
            switch (json["type"].Value)
            {
                case "Time":
                    return new Time(json["value"].AsFloat);
                case "Float":
                    return new Float(json["value"].AsFloat);
                case "Percentage":
                    return new Percentage(json["value"].AsFloat);
                case "String":
                    return new String(json["value"].Value);
                default:
                    return new Json(json["type"].Value, json["value"]);
            }
        }
    }

    public class Settings : Dictionary<string, Setting>
    {
        public static Settings FromJSON(JSONNode json)
        {
            Settings settings = new Settings();
            foreach (var key in json.Keys)
            {
                settings.Add(key, Setting.FromJSON(json[key]));
            }
            return settings;
        }

        public static Settings FromString(string jsonString)
        {
            return FromJSON(JSON.Parse(jsonString));
        }
    }

    public abstract class Metric
    {
        public abstract JSONNode ToJSON();

        private JSONNode Node(string type, JSONNode value)
        {
            JSONObject node = new JSONObject();
            node["type"] = type;
            node["value"] = value;
            return node;
        }

        /* A general, unintepreted float value. */
        public class RawFloat : Metric
        {
            public readonly float value;
            public RawFloat(float f) { value = f; }
            public override JSONNode ToJSON() { return Node("RawFloat", new JSONNumber(this.value)); }
        }

        /* A general, uninterpreted integer value. */
        public class RawInt : Metric
        {
            public readonly int value;
            public RawInt(int i) { value = i; }
            public override JSONNode ToJSON() { return Node("RawInt", new JSONNumber(this.value)); }
        }

        /* A percentage value. */
        public class Percentage : Metric
        {
            public readonly float value;
            public Percentage(float f) { value = f; }
            public override JSONNode ToJSON() { return Node("Percentage", new JSONNumber(this.value)); }
        }

        /* The total of transmitted information (bits). */
        public class TransmittedInformation : Metric
        {
            public readonly float value;
            public TransmittedInformation(float f) { value = f; }
            public override JSONNode ToJSON() { return Node("TransmittedInformation", new JSONNumber(this.value)); }
        }

        /* The information transmission rate. */
        public class TransmissionRate : Metric
        {
            public readonly float value;
            public TransmissionRate(float f) { value = f; }
            public override JSONNode ToJSON() { return Node("TransmissionRate", new JSONNumber(this.value)); }
        }

        /* A reaction time in milliseconds. */
        public class ReactionTime : Metric
        {
            public readonly float value;
            public ReactionTime(float t) { value = t; }
            public override JSONNode ToJSON() { return Node("ReactionTime", new JSONNumber(this.value)); }
        }

        /* A duration in milliseconds. */
        public class Duration : Metric
        {
            public readonly float value;
            public Duration(float d) { value = d; }
            public override JSONNode ToJSON() { return Node("Duration", new JSONNumber(this.value)); }
        }

        /* A text value. */
        public class Text : Metric
        {
            public readonly string value;
            public Text(string t) { value = t; }
            public override JSONNode ToJSON() { return Node("Text", new JSONString(this.value)); }
        }

        /* A boolean flag. */
        public class Flag : Metric
        {
            public readonly bool value;
            public Flag(bool b) { value = b; }
            public override JSONNode ToJSON() { return Node("Flag", new JSONBool(this.value)); }
        }

    }

    public class Metrics : Dictionary<string, Metric>
    {
        public JSONNode ToJSON()
        {
            JSONObject collection = new JSONObject();
            foreach (var key in Keys)
            {
                collection[key] = this[key].ToJSON();
            }
            return collection;
        }

        public String toJSONString()
        {
            return this.ToJSON().ToString();
        }
    }

    public interface IPlayBehaviour
    {
        void OnHello(Settings settings);
        void OnPing();
        void OnSuspend();
        void OnResume();
    }
}

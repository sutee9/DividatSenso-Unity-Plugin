using System;
using System.Collections.Generic;
using SimpleJSON;   

namespace Dividat {
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
}
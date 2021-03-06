using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using AOT;
using UnityEngine;
using SimpleJSON;

namespace Dividat
{

    /// <summary>
    /// <code>Hardware</code> is the representation of the Senso Hardware. It acts as a substitute of the Unity Input, and maps the 5
    /// plates of the Senso to plates, which can be obtained by <code>GetPlateState()</code>, while steps are reported by
    /// <code>GetStep</code> and <code>GetRelease</code>.
    /// Before <code>Hardware</code> can be used, <code>Wire</code> must be called. This is done in <code>Play</code>, and should not be done
    /// by the game itself. Note that this class has no function in the editor, it only activates on the platform. Simulated key input
    /// is done in Senso Manager.
    /// </summary>
    public static class Hardware
    {
        public static Plate[] Plates
        {
            get { return plates; }
        }
        private static Plate[] plates = { new Plate(), new Plate(), new Plate(), new Plate(), new Plate() };

        public static bool GetStep(Direction direction)
        {
            return (GetActiveState(direction) && GetFrameCount(direction) == Time.frameCount - 1);
        }

        public static bool GetRelease(Direction direction)
        {
            return (!GetActiveState(direction) && GetFrameCount(direction) == Time.frameCount - 1);
        }

        public static bool GetPlateActive(Direction direction)
        {
            return GetActiveState(direction);
        }

        public static Plate GetPlateState(Direction direction)
        {
            return plates[(int)direction];
        }

        public static void ActivateMotor(MotorPattern pattern)
        {
            if (pattern.GetPreset() != null)
            {
            #if UNITY_WEBGL && !UNITY_EDITOR
                JSONObject cmd = new JSONObject();
                cmd["type"] = "Motor";
                cmd["preset"] = pattern.GetPreset();
                Play.Command(cmd.ToString());
            #else
                Debug.LogWarning("Warning: MotorPattern=" + pattern.GetPreset() + " received, but motor patterns are not supported on this platform.");
            #endif
            }
        }

        public static void SetLed(LedPattern pattern)
        {
        #if UNITY_WEBGL && !UNITY_EDITOR
            JSONObject settings = new JSONObject();
            settings["channel"] = (int)pattern.channel;
            settings["symbol"] = (int)pattern.symbol;
            settings["mode"] = (int)pattern.mode;
            settings["color"] = new JSONObject();
            settings["color"]["r"] = pattern.color.r;
            settings["color"]["g"] = pattern.color.g;
            settings["color"]["b"] = pattern.color.b;
            settings["brightness"] = (int)pattern.brightness;

            JSONObject cmd = new JSONObject();
            cmd["type"] = "Led";
            cmd["settings"] = settings;

            Play.Command(cmd.ToString());
        #else
            Debug.LogWarning("Warning: LedPattern=" + pattern + " received, but LED patterns are not supported on this platform.");
        #endif
        }

        public static void Wire()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
#elif UNITY_EDITOR
            WebsocketAvatar.Instance.Register();
            //Debug.Log("Hardware->Wire complete.");
#endif
        }

        #region Support Functions
        private static void SetActiveState(Direction direction, bool active)
        {
            var dir = (int)direction;
            plates[dir].active = active;
            plates[dir].changedAt = Time.frameCount;
        }

        private static bool GetActiveState(Direction direction)
        {
            return plates[(int)direction].active;
        }

        private static int GetFrameCount(Direction direction)
        {
            return plates[(int)direction].changedAt;
        }
        #endregion

        #region EGIBridge

        private static void SetPlateState(Direction direction, float x, float y, float f)
        {
            plates[(int)direction].x = Mathf.Clamp(x, 0f, 3f);
            plates[(int)direction].y = Mathf.Clamp(y, 0f, 3f);
            plates[(int)direction].f = f;
        }

        public static void OnStep(Direction direction)
        {
            SetActiveState(direction, true);
        }

        public static void OnRelease(Direction direction)
        {
            SetActiveState(direction, false);
        }

        public static void OnSensoState(Direction direction, float x, float y, float f)
        {
            SetPlateState(direction, x, y, f);
        }

        #endregion EGIBridge
    }
}

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Dividat
{
    public enum Direction { Center = 0, Up = 1, Right = 2, Down = 3, Left = 4 }

    public struct Plate
    {
        public float x, y, f;
        public bool active;
        public int changedAt;
        public Plate(float x_, float y_, float f_)
        {
            x = x_;
            y = y_;
            f = f_;
            active = false;
            changedAt = Int32.MaxValue;
        }

        public Plate(float x_, float y_, float f_, bool active, int changedFrame)
        {
            x = x_;
            y = y_;
            f = f_;
            this.active = active;
            changedAt = changedFrame;
        }

        public override string ToString(){
            return "(" + x + ", " + y + ", f=" + f + "); " + (active? "active " : "inactive") + " since " + changedAt;
        }
    }

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
        // private static bool centerActive = false;
        // private static int centerChangedAt = Int32.MaxValue;
        // private static bool upActive = false;
        // private static int upChangedAt = Int32.MaxValue;
        // private static bool rightActive = false;
        // private static int rightChangedAt = Int32.MaxValue;
        // private static bool downActive = false;
        // private static int downChangedAt = Int32.MaxValue;
        // private static bool leftActive = false;
        // private static int leftChangedAt = Int32.MaxValue;
        public static Plate[] Plates {
            get { return plates; }
        }
        private static Plate[] plates = { new Plate(), new Plate(), new Plate(), new Plate(), new Plate() };

        public static bool GetStep(Direction direction)
        {
            return (GetActiveState(direction) && GetFrameCount(direction) == Time.frameCount - 1);
        }

        public static bool GetRelease(Direction direction)
        {
            return  (!GetActiveState(direction) && GetFrameCount(direction) == Time.frameCount - 1);
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
                #if UNITY_WEBGL
                SendMotorPreset(pattern.GetPreset());
                #endif
            }
        }

        public static void Wire()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            Register(OnStep, OnRelease, OnSensoState);
            #else
            Debug.LogWarning("SENSO Hardware is not supported on this platform. Plates can be simulated with LEFT, RIGHT, UP, DOWN arrows and SPACE key");
            #endif
        }

        #region Simulated Input



        #endregion

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
            // switch (direction)
            // {
            //     case Direction.Center:
            //         return centerChangedAt;
            //     case Direction.Up:
            //         return upChangedAt;
            //     case Direction.Right:
            //         return rightChangedAt;
            //     case Direction.Down:
            //         return downChangedAt;
            //     case Direction.Left:
            //         return leftChangedAt;
            //     default:
            //         return -10;
            // }
        }
        #endregion

        #region EGIBridge
        #if UNITY_WEBGL
        // Implementation of Bridge to EGI
        // Based on ideas from https://forum.unity.com/threads/c-jslib-2-way-communication.323629/#post-2100593

        public delegate void DirectionCallback(int direction);
        public delegate void PlateCallback(int direction, float x, float y, float f);

        [DllImport("__Internal")]
        private static extern void Register(DirectionCallback onStep, DirectionCallback onRelease, PlateCallback onSensoState);

        [DllImport("__Internal")]
        private static extern void SendMotorPreset(string keyword);

        private static void SetPlateState(int direction, float x, float y, float f)
        {
            plates[direction].x = x;
            plates[direction].y = y;
            plates[direction].f = f;
        }



        [MonoPInvokeCallback(typeof(DirectionCallback))]
        private static void OnStep(int direction)
        {
            SetActiveState((Direction)direction, true);
        }

        [MonoPInvokeCallback(typeof(DirectionCallback))]
        private static void OnRelease(int direction)
        {
            SetActiveState((Direction)direction, false);
        }

        [MonoPInvokeCallback(typeof(PlateCallback))]
        private static void OnSensoState(int direction, float x, float y, float f)
        {
            SetPlateState(direction, x, y, f);
        }
        #endif
        #endregion EGIBridge
    }
}

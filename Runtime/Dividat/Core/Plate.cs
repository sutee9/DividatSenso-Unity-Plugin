using System;

namespace Dividat {
    [System.Serializable]
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
}
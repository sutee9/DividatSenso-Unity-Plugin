using System;

namespace Dividat
{
    public abstract class MotorPattern
    {
        public static MotorPattern positive = new Preset("positive");
        public static MotorPattern negative = new Preset("negative");

        public virtual string GetPreset()
        {
            return null;
        }

        // Implementation
        private MotorPattern()
        {
        }

        private class Preset : MotorPattern
        {
            public readonly String preset;

            public Preset(String keyword)
            {
                this.preset = keyword;
            }

            public override string GetPreset()
            {
                return preset;
            }
        }
    }
}

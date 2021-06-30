using System;
using UnityEngine;

namespace Dividat
{
    public class LedPattern
    {
        public static LedPattern off = new LedPattern(Channel.All, Symbol.All, Mode.Off, Color.white, 0);

        /* A bitmask of channels to apply the settings to. */
        public Channel channel;
        /* A bitmask of symbols to alter settings for. */
        public Symbol symbol;
        /* Defines the mode of the LEDs. */
        public Mode mode;
        /* Defines the color the LEDs will be set to. */
        public Color color;
        /* Brightness in range of 0 to 255 */
        public byte brightness;

        public LedPattern(Channel channel, Symbol symbol, Mode mode, Color color, byte brightness)
        {
            this.channel = channel;
            this.symbol = symbol;
            this.mode = mode;
            this.color = color;
            this.brightness = brightness;
        }

        [Flags]
        public enum Channel : byte
        {
            Center = 2,
            Up = 4,
            Right = 8,
            Down = 16,
            Left = 32,
            All = Center | Up | Right | Down | Left
        }

        [Flags]
        public enum Symbol : byte
        {
            Arrow = 1,
            Plus = 2,
            Circle = 4,
            All = Arrow | Plus | Circle
        }

        public enum Mode : byte
        {
            Off = 0,
            On = 1,
            Blink = 2,
            Pulse = 3
        }
    }
}

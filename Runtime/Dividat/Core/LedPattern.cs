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

        /// <summary>
        /// Default ctor switches all LEDs off.
        /// </summary>
        public LedPattern()
        {
            this.channel = 0;
            this.symbol = Symbol.Arrow;
            this.mode = Mode.Off;
            this.color = Color.white;
            this.brightness = 0;
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


        /// <summary>
        /// Switches this channel On, with the given symbol and brightness.
        /// This is a convenience method to not have to repeat the bitmask operations repeatedly.
        ///
        /// Note: You still need 
        /// </summary>
        /// <param name="channel">Which plate(s) should be switched on. If several, combine them with the | operator.</param>
        /// <param name="symbol"></param>
        public void SwitchOn(Channel channel, Symbol symbol = Symbol.Arrow)
        {
            SwitchOn(channel, symbol, Color.white, 255);
        }

        /// <summary>
        /// Switches this channel On, with the given symbol and brightness.
        /// This is a convenience method to not have to repeat the bitmask operations repeatedly.
        ///
        /// NOTE: You can call this method several times on the same pattern to switch several channels off.
        /// But you cannot combine SwitchOn and SwitchOff methods on the same pattern. To switch some
        /// panels off, and some other panels on, you must send two distinct patterns to SensoManager.
        ///
        /// Note: You still need 
        /// </summary>
        /// <param name="channel">A channel (=plate) to be swtiched on. You can combine several channels with the | operator.</param>
        /// <param name="symbol">The symbol the channel(s) should display. Arrow, Circle, Plus or All. You can also combine symbols with the | operator. </param>
        /// <param name="color">The symbol the channel(s) should display. If you call this method several times on the same LedPattern, only the last color will be respected.</param>
        /// <param name="brightness">Value between 0 (black) and 255 (brightest). If you call this method several times on the same LedPattern, only the last brightness will be respected.</param>
        public void SwitchOn(Channel channel, Symbol symbol, Color color, int brightness)
        {
            this.channel |= channel;
            this.mode = Mode.On;
            this.symbol = symbol;
            this.color = color;
            this.brightness = (byte)brightness;
        }

        /// <summary>
        /// Switches this channel on in any available mode (currently On, Blinking, Pulse) with the given symbol and brightness.
        /// This is a convenience method to not have to repeat the bitmask operations repeatedly.
        ///
        /// NOTE: You can call this method several times on the same pattern to switch several channels off.
        /// But you cannot combine SwitchOn and SwitchOff methods on the same pattern. To switch some
        /// panels off, and some other panels on, you must send two distinct patterns to SensoManager.
        ///
        /// Note: You still need 
        /// </summary>
        /// <param name="channel">A channel (=plate) to be swtiched on. You can combine several channels with the | operator.</param>
        /// <param name="symbol">The symbol the channel(s) should display. Arrow, Circle, Plus or All. You can also combine symbols with the | operator. </param>
        /// <param name="color">The symbol the channel(s) should display. If you call this method several times on the same LedPattern, only the last color will be respected.</param>
        /// <param name="brightness">Value between 0 (black) and 255 (brightest). If you call this method several times on the same LedPattern, only the last brightness will be respected.</param>
        public void SwitchOn(Channel channel, Mode mode, Symbol symbol, Color color, int brightness)
        {
            this.channel |= channel;
            this.mode = mode;
            this.symbol = symbol;
            this.color = color;
            this.brightness = (byte)brightness;
        }

        /// <summary>
        /// Removes this channel off.
        /// This is a convenience method to not have to repeat the bitmask operations repeatedly.
        ///
        /// NOTE: If you call this method on a pattern, all previous calls to SwitchOn will be erased. You can only SwitchOn OR SwitchOff. To turn the lights off on one channel,
        /// and then turn it on on another, use two distinct patterns and send them to Senso.
        /// </summary>
        public void SwitchOff(LedPattern.Channel channel, Symbol symbol = Symbol.All)
        {
            this.symbol = symbol;
            this.channel |= channel;
            this.mode = Mode.Off;
        }

        public static LedPattern.Channel DirectionToChannel(Direction dir)
        {
            switch (dir)
            {
                case Direction.Center:
                    return LedPattern.Channel.Center;
                case Direction.Down:
                    return LedPattern.Channel.Down;
                case Direction.Left:
                    return LedPattern.Channel.Left;
                case Direction.Right:
                    return LedPattern.Channel.Right;
                case Direction.Up:
                    return LedPattern.Channel.Up;
                default:
                    return LedPattern.Channel.All;
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dividat;
using System;
using System.Net;

namespace Dividat.Visualizer { 
    public class SensoLEDPatternTest : MonoBehaviour
    {
        private SensoManager _senso;
        private bool _ready = false;

        // Start is called before the first frame update
        void Start()
        {
            _senso = SensoManager.Instance;
            WireEvents();
            InitLED();
        }

        private void InitLED()
        {
            LedPattern pattern = new LedPattern();
            pattern.SwitchOn(LedPattern.Channel.Center, LedPattern.Symbol.Circle, Color.green, 255);
            _senso.ChangeLED(pattern);
        }

        private void AdjustLedPatternReleased(Direction dir)
        {
            AdjustLedPattern(dir, false);
        }

        private void AdjustLedPatternDown(Direction dir)
        {
            AdjustLedPattern(dir, true);
        }

        private void AdjustLedPattern(Direction dir, bool down)
        {
            LedPattern pattern = new LedPattern();
            if (down)
            {
                pattern.SwitchOn(LedPattern.DirectionToChannel(dir), LedPattern.Symbol.Arrow, Color.yellow, 255);
            }
            else
            {
                pattern.SwitchOff(LedPattern.DirectionToChannel(dir));
            }
            Debug.Log("Adjusted LED: " + pattern.channel + ", " + pattern.mode + ", " + pattern.symbol );
            _senso.ChangeLED(pattern);
        }

        private void UnwireEvents()
        {
            _senso.OnStepDown -= AdjustLedPatternDown;
            _senso.OnStepReleased -= AdjustLedPatternReleased;
            _senso.OnReady -= InitLED;
        }

        private void WireEvents()
        {
            _senso.OnStepDown += AdjustLedPatternDown;
            _senso.OnStepReleased += AdjustLedPatternReleased;
            _senso.OnReady += InitLED;
        }

        private void OnEnable()
        {
            if (_senso != null)
            {
                WireEvents();
            }
        }

        private void OnDisable()
        {
            if (_senso != null)
            {
                UnwireEvents();
            }
        }
    }
}

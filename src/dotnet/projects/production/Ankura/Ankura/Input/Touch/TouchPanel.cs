// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Ankura
{
    // https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.touchpanel.aspx
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
    public static class TouchPanel
    {
        internal static bool TouchDeviceExists;

        // The maximum number of simultaneous touches allowed by XNA.
        internal const int MaxTouches = 8;

        // The value that represents the absence of a finger.
        internal const int NoFinger = -1;

        public static int DisplayWidth { get; set; }

        public static int DisplayHeight { get; set; }

        public static DisplayOrientation DisplayOrientation { get; set; }

        public static GestureType EnabledGestures { get; set; }

        public static bool IsGestureAvailable => _gestures.Count > 0;

        public static IntPtr WindowHandle { get; set; }

        private static readonly Queue<GestureSample> _gestures = new Queue<GestureSample>();
        private static readonly TouchLocation[] _touches = new TouchLocation[MaxTouches];
        private static readonly TouchLocation[] _prevTouches = new TouchLocation[MaxTouches];
        private static readonly List<TouchLocation> _validTouches = new List<TouchLocation>();

        public static TouchPanelCapabilities GetCapabilities()
        {
            return FNAPlatform.GetTouchCapabilities();
        }

        public static TouchCollection GetState()
        {
            _validTouches.Clear();
            for (var i = 0; i < MaxTouches; i += 1)
            {
                if (_touches[i].State != TouchLocationState.Invalid)
                {
                    _validTouches.Add(_touches[i]);
                }
            }

            return new TouchCollection(_validTouches.ToArray());
        }

        public static GestureSample ReadGesture()
        {
            if (_gestures.Count == 0)
            {
                throw new InvalidOperationException();
            }

            return _gestures.Dequeue();
        }

        internal static void EnqueueGesture(GestureSample gesture)
        {
            _gestures.Enqueue(gesture);
        }

        internal static void INTERNAL_onTouchEvent(
            int fingerId,
            TouchLocationState state,
            float x,
            float y,
            float dx,
            float dy)
        {
            // Calculate the scaled touch position
            var touchPos = new Vector2(
                (float)Math.Round(x * DisplayWidth),
                (float)Math.Round(y * DisplayHeight));

            // Notify the Gesture Detector about the event
            switch (state)
            {
                case TouchLocationState.Pressed:
                    GestureDetector.OnPressed(fingerId, touchPos);
                    break;

                case TouchLocationState.Moved:
                    var delta = new Vector2(
                        (float)Math.Round(dx * DisplayWidth),
                        (float)Math.Round(dy * DisplayHeight));

                    GestureDetector.OnMoved(fingerId, touchPos, delta);

                    break;

                case TouchLocationState.Released:
                    GestureDetector.OnReleased(fingerId, touchPos);
                    break;
                case TouchLocationState.Invalid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        internal static void SetFinger(int index, int fingerId, Vector2 fingerPos)
        {
            if (fingerId == NoFinger)
            {
                // Was there a finger here before and the user just released it?
                if (_prevTouches[index].State != TouchLocationState.Invalid
                    && _prevTouches[index].State != TouchLocationState.Released)
                {
                    _touches[index] = new TouchLocation(
                        _prevTouches[index].Id,
                        TouchLocationState.Released,
                        _prevTouches[index].Position,
                        _prevTouches[index].State,
                        _prevTouches[index].Position);
                }
                else
                {
                    /* Nothing interesting here at all.
                     * Insert invalid data so this element
                     * is not included in GetState().
                     */
                    _touches[index] = new TouchLocation(
                        NoFinger,
                        TouchLocationState.Invalid,
                        Vector2.Zero);
                }

                return;
            }

            // Is this a newly pressed finger?
            if (_prevTouches[index].State == TouchLocationState.Invalid)
            {
                _touches[index] = new TouchLocation(
                    fingerId,
                    TouchLocationState.Pressed,
                    fingerPos);
            }
            else
            {
                // This finger was already down, so it's "moved"
                _touches[index] = new TouchLocation(
                    fingerId,
                    TouchLocationState.Moved,
                    fingerPos,
                    _prevTouches[index].State,
                    _prevTouches[index].Position);
            }
        }

        internal static void Update()
        {
            // Update Gesture Detector for time-sensitive gestures
            GestureDetector.OnUpdate();

            // Remember the last frame's touches
            _touches.CopyTo(_prevTouches, 0);

            // Get the latest finger data
            FNAPlatform.UpdateTouchPanelState();
        }
    }
}

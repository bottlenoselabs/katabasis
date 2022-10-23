// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace bottlenoselabs.Katabasis
{
	internal static class GestureDetector
	{
		/* How far (in pixels) the user can move their finger in a gesture
		 * before it counts as "moved". This prevents small, accidental
		 * finger movements from interfering with Hold and Tap gestures.
		 */
		private const int MoveThreshold = 35;

		/* How fast the finger velocity must be to register as a Flick.
		 * This helps prevent accidental flicks when a drag or tap was
		 * intended.
		 */
		private const int MinimumFlickVelocity = 100;

		// The ID of the active finger
		private static int _activeFingerId = TouchPanel.NoFinger;

		// The current position of the active finger
		private static Vector2 _activeFingerPosition;

		/* In XNA, if the Pinch gesture was disabled mid-pinch,
		 * it would still dispatch a PinchComplete gesture once *all*
		 * fingers were off the screen. (Not just the ones involved
		 * in the pinch.) Kinda weird, right?
		 * This flag is used to mimic that behavior.
		 */
		private static bool _callBelatedPinchComplete;

		// The time when the most recent active Press/Release occurred
		private static DateTime _eventTimestamp;

		// The IDs of all fingers currently on the screen
		private static readonly List<int> _fingerIds = new();

		// A flag to cancel Taps if a Double Tap has just occurred
		private static bool _justDoubleTapped;

		// The position of the active finger at the last Update tick
		private static Vector2 _lastUpdatePosition;

		// The position where the user first touched the screen
		private static Vector2 _pressPosition;

		// The ID of the second finger (used only for Pinching)
		private static int _secondFingerId = TouchPanel.NoFinger;

		// The current position of the second finger (used only for Pinching)
		private static Vector2 _secondFingerPosition;

		// The current state of gesture detection
		private static GestureState _state = GestureState.NONE;

		// The time of the most recent Update tick
		private static DateTime _updateTimestamp;

		// The current velocity of the active finger
		private static Vector2 _velocity;

		internal static void OnPressed(int fingerId, Vector2 touchPosition)
		{
			_fingerIds.Add(fingerId);

			if (_state == GestureState.PINCHING)
			{
				// None of this method applies to active pinches
				return;
			}

			// Set the active finger if there isn't one already
			if (_activeFingerId == TouchPanel.NoFinger)
			{
				_activeFingerId = fingerId;
				_activeFingerPosition = touchPosition;
			}
			else
			{
				if (IsGestureEnabled(GestureType.Pinch))
				{
					// Initialize a Pinch
					_secondFingerId = fingerId;
					_secondFingerPosition = touchPosition;

					_state = GestureState.PINCHING;
				}

				// No need to do anything more
				return;
			}

			if (_state == GestureState.JUST_TAPPED)
			{
				if (IsGestureEnabled(GestureType.DoubleTap))
				{
					// Must tap again within 300ms of original tap's release
					var timeSinceRelease = DateTime.Now - _eventTimestamp;
					if (timeSinceRelease <= TimeSpan.FromMilliseconds(300))
					{
						// If the new tap is close to the original tap
						var distance = (touchPosition - _pressPosition).Length();
						if (distance <= MoveThreshold)
						{
							// Double Tap!
							TouchPanel.EnqueueGesture(new GestureSample(
								GestureType.DoubleTap,
								GetGestureTimestamp(),
								touchPosition,
								Vector2.Zero,
								Vector2.Zero,
								Vector2.Zero,
								fingerId,
								TouchPanel.NoFinger));

							_justDoubleTapped = true;
						}
					}
				}
			}

			_state = GestureState.HOLDING;
			_pressPosition = touchPosition;
			_eventTimestamp = DateTime.Now;
		}

		internal static void OnReleased(int fingerId, Vector2 touchPosition)
		{
			_fingerIds.Remove(fingerId);

			// Handle release events separately for Pinch gestures
			if (_state == GestureState.PINCHING)
			{
				OnReleased_Pinch(fingerId);
				return;
			}

			// Did the user lift the active finger?
			if (fingerId == _activeFingerId)
			{
				_activeFingerId = TouchPanel.NoFinger;
			}

			// We're only interested in the very last finger to leave
			if (FNAPlatform.GetNumTouchFingers() > 0)
			{
				return;
			}

			if (_state == GestureState.HOLDING)
			{
				// Which Tap gestures are enabled?
				var enabledTap = IsGestureEnabled(GestureType.Tap);
				var enabledDoubleTap = IsGestureEnabled(GestureType.DoubleTap);

				if (enabledTap || enabledDoubleTap)
				{
					// How long did the user hold the touch?
					var timeHeld = DateTime.Now - _eventTimestamp;
					if (timeHeld < TimeSpan.FromSeconds(1))
					{
						// Don't register a Tap immediately after a Double Tap
						if (!_justDoubleTapped)
						{
							if (enabledTap)
							{
								// Tap!
								TouchPanel.EnqueueGesture(new GestureSample(
									GestureType.Tap,
									GetGestureTimestamp(),
									touchPosition,
									Vector2.Zero,
									Vector2.Zero,
									Vector2.Zero,
									fingerId,
									TouchPanel.NoFinger));
							}

							/* Even if Tap isn't enabled, we still
							* need this for Double Tap detection.
							*/
							_state = GestureState.JUST_TAPPED;
						}
					}
				}
			}

			// Reset this flag so we can catch Taps in the future
			_justDoubleTapped = false;

			if (IsGestureEnabled(GestureType.Flick))
			{
				// Only flick if the finger is outside the threshold and moving fast
				var distanceFromPress = (touchPosition - _pressPosition).Length();
				if (distanceFromPress > MoveThreshold &&
				    _velocity.Length() >= MinimumFlickVelocity)
				{
					// Flick!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.Flick,
						GetGestureTimestamp(),
						Vector2.Zero,
						Vector2.Zero,
						_velocity,
						Vector2.Zero,
						fingerId,
						TouchPanel.NoFinger));
				}

				// Reset velocity calculation variables
				_velocity = Vector2.Zero;
				_lastUpdatePosition = Vector2.Zero;
				_updateTimestamp = DateTime.MinValue;
			}

			if (IsGestureEnabled(GestureType.DragComplete))
			{
				var wasDragging = _state == GestureState.DRAGGING_H ||
				                  _state == GestureState.DRAGGING_V ||
				                  _state == GestureState.DRAGGING_FREE;

				if (wasDragging)
				{
					// Drag Complete!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.DragComplete,
						GetGestureTimestamp(),
						Vector2.Zero,
						Vector2.Zero,
						Vector2.Zero,
						Vector2.Zero,
						fingerId,
						TouchPanel.NoFinger));
				}
			}

			if (_callBelatedPinchComplete && IsGestureEnabled(GestureType.PinchComplete))
			{
				TouchPanel.EnqueueGesture(new GestureSample(
					GestureType.PinchComplete,
					GetGestureTimestamp(),
					Vector2.Zero,
					Vector2.Zero,
					Vector2.Zero,
					Vector2.Zero,
					TouchPanel.NoFinger,
					TouchPanel.NoFinger));
			}

			_callBelatedPinchComplete = false;

			// Reset the state if we're not anticipating a Double Tap
			if (_state != GestureState.JUST_TAPPED)
			{
				_state = GestureState.NONE;
			}

			_eventTimestamp = DateTime.Now;
		}

		internal static void OnMoved(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// Handle move events separately for Pinch gestures
			if (_state == GestureState.PINCHING)
			{
				OnMoved_Pinch(fingerId, touchPosition, delta);
				return;
			}

			// Replace the active finger if we lost it
			if (_activeFingerId == TouchPanel.NoFinger)
			{
				_activeFingerId = fingerId;
			}

			// If this finger isn't the active finger
			if (fingerId != _activeFingerId)
			{
				// We don't care about it
				return;
			}

			// Update the position
			_activeFingerPosition = touchPosition;

			// Determine which drag gestures are enabled
			var dragHorizontal = IsGestureEnabled(GestureType.HorizontalDrag);
			var dragVertical = IsGestureEnabled(GestureType.VerticalDrag);
			var dragFree = IsGestureEnabled(GestureType.FreeDrag);

			if (_state == GestureState.HOLDING || _state == GestureState.HELD)
			{
				// Prevent accidental drags
				var distanceFromPress = (touchPosition - _pressPosition).Length();
				if (distanceFromPress > MoveThreshold)
				{
					if (dragHorizontal && Math.Abs(delta.X) > Math.Abs(delta.Y))
					{
						// Horizontal Drag!
						_state = GestureState.DRAGGING_H;
					}
					else if (dragVertical && Math.Abs(delta.Y) > Math.Abs(delta.X))
					{
						// Vertical Drag!
						_state = GestureState.DRAGGING_V;
					}
					else if (dragFree)
					{
						// Free Drag!
						_state = GestureState.DRAGGING_FREE;
					}
					else
					{
						// No drag...
						_state = GestureState.NONE;
					}
				}
			}

			var state = _state;
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (state)
			{
				case GestureState.DRAGGING_H when dragHorizontal:
					// Horizontal Dragging!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.HorizontalDrag,
						GetGestureTimestamp(),
						touchPosition,
						Vector2.Zero,
						new Vector2(delta.X, 0),
						Vector2.Zero,
						fingerId,
						TouchPanel.NoFinger));

					break;
				case GestureState.DRAGGING_V when dragVertical:
					// Vertical Dragging!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.VerticalDrag,
						GetGestureTimestamp(),
						touchPosition,
						Vector2.Zero,
						new Vector2(0, delta.Y),
						Vector2.Zero,
						fingerId,
						TouchPanel.NoFinger));

					break;
				case GestureState.DRAGGING_FREE when dragFree:
					// Free Dragging!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.FreeDrag,
						GetGestureTimestamp(),
						touchPosition,
						Vector2.Zero,
						delta,
						Vector2.Zero,
						fingerId,
						TouchPanel.NoFinger));

					break;
				case GestureState.NONE:
					break;
				case GestureState.HOLDING:
					break;
				case GestureState.HELD:
					break;
				case GestureState.JUST_TAPPED:
					break;
				case GestureState.PINCHING:
					break;
			}

			/* Handle the case where the current drag type
			 * was disabled *while* the user was dragging.
			 */
			if ((_state == GestureState.DRAGGING_H && !dragHorizontal) ||
			    (_state == GestureState.DRAGGING_V && !dragVertical) ||
			    (_state == GestureState.DRAGGING_FREE && !dragFree))
			{
				// Reset the state
				_state = GestureState.HELD;
			}
		}

		internal static void OnUpdate()
		{
			if (_state == GestureState.PINCHING)
			{
				/* Handle the case where the Pinch gesture
				 * was disabled *while* the user was pinching.
				 */
				if (!IsGestureEnabled(GestureType.Pinch))
				{
					_state = GestureState.HELD;
					_secondFingerId = TouchPanel.NoFinger;

					// Still might need to trigger a PinchComplete
					_callBelatedPinchComplete = true;
				}

				// No pinches allowed in the rest of this method!
				return;
			}

			// Must have an active finger to proceed
			if (_activeFingerId == TouchPanel.NoFinger)
			{
				return;
			}

			if (IsGestureEnabled(GestureType.Flick))
			{
				// We need one frame to pass so we can calculate delta time
				if (_updateTimestamp != DateTime.MinValue)
				{
					/* The calculation below is mostly taken from MonoGame.
					 * It accumulates velocity after running it through
					 * a low-pass filter to mitigate the effect of
					 * acceleration spikes. This works pretty well,
					 * but on rare occasions the velocity will still
					 * spike by an order of magnitude.
					 * In practice this tends to be a non-issue, but
					 * if you *really* need to avoid any spikes, you
					 * may want to consider normalizing the delta
					 * reported in the GestureSample and then scaling it
					 * to min(actualVectorLength, preferredMaxLength).
					 * -caleb
					 */

					var dt = (float)(DateTime.Now - _updateTimestamp).TotalSeconds;
					var delta = _activeFingerPosition - _lastUpdatePosition;
					var instVelocity = delta / (0.001f + dt);
					_velocity += (instVelocity - _velocity) * 0.45f;
				}

				_lastUpdatePosition = _activeFingerPosition;
				_updateTimestamp = DateTime.Now;
			}

			if (IsGestureEnabled(GestureType.Hold) && _state == GestureState.HOLDING)
			{
				var timeSincePress = DateTime.Now - _eventTimestamp;
				if (timeSincePress >= TimeSpan.FromSeconds(1))
				{
					// Hold!
					TouchPanel.EnqueueGesture(new GestureSample(
						GestureType.Hold,
						GetGestureTimestamp(),
						_activeFingerPosition,
						Vector2.Zero,
						Vector2.Zero,
						Vector2.Zero,
						_activeFingerId,
						TouchPanel.NoFinger));

					_state = GestureState.HELD;
				}
			}
		}

		private static TimeSpan GetGestureTimestamp() =>
			/* XNA calculates gesture timestamps from
	         * how long the device has been turned on.
	         */
			TimeSpan.FromTicks(Environment.TickCount);

		private static bool IsGestureEnabled(GestureType gestureType) => (TouchPanel.EnabledGestures & gestureType) != 0;

		/* The *_Pinch methods are separate from the standard event methods
		 * because they have to deal with multiple touches. It gets really
		 * messy and ugly if single-touch and multi-touch detection is all
		 * intermingled in the same methods.
		 */

		private static void OnReleased_Pinch(int fingerId)
		{
			// We don't care about fingers that aren't part of the pinch
			if (fingerId != _activeFingerId && fingerId != _secondFingerId)
			{
				return;
			}

			if (IsGestureEnabled(GestureType.PinchComplete))
			{
				// Pinch Complete!
				TouchPanel.EnqueueGesture(new GestureSample(
					GestureType.PinchComplete,
					GetGestureTimestamp(),
					Vector2.Zero,
					Vector2.Zero,
					Vector2.Zero,
					Vector2.Zero,
					_activeFingerId,
					_secondFingerId));
			}

			// If we lost the active finger
			if (fingerId == _activeFingerId)
			{
				// Then the second finger becomes the active finger
				_activeFingerId = _secondFingerId;
				_activeFingerPosition = _secondFingerPosition;
			}

			// Regardless, we no longer have a second finger
			_secondFingerId = TouchPanel.NoFinger;

			// Attempt to replace our fallen comrade
			var replacedSecondFinger = false;
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var id in _fingerIds)
			{
				// Find a finger that's not already spoken for
				if (id != _activeFingerId)
				{
					_secondFingerId = id;
					replacedSecondFinger = true;
					break;
				}
			}

			if (!replacedSecondFinger)
			{
				// And we're back to a single touch
				_state = GestureState.HELD;
			}
		}

		private static void OnMoved_Pinch(int fingerId, Vector2 touchPosition, Vector2 delta)
		{
			// We only care if the finger moved is involved in the pinch
			if (fingerId != _activeFingerId && fingerId != _secondFingerId)
			{
				return;
			}

			/* In XNA, each Pinch gesture sample contained a delta
			 * for both fingers. It was somehow able to detect
			 * simultaneous deltas at an OS level. We don't have that
			 * luxury, so instead, each Pinch gesture will contain the
			 * delta information for just _one_ of the fingers.
			 * In practice what this means is that you'll get twice as
			 * many Pinch gestures added to the queue (one sample for
			 * each finger). This doesn't matter too much, though,
			 * since the resulting behavior is identical to XNA.
			 * -caleb
			 */

			if (fingerId == _activeFingerId)
			{
				_activeFingerPosition = touchPosition;
				TouchPanel.EnqueueGesture(new GestureSample(
					GestureType.Pinch,
					GetGestureTimestamp(),
					_activeFingerPosition,
					_secondFingerPosition,
					delta,
					Vector2.Zero,
					_activeFingerId,
					_secondFingerId));
			}
			else
			{
				_secondFingerPosition = touchPosition;
				TouchPanel.EnqueueGesture(new GestureSample(
					GestureType.Pinch,
					GetGestureTimestamp(),
					_activeFingerPosition,
					_secondFingerPosition,
					Vector2.Zero,
					delta,
					_activeFingerId,
					_secondFingerId));
			}
		}

		// All possible states of Gesture detection.
		[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Native naming conventions.")]
		private enum GestureState
		{
			NONE,
			HOLDING,
			HELD, /* Same as HOLDING, but after a Hold gesture has fired */
			JUST_TAPPED,
			DRAGGING_FREE,
			DRAGGING_H,
			DRAGGING_V,
			PINCHING
		}
	}
}

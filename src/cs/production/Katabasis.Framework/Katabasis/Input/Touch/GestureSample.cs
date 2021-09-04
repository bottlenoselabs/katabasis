// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Katabasis
{
	// https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.input.touch.gesturesample.aspx
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public readonly struct GestureSample
	{
		public Vector2 Delta { get; }

		public Vector2 Delta2 { get; }

		public GestureType GestureType { get; }

		public Vector2 Position { get; }

		public Vector2 Position2 { get; }

		public TimeSpan Timestamp { get; }

		public int FingerIdEXT { get; }

		public int FingerId2EXT { get; }

		public GestureSample(
			GestureType gestureType,
			TimeSpan timestamp,
			Vector2 position,
			Vector2 position2,
			Vector2 delta,
			Vector2 delta2)
			: this()
		{
			GestureType = gestureType;
			Timestamp = timestamp;
			Position = position;
			Position2 = position2;
			Delta = delta;
			Delta2 = delta2;

			// Oh well...
			FingerIdEXT = TouchPanel.NoFinger;
			FingerId2EXT = TouchPanel.NoFinger;
		}

		internal GestureSample(
			GestureType gestureType,
			TimeSpan timestamp,
			Vector2 position,
			Vector2 position2,
			Vector2 delta,
			Vector2 delta2,
			int fingerId,
			int fingerId2)
			: this()
		{
			GestureType = gestureType;
			Timestamp = timestamp;
			Position = position;
			Position2 = position2;
			Delta = delta;
			Delta2 = delta2;
			FingerIdEXT = fingerId;
			FingerId2EXT = fingerId2;
		}
	}
}

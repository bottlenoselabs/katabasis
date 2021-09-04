// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audioemitter.aspx
	public class AudioEmitter
	{
		private static readonly float[] StereoAzimuth = {0.0f, 0.0f};

		private static readonly GCHandle StereoAzimuthHandle = GCHandle.Alloc(StereoAzimuth, GCHandleType.Pinned);
		internal FAudio.F3DAUDIO_EMITTER _emitterData;

		public AudioEmitter()
		{
			_emitterData = default;
			DopplerScale = 1.0f;
			Forward = new Vector3(0, 0, -1);
			Position = Vector3.Zero;
			Up = new Vector3(0, 1, 0);
			Velocity = Vector3.Zero;

			/* Unused variables, defaults based on XNA behavior */
			_emitterData.pCone = IntPtr.Zero;
			_emitterData.ChannelCount = 1;
			_emitterData.ChannelRadius = 1.0f;
			// ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
			_emitterData.pChannelAzimuths = StereoAzimuthHandle.AddrOfPinnedObject();
			_emitterData.pVolumeCurve = IntPtr.Zero;
			_emitterData.pLFECurve = IntPtr.Zero;
			_emitterData.pLPFDirectCurve = IntPtr.Zero;
			_emitterData.pLPFReverbCurve = IntPtr.Zero;
			_emitterData.pReverbCurve = IntPtr.Zero;
			_emitterData.CurveDistanceScaler = 1.0f;
		}

		public float DopplerScale
		{
			get => _emitterData.DopplerScaler;
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_emitterData.DopplerScaler = value;
			}
		}

		public Vector3 Forward
		{
			get =>
				new(
					_emitterData.OrientFront.x,
					_emitterData.OrientFront.y,
					-_emitterData.OrientFront.z);
			set
			{
				_emitterData.OrientFront.x = value.X;
				_emitterData.OrientFront.y = value.Y;
				_emitterData.OrientFront.z = -value.Z;
			}
		}

		public Vector3 Position
		{
			get =>
				new(
					_emitterData.Position.x,
					_emitterData.Position.y,
					-_emitterData.Position.z);
			set
			{
				_emitterData.Position.x = value.X;
				_emitterData.Position.y = value.Y;
				_emitterData.Position.z = -value.Z;
			}
		}

		public Vector3 Up
		{
			get =>
				new(
					_emitterData.OrientTop.x,
					_emitterData.OrientTop.y,
					-_emitterData.OrientTop.z);
			set
			{
				_emitterData.OrientTop.x = value.X;
				_emitterData.OrientTop.y = value.Y;
				_emitterData.OrientTop.z = -value.Z;
			}
		}

		public Vector3 Velocity
		{
			get =>
				new(
					_emitterData.Velocity.x,
					_emitterData.Velocity.y,
					-_emitterData.Velocity.z);
			set
			{
				_emitterData.Velocity.x = value.X;
				_emitterData.Velocity.y = value.Y;
				_emitterData.Velocity.z = -value.Z;
			}
		}
	}
}

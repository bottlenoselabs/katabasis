// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Numerics;
using bottlenoselabs;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audiolistener.aspx
	public unsafe class AudioListener
	{
		internal FAudio.F3DAUDIO_LISTENER _listenerData;

		public AudioListener()
		{
			_listenerData = default;
			Forward = new Vector3(0, 0, -1);
			Position = Vector3.Zero;
			Up = new Vector3(0, 1, 0);
			Velocity = Vector3.Zero;

			/* Unused variables, defaults based on XNA behavior */
			_listenerData.pCone = (FAudio.F3DAUDIO_CONE*)IntPtr.Zero;
		}

		public Vector3 Forward
		{
			get =>
				new(
					_listenerData.OrientFront.x,
					_listenerData.OrientFront.y,
					-_listenerData.OrientFront.z);
			set
			{
				_listenerData.OrientFront.x = value.X;
				_listenerData.OrientFront.y = value.Y;
				_listenerData.OrientFront.z = -value.Z;
			}
		}

		public Vector3 Position
		{
			get =>
				new(
					_listenerData.Position.x,
					_listenerData.Position.y,
					-_listenerData.Position.z);
			set
			{
				_listenerData.Position.x = value.X;
				_listenerData.Position.y = value.Y;
				_listenerData.Position.z = -value.Z;
			}
		}

		public Vector3 Up
		{
			get =>
				new(
					_listenerData.OrientTop.x,
					_listenerData.OrientTop.y,
					-_listenerData.OrientTop.z);
			set
			{
				_listenerData.OrientTop.x = value.X;
				_listenerData.OrientTop.y = value.Y;
				_listenerData.OrientTop.z = -value.Z;
			}
		}

		public Vector3 Velocity
		{
			get =>
				new(
					_listenerData.Velocity.x,
					_listenerData.Velocity.y,
					-_listenerData.Velocity.z);
			set
			{
				_listenerData.Velocity.x = value.X;
				_listenerData.Velocity.y = value.Y;
				_listenerData.Velocity.z = -value.Z;
			}
		}
	}
}

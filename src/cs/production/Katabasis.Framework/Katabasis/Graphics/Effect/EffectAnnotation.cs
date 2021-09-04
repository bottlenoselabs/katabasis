// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will gut Mojo shader soon.")]
	public sealed class EffectAnnotation
	{
		private readonly IntPtr _values;

		internal EffectAnnotation(
			string? name,
			string? semantic,
			int rowCount,
			int columnCount,
			EffectParameterClass parameterClass,
			EffectParameterType parameterType,
			IntPtr data)
		{
			Name = name;
			Semantic = semantic ?? string.Empty;
			RowCount = rowCount;
			ColumnCount = columnCount;
			ParameterClass = parameterClass;
			ParameterType = parameterType;
			_values = data;
		}

		public string? Name { get; }

		public string? Semantic { get; }

		public int RowCount { get; }

		public int ColumnCount { get; }

		public EffectParameterClass ParameterClass { get; }

		public EffectParameterType ParameterType { get; }

		public bool GetValueBoolean()
		{
			unsafe
			{
				// Values are always 4 bytes, so we get to do this. -flibit
				var resPtr = (int*)_values;
				return *resPtr != 0;
			}
		}

		public int GetValueInt32()
		{
			unsafe
			{
				var resPtr = (int*)_values;
				return *resPtr;
			}
		}

		public Matrix4x4 GetValueMatrix()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Matrix4x4(
					resPtr[0],
					resPtr[4],
					resPtr[8],
					resPtr[12],
					resPtr[1],
					resPtr[5],
					resPtr[9],
					resPtr[13],
					resPtr[2],
					resPtr[6],
					resPtr[10],
					resPtr[14],
					resPtr[3],
					resPtr[7],
					resPtr[11],
					resPtr[15]);
			}
		}

		public float GetValueSingle()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return *resPtr;
			}
		}

		public string GetValueString() => throw
			/* FIXME: This requires digging into the effect->objects list.
	         * We've got the data, we just need to hook it up to FNA.
	         * -flibit
	         */
			new NotImplementedException("effect->objects[?]");

		public Vector2 GetValueVector2()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Vector2(resPtr[0], resPtr[1]);
			}
		}

		public Vector3 GetValueVector3()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Vector3(resPtr[0], resPtr[1], resPtr[2]);
			}
		}

		public Vector4 GetValueVector4()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Vector4(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3]);
			}
		}
	}
}

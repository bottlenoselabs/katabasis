// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will gut Mojo shader soon.")]
	public sealed class EffectParameter
	{
		internal Texture? _texture;

		internal IntPtr _values;
		internal uint _valuesSizeBytes;

		internal EffectParameter(
			string? name,
			string? semantic,
			int rowCount,
			int columnCount,
			int elementCount,
			EffectParameterClass parameterClass,
			EffectParameterType parameterType,
			EffectParameterCollection? structureMembers,
			EffectAnnotationCollection? annotations,
			IntPtr data,
			uint dataSizeBytes)
		{
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(data));
			}

			Name = name ?? string.Empty;
			Semantic = semantic ?? string.Empty;
			RowCount = rowCount;
			ColumnCount = columnCount;
			if (elementCount > 0)
			{
				var curOffset = 0;
				List<EffectParameter> elements = new(elementCount);
				for (var i = 0; i < elementCount; i += 1)
				{
					EffectParameterCollection? elementMembers = null;
					if (structureMembers != null)
					{
						List<EffectParameter> memList = new();
						for (var j = 0; j < structureMembers.Count; j += 1)
						{
							var memElems = 0;
							if (structureMembers[j]!.Elements != null)
							{
								memElems = structureMembers[j]!.Elements!.Count;
							}

							var memSize = structureMembers[j]!.RowCount * 4;
							if (memElems > 0)
							{
								memSize *= memElems;
							}

							memList.Add(new EffectParameter(
								structureMembers[j]!.Name,
								structureMembers[j]!.Semantic,
								structureMembers[j]!.RowCount,
								structureMembers[j]!.ColumnCount,
								memElems,
								structureMembers[j]!.ParameterClass,
								structureMembers[j]!.ParameterType,
								null, // FIXME: Nested structs! -flibit
								structureMembers[j]!.Annotations,
								new IntPtr(data.ToInt64() + curOffset),
								(uint)memSize * 4));

							curOffset += memSize * 4;
						}

						elementMembers = new EffectParameterCollection(memList);
					}

					// FIXME: Probably incomplete? -flibit
					elements.Add(new EffectParameter(
						null,
						null,
						rowCount,
						columnCount,
						0,
						ParameterClass,
						parameterType,
						elementMembers,
						null,
						new IntPtr(
							data.ToInt64() + (i * rowCount * 16)),
						// FIXME: Not obvious to me how to compute this -kg
						0));
				}

				Elements = new EffectParameterCollection(elements);
			}

			ParameterClass = parameterClass;
			ParameterType = parameterType;
			StructureMembers = structureMembers;
			Annotations = annotations;
			_values = data;
			_valuesSizeBytes = dataSizeBytes;
		}

		public string Name { get; }

		public string Semantic { get; }

		public int RowCount { get; }

		public int ColumnCount { get; }

		public EffectParameterClass ParameterClass { get; }

		public EffectParameterType ParameterType { get; }

		public EffectParameterCollection? Elements { get; }

		public EffectParameterCollection? StructureMembers { get; }

		public EffectAnnotationCollection? Annotations { get; }

		public bool GetValueBoolean()
		{
			unsafe
			{
				// Values are always 4 bytes, so we get to do this. -flibit
				var resPtr = (int*)_values;
				return *resPtr != 0;
			}
		}

		public bool[] GetValueBooleanArray(int count)
		{
			bool[] result = new bool[count];
			unsafe
			{
				var resPtr = (int*)_values;
				for (var i = 0; i < result.Length; resPtr += 4)
				{
					for (var j = 0; j < ColumnCount; j += 1, i += 1)
					{
						result[i] = *(resPtr + j) != 0;
					}
				}
			}

			return result;
		}

		public int GetValueInt32()
		{
			unsafe
			{
				var resPtr = (int*)_values;
				return *resPtr;
			}
		}

		public int[] GetValueInt32Array(int count)
		{
			int[] result = new int[count];
			for (int i = 0, j = 0; i < result.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(_values + j, result, i, ColumnCount);
			}

			return result;
		}

		public Matrix4x4 GetValueMatrixTranspose()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Matrix4x4(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3],
					resPtr[4],
					resPtr[5],
					resPtr[6],
					resPtr[7],
					resPtr[8],
					resPtr[9],
					resPtr[10],
					resPtr[11],
					resPtr[12],
					resPtr[13],
					resPtr[14],
					resPtr[15]);
			}
		}

		public Matrix4x4[] GetValueMatrixTransposeArray(int count)
		{
			Matrix4x4[] result = new Matrix4x4[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 16)
				{
					result[i] = new Matrix4x4(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3],
						resPtr[4],
						resPtr[5],
						resPtr[6],
						resPtr[7],
						resPtr[8],
						resPtr[9],
						resPtr[10],
						resPtr[11],
						resPtr[12],
						resPtr[13],
						resPtr[14],
						resPtr[15]);
				}
			}

			return result;
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

		public Matrix4x4[] GetValueMatrixArray(int count)
		{
			Matrix4x4[] result = new Matrix4x4[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 16)
				{
					result[i] = new Matrix4x4(
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

			return result;
		}

		public Quaternion GetValueQuaternion()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Quaternion(
					resPtr[0],
					resPtr[1],
					resPtr[2],
					resPtr[3]);
			}
		}

		public Quaternion[] GetValueQuaternionArray(int count)
		{
			Quaternion[] result = new Quaternion[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Quaternion(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3]);
				}
			}

			return result;
		}

		public float GetValueSingle()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return *resPtr;
			}
		}

		public float[] GetValueSingleArray(int count)
		{
			float[] result = new float[count];
			for (int i = 0, j = 0; i < result.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(_values + j, result, i, ColumnCount);
			}

			return result;
		}

		public string GetValueString() => throw
			/* FIXME: This requires digging into the effect->objects list.
	         * We've got the data, we just need to hook it up to FNA.
	         * -flibit
	         */
			new NotImplementedException("effect->objects[?]");

		public Texture2D? GetValueTexture2D() => (Texture2D?)_texture;

		public Texture3D? GetValueTexture3D() => (Texture3D?)_texture;

		public TextureCube? GetValueTextureCube() => (TextureCube?)_texture;

		public Vector2 GetValueVector2()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Vector2(resPtr[0], resPtr[1]);
			}
		}

		public Vector2[] GetValueVector2Array(int count)
		{
			Vector2[] result = new Vector2[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector2(
						resPtr[0],
						resPtr[1]);
				}
			}

			return result;
		}

		public Vector3 GetValueVector3()
		{
			unsafe
			{
				var resPtr = (float*)_values;
				return new Vector3(resPtr[0], resPtr[1], resPtr[2]);
			}
		}

		public Vector3[] GetValueVector3Array(int count)
		{
			Vector3[] result = new Vector3[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector3(
						resPtr[0],
						resPtr[1],
						resPtr[2]);
				}
			}

			return result;
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

		public Vector4[] GetValueVector4Array(int count)
		{
			Vector4[] result = new Vector4[count];
			unsafe
			{
				var resPtr = (float*)_values;
				for (var i = 0; i < count; i += 1, resPtr += 4)
				{
					result[i] = new Vector4(
						resPtr[0],
						resPtr[1],
						resPtr[2],
						resPtr[3]);
				}
			}

			return result;
		}

		public void SetValue(bool value)
		{
			unsafe
			{
				var dstPtr = (int*)_values;
				// Ugh, this branch, stupid C#.
				*dstPtr = value ? 1 : 0;
			}
		}

		public void SetValue(bool[] value)
		{
			unsafe
			{
				var dstPtr = (int*)_values;
				for (var i = 0; i < value.Length; dstPtr += 4)
				{
					for (var j = 0; j < ColumnCount; j += 1, i += 1)
					{
						// Ugh, this branch, stupid C#.
						*(dstPtr + j) = value[i] ? 1 : 0;
					}
				}
			}
		}

		public void SetValue(int value)
		{
			if (ParameterType == EffectParameterType.Single)
			{
				unsafe
				{
					var dstPtr = (float*)_values;
					*dstPtr = value;
				}
			}
			else
			{
				unsafe
				{
					var dstPtr = (int*)_values;
					*dstPtr = value;
				}
			}
		}

		public void SetValue(int[] value)
		{
			for (int i = 0, j = 0; i < value.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(value, i, _values + j, ColumnCount);
			}
		}

		public void SetValueTranspose(Matrix4x4 value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				var dstPtr = (float*)_values;
				switch (ColumnCount)
				{
					case 4 when RowCount == 4:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M12;
						dstPtr[2] = value.M13;
						dstPtr[3] = value.M14;
						dstPtr[4] = value.M21;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M23;
						dstPtr[7] = value.M24;
						dstPtr[8] = value.M31;
						dstPtr[9] = value.M32;
						dstPtr[10] = value.M33;
						dstPtr[11] = value.M34;
						dstPtr[12] = value.M41;
						dstPtr[13] = value.M42;
						dstPtr[14] = value.M43;
						dstPtr[15] = value.M44;
						break;
					case 3 when RowCount == 3:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M12;
						dstPtr[2] = value.M13;
						dstPtr[4] = value.M21;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M23;
						dstPtr[8] = value.M31;
						dstPtr[9] = value.M32;
						dstPtr[10] = value.M33;
						break;
					case 4 when RowCount == 3:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M12;
						dstPtr[2] = value.M13;
						dstPtr[4] = value.M21;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M23;
						dstPtr[8] = value.M31;
						dstPtr[9] = value.M32;
						dstPtr[10] = value.M33;
						dstPtr[12] = value.M41;
						dstPtr[13] = value.M42;
						dstPtr[14] = value.M43;
						break;
					case 3 when RowCount == 4:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M12;
						dstPtr[2] = value.M13;
						dstPtr[3] = value.M14;
						dstPtr[4] = value.M21;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M23;
						dstPtr[7] = value.M24;
						dstPtr[8] = value.M31;
						dstPtr[9] = value.M32;
						dstPtr[10] = value.M33;
						dstPtr[11] = value.M34;
						break;
					case 2 when RowCount == 2:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M12;
						dstPtr[4] = value.M21;
						dstPtr[5] = value.M22;
						break;
					default:
						throw new NotImplementedException($"Matrix Size: {RowCount} {ColumnCount}");
				}
			}
		}

		public void SetValueTranspose(Matrix4x4[] value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				var dstPtr = (float*)_values;
				switch (ColumnCount)
				{
					case 4 when RowCount == 4:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 16)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M12;
							dstPtr[2] = value[i].M13;
							dstPtr[3] = value[i].M14;
							dstPtr[4] = value[i].M21;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M23;
							dstPtr[7] = value[i].M24;
							dstPtr[8] = value[i].M31;
							dstPtr[9] = value[i].M32;
							dstPtr[10] = value[i].M33;
							dstPtr[11] = value[i].M34;
							dstPtr[12] = value[i].M41;
							dstPtr[13] = value[i].M42;
							dstPtr[14] = value[i].M43;
							dstPtr[15] = value[i].M44;
						}

						break;
					}

					case 3 when RowCount == 3:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 12)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M12;
							dstPtr[2] = value[i].M13;
							dstPtr[4] = value[i].M21;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M23;
							dstPtr[8] = value[i].M31;
							dstPtr[9] = value[i].M32;
							dstPtr[10] = value[i].M33;
						}

						break;
					}

					case 4 when RowCount == 3:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 16)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M12;
							dstPtr[2] = value[i].M13;
							dstPtr[4] = value[i].M21;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M23;
							dstPtr[8] = value[i].M31;
							dstPtr[9] = value[i].M32;
							dstPtr[10] = value[i].M33;
							dstPtr[12] = value[i].M41;
							dstPtr[13] = value[i].M42;
							dstPtr[14] = value[i].M43;
						}

						break;
					}

					case 3 when RowCount == 4:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 12)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M12;
							dstPtr[2] = value[i].M13;
							dstPtr[3] = value[i].M14;
							dstPtr[4] = value[i].M21;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M23;
							dstPtr[7] = value[i].M24;
							dstPtr[8] = value[i].M31;
							dstPtr[9] = value[i].M32;
							dstPtr[10] = value[i].M33;
							dstPtr[11] = value[i].M34;
						}

						break;
					}

					case 2 when RowCount == 2:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 8)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M12;
							dstPtr[4] = value[i].M21;
							dstPtr[5] = value[i].M22;
						}

						break;
					}

					default:
						throw new NotImplementedException($"Matrix Size: {RowCount} {ColumnCount}");
				}
			}
		}

		public void SetValue(Matrix4x4 value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				var dstPtr = (float*)_values;
				switch (ColumnCount)
				{
					case 4 when RowCount == 4:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M21;
						dstPtr[2] = value.M31;
						dstPtr[3] = value.M41;
						dstPtr[4] = value.M12;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M32;
						dstPtr[7] = value.M42;
						dstPtr[8] = value.M13;
						dstPtr[9] = value.M23;
						dstPtr[10] = value.M33;
						dstPtr[11] = value.M43;
						dstPtr[12] = value.M14;
						dstPtr[13] = value.M24;
						dstPtr[14] = value.M34;
						dstPtr[15] = value.M44;
						break;
					case 3 when RowCount == 3:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M21;
						dstPtr[2] = value.M31;
						dstPtr[4] = value.M12;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M32;
						dstPtr[8] = value.M13;
						dstPtr[9] = value.M23;
						dstPtr[10] = value.M33;
						break;
					case 4 when RowCount == 3:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M21;
						dstPtr[2] = value.M31;
						dstPtr[3] = value.M41;
						dstPtr[4] = value.M12;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M32;
						dstPtr[7] = value.M42;
						dstPtr[8] = value.M13;
						dstPtr[9] = value.M23;
						dstPtr[10] = value.M33;
						dstPtr[11] = value.M43;
						break;
					case 3 when RowCount == 4:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M21;
						dstPtr[2] = value.M31;
						dstPtr[4] = value.M12;
						dstPtr[5] = value.M22;
						dstPtr[6] = value.M32;
						dstPtr[8] = value.M13;
						dstPtr[9] = value.M23;
						dstPtr[10] = value.M33;
						dstPtr[12] = value.M14;
						dstPtr[13] = value.M24;
						dstPtr[14] = value.M34;
						break;
					case 2 when RowCount == 2:
						dstPtr[0] = value.M11;
						dstPtr[1] = value.M21;
						dstPtr[4] = value.M12;
						dstPtr[5] = value.M22;
						break;
					default:
						throw new NotImplementedException($"Matrix Size: {RowCount} {ColumnCount}");
				}
			}
		}

		public void SetValue(Matrix4x4[] value)
		{
			// FIXME: All Matrix sizes... this will get ugly. -flibit
			unsafe
			{
				var dstPtr = (float*)_values;
				switch (ColumnCount)
				{
					case 4 when RowCount == 4:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 16)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M21;
							dstPtr[2] = value[i].M31;
							dstPtr[3] = value[i].M41;
							dstPtr[4] = value[i].M12;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M32;
							dstPtr[7] = value[i].M42;
							dstPtr[8] = value[i].M13;
							dstPtr[9] = value[i].M23;
							dstPtr[10] = value[i].M33;
							dstPtr[11] = value[i].M43;
							dstPtr[12] = value[i].M14;
							dstPtr[13] = value[i].M24;
							dstPtr[14] = value[i].M34;
							dstPtr[15] = value[i].M44;
						}

						break;
					}

					case 3 when RowCount == 3:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 12)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M21;
							dstPtr[2] = value[i].M31;
							dstPtr[4] = value[i].M12;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M32;
							dstPtr[8] = value[i].M13;
							dstPtr[9] = value[i].M23;
							dstPtr[10] = value[i].M33;
						}

						break;
					}

					case 4 when RowCount == 3:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 12)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M21;
							dstPtr[2] = value[i].M31;
							dstPtr[3] = value[i].M41;
							dstPtr[4] = value[i].M12;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M32;
							dstPtr[7] = value[i].M42;
							dstPtr[8] = value[i].M13;
							dstPtr[9] = value[i].M23;
							dstPtr[10] = value[i].M33;
							dstPtr[11] = value[i].M43;
						}

						break;
					}

					case 3 when RowCount == 4:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 16)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M21;
							dstPtr[2] = value[i].M31;
							dstPtr[4] = value[i].M12;
							dstPtr[5] = value[i].M22;
							dstPtr[6] = value[i].M32;
							dstPtr[8] = value[i].M13;
							dstPtr[9] = value[i].M23;
							dstPtr[10] = value[i].M33;
							dstPtr[12] = value[i].M14;
							dstPtr[13] = value[i].M24;
							dstPtr[14] = value[i].M34;
						}

						break;
					}

					case 2 when RowCount == 2:
					{
						for (var i = 0; i < value.Length; i += 1, dstPtr += 8)
						{
							dstPtr[0] = value[i].M11;
							dstPtr[1] = value[i].M21;
							dstPtr[4] = value[i].M12;
							dstPtr[5] = value[i].M22;
						}

						break;
					}

					default:
						throw new NotImplementedException($"Matrix Size: {RowCount} {ColumnCount}");
				}
			}
		}

		public void SetValue(Quaternion value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
				dstPtr[3] = value.W;
			}
		}

		public void SetValue(Quaternion[] value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				for (var i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
					dstPtr[3] = value[i].W;
				}
			}
		}

		public void SetValue(float value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				*dstPtr = value;
			}
		}

		public void SetValue(float[] value)
		{
			for (int i = 0, j = 0; i < value.Length; i += ColumnCount, j += 16)
			{
				Marshal.Copy(value, i, _values + j, ColumnCount);
			}
		}

		public void SetValue(string value) => throw
			/* FIXME: This requires digging into the effect->objects list.
	         * We've got the data, we just need to hook it up to FNA.
	         * -flibit
	         */
			new NotImplementedException("effect->objects[?]");

		public void SetValue(Texture value) => _texture = value;

		public void SetValue(Vector2 value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
			}
		}

		public void SetValue(Vector2[] value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				for (var i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
				}
			}
		}

		public void SetValue(Vector3 value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
			}
		}

		public void SetValue(Vector3[] value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				for (var i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
				}
			}
		}

		public void SetValue(Vector4 value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				dstPtr[0] = value.X;
				dstPtr[1] = value.Y;
				dstPtr[2] = value.Z;
				dstPtr[3] = value.W;
			}
		}

		public void SetValue(Vector4[] value)
		{
			unsafe
			{
				var dstPtr = (float*)_values;
				for (var i = 0; i < value.Length; i += 1, dstPtr += 4)
				{
					dstPtr[0] = value[i].X;
					dstPtr[1] = value[i].Y;
					dstPtr[2] = value[i].Z;
					dstPtr[3] = value[i].W;
				}
			}
		}
	}
}

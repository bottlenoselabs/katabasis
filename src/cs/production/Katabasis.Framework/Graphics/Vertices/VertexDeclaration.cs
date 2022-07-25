// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class VertexDeclaration : GraphicsResource
	{
		internal VertexElement[] _elements;
		internal IntPtr _elementsPin;
		private GCHandle _handle;

		public VertexDeclaration(params VertexElement[] elements)
			: this(GetVertexStride(elements), elements)
		{
		}

		public VertexDeclaration(
			int vertexStride,
			params VertexElement[] elements)
		{
			if (elements == null || elements.Length == 0)
			{
				throw new ArgumentNullException(nameof(elements), "Elements cannot be empty");
			}

			_elements = (VertexElement[])elements.Clone();
			_handle = GCHandle.Alloc(_elements, GCHandleType.Pinned);
			_elementsPin = _handle.AddrOfPinnedObject();
			VertexStride = vertexStride;
		}

		public int VertexStride { get; }

		~VertexDeclaration() => _handle.Free();

		public VertexElement[] GetVertexElements() => (VertexElement[])_elements.Clone();

		internal static VertexDeclaration FromType(Type vertexType)
		{
			if (vertexType == null)
			{
				throw new ArgumentNullException(nameof(vertexType), "Cannot be null");
			}

			if (!vertexType.IsValueType)
			{
				throw new ArgumentException("Must be value type", nameof(vertexType));
			}

			if (!(Activator.CreateInstance(vertexType) is IVertexType type))
			{
				throw new ArgumentException("vertexData does not inherit IVertexType");
			}

			VertexDeclaration vertexDeclaration = type.VertexDeclaration;
			if (vertexDeclaration == null)
			{
				throw new ArgumentException("vertexType's VertexDeclaration cannot be null");
			}

			return vertexDeclaration;
		}

		private static int GetVertexStride(VertexElement[] elements)
		{
			var max = 0;

			for (var i = 0; i < elements.Length; i += 1)
			{
				var start = elements[i].Offset + GetTypeSize(elements[i].VertexElementFormat);
				if (max < start)
				{
					max = start;
				}
			}

			return max;
		}

		private static int GetTypeSize(VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			return 0;
		}
	}
}

// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	internal class PipelineCache
	{
		private const ulong HashFactor = 39;

		private readonly Dictionary<StateHash, BlendState> _blendCache = new();
		private readonly Dictionary<StateHash, DepthStencilState> _depthStencilCache = new();
		private readonly Dictionary<StateHash, RasterizerState> _rasterizerCache = new();
		private readonly Dictionary<StateHash, SamplerState> _samplerCache = new();
		public TextureAddressMode AddressU;
		public TextureAddressMode AddressV;
		public TextureAddressMode AddressW;
		public BlendFunction AlphaBlendFunction;
		public Blend AlphaDestinationBlend;
		public Blend AlphaSourceBlend;
		public Color BlendFactor;
		public StencilOperation CCWStencilDepthBufferFail;
		public StencilOperation CCWStencilFail;
		public CompareFunction CCWStencilFunction;
		public StencilOperation CCWStencilPass;
		public BlendFunction ColorBlendFunction;
		public Blend ColorDestinationBlend;
		public Blend ColorSourceBlend;
		public ColorWriteChannels ColorWriteChannels;
		public ColorWriteChannels ColorWriteChannels1;
		public ColorWriteChannels ColorWriteChannels2;
		public ColorWriteChannels ColorWriteChannels3;
		public CullMode CullMode;
		public float DepthBias;
		public bool DepthBufferEnable;
		public CompareFunction DepthBufferFunction;
		public bool DepthBufferWriteEnable;
		public FillMode FillMode;
		public TextureFilter Filter;
		public int MaxAnisotropy;
		public int MaxMipLevel;
		public float MipMapLODBias;
		public bool MultiSampleAntiAlias;
		public int MultiSampleMask;
		public int ReferenceStencil;
		public bool ScissorTestEnable;

		/* FIXME: Do we actually care about this calculation, or do we
		 * just assume false each time?
		 * -flibit
		 */
		public bool SeparateAlphaBlend;
		public float SlopeScaleDepthBias;
		public StencilOperation StencilDepthBufferFail;
		public bool StencilEnable;
		public StencilOperation StencilFail;
		public CompareFunction StencilFunction;
		public int StencilMask;
		public StencilOperation StencilPass;
		public int StencilWriteMask;
		public bool TwoSidedStencilMode;

		public static StateHash GetBlendHash(BlendState state) =>
			GetBlendHash(
				state.AlphaBlendFunction,
				state.AlphaDestinationBlend,
				state.AlphaSourceBlend,
				state.ColorBlendFunction,
				state.ColorDestinationBlend,
				state.ColorSourceBlend,
				state.ColorWriteChannels,
				state.ColorWriteChannels1,
				state.ColorWriteChannels2,
				state.ColorWriteChannels3,
				state.BlendFactor,
				state.MultiSampleMask);

		public void BeginApplyBlend()
		{
			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			var oldBlendState = graphicsDevice.BlendState;

			AlphaBlendFunction = oldBlendState!.AlphaBlendFunction;
			AlphaDestinationBlend = oldBlendState.AlphaDestinationBlend;
			AlphaSourceBlend = oldBlendState.AlphaSourceBlend;
			ColorBlendFunction = oldBlendState.ColorBlendFunction;
			ColorDestinationBlend = oldBlendState.ColorDestinationBlend;
			ColorSourceBlend = oldBlendState.ColorSourceBlend;
			ColorWriteChannels = oldBlendState.ColorWriteChannels;
			ColorWriteChannels1 = oldBlendState.ColorWriteChannels1;
			ColorWriteChannels2 = oldBlendState.ColorWriteChannels2;
			ColorWriteChannels3 = oldBlendState.ColorWriteChannels3;
			BlendFactor = oldBlendState.BlendFactor;
			MultiSampleMask = oldBlendState.MultiSampleMask;
			SeparateAlphaBlend = ColorBlendFunction != AlphaBlendFunction ||
			                     ColorDestinationBlend != AlphaDestinationBlend;
		}

		public void EndApplyBlend()
		{
			var hash = GetBlendHash(
				AlphaBlendFunction,
				AlphaDestinationBlend,
				AlphaSourceBlend,
				ColorBlendFunction,
				ColorDestinationBlend,
				ColorSourceBlend,
				ColorWriteChannels,
				ColorWriteChannels1,
				ColorWriteChannels2,
				ColorWriteChannels3,
				BlendFactor,
				MultiSampleMask);

			if (!_blendCache.TryGetValue(hash, out var newBlend))
			{
				newBlend = new BlendState();

				newBlend.AlphaBlendFunction = AlphaBlendFunction;
				newBlend.AlphaDestinationBlend = AlphaDestinationBlend;
				newBlend.AlphaSourceBlend = AlphaSourceBlend;
				newBlend.ColorBlendFunction = ColorBlendFunction;
				newBlend.ColorDestinationBlend = ColorDestinationBlend;
				newBlend.ColorSourceBlend = ColorSourceBlend;
				newBlend.ColorWriteChannels = ColorWriteChannels;
				newBlend.ColorWriteChannels1 = ColorWriteChannels1;
				newBlend.ColorWriteChannels2 = ColorWriteChannels2;
				newBlend.ColorWriteChannels3 = ColorWriteChannels3;
				newBlend.BlendFactor = BlendFactor;
				newBlend.MultiSampleMask = MultiSampleMask;

				_blendCache.Add(hash, newBlend);
				FNALoggerEXT.LogInfo!(
					"New BlendState added to pipeline cache with hash:\n" +
					hash);

				FNALoggerEXT.LogInfo(
					"Updated size of BlendState cache: " +
					_blendCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo!(
					"Retrieved BlendState from pipeline cache with hash:\n" +
					hash);
			}

			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			graphicsDevice.BlendState = newBlend;
		}

		public static StateHash GetDepthStencilHash(DepthStencilState state) =>
			GetDepthStencilHash(
				state.DepthBufferEnable,
				state.DepthBufferWriteEnable,
				state.DepthBufferFunction,
				state.StencilEnable,
				state.StencilFunction,
				state.StencilPass,
				state.StencilFail,
				state.StencilDepthBufferFail,
				state.TwoSidedStencilMode,
				state.CounterClockwiseStencilFunction,
				state.CounterClockwiseStencilPass,
				state.CounterClockwiseStencilFail,
				state.CounterClockwiseStencilDepthBufferFail,
				state.StencilMask,
				state.StencilWriteMask,
				state.ReferenceStencil);

		public void BeginApplyDepthStencil()
		{
			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			var oldDepthStencilState = graphicsDevice.DepthStencilState;

			DepthBufferEnable = oldDepthStencilState!.DepthBufferEnable;
			DepthBufferWriteEnable = oldDepthStencilState.DepthBufferWriteEnable;
			DepthBufferFunction = oldDepthStencilState.DepthBufferFunction;
			StencilEnable = oldDepthStencilState.StencilEnable;
			StencilFunction = oldDepthStencilState.StencilFunction;
			StencilPass = oldDepthStencilState.StencilPass;
			StencilFail = oldDepthStencilState.StencilFail;
			StencilDepthBufferFail = oldDepthStencilState.StencilDepthBufferFail;
			TwoSidedStencilMode = oldDepthStencilState.TwoSidedStencilMode;
			CCWStencilFunction = oldDepthStencilState.CounterClockwiseStencilFunction;
			CCWStencilFail = oldDepthStencilState.CounterClockwiseStencilFail;
			CCWStencilPass = oldDepthStencilState.CounterClockwiseStencilPass;
			CCWStencilDepthBufferFail = oldDepthStencilState.CounterClockwiseStencilDepthBufferFail;
			StencilMask = oldDepthStencilState.StencilMask;
			StencilWriteMask = oldDepthStencilState.StencilWriteMask;
			ReferenceStencil = oldDepthStencilState.ReferenceStencil;
		}

		public void EndApplyDepthStencil()
		{
			var hash = GetDepthStencilHash(
				DepthBufferEnable,
				DepthBufferWriteEnable,
				DepthBufferFunction,
				StencilEnable,
				StencilFunction,
				StencilPass,
				StencilFail,
				StencilDepthBufferFail,
				TwoSidedStencilMode,
				CCWStencilFunction,
				CCWStencilPass,
				CCWStencilFail,
				CCWStencilDepthBufferFail,
				StencilMask,
				StencilWriteMask,
				ReferenceStencil);

			if (!_depthStencilCache.TryGetValue(hash, out var newDepthStencil))
			{
				newDepthStencil = new DepthStencilState();

				newDepthStencil.DepthBufferEnable = DepthBufferEnable;
				newDepthStencil.DepthBufferWriteEnable = DepthBufferWriteEnable;
				newDepthStencil.DepthBufferFunction = DepthBufferFunction;
				newDepthStencil.StencilEnable = StencilEnable;
				newDepthStencil.StencilFunction = StencilFunction;
				newDepthStencil.StencilPass = StencilPass;
				newDepthStencil.StencilFail = StencilFail;
				newDepthStencil.StencilDepthBufferFail = StencilDepthBufferFail;
				newDepthStencil.TwoSidedStencilMode = TwoSidedStencilMode;
				newDepthStencil.CounterClockwiseStencilFunction = CCWStencilFunction;
				newDepthStencil.CounterClockwiseStencilFail = CCWStencilFail;
				newDepthStencil.CounterClockwiseStencilPass = CCWStencilPass;
				newDepthStencil.CounterClockwiseStencilDepthBufferFail = CCWStencilDepthBufferFail;
				newDepthStencil.StencilMask = StencilMask;
				newDepthStencil.StencilWriteMask = StencilWriteMask;
				newDepthStencil.ReferenceStencil = ReferenceStencil;

				_depthStencilCache.Add(hash, newDepthStencil);
				FNALoggerEXT.LogInfo!(
					"New DepthStencilState added to pipeline cache with hash:\n" +
					hash);

				FNALoggerEXT.LogInfo(
					"Updated size of DepthStencilState cache: " +
					_depthStencilCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo!(
					"Retrieved DepthStencilState from pipeline cache with hash:\n" +
					hash);
			}

			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			graphicsDevice.DepthStencilState = newDepthStencil;
		}

		public static StateHash GetRasterizerHash(RasterizerState state) =>
			GetRasterizerHash(
				state.CullMode,
				state.FillMode,
				state.DepthBias,
				state.MultiSampleAntiAlias,
				state.ScissorTestEnable,
				state.SlopeScaleDepthBias);

		public void BeginApplyRasterizer()
		{
			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			var oldRasterizerState = graphicsDevice.RasterizerState;

			CullMode = oldRasterizerState!.CullMode;
			FillMode = oldRasterizerState.FillMode;
			DepthBias = oldRasterizerState.DepthBias;
			MultiSampleAntiAlias = oldRasterizerState.MultiSampleAntiAlias;
			ScissorTestEnable = oldRasterizerState.ScissorTestEnable;
			SlopeScaleDepthBias = oldRasterizerState.SlopeScaleDepthBias;
		}

		public void EndApplyRasterizer()
		{
			var hash = GetRasterizerHash(
				CullMode,
				FillMode,
				DepthBias,
				MultiSampleAntiAlias,
				ScissorTestEnable,
				SlopeScaleDepthBias);

			if (!_rasterizerCache.TryGetValue(hash, out var newRasterizer))
			{
				newRasterizer = new RasterizerState();

				newRasterizer.CullMode = CullMode;
				newRasterizer.FillMode = FillMode;
				newRasterizer.DepthBias = DepthBias;
				newRasterizer.MultiSampleAntiAlias = MultiSampleAntiAlias;
				newRasterizer.ScissorTestEnable = ScissorTestEnable;
				newRasterizer.SlopeScaleDepthBias = SlopeScaleDepthBias;

				_rasterizerCache.Add(hash, newRasterizer);
				FNALoggerEXT.LogInfo!(
					"New RasterizerState added to pipeline cache with hash:\n" +
					hash);

				FNALoggerEXT.LogInfo!(
					"Updated size of RasterizerState cache: " +
					_rasterizerCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo!(
					"Retrieved RasterizerState from pipeline cache with hash:\n" +
					hash);
			}

			var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			graphicsDevice.RasterizerState = newRasterizer;
		}

		public static StateHash GetSamplerHash(SamplerState state) =>
			GetSamplerHash(
				state.AddressU,
				state.AddressV,
				state.AddressW,
				state.MaxAnisotropy,
				state.MaxMipLevel,
				state.MipMapLevelOfDetailBias,
				state.Filter);

		public void BeginApplySampler(SamplerStateCollection samplers, int register)
		{
			var oldSampler = samplers[register];
			if (oldSampler == null)
			{
				return;
			}

			AddressU = oldSampler.AddressU;
			AddressV = oldSampler.AddressV;
			AddressW = oldSampler.AddressW;
			MaxAnisotropy = oldSampler.MaxAnisotropy;
			MaxMipLevel = oldSampler.MaxMipLevel;
			MipMapLODBias = oldSampler.MipMapLevelOfDetailBias;
			Filter = oldSampler.Filter;
		}

		public void EndApplySampler(SamplerStateCollection samplers, int register)
		{
			var hash = GetSamplerHash(
				AddressU,
				AddressV,
				AddressW,
				MaxAnisotropy,
				MaxMipLevel,
				MipMapLODBias,
				Filter);

			if (!_samplerCache.TryGetValue(hash, out var newSampler))
			{
				newSampler = new SamplerState();

				newSampler.Filter = Filter;
				newSampler.AddressU = AddressU;
				newSampler.AddressV = AddressV;
				newSampler.AddressW = AddressW;
				newSampler.MaxAnisotropy = MaxAnisotropy;
				newSampler.MaxMipLevel = MaxMipLevel;
				newSampler.MipMapLevelOfDetailBias = MipMapLODBias;

				_samplerCache.Add(hash, newSampler);
				FNALoggerEXT.LogInfo!(
					"New SamplerState added to pipeline cache with hash:\n" +
					hash);

				FNALoggerEXT.LogInfo(
					"Updated size of SamplerState cache: " +
					_samplerCache.Count);
			}
			else
			{
				FNALoggerEXT.LogInfo!(
					"Retrieved SamplerState from pipeline cache with hash:\n" +
					hash);
			}

			samplers[register] = newSampler;
		}

		/* The algorithm for these hashing methods
		 * is taken from Josh Bloch's "Effective Java".
		 * (https://stackoverflow.com/a/113600/12492383)
		 *
		 * FIXME: Is there a better way to hash this?
		 * -caleb
		 */
		public static ulong GetVertexDeclarationHash(VertexDeclaration declaration, ulong vertexShader)
		{
			var hash = vertexShader;
			unchecked
			{
				for (var i = 0; i < declaration._elements.Length; i += 1)
				{
					hash = (hash * HashFactor) + (ulong)declaration._elements[i].GetHashCode();
				}

				hash = (hash * HashFactor) + (ulong)declaration.VertexStride;
			}

			return hash;
		}

		public static ulong GetVertexBindingHash(
			VertexBufferBinding[] bindings,
			int numBindings,
			ulong vertexShader)
		{
			var hash = vertexShader;
			unchecked
			{
				for (var i = 0; i < numBindings; i += 1)
				{
					var binding = bindings[i];
					hash = (hash * HashFactor) + (ulong)binding.InstanceFrequency;
					hash = (hash * HashFactor) +
					       GetVertexDeclarationHash(binding.VertexBuffer!.VertexDeclaration, vertexShader);
				}
			}

			return hash;
		}

		private static unsafe ulong FloatToULong(float f)
		{
			var uintRep = *(uint*)&f;
			return uintRep;
		}

		private static StateHash GetBlendHash(
			BlendFunction alphaBlendFunc,
			Blend alphaDestBlend,
			Blend alphaSrcBlend,
			BlendFunction colorBlendFunc,
			Blend colorDestBlend,
			Blend colorSrcBlend,
			ColorWriteChannels channels,
			ColorWriteChannels channels1,
			ColorWriteChannels channels2,
			ColorWriteChannels channels3,
			Color blendFactor,
			int multiSampleMask)
		{
			var functions = ((int)alphaBlendFunc << 4) | (int)colorBlendFunc;
			var blendsAndColorWriteChannels =
				((int)alphaDestBlend << 28)
				| ((int)alphaSrcBlend << 24)
				| ((int)colorDestBlend << 20)
				| ((int)colorSrcBlend << 16)
				| ((int)channels << 12)
				| ((int)channels1 << 8)
				| ((int)channels2 << 4)
				| (int)channels3;

			unchecked
			{
				return new StateHash(
					((ulong)functions << 32) | ((ulong)blendsAndColorWriteChannels << 0),
					((ulong)multiSampleMask << 32) | ((ulong)blendFactor.PackedValue << 0));
			}
		}

		private static StateHash GetDepthStencilHash(
			bool depthBufferEnable,
			bool depthWriteEnable,
			CompareFunction depthFunc,
			bool stencilEnable,
			CompareFunction stencilFunc,
			StencilOperation stencilPass,
			StencilOperation stencilFail,
			StencilOperation stencilDepthFail,
			bool twoSidedStencil,
			CompareFunction ccwStencilFunc,
			StencilOperation ccwStencilPass,
			StencilOperation ccwStencilFail,
			StencilOperation ccwStencilDepthFail,
			int stencilMask,
			int stencilWriteMask,
			int referenceStencil)
		{
			// Bool -> Int32 conversion
			var zEnable = depthBufferEnable ? 1 : 0;
			var zWriteEnable = depthWriteEnable ? 1 : 0;
			var sEnable = stencilEnable ? 1 : 0;
			var twoSided = twoSidedStencil ? 1 : 0;

			var packedProperties =
				(zEnable << 30)
				| (zWriteEnable << 29)
				| (sEnable << 28)
				| (twoSided << 27)
				| ((int)depthFunc << 24)
				| ((int)stencilFunc << 21)
				| ((int)ccwStencilFunc << 18)
				| ((int)stencilPass << 15)
				| ((int)stencilFail << 12)
				| ((int)stencilDepthFail << 9)
				| ((int)ccwStencilFail << 6)
				| ((int)ccwStencilPass << 3)
				| (int)ccwStencilDepthFail;

			unchecked
			{
				return new StateHash(
					((ulong)stencilMask << 32) | ((ulong)packedProperties << 0),
					((ulong)referenceStencil << 32) | ((ulong)stencilWriteMask << 0));
			}
		}

		private static StateHash GetRasterizerHash(
			CullMode cullMode,
			FillMode fillMode,
			float depthBias,
			bool msaa,
			bool scissor,
			float slopeScaleDepthBias)
		{
			// Bool -> Int32 conversion
			var multiSampleAntiAlias = msaa ? 1 : 0;
			var scissorTestEnable = scissor ? 1 : 0;

			var packedProperties =
				(multiSampleAntiAlias << 4)
				| (scissorTestEnable << 3)
				| ((int)cullMode << 1)
				| (int)fillMode;

			unchecked
			{
				return new StateHash(
					(ulong)packedProperties,
					(FloatToULong(slopeScaleDepthBias) << 32) | FloatToULong(depthBias));
			}
		}

		private static StateHash GetSamplerHash(
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			TextureAddressMode addressW,
			int maxAnisotropy,
			int maxMipLevel,
			float mipLODBias,
			TextureFilter filter)
		{
			var filterAndAddresses =
				((int)filter << 6)
				| ((int)addressU << 4)
				| ((int)addressV << 2)
				| (int)addressW;

			unchecked
			{
				return new StateHash(
					((ulong)maxAnisotropy << 32) | ((ulong)filterAndAddresses << 0),
					(FloatToULong(mipLODBias) << 32) | ((ulong)maxMipLevel << 0));
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal readonly struct StateHash : IEquatable<StateHash>
		{
			private readonly ulong _a;
			private readonly ulong _b;

			public StateHash(ulong a, ulong b)
			{
				_a = a;
				_b = b;
			}

			public override string ToString() =>
				Convert.ToString((long)_a, 2).PadLeft(64, '0') + "|" +
				Convert.ToString((long)_b, 2).PadLeft(64, '0');

			bool IEquatable<StateHash>.Equals(StateHash hash) => _a == hash._a && _b == hash._b;

			public override bool Equals(object? obj)
			{
				if (obj == null || obj.GetType() != GetType())
				{
					return false;
				}

				var hash = (StateHash)obj;
				return _a == hash._a && _b == hash._b;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var i1 = (int)(_a ^ (_a >> 32));
					var i2 = (int)(_b ^ (_b >> 32));
					return i1 + i2;
				}
			}
		}
	}
}

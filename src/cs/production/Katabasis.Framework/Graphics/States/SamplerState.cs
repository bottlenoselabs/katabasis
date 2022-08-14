// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class SamplerState : GraphicsResource
	{
		public static readonly SamplerState AnisotropicClamp = new(
			"SamplerState.AnisotropicClamp",
			TextureFilter.Anisotropic,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp);

		public static readonly SamplerState AnisotropicWrap = new(
			"SamplerState.AnisotropicWrap",
			TextureFilter.Anisotropic,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap);

		public static readonly SamplerState LinearClamp = new(
			"SamplerState.LinearClamp",
			TextureFilter.Linear,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp);

		public static readonly SamplerState LinearWrap = new(
			"SamplerState.LinearWrap",
			TextureFilter.Linear,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap);

		public static readonly SamplerState PointClamp = new(
			"SamplerState.PointClamp",
			TextureFilter.Point,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp);

		public static readonly SamplerState PointWrap = new(
			"SamplerState.PointWrap",
			TextureFilter.Point,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap);

		internal FNA3D.FNA3D_SamplerState _state;

		public SamplerState()
		{
			Filter = TextureFilter.Linear;
			AddressU = TextureAddressMode.Wrap;
			AddressV = TextureAddressMode.Wrap;
			AddressW = TextureAddressMode.Wrap;
			MaxAnisotropy = 4;
			MaxMipLevel = 0;
			MipMapLevelOfDetailBias = 0.0f;
		}

		private SamplerState(
			string name,
			TextureFilter filter,
			TextureAddressMode addressU,
			TextureAddressMode addressV,
			TextureAddressMode addressW)
			: this()
		{
			GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			Name = name;
			Filter = filter;
			AddressU = addressU;
			AddressV = addressV;
			AddressW = addressW;
		}

		public TextureAddressMode AddressU
		{
			get => (TextureAddressMode)_state.addressU;
			set => _state.addressU = (FNA3D.FNA3D_TextureAddressMode)value;
		}

		public TextureAddressMode AddressV
		{
			get => (TextureAddressMode)_state.addressV;
			set => _state.addressV = (FNA3D.FNA3D_TextureAddressMode)value;
		}

		public TextureAddressMode AddressW
		{
			get => (TextureAddressMode)_state.addressW;
			set => _state.addressW = (FNA3D.FNA3D_TextureAddressMode)value;
		}

		public TextureFilter Filter
		{
			get => (TextureFilter)_state.filter;
			set => _state.filter = (FNA3D.FNA3D_TextureFilter)value;
		}

		public int MaxAnisotropy
		{
			get => _state.maxAnisotropy;
			set => _state.maxAnisotropy = value;
		}

		public int MaxMipLevel
		{
			get => _state.maxMipLevel;
			set => _state.maxMipLevel = value;
		}

		public float MipMapLevelOfDetailBias
		{
			get => _state.mipMapLevelOfDetailBias;
			set => _state.mipMapLevelOfDetailBias = value;
		}
	}
}

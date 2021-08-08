// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
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
			get => _state.AddressU;
			set => _state.AddressU = value;
		}

		public TextureAddressMode AddressV
		{
			get => _state.AddressV;
			set => _state.AddressV = value;
		}

		public TextureAddressMode AddressW
		{
			get => _state.AddressW;
			set => _state.AddressW = value;
		}

		public TextureFilter Filter
		{
			get => _state.Filter;
			set => _state.Filter = value;
		}

		public int MaxAnisotropy
		{
			get => _state.MaxAnisotropy;
			set => _state.MaxAnisotropy = value;
		}

		public int MaxMipLevel
		{
			get => _state.MaxMipLevel;
			set => _state.MaxMipLevel = value;
		}

		public float MipMapLevelOfDetailBias
		{
			get => _state.MipMapLevelOfDetailBias;
			set => _state.MipMapLevelOfDetailBias = value;
		}
	}
}

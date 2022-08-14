// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Runtime.CompilerServices;

namespace bottlenoselabs.Katabasis
{
	public unsafe class BlendState : GraphicsResource
	{
		public static readonly BlendState Additive = new(
			"BlendState.Additive",
			Blend.SourceAlpha,
			Blend.SourceAlpha,
			Blend.One,
			Blend.One);

		public static readonly BlendState AlphaBlend = new(
			"BlendState.AlphaBlend",
			Blend.One,
			Blend.One,
			Blend.InverseSourceAlpha,
			Blend.InverseSourceAlpha);

		public static readonly BlendState NonPremultiplied = new(
			"BlendState.NonPremultiplied",
			Blend.SourceAlpha,
			Blend.SourceAlpha,
			Blend.InverseSourceAlpha,
			Blend.InverseSourceAlpha);

		public static readonly BlendState Opaque = new(
			"BlendState.Opaque",
			Blend.One,
			Blend.One,
			Blend.Zero,
			Blend.Zero);

		internal FNA3D.FNA3D_BlendState _state;

		public BlendState()
		{
			AlphaBlendFunction = BlendFunction.Add;
			AlphaDestinationBlend = Blend.Zero;
			AlphaSourceBlend = Blend.One;
			ColorBlendFunction = BlendFunction.Add;
			ColorDestinationBlend = Blend.Zero;
			ColorSourceBlend = Blend.One;
			ColorWriteChannels = ColorWriteChannels.All;
			ColorWriteChannels1 = ColorWriteChannels.All;
			ColorWriteChannels2 = ColorWriteChannels.All;
			ColorWriteChannels3 = ColorWriteChannels.All;
			BlendFactor = Color.White;
			MultiSampleMask = -1; // AKA 0xFFFFFFFF
		}

		private BlendState(
			string name,
			Blend colorSourceBlend,
			Blend alphaSourceBlend,
			Blend colorDestBlend,
			Blend alphaDestBlend)
			: this()
		{
			Name = name;
			ColorSourceBlend = colorSourceBlend;
			AlphaSourceBlend = alphaSourceBlend;
			ColorDestinationBlend = colorDestBlend;
			AlphaDestinationBlend = alphaDestBlend;
		}

		public BlendFunction AlphaBlendFunction
		{
			get => (BlendFunction)_state.alphaBlendFunction;
			set => _state.alphaBlendFunction = (FNA3D.FNA3D_BlendFunction)value;
		}

		public Blend AlphaDestinationBlend
		{
			get => (Blend)_state.alphaDestinationBlend;
			set => _state.alphaDestinationBlend = (FNA3D.FNA3D_Blend)value;
		}

		public Blend AlphaSourceBlend
		{
			get => (Blend)_state.alphaSourceBlend;
			set => _state.alphaSourceBlend = (FNA3D.FNA3D_Blend)value;
		}

		public BlendFunction ColorBlendFunction
		{
			get => (BlendFunction)_state.colorBlendFunction;
			set => _state.colorBlendFunction = (FNA3D.FNA3D_BlendFunction)value;
		}

		public Blend ColorDestinationBlend
		{
			get => (Blend)_state.colorDestinationBlend;
			set => _state.colorDestinationBlend = (FNA3D.FNA3D_Blend)value;
		}

		public Blend ColorSourceBlend
		{
			get => (Blend)_state.colorSourceBlend;
			set => _state.colorSourceBlend = (FNA3D.FNA3D_Blend)value;
		}

		public ColorWriteChannels ColorWriteChannels
		{
			get => (ColorWriteChannels)_state.colorWriteEnable;
			set => _state.colorWriteEnable = (FNA3D.FNA3D_ColorWriteChannels)value;
		}

		public ColorWriteChannels ColorWriteChannels1
		{
			get => (ColorWriteChannels)_state.colorWriteEnable1;
			set => _state.colorWriteEnable1 = (FNA3D.FNA3D_ColorWriteChannels)value;
		}

		public ColorWriteChannels ColorWriteChannels2
		{
			get => (ColorWriteChannels)_state.colorWriteEnable2;
			set => _state.colorWriteEnable2 = (FNA3D.FNA3D_ColorWriteChannels)value;
		}

		public ColorWriteChannels ColorWriteChannels3
		{
			get => (ColorWriteChannels)_state.colorWriteEnable3;
			set => _state.colorWriteEnable3 = (FNA3D.FNA3D_ColorWriteChannels)value;
		}

		public Color BlendFactor
		{
			get => Unsafe.ReadUnaligned<Color>(Unsafe.AsPointer(ref _state.blendFactor));
			set => _state.blendFactor = Unsafe.ReadUnaligned<FNA3D.FNA3D_Color>(Unsafe.AsPointer(ref _state.blendFactor));
		}

		public int MultiSampleMask
		{
			get => _state.multiSampleMask;
			set => _state.multiSampleMask = value;
		}
	}
}

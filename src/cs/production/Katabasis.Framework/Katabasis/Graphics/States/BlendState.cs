// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
namespace Katabasis
{
	public class BlendState : GraphicsResource
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
			get => _state.AlphaBlendFunction;
			set => _state.AlphaBlendFunction = value;
		}

		public Blend AlphaDestinationBlend
		{
			get => _state.AlphaDestinationBlend;
			set => _state.AlphaDestinationBlend = value;
		}

		public Blend AlphaSourceBlend
		{
			get => _state.AlphaSourceBlend;
			set => _state.AlphaSourceBlend = value;
		}

		public BlendFunction ColorBlendFunction
		{
			get => _state.ColorBlendFunction;
			set => _state.ColorBlendFunction = value;
		}

		public Blend ColorDestinationBlend
		{
			get => _state.ColorDestinationBlend;
			set => _state.ColorDestinationBlend = value;
		}

		public Blend ColorSourceBlend
		{
			get => _state.ColorSourceBlend;
			set => _state.ColorSourceBlend = value;
		}

		public ColorWriteChannels ColorWriteChannels
		{
			get => _state.ColorWriteEnable;
			set => _state.ColorWriteEnable = value;
		}

		public ColorWriteChannels ColorWriteChannels1
		{
			get => _state.ColorWriteEnable1;
			set => _state.ColorWriteEnable1 = value;
		}

		public ColorWriteChannels ColorWriteChannels2
		{
			get => _state.ColorWriteEnable2;
			set => _state.ColorWriteEnable2 = value;
		}

		public ColorWriteChannels ColorWriteChannels3
		{
			get => _state.ColorWriteEnable3;
			set => _state.ColorWriteEnable3 = value;
		}

		public Color BlendFactor
		{
			get => _state.BlendFactor;
			set => _state.BlendFactor = value;
		}

		public int MultiSampleMask
		{
			get => _state.MultiSampleMask;
			set => _state.MultiSampleMask = value;
		}
	}
}

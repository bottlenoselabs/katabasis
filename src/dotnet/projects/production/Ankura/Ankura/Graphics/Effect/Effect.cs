// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Ankura
{
    [SuppressMessage("ReSharper", "SA1202", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "CommentTypo", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "SA1307", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "SA1310", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Will gut Mojo shader soon.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Will gut Mojo shader soon.")]
    public class Effect : GraphicsResource
    {
        internal IntPtr _glEffect;
        private EffectTechnique? _currentTechnique;
        private IntPtr _stateChangesPtr;
        private readonly Dictionary<IntPtr, EffectParameter> _samplerMap = new Dictionary<IntPtr, EffectParameter>(new IntPtrBoxlessComparer());

        public EffectParameterCollection? Parameters { get; private set; }

        public EffectTechniqueCollection? Techniques { get; private set; }

        public EffectTechnique? CurrentTechnique
        {
            get => _currentTechnique;
            set
            {
                FNA3D.FNA3D_SetEffectTechnique(
                    GraphicsDevice.GLDevice,
                    _glEffect,
                    value!.TechniquePointer);
                _currentTechnique = value;
            }
        }

        public Effect(byte[] effectCode)
        {
            GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;

            // Send the blob to the GLDevice to be parsed/compiled
            FNA3D.FNA3D_CreateEffect(
                GraphicsDevice.GLDevice,
                effectCode,
                effectCode.Length,
                out _glEffect,
                out var effectData);

            // This is where it gets ugly...
            INTERNAL_parseEffectStruct(effectData);

            // The default technique is the first technique.
            CurrentTechnique = Techniques![0];

            // Use native memory for changes, .NET loves moving this around
            unsafe
            {
                _stateChangesPtr = Marshal.AllocHGlobal(
                    sizeof(MOJOSHADER_effectStateChanges));
                var stateChanges =
                    (MOJOSHADER_effectStateChanges*)_stateChangesPtr;
                stateChanges->render_state_change_count = 0;
                stateChanges->sampler_state_change_count = 0;
                stateChanges->vertex_sampler_state_change_count = 0;
            }
        }

        protected Effect(Effect cloneSource)
        {
            GraphicsDevice = cloneSource.GraphicsDevice;

            // Send the parsed data to be cloned and recompiled by MojoShader
            FNA3D.FNA3D_CloneEffect(
                GraphicsDevice.GLDevice,
                cloneSource._glEffect,
                out _glEffect,
                out var effectData);

            // Double the ugly, double the fun!
            INTERNAL_parseEffectStruct(effectData);

            var parameters = cloneSource.Parameters!;
            // Copy texture parameters, if applicable
            for (var i = 0; i < parameters.Count; i += 1)
            {
                Parameters![i]!._texture = parameters[i]!._texture;
            }

            // The default technique is whatever the current technique was.
            for (var i = 0; i < cloneSource!.Techniques!.Count; i += 1)
            {
                if (cloneSource.Techniques[i] == cloneSource.CurrentTechnique)
                {
                    CurrentTechnique = Techniques![i];
                }
            }

            // Use native memory for changes, .NET loves moving this around
            unsafe
            {
                _stateChangesPtr = Marshal.AllocHGlobal(sizeof(MOJOSHADER_effectStateChanges));
                var stateChanges = (MOJOSHADER_effectStateChanges*)_stateChangesPtr;
                stateChanges->render_state_change_count = 0;
                stateChanges->sampler_state_change_count = 0;
                stateChanges->vertex_sampler_state_change_count = 0;
            }
        }

        public static Effect FromStream(Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            var effect = new Effect(data);
            return effect;
        }

        public virtual Effect Clone()
        {
            return new Effect(this);
        }

        private class IntPtrBoxlessComparer : IEqualityComparer<IntPtr>
        {
            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly EffectParameterType[] XNAType =
        {
            EffectParameterType.Void, // MOJOSHADER_SYMTYPE_VOID
            EffectParameterType.Bool, // MOJOSHADER_SYMTYPE_BOOL
            EffectParameterType.Int32, // MOJOSHADER_SYMTYPE_INT
            EffectParameterType.Single, // MOJOSHADER_SYMTYPE_FLOAT
            EffectParameterType.String, // MOJOSHADER_SYMTYPE_STRING
            EffectParameterType.Texture, // MOJOSHADER_SYMTYPE_TEXTURE
            EffectParameterType.Texture1D, // MOJOSHADER_SYMTYPE_TEXTURE1D
            EffectParameterType.Texture2D, // MOJOSHADER_SYMTYPE_TEXTURE2D
            EffectParameterType.Texture3D, // MOJOSHADER_SYMTYPE_TEXTURE3D
            EffectParameterType.TextureCube // MOJOSHADER_SYMTYPE_TEXTURECUBE
        };

        private static readonly EffectParameterClass[] XNAClass =
        {
            EffectParameterClass.Scalar, // MOJOSHADER_SYMCLASS_SCALAR
            EffectParameterClass.Vector, // MOJOSHADER_SYMCLASS_VECTOR
            EffectParameterClass.Matrix, // MOJOSHADER_SYMCLASS_MATRIX_ROWS
            EffectParameterClass.Matrix, // MOJOSHADER_SYMCLASS_MATRIX_COLUMNS
            EffectParameterClass.Object, // MOJOSHADER_SYMCLASS_OBJECT
            EffectParameterClass.Struct // MOJOSHADER_SYMCLASS_STRUCT
        };

        private static readonly Blend[] XNABlend =
        {
            (Blend)(-1), // NOPE
            Blend.Zero, // MOJOSHADER_BLEND_ZERO
            Blend.One, // MOJOSHADER_BLEND_ONE
            Blend.SourceColor, // MOJOSHADER_BLEND_SRCCOLOR
            Blend.InverseSourceColor, // MOJOSHADER_BLEND_INVSRCCOLOR
            Blend.SourceAlpha, // MOJOSHADER_BLEND_SRCALPHA
            Blend.InverseSourceAlpha, // MOJOSHADER_BLEND_INVSRCALPHA
            Blend.DestinationAlpha, // MOJOSHADER_BLEND_DESTALPHA
            Blend.InverseDestinationAlpha, // MOJOSHADER_BLEND_INVDESTALPHA
            Blend.DestinationColor, // MOJOSHADER_BLEND_DESTCOLOR
            Blend.InverseDestinationColor, // MOJOSHADER_BLEND_INVDESTCOLOR
            Blend.SourceAlphaSaturation, // MOJOSHADER_BLEND_SRCALPHASAT
            (Blend)(-1), // NOPE
            (Blend)(-1), // NOPE
            Blend.BlendFactor, // MOJOSHADER_BLEND_BLENDFACTOR
            Blend.InverseBlendFactor // MOJOSHADER_BLEND_INVBLENDFACTOR
        };

        private static readonly BlendFunction[] XNABlendOp =
        {
            (BlendFunction)(-1), // NOPE
            BlendFunction.Add, // MOJOSHADER_BLENDOP_ADD
            BlendFunction.Subtract, // MOJOSHADER_BLENDOP_SUBTRACT
            BlendFunction.ReverseSubtract, // MOJOSHADER_BLENDOP_REVSUBTRACT
            BlendFunction.Min, // MOJOSHADER_BLENDOP_MIN
            BlendFunction.Max // MOJOSHADER_BLENDOP_MAX
        };

        private static readonly CompareFunction[] XNACompare =
        {
            (CompareFunction)(-1), // NOPE
            CompareFunction.Never, // MOJOSHADER_CMP_NEVER
            CompareFunction.Less, // MOJOSHADER_CMP_LESS
            CompareFunction.Equal, // MOJOSHADER_CMP_EQUAL
            CompareFunction.LessEqual, // MOJOSHADER_CMP_LESSEQUAL
            CompareFunction.Greater, // MOJOSHADER_CMP_GREATER
            CompareFunction.NotEqual, // MOJOSHADER_CMP_NOTEQUAL
            CompareFunction.GreaterEqual, // MOJOSHADER_CMP_GREATEREQUAL
            CompareFunction.Always // MOJOSHADER_CMP_ALWAYS
        };

        private static readonly StencilOperation[] XNAStencilOp =
        {
            (StencilOperation)(-1), // NOPE
            StencilOperation.Keep, // MOJOSHADER_STENCILOP_KEEP
            StencilOperation.Zero, // MOJOSHADER_STENCILOP_ZERO
            StencilOperation.Replace, // MOJOSHADER_STENCILOP_REPLACE
            StencilOperation.IncrementSaturation, // MOJOSHADER_STENCILOP_INCRSAT
            StencilOperation.DecrementSaturation, // MOJOSHADER_STENCILOP_DECRSAT
            StencilOperation.Invert, // MOJOSHADER_STENCILOP_INVERT
            StencilOperation.Increment, // MOJOSHADER_STENCILOP_INCR
            StencilOperation.Decrement // MOJOSHADER_STENCILOP_DECR
        };

        private static readonly TextureAddressMode[] XNAAddress =
        {
            (TextureAddressMode)(-1), // NOPE
            TextureAddressMode.Wrap, // MOJOSHADER_TADDRESS_WRAP
            TextureAddressMode.Mirror, // MOJOSHADER_TADDRESS_MIRROR
            TextureAddressMode.Clamp // MOJOSHADER_TADDRESS_CLAMP
        };

        private static readonly MOJOSHADER_textureFilterType[] XNAMag =
        {
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.Linear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.Point
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC, // TextureFilter.Anisotropic
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.LinearMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.PointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.MinLinearMagPointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.MinLinearMagPointMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.MinPointMagLinearMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR // TextureFilter.MinPointMagLinearMipPoint
        };

        private static readonly MOJOSHADER_textureFilterType[] XNAMin =
        {
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.Linear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.Point
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC, // TextureFilter.Anisotropic
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.LinearMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.PointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.MinLinearMagPointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.MinLinearMagPointMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.MinPointMagLinearMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT // TextureFilter.MinPointMagLinearMipPoint
        };

        private static readonly MOJOSHADER_textureFilterType[] XNAMip =
        {
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.Linear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.Point
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC, // TextureFilter.Anisotropic
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.LinearMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.PointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.MinLinearMagPointMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT, // TextureFilter.MinLinearMagPointMipPoint
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR, // TextureFilter.MinPointMagLinearMipLinear
            MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT // TextureFilter.MinPointMagLinearMipPoint
        };

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (_glEffect != IntPtr.Zero)
                {
                    FNA3D.FNA3D_AddDisposeEffect(
                        GraphicsDevice.GLDevice,
                        _glEffect);
                }

                if (_stateChangesPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_stateChangesPtr);
                    _stateChangesPtr = IntPtr.Zero;
                }
            }

            base.Dispose(disposing);
        }

        protected internal virtual void OnApply()
        {
        }

        internal unsafe void INTERNAL_applyEffect(uint pass)
        {
            FNA3D.FNA3D_ApplyEffect(
                GraphicsDevice.GLDevice,
                _glEffect,
                pass,
                _stateChangesPtr);
            var stateChanges =
                (MOJOSHADER_effectStateChanges*)_stateChangesPtr;
            if (stateChanges->render_state_change_count > 0)
            {
                PipelineCache pipelineCache = GraphicsDevice.PipelineCache;
                pipelineCache.BeginApplyBlend();
                pipelineCache.BeginApplyDepthStencil();
                pipelineCache.BeginApplyRasterizer();

                // Used to avoid redundant device state application
                var blendStateChanged = false;
                var depthStencilStateChanged = false;
                var rasterizerStateChanged = false;

                var states = (MOJOSHADER_effectState*)stateChanges->render_state_changes;
                for (var i = 0; i < stateChanges->render_state_change_count; i += 1)
                {
                    var type = states[i].type;
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (type)
                    {
                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_VERTEXSHADER:
                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_PIXELSHADER:
                            // Skip shader states
                            continue;
                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_ZENABLE:
                        {
                            var val = (MOJOSHADER_zBufferType*)states[i].value.values;
                            pipelineCache.DepthBufferEnable =
                                *val == MOJOSHADER_zBufferType.MOJOSHADER_ZB_TRUE;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_FILLMODE:
                        {
                            var val = (MOJOSHADER_fillMode*)states[i].value.values;
                            if (*val == MOJOSHADER_fillMode.MOJOSHADER_FILL_SOLID)
                            {
                                pipelineCache.FillMode = FillMode.Solid;
                            }
                            else if (*val == MOJOSHADER_fillMode.MOJOSHADER_FILL_WIREFRAME)
                            {
                                pipelineCache.FillMode = FillMode.WireFrame;
                            }

                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_ZWRITEENABLE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.DepthBufferWriteEnable = *val == 1;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLEND:
                        {
                            var val = (MOJOSHADER_blendMode*)states[i].value.values;
                            pipelineCache.ColorSourceBlend = XNABlend[(int)*val];
                            if (!pipelineCache.SeparateAlphaBlend)
                            {
                                pipelineCache.AlphaSourceBlend = XNABlend[(int)*val];
                            }

                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLEND:
                        {
                            var val = (MOJOSHADER_blendMode*)states[i].value.values;
                            pipelineCache.ColorDestinationBlend = XNABlend[(int)*val];
                            if (!pipelineCache.SeparateAlphaBlend)
                            {
                                pipelineCache.AlphaDestinationBlend = XNABlend[(int)*val];
                            }

                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_CULLMODE:
                        {
                            var val = (MOJOSHADER_cullMode*)states[i].value.values;
                            if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_NONE)
                            {
                                pipelineCache.CullMode = CullMode.None;
                            }
                            else if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_CW)
                            {
                                pipelineCache.CullMode = CullMode.CullClockwiseFace;
                            }
                            else if (*val == MOJOSHADER_cullMode.MOJOSHADER_CULL_CCW)
                            {
                                pipelineCache.CullMode = CullMode.CullCounterClockwiseFace;
                            }

                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_ZFUNC:
                        {
                            var val = (MOJOSHADER_compareFunc*)states[i].value.values;
                            pipelineCache.DepthBufferFunction = XNACompare[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_ALPHABLENDENABLE:
                        {
                            // FIXME: Assuming no other blend calls are made in the effect! -flibit
                            var val = (int*)states[i].value.values;
                            if (*val == 0)
                            {
                                pipelineCache.ColorSourceBlend = Blend.One;
                                pipelineCache.ColorDestinationBlend = Blend.Zero;
                                pipelineCache.AlphaSourceBlend = Blend.One;
                                pipelineCache.AlphaDestinationBlend = Blend.Zero;
                                blendStateChanged = true;
                            }

                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILENABLE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.StencilEnable = *val == 1;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFAIL:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.StencilFail = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILZFAIL:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.StencilDepthBufferFail = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILPASS:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.StencilPass = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFUNC:
                        {
                            var val = (MOJOSHADER_compareFunc*)states[i].value.values;
                            pipelineCache.StencilFunction = XNACompare[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILREF:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ReferenceStencil = *val;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILMASK:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.StencilMask = *val;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILWRITEMASK:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.StencilWriteMask = *val;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEANTIALIAS:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.MultiSampleAntiAlias = *val == 1;
                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEMASK:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.MultiSampleMask = *val;
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ColorWriteChannels = (ColorWriteChannels)(*val);
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOP:
                        {
                            var val = (MOJOSHADER_blendOp*)states[i].value.values;
                            pipelineCache.ColorBlendFunction = XNABlendOp[(int)*val];
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_SCISSORTESTENABLE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ScissorTestEnable = *val == 1;
                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_SLOPESCALEDEPTHBIAS:
                        {
                            var val = (float*)states[i].value.values;
                            pipelineCache.SlopeScaleDepthBias = *val;
                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_TWOSIDEDSTENCILMODE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.TwoSidedStencilMode = *val == 1;
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFAIL:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.CCWStencilFail = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILZFAIL:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.CCWStencilDepthBufferFail = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILPASS:
                        {
                            var val = (MOJOSHADER_stencilOp*)states[i].value.values;
                            pipelineCache.CCWStencilPass = XNAStencilOp[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFUNC:
                        {
                            var val = (MOJOSHADER_compareFunc*)states[i].value.values;
                            pipelineCache.CCWStencilFunction = XNACompare[(int)*val];
                            depthStencilStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE1:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ColorWriteChannels1 = (ColorWriteChannels)(*val);
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE2:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ColorWriteChannels2 = (ColorWriteChannels)(*val);
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE3:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.ColorWriteChannels3 = (ColorWriteChannels)(*val);
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDFACTOR:
                        {
                            // FIXME: RGBA? -flibit
                            var val = (int*)states[i].value.values;
                            pipelineCache.BlendFactor = new Color(
                                (*val >> 24) & 0xFF,
                                (*val >> 16) & 0xFF,
                                (*val >> 8) & 0xFF,
                                *val & 0xFF);
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_DEPTHBIAS:
                        {
                            var val = (float*)states[i].value.values;
                            pipelineCache.DepthBias = *val;
                            rasterizerStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_SEPARATEALPHABLENDENABLE:
                        {
                            var val = (int*)states[i].value.values;
                            pipelineCache.SeparateAlphaBlend = *val == 1;
                            // FIXME: Do we want to update the state for this...? -flibit
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLENDALPHA:
                        {
                            var val = (MOJOSHADER_blendMode*)states[i].value.values;
                            pipelineCache.AlphaSourceBlend = XNABlend[(int)*val];
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLENDALPHA:
                        {
                            var val = (MOJOSHADER_blendMode*)states[i].value.values;
                            pipelineCache.AlphaDestinationBlend = XNABlend[(int)*val];
                            blendStateChanged = true;
                            break;
                        }

                        case MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOPALPHA:
                        {
                            var val = (MOJOSHADER_blendOp*)states[i].value.values;
                            pipelineCache.AlphaBlendFunction = XNABlendOp[(int)*val];
                            blendStateChanged = true;
                            break;
                        }

                        case (MOJOSHADER_renderStateType)178:
                            /* Apparently this is "SetSampler"? */
                            break;
                        default:
                            throw new NotImplementedException("Unhandled render state! " + type);
                    }
                }

                if (blendStateChanged)
                {
                    pipelineCache.EndApplyBlend();
                }

                if (depthStencilStateChanged)
                {
                    pipelineCache.EndApplyDepthStencil();
                }

                if (rasterizerStateChanged)
                {
                    pipelineCache.EndApplyRasterizer();
                }
            }

            if (stateChanges->sampler_state_change_count > 0)
            {
                INTERNAL_updateSamplers(
                    stateChanges->sampler_state_change_count,
                    (MOJOSHADER_samplerStateRegister*)stateChanges->sampler_state_changes,
                    GraphicsDevice.Textures,
                    GraphicsDevice.SamplerStates);
            }

            if (stateChanges->vertex_sampler_state_change_count > 0)
            {
                INTERNAL_updateSamplers(
                    stateChanges->vertex_sampler_state_change_count,
                    (MOJOSHADER_samplerStateRegister*)stateChanges->vertex_sampler_state_changes,
                    GraphicsDevice.VertexTextures,
                    GraphicsDevice.VertexSamplerStates);
            }
        }

        private unsafe void INTERNAL_updateSamplers(
            uint changeCount,
            MOJOSHADER_samplerStateRegister* registers,
            TextureCollection textures,
            SamplerStateCollection samplers)
        {
            for (var i = 0; i < changeCount; i += 1)
            {
                if (registers[i].sampler_state_count == 0)
                {
                    // Nothing to do
                    continue;
                }

                var register = (int)registers[i].sampler_register;

                PipelineCache pipelineCache = GraphicsDevice.PipelineCache;
                pipelineCache.BeginApplySampler(samplers, register);

                // Used to prevent redundant sampler changes
                var samplerChanged = false;
                var filterChanged = false;

                // Current sampler filter
                var filter = pipelineCache.Filter;
                var magFilter = XNAMag[(int)filter];
                var minFilter = XNAMin[(int)filter];
                var mipFilter = XNAMip[(int)filter];

                var states = (MOJOSHADER_effectSamplerState*)registers[i].sampler_states;
                for (var j = 0; j < registers[i].sampler_state_count; j += 1)
                {
                    var type = states[j].type;
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (type)
                    {
                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_TEXTURE:
                        {
                            if (_samplerMap.TryGetValue(registers[i].sampler_name, out var texParam))
                            {
                                var texture = texParam._texture;
                                if (texture != null)
                                {
                                    textures[register] = texture;
                                }
                            }

                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSU:
                        {
                            var val = (MOJOSHADER_textureAddress*)states[j].value.values;
                            pipelineCache.AddressU = XNAAddress[(int)*val];
                            samplerChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSV:
                        {
                            var val = (MOJOSHADER_textureAddress*)states[j].value.values;
                            pipelineCache.AddressV = XNAAddress[(int)*val];
                            samplerChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSW:
                        {
                            var val = (MOJOSHADER_textureAddress*)states[j].value.values;
                            pipelineCache.AddressW = XNAAddress[(int)*val];
                            samplerChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAGFILTER:
                        {
                            var val = (MOJOSHADER_textureFilterType*)states[j].value.values;
                            magFilter = *val;
                            filterChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MINFILTER:
                        {
                            var val = (MOJOSHADER_textureFilterType*)states[j].value.values;
                            minFilter = *val;
                            filterChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPFILTER:
                        {
                            var val = (MOJOSHADER_textureFilterType*)states[j].value.values;
                            mipFilter = *val;
                            filterChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPMAPLODBIAS:
                        {
                            var val = (float*)states[j].value.values;
                            pipelineCache.MipMapLODBias = *val;
                            samplerChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXMIPLEVEL:
                        {
                            var val = (int*)states[j].value.values;
                            pipelineCache.MaxMipLevel = *val;
                            samplerChanged = true;
                            break;
                        }

                        case MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXANISOTROPY:
                        {
                            var val = (int*)states[j].value.values;
                            pipelineCache.MaxAnisotropy = *val;
                            samplerChanged = true;
                            break;
                        }

                        default:
                            throw new NotImplementedException("Unhandled sampler state! " + type);
                    }
                }

                if (filterChanged)
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (magFilter)
                    {
                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                            switch (minFilter)
                            {
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                    switch (mipFilter)
                                    {
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                            pipelineCache.Filter = TextureFilter.Point;
                                            break;
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                            pipelineCache.Filter = TextureFilter.PointMipLinear;
                                            break;
                                        default:
                                            throw new NotImplementedException("Unhandled mip filter type! " + mipFilter);
                                    }

                                    break;
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                    switch (mipFilter)
                                    {
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                            pipelineCache.Filter = TextureFilter.MinLinearMagPointMipPoint;
                                            break;
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                            pipelineCache.Filter = TextureFilter.MinLinearMagPointMipLinear;
                                            break;
                                        default:
                                            throw new NotImplementedException("Unhandled mip filter type! " + mipFilter);
                                    }

                                    break;
                                default:
                                    throw new NotImplementedException("Unhandled min filter type! " + minFilter);
                            }

                            break;
                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                            switch (minFilter)
                            {
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                    switch (mipFilter)
                                    {
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                            pipelineCache.Filter = TextureFilter.MinPointMagLinearMipPoint;
                                            break;
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                            pipelineCache.Filter = TextureFilter.MinPointMagLinearMipLinear;
                                            break;
                                        default:
                                            throw new NotImplementedException("Unhandled mip filter type! " + mipFilter);
                                    }

                                    break;
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                    switch (mipFilter)
                                    {
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_NONE:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_POINT:
                                            pipelineCache.Filter = TextureFilter.LinearMipPoint;
                                            break;
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_LINEAR:
                                        case MOJOSHADER_textureFilterType.MOJOSHADER_TEXTUREFILTER_ANISOTROPIC:
                                            pipelineCache.Filter = TextureFilter.Linear;
                                            break;
                                        default:
                                            throw new NotImplementedException("Unhandled mip filter type! " + mipFilter);
                                    }

                                    break;
                                default:
                                    throw new NotImplementedException("Unhandled min filter type! " + minFilter);
                            }

                            break;
                        default:
                            throw new NotImplementedException("Unhandled mag filter type! " + magFilter);
                    }

                    samplerChanged = true;
                }

                if (samplerChanged)
                {
                    pipelineCache.EndApplySampler(samplers, register);
                }
            }
        }

        private unsafe void INTERNAL_parseEffectStruct(IntPtr effectData)
        {
            var effectPtr = (MOJOSHADER_effect*)effectData;

            // Set up Parameters
            var paramPtr = (MOJOSHADER_effectParam*)effectPtr->parameters;
            List<EffectParameter> parameters = new List<EffectParameter>();
            for (var i = 0; i < effectPtr->param_count; i += 1)
            {
                var param = paramPtr[i];
                if (param.value.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VERTEXSHADER ||
                    param.value.type.parameter_type == MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_PIXELSHADER)
                {
                    // Skip shader objects...
                    continue;
                }

                if (param.value.type.parameter_type >= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER &&
                    param.value.type.parameter_type <= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLERCUBE)
                {
                    var textureName = string.Empty;
                    var states = (MOJOSHADER_effectSamplerState*)param.value.values;
                    for (var j = 0; j < param.value.value_count; j += 1)
                    {
                        if (states[j].value.type.parameter_type >= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE &&
                            states[j].value.type.parameter_type <= MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURECUBE)
                        {
                            var objectPtr = (MOJOSHADER_effectObject*)effectPtr->objects;
                            var index = (int*)states[j].value.values;
                            textureName = Marshal.PtrToStringAnsi(objectPtr[*index].mapping.name);
                            break;
                        }
                    }

                    /* Because textures have to be declared before the sampler,
                     * we can assume that it will always be in the list by the
                     * time we get to this point.
                     * -flibit
                     */
                    for (var j = 0; j < parameters.Count; j += 1)
                    {
                        if (!string.IsNullOrEmpty(textureName) && textureName.Equals(parameters[j].Name))
                        {
                            _samplerMap[param.value.name] = parameters[j];
                            break;
                        }
                    }

                    continue;
                }

                EffectParameterCollection? structMembers = null;
                if (param.value.type.member_count > 0)
                {
                    var memList = new List<EffectParameter>();
                    var mem = (MOJOSHADER_symbolStructMember*)param.value.type.members;
                    var curOffset = IntPtr.Zero;
                    for (var j = 0; j < param.value.type.member_count; j += 1)
                    {
                        var memSize = mem[j].info.rows * mem[j].info.columns;
                        if (mem[j].info.elements > 0)
                        {
                            memSize *= mem[j].info.elements;
                        }

                        memList.Add(new EffectParameter(
                            Marshal.PtrToStringAnsi(mem[j].name),
                            null,
                            (int)mem[j].info.rows,
                            (int)mem[j].info.columns,
                            (int)mem[j].info.elements,
                            XNAClass[(int)mem[j].info.parameter_class],
                            XNAType[(int)mem[j].info.parameter_type],
                            null, // FIXME: Nested structs! -flibit
                            null,
                            param.value.values + curOffset.ToInt32(),
                            memSize * 4));
                        curOffset += (int)memSize * 4;
                    }

                    structMembers = new EffectParameterCollection(memList);
                }

                parameters.Add(new EffectParameter(
                    Marshal.PtrToStringAnsi(param.value.name),
                    Marshal.PtrToStringAnsi(param.value.semantic),
                    (int)param.value.type.rows,
                    (int)param.value.type.columns,
                    (int)param.value.type.elements,
                    XNAClass[(int)param.value.type.parameter_class],
                    XNAType[(int)param.value.type.parameter_type],
                    structMembers,
                    INTERNAL_readAnnotations(
                        param.annotations,
                        param.annotation_count),
                    param.value.values,
                    param.value.value_count * sizeof(float)));
            }

            Parameters = new EffectParameterCollection(parameters);

            // Set up Techniques
            var techPtr = (MOJOSHADER_effectTechnique*)effectPtr->techniques;
            List<EffectTechnique> techniques = new List<EffectTechnique>(effectPtr->technique_count);
            for (var i = 0; i < techniques.Capacity; i += 1, techPtr += 1)
            {
                // Set up Passes
                var passPtr = (MOJOSHADER_effectPass*)techPtr->passes;
                var passes = new List<EffectPass>((int)techPtr->pass_count);
                for (var j = 0; j < passes.Capacity; j += 1)
                {
                    var pass = passPtr[j];
                    passes.Add(new EffectPass(
                        Marshal.PtrToStringAnsi(pass.name),
                        INTERNAL_readAnnotations(
                            pass.annotations,
                            pass.annotation_count),
                        this,
                        (IntPtr)techPtr,
                        (uint)j));
                }

                techniques.Add(new EffectTechnique(
                    Marshal.PtrToStringAnsi(techPtr->name),
                    (IntPtr)techPtr,
                    new EffectPassCollection(passes),
                    INTERNAL_readAnnotations(
                        techPtr->annotations,
                        techPtr->annotation_count)));
            }

            Techniques = new EffectTechniqueCollection(techniques);
        }

        private unsafe EffectAnnotationCollection INTERNAL_readAnnotations(
            IntPtr rawAnnotations,
            uint numAnnotations)
        {
            var annoPtr = (MOJOSHADER_effectAnnotation*)rawAnnotations;
            List<EffectAnnotation> annotations = new List<EffectAnnotation>((int)numAnnotations);
            for (var i = 0; i < numAnnotations; i += 1)
            {
                var anno = annoPtr[i];
                annotations.Add(new EffectAnnotation(
                    Marshal.PtrToStringAnsi(anno.name),
                    Marshal.PtrToStringAnsi(anno.semantic),
                    (int)anno.type.rows,
                    (int)anno.type.columns,
                    XNAClass[(int)anno.type.parameter_class],
                    XNAType[(int)anno.type.parameter_type],
                    anno.values));
            }

            return new EffectAnnotationCollection(annotations);
        }

        private enum MOJOSHADER_symbolClass
        {
            MOJOSHADER_SYMCLASS_SCALAR = 0,
            MOJOSHADER_SYMCLASS_VECTOR,
            MOJOSHADER_SYMCLASS_MATRIX_ROWS,
            MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
            MOJOSHADER_SYMCLASS_OBJECT,
            MOJOSHADER_SYMCLASS_STRUCT,
            MOJOSHADER_SYMCLASS_TOTAL
        }

        private enum MOJOSHADER_symbolType
        {
            MOJOSHADER_SYMTYPE_VOID = 0,
            MOJOSHADER_SYMTYPE_BOOL,
            MOJOSHADER_SYMTYPE_INT,
            MOJOSHADER_SYMTYPE_FLOAT,
            MOJOSHADER_SYMTYPE_STRING,
            MOJOSHADER_SYMTYPE_TEXTURE,
            MOJOSHADER_SYMTYPE_TEXTURE1D,
            MOJOSHADER_SYMTYPE_TEXTURE2D,
            MOJOSHADER_SYMTYPE_TEXTURE3D,
            MOJOSHADER_SYMTYPE_TEXTURECUBE,
            MOJOSHADER_SYMTYPE_SAMPLER,
            MOJOSHADER_SYMTYPE_SAMPLER1D,
            MOJOSHADER_SYMTYPE_SAMPLER2D,
            MOJOSHADER_SYMTYPE_SAMPLER3D,
            MOJOSHADER_SYMTYPE_SAMPLERCUBE,
            MOJOSHADER_SYMTYPE_PIXELSHADER,
            MOJOSHADER_SYMTYPE_VERTEXSHADER,
            MOJOSHADER_SYMTYPE_PIXELFRAGMENT,
            MOJOSHADER_SYMTYPE_VERTEXFRAGMENT,
            MOJOSHADER_SYMTYPE_UNSUPPORTED,
            MOJOSHADER_SYMTYPE_TOTAL
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_symbolTypeInfo
        {
            public readonly MOJOSHADER_symbolClass parameter_class;
            public readonly MOJOSHADER_symbolType parameter_type;
            public readonly uint rows;
            public readonly uint columns;
            public readonly uint elements;
            public readonly uint member_count;
            public readonly IntPtr members; // MOJOSHADER_symbolStructMember*
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_symbolStructMember
        {
            public readonly IntPtr name; // const char*
            public readonly MOJOSHADER_symbolTypeInfo info;
        }

        /* MOJOSHADER_effectState types... */

        private enum MOJOSHADER_renderStateType
        {
            MOJOSHADER_RS_ZENABLE,
            MOJOSHADER_RS_FILLMODE,
            MOJOSHADER_RS_SHADEMODE,
            MOJOSHADER_RS_ZWRITEENABLE,
            MOJOSHADER_RS_ALPHATESTENABLE,
            MOJOSHADER_RS_LASTPIXEL,
            MOJOSHADER_RS_SRCBLEND,
            MOJOSHADER_RS_DESTBLEND,
            MOJOSHADER_RS_CULLMODE,
            MOJOSHADER_RS_ZFUNC,
            MOJOSHADER_RS_ALPHAREF,
            MOJOSHADER_RS_ALPHAFUNC,
            MOJOSHADER_RS_DITHERENABLE,
            MOJOSHADER_RS_ALPHABLENDENABLE,
            MOJOSHADER_RS_FOGENABLE,
            MOJOSHADER_RS_SPECULARENABLE,
            MOJOSHADER_RS_FOGCOLOR,
            MOJOSHADER_RS_FOGTABLEMODE,
            MOJOSHADER_RS_FOGSTART,
            MOJOSHADER_RS_FOGEND,
            MOJOSHADER_RS_FOGDENSITY,
            MOJOSHADER_RS_RANGEFOGENABLE,
            MOJOSHADER_RS_STENCILENABLE,
            MOJOSHADER_RS_STENCILFAIL,
            MOJOSHADER_RS_STENCILZFAIL,
            MOJOSHADER_RS_STENCILPASS,
            MOJOSHADER_RS_STENCILFUNC,
            MOJOSHADER_RS_STENCILREF,
            MOJOSHADER_RS_STENCILMASK,
            MOJOSHADER_RS_STENCILWRITEMASK,
            MOJOSHADER_RS_TEXTUREFACTOR,
            MOJOSHADER_RS_WRAP0,
            MOJOSHADER_RS_WRAP1,
            MOJOSHADER_RS_WRAP2,
            MOJOSHADER_RS_WRAP3,
            MOJOSHADER_RS_WRAP4,
            MOJOSHADER_RS_WRAP5,
            MOJOSHADER_RS_WRAP6,
            MOJOSHADER_RS_WRAP7,
            MOJOSHADER_RS_WRAP8,
            MOJOSHADER_RS_WRAP9,
            MOJOSHADER_RS_WRAP10,
            MOJOSHADER_RS_WRAP11,
            MOJOSHADER_RS_WRAP12,
            MOJOSHADER_RS_WRAP13,
            MOJOSHADER_RS_WRAP14,
            MOJOSHADER_RS_WRAP15,
            MOJOSHADER_RS_CLIPPING,
            MOJOSHADER_RS_LIGHTING,
            MOJOSHADER_RS_AMBIENT,
            MOJOSHADER_RS_FOGVERTEXMODE,
            MOJOSHADER_RS_COLORVERTEX,
            MOJOSHADER_RS_LOCALVIEWER,
            MOJOSHADER_RS_NORMALIZENORMALS,
            MOJOSHADER_RS_DIFFUSEMATERIALSOURCE,
            MOJOSHADER_RS_SPECULARMATERIALSOURCE,
            MOJOSHADER_RS_AMBIENTMATERIALSOURCE,
            MOJOSHADER_RS_EMISSIVEMATERIALSOURCE,
            MOJOSHADER_RS_VERTEXBLEND,
            MOJOSHADER_RS_CLIPPLANEENABLE,
            MOJOSHADER_RS_POINTSIZE,
            MOJOSHADER_RS_POINTSIZE_MIN,
            MOJOSHADER_RS_POINTSPRITEENABLE,
            MOJOSHADER_RS_POINTSCALEENABLE,
            MOJOSHADER_RS_POINTSCALE_A,
            MOJOSHADER_RS_POINTSCALE_B,
            MOJOSHADER_RS_POINTSCALE_C,
            MOJOSHADER_RS_MULTISAMPLEANTIALIAS,
            MOJOSHADER_RS_MULTISAMPLEMASK,
            MOJOSHADER_RS_PATCHEDGESTYLE,
            MOJOSHADER_RS_DEBUGMONITORTOKEN,
            MOJOSHADER_RS_POINTSIZE_MAX,
            MOJOSHADER_RS_INDEXEDVERTEXBLENDENABLE,
            MOJOSHADER_RS_COLORWRITEENABLE,
            MOJOSHADER_RS_TWEENFACTOR,
            MOJOSHADER_RS_BLENDOP,
            MOJOSHADER_RS_POSITIONDEGREE,
            MOJOSHADER_RS_NORMALDEGREE,
            MOJOSHADER_RS_SCISSORTESTENABLE,
            MOJOSHADER_RS_SLOPESCALEDEPTHBIAS,
            MOJOSHADER_RS_ANTIALIASEDLINEENABLE,
            MOJOSHADER_RS_MINTESSELLATIONLEVEL,
            MOJOSHADER_RS_MAXTESSELLATIONLEVEL,
            MOJOSHADER_RS_ADAPTIVETESS_X,
            MOJOSHADER_RS_ADAPTIVETESS_Y,
            MOJOSHADER_RS_ADAPTIVETESS_Z,
            MOJOSHADER_RS_ADAPTIVETESS_W,
            MOJOSHADER_RS_ENABLEADAPTIVETESSELLATION,
            MOJOSHADER_RS_TWOSIDEDSTENCILMODE,
            MOJOSHADER_RS_CCW_STENCILFAIL,
            MOJOSHADER_RS_CCW_STENCILZFAIL,
            MOJOSHADER_RS_CCW_STENCILPASS,
            MOJOSHADER_RS_CCW_STENCILFUNC,
            MOJOSHADER_RS_COLORWRITEENABLE1,
            MOJOSHADER_RS_COLORWRITEENABLE2,
            MOJOSHADER_RS_COLORWRITEENABLE3,
            MOJOSHADER_RS_BLENDFACTOR,
            MOJOSHADER_RS_SRGBWRITEENABLE,
            MOJOSHADER_RS_DEPTHBIAS,
            MOJOSHADER_RS_SEPARATEALPHABLENDENABLE,
            MOJOSHADER_RS_SRCBLENDALPHA,
            MOJOSHADER_RS_DESTBLENDALPHA,
            MOJOSHADER_RS_BLENDOPALPHA,
            MOJOSHADER_RS_VERTEXSHADER = 146,
            MOJOSHADER_RS_PIXELSHADER = 147
        }

        private enum MOJOSHADER_zBufferType
        {
            MOJOSHADER_ZB_FALSE,
            MOJOSHADER_ZB_TRUE,
            MOJOSHADER_ZB_USEW
        }

        private enum MOJOSHADER_fillMode
        {
            MOJOSHADER_FILL_POINT = 1,
            MOJOSHADER_FILL_WIREFRAME = 2,
            MOJOSHADER_FILL_SOLID = 3
        }

        private enum MOJOSHADER_blendMode
        {
            MOJOSHADER_BLEND_ZERO = 1,
            MOJOSHADER_BLEND_ONE = 2,
            MOJOSHADER_BLEND_SRCCOLOR = 3,
            MOJOSHADER_BLEND_INVSRCCOLOR = 4,
            MOJOSHADER_BLEND_SRCALPHA = 5,
            MOJOSHADER_BLEND_INVSRCALPHA = 6,
            MOJOSHADER_BLEND_DESTALPHA = 7,
            MOJOSHADER_BLEND_INVDESTALPHA = 8,
            MOJOSHADER_BLEND_DESTCOLOR = 9,
            MOJOSHADER_BLEND_INVDESTCOLOR = 10,
            MOJOSHADER_BLEND_SRCALPHASAT = 11,
            MOJOSHADER_BLEND_BOTHSRCALPHA = 12,
            MOJOSHADER_BLEND_BOTHINVSRCALPHA = 13,
            MOJOSHADER_BLEND_BLENDFACTOR = 14,
            MOJOSHADER_BLEND_INVBLENDFACTOR = 15,
            MOJOSHADER_BLEND_SRCCOLOR2 = 16,
            MOJOSHADER_BLEND_INVSRCCOLOR2 = 17
        }

        private enum MOJOSHADER_cullMode
        {
            MOJOSHADER_CULL_NONE = 1,
            MOJOSHADER_CULL_CW = 2,
            MOJOSHADER_CULL_CCW = 3
        }

        private enum MOJOSHADER_compareFunc
        {
            MOJOSHADER_CMP_NEVER = 1,
            MOJOSHADER_CMP_LESS = 2,
            MOJOSHADER_CMP_EQUAL = 3,
            MOJOSHADER_CMP_LESSEQUAL = 4,
            MOJOSHADER_CMP_GREATER = 5,
            MOJOSHADER_CMP_NOTEQUAL = 6,
            MOJOSHADER_CMP_GREATEREQUAL = 7,
            MOJOSHADER_CMP_ALWAYS = 8
        }

        private enum MOJOSHADER_stencilOp
        {
            MOJOSHADER_STENCILOP_KEEP = 1,
            MOJOSHADER_STENCILOP_ZERO = 2,
            MOJOSHADER_STENCILOP_REPLACE = 3,
            MOJOSHADER_STENCILOP_INCRSAT = 4,
            MOJOSHADER_STENCILOP_DECRSAT = 5,
            MOJOSHADER_STENCILOP_INVERT = 6,
            MOJOSHADER_STENCILOP_INCR = 7,
            MOJOSHADER_STENCILOP_DECR = 8
        }

        private enum MOJOSHADER_blendOp
        {
            MOJOSHADER_BLENDOP_ADD = 1,
            MOJOSHADER_BLENDOP_SUBTRACT = 2,
            MOJOSHADER_BLENDOP_REVSUBTRACT = 3,
            MOJOSHADER_BLENDOP_MIN = 4,
            MOJOSHADER_BLENDOP_MAX = 5
        }

        /* MOJOSHADER_effectSamplerState types... */

        private enum MOJOSHADER_samplerStateType
        {
            MOJOSHADER_SAMP_UNKNOWN0 = 0,
            MOJOSHADER_SAMP_UNKNOWN1 = 1,
            MOJOSHADER_SAMP_UNKNOWN2 = 2,
            MOJOSHADER_SAMP_UNKNOWN3 = 3,
            MOJOSHADER_SAMP_TEXTURE = 4,
            MOJOSHADER_SAMP_ADDRESSU = 5,
            MOJOSHADER_SAMP_ADDRESSV = 6,
            MOJOSHADER_SAMP_ADDRESSW = 7,
            MOJOSHADER_SAMP_BORDERCOLOR = 8,
            MOJOSHADER_SAMP_MAGFILTER = 9,
            MOJOSHADER_SAMP_MINFILTER = 10,
            MOJOSHADER_SAMP_MIPFILTER = 11,
            MOJOSHADER_SAMP_MIPMAPLODBIAS = 12,
            MOJOSHADER_SAMP_MAXMIPLEVEL = 13,
            MOJOSHADER_SAMP_MAXANISOTROPY = 14,
            MOJOSHADER_SAMP_SRGBTEXTURE = 15,
            MOJOSHADER_SAMP_ELEMENTINDEX = 16,
            MOJOSHADER_SAMP_DMAPOFFSET = 17
        }

        private enum MOJOSHADER_textureAddress
        {
            MOJOSHADER_TADDRESS_WRAP = 1,
            MOJOSHADER_TADDRESS_MIRROR = 2,
            MOJOSHADER_TADDRESS_CLAMP = 3,
            MOJOSHADER_TADDRESS_BORDER = 4,
            MOJOSHADER_TADDRESS_MIRRORONCE = 5
        }

        private enum MOJOSHADER_textureFilterType
        {
            MOJOSHADER_TEXTUREFILTER_NONE,
            MOJOSHADER_TEXTUREFILTER_POINT,
            MOJOSHADER_TEXTUREFILTER_LINEAR,
            MOJOSHADER_TEXTUREFILTER_ANISOTROPIC,
            MOJOSHADER_TEXTUREFILTER_PYRAMIDALQUAD,
            MOJOSHADER_TEXTUREFILTER_GAUSSIANQUAD,
            MOJOSHADER_TEXTUREFILTER_CONVOLUTIONMONO
        }

        /* Effect value types... */

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectValue
        {
            public readonly IntPtr name; // const char*
            public readonly IntPtr semantic; // const char*
            public readonly MOJOSHADER_symbolTypeInfo type;
            public readonly uint value_count;
            public readonly IntPtr values; // You know what, just look at the C header...
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectState
        {
            public readonly MOJOSHADER_renderStateType type;
            public readonly MOJOSHADER_effectValue value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectSamplerState
        {
            public readonly MOJOSHADER_samplerStateType type;
            public readonly MOJOSHADER_effectValue value;
        }

        /* typedef MOJOSHADER_effectValue MOJOSHADER_effectAnnotation; */
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectAnnotation
        {
            public readonly IntPtr name; // const char*
            public readonly IntPtr semantic; // const char*
            public readonly MOJOSHADER_symbolTypeInfo type;
            public readonly uint value_count;
            public readonly IntPtr values; // You know what, just look at the C header...
        }

        /* Effect interface structures... */

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectParam
        {
            public readonly MOJOSHADER_effectValue value;
            public readonly uint annotation_count;
            public readonly IntPtr annotations; // MOJOSHADER_effectAnnotations*
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectPass
        {
            public readonly IntPtr name; // const char*
            public readonly uint state_count;
            public readonly IntPtr states; // MOJOSHADER_effectState*
            public readonly uint annotation_count;
            public readonly IntPtr annotations; // MOJOSHADER_effectAnnotations*
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectTechnique
        {
            public readonly IntPtr name; // const char*
            public readonly uint pass_count;
            public readonly IntPtr passes; // MOJOSHADER_effectPass*
            public readonly uint annotation_count;
            public readonly IntPtr annotations; // MOJOSHADER_effectAnnotations*
        }

        /* Effect "objects"... */

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectShader
        {
            public readonly MOJOSHADER_symbolType type;
            public readonly uint technique;
            public readonly uint pass;
            public readonly uint is_preshader;
            public readonly uint preshader_param_count;
            public readonly IntPtr preshader_params; // unsigned int*
            public readonly uint param_count;
            public readonly IntPtr parameters; // unsigned int*
            public readonly uint sampler_count;
            public readonly IntPtr samplers; // MOJOSHADER_samplerStateRegister*
            public readonly IntPtr shader; // *shader/*preshader union
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectSamplerMap
        {
            public readonly MOJOSHADER_symbolType type;
            public readonly IntPtr name; // const char*
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectString
        {
            public readonly MOJOSHADER_symbolType type;
            public readonly IntPtr stringvalue; // const char*
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effectTexture
        {
            public readonly MOJOSHADER_symbolType type;
        }

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct MOJOSHADER_effectObject
        {
            [FieldOffset(0)]
            public readonly MOJOSHADER_symbolType type;

            [FieldOffset(0)]
            public readonly MOJOSHADER_effectShader shader;

            [FieldOffset(0)]
            public readonly MOJOSHADER_effectSamplerMap mapping;

            [FieldOffset(0)]
            public readonly MOJOSHADER_effectString stringvalue;

            [FieldOffset(0)]
            public readonly MOJOSHADER_effectTexture texture;
        }

        /* Effect state change types... */

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_samplerStateRegister
        {
            public readonly IntPtr sampler_name; // const char*
            public readonly uint sampler_register;
            public readonly uint sampler_state_count;
            public readonly IntPtr sampler_states; // const MOJOSHADER_effectSamplerState*
        }

        // Needed by VideoPlayer...
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOJOSHADER_effectStateChanges
        {
            public uint render_state_change_count;
            public IntPtr render_state_changes; // const MOJOSHADER_effectState*
            public uint sampler_state_change_count;
            public IntPtr sampler_state_changes; // const MOJOSHADER_samplerStateRegister*
            public uint vertex_sampler_state_change_count;
            public IntPtr vertex_sampler_state_changes; // const MOJOSHADER_samplerStateRegister*
        }

        /* Effect parsing interface... this is a partial struct! */

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct MOJOSHADER_effect
        {
            public readonly int error_count;
            public readonly IntPtr errors; // MOJOSHADER_error*
            public readonly int param_count;
            public readonly IntPtr parameters; // MOJOSHADER_effectParam* params, lolC#
            public readonly int technique_count;
            public readonly IntPtr techniques; // MOJOSHADER_effectTechnique*
            public readonly int object_count;
            public readonly IntPtr objects; // MOJOSHADER_effectObject*
        }
    }
}

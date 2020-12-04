// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Katabasis
{
    [Serializable]
    [TypeConverter(typeof(ColorConverter))]
    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public struct Color : IEquatable<Color>, IPackedVector<uint>
    {
        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte)(packedValue >> 16);
                }
            }
            set => packedValue = (packedValue & 0xff00ffff) | ((uint)value << 16);
        }

        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(packedValue >> 8);
                }
            }
            set => packedValue = (packedValue & 0xffff00ff) | ((uint)value << 8);
        }

        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte)packedValue;
                }
            }
            set => packedValue = (packedValue & 0xffffff00) | value;
        }

        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)(packedValue >> 24);
                }
            }
            set => packedValue = (packedValue & 0x00ffffff) | ((uint)value << 24);
        }

        public uint PackedValue
        {
            get => packedValue;
            set => packedValue = value;
        }

        public static Color Transparent { get; private set; }

        public static Color AliceBlue { get; private set; }

        public static Color AntiqueWhite { get; private set; }

        public static Color Aqua { get; private set; }

        public static Color Aquamarine { get; private set; }

        public static Color Azure { get; private set; }

        public static Color Beige { get; private set; }

        public static Color Bisque { get; private set; }

        public static Color Black { get; private set; }

        public static Color BlanchedAlmond { get; private set; }

        public static Color Blue { get; private set; }

        public static Color BlueViolet { get; private set; }

        public static Color Brown { get; private set; }

        public static Color BurlyWood { get; private set; }

        public static Color CadetBlue { get; private set; }

        public static Color Chartreuse { get; private set; }

        public static Color Chocolate { get; private set; }

        public static Color Coral { get; private set; }

        public static Color CornflowerBlue { get; private set; }

        public static Color Cornsilk { get; private set; }

        public static Color Crimson { get; private set; }

        public static Color Cyan { get; private set; }

        public static Color DarkBlue { get; private set; }

        public static Color DarkCyan { get; private set; }

        public static Color DarkGoldenrod { get; private set; }

        public static Color DarkGray { get; private set; }

        public static Color DarkGreen { get; private set; }

        public static Color DarkKhaki { get; private set; }

        public static Color DarkMagenta { get; private set; }

        public static Color DarkOliveGreen { get; private set; }

        public static Color DarkOrange { get; private set; }

        public static Color DarkOrchid { get; private set; }

        public static Color DarkRed { get; private set; }

        public static Color DarkSalmon { get; private set; }

        public static Color DarkSeaGreen { get; private set; }

        public static Color DarkSlateBlue { get; private set; }

        public static Color DarkSlateGray { get; private set; }

        public static Color DarkTurquoise { get; private set; }

        public static Color DarkViolet { get; private set; }

        public static Color DeepPink { get; private set; }

        public static Color DeepSkyBlue { get; private set; }

        public static Color DimGray { get; private set; }

        public static Color DodgerBlue { get; private set; }

        public static Color Firebrick { get; private set; }

        public static Color FloralWhite { get; private set; }

        public static Color ForestGreen { get; private set; }

        public static Color Fuchsia { get; private set; }

        public static Color Gainsboro { get; private set; }

        public static Color GhostWhite { get; private set; }

        public static Color Gold { get; private set; }

        public static Color Goldenrod { get; private set; }

        public static Color Gray { get; private set; }

        public static Color Green { get; private set; }

        public static Color GreenYellow { get; private set; }

        public static Color Honeydew { get; private set; }

        public static Color HotPink { get; private set; }

        public static Color IndianRed { get; private set; }

        public static Color Indigo { get; private set; }

        public static Color Ivory { get; private set; }

        public static Color Khaki { get; private set; }

        public static Color Lavender { get; private set; }

        public static Color LavenderBlush { get; private set; }

        public static Color LawnGreen { get; private set; }

        public static Color LemonChiffon { get; private set; }

        public static Color LightBlue { get; private set; }

        public static Color LightCoral { get; private set; }

        public static Color LightCyan { get; private set; }

        public static Color LightGoldenrodYellow { get; private set; }

        public static Color LightGray { get; private set; }

        public static Color LightGreen { get; private set; }

        public static Color LightPink { get; private set; }

        public static Color LightSalmon { get; private set; }

        public static Color LightSeaGreen { get; private set; }

        public static Color LightSkyBlue { get; private set; }

        public static Color LightSlateGray { get; private set; }

        public static Color LightSteelBlue { get; private set; }

        public static Color LightYellow { get; private set; }

        public static Color Lime { get; private set; }

        public static Color LimeGreen { get; private set; }

        public static Color Linen { get; private set; }

        public static Color Magenta { get; private set; }

        public static Color Maroon { get; private set; }

        public static Color MediumAquamarine { get; private set; }

        public static Color MediumBlue { get; private set; }

        public static Color MediumOrchid { get; private set; }

        public static Color MediumPurple { get; private set; }

        public static Color MediumSeaGreen { get; private set; }

        public static Color MediumSlateBlue { get; private set; }

        public static Color MediumSpringGreen { get; private set; }

        public static Color MediumTurquoise { get; private set; }

        public static Color MediumVioletRed { get; private set; }

        public static Color MidnightBlue { get; private set; }

        public static Color MintCream { get; private set; }

        public static Color MistyRose { get; private set; }

        public static Color Moccasin { get; private set; }

        public static Color NavajoWhite { get; private set; }

        public static Color Navy { get; private set; }

        public static Color OldLace { get; private set; }

        public static Color Olive { get; private set; }

        public static Color OliveDrab { get; private set; }

        public static Color Orange { get; private set; }

        public static Color OrangeRed { get; private set; }

        public static Color Orchid { get; private set; }

        public static Color PaleGoldenrod { get; private set; }

        public static Color PaleGreen { get; private set; }

        public static Color PaleTurquoise { get; private set; }

        public static Color PaleVioletRed { get; private set; }

        public static Color PapayaWhip { get; private set; }

        public static Color PeachPuff { get; private set; }

        public static Color Peru { get; private set; }

        public static Color Pink { get; private set; }

        public static Color Plum { get; private set; }

        public static Color PowderBlue { get; private set; }

        public static Color Purple { get; private set; }

        public static Color Red { get; private set; }

        public static Color RosyBrown { get; private set; }

        public static Color RoyalBlue { get; private set; }

        public static Color SaddleBrown { get; private set; }

        public static Color Salmon { get; private set; }

        public static Color SandyBrown { get; private set; }

        public static Color SeaGreen { get; private set; }

        public static Color SeaShell { get; private set; }

        public static Color Sienna { get; private set; }

        public static Color Silver { get; private set; }

        public static Color SkyBlue { get; private set; }

        public static Color SlateBlue { get; private set; }

        public static Color SlateGray { get; private set; }

        public static Color Snow { get; private set; }

        public static Color SpringGreen { get; private set; }

        public static Color SteelBlue { get; private set; }

        public static Color Tan { get; private set; }

        public static Color Teal { get; private set; }

        public static Color Thistle { get; private set; }

        public static Color Tomato { get; private set; }

        public static Color Turquoise { get; private set; }

        public static Color Violet { get; private set; }

        public static Color Wheat { get; private set; }

        public static Color White { get; private set; }

        public static Color WhiteSmoke { get; private set; }

        public static Color Yellow { get; private set; }

        public static Color YellowGreen { get; private set; }

        internal string DebugDisplayString =>
            string.Concat(
                R.ToString(),
                " ",
                G.ToString(),
                " ",
                B.ToString(),
                " ",
                A.ToString());

        [SuppressMessage("ReSharper", "SX1309", Justification = "Keep this name as it is used by XNA games in reflection!")]
        private uint packedValue;

        static Color()
        {
            Transparent = new Color(0);
            AliceBlue = new Color(0xfffff8f0);
            AntiqueWhite = new Color(0xffd7ebfa);
            Aqua = new Color(0xffffff00);
            Aquamarine = new Color(0xffd4ff7f);
            Azure = new Color(0xfffffff0);
            Beige = new Color(0xffdcf5f5);
            Bisque = new Color(0xffc4e4ff);
            Black = new Color(0xff000000);
            BlanchedAlmond = new Color(0xffcdebff);
            Blue = new Color(0xffff0000);
            BlueViolet = new Color(0xffe22b8a);
            Brown = new Color(0xff2a2aa5);
            BurlyWood = new Color(0xff87b8de);
            CadetBlue = new Color(0xffa09e5f);
            Chartreuse = new Color(0xff00ff7f);
            Chocolate = new Color(0xff1e69d2);
            Coral = new Color(0xff507fff);
            CornflowerBlue = new Color(0xffed9564);
            Cornsilk = new Color(0xffdcf8ff);
            Crimson = new Color(0xff3c14dc);
            Cyan = new Color(0xffffff00);
            DarkBlue = new Color(0xff8b0000);
            DarkCyan = new Color(0xff8b8b00);
            DarkGoldenrod = new Color(0xff0b86b8);
            DarkGray = new Color(0xffa9a9a9);
            DarkGreen = new Color(0xff006400);
            DarkKhaki = new Color(0xff6bb7bd);
            DarkMagenta = new Color(0xff8b008b);
            DarkOliveGreen = new Color(0xff2f6b55);
            DarkOrange = new Color(0xff008cff);
            DarkOrchid = new Color(0xffcc3299);
            DarkRed = new Color(0xff00008b);
            DarkSalmon = new Color(0xff7a96e9);
            DarkSeaGreen = new Color(0xff8bbc8f);
            DarkSlateBlue = new Color(0xff8b3d48);
            DarkSlateGray = new Color(0xff4f4f2f);
            DarkTurquoise = new Color(0xffd1ce00);
            DarkViolet = new Color(0xffd30094);
            DeepPink = new Color(0xff9314ff);
            DeepSkyBlue = new Color(0xffffbf00);
            DimGray = new Color(0xff696969);
            DodgerBlue = new Color(0xffff901e);
            Firebrick = new Color(0xff2222b2);
            FloralWhite = new Color(0xfff0faff);
            ForestGreen = new Color(0xff228b22);
            Fuchsia = new Color(0xffff00ff);
            Gainsboro = new Color(0xffdcdcdc);
            GhostWhite = new Color(0xfffff8f8);
            Gold = new Color(0xff00d7ff);
            Goldenrod = new Color(0xff20a5da);
            Gray = new Color(0xff808080);
            Green = new Color(0xff008000);
            GreenYellow = new Color(0xff2fffad);
            Honeydew = new Color(0xfff0fff0);
            HotPink = new Color(0xffb469ff);
            IndianRed = new Color(0xff5c5ccd);
            Indigo = new Color(0xff82004b);
            Ivory = new Color(0xfff0ffff);
            Khaki = new Color(0xff8ce6f0);
            Lavender = new Color(0xfffae6e6);
            LavenderBlush = new Color(0xfff5f0ff);
            LawnGreen = new Color(0xff00fc7c);
            LemonChiffon = new Color(0xffcdfaff);
            LightBlue = new Color(0xffe6d8ad);
            LightCoral = new Color(0xff8080f0);
            LightCyan = new Color(0xffffffe0);
            LightGoldenrodYellow = new Color(0xffd2fafa);
            LightGray = new Color(0xffd3d3d3);
            LightGreen = new Color(0xff90ee90);
            LightPink = new Color(0xffc1b6ff);
            LightSalmon = new Color(0xff7aa0ff);
            LightSeaGreen = new Color(0xffaab220);
            LightSkyBlue = new Color(0xffface87);
            LightSlateGray = new Color(0xff998877);
            LightSteelBlue = new Color(0xffdec4b0);
            LightYellow = new Color(0xffe0ffff);
            Lime = new Color(0xff00ff00);
            LimeGreen = new Color(0xff32cd32);
            Linen = new Color(0xffe6f0fa);
            Magenta = new Color(0xffff00ff);
            Maroon = new Color(0xff000080);
            MediumAquamarine = new Color(0xffaacd66);
            MediumBlue = new Color(0xffcd0000);
            MediumOrchid = new Color(0xffd355ba);
            MediumPurple = new Color(0xffdb7093);
            MediumSeaGreen = new Color(0xff71b33c);
            MediumSlateBlue = new Color(0xffee687b);
            MediumSpringGreen = new Color(0xff9afa00);
            MediumTurquoise = new Color(0xffccd148);
            MediumVioletRed = new Color(0xff8515c7);
            MidnightBlue = new Color(0xff701919);
            MintCream = new Color(0xfffafff5);
            MistyRose = new Color(0xffe1e4ff);
            Moccasin = new Color(0xffb5e4ff);
            NavajoWhite = new Color(0xffaddeff);
            Navy = new Color(0xff800000);
            OldLace = new Color(0xffe6f5fd);
            Olive = new Color(0xff008080);
            OliveDrab = new Color(0xff238e6b);
            Orange = new Color(0xff00a5ff);
            OrangeRed = new Color(0xff0045ff);
            Orchid = new Color(0xffd670da);
            PaleGoldenrod = new Color(0xffaae8ee);
            PaleGreen = new Color(0xff98fb98);
            PaleTurquoise = new Color(0xffeeeeaf);
            PaleVioletRed = new Color(0xff9370db);
            PapayaWhip = new Color(0xffd5efff);
            PeachPuff = new Color(0xffb9daff);
            Peru = new Color(0xff3f85cd);
            Pink = new Color(0xffcbc0ff);
            Plum = new Color(0xffdda0dd);
            PowderBlue = new Color(0xffe6e0b0);
            Purple = new Color(0xff800080);
            Red = new Color(0xff0000ff);
            RosyBrown = new Color(0xff8f8fbc);
            RoyalBlue = new Color(0xffe16941);
            SaddleBrown = new Color(0xff13458b);
            Salmon = new Color(0xff7280fa);
            SandyBrown = new Color(0xff60a4f4);
            SeaGreen = new Color(0xff578b2e);
            SeaShell = new Color(0xffeef5ff);
            Sienna = new Color(0xff2d52a0);
            Silver = new Color(0xffc0c0c0);
            SkyBlue = new Color(0xffebce87);
            SlateBlue = new Color(0xffcd5a6a);
            SlateGray = new Color(0xff908070);
            Snow = new Color(0xfffafaff);
            SpringGreen = new Color(0xff7fff00);
            SteelBlue = new Color(0xffb48246);
            Tan = new Color(0xff8cb4d2);
            Teal = new Color(0xff808000);
            Thistle = new Color(0xffd8bfd8);
            Tomato = new Color(0xff4763ff);
            Turquoise = new Color(0xffd0e040);
            Violet = new Color(0xffee82ee);
            Wheat = new Color(0xffb3def5);
            White = new Color(uint.MaxValue);
            WhiteSmoke = new Color(0xfff5f5f5);
            Yellow = new Color(0xff00ffff);
            YellowGreen = new Color(0xff32cd9a);
        }

        public Color(Vector4 color)
        {
            packedValue = 0;

            R = (byte)MathHelper.Clamp(color.X * 255, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, byte.MinValue, byte.MaxValue);
            A = (byte)MathHelper.Clamp(color.W * 255, byte.MinValue, byte.MaxValue);
        }

        public Color(Vector3 color)
        {
            packedValue = 0;

            R = (byte)MathHelper.Clamp(color.X * 255, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, byte.MinValue, byte.MaxValue);
            A = 255;
        }

        public Color(float r, float g, float b)
        {
            packedValue = 0;

            R = (byte)MathHelper.Clamp(r * 255, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, byte.MinValue, byte.MaxValue);
            A = 255;
        }

        public Color(int r, int g, int b)
        {
            packedValue = 0;
            R = (byte)MathHelper.Clamp(r, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, byte.MinValue, byte.MaxValue);
            A = 255;
        }

        public Color(int r, int g, int b, int alpha)
        {
            packedValue = 0;
            R = (byte)MathHelper.Clamp(r, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, byte.MinValue, byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha, byte.MinValue, byte.MaxValue);
        }

        public Color(float r, float g, float b, float alpha)
        {
            packedValue = 0;

            R = (byte)MathHelper.Clamp(r * 255, byte.MinValue, byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, byte.MinValue, byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, byte.MinValue, byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha * 255, byte.MinValue, byte.MaxValue);
        }

        private Color(uint packedValue)
        {
            this.packedValue = packedValue;
        }

        public bool Equals(Color other)
        {
            return PackedValue == other.PackedValue;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public static Color Lerp(Color value1, Color value2, float amount)
        {
            amount = MathHelper.Clamp(amount, 0.0f, 1.0f);
            return new Color(
                (int)MathHelper.Lerp(value1.R, value2.R, amount),
                (int)MathHelper.Lerp(value1.G, value2.G, amount),
                (int)MathHelper.Lerp(value1.B, value2.B, amount),
                (int)MathHelper.Lerp(value1.A, value2.A, amount));
        }

        public static Color FromNonPremultiplied(Vector4 vector)
        {
            return new Color(
                vector.X * vector.W,
                vector.Y * vector.W,
                vector.Z * vector.W,
                vector.W);
        }

        public static Color FromNonPremultiplied(int r, int g, int b, int a)
        {
            return new Color(
                r * a / 255,
                g * a / 255,
                b * a / 255,
                a);
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.A == b.A &&
                   a.R == b.R &&
                   a.G == b.G &&
                   a.B == b.B;
        }

        public static bool operator !=(Color a, Color b)
        {
            return !(a == b);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
        public override int GetHashCode()
        {
            return packedValue.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is Color color && Equals(color);
        }

        public static Color Multiply(Color value, float scale)
        {
            return new Color(
                (int)(value.R * scale),
                (int)(value.G * scale),
                (int)(value.B * scale),
                (int)(value.A * scale));
        }

        public static Color operator *(Color value, float scale)
        {
            return new Color(
                (int)(value.R * scale),
                (int)(value.G * scale),
                (int)(value.B * scale),
                (int)(value.A * scale));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(25);
            sb.Append("{R:");
            sb.Append(R);
            sb.Append(" G:");
            sb.Append(G);
            sb.Append(" B:");
            sb.Append(B);
            sb.Append(" A:");
            sb.Append(A);
            sb.Append("}");
            return sb.ToString();
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            // Should we round here?
            R = (byte)(vector.X * 255.0f);
            G = (byte)(vector.Y * 255.0f);
            B = (byte)(vector.Z * 255.0f);
            A = (byte)(vector.W * 255.0f);
        }
    }
}

// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace Katabasis
{
	public class ColorConverter : MathTypeConverter
	{
		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value)
		{
			if (value is string s)
			{
				string[] v = s.Split(
					culture.TextInfo.ListSeparator.ToCharArray());

				return new Color(
					int.Parse(v[0], culture),
					int.Parse(v[1], culture),
					int.Parse(v[2], culture),
					int.Parse(v[3], culture));
			}

			return base.ConvertFrom(context, culture, value)!;
		}

		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				var src = (Color)value;
				return string.Join(
					culture.TextInfo.ListSeparator + " ",
					src.R.ToString(culture),
					src.G.ToString(culture),
					src.B.ToString(culture),
					src.A.ToString(culture));
			}

			return base.ConvertTo(context, culture, value, destinationType)!;
		}

		public override object CreateInstance(
			ITypeDescriptorContext context,
			IDictionary propertyValues) =>
			new Color(
				(int)(propertyValues["R"] ?? byte.MaxValue),
				(int)(propertyValues["G"] ?? byte.MaxValue),
				(int)(propertyValues["B"] ?? byte.MaxValue),
				(int)(propertyValues["A"] ?? byte.MaxValue));
	}
}

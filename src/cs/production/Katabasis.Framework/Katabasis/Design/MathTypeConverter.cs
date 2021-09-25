// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.ComponentModel;

namespace Katabasis
{
	public abstract class MathTypeConverter : ExpandableObjectConverter
	{
		protected PropertyDescriptorCollection _propertyDescriptions = null!;

		protected bool _supportStringConvert;

		protected MathTypeConverter() => _supportStringConvert = true;

		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			if (_supportStringConvert && sourceType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
		{
			if (_supportStringConvert && destinationType == typeof(string))
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}

		public override bool GetCreateInstanceSupported(ITypeDescriptorContext? context) => true;

		public override PropertyDescriptorCollection GetProperties(
			ITypeDescriptorContext? context,
			object value,
			Attribute[]? attributes) =>
			_propertyDescriptions;

		public override bool GetPropertiesSupported(ITypeDescriptorContext? context) => true;
	}
}

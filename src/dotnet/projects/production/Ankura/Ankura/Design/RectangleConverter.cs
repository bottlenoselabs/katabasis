// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections;
using System.ComponentModel;

namespace Ankura
{
    public class RectangleConverter : MathTypeConverter
    {
        public RectangleConverter()
        {
            // FIXME: Initialize propertyDescriptions... how? -flibit
            _supportStringConvert = false;
        }

        public override object CreateInstance(
            ITypeDescriptorContext context,
            IDictionary propertyValues)
        {
            return new Rectangle(
                (int)(propertyValues["X"] ?? 0),
                (int)(propertyValues["Y"] ?? 0),
                (int)(propertyValues["Width"] ?? 0),
                (int)(propertyValues["Height"] ?? 0));
        }
    }
}

// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

// ReSharper disable once CheckNamespace
namespace ObjCRuntime
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class MonoPInvokeCallbackAttribute : Attribute
    {
        // ReSharper disable once UnusedParameter.Local
        public MonoPInvokeCallbackAttribute(Type t)
        {
        }
    }
}

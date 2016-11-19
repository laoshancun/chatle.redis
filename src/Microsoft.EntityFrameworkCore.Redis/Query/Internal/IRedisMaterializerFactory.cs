// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public interface IRedisMaterializerFactory
    {
        Expression<Func<IEntityType, ValueBuffer, object>> CreateMaterializer([NotNull] IEntityType entityType);
    }
}

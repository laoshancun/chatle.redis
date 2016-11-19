// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public interface IRedisTable
    {
        IReadOnlyList<object[]> SnapshotRows();

        void Create([NotNull] IUpdateEntry entry);

        void Delete([NotNull] IUpdateEntry entry);

        void Update([NotNull] IUpdateEntry entry);
    }
}

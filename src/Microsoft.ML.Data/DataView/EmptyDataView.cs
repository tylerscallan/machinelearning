// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.ML.Internal.Utilities;

namespace Microsoft.ML.Data
{
    /// <summary>
    /// This implements a data view that has a schema, but no rows.
    /// </summary>
    public sealed class EmptyDataView : IDataView
    {
        private readonly IHost _host;

        public bool CanShuffle => true;
        public Schema Schema { get; }

        public EmptyDataView(IHostEnvironment env, Schema schema)
        {
            Contracts.CheckValue(env, nameof(env));
            _host = env.Register(nameof(EmptyDataView));
            _host.CheckValue(schema, nameof(schema));
            Schema = schema;
        }

        public long? GetRowCount() => 0;

        public RowCursor GetRowCursor(IEnumerable<Schema.Column> columnsNeeded, Random rand = null)
        {
            _host.CheckValueOrNull(rand);
            return new Cursor(_host, Schema, columnsNeeded);
        }

        public RowCursor[] GetRowCursorSet(IEnumerable<Schema.Column> columnsNeeded, int n, Random rand = null)
        {
            _host.CheckValueOrNull(rand);
            return new[] { new Cursor(_host, Schema, columnsNeeded) };
        }

        private sealed class Cursor : RootCursorBase
        {
            private readonly bool[] _active;

            public override Schema Schema { get; }
            public override long Batch => 0;

            public Cursor(IChannelProvider provider, Schema schema, IEnumerable<Schema.Column> columnsNeeded)
                : base(provider)
            {
                Ch.AssertValue(schema);
                Schema = schema;
                _active = Utils.BuildArray(Schema.Count, columnsNeeded);
            }

            public override ValueGetter<RowId> GetIdGetter()
            {
                return (ref RowId val) => throw Ch.Except(RowCursorUtils.FetchValueStateError);
            }

            protected override bool MoveNextCore() => false;

            public override bool IsColumnActive(int col) => 0 <= col && col < _active.Length && _active[col];

            public override ValueGetter<TValue> GetGetter<TValue>(int col)
            {
                Ch.Check(IsColumnActive(col), "Cannot get getter for inactive column");
                return (ref TValue value) => throw Ch.Except(RowCursorUtils.FetchValueStateError);
            }
        }
    }
}
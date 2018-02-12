using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestWebApiAzure.TableStores
{
    public static class TableStorageExtensions
    {
        public static async Task<IReadOnlyCollection<T>> ExecuteQueryAsync<T>(this CloudTable table, 
            TableQuery<T> query, CancellationToken ct = default(CancellationToken), 
            Action<IReadOnlyCollection<T>> onProgress = null) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;
            do
            {

                TableQuerySegment<T> seg = await table
                    .ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                onProgress?.Invoke(items);
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }
    }
}
// MIT License Copyright 2014 (c) David Melendez. All rights reserved. See License.txt in the project root for license information.

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if net45
namespace ElCamino.AspNet.Identity.Dynamo.Helpers
#else
namespace ElCamino.AspNetCore.Identity.Dynamo.Helpers
#endif
{
    /// <summary>
    /// Used to instantiate multiple WriteRequests when the 
    /// WriteRequests maximum is reached on a single BatchWriteItemRequest
    /// </summary>
    internal class BatchOperationHelper
    {
        /// <summary>
        /// Current max operations supported in a BatchWriteItem
        ///http://aws.amazon.com/about-aws/whats-new/2012/04/19/dynamodb-announces-batchwriteitem/
        /// </summary>
        public const int MaxOperationsPerBatch = 25;

        private readonly List<BatchWriteItemRequest> _batches = new List<BatchWriteItemRequest>(100);

        public BatchOperationHelper() { }

        /// <summary>
        /// Adds a WriteRequest to a BatchWriteItemRequest by table name
        /// and automatically adds a new WriteRequest if max BatchWriteItemRequest.RequestItems[tableName].WriteRequests are 
        /// exceeded.
        /// </summary>
        /// <param name="operation"></param>
        public void Add(string tableName, WriteRequest operation)
        {
            BatchWriteItemRequest current = GetCurrent(tableName);
            current.RequestItems[tableName].Add(operation);
        }

        private BatchWriteItemRequest CreateBatchWriteItemRequest(string tableName)
        {
            var batchWriteReq = new BatchWriteItemRequest();
            batchWriteReq.RequestItems = new Dictionary<string, List<WriteRequest>>(10);
            batchWriteReq.RequestItems.Add(tableName, new List<WriteRequest>(MaxOperationsPerBatch));
            return batchWriteReq;
        }

        public async Task<IList<BatchWriteItemResponse>> ExecuteBatchAsync(AmazonDynamoDBClient client)
        {
            return await new TaskFactory<IList<BatchWriteItemResponse>>().StartNew(
            () =>
            {
                ConcurrentBag<BatchWriteItemResponse> results = new ConcurrentBag<BatchWriteItemResponse>();
#if net45
                Parallel.ForEach(_batches,
#else
                _batches.ForEach(
#endif
                async (batchWriteReq) =>
                {
                    var x = await client.BatchWriteItemAsync(batchWriteReq);
                    results.Add(x);
                });
                Clear();
                return results.ToList();
            });
        }

        public void Clear()
        {
            _batches.Clear();
        }

        private BatchWriteItemRequest GetCurrent(string tableName)
        {
            var current = _batches
                .FirstOrDefault(b => b.RequestItems
                    .Any(k => tableName.Equals(k.Key, StringComparison.OrdinalIgnoreCase) && k.Value.Count < MaxOperationsPerBatch));
            if (current == null)
            {
                current = CreateBatchWriteItemRequest(tableName);
                _batches.Add(current);
            }

            return current;
        }
    }
}

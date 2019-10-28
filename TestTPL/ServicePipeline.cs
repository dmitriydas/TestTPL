using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using CSharpFunctionalExtensions;

namespace TestTPL
{
    public class ServicePipeline
    {
        public const int batches = 100;
        private int currentBatch = 0;

        public ServicePipeline(int maxRequestsInParallel)
        {
            MaxRequestsInParallel = maxRequestsInParallel;
        }

        public int MaxRequestsInParallel { get; }
        public BufferBlock<MyData> QueueBlock { get; private set; }
        public List<TransformBlock<MyData, Result>> ExecutionBlocks { get; private set; }
        public ActionBlock<Result> ResultBlock { get; private set; }

        private void Init()
        {
            QueueBlock = new BufferBlock<MyData>(new DataflowBlockOptions() { BoundedCapacity = MaxRequestsInParallel });
            ExecutionBlocks = new List<TransformBlock<MyData, Result>>();
            ResultBlock = new ActionBlock<Result>(_ => _.OnFailure(() => Console.WriteLine($"Error: {_.Error}")));

            for (int blockIndex = 0; blockIndex < MaxRequestsInParallel; blockIndex++)
            {
                var executionBlock = new TransformBlock<MyData, Result>((d) =>
                {
                    return ProcessService.ExecuteAsync(d);
                }, new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });
                executionBlock.LinkTo(ResultBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                QueueBlock.LinkTo(executionBlock, new DataflowLinkOptions() { PropagateCompletion = true });
                ExecutionBlocks.Add(executionBlock);
            }
        }

        public async void Start()
        {
            Init();
            while (currentBatch < batches)
            {
                Thread.Sleep(1000);
                await SubmitNextRequests();
            }
            Console.WriteLine($"Completed: {batches}");
        }

        private async Task<int> SubmitNextRequests()
        {
            var emptySlots = MaxRequestsInParallel - QueueBlock.Count;
            Console.WriteLine($"Empty slots: {emptySlots}, left = {batches - currentBatch}");
            if (emptySlots > 0)
            {
                var dataRequests = await GetNextRequests(emptySlots);
                foreach (var data in dataRequests)
                {
                    await QueueBlock.SendAsync(data);
                }
            }
            return emptySlots;
        }

        private async Task<List<MyData>> GetNextRequests(int request)
        {
            MyData[] myDatas = new MyData[request];
            Task<List<MyData>> task = Task<List<MyData>>.Run(() =>
            {
                for (int i = 0; i < request; i++)
                {
                    myDatas[i++] = new MyData(currentBatch);
                    currentBatch++;
                }
                return new List<MyData>(myDatas);
            });
            return await task;
        }
    }

    public class MyData
    {
        public int Id { get; set; }
        public MyData(int id) => Id = id;
        public override string ToString() { return Id.ToString(); }
    }
}

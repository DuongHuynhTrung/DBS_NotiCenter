using System.Collections.Concurrent;

namespace Background
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private static readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
        }

        public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Func<CancellationToken, Task> workItem;
                while (!_workItems.TryDequeue(out workItem))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    Thread.Sleep(100); // Đợi 100ms trước khi thử lại
                }

                return workItem;
            }, cancellationToken);
        }
    }
}


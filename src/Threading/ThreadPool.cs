using System;
using System.Threading;

namespace PaintDotNet.Threading
{
	/// <summary>
	/// Uses the .NET ThreadPool to do our own type of thread pool. The main difference
	/// here is that we limit our usage of the thread pool, and that we can also drain
	/// the threads we have ("fence"). The default maximum number of threads is four
	/// times the logical CPU count. So on a Dual Xeon w/ HyperThreading, this equals 
	/// 16 (four logical CPUs).
	/// </summary>
	public class ThreadPool
	{
        private WaitableCounter counter;

		public ThreadPool()
            : this(PaintDotNet.Utility.LogicalCpuCount * 4)
		{
		}

        public ThreadPool(int maxThreads)
        {
            counter = new WaitableCounter(maxThreads);
        }

        public void QueueUserWorkItem(WaitCallback callback)
        {
            QueueUserWorkItem(callback, null);
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            IDisposable token = counter.AcquireToken();
            ThreadWrapperContext twc = new ThreadWrapperContext(callback, state, token);
            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadWrapper), twc);
        }

        public void Drain()
        {
            counter.WaitForEmpty();
        }

        private class ThreadWrapperContext
        {
            public WaitCallback Callback;
            public object Context;
            public IDisposable CounterToken;

            public ThreadWrapperContext(WaitCallback callback, object context, IDisposable counterToken)
            {
                this.Callback = callback;
                this.Context = context;
                this.CounterToken = counterToken;
            }
        }

        private void ThreadWrapper(object state)
        {
            using (IDisposable token = ((ThreadWrapperContext)state).CounterToken)
            {
                ThreadWrapperContext context = (ThreadWrapperContext)state;
                context.Callback(context.Context);
            }
        }
	}
}

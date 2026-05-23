/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace PaintDotNet.Threading
{
	/// <summary>
	/// Uses the .NET ThreadPool to do our own type of thread pool. The main difference
	/// here is that we limit our usage of the thread pool, and that we can also drain
	/// the threads we have ("fence"). The default maximum number of threads is 16.
	/// </summary>
	public class ThreadPool
	{
        public static int MinimumCount
        {
            get
            {
                return WaitableCounter.MinimumCount;
            }
        }

        public static int MaximumCount
        {
            get
            {
                return WaitableCounter.MaximumCount;
            }
        }

        private WaitableCounter counter;

		public ThreadPool()
            : this(16)
		{
		}

        public ThreadPool(int maxThreads)
        {
            if (maxThreads < MinimumCount || maxThreads > MaximumCount)
            {
                throw new ArgumentOutOfRangeException("maxThreads", "must be between " + MinimumCount.ToString() + " and " + MaximumCount.ToString() + " inclusive");
            }

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

        private sealed class ThreadWrapperContext
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
            ThreadWrapperContext context = (ThreadWrapperContext)state;

            using (IDisposable token = context.CounterToken)
            {
                context.Callback(context.Context);
            }
        }
	}
}

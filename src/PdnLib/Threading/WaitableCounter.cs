using System;
using System.Diagnostics;
using System.Threading;

namespace PaintDotNet.Threading
{
	/// <summary>
	/// Threading primitive that allows you to "count" and to wait on two conditions:
	/// 1. Empty -- this is when we have not dished out any "tokens"
	/// 2. NotFull -- this is when we currently have 1 or more "tokens" out in the wild
	/// </summary>
	public class WaitableCounter
	{
        private class CounterToken
            : IDisposable
        {
            WaitableCounter owner;

            public CounterToken(WaitableCounter owner)
            {
                this.owner = owner;
            }

            ~CounterToken()
            {
                ////Debug.WriteLine("WaitableCounter.Finalize(), hash=" + GetHashCode().ToString());
                Dispose(false);
            }

            private bool disposed = false;
            public void Dispose()
            {
                ////Debug.WriteLine("WaitableCounter.Dispose(), hash=" + GetHashCode().ToString());
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                ////Debug.WriteLine("WaitableCounter.Dispose(" + disposing.ToString() + "), hash=" + GetHashCode().ToString());
                if (!disposed)
                {
                    disposed = true;

                    if (disposing)
                    {
                        owner.ReleaseToken(this);
                        owner = null;
                    }
                }
            }
        }

        private int count;
        private int maxCount;
        private ManualResetEvent emptyEvent;
        private ManualResetEvent notFullEvent;

        private void ReleaseToken(CounterToken token)
        {
            try
            {
            }

            finally
            {
                ////Debug.WriteLine("ReleaseToken (" + count.ToString() + ")");

                lock (this)
                {
                    int newCount = Interlocked.Decrement(ref count);

                    if (newCount == 0)
                    {
                        emptyEvent.Set();
                    }

                    notFullEvent.Set();
                }
            }
        }

        public IDisposable AcquireToken()
        {
            CounterToken token = null;
            ////Debug.WriteLine("AcquireToken (" + count.ToString() + ")");

            try
            {
                while (token == null)
                {
                    WaitForNotFull();

                    lock (this)
                    {
                        if (count < maxCount)
                        {
                            Interlocked.Increment(ref count);
                            token = new CounterToken(this);

                            if (count == maxCount)
                            {
                                notFullEvent.Reset();
                            }

                            emptyEvent.Reset();
                        }
                    }
                }
            }

            catch
            {
                if (token != null)
                {
                    token.Dispose();
                }
            }

            return token;
        }

        public void WaitForEmpty()
        {
            emptyEvent.WaitOne();
        }

        public void WaitForNotFull()
        {
            notFullEvent.WaitOne();
        }

		public WaitableCounter(int maxCount)
		{
            this.count = 0;
            this.maxCount = maxCount;
            emptyEvent = new ManualResetEvent(true);
            notFullEvent = new ManualResetEvent(true);
		}
	}
}

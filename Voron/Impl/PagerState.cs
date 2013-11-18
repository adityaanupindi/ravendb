﻿using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using Voron.Impl.Paging;

namespace Voron.Impl
{
    public unsafe class PagerState
    {
#if DEBUG
        public static ConcurrentDictionary<PagerState, StackTrace> Instances = new ConcurrentDictionary<PagerState, StackTrace>();

        public PagerState()
        {
            Instances[this] = new StackTrace(true);
        }

#endif

        private int _refs;

        public MemoryMappedViewAccessor Accessor;

        public MemoryMappedFile File;

	    public byte* MapBase { get; set; }

	    public void Release()
        {
            if (Interlocked.Decrement(ref _refs) != 0)
                return;

#if DEBUG
            StackTrace value;
            Instances.TryRemove(this, out value);
#endif

            if (Accessor != null)
            {
                Accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                Accessor.Dispose();
            }
            if (File != null)
                File.Dispose();
        }

#if DEBUG
        public ConcurrentQueue<StackTrace> AddedRefs = new ConcurrentQueue<StackTrace>();
#endif

        public void AddRef()
        {
            Interlocked.Increment(ref _refs);
#if DEBUG
            AddedRefs.Enqueue(new StackTrace(true));
	        while (AddedRefs.Count > 500)
	        {
		        StackTrace trace;
		        AddedRefs.TryDequeue(out trace);
	        }
#endif
        }
    }
}
import multiprocessing as mp
from multiprocessing import Queue 
from joblib import Parallel, delayed
from time import sleep

import functools
'''
def Queued(func):
    @functools.wraps(func)
    def _inner(*args, **kwargs):
        key = tuple(args + sorted( kwargs.items() ))
        func.cache = getattr(func, 'cache', {})
        if key not in func.cache:
            func.cache[key] = func(*args)
        return func.cache[key]
    return _inner
'''    
    
def Process(q):
    def _wrap(f):
        def __inner(*args, **kwargs):
            if getattr(f, 'queued', None) is None:
                f.queued = lambda *args, **kwargs: q.put(func(*args, **kwargs))
            return mp.Process(target=f.queued, args=args, kwargs=kwargs)
        return __inner
    return _wrap


def foo(i):
    sleep(1)
    return i

output = Queue()

@Process(output)
def bar(i):
    sleep(1)
    return i

def Execute(processes, output=None):
    for p in processes:
        p.start()

    for p in processes:
        p.join()

    if output is None:
	return None
    else:
    	return [output.get() for p in processes]
    
        
def test1(N=10):
    processes = [bar(i) for i in xrange(N)]

    return Execute(processes)

def test2(N=10, nj=4):
    return Parallel(n_jobs=4)(delayed(foo)(i) for i in xrange(N))





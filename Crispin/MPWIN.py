import multiprocessing as mp
from multiprocessing import Queue 

from time import sleep


def foo(i, q):
    sleep(1)
    return q.put(i)


if __name__ == '__main__':
    processes = []
    output = Queue()
 
    
    for i in xrange(10):
        processes.append( mp.Process(target=foo, args=(i,output)) )
    '''    
    for p in processes:
        p.start()

    for p in processes:
        p.join()

    print [output.get() for p in processes]
    
    q = Queue()
    output = []

    for i in xrange(10):
        p = mp.Process(target=Queued(foo)(q), args=(i,))
        processes.append(p)
        p.start()
    
    for p in processes:
        p.join()
        output.append( q.get() )
    '''
    



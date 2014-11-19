from Function_Module import *
from Large_Currency import curr

path = lambda *args: '/'.join(map(str,args))
mkpath = lambda path: bash_command('mkdir -p ' + path)

def Timer(func):
    import time
    def _inner(*args, **kwargs):
        T1 = time.time()
        Results = func(*args, **kwargs)
        T2 = time.time()
        return Results, T2 - T1
    return _inner

    
@Timer
def Bhanu_code():
    command = "echo '' | mono WITP-TPH.exe"
    output = bash_command(command)
     
    Last_line = output[-1]
    assert 'minimum cost' in Last_line
    
    S = float(Last_line.split()[-1])      
    return S   


@Timer
def Bhanu_run(cpath, N=10):
    BS, BT = [], []
    for idx in xrange(1, N+1):
        Instance_path = path(cpath,'Instance_{}'.format(idx))
        mkpath(Instance_path)
        
        qprint( "Instance {} of {}:".format(idx, N) )
          
        S, T = Bhanu_code()             
 
        qprint('RUN TIME [Bhanu]: ' + ptime(T), t=1)    
        qprint('MIN COST [Bhanu]: ' + curr(S), t=1, n=1)
                     
        BT.append(T)
        BS.append(S)
        
        sol_file = '{}/Solution_{}.txt'.format(Instance_path,idx)
        
        bash_command('cp *Sol* {}'.format(sol_file))
        bash_command('rm *Sol*')
        
        bash_command('cp TPH.cs {}/'.format(Instance_path))
        bash_command('cp WITP-TPH.exe {}/'.format(Instance_path))
        bash_command('cp WITPdataSet* {}/'.format(Instance_path)) 
    return sum(BT)       



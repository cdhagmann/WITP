from Function_Module import *


B_timeout = 2 * 60 * 60
B_tol = 1
B_setup = 'gc.enable()\nfrom Bhanu_code import Bhanu_code'
B_command ='Bhanu_code()'

def Bhanu_compile():
    bash_command('cp TPH.cs BhanuCode/WITP-TPH/TPH.cs')
    bash_command('xbuild BhanuCode/WITP-TPH.sln')
    bash_command('rm -f WITP-TPH.exe')
    bash_command('cp BhanuCode/WITP-TPH/bin/Debug/WITP-TPH.exe ./')
    bash_command('cp BhanuCode/WITP-TPH/TPH.cs ./')
    return True
    
def Bhanu_code():
    command = "echo '' | mono WITP-TPH.exe"
    output = bash_command(command) 
    Last_line = output[-1]
    assert 'minimum cost' in Last_line
    S = float(Last_line.split()[-1])      
    return S

def read_B_sol(sol_file):
    with open(sol_file,'r') as f:
        lines = f.readlines()
    
    L = [l for l in lines if 'Total' in l]
    return float(L[-1].split()[-1])

def B_M1_prep():
    bash_command('rm -f Sol*')
    bash_command('rm -f WITPdataSet*')
    
def Bhanu_cleanup(path):
    idx = num_strip(path.split('/')[-1])
    sol_file = '{}/Solution_{}.txt'.format(path,idx)
    
    bash_command('cp *Sol* {}'.format(sol_file))
    bash_command('rm *Sol*')
    bash_command('cp TPH.cs {}/'.format(path))
    bash_command('cp WITP-TPH.exe {}/'.format(path))
    bash_command('cp WITPdataSet* {}/'.format(path))
    
    return read_B_sol(sol_file)

if '__main__' == __name__:
    print 'Compiling changes...'
    
    Bhanu_compile()
    obj = []
    
    print 'Starting Test'
    import time
    T1, T2 = time.time(), time.time()
    while T2 - T1 < 2 * 60 and len(obj) < 30:
        print '{0:3} - {1}'.format(len(obj), time.ctime())
        obj.append(Bhanu_code())
        T2 = time.time()
    else:
        print time.ctime()
        obj.sort()
        print ''
    
    print len(obj), min(obj), max(obj)

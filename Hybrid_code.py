from Pyomo_code import *
import time

def Bhanu_run(reset=False, count={}):
    if reset:
        count = {}
        return None
        
    bash_command('rm *Sol*')
    
    command = "echo '' | mono WITP-TPH.exe"
    output = bash_command(command) 
    
    bash_command('cp *Sol* temp.txt')
    with open('temp.txt','r') as f:
        lines = f.readlines()    
    bash_command('rm temp.txt')
    
    TC = [tuple(map(int, l.split()[1:3])) for l in lines[-7:-2]]
    
    tech_choices = []
    for tup in TC:
        if tup not in tech_choices:
            tech_choices.append(tup)
            
    # print tech_choices        
    
    for idx, (i, j) in enumerate(tech_choices):
        index = (i - 1) * 6 + (j - 1)
        count[index] = count.get(index, 0) + (len(tech_choices) - idx)
        
    return count    
    
def Hybrid_code():
    Bhanu_run(reset=True)
    T1, T2 = time.time(), time.time()
    C = 0
    while T2 - T1 < 1 * 60 and C < 30:
        count = Bhanu_run()
        # print count
        if max(count.values()) > 25:
            break
            
        T2 = time.time()
        C += 1
        print C

    L = sorted(count.items(), key=lambda (idx, c): c, reverse=True)
    indices = [idx for idx, c in L[:5]]
    
    # print L
    # print indices
        
    T, S = Pyomo_code(indices=indices, cutoff=True, gap=.02)
    
    T2 = time.time()
    return T2 - T1, S
    
def idx2pair(idx):
    return int(idx)/6 + 1, int(idx)%6 + 1
    
def pair2idx(ij):
    i,j = map(int, ij)
    return (i - 1) * 6 + (j - 1)
    
if '__main__' == __name__:   
    print Hybrid_code()



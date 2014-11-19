#-----------------------------------------------------------------------------
#                          IMPORT MODULES
#-----------------------------------------------------------------------------

import Instance2
from itertools import product
from Function_Module import *
from Bhanu_code import *
from Pyomo_code import *
import glob
from Master import main as Master

###########################################################
# instance_generator(S,V,P,Q,T,DEFAULT=True)
###########################################################

if True:
    S = [5, 10, 25]
    V = [2, 5]
    P = [10, 20, 50, 100]
    T = [5]
elif True:
    S = [5, 10]
    V = [2, 5]
    P = [10, 20]
    T = [5]
else:
    S = [1,2]
    V = [2]
    P = [1,2]
    T = [5]



case_list_indices = [(s,v,p,t) for s,v,p,t in product(S,V,P,T)
                                   if 2 * p >= v]
                                   
case_list = [(s,v,p,p,t) for s,v,p,t in case_list_indices]
cases = sorted(case_list, key=lambda (s,v,p,q,t): 2.5 * s + v + p)

cases = [(5,2,10,10,5), (25,10,250,250,10), (100, 20, 1000, 1000, 14), (250, 50, 5000,5000,28)]
N = len(cases)
# Different instances of the same size
M1 = 1
# Identical instances runs
M2 = 6


AD = [15000, 12500, 10000, 7500, 5000, 2500, 1000, 500, 200, 50]
AD = map(float, AD)
###########################################################
###########################################################
###########################################################
# Code will run N * M1 * M2 times

path = lambda *args: '/'.join(map(str,args))
mkpath = lambda path: bash_command('mkdir -p ' + path)
    
def main(ID, resume=0):
    if isinstance(resume, (int, float)):
        exclude = range(resume)
    else:
        exclude = list(resume)
        
    TestID = 'TEST ID: {}\n'.format(ID)

    foldername = 'Results_' + ID
    mkpath(foldername)
    
    overview = path(foldername,'Overview.txt')

    for idx,case in enumerate(cases,1):
        if idx in exclude:
            continue      
        BT, BS = [], []
        PT, PS = [], []
        S,V,P,Q,T = case
        AD = (4000. if S > 20 else 1000.) / (P+Q)
        cpath = path(foldername,'Case_{}'.format(idx))
        mkpath(cpath)
        
        string = TestID + "[{}] (Case {} of {}):".format(','.join(map(str,case)),idx,N)
        
        print line('@', 72)
        tee_print(overview, string)
        tee_print(overview, time.strftime("%a, %d %b %Y %I:%M:%S %p"), n=1)
        print line('@', 72) + '\n'
        
        for m1 in xrange(M1):
            B_timed_out = 0            
            B_M1_prep()
            
            P_timed_out = 0            
            P_M1_prep()            
            
            print line('*', 72)
            print 'AD = {0:.0f}'.format(AD) 
            Instance2.instance_generator(*case, AD=AD)
            print line('*', 72) + '\n'

            Master(cpath, N=M2)
        
        
                                        
if __name__ == '__main__':
    try:
        finished = False
        ID = id_generator()
        resume = []
        # ID = 'ON8VTJ'
        # resume = [3,4,6,8,9,10,11,12,15,16,17,18]
        main(ID, resume)
        finished = True
    except KeyboardInterrupt:
        finished = False
    except Exception as e:
        print e
    finally:
        bash_command('mv streaming_output Results_{}/Console_output.txt'.format(ID))
        try:
            case_count = len(glob.glob('Results_{}/Case*'.format(ID)))
            if case_count <= 2 and not finished:
                pass# bash_command('rm -r Results_{}'.format(ID))
            else:
                if not finished:
                    path = 'Results_{}/Case_{}/Pyomo_Benchmark'.format(ID, idx)
                    bash_command('cp results_* {}/'.format(path))          
                    bash_command('cp summary_* {}/'.format(path))                  
                    bash_command('cp PyomoCode/ReferenceModel* {}/'.format(path))     
                if False:
                    results = 'Results_{0}.zip'.format(ID)
                    zip_command = 'zip -r {} Results_{}/ -x \*.exe\*'.format(results, ID)
                    bash_command(zip_command)
                
                    
                    CC = 'chagmann@purdue.edu'
                    To = 'BSainathuni@manh.com'
                    results = 'Results_{0}.zip'.format(ID)
                    sub = '"Results from {0}"'.format(ID)
                    text = 'Results_{0}/Overview.txt'.format(ID)
                    args = [sub, To, CC, results, text]
                    mutt_command = 'mutt -s {} {} -c {} -a {} < {}'.format(*args)
                    # print mutt_command
                    bash_command(mutt_command)    
        finally:
            bash_command('rm -f results_*')
            #bash_command('rm -f streaming_output')
            bash_command('rm -f summary_*')
            bash_command('rm -f *log')        
            bash_command('rm -f ReferenceModel*') 
            bash_command('rm -f WITP*')
            bash_command('rm -f TPH.cs')
            bash_command('rm -f *pyc')
            bash_command('clear')
            print 'Clean up Sucessful!'
        
           

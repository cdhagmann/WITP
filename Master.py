from Function_Module import *
from Master_code import *
import sys

        
def main(cpath, Type, N=6):
    overview = path(cpath,'Overview.txt')
    
    pyomo_code = False
    if Type in ('S', 'Test'):
        GG = .02
        pyomo_code = True
    elif Type == 'M':
        GG = .15
    elif Type == 'L':
        GG = .30
    else:
        GG = None
        pyomo_code = True
        
          
    (BS, BT, HS), HT = Hybrid_code(cpath, N=N, GG=GG)
    print
    if Pyomo_code:
        PS, PT = Pyomo_wrapper(cpath)
        
    print line('#',10) 
    tee_print(overview,'RESULTS [Bhanu]: ',t=0, n=0)
    tee_print(overview,'MEAN:  {0:.2f} seconds'.format(mean(BT)),t=1, n=0)
    tee_print(overview,'STDEV: {0:.2f} seconds'.format(stdev(BT)),t=1, n=0)
    tee_print(overview,'MIN:   {0:.2f} seconds'.format(min(BT)),t=1, n=0)
    tee_print(overview,'MAX:   {0:.2f} seconds'.format(max(BT)),t=1, n=1)

    tee_print(overview,'MEAN:  ' + curr(mean(BS)),t=1, n=0)
    tee_print(overview,'STDEV: ' + curr(stdev(BS)),t=1, n=0)
    tee_print(overview,'ERROR: ' + perr(stdev(BS),mean(BS)),t=1, n=0)
    tee_print(overview,'MIN:   ' + curr(min(BS)),t=1, n=0)
    tee_print(overview,'MAX:   ' + curr(max(BS)),t=1, n=2)

    tee_print(overview,'RESULTS [Bhanu]: ',t=0, n=0)
    tee_print(overview,'TIME:  {0:.2f} seconds'.format(sum(BT)),t=1, n=0)
    tee_print(overview,'OBJ:  ' + curr(min(BS)),t=1, n=1)
    
    tee_print(overview,'RESULTS [Hybrid]: ',t=0, n=0)
    tee_print(overview,'TIME:  {0:.2f} seconds'.format(HT),t=1, n=0)
    tee_print(overview,'OBJ:  ' + curr(HS),t=1, n=1)
    
    if Pyomo_code:
        tee_print(overview,'RESULTS [Pyomo]: ',t=0, n=0)
        tee_print(overview,'TIME:  {0:.2f} seconds'.format(PT),t=1, n=0)
        tee_print(overview,'OBJ:  ' + curr(PS),t=1, n=2)
    
    tee_print(overview,'COMPARISON [Bhanu vs Hybrid]: ',t=0, n=0)
    comp_err = perr(min(BS)-HS,HS)
    tee_print(overview,'ERROR: ' + comp_err,t=1, n=0)
    speedup = '{0:.2f}'.format( (HT - sum(BT)) / sum(BT) )
    tee_print(overview,'SPEEDUP: ' + speedup,t=1, n=1)
    
    if Pyomo_code:
        tee_print(overview,'COMPARISON [Hybrid vs Pyomo]: ',t=0, n=0)
        comp_err = perr(HS-PS,PS)
        tee_print(overview,'ERROR: ' + comp_err,t=1, n=0)
        speedup = '{0:.2f}'.format( (PT - HT) / HT )
        tee_print(overview,'SPEEDUP: ' + speedup,t=1, n=2)
    
    _, BI = argmin(BS)
    I_path = path(cpath,'Bhanu_Results')
    #I_path = path(cpath,'Instance_{}'.format(BI + 1))
    Best_path = path(cpath,'Best_Instance')

    cp('{0}/Solution_{1}.txt', '{0}/Best_Solution.txt'.format(I_path, BI + 1))

    print line('#',10) + '\n'
    
if '__main__' == __name__:
    try:
        Type = sys.argv[1]
    except IndexError:
        Type = 'Misc'
        
    ID = 'M' + Type + '_' + id_generator(size=4)

    if Type in ('S','M','L', 'Test'):     
        cpath = 'Results/{}/Results_{}'.format(Type, ID)
    else:
        cpath = 'Results/{}/Results_{}'.format('Misc', ID)
    overview = path(cpath,'Overview.txt')    

    print ID
    main(cpath, Type)
    rm('WITP*txt*')        

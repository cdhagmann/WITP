from Function_Module import *
from Pyomo_code import *
import time

global_gap = .02

def tech_idx(tup):
    i, j = tup
    return (i - 1) * 6 + (j - 1)

@Timer
def Bhanu_code():
    command = "echo '' | mono WITP-TPH.exe"
    output = bash_command(command)

    Last_line = output[-1]
    assert 'minimum cost' in Last_line

    S = float(Last_line.split()[-1])
    return S

@Timer
def Hybrid_code(cpath, N, GG=None):
    BS, BT = [], []
    count, k, gap = {}, 1, 0
    best = set()
    Instance_path = path(cpath,'Bhanu_Results')
    mkpath(Instance_path)
    for k in xrange(1, N+1):
        # Instance_path = cpath #path(cpath,'Instance_{}'.format(k))
        # mkpath(Instance_path)

        print "Instance {} of {}:".format(k, N)

        S, T = Bhanu_code()

        qprint('RUN TIME [Bhanu]: ' +ptime(T), t=1)
        qprint('MIN COST [Bhanu]: ' + curr(S) , t=1, n=1)

        BT.append(T)
        BS.append(S)

        sol_file = '{}/Solution_{}.txt'.format(Instance_path, k)
        mv('*Sol*', sol_file)

        with open(sol_file,'r') as f:
            lines = f.readlines()

        TC = OrderedSet(tuple(map(int, l.split()[1:3])) for l in lines[-7:-2]
                        if 'None' not in l)

        tech_choices = map(tech_idx, TC)

        # print tech_choices

        best.add(tech_choices[0])

        for idx, index in enumerate(tech_choices):
            count[index] = count.get(index, 0) + (len(tech_choices) - idx)

        if k == 1:
            cp('TPH.cs',       '{}/'.format(Instance_path))
            cp('WITP-TPH.exe', '{}/'.format(Instance_path))
            cp('WITPdataSet*', '{}/'.format(Instance_path))

    L = sorted(count.items(), key=lambda (idx, c): c, reverse=True)
    '''
    for index in L:
        if len(best) >= 5:
            break
        else:
            best.add(index)
    '''
    #print sorted(list(best), key=lambda i: count[i], reverse=True)
    #indices = list(idx for idx, c in L[:5])
    indices = sorted(list(best), key=lambda i: count[i], reverse=True)

    #print
    #print L
    #print

    # print indices
    global global_gap
    #global_gap = GG

    #GG = stdev(BS) * 10 / mean(BS) if GG is None else GG
    #global_gap = round(min([(GG if GG < 1 else GG/100.), GG_min]), 4)
    qprint('Optimality Gap: {:.2%}'.format(global_gap))
    print

    qprint("Hybrid Method:")

    Instance_path = path(cpath,'Hybrid_Method')
    mkpath(Instance_path)

    HS, _ = Pyomo_code(indices=indices, cutoff=True, gap=global_gap)

    mv('results_best.yml', '{}/Best_Results.yml'.format(Instance_path))
    mv('results_*',                        '{}/'.format(Instance_path))
    mv('summary_*',                        '{}/'.format(Instance_path))

    return BS, BT, HS

@Timer
def Pyomo_wrapper(cpath):
    qprint("Pyomo Benchmark:")

    Instance_path = path(cpath,'Pyomo_Benchmark')
    mkpath(Instance_path)

    PS, PT = Pyomo_code(cutoff=True, gap=global_gap)

    mv('results_best.yml', '{}/Best_Results.yml'.format(Instance_path))
    mv('results_*',              '{}/'.format(Instance_path))
    mv('summary_*',              '{}/'.format(Instance_path))
    cp('PyomoCode/Pickled_Data', '{}/'.format(Instance_path))

    return PS

if '__main__' == __name__:
    ID = 'C' + id_generator(size=5) 
    foldername = 'Results_' + ID
    overview = path(foldername,'Overview.txt')
    cpath = path(foldername,'Case_1')
    print ID
    (BS, BT, HS), HT = Hybrid_code(cpath, N=6)
    print
    PS, PT = Pyomo_wrapper(cpath)

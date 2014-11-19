import itertools
from itertools import chain, product

def flatten(x):
    result = []
    for item in x:
        if hasattr(item, "__iter__") and not isinstance(item, basestring):
            result.extend(flatten(item))
        else:
            result.append(item)
    return list(result)

def list_count(x):
    if hasattr(x, "__iter__") and not isinstance(x, basestring):
        return 1
    else:
        return len(x)
            
def tup_list(*args):
    if len(args) == 1:
        return tuple(*args)
    else:
        return tuple(itertools.imap(flatten, product(*args)))

def fpr(num):
    i = 3
    while True:
        a,b,c = round(num,i),round(num,i+1),round(num,i+2)
        if a == b == c: break
        elif i > 16: return num
        else:i += 1
    if int(b) == b: return int(b)
    else: return b    
               
def str2data(s):
    try:
        s = float(s)
        return fpr(s)
    except ValueError:
        if s in ('True','true','yes'):return True
        elif s in ('False','false','no'): return False
        else: return s


def Pyomo_data(f,name,data,sets=None,dtype='param'):
    idx_strip = (lambda idx: str(int(''.join([i for i in idx if i.isdigit()]))))
    if dtype.lower() == 'param':
        if sets is None:
            f.write('{} {} := {}; \n\n'.format(dtype,name,data))
        else:
            f.write('{} {} :=\n'.format(dtype,name))
            if sets == list(flatten(sets)):
                tups = tup_list(sets)
                for tup in tups:
                    # tup1 = idx_strip(tup)
                    tup1 = tup
                    f.write('{} {}\n'.format(tup1, data[tup]))              
            else:
                tups = tup_list(*sets)       
                for tup in tups:
                    # tup1 = map(idx_strip,tup)
                    tup1 = map(str,tup)
                    f.write('{} {}\n'.format(' '.join(tup1), data[tup]))
            f.write(';\n\n')
    else:
        data = map(str,data)
        # data = map(idx_strip,data)
        f.write('set {} := {};\n\n'.format(name,' '.join(data)))


def Py_data(f,name,data,sets=None,dtype='param'):
    if dtype.lower() == 'param':
        if sets is None:
            f.write('{} = {}\n\n'.format(name,data))
        else:               
            f.write('{} = {{\n'.format(name))

            new_data = []
            for idx, (k, v) in enumerate(data.items()):               
                if type(k) is not str:
                    if k not in product(*sets):
                        continue
                    else:    
                        new_data.append(((tuple(str2data(ke) for ke in k)), str2data(v)))
                        iterator = range(len(new_data[-1][0]))[::-1]
                        for i in iterator:
                            new_data.sort(key=lambda (k,v): k[i])
                else:
                    if k not in sets:
                        continue
                    else:
                        new_data.append((repr(str2data(k)), str2data(v)))
                        new_data.sort(key=lambda (k,v): type(str2data(k))(k))
            N = len(new_data)
            for idx, (k, v) in enumerate(new_data): 
                if idx + 1 == N:
                    f.write('{}: {}\n'.format(k, v))
                else:
                    f.write('{}: {},\n'.format(k, v))  
            f.write('}\n\n')
    else:
        f_tup = (name, map(str2data,data))
        f.write('{} = {}\n\n'.format(*f_tup))


def Xpress_data(f,name,data,sets=None):
    idx_strip = (lambda idx: str(int(''.join([i for i in str(idx) if i.isdigit()]))))
    wl = u"\r\n"
    if sets is None:
        f.write('{}:[{WL}(1)\t[{}]{WL}]{WL}'.format(name,data,WL=wl))
    else:
        f.write('{}:[{WL}'.format(name,WL=wl))
        if sets == list(flatten(sets)):
            tups = tup_list(sets)
            for tup in tups:
                f.write('({})\t[{}]{WL}'.format(idx_strip(tup), data[tup],WL=wl))              
        else:
            tups = tup_list(*sets)       
            for tup in tups:
                tup1 = list(set(tup)-set(['w1'])) if 'w1' in tup else tup
                tup1 = tup1[0] if len(tup1) == 1 else tuple(tup1)
                f.write('({})\t[{}]{WL}'.format('\t'.join(map(idx_strip,tup)),data[tup1],WL=wl))
        f.write(']{WL}'.format(WL=wl))

def Dual_data(f,g,name,data,sets=None,dtype='param'):
    if dtype.lower() == 'param': 
        Pyomo_data(f,name,data,sets=sets,dtype=dtype)
        Xpress_data(g,name,data,sets=sets)
    else:
        Pyomo_data(f,name,data,sets=sets,dtype=dtype)
    
        
if __name__ == '__main__':
    stores = ["s" + str(s + 1) for s in xrange(3)]
    vendors = ["v" + str(v + 1) for v in xrange(3)]
    products = ["p" + str(p + 1) for p in xrange(3)]
    times = ["t" + str(t + 1) for t in xrange(3)]
    
    gamma = 1
    
    # Data for holding cost for product p at the warehouse
    Cz = {tup: 0.05 for tup in chain(products, itertools.product(stores, products))}

    # Data for backlog cost for product p at store s
    Cr = {p: .10 for p in products}
    
    f,g  = open('Pyomo_test.dat', 'w'), open('Xpress_test.txt', 'w')

    Dual_data(f,g,'STORES',stores,dtype='set')
    Dual_data(f,g,'PRODUCTS',products,dtype='set')
    Dual_data(f,g,'VENDORS',vendors,dtype='set')
    Dual_data(f,g,'TIMES',times,dtype='set')

    Dual_data(f,g,'FractionalFullTimeLoad',gamma,dtype='param')
    Dual_data(f,g,'ProductHoldingCostWarhouse', Cz, products,dtype='param')
    Dual_data(f,g,'ProductHoldingCostStore', Cz, [stores, products],dtype='param')  
    
    f.close()
    g.close()  
    

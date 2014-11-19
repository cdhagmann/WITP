from coopr.pyomo import *
from coopr.opt import SolverFactory
import os
import sys

class Printer():
    def __init__(self,data):
        sys.stdout.write("\r\x1b[K"+data.__str__())
        sys.stdout.flush()

def fpr(num):
    i = 1
    while True:
        a,b,c = round(num,i),round(num,i+1),round(num,i+2)
        if a == b == c: break
        elif i > 16: return round(num,6)
        else:i += 1
    if int(b) == b: return int(b)
    else: return b  

class Redirect(object):
    def __init__(self, stdout=None, stderr=None):
        self._stdout = stdout or sys.stdout
        self._stderr = stderr or sys.stderr

    def __enter__(self):
        self.old_stdout, self.old_stderr = sys.stdout, sys.stderr
        self.old_stdout.flush(); self.old_stderr.flush()
        sys.stdout, sys.stderr = self._stdout, self._stderr

    def __exit__(self, exc_type, exc_value, traceback):
        self._stdout.flush(); self._stderr.flush()
        sys.stdout = self.old_stdout
        sys.stderr = self.old_stderr

devnull = open(os.devnull, 'w')

suppress = Redirect(stdout=devnull, stderr=devnull)
        
def model_to_instance(model):
    instance = model.create()

    opt = SolverFactory("gurobi")
    results = opt.solve(model)
    
    instance.load(results)
    
    return instance

def instance_to_variables(instance):
    filename = 'results.txt'
    
    with open(filename,'w') as f:
        with Redirect(stdout=f):
            display(instance)
    with open(filename,'r') as f:
        lines = f.readlines()
    
    Variable = {}    
    for idx, line in enumerate(lines):
        if 'Variable ' in line:
            vname = line.split()[1]
            value = float(lines[idx + 1].split()[0].split('=')[-1]) 
            Variable[vname] = value    
    try:
        os.remove(filename)
    except OSError:
        pass
        
    return Variable

if __name__ == '__main__':
    def Principal_Objective(x, y):
        return x ** 2 - 8 * x + y ** 2 - 12 * y + 48

    def Lagrangian_Constraint(x, y):
        return (x + y - 8)
         
    def create_model():
        model = ConcreteModel()

        model.x = Var(within=NonNegativeReals, name='x')
        model.y = Var(within=NonNegativeReals, name='y')

        def obj_rule(model):
            objective = Principal_Objective(model.x, model.y)
            return objective

        model.obj = Objective(rule=obj_rule)

        def con_rule(model):
            return Lagrangian_Constraint(model.x, model.y) == 0
            
        model.con = Constraint(rule=con_rule)

        return model

    model = create_model()
    instance = model_to_instance(model)
    variables = instance_to_variables(instance)
    
    for k, v in variables.iteritems():
        exec('{KEY} = {VALUE}'.format(KEY = k, VALUE = repr(v)))


import pickle, os



class Struct(object):
    '''
    Struct is a generalized structure. Meant to be used as easy storage and 
    saving.
    
    foo = Struct()
    foo.x = 3
    foo.y = 4
    foo.write('test')
    bar = Struct('test')
    print bar.x, bar.y
    3, 4
    baz = bar.get_dict()
    print baz
    {'x': 3, 'y': 4}
    '''
    def __init__(self, _input=None):
        if _input is None:
            pass
        elif isinstance(_input, str):
            self.load_file(_input)
        elif isinstance(_input, dict):
            self.load_dict(**_input)
            
            
    def load_file(self, filename):
        self.archive = filename + '.pickle'
        if os.path.isfile(self.archive):
            with open(self.archive, 'rb') as f:
                OLD = pickle.load(f)
                for m in dir(OLD):
                    if '_' != m[0]:
                       setattr(self, m, getattr(OLD, m))
    
    
    def write_file(self, filename):
        self.archive = filename + '.pickle'
        with open(filename + '.pickle', 'wb') as f:
            pickle.dump(self, f, protocol=-1)


    def load_dict(self, **kwargs):
        for k, v in kwargs.iteritems():
            if '_' != k[0]:
                setattr(self, k, v)  
    
           
    def get_dict(self, keys=None):
        d = {}
        K = ('get_dict', 'load_file', 'write_file', 'load_dict', 'archive', 'contents')
        for m in dir(self):
            if '_' != m[0] and m not in K:
                d[m] = getattr(self, m)
        return d if keys is None else {k: v for k, v in d.iteritems if k in keys}
    
         
    def contents(self):
        d = self.get_dict()
        return d.keys()
        
        
class OrderedSet(list):
    '''
    Create an ordered set. 
    
    foo = OrderedSet()
    foo.append(3)
    foo.append(2)
    foo.append(3)
    print foo
    [3, 2]
    
    foo = OrderedSet(i%2 for i in xrange(1000))
    print foo
    [0, 1]
    '''
    def __init__(self, iterable=None):
        list.__init__(self)
        if iterable is not None:
            assert hasattr(iterable, '__iter__')
            
            for item in iterable:
                self.append(item)
            
    def append(self, item):
        if item not in self:
            list.append(self, item)


def boolean(choice):
    valid = {"yes":True,"y":True,"ye":True,"no":False,"n":False,
             "true":True,'t':True, "on":True,"1":True,
             "false":False,'f':False,'off':False,'0':False}
    if type(choice) is not str:
        return None
                 
    return valid.get(choice.lower(),None)

def qo_guess(val):
    if type(val) is bool: 
        if val:
            return('forceDefault',val)
        else:
            return('showPrompt',val)
    elif type(val) is list: return('options',val)
    elif type(val) is type or val is boolean: return ('response',val)
    else: return ('default',val)
    
def q_opts(*options):
    d = {'default':None,'options':None,'response':str,'forceDefault':False, 'showPrompt':True} 

    d_option = dict(map(qo_guess,options))
    d = {k:d_option.get(k,d[k]) for k in d}
    
    return d
  
def query(question, default=None, options=None, response=str, forceDefault=False, showPrompt=True):
    """Ask a question using raw_input that allow a default, fixed set of options, 
    set output type
    """
    if forceDefault and default is not None:
        return response(default)
    
    prompter = lambda options: " ({0}) ".format('/'.join(options))       

    if default is None and options is None:
        try:
            if not response('n'):
                prompt = prompter(['y','n'])
            else:
                prompt = ''
        except:
            prompt = ''
    elif default is not None and options is None:
        test = boolean(default)
        if test is None:
            S = lambda s: '[{0}]'.format(s) if s == default else str(s) 
            prompt = S(default)
        elif test:
            prompt = prompter(['Y','n'])
        else:
            prompt = prompter(['y','N'])

    elif default is not None and options is not None:
        test = boolean(default)
        if test is None:
            options.append(default)
            options = list(set(options))
            options.sort()

            S = lambda s: '[{0}]'.format(s) if s == default else str(s)   
            prompt = prompter(map(S,options))
        elif test:
            prompt = prompter(['Y','n'])
        else:
            prompt = prompter(['y','N'])
    else:
        test = boolean(options[0])
        if test is None:
            options.sort()  
            prompt = prompter(map(str,options))
        else:
            prompt = prompter(['y','n'])

    while True:
        choice = raw_input(question + prompt + ' ' if showPrompt else question+ ' ')
        choice = choice.strip()
        if default is not None and choice == '':
            return response(default)
        else:
            try:
                if response(choice) is None:
                    raise Exception
                if options is not None and (response(choice) not in options or response(choice) is bool):
                    raise Exception
                    
                return response(choice)
            except:
                print 'Please respond with a valid option: {0}\n'.format(prompt)
                
                
if __name__ == '__main__':
    name = query('What is your name?')
    A = query('What is the value of A?', default=3, options=[.5,1,2,3,4,5], response=float)
    B = query('Is this statement false?', default='yes', response=boolean)
    C = query('What is your age?', default=25, response=int)

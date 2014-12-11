from bs4 import BeautifulSoup
from messenger import text_notification   
import time, datetime, parse
import requests, csv, os
       

def raw_stop_info(stop):
    stop = _stop_names.get(stop.lower(), stop)
    
    assert stop.upper() in Stops
    
    url = r'http://wap.gocitybus.com/ViewStop.aspx?sp='
    url += Stops[stop.upper()]
    Soup = BeautifulSoup(requests.get(url).text)
    temp1 = Soup.findAll('font')[2].getText().split('\r\n')
    temp2 = (f.split() for f in temp1 if 'No routes' not in f)
    temp3 = ((s[0], s[-2]) for s in temp2)
    
    ans = []
    for i, (line, delay) in enumerate(temp3):
        try:
            ans.append( (str(line), int(delay)) )
        except ValueError:
            if delay == 'now':
                ans.append( (line, 0) )
            else:
                print stop.upper(), temp1[i]
    
    return ans


def get_stop_info(stop, line=None, output=False):
    if line is None:
        ans = raw_stop_info(stop)
    else:
        ans = [(r, t) for r, t in raw_stop_info(stop) if r in line]

    if output:        
        if ans:
            for r, t in ans:
                eta = _eta(t)
                print '{} - {} [{} minutes away]'.format(r, _eta(t), t)
        else:
            print 'There are no more stops today.' 
    
    return ans


def pacer(inteval, duration):
    N = int( duration / inteval )
    for i in xrange(N):
        time.sleep(1)
        while True:
            t = datetime.datetime.now()
            test = _time_to_integer(t) % inteval
            if test == 0:
                yield t
                break
            else:
                time.sleep(inteval - test)

                    
def stop_watch(stop, line, travel_time=5, text=True):
    message = 'The {} will be at {} in {} minutes.'.format(line, stop, '{}')
     
    delay = get_stop_info(stop, line)[0][1]
    print message.format(delay)
    
    immiment_flag = False
    for t in pacer(90, 45 * 60):
        delay = get_stop_info(stop, line)[0][1]
        
        N = int(round(delay - travel_time + .5))
        
        if N <= 5:
            immiment_flag = True if N <=2 else False
            if text:
                M = ('IMMIMENT: ' if immiment_flag else '') + message.format(delay)
                text_notification(M)
        elif immiment_flag:
            break
        
        print message.format(delay)



def schedule_writer(duration):
    for t in pacer(30, duration):
        with open('busdata.csv', 'ab') as f:
            csv_writer = csv.writer(f)
            for stop in Stops:
                for line, delay in get_stop_info(stop, line=('5B','5A','17')):
                    csv_writer.writerow( _create_row(stop, line, delay, t) )
                    
#***************************************************************************                    
#***************************************************************************
#***************************  INTERNAL DATA ********************************
#***************************************************************************
#***************************************************************************                    



Stops = {'BUS563'  : '16eb75a2-9f51-4ddf-a1c9-7968d642717d',
         'BUS944S' : '1c47b766-f64e-4815-9f9d-08321fd0c36d',
         'BUS944N' : '5f9b6b54-4619-4fb2-8b37-eb9a254ef449',
         'BUS111'  : '270f40fb-e2f9-4df0-bcad-0478b4c91a6b',
         'BUS249'  : '78b67182-6ea1-4f4b-ba92-8a1e65a3f283',
         'BUS347'  : 'e906beef-c932-46d8-bc9c-a64ca2198519',
         'BUS426'  : '4f374386-a59b-4f57-aa37-e58c034786a8',
         'BUS190'  : 'daf95931-f7af-4599-8362-50cc1f3c3651',
         'BUS115S' : '341f45e0-3331-42b4-9967-391b299986bb',
         'BUS114W' : 'eb913216-dfad-4b81-9d3a-2a6286257202',
         'BUS945SW': 'b32a9fcc-6a26-4b12-ae1a-fb9e8c0c51f6',
         'BUS631W' : 'f16f57f5-4ddb-4c08-8821-50b362d98a7c',
         'BUS113NW': 'a4615096-a32c-4e03-856e-a2143b3aa46f',
         'BUS117SW': 'ddde0da9-5a9e-42b8-89b3-8e6a28467d1d',
         'BUS118NW': '714dd833-be0a-4fad-ad6c-6f3770b5631f',
         'BUS486N' : '1c17db6d-e2ae-41d6-9748-7faae00f3980',
         'BUS557'  : '87dc32ff-e17e-4cf5-9be4-4715431c6f43'}

_stop_names = {'from home'   : 'BUS944S',
               'home'        : 'BUS944S',
               'to home'     : 'BUS944N',
               'push'        : 'BUS563' ,
               'lambert'     : 'BUS111' ,
               'armstrong'   : 'BUS190' ,
               'discovery'   : 'BUS249' ,
               'to parking'  : 'BUS347' ,
               'from parking': 'BUS426' ,
               'lawson'      : 'BUS486N', 
               'physics'     : 'BUS557' }


                    
#***************************************************************************                    
#***************************************************************************
#*************************  PRIVATE FUNCTIONS ******************************
#***************************************************************************
#***************************************************************************



_time_parser = parse.compile('{H}:{M}:{S} {SUF}')


def _time_formatter(*args):
    H, M, S, SUF = 0, 0, 0, None
    if len(args) == 4:
        H, M, S, SUF = args
    elif len(args) == 3:
        H, M, S = args
    elif len(args) == 2:
        H, M = args
    elif len(args) == 1:
        if isinstance(args[0], (datetime.datetime, datetime.time)):
            H, M, S = args[0].hour, args[0].minute, args[0].second
        elif isinstance(args[0], int):
            H, i = divmod(args[0], 3600)
            M, S = divmod(i, 60)      
        else:
            print args[0]
    else:
        print args
        
    if SUF is None:
        SUF = 'PM' if H >= 12 else 'AM'
        H = H if H <= 12 else H - 12
        
    return '{}:{:02}:{:02} {}'.format(H, M, S, SUF)


def _time_to_integer(t):
    if isinstance(t, (datetime.datetime, datetime.time)):
        return t.hour * 60 * 60 + t.minute * 60 + t.second
    else:
        foo = _time_parser.parse(t)
        H = int(foo['H']) + (0 if foo['SUF'] == 'AM' else 12)
        M, S = int(foo['M']), int(foo['S'])
        return H * 60 * 60 + M * 60 + S

        
def _integer_to_time(i):
    return _time_formatter(i) 
    

def _current_time_integer():
    t = datetime.datetime.now()
    return _time_to_integer(t)


def _today_base():
    return int(time.time()) - _current_time_integer()
    
    
def _current_time():
    t = datetime.datetime.now()
    return _time_formatter(t)    


def _create_timer(duration):
    t1 = time.time()
    return lambda: time.time() - t1 <= duration

  
def _eta(delay, t=None, output='string'):
    t = _current_time_integer() if t is None else _time_to_integer(t)
    eta = t + delay * 60
    if output.lower() == 'string':
        return _integer_to_time(eta)
    elif output.lower() in ('int', 'integer'):
        return eta
    else:
        err = "output accepts 'string' or 'integer', not {}".format(output)
        raise ValueError(err)    
            
def _create_row(stop, line, delay, t):
    info = [_today_base()]
    info += time.strftime('%w,%m,%d,%Y', time.localtime()).split(',')
    info = map(int, info)
    
    info += [stop,
            str(line),
            _time_formatter(t),
            _eta(delay, t),
            _time_to_integer(t),
            _eta(delay, t, output='integer')]
            

    return info



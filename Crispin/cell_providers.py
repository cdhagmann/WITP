from bs4 import BeautifulSoup
import requests

def _extract_provider(response):
    try:
        soup = BeautifulSoup(response, 'html.parser')
        return soup.findAll('b')[-1].getText().split()[-1]
    except IndexError as exc:
        pass
            

def find_provider(number):
    url = 'http://www.txt2day.com/lookup.php'

    values = {'action' : 'lookup',
               'pre' : number[0:3],
               'ex' : number[3:6],
               'myButton' : 'Find Provider'}

    response = requests.post(url, data=values).text
    return _extract_provider(response)

    
def get_provider_suffix(provider, message_type='sms'):
    return providers[message_type][provider.lower()]



def number_to_email(number, message_type='sms'):
    provider = find_provider(number)
    suffix = get_provider_suffix(provider, message_type)
    return '@'.join([number, suffix])
    
    
providers = {'sms': {}, 'mms': {}}

providers['sms']['alltel'] = 'text.wireless.alltel.com'
providers['mms']['alltel'] = 'mms.alltel.net'

providers['sms']['att'] = 'txt.att.net'
providers['mms']['att'] = 'mms.att.net'

providers['sms']['boost'] = 'myboostmobile.com'
providers['mms']['boost'] = 'myboostmobile.com'

providers['sms']['cricket'] = 'sms.mycricket.com'
providers['mms']['cricket'] = 'mms.mycricket.com'

providers['sms']['sprint'] = 'messaging.sprintpcs.com'
providers['mms']['sprint'] = 'pm.sprint.com'

providers['sms']['verizon'] = 'vtext.com'
providers['mms']['verizon'] = 'vzwpix.com'

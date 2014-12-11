import smtplib
from getpass import getpass
from contextlib import contextmanager
from email.MIMEText import MIMEText
import requests 

@contextmanager
def gmx_server(username, password=None):
    password = getpass() if password is None else password
    account, ports = 'gmx', (25, 465)
    for p in ports:
        try:
            server = smtplib.SMTP('smtp.{}.com:{}'.format(account, p))
            server.login(username, password)
            break
        except smtplib.SMTPAuthenticationError as exc:
            server.quit()
            error = exc
            if exc.smtp_code == 454:
                continue
    else:
        raise error
                
    yield server
    server.quit()

    
@contextmanager
def gmail_server(username):
    server = smtplib.SMTP('smtp.gmail.com:587')
    server.ehlo();server.starttls();server.ehlo()
    password = getpass()
    server.login(username,password)
    yield server
    server.quit()
    

def create_email(to, From=None, subject=None, message=None):
    if isinstance(to, str):
        if to == 'phone':
            toaddrs = ['2108602966@txt.att.net']
        else:            
            toaddrs = [to]
    
    for t in toaddrs:
        assert '@' in t
        
    if message is None:
        message = str(raw_input("Enter Message:\n"))
        
    msg = MIMEText(message)
    msg['From'] = 'Python_Script' if From is None else From
    if subject is not None:
        msg['Subject'] = subject
    
    return toaddrs, msg
    
    
def send_gmx_message(to, message=None, From=None, 
                     subject=None, debug=True):
    
    toaddrs, msg = create_email(to, From, subject, message)
    
    username = 'cdhagmann@gmx.com'
    fromaddr = 'cdhagmann@gmx.com'
    with gmx_server(username) as server:
        server.set_debuglevel(debug) 
        server.sendmail(fromaddr, toaddrs, msg.as_string())

        
def send_gmail_message(to, message=None, From=None, 
                     subject=None, debug=False):
    
    toaddrs, msg = create_email(to, From, subject, message)
    
    username = 'cdhagmann'
    fromaddr = 'cdhagmann@gmail.com'
    with gmail_server(username) as server:
        server.set_debuglevel(debug) 
        server.sendmail(fromaddr, toaddrs, msg.as_string())

       
def text_me(number='2108602966', message=None):
    if message is None:
        message = str(raw_input("Enter Message:\n"))
        
    r = requests.post('http://textbelt.com/text', locals())
    
    if r.json()['success']:
        return True
    else:
        print r.json()['message']
        return False
        
def text_notification(message, subject=None):
    
    toaddrs, msg = create_email('2108602966@mms.att.net', 
                                subject=subject,
                                message=message)
    
    username = 'cdhagmann@gmx.com'
    fromaddr = 'cdhagmann@gmx.com'
    with gmx_server(username, 'B@shsh3ll') as server:
        server.sendmail(fromaddr, toaddrs, msg.as_string())    

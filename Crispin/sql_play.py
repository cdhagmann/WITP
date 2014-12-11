import pandas as pd
import sqlite3
import pandas.io.sql as sql
from contextlib import contextmanager



@contextmanager
def database(connection=':memory:'):
    db = sqlite3.connect(connection)
    yield db
    db.close()

    
@contextmanager
def sql_cursor(db):
    cursor = db.cursor()
    yield cursor
    db.commit()    


class SQL(object):   
    def __init__(self, data, name):
        self.connection = sqlite3.connect(':memory:')
        self.name = name
        if isinstance(data, dict):
            self.db = pd.DataFrame(data)
        elif isinstance(data, pd.core.frame.DataFrame):
            self.db = data
            
        sql.write_frame(self.db, name=self.name, con=self.connection)
    
    def read(self, command):
        return sql.read_frame(command, self.connection)
    
    def select(self, *columns):
        if columns:
            command = 'SELECT {} FROM {}'.format(','.join(columns), self.name)
        else:
            command = 'SELECT * FROM {}'.format(self.name)
        return self.read(command)
        
    def where(self, condition):
        command = 'SELECT * FROM {} WHERE {}'.format(self.name, condition)
        return self.read(command)
        
        
    def pprint(self):
        print self.select()
        
        
'''
import pandas as pd
import pandas.io.sql as pd_sql
import sqlite3 as sql


###################################################

import numpy as np
import pandas as pd
from pandas import DataFrame, Series
import sqlite3 as db

# download data from yahoo
all_data = {}

for ticker in ['AAPL', 'GE']:
    all_data[ticker] = pd.io.data.get_data_yahoo(ticker, '1/1/2009','12/31/2012')

# create a data frame
price = DataFrame({tic: data['Adj Close'] for tic, data in all_data.iteritems()})

# get output ready for database export
output = price.itertuples()
data = tuple(output)

# connect to a test DB with one three-column table titled "Demo"
con = db.connect('c:/Python27/test.db')
wildcards = ','.join(['?'] * 3)
insert_sql = 'INSERT INTO Demo VALUES (%s)' % wildcards
con.executemany(insert_sql, data)

#################################################################

price2 = price.reset_index()

In [11]: price2
Out[11]: 
<class 'pandas.core.frame.DataFrame'>
Int64Index: 1006 entries, 0 to 1005
Data columns:
Date    1006  non-null values
AAPL    1006  non-null values
GE      1006  non-null values
dtypes: datetime64[ns](1), float64(2)

import sqlite3
from pandas.io import sql
# Create your connection.
cnx = sqlite3.connect(':memory:')

sql.write_frame(price2, name='price2', con=cnx)

p2 = sql.read_frame('select * from price2', cnx)

'''


            

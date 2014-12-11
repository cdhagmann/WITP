# -*- coding: utf-8 -*-
"""
Created on Wed Dec 10 16:10:42 2014

@author: cdhagmann
"""

import csv, glob, os.path, re, numpy
from collections import defaultdict

def objective_strip(line):
    raw_string = line.split()[-1]
    return float(re.sub(r'[^\d.]', '', raw_string))
    
def time_strip(line):
    raw_string = line.split()[-2]
    return float(raw_string)
    
def scale_list(lst):
    return list(numpy.array(lst)/float(lst[1]))  

raw_rows = []
scaled_rows = []

raw_time_rows = []
raw_solution_rows = []

scaled_time_rows = []
scaled_solution_rows = []

methods = ['HEURISTIC', 'WARM START', 'BIG M']    
for archive in glob.glob('*/*/streaming_output'):
    INSTANCE_SIZE = archive.split(os.path.sep)[0]
    ID = archive.split(os.path.sep)[1].split('_')[-1]
    
    with open(archive, 'rb') as f:
        lines = f.readlines()
        lines = [l.strip() for l in lines]
    
    results = []    
    for idx, line in enumerate(lines):
        if 'RESULTS' in line:
            results.append(idx)

    while len(results) > 3:
        results.pop(0)

    times, solutions = [], []
        
    for i, idx in enumerate(results):
        METHOD = methods[i]           
        SOLUTION = objective_strip(lines[idx+2])
        TIME = time_strip(lines[idx+1])
        raw_rows.append( (ID, INSTANCE_SIZE, METHOD, TIME, SOLUTION) )
        
        times.append(TIME)
        solutions.append(SOLUTION)

    raw_time_rows.append( [ID, INSTANCE_SIZE] + times )
    raw_solution_rows.append( [ID, INSTANCE_SIZE] + solutions )
    
    times = scale_list(times)
    solutions = scale_list(solutions)

    scaled_time_rows.append( [ID, INSTANCE_SIZE] + times )
    scaled_solution_rows.append( [ID, INSTANCE_SIZE] + solutions )
    
    for i, (TIME, SOLUTION) in enumerate(zip(times, solutions)):           
        METHOD = methods[i]   
        scaled_rows.append( (ID, INSTANCE_SIZE, METHOD, TIME, SOLUTION) )  
        
with open('RAW_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'METHOD', 'TIME', 'SOLUTION'))
    csv_writer.writerows(raw_rows)
    
with open('SCALED_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'METHOD', 'TIME', 'SOLUTION'))
    csv_writer.writerows(scaled_rows)

with open('RAW_TIME_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'HEURISTIC', 'WARM START', 'BIG M'))
    csv_writer.writerows(raw_time_rows)

with open('RAW_SOLUTION_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'HEURISTIC', 'WARM START', 'BIG M'))
    csv_writer.writerows(raw_solution_rows)
    
with open('SCALED_TIME_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'HEURISTIC', 'WARM START', 'BIG M'))
    csv_writer.writerows(scaled_time_rows)

with open('SCALED_SOLUTION_DATA.csv', 'wb') as f:
    csv_writer = csv.writer(f)
    csv_writer.writerow(('ID', 'INSTANCE_SIZE', 'HEURISTIC', 'WARM START', 'BIG M'))
    csv_writer.writerows(scaled_solution_rows)    
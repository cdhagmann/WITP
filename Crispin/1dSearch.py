from math import sqrt, ceil, log, copysign
import numpy as np

eps = 1e-8
rho = (3 - sqrt(5)) / 2


def Bracket(f, a=0, delta=1, ptype="min"):
	b = a + delta
	c = b + delta
	i = 0
	if ptype.lower() == "min":	
		while f(b) > f(c):
			i += 1
			a, b, c = b, c, b + (2 ** i) * delta
	elif ptype.lower() == "zero":
		while f(b) * f(c) > 0:
			i += 1
			a, b, c = b, c, c - (f(c) * (c - b)) / (f(c) - f(b)) * (1 + delta)
	elif ptype.lower() == "max":
		negf = lambda x: -f(x)
		[a,c] = Bracket(negf, a, delta, ptype = "min")
	else:
		print "Error"
	
	if c < a:
	    a, c = c, a
	if c - a > 4: 
	    a, c = Bracket(f, a, min(delta / 2, .0625), type)
	return a, c	
		
		
def Golden(f,a = 0,delta = .0001):
	[LB,UB] = Bracket(f, a)
	N = int(ceil(log(delta/(UB - LB))/log(1-rho)))
	a = LB + rho*(UB-LB)
	b = LB + (1-rho)*(UB-LB)
	for i in range(N):
		if f(a) < f(b):
			a,b,UB = LB + rho*(b-LB),a,b
		else:
			a,b,LB = b,a + (1-rho)*(UB-a),a
	return round((UB + LB)/2,int(('%e' % delta)[-2:]))
	
	
def Newton(f,x=0,n=20):
	f_prime = (lambda x: (f(x + eps) - f(x - eps)) / (2.0 * eps))
	for i in range(n):
		if f_prime(x) == 0:
			return x
		x = x - f(x)/f_prime(x)
	return x

	
def Secant(f,x=[0,10],n=20):
	x0,x1 = x[0],x[1]
	for i in range(n):
		if f(x1)-f(x0) == 0:
			return x1
		x0, x1 = x1, x1 - (f(x1)*(x1-x0))/(f(x1)-f(x0))
	return x1


#-----------------------------------------------------------------------------
#                              IMPORT MODULES
#-----------------------------------------------------------------------------

from coopr.pyomo import *
import string
import random
import pickle

#-----------------------------------------------------------------------------
#                            MOTIVATION FROM WIFE
#-----------------------------------------------------------------------------

''' my husband is amazingly sexy! RAWR!! love, sara '''

#-----------------------------------------------------------------------------
#                               INITIATE MODEL
#-----------------------------------------------------------------------------

def cat(output, *filenames):
    with open(output, 'w') as outfile:
        for fname in filenames:
            with open(fname) as infile:
                for line in infile:
                    outfile.write(line)
                else:
                    outfile.write('\n')
                    

def id_generator(size=6, chars=string.ascii_uppercase):
	return ''.join(random.choice(chars) for x in range(size))

def idx2pair(idx):
    return 'i{}'.format(int(idx)/6 + 1), 'j{}'.format(int(idx)%6 + 1)
                    
def Pyomo_tech(index):
    i, j = idx2pair(index)

    with open('PyomoCode/Pickled_Data', 'rb') as f:
        rd = pickle.load(f)
    
    M = ConcreteModel()
    
    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL PARAMETERS
    #-----------------------------------------------------------------------------

    M.STORES = Set(initialize=rd.STORES)
    M.PRODUCTS = Set(initialize=rd.PRODUCTS)
    M.FASHION = Set(initialize=rd.FASHION)
    M.VENDORS_Q = Set(initialize=rd.VENDORS_Q)
    M.VENDORS_P = Set(initialize=rd.VENDORS_P)
    M.VENDORS = Set(initialize=rd.VENDORS)
    M.TIMES = Set(initialize=rd.TIMES)

    M.OMEGA_Q = Set(initialize=rd.Omega_q, dimen=2)
    M.OMEGA_P = Set(initialize=rd.Omega_p, dimen=2)

    M.T_minus_One = Param(M.TIMES, initialize=rd.T_minus_One)

    def K_init(model, s):
        return 10000000
    M.K_s = Param(M.STORES, initialize=K_init)

    def L_init(model):
        return 0
    M.L = Param(initialize=L_init)

    M.Script_Q = Param(initialize=15000)

    M.phi_put = Param(initialize=1.0)
    M.phi_pick = Param(initialize=1.0)
    M.gamma = Param(initialize=0.5)

    M.pt = Param(initialize=1)
    M.tb = Param(initialize=rd.tb)
    M.te = Param(initialize=rd.te)
    M.ty = Param(initialize=rd.ty)

    M.Demand = Param(M.STORES, M.PRODUCTS, M.TIMES, initialize=rd.Demand)

    M.V_p = Param(M.PRODUCTS, initialize=rd.V_p)
    M.V_q = Param(M.FASHION, initialize=rd.V_q)
    M.W_p = Param(M.PRODUCTS, initialize=rd.W_p)
    M.W_q = Param(M.FASHION, initialize=rd.W_q)

    X_ivq = {tup:rd.X_ivq[tup] for tup in M.OMEGA_Q}
    M.X_ivq = Param(M.OMEGA_Q, initialize=X_ivq)
    M.X_osq = Param(M.STORES, M.FASHION, initialize=rd.X_osq)
    M.C_alpha = Param(initialize=rd.C_alpha)
    M.C_beta = Param(initialize=rd.C_beta)

    def C_hp_init(model, p):
        return .01
    M.C_hp = Param(M.PRODUCTS, initialize=C_hp_init)
    M.C_hq = Param(M.FASHION, initialize=C_hp_init)


    def C_hsp_init(model, s, p):
        return .05
    M.C_hsp = Param(M.STORES, M.PRODUCTS, initialize=C_hsp_init)
    M.C_hsq = Param(M.STORES, M.FASHION, initialize=C_hsp_init)


    M.C_fv = Param(M.VENDORS, initialize=rd.C_fv)
    M.C_fs = Param(M.STORES, initialize=rd.C_fs)
    M.C_vv = Param(M.VENDORS, initialize=rd.C_vv)
    M.C_vs = Param(M.STORES, initialize=rd.C_vs)

    M.lambda_put = Param(initialize=rd.lambda_put[i])
    M.lambda_pick = Param(initialize=rd.lambda_pick[j])
    M.MHE = Param(initialize=rd.MHE[i])
    M.C_put = Param(initialize=rd.C_put[i])
    M.C_pick = Param(initialize=rd.C_pick[j])


    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL VARIABLES
    #-----------------------------------------------------------------------------

    M.alpha_put =  Var(bounds=(0.0, None),
                         within=NonNegativeIntegers)
                         
    M.alpha_pick = Var(bounds=(0.0, None),
                         within=NonNegativeIntegers)

    M.beta_put =   Var(M.TIMES, 
                         bounds=(0.0, None),
                         within=NonNegativeIntegers)
                        
    M.beta_pick =  Var(M.TIMES, 
                         bounds=(0.0, None),
                         within=NonNegativeIntegers)

    M.tau_sq =     Var(M.STORES, M.FASHION,
                         within=NonNegativeIntegers)
                     
    M.rho_vqt =    Var(M.OMEGA_Q, M.TIMES,
                         within=Binary)
                      
    M.rho_sqt =    Var(M.STORES, M.FASHION, M.TIMES,
                         within=Binary)
                      
    M.x_vpt =      Var(M.OMEGA_P, M.TIMES,
                         within=NonNegativeIntegers)
                      
    M.x_spt =      Var(M.STORES, M.PRODUCTS, M.TIMES,
                         within=NonNegativeIntegers)

    M.y_pt =       Var(M.PRODUCTS, M.TIMES,
                         within=NonNegativeIntegers)
                     
    M.y_spt =      Var(M.STORES, M.PRODUCTS, M.TIMES,
                         within=NonNegativeIntegers)

    M.y_sqt =      Var(M.STORES, M.FASHION, M.TIMES,
                         within=NonNegativeIntegers)

    M.z_spt =      Var(M.STORES, M.PRODUCTS, M.TIMES,
                         within=NonNegativeIntegers)

    M.z_sqt =      Var(M.STORES, M.FASHION, M.TIMES,
                         within=NonNegativeIntegers)

    M.n_vt =       Var(M.VENDORS, M.TIMES,
                         within=NonNegativeIntegers)
                     
    M.n_st =       Var(M.STORES, M.TIMES,
                         within=NonNegativeIntegers)
                                       
    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL CONSTRAINTS
    #-----------------------------------------------------------------------------


    def Total_Cost_Objective_rule(model):
        Workers_Cost =  M.C_alpha * M.alpha_pick
        Workers_Cost += M.C_alpha * M.alpha_put 
        Workers_Cost += M.C_beta * sum(M.beta_pick[t] for t in M.TIMES)
        Workers_Cost += M.C_beta * sum(M.beta_put[t] for t in M.TIMES)
        
        MHE_Cost = sum( M.MHE * (M.alpha_put + M.beta_put[t]) for t in M.TIMES ) 
        
        Tech_Cost = M.C_put + M.C_pick
        
        
        Holding_Cost =  sum(M.C_hp[p] * M.y_pt[p,t] 
                            for p in M.PRODUCTS for t in M.TIMES)
        Holding_Cost += sum(M.C_hq[q] * M.X_osq[s,q] * M.tau_sq[s,q] 
                            for s in M.STORES for q in M.FASHION)
        Holding_Cost += sum(M.C_hsp[s,p] * M.y_spt[s,p,t] 
                            for s in M.STORES for p in M.PRODUCTS for t in M.TIMES)
        Holding_Cost += sum(M.C_hsq[s,q] * M.y_sqt[s,q,t] 
                            for s in M.STORES for q in M.FASHION for t in M.TIMES)


        Fixed_Shipping_Cost =  sum(M.C_fv[v] * M.n_vt[v,t]
                                  for v in M.VENDORS for t in M.TIMES)
        Fixed_Shipping_Cost += sum(M.C_fs[s] * M.n_st[s,t]
                                  for s in M.STORES for t in M.TIMES)
        
                                  
        Var_Shipping_Cost =  sum(M.C_vv[v] * M.W_p[p] * M.x_vpt[v,p,t] 
                                  for v, p in M.OMEGA_P for t in M.TIMES)
        Var_Shipping_Cost += sum(M.C_vv[v] * M.W_q[q] * M.X_ivq[v,q] * M.rho_vqt[v,q,t]
                                  for v, q in M.OMEGA_Q for t in M.TIMES)    
        Var_Shipping_Cost += sum(M.C_vs[s] * M.W_p[p] * M.x_spt[s,p,t] 
                                  for s in M.STORES for p in M.PRODUCTS for t in M.TIMES)
        Var_Shipping_Cost += sum(M.C_vs[s] * M.W_q[q] * M.X_osq[s,q] * M.rho_sqt[s,q,t]
                                  for s in M.STORES for q in M.FASHION for t in M.TIMES)     
        
        
        FS_Expr = (Workers_Cost + MHE_Cost + Tech_Cost + Holding_Cost
                                + Fixed_Shipping_Cost + Var_Shipping_Cost) 
                                
        return FS_Expr

    M.Total_Cost_Objective = Objective(sense=minimize)



    # Constraint Two -  Constraint Three
    '''Not needed in Explicit Enumeration Models'''



    # Constraint Four
    def ConstraintFour_rule(model, t):
        Four_expr1 = sum(M.x_vpt[v,p,t] for v, p in M.OMEGA_P)
        Four_expr1 += sum(M.X_ivq[v,q] * M.rho_vqt[v,q,t] for v, q in M.OMEGA_Q)
        Four_expr2 = M.lambda_put * (M.alpha_put + M.phi_put * M.beta_put[t])
        return (Four_expr1 - Four_expr2 <= 0)

    M.ConstraintFour = Constraint(M.TIMES)



    # Constraint Five
    def ConstraintFive_rule(model, t):
        Five_expr1 = sum(M.x_spt[s,p,t] for s in M.STORES for p in M.PRODUCTS)
        Five_expr1 += sum(M.X_osq[s,q] * M.rho_sqt[s,q,t] for s in M.STORES for q in M.FASHION)
        Five_expr2 = M.lambda_pick * (M.alpha_pick + M.phi_pick * M.beta_pick[t])
        return (Five_expr1 - Five_expr2 <= 0)

    M.ConstraintFive = Constraint(M.TIMES)



    # Constraint Six
    def ConstraintSixPutaway_rule(model, t):
        Six_expr1 = M.beta_put[t]
        Six_expr2 = M.gamma * M.alpha_put
        return (Six_expr1 - Six_expr2 <= 0)

    M.ConstraintSixPutaway = Constraint(M.TIMES)



    def ConstraintSixPicking_rule(model, t):
        Six_expr1 = M.beta_pick[t]
        Six_expr2 = M.gamma * M.alpha_pick
        return (Six_expr1 - Six_expr2 <= 0)

    M.ConstraintSixPicking = Constraint(M.TIMES)

     
           
    # Constraint Seven
    def ConstraintSeven_rule(model, v, q, s):
        Seven_expr1 = M.tau_sq[s,q]
        Seven_expr2 =  sum(t * M.rho_sqt[s, q, t] for t in M.TIMES)
        Seven_expr2 -= sum(t * M.rho_vqt[v, q, t] for t in M.TIMES)
        return (Seven_expr1 - Seven_expr2 == 0)

    M.ConstraintSeven = Constraint(M.OMEGA_Q, M.STORES)



    # Constraint Eight
    def ConstraintEight_rule(model, s, q):
        Eight_expr1 = M.tau_sq[s,q]
        return (M.pt - Eight_expr1 <= 0)

    M.ConstraintEight = Constraint(M.STORES, M.FASHION)



    # Constraint Nine
    def ConstraintNine_rule(model, s, q):
        Nine_expr1 = sum(t * M.rho_sqt[s, q, t] for t in M.TIMES)
        Nine_expr2 = M.ty - M.L
        return (Nine_expr1 - Nine_expr2 <= 0)

    M.ConstraintNine = Constraint(M.STORES, M.FASHION)



    # Constraint Ten
    def ConstraintTenVendor_rule(model, v, q):
        Ten_expr1 = sum(M.rho_vqt[v, q, t] for t in M.TIMES if M.tb <= t <= M.te)
        return (Ten_expr1 - 1 == 0)

    M.ConstraintTenVendor = Constraint(M.OMEGA_Q)



    def ConstraintTenStore_rule(model, s, q):
        Ten_expr1 = sum(M.rho_sqt[s, q, t] for t in M.TIMES if M.tb <= t <= M.ty)
        return (Ten_expr1 - 1 == 0)

    M.ConstraintTenStore = Constraint(M.STORES, M.FASHION)

    # Constraint Ten Prime
    def ConstraintTenPrimeVendor_rule(model, v, q, t):
        if not M.tb <= t <= M.ty:
            return M.rho_vqt[v, q, t] == 0
        else:
            return Constraint.Skip

    M.ConstraintTenPrimeVendor = Constraint(M.OMEGA_Q, M.TIMES)

    def ConstraintTenPrimeStore_rule(model, s, q, t):     
        if not M.tb <= t <= M.ty:
            return M.rho_sqt[s, q, t] == 0
        else:
            return Constraint.Skip

    M.ConstraintTenPrimeStore = Constraint(M.STORES, M.FASHION, M.TIMES)
        
    # Constraint Eleven and Twelve
    def ConstraintEleven_rule(model, s, p, t):
        assert M.L == 0
        
        Eleven_expr1 = M.z_spt[s,p,t]
        Eleven_expr2 = M.x_spt[s,p,t]
        return (Eleven_expr1 - Eleven_expr2 == 0)

    M.ConstraintEleven = Constraint(M.STORES, M.PRODUCTS, M.TIMES)


    # Constraint Thirteen
    def ConstraintThirteen_rule(model, s, q, t):
        assert M.L== 0
        if M.tb <= t <= M.ty:
            Thirteen_expr1 = M.z_sqt[s,q,t]
            Thirteen_expr2 = M.X_osq[s,q] * M.rho_sqt[s,q,t]
            return (Thirteen_expr1 - Thirteen_expr2 == 0)
        else:
            return Constraint.Skip

    M.ConstraintThirteen = Constraint(M.STORES, M.FASHION, M.TIMES)
        
    # Constraint Fourteen and Fifteen
    def ConstraintFourteen_rule(model, p, t):
        Fourteen_expr1 = sum(M.x_vpt[v, p, t] for v, pee in M.OMEGA_P if pee == p)
        Fourteen_expr1 -= sum(M.x_spt[s, p, t] for s in M.STORES)
        Fourteen_expr2 = M.y_pt[p,t] - M.y_pt[p,M.T_minus_One[t]]
        return (Fourteen_expr1 - Fourteen_expr2 == 0)

    M.ConstraintFourteen = Constraint(M.PRODUCTS, M.TIMES)



    # Constraint Sixteen and Seventeen
    def ConstraintSixteen_rule(model,s,p,t):
        Sixteen_expr1 = M.z_spt[s, p, t]
        Sixteen_expr1 -= M.Demand[s,p,t]
        Sixteen_expr2 = M.y_spt[s,p,t] - M.y_spt[s,p,M.T_minus_One[t]]
        return (Sixteen_expr1 - Sixteen_expr2 == 0)

    M.ConstraintSixteen = Constraint(M.STORES, M.PRODUCTS, M.TIMES)



    # Constraint Eighteen
    def ConstraintEighteen_rule(model,s,q,t):
        if M.tb <= t <= M.ty:
            if t == 1:
                Eighteen_expr1 = M.z_sqt[s, q, t]
                Eighteen_expr2 = M.y_sqt[s,q,t]
                return (Eighteen_expr1 - Eighteen_expr2 == 0)                
            else:
                Eighteen_expr1 = M.z_sqt[s, q, t]
                Eighteen_expr2 = M.y_sqt[s,q,t] - M.y_sqt[s,q,M.T_minus_One[t]]
                return (Eighteen_expr1 - Eighteen_expr2 == 0)
        else:
            return Constraint.Skip

    M.ConstraintEighteen = Constraint(M.STORES, M.FASHION, M.TIMES)



    # Constraints Nineteen
    def ConstraintNineteen_rule(model, s, t):
        Nineteen_expr1 =  sum(M.V_p[p] * M.z_spt[s, p, t] for p in M.PRODUCTS)
        Nineteen_expr1 += sum(M.V_q[q] * M.z_sqt[s, q, t] for q in M.FASHION)
        Nineteen_expr2 = M.K_s[s]                    
        return (Nineteen_expr1 - Nineteen_expr2 <= 0)

    M.ConstraintNineteen = Constraint(M.STORES, M.TIMES)



    # Constraints Twenty
    def ConstraintTwenty_rule(model, v, t):
        if v in M.VENDORS_P:
            Twenty_expr1 =  sum(M.W_p[p] * M.x_vpt[v,p,t] for ve, p in M.OMEGA_P 
                                                           if ve == v)
        else:
            Twenty_expr1 = 0
            
        if v in M.VENDORS_Q:    
            Twenty_expr1 += sum(M.W_q[q] * M.X_ivq[v,q] * M.rho_vqt[v,q,t] 
                            for ve, q in M.OMEGA_Q if ve == v)
            
        Twenty_expr2 = M.Script_Q * M.n_vt[v,t]                    
        return (Twenty_expr1 - Twenty_expr2 <= 0)

    M.ConstraintTwenty = Constraint(M.VENDORS, M.TIMES)


    # Constraints TwentyOne
    def ConstraintTwentyOne_rule(model, s, t):
        TwentyOne_expr1 =  sum(M.W_p[p] * M.x_spt[s,p,t] for p in M.PRODUCTS)
        TwentyOne_expr1 += sum(M.W_q[q] * M.X_osq[s,q] * M.rho_sqt[s,q,t] for q in M.FASHION)
        TwentyOne_expr2 = M.Script_Q * M.n_st[s,t]                    
        return (TwentyOne_expr1 - TwentyOne_expr2 <= 0)
        
    M.ConstraintTwentyOne = Constraint(M.STORES, M.TIMES)

    return M

if __name__ == '__main__':
    model = Pyomo_tech(0)


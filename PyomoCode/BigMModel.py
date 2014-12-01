#-----------------------------------------------------------------------------
#                              IMPORT MODULES
#-----------------------------------------------------------------------------

from coopr.pyomo import ConcreteModel, Set, Param, PositiveReals
from coopr.pyomo import NonNegativeIntegers, Binary, Var, summation

#-----------------------------------------------------------------------------
#                            MOTIVATION FROM WIFE
#-----------------------------------------------------------------------------

''' my husband is amazingly sexy! RAWR!! love, sara '''

#-----------------------------------------------------------------------------
#                           DECLARE MODEL constraintS
#-----------------------------------------------------------------------------

def big_m_model():

    with open('PyomoCode/Pickled_Data', 'rb') as f:
        rd = pickle.load(f)

    model = ConcreteModel()
    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL PARAMETERS
    #-----------------------------------------------------------------------------

    model.STORES = Set(initialize=rd.STORES)
    model.PRODUCTS = Set(initialize=rd.PRODUCTS)
    model.FASHION = Set(initialize=rd.FASHION)
    model.VENDORS_Q = Set(initialize=rd.VENDORS_Q)
    model.VENDORS_P = Set(initialize=rd.VENDORS_P)
    model.VENDORS = Set(initialize=rd.VENDORS)
    model.TIMES = Set(initialize=rd.TIMES)
    model.PICKING = Set(initialize=rd.PICKING)
    model.PUTAWAY = Set(initialize=rd.PUTAWAY)

    model.SP = model.STORES * model.PRODUCTS
    model.VP = model.VENDORS * model.PRODUCTS
    model.ST = model.STORES * model.TIMES
    model.PT = model.PRODUCTS * model.TIMES
    model.VT = model.VENDORS * model.TIMES
    model.VPT = model.VENDORS * model.PRODUCTS * model.TIMES
    model.SPT = model.STORES * model.PRODUCTS * model.TIMES


    model.OMEGA_Q = Set(initialize=rd.Omega_q, dimen=2)
    model.OMEGA_P = Set(initialize=rd.Omega_p, dimen=2)

    model.T_minus_One = Param(model.TIMES, initialize=rd.T_minus_One)

    def K_init(model, s): return 10000000
    model.K_s = Param(model.STORES, initialize=K_init)

    def L_init(model): return 0
    model.L = Param(initialize=L_init)

    model.Script_Q = Param(initialize=15000)

    model.phi_put = Param(initialize=1.0)
    model.phi_pick = Param(initialize=1.0)
    model.gamma = Param(initialize=0.5)

    model.pt = Param(initialize=1)
    model.tb = Param(initialize=rd.tb)
    model.te = Param(initialize=rd.te)
    model.ty = Param(initialize=rd.ty)

    model.Demand = Param(model.STORES, model.PRODUCTS, model.TIMES, initialize=rd.Demand)

    model.V_p = Param(model.PRODUCTS, initialize=rd.V_p)
    model.V_q = Param(model.FASHION, initialize=rd.V_q)
    model.W_p = Param(model.PRODUCTS, initialize=rd.W_p)
    model.W_q = Param(model.FASHION, initialize=rd.W_q)

    X_ivq = {tup:rd.X_ivq[tup] for tup in model.OMEGA_Q}
    model.X_ivq = Param(model.OMEGA_Q, initialize=X_ivq)
    model.X_osq = Param(model.STORES, model.FASHION, initialize=rd.X_osq)
    model.C_alpha = Param(initialize=rd.C_alpha)
    model.C_beta = Param(initialize=rd.C_beta)

    def C_hp_init(model, p): return .01

    model.C_hp = Param(model.PRODUCTS, initialize=C_hp_init)
    model.C_hq = Param(model.FASHION, initialize=C_hp_init)


    def C_hsp_init(model, s, p): return .05

    model.C_hsp = Param(model.STORES, model.PRODUCTS, initialize=C_hsp_init)
    model.C_hsq = Param(model.STORES, model.FASHION, initialize=C_hsp_init)


    model.C_fv = Param(model.VENDORS, initialize=rd.C_fv)
    model.C_fs = Param(model.STORES, initialize=rd.C_fs)
    model.C_vv = Param(model.VENDORS, initialize=rd.C_vv)
    model.C_vs = Param(model.STORES, initialize=rd.C_vs)


    model.Lambda_put = Param(model.PUTAWAY, initialize=rd.lambda_put)
    model.MHE = Param(model.PUTAWAY, initialize=rd.MHE)
    model.Lambda_pick = Param(model.PICKING, initialize=rd.lambda_pick)
    model.Cth_put = Param(model.PUTAWAY, initialize=rd.C_put)
    model.Cth_pick = Param(model.PICKING, initialize=rd.C_pick)

    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL VARIABLES
    #-----------------------------------------------------------------------------

    model.alpha_put = Var(bounds=(0.0, 500),
                           within=NonNegativeIntegers)

    model.alpha_pick = Var(bounds=(0.0, 500),
                           within=NonNegativeIntegers)

    model.theta_put = Var(model.PUTAWAY, within=Binary)
    model.theta_pick = Var(model.PICKING, within=Binary)

    model.MHE_cost =  = Var(model.PUTAWAY, within=NonNegativeIntegers)


    model.beta_put = Var(model.TIMES,
                         within=NonNegativeIntegers)

    model.beta_pick = Var(model.TIMES,
                         within=NonNegativeIntegers)

    model.tau_sq = Var(model.STORES, model.FASHION,
                         within=NonNegativeIntegers)

    model.rho_vqt = Var(model.OMEGA_Q, model.TIMES,
                         within=Binary)

    model.rho_sqt = Var(model.STORES, model.FASHION, model.TIMES,
                         within=Binary)

    model.x_vpt = Var(model.OMEGA_P, model.TIMES,
                         within=NonNegativeIntegers)

    model.x_spt = Var(model.STORES, model.PRODUCTS, model.TIMES,
                         within=NonNegativeIntegers)

    model.y_pt = Var(model.PRODUCTS, model.TIMES,
                         within=NonNegativeIntegers)

    model.y_spt = Var(model.STORES, model.PRODUCTS, model.TIMES,
                         within=NonNegativeIntegers)

    model.y_sqt = Var(model.STORES, model.FASHION, model.TIMES,
                         within=NonNegativeIntegers)

    model.z_spt = Var(model.STORES, model.PRODUCTS, model.TIMES,
                         within=NonNegativeIntegers)

    model.z_sqt = Var(model.STORES, model.FASHION, model.TIMES,
                         within=NonNegativeIntegers)

    model.n_vt =  Var(model.VENDORS, model.TIMES,
                         within=NonNegativeIntegers)

    model.n_st =  Var(model.STORES, model.TIMES,
                         within=NonNegativeIntegers)



    def objective_rule(model):
        Workers_Cost = model.Ca * (model.alpha_put + model.alpha_pick) + \
                       model.Cb * sum((model.beta_put[t] + model.beta_pick[t])
                                       for t in model.TIMES)

        MHE_Cost = summation(model.MHE_cost)

        Tech_Cost = summation(model.Cth_put, model.theta_put) + \
                    summation(model.Cth_pick, model.theta_pick)


        Holding_Cost =  sum(model.C_hp[p] * model.y_pt[p,t]
                            for p in model.PRODUCTS for t in model.TIMES)
        Holding_Cost += sum(model.C_hq[q] * model.X_osq[s,q] * model.tau_sq[s,q]
                            for s in model.STORES for q in model.FASHION)
        Holding_Cost += sum(model.C_hsp[s,p] * model.y_spt[s,p,t]
                            for s in model.STORES for p in model.PRODUCTS for t in model.TIMES)
        Holding_Cost += sum(model.C_hsq[s,q] * model.y_sqt[s,q,t]
                            for s in model.STORES for q in model.FASHION for t in model.TIMES)


        Fixed_Shipping_Cost =  sum(model.C_fv[v] * model.n_vt[v,t]
                                  for v in model.VENDORS for t in model.TIMES)
        Fixed_Shipping_Cost += sum(model.C_fs[s] * model.n_st[s,t]
                                  for s in model.STORES for t in model.TIMES)


        Var_Shipping_Cost =  sum(model.C_vv[v] * model.W_p[p] * model.x_vpt[v,p,t]
                                  for v, p in model.OMEGA_P for t in model.TIMES)
        Var_Shipping_Cost += sum(model.C_vv[v] * model.W_q[q] * model.X_ivq[v,q] * model.rho_vqt[v,q,t]
                                  for v, q in model.OMEGA_Q for t in model.TIMES)
        Var_Shipping_Cost += sum(model.C_vs[s] * model.W_p[p] * model.x_spt[s,p,t]
                                  for s in model.STORES for p in model.PRODUCTS for t in model.TIMES)
        Var_Shipping_Cost += sum(model.C_vs[s] * model.W_q[q] * model.X_osq[s,q] * model.rho_sqt[s,q,t]
                                  for s in model.STORES for q in model.FASHION for t in model.TIMES)


        FS_Expr = (Workers_Cost + MHE_Cost + Tech_Cost + Holding_Cost
                                + Fixed_Shipping_Cost + Var_Shipping_Cost)

        return FS_Expr

    model.objective = Objective(sense=minimize)


    def BigM_MHE_LOWER_rule(model, i):
        expr = model.MHE_COST[i]
        expr -= sum(model.MHE[i] * (model.alpha_put + model.beta_put[t])
                    for t in model.TIMES)
        return (-model.M_MHE * (1 - model.theta_put[i]), expr, None)

    model.BigM_MHE_LOWER = Constraint(model.PUTAWAY, model.TIMES)

    def BigM_MHE_UPPER_rule(model, i):
        expr = model.MHE_COST[i]
        expr -= sum(model.MHE[i] * (model.alpha_put + model.beta_put[t])
                    for t in model.TIMES)
        return (None, expr, model.M_MHE * (1 - model.theta_put[i]))

    model.BigM_MHE_UPPER = Constraint(model.PUTAWAY, model.TIMES)

    def ConstraintFour_rule(model, i, t):
        Four_expr1 = sum(model.x_vpt[v,p,t] for v, p in model.OMEGA_P)
        Four_expr1 += sum(model.X_ivq[v,q] * model.rho_vqt[v,q,t] for v, q in model.OMEGA_Q)
        Four_expr2 = model.Lambda_put[i] * (model.alpha_put + model.phi_put * model.beta_put[t])
        return (Four_expr1 - Four_expr2 <= model.BigM * (1 - model.theta_put[i]))

    model.ConstraintFour = Constraint(model.PUTAWAY, model.TIMES)


    def ConstraintFive_rule(model, j, t):
        Five_expr1 = sum(model.x_spt[s,p,t] for s in model.STORES for p in model.PRODUCTS)
        Five_expr1 += sum(model.X_osq[s,q] * model.rho_sqt[s,q,t] for s in model.STORES for q in model.FASHION)
        Five_expr2 = model.Lambda_pick[j]  * (model.alpha_pick + model.phi_pick * model.beta_pick[t])
        return (Five_expr1 - Five_expr2 <= model.BigM * (1 - model.theta_pick[j]))

    model.ConstraintFive = Constraint(model.PICKING, model.TIMES)



    # Constraint Six
    def ConstraintSixPutaway_rule(model, t):
        Six_expr1 = model.beta_put[t]
        Six_expr2 = model.gamma * model.alpha_put
        return (Six_expr1 - Six_expr2 <= 0)

    model.ConstraintSixPutaway = Constraint(model.TIMES)



    def ConstraintSixPicking_rule(model, t):
        Six_expr1 = model.beta_pick[t]
        Six_expr2 = model.gamma * model.alpha_pick
        return (Six_expr1 - Six_expr2 <= 0)

    model.ConstraintSixPicking = Constraint(model.TIMES)



    # Constraint Seven
    def ConstraintSeven_rule(model, v, q, s):
        Seven_expr1 = model.tau_sq[s,q]
        Seven_expr2 =  sum(t * model.rho_sqt[s, q, t] for t in model.TIMES)
        Seven_expr2 -= sum(t * model.rho_vqt[v, q, t] for t in model.TIMES)
        return (Seven_expr1 - Seven_expr2 == 0)

    model.ConstraintSeven = Constraint(model.OMEGA_Q, model.STORES)



    # Constraint Eight
    def ConstraintEight_rule(model, s, q):
        Eight_expr1 = model.tau_sq[s,q]
        return (model.pt - Eight_expr1 <= 0)

    model.ConstraintEight = Constraint(model.STORES, model.FASHION)



    # Constraint Nine
    def ConstraintNine_rule(model, s, q):
        Nine_expr1 = sum(t * model.rho_sqt[s, q, t] for t in model.TIMES)
        Nine_expr2 = model.ty - model.L
        return (Nine_expr1 - Nine_expr2 <= 0)

    model.ConstraintNine = Constraint(model.STORES, model.FASHION)



    # Constraint Ten
    def ConstraintTenVendor_rule(model, v, q):
        Ten_expr1 = sum(model.rho_vqt[v, q, t] for t in model.TIMES if model.tb <= t <= model.te)
        return (Ten_expr1 - 1 == 0)

    model.ConstraintTenVendor = Constraint(model.OMEGA_Q)



    def ConstraintTenStore_rule(model, s, q):
        Ten_expr1 = sum(model.rho_sqt[s, q, t] for t in model.TIMES if model.tb <= t <= model.ty)
        return (Ten_expr1 - 1 == 0)

    model.ConstraintTenStore = Constraint(model.STORES, model.FASHION)

    # Constraint Ten Prime
    def ConstraintTenPrimeVendor_rule(model, v, q, t):
        if not model.tb <= t <= model.ty:
            return model.rho_vqt[v, q, t] == 0
        else:
            return Constraint.Skip

    model.ConstraintTenPrimeVendor = Constraint(model.OMEGA_Q, model.TIMES)

    def ConstraintTenPrimeStore_rule(model, s, q, t):
        if not model.tb <= t <= model.ty:
            return model.rho_sqt[s, q, t] == 0
        else:
            return Constraint.Skip

    model.ConstraintTenPrimeStore = Constraint(model.STORES, model.FASHION, model.TIMES)

    # Constraint Eleven and Twelve
    def ConstraintEleven_rule(model, s, p, t):
        assert model.L == 0

        Eleven_expr1 = model.z_spt[s,p,t]
        Eleven_expr2 = model.x_spt[s,p,t]
        return (Eleven_expr1 - Eleven_expr2 == 0)

    model.ConstraintEleven = Constraint(model.STORES, model.PRODUCTS, model.TIMES)


    # Constraint Thirteen
    def ConstraintThirteen_rule(model, s, q, t):
        assert model.L== 0
        if model.tb <= t <= model.ty:
            Thirteen_expr1 = model.z_sqt[s,q,t]
            Thirteen_expr2 = model.X_osq[s,q] * model.rho_sqt[s,q,t]
            return (Thirteen_expr1 - Thirteen_expr2 == 0)
        else:
            return Constraint.Skip

    model.ConstraintThirteen = Constraint(model.STORES, model.FASHION, model.TIMES)

    # Constraint Fourteen and Fifteen
    def ConstraintFourteen_rule(model, p, t):
        Fourteen_expr1 = sum(model.x_vpt[v, p, t] for v, pee in model.OMEGA_P if pee == p)
        Fourteen_expr1 -= sum(model.x_spt[s, p, t] for s in model.STORES)
        Fourteen_expr2 = model.y_pt[p,t] - model.y_pt[p,model.T_minus_One[t]]
        return (Fourteen_expr1 - Fourteen_expr2 == 0)

    model.ConstraintFourteen = Constraint(model.PRODUCTS, model.TIMES)



    # Constraint Sixteen and Seventeen
    def ConstraintSixteen_rule(model,s,p,t):
        Sixteen_expr1 = model.z_spt[s, p, t]
        Sixteen_expr1 -= model.Demand[s,p,t]
        Sixteen_expr2 = model.y_spt[s,p,t] - model.y_spt[s,p,model.T_minus_One[t]]
        return (Sixteen_expr1 - Sixteen_expr2 == 0)

    model.ConstraintSixteen = Constraint(model.STORES, model.PRODUCTS, model.TIMES)



    # Constraint Eighteen
    def ConstraintEighteen_rule(model,s,q,t):
        if model.tb <= t <= model.ty:
            if t == 1:
                Eighteen_expr1 = model.z_sqt[s, q, t]
                Eighteen_expr2 = model.y_sqt[s,q,t]
                return (Eighteen_expr1 - Eighteen_expr2 == 0)
            else:
                Eighteen_expr1 = model.z_sqt[s, q, t]
                Eighteen_expr2 = model.y_sqt[s,q,t] - model.y_sqt[s,q,model.T_minus_One[t]]
                return (Eighteen_expr1 - Eighteen_expr2 == 0)
        else:
            return Constraint.Skip

    model.ConstraintEighteen = Constraint(model.STORES, model.FASHION, model.TIMES)



    # Constraints Nineteen
    def ConstraintNineteen_rule(model, s, t):
        Nineteen_expr1 =  sum(model.V_p[p] * model.z_spt[s, p, t] for p in model.PRODUCTS)
        Nineteen_expr1 += sum(model.V_q[q] * model.z_sqt[s, q, t] for q in model.FASHION)
        Nineteen_expr2 = model.K_s[s]
        return (Nineteen_expr1 - Nineteen_expr2 <= 0)

    model.ConstraintNineteen = Constraint(model.STORES, model.TIMES)



    # Constraints Twenty
    def ConstraintTwenty_rule(model, v, t):
        if v in model.VENDORS_P:
            Twenty_expr1 =  sum(model.W_p[p] * model.x_vpt[v,p,t] for ve, p in model.OMEGA_P
                                                           if ve == v)
        else:
            Twenty_expr1 = 0

        if v in model.VENDORS_Q:
            Twenty_expr1 += sum(model.W_q[q] * model.X_ivq[v,q] * model.rho_vqt[v,q,t]
                            for ve, q in model.OMEGA_Q if ve == v)

        Twenty_expr2 = model.Script_Q * model.n_vt[v,t]
        return (Twenty_expr1 - Twenty_expr2 <= 0)

    model.ConstraintTwenty = Constraint(model.VENDORS, model.TIMES)


    # Constraints TwentyOne
    def ConstraintTwentyOne_rule(model, s, t):
        TwentyOne_expr1 =  sum(model.W_p[p] * model.x_spt[s,p,t] for p in model.PRODUCTS)
        TwentyOne_expr1 += sum(model.W_q[q] * model.X_osq[s,q] * model.rho_sqt[s,q,t] for q in model.FASHION)
        TwentyOne_expr2 = model.Script_Q * model.n_st[s,t]
        return (TwentyOne_expr1 - TwentyOne_expr2 <= 0)

    model.ConstraintTwentyOne = Constraint(model.STORES, model.TIMES)

    return model

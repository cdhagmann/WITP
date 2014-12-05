#-----------------------------------------------------------------------------
#                              IMPORT MODULES
#-----------------------------------------------------------------------------

from coopr.pyomo import *
import pickle
from coopr import neos
from coopr.opt import SolverFactory
import coopr.environ
from Function_Module import *
#-----------------------------------------------------------------------------
#                            MOTIVATION FROM WIFE
#-----------------------------------------------------------------------------

''' my husband is amazingly sexy! RAWR!! love, sara '''

#-----------------------------------------------------------------------------
#                           DECLARE MODEL constraintS
#-----------------------------------------------------------------------------

class Struct():
    pass

def num_strip(s):
    return int( ''.join( c for c in str(s) if c.isdigit() ) )


def tech_idx(tup):
    i, j = tup
    return (i - 1) * 6 + (j - 1)

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

    model.BigM = Param(initialize=500)
    model.M_MHE = Param(initialize=50000)

    #-----------------------------------------------------------------------------
    #                           DECLARE MODEL VARIABLES
    #-----------------------------------------------------------------------------

    model.alpha_put = Var(bounds=(0.0, model.BigM),
                           within=NonNegativeIntegers)

    model.alpha_pick = Var(bounds=(0.0, model.BigM),
                           within=NonNegativeIntegers)

    model.theta_put = Var(model.PUTAWAY, within=Binary)
    model.theta_pick = Var(model.PICKING, within=Binary)

    model.MHE_Cost = Var(model.PUTAWAY, within=NonNegativeIntegers)


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
        Workers_Cost = model.C_alpha * (model.alpha_put + model.alpha_pick) + \
                       model.C_beta * sum((model.beta_put[t] + model.beta_pick[t])
                                       for t in model.TIMES)

        MHE_Cost = summation(model.MHE_Cost)

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
    '''FLAG'''

    model.c1 = Constraint(expr=model.theta_put['i2'] == 1)
    model.c2 = Constraint(expr=model.theta_pick['j5'] == 1)
    model.c3 = Constraint(expr=model.alpha_pick <= 10)

    def constraint1_rule(model):
        return (summation(model.theta_put), 1)

    model.constraint1 = Constraint()
    def constraint2_rule(model):
        return (summation(model.theta_pick), 1)

    model.constraint2 = Constraint()
    def BigM_MHE_LOWER_rule(model, i):
        expr = model.MHE_Cost[i]
        expr -= sum(model.MHE[i] * (model.alpha_put + model.beta_put[t])
                    for t in model.TIMES)
        return -model.M_MHE * (1 - model.theta_put[i]) <= expr

    model.BigM_MHE_LOWER = Constraint(model.PUTAWAY)

    def BigM_MHE_UPPER_rule(model, i):
        expr = model.MHE_Cost[i]
        expr -= sum(model.MHE[i] * (model.alpha_put + model.beta_put[t])
                    for t in model.TIMES)
        return expr <= model.M_MHE * (1 - model.theta_put[i])

    model.BigM_MHE_UPPER = Constraint(model.PUTAWAY)

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

def solve_big_m_model(model=None):
    if model is None:
        model = big_m_model()


    instance = model.create()
    opt = SolverFactory('gurobi')
    results = opt.solve(model)

    instance.load(results)
    return instance


def big_M_output(inst):
    obj = instance.objective()

    for t in inst.theta_put:
        if inst.theta_put[t].value == 1:
            i = t


    for t in inst.theta_pick:
        if inst.theta_pick[t].value == 1:
            j = t

    idx = tech_idx((num_strip(i), num_strip(j)))

    tech = 'Tech' + str(idx)

    PickingCost = inst.C_alpha.value * inst.alpha_pick.value
    PickingCost += inst.C_beta.value * sum(inst.beta_pick[t].value for t in inst.TIMES)

    PutawayCost = inst.C_alpha.value * inst.alpha_put.value
    PutawayCost += inst.C_beta.value * sum(inst.beta_put[t].value for t in inst.TIMES)

    MHECost = sum(inst.MHE_Cost[i] for i in inst.PUTAWAY)

    PutawayTechCost = inst.Cth_put[i]

    PickingTechCost = inst.Cth_pick[j]

    WhBasicInvCost = sum(inst.C_hp[p] * inst.y_pt[p, t].value
                         for p in inst.PRODUCTS for t in inst.TIMES)

    WhFashionInvCost = sum(inst.C_hq[q] * inst.X_osq[s, q] * inst.tau_sq[s, q].value
                           for s in inst.STORES for q in inst.FASHION)

    StoreBasicInvCost = sum(inst.C_hsp[s, p] * inst.y_spt[s, p, t].value
                            for s in inst.STORES for p in inst.PRODUCTS for t in inst.TIMES)

    StoreFashionInvCost = sum(inst.C_hsq[s, q] * inst.y_sqt[s, q, t].value
                              for s in inst.STORES for q in inst.FASHION for t in inst.TIMES)

    BasicInbCost = sum(inst.C_fv[v] * inst.n_vt[v, t].value
                       for v in inst.VENDORS_P for t in inst.TIMES)
    BasicInbCost += sum(inst.C_vv[v] * inst.W_p[p] * inst.x_vpt[v, p, t].value
                        for v, p in inst.OMEGA_P for t in inst.TIMES)

    FashionInbCost = sum(inst.C_fv[v] * inst.n_vt[v, t].value
                         for v in inst.VENDORS_Q for t in inst.TIMES)
    FashionInbCost += sum(inst.C_vv[v] * inst.W_q[q] *
                          inst.X_ivq[v, q] * inst.rho_vqt[v, q, t].value
                          for v, q in inst.OMEGA_Q for t in inst.TIMES)

    OutboundCost = sum(inst.C_fs[s] * inst.n_st[s, t].value
                       for s in inst.STORES for t in inst.TIMES)
    OutboundCost += sum(inst.C_vs[s] * inst.W_p[p] * inst.x_spt[s, p, t].value
                        for s in inst.STORES for p in inst.PRODUCTS for t in inst.TIMES)
    OutboundCost += sum(inst.C_vs[s] * inst.W_q[q] *
                        inst.X_osq[s, q] * inst.rho_sqt[s, q, t].value
                        for s in inst.STORES for q in inst.FASHION for t in inst.TIMES)

    with open('BigM_{}.txt'.format(idx), 'w') as f:
        f.write('Results from {}\n'.format(tech))
        f.write('Putaway Technology: {}\n'.format(instance.Lambda_put[i]/8.))
        f.write('Picking Technology: {}\n\n'.format(instance.Lambda_pick[j]/8.))
        f.write('Full-Time Putaway workers: {}\n'.format(inst.alpha_put.value))
        f.write('Full-Time Picking workers: {}\n'.format(inst.alpha_pick.value))
        f.write('\nCost Breakdown:\n')
        f.write('\tMHECost              {}\n'.format(curr(MHECost)))
        f.write('\tPutawayTechCost      {}\n'.format(curr(PutawayTechCost)))
        f.write('\tPickingTechCost      {}\n'.format(curr(PickingTechCost)))
        f.write('\tWhBasicInvCost       {}\n'.format(curr(WhBasicInvCost)))
        f.write('\tBasicInbCost         {}\n'.format(curr(BasicInbCost)))
        f.write('\tWhFashionInvCost     {}\n'.format(curr(WhFashionInvCost)))
        f.write('\tFashionInbCost       {}\n'.format(curr(FashionInbCost)))
        f.write('\tPutawayCost          {}\n'.format(curr(PutawayCost)))
        f.write('\tStoreBasicInvCost    {}\n'.format(curr(StoreBasicInvCost)))
        f.write('\tStoreFashionInvCost  {}\n'.format(curr(StoreFashionInvCost)))
        f.write('\tOutboundCost         {}\n'.format(curr(OutboundCost)))
        f.write('\tPickingCost          {}\n'.format(curr(PickingCost)))
        f.write('\tTotal                {}\n'.format(curr(obj)))
        with Redirect(f, f):
            print '\n\nPrinting New Fashion Solution:\n'
            ri = lambda num: int(round(num, 0))
            print 'Inbound Fashion Solution'
            for v, q in sorted(inst.OMEGA_Q):
                print num_strip(v), '\t',
                print num_strip(q), '\t',
                for t in sorted(inst.TIMES):
                    if inst.rho_vqt[v, q, t].value == 1:
                        print t, '\t', ri(inst.X_ivq[v, q])

            print '\nOutbound Solution'
            for s in sorted(inst.STORES):
                for q in sorted(inst.FASHION):
                    print num_strip(s), '\t',
                    print num_strip(q), '\t',
                    for t in sorted(inst.TIMES):
                        if inst.rho_sqt[s, q, t].value == 1:
                            print t, '\t', ri(inst.X_osq[s, q])

            print '\nStore Inventory'
            for s in sorted(inst.STORES):
                print 'Store{}'.format(num_strip(s))
                for q in sorted(inst.FASHION):
                    print num_strip(v),
                    for t in sorted(inst.TIMES):
                        num = ri(inst.y_sqt[s, q, t].value)
                        print str(num) + (' ' * (10 - len(str(num)))),
                    print

            print '\nShipments from (Basic) Vendor to Warehouse'
            for v in sorted(inst.VENDORS_Q):
                for t in sorted(inst.TIMES):
                    num = ri(inst.n_vt[v, t].value)
                    print str(num) + (' ' * (10 - len(str(num)))),
                print

            print '\nPrinting New Basic Solution:\n'

            print '\nInbound Solution'
            for v, p in sorted(inst.OMEGA_P):
                for t in sorted(inst.TIMES):
                    print num_strip(v), '\t',
                    print num_strip(p), '\t',
                    print t, '\t',
                    print ri(inst.x_vpt[v, p, t].value)

            print '\nOutbound Solution'
            for s in sorted(inst.STORES):
                print 'Store{}'.format(num_strip(s))
                for p in sorted(inst.PRODUCTS):
                    print num_strip(p),
                    for t in sorted(inst.TIMES):
                        num = ri(inst.x_spt[s, p, t].value)
                        print str(num) + (' ' * (10 - len(str(num)))),
                    print

            print '\nWarehouse Inventory'
            for p in sorted(inst.PRODUCTS):
                print num_strip(p),
                for t in sorted(inst.TIMES):
                    num = ri(inst.y_pt[p, t].value)
                    print str(num) + (' ' * (10 - len(str(num)))),
                print

            print '\nStore Inventory'
            for s in sorted(inst.STORES):
                print 'Store{}'.format(num_strip(s))
                for p in sorted(inst.PRODUCTS):
                    print num_strip(p),
                    for t in sorted(inst.TIMES):
                        num = ri(inst.y_spt[s, p, t].value)
                        print str(num) + (' ' * (10 - len(str(num)))),
                    print

            print '\nShipments from (Basic) Vendor to Warehouse'
            for v in sorted(inst.VENDORS_P):
                for t in sorted(inst.TIMES):
                    num = ri(inst.n_vt[v, t].value)
                    print str(num) + (' ' * (10 - len(str(num)))),
                print

            print '\nShipments from (Fashion) Vendor to Warehouse'
            for v in sorted(inst.VENDORS_Q):
                for t in sorted(inst.TIMES):
                    num = ri(inst.n_vt[v, t].value)
                    print str(num) + (' ' * (10 - len(str(num)))),
                print

            print '\nShipments from Warehouse to Stores'
            for s in sorted(inst.STORES):
                print num_strip(s),
                for t in sorted(inst.TIMES):
                    num = ri(inst.n_st[s, t].value)
                    print str(num) + (' ' * (10 - len(str(num)))),
                print

            print '\nNo of part-time putaway workers:'
            for t in sorted(inst.TIMES):
                num = ri(inst.beta_put[t].value)
                print str(num) + (' ' * (10 - len(str(num)))),
            print

            print '\nNo of part-time picking workers:'
            for t in sorted(inst.TIMES):
                num = ri(inst.beta_pick[t].value)
                print str(num) + (' ' * (10 - len(str(num)))),
            print


if __name__ == '__main__':
    instance = solve_big_m_model()
    for t in instance.theta_put:
        if instance.theta_put[t].value == 1:
            i = t


    for t in instance.theta_pick:
        if instance.theta_pick[t].value == 1:
            j = t

    idx = tech_idx((num_strip(i), num_strip(j)))
    tech = 'Tech' + str(idx) 

    print tech, instance.objective()
    big_M_output(instance)

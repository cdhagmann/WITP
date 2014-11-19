using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WITP_TPH
{
    class TPH
    {
        public int _numStores;
        public int _numBvendors;
        public int _numFvendors;
        public int _numWareHouses;
        public int _numTimePeriods;
        public int _numBasic;
        public int _numFashion;
        List<int> _numPickers;                          //List that contains number of required pickers in each time period
        List<int> _numPutaway;                          //List that contains number of putaway workers required in each time period
        List<List<float>> _numBasicShipmentsVtoW;       //[time period] [vendor] as currently we deal with only one warehouse -- i took float for the sake of heuristic
        List<List<float>> _numFashionShipmentsVtoW;     //[time period] [vendor] as currently we deal with only one warehouse -- i took float for the sake of heuristic
        List<List<float>> _numShipmentsWtoS;            //[time period] [store] as currently we deal with only one warehouse
        //List<float> _pickerBlocking;

        string fileName = "FILENAMEVAL";
        StreamReader sr = new StreamReader("WITPdataSet_FILENAMEVAL.txt");

        TextWriter twSol;               //= new StreamWriter("OptPolicyResults\\Solution.txt");  
        bool AllParamComb = false;      //flag to print the output files
        List<int> _warehouseVol;
        List<int> _storeVol;
        List<List<List<int>>> _demand;                  //[Store][Product][time]
        List<List<int>> _demandFashion;                 //[Store][Product]
        // List<List<float>> _storeHoldingCost;         //[Store][Product]    
        // List<List<float>> _warehouseHoldingCost;     //[WareHouse][Product]    
        List<List<float>> _fixedCostBasicVtoW;          //[Vendor][WareHouse]
        List<List<float>> _fixedCostFashionVtoW;        //[Vendor][WareHouse]
        List<List<float>> _fixedCostWtoS;               //[WareHouse][Store]
        List<List<float>> _varFixedCostBasicVtoW;       //
        List<List<float>> _varFixedCostFashionVtoW;
        List<List<float>> _varFixedCostWtoS;
        List<List<float>> _variableCostBasicVtoW;
        List<List<float>> _variableCostFashionVtoW;
        List<List<float>> _variableCostWtoS;
        List<List<int>> _VPbasic;
        List<List<int>> _VPfashion;
        List<float> _productVol;
        List<float> _fashionVol;
        List<float> _weightBasic;
        List<float> _weightFashion;
        List<List<List<int>>> _WhBasicInv;          //[WareHouse][Product][time]
        List<List<List<int>>> _WhFashionInv;        //[WareHouse][Fashion][time]
        List<List<List<int>>> _StoreBasicInv;       //[Store][Product][time]
        List<List<List<int>>> _StoreFashionInv;     //[Store][Fashion][time]
        List<int> _leadTime;                        //[Warehouse][store]      
        //List<int> _putawayRateList = new List<int> { 1200, 2400, 3360, 4080, 4800, 6000 };
        List<int> _putawayRateList = new List<int> { 600, 1200, 2400, 3600, 4800, 6000 };
        List<int> _pickRateList = new List<int> { 100, 200, 300, 400, 500, 1000 };
        List<float> _putawayTechCost = new List<float> { 0, 212, 708, 959, 2055, 2740 };       // costs calculated for 1 day
        List<float> _pickTechCost = new List<float> { 23, 121, 274, 411, 639, 1826 };       // costs calculated for 1 day
        List<float> _putMHECost = new List<float> { 9, 27, 37, 87, 0, 0 };                          //Material handling cost for 1 day


        List<int> _InbBasicSol;  //      
        List<long> _OutbBasicSol; //   
        List<int> _InbFashionSol;  //      
        List<long> _OutbFashionSol; //

        int TRUCKCAP = 15000;                   //Capacity of a trailer in lbs
        int currentPutawayTech;
        int prevPutawayTech;
        int currentPickTech;
        int prevPickTech;
        int PICKRATE;                          //pick rate of a worker - 300 units/hr during a shift of 8hrs duration
        int PUTAWAYRATE;                       //pick crate of a worker - 300 units/hr during a shift of 8hrs duration
        float PutawayTechCost;
        float PickTechCost;
        float FT_PUTCOST = FULLTIMECOSTVAL;
        float FT_PICKCOST = FULLTIMECOSTVAL;
        float PT_PUTCOST = PARTTIMECOSTVAL;
        float PT_PICKCOST = PARTTIMECOSTVAL;
        float gamma = GAMMAVALF;                     //Percentage of fulll time workers allowed to work as part time at the warehouse
        float phi = PHIVALF;

        float _whHoldingCost = 0.01F;           //fixed value ..instead of reading from dataset..to minimize computation time...
        float _stHoldingCost = 0.05F;
        int _beginTime = BEGINTIMEVAL;                    //Begin time period for fashion event in the time-horizon
        int _endTime = ENDTIMEVAL;                      //*********End time-period for fashion event in the time-horizon ...very important..check for every data set else it will throw out of memory exception
        int _dueTime = DUETIMEVAL;                      //Due date for fashion products to reach store..in the time-horizon
        int pt = PROCESSINGTIMEVAL;                             //processing time of a product at warehouse 


    //Iterations set up
        int NoTotalIterations = 100;           //iterations for outer loop
        int NoOfIterations = 1000;               //iterations for inner loop
        int TotalGenerations = 1;               // 
        int NumSwapIterations = 25;              //determines the max number of iterations to swap 
        int stopIter = 5;                       //determines the number of iterations to stop if there is no improvement in the solutions
        int MaxTechIter = 5;                    //the maximum number of allowed iterations to select technology
        int ProbabilityValue = 5;
        int Penalty;



        string ResPath = "./RunAllParamComb-Results/";        // your code goes here
        List<int> PickRateArray = new List<int>() { 200, 300, 500 };
        List<int> PutawayRateArray = new List<int>() { 200, 300, 500 };

        List<float> gammaArray = new List<float>() { 0.0F, 0.5F, 1.0F, 2.0F };
        List<float> phiArray = new List<float>() { 0.75F, 1.0F };

        
        public void SetSolutions(List<int> InboundBasic, List<int> InboundFashion, List<long> OutboundBasic, List<long> OutboundFashion)
        {
            _InbBasicSol = new List<int>();
            for (int i = 0; i < InboundBasic.Count; i++)
            {
                _InbBasicSol.Add(InboundBasic[i]);
            }
            _InbFashionSol = new List<int>();
            for (int i = 0; i < InboundFashion.Count; i++)
            {
                _InbFashionSol.Add(InboundFashion[i]);
            }
            _OutbBasicSol = new List<long>();
            for (int i = 0; i < OutboundBasic.Count; i++)
            {
                _OutbBasicSol.Add(OutboundBasic[i]);
            }
            _OutbFashionSol = new List<long>();
            for (int i = 0; i < OutboundFashion.Count; i++)
            {
                _OutbFashionSol.Add(OutboundFashion[i]);
            }
        }




        /// <summary>
        /// Method to run all possible combinations for sensitive analysis ...code for automation
        /// </summary>
        public void RunAllParamComb()
        {
            AllParamComb = true;            //To print output files
            ReadInputData();
            string DirPath = "RunAllParamComb-Results"; // your code goes here
            try
            {
                if (!System.IO.Directory.Exists(DirPath))
                {
                    // Try to create the directory.
                    DirPath = "RunAllParamComb-Results"; // your code goes here
                    System.IO.Directory.CreateDirectory(DirPath);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
            }
            string SubDirectoryPath = "";

            for (int _pra = 0; _pra < PickRateArray.Count; _pra++) //Pick rate array bounding for loop
                for (int _gamma = 0; _gamma < gammaArray.Count; _gamma++) //Gamma bounding for loop
                    for (int _phi = 0; _phi < phiArray.Count; _phi++) //Phi bounding for loop
                    {
                        PICKRATE = PickRateArray[_pra];                         //pick rate of a worker - 300 units/hr during a shift of 8hrs duration
                        PUTAWAYRATE = PutawayRateArray[_pra];                      //pick crate of a worker - 300 units/hr during a shift of 8hrs duration                        
                        gamma = gammaArray[_gamma];                         //Percentage of fulll time workers allowed to work as part time at the warehouse
                        phi = phiArray[_phi];

                        //Creating a directory for storing solutions
                        SubDirectoryPath = "pickput_" + PICKRATE.ToString() + "_g_" + gamma.ToString() + "_p_" + phi.ToString();
                        String CreateDir = DirPath + "//" + SubDirectoryPath;
                        ResPath = CreateDir + "_";
                        //  OptimizeFn(true);
                        //Console.Read();
                    }
        }


        public void ReadInputData()
        {
            string parameter;
            while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
            {
                //to read and store WarehouseVolume
                if (parameter.Equals("WarehouseVolume:["))
                {
                    int i = 0;
                    _warehouseVol = new List<int>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', ')', '\t', '[', ']' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _warehouseVol[i].Add(Int32.Parse(parts[0]));
                            _warehouseVol.Add(Int32.Parse(parts[1]));
                            i++;
                        }
                    }
                }

                //to read and store StoreVolume
                parameter = sr.ReadLine();
                if (parameter.Equals("StoreVolume:["))
                {
                    int i = 0;
                    _storeVol = new List<int>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _storeVol[i].Add(Int32.Parse(parts[0]));
                            _storeVol.Add(Int32.Parse(parts[1]));
                            i++;
                        }
                    }
                }

                //to read and store demand of each store for each product in each time-period
                parameter = sr.ReadLine();
                if (parameter.Equals("StoreDemand:["))
                {
                    _demand = new List<List<List<int>>>();
                    int _pStoreId = -1;
                    int _pProductId = -1;
                    int _cStoreId = 0;
                    int _cProductId = 0;
                    int Storeid = -1;
                    int _ProductId = -1;
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cStoreId = Int32.Parse(parts[0]);
                            if (_pStoreId != _cStoreId)
                            {
                                var l = new List<List<int>>();
                                _pStoreId = _cStoreId;// = Int32.Parse(parts[0]);
                                _demand.Add(l);
                                _pProductId = -1;
                                _cProductId = 0;
                                Storeid++; _ProductId = -1;
                            }
                            _cProductId = Int32.Parse(parts[1]);
                            if (_pProductId != _cProductId)
                            {
                                var n = new List<int>();
                                _pProductId = _cProductId;// = Int32.Parse(parts[1]);
                                _demand[Storeid].Add(n);
                                _ProductId++;
                            }

                            _demand[Storeid][_ProductId].Add(Int32.Parse(parts[3]));
                        }
                    }
                }


                //to read and store fixed cost from vendor to warehouse
                parameter = sr.ReadLine();
                if (parameter.Equals("FixedCostVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _fixedCostBasicVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _fixedCostBasicVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }

                            _fixedCostBasicVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }
                    }

                }
                //to read and store fixed cost from warehouse to stores
                parameter = sr.ReadLine();
                if (parameter.Equals("FixedCostWarehouseToStore:["))
                {
                    int i = 0;
                    int _pWareHouseId = -1;
                    int _cWareHouseId = 0;
                    int WareHouseCount = -1;

                    _fixedCostWtoS = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cWareHouseId = Int32.Parse(parts[0]);
                            if (_pWareHouseId != _cWareHouseId)
                            {
                                var l = new List<float>();
                                _fixedCostWtoS.Add(l);
                                _pWareHouseId = _cWareHouseId;// = Int32.Parse(parts[0]);
                                WareHouseCount++;
                            }
                            _fixedCostWtoS[WareHouseCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }

                //to read and store varible fixed cost from vendor to warehouse
                parameter = sr.ReadLine();
                if (parameter.Equals("Variable_FixedCostVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _varFixedCostBasicVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _varFixedCostBasicVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }

                            _varFixedCostBasicVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }

                //to read and store varible cost from vendor to warehouse
                parameter = sr.ReadLine();
                if (parameter.Equals("VariableCostVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _variableCostBasicVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _variableCostBasicVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }
                            _variableCostBasicVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }

                //to read and store varible fixed cost from warehouse to stores
                parameter = sr.ReadLine();
                if (parameter.Equals("Variable_FixedCostWarehouseToStore:["))
                {
                    int i = 0;
                    int _pWareHouseId = -1;
                    int _cWareHouseId = 0;
                    int WareHouseCount = -1;


                    _varFixedCostWtoS = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cWareHouseId = Int32.Parse(parts[0]);
                            if (_pWareHouseId != _cWareHouseId)
                            {
                                var l = new List<float>();
                                _varFixedCostWtoS.Add(l);
                                _pWareHouseId = _cWareHouseId;// = Int32.Parse(parts[0]);
                                WareHouseCount++;
                            }

                            _varFixedCostWtoS[WareHouseCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }

                //to read and store varible cost from warehouse to store
                parameter = sr.ReadLine();
                if (parameter.Equals("VariableCostWarehouseToStore:["))
                {
                    int i = 0;
                    int _pWareHouseId = -1;
                    int _cWareHouseId = 0;
                    int WareHouseCount = -1;


                    _variableCostWtoS = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cWareHouseId = Int32.Parse(parts[0]);
                            if (_pWareHouseId != _cWareHouseId)
                            {
                                var l = new List<float>();
                                _variableCostWtoS.Add(l);
                                _pWareHouseId = _cWareHouseId;// = Int32.Parse(parts[0]);
                                WareHouseCount++;
                            }

                            _variableCostWtoS[WareHouseCount].Add(float.Parse(parts[2]));
                            i++;
                        }
                    }
                }

                //to read and store product volume
                parameter = sr.ReadLine();
                if (parameter.Equals("ProductVolume:["))
                {
                    int i = 0;
                    _productVol = new List<float>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _productVol.Add(Int32.Parse(parts[0]));
                            _productVol.Add(float.Parse(parts[1]));
                            i++;
                        }
                    }
                }


                parameter = sr.ReadLine();
                if (parameter.Equals("ProductWeight:["))
                {
                    int i = 0;
                    _weightBasic = new List<float>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _weightBasic.Add(Int32.Parse(parts[0]));
                            _weightBasic.Add(float.Parse(parts[1]));
                            i++;
                        }
                    }
                }

                parameter = sr.ReadLine();
                if (parameter.Equals("MapVendorToProduct:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;
                    _VPbasic = new List<List<int>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<int>();
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                _VPbasic.Add(l);
                                VendorCount++;
                            }
                            _VPbasic[VendorCount].Add(int.Parse(parts[2]));
                            i++;
                        }
                    }
                }

                //to read and store lead time to each store 
                parameter = sr.ReadLine();
                if (parameter.Equals("LeadTimeWarehouseToStores:["))
                {
                    int i = 0;
                    int _pWareHouseId = -1;
                    int _cWareHouseId = 0;
                    int WareHouseCount = -1;

                    _leadTime = new List<int>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cWareHouseId = Int32.Parse(parts[0]);
                            _leadTime.Add(Int32.Parse(parts[2]));
                            i++;
                        }

                    }
                }

                //to read Fashion store demand of each store for each product 
                parameter = sr.ReadLine();
                if (parameter.Equals("StoreDemandForFashionProducts:["))
                {
                    _demandFashion = new List<List<int>>();
                    int _pStoreId = -1;
                    int _cStoreId = 0;
                    int Storeid = -1;
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cStoreId = Int32.Parse(parts[0]);
                            if (_pStoreId != _cStoreId)
                            {
                                var l = new List<int>();
                                _pStoreId = _cStoreId;// = Int32.Parse(parts[0]);
                                _demandFashion.Add(l);
                                Storeid++;
                            }
                            _demandFashion[Storeid].Add(Int32.Parse(parts[2]));
                        }
                    }
                }


                //to read and store fixed cost from vendor to warehouse for fashion products
                parameter = sr.ReadLine();
                if (parameter.Equals("FixedCostFashionVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _fixedCostFashionVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _fixedCostFashionVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }

                            _fixedCostFashionVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }
                    }

                }


                //to read and store varible fixed cost from vendor to warehouse
                parameter = sr.ReadLine();
                if (parameter.Equals("Variable_FixedCostFashionVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _varFixedCostFashionVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _varFixedCostFashionVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }

                            _varFixedCostFashionVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }

                //to read and store varible cost from vendor to warehouse for fashion products
                parameter = sr.ReadLine();
                if (parameter.Equals("VariableCostFashionVendorToWarehouse:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;

                    _variableCostFashionVtoW = new List<List<float>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<float>();
                                _variableCostFashionVtoW.Add(l);
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                VendorCount++;
                            }
                            _variableCostFashionVtoW[VendorCount].Add(float.Parse(parts[2]));
                            i++;
                        }

                    }

                }


                //to read and store product volume for fashion products
                parameter = sr.ReadLine();
                if (parameter.Equals("FashionProductVolume:["))
                {
                    int i = 0;
                    _fashionVol = new List<float>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _productVol.Add(Int32.Parse(parts[0]));
                            _fashionVol.Add(float.Parse(parts[1]));
                            i++;
                        }
                    }
                }

                parameter = sr.ReadLine();
                if (parameter.Equals("FashionProductWeight:["))
                {
                    int i = 0;
                    _weightFashion = new List<float>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            // _weightBasic.Add(Int32.Parse(parts[0]));
                            _weightFashion.Add(float.Parse(parts[1]));
                            i++;
                        }
                    }
                }

                parameter = sr.ReadLine();
                if (parameter.Equals("MapFashionVendorToProduct:["))
                {
                    int i = 0;
                    int _pVendorId = -1;
                    int _cVendorId = 0;
                    int VendorCount = -1;
                    _VPfashion = new List<List<int>>();
                    while (!string.IsNullOrEmpty(parameter = sr.ReadLine()))
                    {
                        if (parameter.Equals("]"))
                        {
                            break;
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', '\t', ')', '\t', '[', ']', ' ' };
                            string[] parts = parameter.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            _cVendorId = Int32.Parse(parts[0]);
                            if (_pVendorId != _cVendorId)
                            {
                                var l = new List<int>();
                                _pVendorId = _cVendorId;// = Int32.Parse(parts[0]);
                                _VPfashion.Add(l);
                                VendorCount++;
                            }
                            _VPfashion[VendorCount].Add(int.Parse(parts[2]));
                            i++;
                        }
                    }
                }


            }

            _numStores = _storeVol.Count;
            _numWareHouses = _warehouseVol.Count;
            _numBvendors = _fixedCostBasicVtoW.Count;
            _numFvendors = _fixedCostFashionVtoW.Count;
            _numTimePeriods = _demand[0][0].Count;
            _numBasic = _demand[0].Count;
            if (_numFvendors == 0)
                _numFashion = 0;
            else
                _numFashion = _demandFashion[0].Count;

            return;
        }

        public void InitializeBasicInventory()
        {
            _WhBasicInv = new List<List<List<int>>>();          //[warehouse][product][time period]            
            for (int w = 0; w < _numWareHouses; w++)
            {
                List<List<int>> lb = new List<List<int>>();
                _WhBasicInv.Add(lb);
                for (int p = 0; p < _numBasic; p++) //initializing inventory for basic products
                {
                    List<int> m = new List<int>();
                    _WhBasicInv[w].Add(m);
                    for (int t = 0; t <= _numTimePeriods; t++)      //Make sure it is less than equal to ..as we consider initial inventory
                    {
                        _WhBasicInv[w][p].Add(0);           //Declare initial inventory at warehoue here...
                    }
                }
            }

            _StoreBasicInv = new List<List<List<int>>>();          //[store][product][timeperiod]
            for (int s = 0; s < _numStores; s++)
            {
                List<List<int>> l = new List<List<int>>();
                _StoreBasicInv.Add(l);
                for (int p = 0; p < _numBasic; p++)
                {
                    List<int> m = new List<int>();
                    _StoreBasicInv[s].Add(m);
                    for (int t = 0; t <= _numTimePeriods; t++)      //Make sure it is less than equal to..as we are considering initial inventory here
                    {
                        _StoreBasicInv[s][p].Add(0);               //Declare initial store inventory here..
                    }
                }
            }
        }

        public void InitializeFashionInventory()
        {
            _WhFashionInv = new List<List<List<int>>>();        //[warehouse][Fashion][time period]
            for (int w = 0; w < _numWareHouses; w++)
            {
                List<List<int>> lf = new List<List<int>>();
                _WhFashionInv.Add(lf);
                for (int f = 0; f < _numFashion; f++)   //initializing inventory for fashion products
                {
                    List<int> m = new List<int>();
                    _WhFashionInv[w].Add(m);
                    //********* t is between begin and due date to reach stores ...
                    for (int t = _beginTime; t <= _dueTime; t++)
                    {
                        _WhFashionInv[w][f].Add(0);           //Declare initial inventory at warehoue here...
                    }
                }
            }

            _StoreFashionInv = new List<List<List<int>>>();          //[store][Fashion][timeperiod]
            for (int s = 0; s < _numStores; s++)
            {
                List<List<int>> l = new List<List<int>>();
                _StoreFashionInv.Add(l);
                for (int f = 0; f < _numFashion; f++)
                {
                    List<int> m = new List<int>();
                    _StoreFashionInv[s].Add(m);
                    //***********set time-periods for the arrival window at the stores
                    for (int t = _leadTime[s]; t <= _dueTime - _beginTime - pt; t++)      //Make sure it is less than equal to..as we are considering initial inventory here
                    {
                        _StoreFashionInv[s][f].Add(0);               //Declare initial store inventory here..
                    }
                }
            }
        }

        ///////////updating inventory for basic products..cyclical pattern
        public void UpdateWarehouseInventory(List<int> _inboundSol, List<long> _outboundSol)
        {
            List<List<int>> _prodOutbound = new List<List<int>>();      //variable to calculate the total outbound qty of each product from the warehouse in a time period
            for (int t = 0; t < _numTimePeriods; t++)
            {
                List<int> sl = new List<int>();
                _prodOutbound.Add(sl);
                for (int p = 0; p < _numBasic; p++)
                {
                    int qtyOb = 0;
                    for (int s = 0; s < _numStores; s++)
                    {
                        qtyOb = qtyOb + (int)_outboundSol[t * _numBasic * _numStores + p * _numStores + s];
                    }
                    _prodOutbound[t].Add(qtyOb);
                }
            }

            for (int v = 0; v < _numBvendors; v++)
            {
                for (int p = 0; p < _numBasic; p++)
                {
                    if (_VPbasic[v][p] == 1)
                    {
                        for (int t = 0; t < _numTimePeriods; t++)
                        {
                            if (t == 0)
                            {
                                _WhBasicInv[0][p][t] = 0;
                                _WhBasicInv[0][p][t + 1] = _WhBasicInv[0][p][t] + _inboundSol[t * _numBvendors * _numBasic + v * _numBasic + p] - _prodOutbound[t][p];
                            }
                            else
                            {
                                _WhBasicInv[0][p][t + 1] = _WhBasicInv[0][p][t] + _inboundSol[t * _numBvendors * _numBasic + v * _numBasic + p] - _prodOutbound[t][p];
                            }

                            if (_WhBasicInv[0][p][t + 1] < 0)
                            {
                                for (int i = 0; i <= t + 1; i++)
                                {
                                    _WhBasicInv[0][p][i] += (-1 * _WhBasicInv[0][p][t + 1]);
                                }
                                //_WhBasicInv[0][p][0] = _WhBasicInv[0][p][0] + (-1 * _WhBasicInv[0][p][t + 1]);   //we have only one warehouse hence o in the first array
                                //_WhBasicInv[0][p][t + 1] = 0;
                            }
                        }
                    }
                }
            }
        }

        ///////////updating initial inventory for fashion..non-cyclical pattern
        public void UpdateWhFashionInventory(List<int> _inbFashionSol, List<long> _outbFashionSol)
        {
            List<List<int>> _fashionOutbound = new List<List<int>>();      //[prod][time] variable to calculate the total outbound qty of each product from the warehouse in a time period
            //*********************************** Check time-periods
            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    if (t == 0)
                    {
                        List<int> sl = new List<int>();
                        _fashionOutbound.Add(sl);
                        for (int i = 0; i <= _dueTime - _beginTime; i++)
                        {
                            _fashionOutbound[f].Add(0);
                        }
                    }
                    int qtyOb = 0;
                    for (int s = 0; s < _numStores; s++)
                    {
                        qtyOb = qtyOb + (int)_outbFashionSol[t * _numFashion * _numStores + f * _numStores + s];
                    }
                    _fashionOutbound[f][t + pt] += qtyOb;
                }
            }

            for (int v = 0; v < _numFvendors; v++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    if (_VPfashion[v][f] == 1)
                    {
                        //********************** Check the for loop for time-periods
                        for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
                        {
                            if (t == 0)
                                _WhFashionInv[0][f][t] = 0 + _inbFashionSol[t * _numFvendors * _numFashion + v * _numFashion + f] - _fashionOutbound[f][t];

                            else
                                _WhFashionInv[0][f][t] = _WhFashionInv[0][f][t - 1] + _inbFashionSol[t * _numFvendors * _numFashion + v * _numFashion + f] - _fashionOutbound[f][t];
                        }
                        for (int t = _dueTime - _beginTime - pt + 1; t <= _dueTime - _beginTime; t++)
                            _WhFashionInv[0][f][t] = _WhFashionInv[0][f][t - 1] - _fashionOutbound[f][t];
                    }
                }
            }
        }



        //updating inventory for basic products..cyclical pattern for stores
        //*******************************************************  Considering Lead time to stores

        public void UpdateStoreInventory(List<long> _outboundSol)
        {
            for (int s = 0; s < _numStores; s++)
            {
                for (int p = 0; p < _numBasic; p++)
                {
                    if (_leadTime[s] == 0)
                    {
                        for (int t = 0; t < _numTimePeriods; t++)
                        {
                            if (t == 0)
                            {
                                _StoreBasicInv[s][p][t] = 0;
                                _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[t * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];
                            }
                            else
                            {
                                _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[t * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];
                            }

                            if (_StoreBasicInv[s][p][t + 1] < 0)
                            {
                                for (int i = 0; i <= t + 1; i++)
                                {
                                    _StoreBasicInv[s][p][i] += (-1 * _StoreBasicInv[s][p][t + 1]);
                                }
                                // _StoreBasicInv[s][p][0] = _StoreBasicInv[s][p][0] + (-1 * _StoreBasicInv[s][p][t + 1]);
                                // _StoreBasicInv[s][p][t + 1] = 0;
                            }
                        }
                    }
                    else
                    {
                        //if lead time to a store is at least one day
                        for (int t = 0; t < _leadTime[s]; t++)
                        {
                            if (t == 0)
                            {
                                _StoreBasicInv[s][p][t] = 0;
                                _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[(_numTimePeriods - _leadTime[s]) * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];
                            }
                            else
                            {
                                _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[(_numTimePeriods - _leadTime[s] + t) * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];
                            }

                            if (_StoreBasicInv[s][p][t + 1] < 0)
                            {
                                for (int i = 0; i <= t + 1; i++)
                                {
                                    _StoreBasicInv[s][p][i] += (-1 * _StoreBasicInv[s][p][t + 1]);
                                }
                            }
                        }

                        for (int t = _leadTime[s]; t < _numTimePeriods; t++) // starting from lead time of store s;
                        {
                            _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[(t - _leadTime[s]) * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];

                            if (_StoreBasicInv[s][p][t + 1] < 0)
                            {
                                for (int i = 0; i <= t + 1; i++)
                                {
                                    _StoreBasicInv[s][p][i] += (-1 * _StoreBasicInv[s][p][t + 1]);
                                }
                            }
                        }
                    }
                }
            }
        }

        //updating inventory for fashion products..non-cyclical pattern for stores
        //*******************************************************  Considering Lead time to stores

        public void UpdateStoreFashionInventory(List<long> _outbFashionSol)
        {
            for (int s = 0; s < _numStores; s++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    if (_leadTime[s] == 0)  //if condition for lead time
                    {
                        //****************** Check time-periods for fashion period
                        for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
                        {
                            if (t == 0)
                                _StoreFashionInv[s][f][t] = 0 + (int)_outbFashionSol[t * _numFashion * _numStores + f * _numStores + s];
                            else
                                _StoreFashionInv[s][f][t] = _StoreFashionInv[s][f][t - 1] + (int)_outbFashionSol[t * _numFashion * _numStores + f * _numStores + s];
                        }
                    }
                    else     //if lead time to a store is at least one day                    
                    {
                        //****************** Check time-periods for fashion products...
                        //outbound in t will reach store in t+leadTime[s]. The datastructures for fashion products consider begintime as t=0
                        //the total no of elements would be t = 0 to _endTime - _beginTime at warehouse and t=0+leadTime[s] to _dueTime at stores
                        for (int t = _leadTime[s]; t < _dueTime - _beginTime - pt; t++)
                        {
                            if (t == _leadTime[s])
                                _StoreFashionInv[s][f][t - _leadTime[s]] = 0 + (int)_outbFashionSol[(t - _leadTime[s]) * _numFashion * _numStores + f * _numStores + s];
                            else
                                _StoreFashionInv[s][f][t - _leadTime[s]] = _StoreFashionInv[s][f][t - _leadTime[s] - 1] + (int)_outbFashionSol[(t - _leadTime[s]) * _numFashion * _numStores + f * _numStores + s];
                        }
                    }
                }
            }
        }

        public void CheckStoreVolume()
        {
            for (int s = 0; s < _numStores; s++)
            {
                for (int t = 0; t < _numTimePeriods; t++)
                {
                    float totalProdVol = 0;
                    for (int p = 0; p < _numBasic; p++)
                    {
                        totalProdVol += _StoreBasicInv[s][p][t + 1] * _productVol[p];        //store inventory contains initial inventory..so t+1 instead of t
                    }
                    //if (totalProdVol > _storeVol[s])
                    //    Penalty = 1000000;
                }
            }
        }

        //List<float> ListOfCosts = new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //check...*************
        List<float> ListOfCosts = new List<float> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //check...*************
        List<List<int>> TotalBasicOutbQty;     //[time period][product] - list that contains the total outbound quantity of each product shipped to all the stores from warehouse in each time perid    
        List<List<int>> TotalBasicInbQty;      //[time period][product] - list that contains the total inbound quantity of each product shipped from vendor to warehouse in each time perid 
        List<List<int>> TotalFashionOutbQty;     //[time period][product] - list that contains the total outbound quantity of each product shipped to all the stores from warehouse in each time perid    
        List<List<int>> TotalFashionInbQty;      //[time period][product] - list that contains the total inbound quantity of each product shipped from vendor to warehouse in each time perid 
        List<float> TotalInboundPutHrs;            //List to store total putaway hrs for each time period
        List<float> TotalOutboundPickHrs;          //List to store total pick hrs for each time period
        List<float> FashionPutHrs;
        List<float> FashionPickHrs;
        List<float> BasicPutHrs;
        List<float> BasicPickHrs;
        List<int> PT_Putaway;            //List to store total putaway hrs for each time period
        List<int> PT_Pickers;          //List to store total pick hrs for each time period
        List<int> _putawayBasicQty;
        List<int> _pickBasicQty;
        List<int> _putawayFashionQty;
        List<int> _pickFashionQty;

        /// <summary>
        /// Method to select technology for putaway. This method randomly selects a technology from the tech list. 
        /// </summary>
        public void PutawayTechnologySelection()
        {
            while (currentPutawayTech == prevPutawayTech)
                currentPutawayTech = rand.Next(0, _putawayRateList.Count);

            prevPutawayTech = currentPutawayTech;
        }

        public void PickTechnologySelection()
        {
            while (currentPickTech == prevPickTech)
                currentPickTech = rand.Next(0, _pickRateList.Count);

            prevPickTech = currentPickTech;
        }

        public float CalculateWarehouseInventoryCost()
        {
            float WhInvCost = 0;
            for (int t = 0; t < _numTimePeriods; t++)
            {
                for (int p = 0; p < _numBasic; p++)
                {
                    WhInvCost = WhInvCost + (_WhBasicInv[0][p][t + 1] * _whHoldingCost); //(_warehouseHoldingCost[0][p]));
                }
            }
            //ListOfCosts[0] = WhInvCost;
            ListOfCosts[3] = WhInvCost;
            return WhInvCost;
        }

        public float CalWhFashionInvCost()
        {
            float WhInvCost = 0;
            //********** check time-periods
            for (int t = 0; t <= _dueTime - _beginTime; t++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    WhInvCost = WhInvCost + (_WhFashionInv[0][f][t] * _whHoldingCost);
                }
            }
            //ListOfCosts[2] = WhInvCost;
            ListOfCosts[5] = WhInvCost;
            return WhInvCost;
        }

        public float CalStoreFashionInvCost()
        {
            float storeInvCost = 0;
            for (int s = 0; s < _numStores; s++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    for (int t = _leadTime[s]; t <= _dueTime - _beginTime - pt; t++)
                    {
                        storeInvCost = storeInvCost + (_StoreFashionInv[s][f][t - _leadTime[s]] * _stHoldingCost);
                    }
                }
            }
            //ListOfCosts[6] = storeInvCost;
            ListOfCosts[9] = storeInvCost;

            return storeInvCost;
        }


        public float CalculateInboundCost(List<int> _inboundSol, int Whouse)
        {
            float InboundCost = 0;
            float WhInvCost = 0;
            float InboundTransCost = 0;
            _numBasicShipmentsVtoW = new List<List<float>>();                    //[Time][Vendor] list to contain 
            TotalBasicInbQty = new List<List<int>>();                        //[Time][Product] list to contain total inbound quantity of each product to the warehouse summed accross all the vendors in a time period
            _putawayBasicQty = new List<int>();

            for (int t = 0; t < _numTimePeriods; t++)           // For all time-periods       
            {
                List<float> TotalInboundWeight = new List<float>();        //list that contains total inbound qty (in lbs) summed accross all products from each vendor - to calculate transportation cost
                List<int> slInboundQty = new List<int>();                  // sublist to contain total inbound quantity of each product to the warehouse summed accross all the vendors in a time period
                TotalBasicInbQty.Add(slInboundQty);

                for (int v = 0; v < _numBvendors; v++)   //VENDORS
                {
                    int _totInboundQty = 0;                      //to store the total inbound Quantity  from the vendor
                    float totalWtShipped = 0;
                    for (int p = 0; p < _numBasic; p++)       // For all products
                    {
                        if (_VPbasic[v][p] == 1)
                        {
                            int temp = _inboundSol[t * _numBvendors * _numBasic + v * _numBasic + p];
                            _totInboundQty = _totInboundQty + temp;
                            totalWtShipped = totalWtShipped + (temp * _weightBasic[p]);
                            TotalBasicInbQty[t].Add(temp);
                        }
                    }
                    TotalInboundWeight.Add(totalWtShipped);
                }

                List<float> sl1 = new List<float>();
                _numBasicShipmentsVtoW.Add(sl1);
                float fixedCostVtoW = 0;
                float varCostVtoW = 0;
                for (int i = 0; i < TotalInboundWeight.Count; i++)
                {
                    float noShipments = ((float)TotalInboundWeight[i]) / (float)TRUCKCAP;
                    int numShipments = (int)Math.Ceiling(noShipments);
                    fixedCostVtoW = fixedCostVtoW + (numShipments * (_fixedCostBasicVtoW[i][Whouse] + _varFixedCostBasicVtoW[i][Whouse]));
                    varCostVtoW = varCostVtoW + (TotalInboundWeight[i] * _variableCostBasicVtoW[i][Whouse]);
                    _numBasicShipmentsVtoW[t].Add(noShipments);
                }
                InboundTransCost = InboundTransCost + fixedCostVtoW + varCostVtoW;

                int putawayQty = 0;
                for (int p = 0; p < _numBasic; p++)  //number of Products
                {
                    putawayQty = putawayQty + TotalBasicInbQty[t][p];
                    WhInvCost = WhInvCost + (_WhBasicInv[Whouse][p][t + 1] * _whHoldingCost);   //(_warehouseHoldingCost[Whouse][p]));    //////****Wh holdingcost check
                }
                _putawayBasicQty.Add(putawayQty);
            }

            // InboundTransCost = InboundTransCost * 0.8F; //////*************Check thhis **********
            InboundCost = InboundTransCost + WhInvCost;

            ListOfCosts[3] = WhInvCost;
            //ListOfCosts[1] = InboundTransCost;
            ListOfCosts[4] = InboundTransCost;

            //Penalty = 0;
            return InboundCost;
        }
        /// <summary>
        /// Method to calculate inbound transportation cost for fashion products
        /// </summary>
        /// <param name="_inbFashionSol"></param>
        /// <param name="Whouse"></param>
        /// <returns></returns>
        List<int> FashionInbTimeperiod;
        public float CalFashionInbCost(List<int> _inbFashionSol, int Whouse)
        {
            float InboundCost = 0;
            //float WhInvCost = 0;
            float InboundTransCost = 0;
            _numFashionShipmentsVtoW = new List<List<float>>();                    //[Time][Vendor] list to contain 
            TotalFashionInbQty = new List<List<int>>();                        //[Time][Product] list to contain total inbound quantity of each product to the warehouse summed accross all the vendors in a time period
            _putawayFashionQty = new List<int>();
            FashionInbTimeperiod = new List<int>();                     //List to store arrival time of fashion products from vendor to use it to schedule feasible outbound
            //**********Check time-periods
            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)           // For all time-periods       
            {
                List<float> TotalInboundWeight = new List<float>();        //list that contains total inbound qty (in lbs) summed accross all products from each vendor - to calculate transportation cost
                List<int> slInboundQty = new List<int>();                  // sublist to contain total inbound quantity of each product to the warehouse summed accross all the vendors in a time period
                TotalFashionInbQty.Add(slInboundQty);
                int putawayQty = 0;
                for (int v = 0; v < _numFvendors; v++)             //VENDORS
                {
                    int _totInboundQty = 0;                      //to store the total inbound Quantity  from the vendor
                    float totalWtShipped = 0;
                    for (int f = 0; f < _numFashion; f++)       // For all products
                    {
                        if (_VPfashion[v][f] == 1)
                        {
                            if (t == 0)
                                FashionInbTimeperiod.Add(0);
                            int temp = _inbFashionSol[t * _numFvendors * _numFashion + v * _numFashion + f];
                            if (temp > 0)
                                FashionInbTimeperiod[f] = t;    //storing time period of inbound fashion product for outbound feasibility check
                            _totInboundQty = _totInboundQty + temp;
                            totalWtShipped = totalWtShipped + (temp * _weightFashion[f]);
                            TotalFashionInbQty[t].Add(temp);
                            putawayQty = putawayQty + TotalFashionInbQty[t][f];
                            // WhInvCost = WhInvCost + (_WhFashionInv[Whouse][f][t] * _whHoldingCost); //(_warehouseHoldingCost[Whouse][p]));    //////****Wh holdingcost check
                        }
                    }
                    TotalInboundWeight.Add(totalWtShipped);
                }

                List<float> sl1 = new List<float>();
                _numFashionShipmentsVtoW.Add(sl1);
                float fixedCostVtoW = 0;
                float varCostVtoW = 0;
                for (int i = 0; i < TotalInboundWeight.Count; i++)
                {
                    float noShipments = ((float)TotalInboundWeight[i]) / (float)TRUCKCAP;
                    int numShipments = (int)Math.Ceiling(noShipments);
                    fixedCostVtoW = fixedCostVtoW + (numShipments * (_fixedCostFashionVtoW[i][Whouse] + _varFixedCostFashionVtoW[i][Whouse]));
                    varCostVtoW = varCostVtoW + (TotalInboundWeight[i] * _variableCostFashionVtoW[i][Whouse]);
                    _numFashionShipmentsVtoW[t].Add(noShipments);
                }
                InboundTransCost = InboundTransCost + fixedCostVtoW + varCostVtoW;

                //int putawayQty = 0;
                //for (int f = 0; f < _numFashion; f++)  //number of Products
                //{
                //    putawayQty = putawayQty + TotalFashionInbQty[t][f];
                //    WhInvCost = WhInvCost + (_WhFashionInv[Whouse][f][t] * _whHoldingCost); //(_warehouseHoldingCost[Whouse][p]));    //////****Wh holdingcost check
                //}
                _putawayFashionQty.Add(putawayQty);
            }

            // InboundTransCost = InboundTransCost * 0.8F; //////*************Check thhis **********
            InboundCost = InboundTransCost; // +WhInvCost;

            // ListOfCosts[2] = WhInvCost;
            //ListOfCosts[3] = InboundTransCost;
            ListOfCosts[6] = InboundTransCost;

            //Penalty = 0;
            return InboundCost;
        }


        public float CalculatePutawayCost()
        {
            //bool flag = false;
            float PutawayCost = 0;
            _numPutaway = new List<int>();  //number of workers required for putaway which includes both basic and fashion products
            PT_Putaway = new List<int>();
            TotalInboundPutHrs = new List<float>();
            FashionPutHrs = new List<float>();
            BasicPutHrs = new List<float>();
            int NumPutaway = 0;
            PUTAWAYRATE = _putawayRateList[currentPutawayTech];
            PutawayTechCost = _putawayTechCost[currentPutawayTech] * _numTimePeriods;
            float MHECost = _putMHECost[currentPutawayTech];
            float TotalMHECost = 0;
            float TotalPutawayCost = 0;
            // Check time-periods for basic and fashion products
            for (int t = 0; t < _numTimePeriods; t++)
            {
                FashionPutHrs.Add(0);
                BasicPutHrs.Add(0);
                if (t >= _beginTime && t <= _dueTime - pt)
                {
                    NumPutaway = (int)Math.Ceiling((double)(_putawayBasicQty[t] + _putawayFashionQty[t - _beginTime]) / (double)(PUTAWAYRATE * 8));
                    _numPutaway.Add(NumPutaway);
                    FashionPutHrs[t] = (float)((float)_putawayFashionQty[t - _beginTime] / (float)PUTAWAYRATE);
                    BasicPutHrs[t] = (float)((float)_putawayBasicQty[t] / (float)PUTAWAYRATE);
                    TotalInboundPutHrs.Add((float)((float)(_putawayBasicQty[t] + _putawayFashionQty[t - _beginTime]) / (float)PUTAWAYRATE));
                }
                else
                {
                    NumPutaway = (int)Math.Ceiling((double)_putawayBasicQty[t] / (double)(PUTAWAYRATE * 8));
                    _numPutaway.Add(NumPutaway);
                    BasicPutHrs[t] = (float)((float)_putawayBasicQty[t] / (float)PUTAWAYRATE);
                    TotalInboundPutHrs.Add((float)((float)_putawayBasicQty[t] / (float)PUTAWAYRATE));
                }
            }
            int Max = _numPutaway.Max();
            int alpha = (int)(Math.Ceiling((Max / (1 + (gamma * phi)))));      //gives required number of full time workers
            //PutawayCost = (alpha * (FT_PUTCOST + MHECost) * _numTimePeriods);         //calculates full time cost along with MHE cost
            PutawayCost = (alpha * FT_PUTCOST * _numTimePeriods);
            TotalMHECost = (alpha * MHECost * _numTimePeriods);
            for (int j = 0; j < _numTimePeriods; j++)
            {
                //PutawayCost += Math.Max(0, _numPutaway[j] - alpha) * (PT_PUTCOST + MHECost);   //calculates part time cost
                PutawayCost += Math.Max(0, _numPutaway[j] - alpha) * (PT_PUTCOST);
                TotalMHECost += Math.Max(0, _numPutaway[j] - alpha) * (MHECost);
                PT_Putaway.Add(Math.Max(0, _numPutaway[j] - alpha));
            }

            //ListOfCosts[4] = PutawayCost + PutawayTechCost;
            ListOfCosts[0] = TotalMHECost;
            ListOfCosts[1] = PutawayTechCost;
            ListOfCosts[7] = PutawayCost;
            TotalPutawayCost = TotalMHECost + PutawayTechCost + PutawayTechCost;

            return TotalPutawayCost;
        }

        /// <summary>
        /// Method to calculate outbound weight for basic products and also updates store inventory cost in the list of costs at index [5]
        /// </summary>
        List<List<float>> TotalBasicOutbWeight;
        public void CalBasicOutbWeight(List<long> _outboundSol, int Whouse)
        {
            float SInvCost = 0;
            _pickBasicQty = new List<int>();
            TotalBasicOutbQty = new List<List<int>>();
            TotalBasicOutbWeight = new List<List<float>>();

            for (int t = 0; t < _numTimePeriods; t++)           // For all time-periods       
            {
                int pickQty = 0;
                List<int> sl1 = new List<int>();                 // sublist to contain total outbound quantity of each product from warehouse to all the stores in a time period
                TotalBasicOutbQty.Add(sl1);
                List<float> sl2 = new List<float>();
                TotalBasicOutbWeight.Add(sl2);
                List<float> _StoreBasicInvCost = new List<float>();

                for (int p = 0; p < _numBasic; p++)       // Basic Products
                {
                    float _Weight = _weightBasic[p];
                    int _prodOutboundQty = 0;
                    for (int s = 0; s < _numStores; s++)   //Stores
                    {
                        _prodOutboundQty = _prodOutboundQty + (int)_outboundSol[t * _numBasic * _numStores + p * _numStores + s];
                        // _StoreBasicInv[s][p][t + 1] = _StoreBasicInv[s][p][t] + (int)_outboundSol[t * _numBasic * _numStores + p * _numStores + s] - _demand[s][p][t];
                        if (p == 0)
                        {
                            _StoreBasicInvCost.Add((_StoreBasicInv[s][p][t + 1]) * _stHoldingCost);    //_storeHoldingCost[s][p]); ////Store holding cost
                            TotalBasicOutbWeight[t].Add(_outboundSol[t * _numBasic * _numStores + p * _numStores + s] * _Weight);
                        }
                        else
                        {
                            _StoreBasicInvCost[s] = _StoreBasicInvCost[s] + (_StoreBasicInv[s][p][t + 1]) * _stHoldingCost;  //_storeHoldingCost[s][p];
                            TotalBasicOutbWeight[t][s] = TotalBasicOutbWeight[t][s] + (_Weight * _outboundSol[t * _numBasic * _numStores + p * _numStores + s]);
                        }
                    }
                    TotalBasicOutbQty[t].Add(_prodOutboundQty);
                    pickQty = pickQty + TotalBasicOutbQty[t][p];     //Calculating pick quantity to calculate number of required pickers in each time period
                }   //End of loop for products
                _pickBasicQty.Add(pickQty);

                SInvCost = SInvCost + _StoreBasicInvCost.Sum();
            }   //End of loop for all time periods

            //OutboundTransCost = OutboundTransCost * 1 / 2;  ////**************Transportation cost ----check

            //ListOfCosts[5] = SInvCost;
            ListOfCosts[8] = SInvCost;

            //Penalty = 0;
        }

        /// <summary>
        /// Method to calculate outbound transportation cost for both basic and fashion products
        /// </summary>
        /// <returns></returns>
        public float CalculateOutboundCost()
        {
            float outboundCost = 0;
            float OutboundTransCost = 0;
            _numShipmentsWtoS = new List<List<float>>();
            TotalBasicOutbQty = new List<List<int>>();
            //Check time-periods for both basic and fashion products
            for (int t = 0; t < _numTimePeriods; t++)           // For all time-periods       
            {
                if (t >= _beginTime + pt && t <= _dueTime)
                {
                    float fixedCostWtoS = 0;
                    float varCostWtoS = 0;
                    List<float> slShipmentsWtoS = new List<float>();
                    _numShipmentsWtoS.Add(slShipmentsWtoS);
                    for (int s = 0; s < _numStores; s++)
                    {
                        float noShipments = (float)((float)(TotalBasicOutbWeight[t][s] + TotalFashionOutbWeight[t - _beginTime - pt][s]) / (float)TRUCKCAP);
                        int numShipments = (int)Math.Ceiling(noShipments);
                        fixedCostWtoS = fixedCostWtoS + (numShipments * (_fixedCostWtoS[0][s] + _varFixedCostWtoS[0][s]));
                        varCostWtoS = varCostWtoS + ((TotalBasicOutbWeight[t][s] + TotalFashionOutbWeight[t - _beginTime - pt][s]) * _variableCostWtoS[0][s]);
                        _numShipmentsWtoS[t].Add(noShipments);
                    }
                    OutboundTransCost = OutboundTransCost + fixedCostWtoS + varCostWtoS;
                }

                else
                {
                    float fixedCostWtoS = 0;
                    float varCostWtoS = 0;
                    List<float> slShipmentsWtoS = new List<float>();
                    _numShipmentsWtoS.Add(slShipmentsWtoS);
                    for (int s = 0; s < _numStores; s++)
                    {
                        float noShipments = (float)((float)TotalBasicOutbWeight[t][s] / (float)TRUCKCAP);
                        int numShipments = (int)Math.Ceiling(noShipments);
                        fixedCostWtoS = fixedCostWtoS + (numShipments * (_fixedCostWtoS[0][s] + _varFixedCostWtoS[0][s]));
                        varCostWtoS = varCostWtoS + (TotalBasicOutbWeight[t][s] * _variableCostWtoS[0][s]);
                        _numShipmentsWtoS[t].Add(noShipments);
                    }
                    OutboundTransCost = OutboundTransCost + fixedCostWtoS + varCostWtoS;
                }
            }   //End of loop for all time periods

            //OutboundTransCost = OutboundTransCost * 1 / 2;  ////**************Transportation cost ----check               
            //ListOfCosts[7] = OutboundTransCost;
            ListOfCosts[10] = OutboundTransCost;

            //Penalty = 0;

            return outboundCost;
        }

        List<List<int>> FashionOBStoreTimePeriod;      //[product][store] a list to use for feasibility check
        List<List<float>> TotalFashionOutbWeight;   //[time][store] outbound weight of fashion products to each store in each period
        /// <summary>
        /// Method to calculate the outbound weight of fashion products in each time period and pick quantity which are used
        /// to calculate outbound transportation cost and picking costs
        /// </summary>
        /// <param name="_outbFashionSol"></param>
        /// <param name="Whouse"></param>
        public void CalFashionOutbWeight(List<long> _outbFashionSol, int Whouse)
        {
            //float SInvCost = 0;
            _pickFashionQty = new List<int>();              //total qty of fashion products to be picked at warehouse in each time-period
            TotalFashionOutbQty = new List<List<int>>();    //outbound qty of fashion products to each store in each time-period
            TotalFashionOutbWeight = new List<List<float>>();
            FashionOBStoreTimePeriod = new List<List<int>>();
            //******* Check time-periods..should be between begin and end periods of fashion event
            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
            {
                int pickQty = 0;
                List<int> sl = new List<int>();                 // sublist to contain total outbound quantity of each product from warehouse to all the stores in a time period
                TotalFashionOutbQty.Add(sl);
                List<float> sl2 = new List<float>();
                TotalFashionOutbWeight.Add(sl2);

                for (int f = 0; f < _numFashion; f++)       // PRODUCT
                {
                    if (t == 0)     //create prodcut list only in first time period and then update the values in every time period..other wise it will create a new sublist in each time period
                    {
                        List<int> sl3 = new List<int>();
                        FashionOBStoreTimePeriod.Add(sl3);
                    }
                    float _Weight = _weightFashion[f];
                    int _prodOutboundQty = 0;
                    for (int s = 0; s < _numStores; s++)   //Stores
                    {
                        if (t == 0)
                            FashionOBStoreTimePeriod[f].Add(0);

                        int tempQty = (int)_outbFashionSol[t * _numFashion * _numStores + f * _numStores + s];
                        _prodOutboundQty = _prodOutboundQty + tempQty;
                        if (tempQty > 0)
                        {
                            FashionOBStoreTimePeriod[f][s] = t;     //the time period in whihc the fashion product f is shipped to store s..is used for feasibility check
                        }
                        // Check this loop   **********************************
                        if (f == 0)
                        {
                            TotalFashionOutbWeight[t].Add(_outbFashionSol[t * _numFashion * _numStores + f * _numStores + s] * _Weight);
                        }
                        else
                        {
                            TotalFashionOutbWeight[t][s] = TotalFashionOutbWeight[t][s] + (_Weight * _outbFashionSol[t * _numFashion * _numStores + f * _numStores + s]);
                        }
                    }
                    TotalFashionOutbQty[t].Add(_prodOutboundQty);
                    pickQty = pickQty + TotalFashionOutbQty[t][f];     //Calculating pick quantity to calculate number of required pickers in each time period
                }   //End of loop for products
                _pickFashionQty.Add(pickQty);

                //SInvCost = SInvCost + _StoreFashionInvCost.Sum();
            }   //End of loop for all time periods for fashion

            //OutboundTransCost = OutboundTransCost * 1 / 2;  ////**************Transportation cost ----check
            // outboundCost = OutboundTransCost + SInvCost;

            //ListOfCosts[6] = SInvCost;
            //  ListOfCosts[6] = OutboundTransCost;

            //Penalty = 0;
        }

        public float CalculatePickingCost()
        {
            float PickCost = 0;
            _numPickers = new List<int>();
            PT_Pickers = new List<int>();
            TotalOutboundPickHrs = new List<float>();
            FashionPickHrs = new List<float>();
            BasicPickHrs = new List<float>();
            int NumPickers = 0;
            PICKRATE = _pickRateList[currentPickTech];
            PickTechCost = _pickTechCost[currentPickTech] * _numTimePeriods;
            float TotalPickingCost = 0;
            for (int t = 0; t < _numTimePeriods; t++)
            {
                FashionPickHrs.Add(0);
                BasicPickHrs.Add(0);
                if (t >= _beginTime + pt && t <= _dueTime)
                {
                    NumPickers = (int)Math.Ceiling((double)(_pickBasicQty[t] + _pickFashionQty[t - _beginTime - pt]) / (double)(PICKRATE * 8));
                    _numPickers.Add(NumPickers);
                    FashionPickHrs[t] = (float)((float)(_pickFashionQty[t - _beginTime - pt]) / (float)PICKRATE);
                    BasicPickHrs[t] = (float)((float)_pickBasicQty[t] / (float)PICKRATE);
                    TotalOutboundPickHrs.Add((float)((float)(_pickBasicQty[t] + _pickFashionQty[t - _beginTime - pt]) / (float)PICKRATE));
                }
                else
                {
                    NumPickers = (int)Math.Ceiling((double)_pickBasicQty[t] / (double)(PICKRATE * 8));
                    _numPickers.Add(NumPickers);
                    BasicPickHrs[t] = (float)((float)_pickBasicQty[t] / (float)PICKRATE);
                    TotalOutboundPickHrs.Add((float)((float)_pickBasicQty[t] / (float)PICKRATE));
                }
            }
            int Max = _numPickers.Max();
            int alpha = (int)(Math.Ceiling((Max / (1 + (gamma * phi)))));      //gives required number of full time workers            
            PickCost = alpha * FT_PICKCOST * _numTimePeriods;                 //calculates full time cost

            for (int j = 0; j < _numTimePeriods; j++)
            {
                PickCost += Math.Max(0, _numPickers[j] - alpha) * PT_PICKCOST;   //calculates part time cost
                PT_Pickers.Add(Math.Max(0, _numPickers[j] - alpha));
            }

            //ListOfCosts[8] = PickCost + PickTechCost;
            ListOfCosts[2] = PickTechCost;
            ListOfCosts[11] = PickCost;
            TotalPickingCost = PickTechCost + PickCost;

            return TotalPickingCost;
        }

        List<List<int>> _totalStoreDemand;          //[store][product]
        List<List<int>> _totalWarehouseDemand;      //[Product][Time]

        static int increment_value = 999;
        static Random rand;

        public void GenerateRandomInitialSol()
        {
            InitializeBasicInventory();
            // first calculate total demand for each store for each product
            _totalStoreDemand = new List<List<int>>();              //[store][product]
            for (int s = 0; s < _numStores; s++)
            {
                List<int> _prodTotal = new List<int>();
                for (int p = 0; p < _numBasic; p++)
                {
                    _prodTotal.Add(_demand[s][p].Sum());
                    //twTest.WriteLine(s + "\t" + p + "\t" + _prodTotal[p]);
                }
                _totalStoreDemand.Add(_prodTotal);
            }

            //twTest.WriteLine("TotalWarehouseDemand (P,T)");
            //Calculate total demand at the warehouse (considering all warehouses as one) for each product in each time-period
            _totalWarehouseDemand = new List<List<int>>();                  //[product][Timeperiod]
            for (int p = 0; p < _numBasic; p++)
            {
                List<int> _prodTotal = new List<int>();
                for (int t = 0; t < _numTimePeriods; t++)
                {
                    int tempTotal = 0;
                    for (int s = 0; s < _numStores; s++)
                    {
                        tempTotal = tempTotal + _demand[s][p][t];
                    }
                    _prodTotal.Add(tempTotal);
                    //twTest.WriteLine(p + "\t" + t + "\t" + _prodTotal[t]);
                }
                _totalWarehouseDemand.Add(_prodTotal);
            }


            /////// Block of code to generate inbound any random number which may or may not meet demand in each time period
            List<List<int>> ProductInboundQty = new List<List<int>>();   // [Product][TimePeriod] total quantity shipped from vendors to warehouses for each product in each timeperiod
            for (int p = 0; p < _numBasic; p++)
            {
                int totQtyShipped = 0;
                List<int> l = new List<int>();
                ProductInboundQty.Add(l);
                //This for-loop is to determine the quantity of each product to be delivered in each time period from vendors to warehouses                  
                for (int t = 0; t < _numTimePeriods - 1; t++)             //randomly generating product quantities for time periods (one less than total time periods)          
                {
                    int temp = rand.Next(0, _totalWarehouseDemand[p].Sum() - totQtyShipped + 1);
                    totQtyShipped = totQtyShipped + temp;
                    ProductInboundQty[p].Add(temp);
                }
                ProductInboundQty[p].Add(_totalWarehouseDemand[p].Sum() - totQtyShipped);     //product quantity to be shipped in last time period                                    
            }

            //Loops to determine outbound 
            List<List<List<int>>> ProductOutboundQty = new List<List<List<int>>>();     //[time period][product][store]
            for (int t = 0; t < _numTimePeriods; t++)
            {
                List<List<int>> sl1 = new List<List<int>>();
                ProductOutboundQty.Add(sl1);
                for (int p = 0; p < _numBasic; p++)
                {
                    List<int> sl2 = new List<int>();
                    ProductOutboundQty[t].Add(sl2);
                    int QtyInWH = _WhBasicInv[0][p][t] + ProductInboundQty[p][t];
                    for (int s = 0; s < _numStores; s++)
                    {
                        if (t < _numTimePeriods - 1)
                        {
                            int temp = Math.Min(QtyInWH, _totalStoreDemand[s][p]);
                            int temp1 = rand.Next(0, temp);
                            ProductOutboundQty[t][p].Add(temp1);
                            _totalStoreDemand[s][p] = _totalStoreDemand[s][p] - temp1;
                            QtyInWH = QtyInWH - temp1;
                        }
                        else
                        {
                            if (QtyInWH >= _totalStoreDemand[s][p])
                            {
                                int temp = _totalStoreDemand[s][p];
                                ProductOutboundQty[t][p].Add(temp);
                                QtyInWH = QtyInWH - temp;
                            }
                            else
                                Console.WriteLine("Insufficient quantity in warehouse to supply to store");
                        }
                    }
                    _WhBasicInv[0][p][t + 1] = Math.Max(0, QtyInWH);
                    if (t == _numTimePeriods - 1)
                        _WhBasicInv[0][p][0] += _WhBasicInv[0][p][t + 1];
                }
            }

            //Generating inbound and outbound solutions
            float inboundCost = 0;
            float putawayCost = 0;
            float outboundCost = 0;
            float pickingCost = 0;
            _InbBasicSol = new List<int>();
            _OutbBasicSol = new List<long>();

            for (int t = 0; t < _numTimePeriods; t++)
            {
                for (int v = 0; v < _numBvendors; v++)
                {
                    for (int p = 0; p < _numBasic; p++)
                    {
                        int temp = (int)(_VPbasic[v][p] * ProductInboundQty[p][t]);
                        _InbBasicSol.Add(temp);
                    }
                }

                for (int p = 0; p < _numBasic; p++)
                {
                    for (int s = 0; s < _numStores; s++)
                    {
                        int temp = ProductOutboundQty[t][p][s];
                        _OutbBasicSol.Add(temp);
                    }
                }
            }
            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            UpdateStoreInventory(_OutbBasicSol);

            for (int i = 0; i < _numWareHouses; i++)
            {
                inboundCost = CalculateInboundCost(_InbBasicSol, i);
                CalBasicOutbWeight(_OutbBasicSol, i);
                // putawayCost = CalculatePutawayCost();
                //  outboundCost = CalculateOutboundCost(_OutbBasicSol, i);
                // pickingCost = CalculatePickingCost();
            }

        }

        public void GenerateDemandQtyAsInitialSol()
        {
            InitializeBasicInventory();
            // first calculate total demand for each store for each product
            _totalStoreDemand = new List<List<int>>();              //[store][product]
            for (int s = 0; s < _numStores; s++)
            {
                List<int> _prodTotal = new List<int>();
                for (int p = 0; p < _numBasic; p++)
                {
                    _prodTotal.Add(_demand[s][p].Sum());
                    //twTest.WriteLine(s + "\t" + p + "\t" + _prodTotal[p]);
                }
                _totalStoreDemand.Add(_prodTotal);
            }

            //twTest.WriteLine("TotalWarehouseDemand (P,T)");
            //Calculate total demand at the warehouse (considering all warehouses as one) for each product in each time-period
            _totalWarehouseDemand = new List<List<int>>();                  //[product][Timeperiod]
            for (int p = 0; p < _numBasic; p++)
            {
                List<int> _prodTotal = new List<int>();
                for (int t = 0; t < _numTimePeriods; t++)
                {
                    int tempTotal = 0;
                    for (int s = 0; s < _numStores; s++)
                    {
                        tempTotal = tempTotal + _demand[s][p][t];
                    }
                    _prodTotal.Add(tempTotal);
                    //twTest.WriteLine(p + "\t" + t + "\t" + _prodTotal[t]);
                }
                _totalWarehouseDemand.Add(_prodTotal);
            }


            /////// Block of code to generate inbound any random number which may or may not meet demand in each time period
            List<List<int>> ProductInboundQty = new List<List<int>>();   // [Product][TimePeriod] total quantity shipped from vendors to warehouses for each product in each timeperiod
            for (int p = 0; p < _numBasic; p++)
            {               
                List<int> l = new List<int>();
                ProductInboundQty.Add(l);
                //This for-loop is to determine the quantity of each product to be delivered in each time period from vendors to warehouses                  
                for (int t = 0; t < _numTimePeriods; t++)                      
                {
                    int temp = _totalWarehouseDemand[p][t];                    
                    ProductInboundQty[p].Add(temp);
                }                                                
            }

            //Loops to determine outbound 
            List<List<List<int>>> ProductOutboundQty = new List<List<List<int>>>();     //[time period][product][store]
            for (int t = 0; t < _numTimePeriods; t++)
            {
                List<List<int>> sl1 = new List<List<int>>();
                ProductOutboundQty.Add(sl1);
                for (int p = 0; p < _numBasic; p++)
                {
                    List<int> sl2 = new List<int>();
                    ProductOutboundQty[t].Add(sl2);
                    //int QtyInWH = _WhBasicInv[0][p][t] + ProductInboundQty[p][t];
                    for (int s = 0; s < _numStores; s++)
                    {                                               
                        int temp1 = _demand[s][p][t];
                        ProductOutboundQty[t][p].Add(temp1);                        
                    }                    
                }
            }

            //Generating inbound and outbound solutions
            float inboundCost = 0;
            float putawayCost = 0;
            float outboundCost = 0;
            float pickingCost = 0;
            _InbBasicSol = new List<int>();
            _OutbBasicSol = new List<long>();

            for (int t = 0; t < _numTimePeriods; t++)
            {
                for (int v = 0; v < _numBvendors; v++)
                {
                    for (int p = 0; p < _numBasic; p++)
                    {
                        int temp = (int)(_VPbasic[v][p] * ProductInboundQty[p][t]);
                        _InbBasicSol.Add(temp);
                    }
                }

                for (int p = 0; p < _numBasic; p++)
                {
                    for (int s = 0; s < _numStores; s++)
                    {
                        int temp = ProductOutboundQty[t][p][s];
                        _OutbBasicSol.Add(temp);
                    }
                }
            }
            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            UpdateStoreInventory(_OutbBasicSol);

            for (int i = 0; i < _numWareHouses; i++)
            {
                inboundCost = CalculateInboundCost(_InbBasicSol, i);
                CalBasicOutbWeight(_OutbBasicSol, i);
                // putawayCost = CalculatePutawayCost();
                //  outboundCost = CalculateOutboundCost(_OutbBasicSol, i);
                // pickingCost = CalculatePickingCost();
            }

        }

        List<int> _totalWhFashionDemand;
        public void GenRandInitFashionSol()
        {
            InitializeFashionInventory();

            //Calculate total demand at the warehouse (considering all warehouses as one) for each product
            _totalWhFashionDemand = new List<int>();                  //[product]
            for (int f = 0; f < _numFashion; f++)
            {
                int tempTotal = 0;
                for (int s = 0; s < _numStores; s++)
                {
                    tempTotal = tempTotal + _demandFashion[s][f];
                }

                _totalWhFashionDemand.Add(tempTotal);
            }

            /////// Block of code to generate inbound meeting demand in the arrival time window
            List<List<int>> ProductInboundQty = new List<List<int>>();   // [Product][TimePeriod] total quantity shipped from vendors to warehouses for each product in each timeperiod
            for (int f = 0; f < _numFashion; f++)
            {
                List<int> l = new List<int>();
                ProductInboundQty.Add(l);
                for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
                    ProductInboundQty[f].Add(0);
            }
            //different from teh way I generate random fashion solution of policy 1 and 2...*** check later***********************************
            for (int v = 0; v < _numFvendors; v++)
            {
                int temp = rand.Next(0, _endTime - _beginTime + 1);
                for (int f = 0; f < _numFashion; f++)
                {
                    if (_VPfashion[v][f] == 1)
                    {
                        ProductInboundQty[f][temp] = _totalWhFashionDemand[f];
                    }
                }
            }

            //Loops to determine outbound 
            List<List<List<int>>> ProductOutboundQty = new List<List<List<int>>>();     //[time period][product][store]
            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
            {
                List<List<int>> sl1 = new List<List<int>>();
                ProductOutboundQty.Add(sl1);
                for (int f = 0; f < _numFashion; f++)
                {
                    List<int> sl2 = new List<int>();
                    ProductOutboundQty[t].Add(sl2);
                    //int QtyInWH = _WhBasicInv[0][p][t] + ProductInboundQty[p][t];
                    for (int s = 0; s < _numStores; s++)
                    {
                        ProductOutboundQty[t][f].Add(0);
                        if (ProductInboundQty[f][t] > 0)
                        {
                            ProductOutboundQty[t][f][s] = _demandFashion[s][f]; //ProductInboundQty[f][t];                            
                        }
                        else
                        {
                            ProductOutboundQty[t][f][s] = 0;
                        }
                    }   //end of stores loop                  
                }   //end of products loop
            }   //end of time-periods loop

            //Generating inbound and outbound solutions
            float inboundCost = 0;
            float putawayCost = 0;
            float outboundCost = 0;
            float pickingCost = 0;
            float whInvCost = 0;
            _InbFashionSol = new List<int>();
            _OutbFashionSol = new List<long>();

            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
            {
                for (int v = 0; v < _numFvendors; v++)
                {
                    for (int f = 0; f < _numFashion; f++)
                    {
                        int temp = (int)(_VPfashion[v][f] * ProductInboundQty[f][t]);
                        _InbFashionSol.Add(temp);
                    }
                }
            }
            for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
            {
                for (int f = 0; f < _numFashion; f++)
                {
                    for (int s = 0; s < _numStores; s++)
                    {
                        int temp = ProductOutboundQty[t][f][s];
                        _OutbFashionSol.Add(temp);
                    }
                }
            }
            UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
            UpdateStoreFashionInventory(_OutbFashionSol);

            for (int i = 0; i < _numWareHouses; i++)
            {
                inboundCost = CalFashionInbCost(_InbFashionSol, i);
                whInvCost = CalWhFashionInvCost();
                putawayCost = CalculatePutawayCost();
                CalFashionOutbWeight(_OutbFashionSol, i);
                outboundCost = CalculateOutboundCost();
                CalStoreFashionInvCost();
                pickingCost = CalculatePickingCost();
            }
            //PrintSolution();
            //twSol.Close();
        }

        //        public void SetNewRandom()
        //        {
        //            increment_value = increment_value    
        //            rand = new Random(increment_value);
        //        }


        //this method is same as policy 1...with few changes in making outbound move
        public float ImproveFashionInboundSol(float nextTotCost)
        {
            float NextTotalCost = 0;
            bool SolChanged = false;

            int time = 0;
            time = rand.Next(0, _endTime - _beginTime + 1); //Identifier to use go to statement
            int v = 0;
            while (v < _numFvendors)
            {
                if (_numFashionShipmentsVtoW[time][v] > 0 && (rand.Next(1, 100) < 50))
                {
                    if (time == 0) //then only delay move is possible ...check the feasibility after making an inbound move and correct it...
                    {
                        for (int f = 0; f < _numFashion; f++)
                        {
                            if (_VPfashion[v][f] == 1)
                            {
                                int inTime = rand.Next(time + 1, _endTime - _beginTime + 1); //
                                _InbFashionSol[inTime * _numFvendors * _numFashion + v * _numFashion + f] += _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f];
                                _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f] = 0;

                                int outTime = FashionOBStoreTimePeriod[f].Min();    //considering the earliest outbound for that product f to check for the feasibility

                                if (inTime > outTime)     //checking if inbound time is greater than outbound time..if true then makes the solution infeasible
                                {
                                    for (int s = 0; s < _numStores; s++)
                                    {
                                        outTime = FashionOBStoreTimePeriod[f][s];   //outbound time for product f to store s
                                        if (inTime > outTime)   //only make outbound move for stores which has early outbound than inbound time..i.e. make outbound feasible 
                                        {
                                            int moveTime = rand.Next(inTime, _dueTime - _beginTime - _leadTime[s] - pt + 1);    //selecting random time between moveTime and last time 
                                            //moving from outTime to MoveTime to make it a feasible solution
                                            _OutbFashionSol[moveTime * _numFashion * _numStores + f * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + f * _numStores + s];
                                            _OutbFashionSol[outTime * _numFashion * _numStores + f * _numStores + s] = 0;
                                        }
                                    }
                                }

                                SolChanged = true;
                            }
                        }
                        //if (SolChanged == true)
                        //    goto B;
                        //else
                        //    goto A;
                    }
                    if (time > 0 && time < _endTime - _beginTime)    //*************** may cause out of memory exception
                    {
                        if (rand.Next(1, 100) < 50)  //advance move..so no feasibility check is necessary
                        {
                            for (int f = 0; f < _numFashion; f++)
                            {
                                if (_VPfashion[v][f] == 1)
                                {
                                    int inTime = rand.Next(0, time);
                                    _InbFashionSol[inTime * _numFvendors * _numFashion + v * _numFashion + f] += _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f];
                                    _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f] = 0;

                                    SolChanged = true;
                                }
                            }
                            //if (SolChanged == true)
                            //    goto B; //if a move is implemented then go out of loop to calculate next total cost
                            //else
                            //    goto A;
                        }
                        else   //delay move...and select randomly time period between the selected period and last period
                        {
                            for (int f = 0; f < _numFashion; f++)
                            {
                                if (_VPfashion[v][f] == 1)
                                {
                                    int inTime = rand.Next(time + 1, _endTime - _beginTime + 1); //
                                    _InbFashionSol[inTime * _numFvendors * _numFashion + v * _numFashion + f] += _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f];
                                    _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f] = 0;

                                    int outTime = FashionOBStoreTimePeriod[f].Min();    //considering the earliest outbound for that product f

                                    if (inTime > outTime)     //checking if inbound time is greater than outbound time..if true then makes the solution infeasible
                                    {
                                        for (int s = 0; s < _numStores; s++)
                                        {
                                            outTime = FashionOBStoreTimePeriod[f][s];   //outbound time for product f to store s
                                            if (inTime > outTime)   //only make outbound move for stores which has early outbound than inbound time..i.e. make outbound feasible
                                            {
                                                int moveTime = rand.Next(inTime, _dueTime - _beginTime - _leadTime[s] - pt + 1);    //selecting random time between inTime and last time 
                                                //moving from outTime to moveTime to make it a feasible solution
                                                _OutbFashionSol[moveTime * _numFashion * _numStores + f * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + f * _numStores + s];
                                                _OutbFashionSol[outTime * _numFashion * _numStores + f * _numStores + s] = 0;
                                            }
                                        }
                                    }

                                    SolChanged = true;

                                }
                            }
                            //if (SolChanged == true)
                            //    goto B;
                            //else
                            //    goto A;
                        }
                    }
                    if (time == _endTime - _beginTime)       //if time is last period then you can only advance and so no need to feasibility check
                    {
                        for (int f = 0; f < _numFashion; f++)
                        {
                            if (_VPfashion[v][f] == 1 && _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f] > 0)
                            {
                                int inTime = rand.Next(0, time);
                                if (inTime != time)       //only to make sure moveTime is not equal to time..otherwise the inbound qty in that period becomes zero..
                                {
                                    _InbFashionSol[inTime * _numFvendors * _numFashion + v * _numFashion + f] += _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f];
                                    _InbFashionSol[time * _numFvendors * _numFashion + v * _numFashion + f] = 0;
                                }

                                SolChanged = true;
                            }
                        }

                    }
                }
            A: v++;
            }   //end of while loop for vendors
        B: if (SolChanged == true)     //calculating the total costs only if the solution is changes
            {
                UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
                UpdateStoreFashionInventory(_OutbFashionSol);

                CalFashionInbCost(_InbFashionSol, 0);
                CalWhFashionInvCost();
                CalculatePutawayCost();
                CalFashionOutbWeight(_OutbFashionSol, 0);
                CalculateOutboundCost();
                CalculatePickingCost();
                CalStoreFashionInvCost();
                NextTotalCost = ListOfCosts.Sum();
                SolChanged = false;
            }
            else
                NextTotalCost = nextTotCost;

            //PrintSolution();
            return NextTotalCost;
        }

        /// <summary>
        /// Method to improve fashion outbound solution
        /// </summary>
        /// <param name="nextTotCost"></param>
        /// <returns></returns>
        public float ImproveFashionOutboundSol(float nextTotCost)
        {
            float NextTotalCost = 0;
            bool SolChanged = false;
            //int randomNum = rand.Next(40, 70);        //to vary the percentage of moving a product in each iteration
            for (int prod = 0; prod < _numFashion; prod++)
            {
                if (rand.Next(1, 100) < 100)
                {
                    for (int s = 0; s < _numStores; s++)
                    {
                        int inTime = FashionInbTimeperiod[prod];
                        int outTime = FashionOBStoreTimePeriod[prod][s];
                        int randNum = rand.Next(1, 100);

                        if (outTime > inTime && outTime <= _dueTime - _beginTime - _leadTime[s] - pt)
                        {
                            if (randNum < 50)   //advance
                            {
                                int MoveTime = rand.Next(inTime, outTime); //select a period between inbound time and outbound scheduled time..ie advance feasible move for outbound
                                _OutbFashionSol[(MoveTime) * _numFashion * _numStores + prod * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s];
                                _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s] = 0;
                                SolChanged = true;
                            }
                            //we are delaying in inbound move...so no need again
                            //else   //delay only if outTime is not the last time period 
                            //{
                            //    if (outTime != _dueTime - _beginTime - _leadTime[s] - pt)
                            //    {
                            //        //int MoveTime = rand.Next(outTime + 1, _dueTime - _beginTime - _leadTime[s] - pt + 1); //delay move...between scheduled time and duedate
                            //        _OutbFashionSol[(outTime + 1) * _numFashion * _numStores + prod * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s];
                            //        _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s] = 0;
                            //        SolChanged = true;
                            //    }
                            //}
                        }

                        if (outTime == inTime && outTime < _dueTime - _beginTime - _leadTime[s] - pt)   //delay moves
                        {
                            List<int> _storeTimeList = new List<int>();     //list that contains the time periods at which warehouse outbound shipment to that store
                            for (int t = _beginTime + pt; t < _dueTime - _leadTime[s]; t++)
                            {
                                if (_numShipmentsWtoS[t][s] > 0 && t > (inTime + _beginTime + pt))      //make sure you are looking at the right time period
                                    _storeTimeList.Add(t);
                            }
                            if (_storeTimeList.Count >= 1)
                            {
                                int MoveTime = rand.Next(0, _storeTimeList.Count);      //*********moving to right time period is critical..check carefully the below line of code
                                _OutbFashionSol[(_storeTimeList[MoveTime] - _beginTime - pt) * _numFashion * _numStores + prod * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s];
                                _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s] = 0;
                                SolChanged = true;
                            }
                            else
                            {
                                //int MoveTime = rand.Next(outTime + 1, _dueTime - _beginTime - _leadTime[s] - pt + 1); //delay move...between scheduled time and duedate                           

                                _OutbFashionSol[(outTime + 1) * _numFashion * _numStores + prod * _numStores + s] += _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s];
                                _OutbFashionSol[outTime * _numFashion * _numStores + prod * _numStores + s] = 0;
                                SolChanged = true;
                            }
                        }
                        //    }
                        //}//end of fashion products loop                                         
                    }  // end of stores loop  
                }
            }//

            if (SolChanged == true)     //calculating the total costs only if the solution is changes
            {
                UpdateStoreFashionInventory(_OutbFashionSol);
                UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
                CalFashionOutbWeight(_OutbFashionSol, 0);
                CalculateOutboundCost();
                CalculatePickingCost();
                CalWhFashionInvCost();
                CalStoreFashionInvCost();
                NextTotalCost = ListOfCosts.Sum();
                SolChanged = false;
            }
            else
                NextTotalCost = nextTotCost;

            //PrintSolution();
            return NextTotalCost;
        }

        public float ImproveBasicInboundSol(float nextTotCost)
        {
            float NextTotalCost = 0;
            bool SolChanged = false;
            int T = 0;

            if (rand.Next(1, 100) < 60)
                T = rand.Next(0, _numTimePeriods);
            else
                T = _numPutaway.IndexOf(_numPutaway.Max());

            int minTime = _numPutaway.IndexOf(_numPutaway.Min());

            for (int v = 0; v < _numBvendors; v++)
            {
                if (rand.Next(1, 100) < 75)
                {
                    if (_numBasicShipmentsVtoW[T][v] > 0)  //checking if there is a positive shipment in that time period
                    {
                        if (_numBasicShipmentsVtoW[T][v] <= 1)        //0.4 * totalQtyFromVendor)  //0.4 * totalShipment)
                        {
                            List<int> _vendorTimeList = new List<int>();    //list for inbound times from each vendor
                            for (int t = 0; t < _numTimePeriods; t++)
                            {
                                if (_numBasicShipmentsVtoW[t][v] > 0)
                                    _vendorTimeList.Add(t);
                            }
                            int moveTime = rand.Next(0, _vendorTimeList.Count);
                            if (_vendorTimeList[moveTime] != T && _vendorTimeList.Count > 1)
                            {
                                for (int p = 0; p < _numBasic; p++)
                                {
                                    _InbBasicSol[_vendorTimeList[moveTime] * _numBvendors * _numBasic + v * _numBasic + p] += _InbBasicSol[T * _numBvendors * _numBasic + v * _numBasic + p];
                                    _InbBasicSol[T * _numBvendors * _numBasic + v * _numBasic + p] = 0;
                                }
                                SolChanged = true;
                                //goto B;
                            }
                        }

                        else //if (_numBasicShipmentsVtoW[T][v] > 1)
                        {                            
                            if (minTime != T)
                            {
                                float percentMove = 0.0F;
                                for (int p = 0; p < _numBasic; p++)
                                {
                                    percentMove = rand.Next(0, 11) * 0.1F;
                                    _InbBasicSol[(minTime) * _numBvendors * _numBasic + v * _numBasic + p] += (int)(percentMove * _InbBasicSol[T * _numBvendors * _numBasic + v * _numBasic + p]);
                                    _InbBasicSol[T * _numBvendors * _numBasic + v * _numBasic + p] -= (int)(percentMove * _InbBasicSol[T * _numBvendors * _numBasic + v * _numBasic + p]);
                                }
                                SolChanged = true;
                                //goto B;
                            }
                        }
                    }
                }
            }
        B: if (SolChanged == true)
            {
                UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
                float inboundCost = CalculateInboundCost(_InbBasicSol, 0);  //calculates inbound transportation and warehouse inventory costs
                float putawayCost = CalculatePutawayCost();
                NextTotalCost = ListOfCosts.Sum();
                SolChanged = false;
                //PrintSolution();
            }
            else
                NextTotalCost = nextTotCost;

            return NextTotalCost;
        }

        /// <summary>
        /// Method to improve basic outbound solution considering store distance before making a move
        /// </summary>
        /// <returns></returns>
        public float ImproveBasicOutboundSol()
        {
            float NextTotalCost = 0;
            int T = 0;

            if (rand.Next(1, 100) < 75)     //75% chance ..to prevent from cycling..
                T = rand.Next(0, _numTimePeriods);
            else
                T = _numPickers.IndexOf(_numPickers.Max());

            //int T = _numPickers.IndexOf(_numPickers.Max());     //selecting a time period with maximum putaway workers
            int minTime = _numPickers.IndexOf(_numPickers.Min());

            for (int s = 0; s < _numStores; s++)
            {
                //rand.Next(1, Math.Max(2, (int)_numStores / 10));      //percentage of selecting a store is set based on the total number of stores**********
                if (rand.Next(1, 100) < rand.Next(50, 100))              //Percentage of selecting a store is between 50% - 75%    
                {
                    if (_numShipmentsWtoS[T][s] > 0)  //checking if there is a positive shipment in that time period
                    {
                        if (rand.Next(1, 100) < 40) //20% chance of consolidation based on distance
                        {
                            List<int> _storeTimeList = new List<int>();
                            for (int t = 0; t < _numTimePeriods; t++)
                            {
                                if (_numShipmentsWtoS[t][s] > 0)
                                    _storeTimeList.Add(t);
                            }
                            int moveTime = rand.Next(0, _storeTimeList.Count);
                            if (_storeTimeList[moveTime] != T)
                            {
                                for (int p = 0; p < _numBasic; p++)
                                {
                                    _OutbBasicSol[_storeTimeList[moveTime] * _numBasic * _numStores + p * _numStores + s] += _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s];
                                    _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s] = 0;
                                }
                            }
                        }
                        else    //other 80% chance of splitting the shipments..based on idea of having more shipments to near stores which could 
                        //in reducing warehousing and inventory costs
                        {
                            if (minTime != T)
                            {
                                //float percentMove = 0.0F;       
                                for (int p = 0; p < _numBasic; p++)
                                {
                                    //percentMove = rand.Next(4, 7) * 0.1F;
                                    //    _OutbBasicSol[minTime * _numBasic * _numStores + p * _numStores + s] += (int)(percentMove * _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s]);
                                    //    _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s] -= (int)(percentMove * _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s]);
                                    _OutbBasicSol[minTime * _numBasic * _numStores + p * _numStores + s] += _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s];
                                    _OutbBasicSol[T * _numBasic * _numStores + p * _numStores + s] = 0;

                                }
                            }
                        }

                    }
                }
            }
            UpdateStoreInventory(_OutbBasicSol);
            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            CalBasicOutbWeight(_OutbBasicSol, 0);
            float outboundTransCost = CalculateOutboundCost();
            float pickingCost = CalculatePickingCost();
            float wInvCost = CalculateWarehouseInventoryCost();
            NextTotalCost = ListOfCosts.Sum();

            //PrintSolution();
            return NextTotalCost;
        }



        public float SwapInboundShipments()
        {
            //select two time periods
            int t1 = 0;
            int t2 = 0;
            int v1 = 0;
            int v2 = 0;

            t1 = rand.Next(0, _numTimePeriods);
            t2 = rand.Next(0, _numTimePeriods);
            while (t1 == t2)
                t2 = rand.Next(0, _numTimePeriods);

            int temp = rand.Next(1, 6);                                            //randomizing the perturbation move
            int swapNum = (int)Math.Round(_numBvendors * 0.1F * temp);           //Swap is considered as the perturbation of local optimum. This is deterministic as everytime we only swap 10% of stroes. If we randomize this then it is VNS
            for (int v = 0; v < swapNum; v++)
            {
                v1 = rand.Next(0, _numBvendors);
                v2 = rand.Next(0, _numBvendors);
                for (int p = 0; p < _numBasic; p++)
                {
                    _InbBasicSol[t2 * _numBvendors * _numBasic + v1 * _numBasic + p] += _InbBasicSol[t1 * _numBvendors * _numBasic + v1 * _numBasic + p];
                    _InbBasicSol[t1 * _numBvendors * _numBasic + v1 * _numBasic + p] = 0;
                    _InbBasicSol[t1 * _numBvendors * _numBasic + v2 * _numBasic + p] += _InbBasicSol[t2 * _numBvendors * _numBasic + v2 * _numBasic + p];
                    _InbBasicSol[t2 * _numBvendors * _numBasic + v2 * _numBasic + p] = 0;
                }
            }

            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            float inboundCost = CalculateInboundCost(_InbBasicSol, 0);  //calculates inbound transportation and warehouse inventory costs
            float putawayCost = CalculatePutawayCost();
            float TotalCost = ListOfCosts.Sum();

            return TotalCost;
        }

        ////////Method to swap outbound shipments
        public float SwapOutboundShipments()
        {
            //select two time periods
            int t1 = 0;
            int t2 = 0;
            int s1 = 0;
            int s2 = 0;

            t1 = rand.Next(0, _numTimePeriods);
            t2 = rand.Next(0, _numTimePeriods);
            while (t1 == t2)
                t2 = rand.Next(0, _numTimePeriods);

            int temp = rand.Next(3, 8);                                            //randomizing the perturbation move
            int swapNum = (int)Math.Round(_numStores * 0.1F * temp);           //Swap is considered as the perturbation of local optimum. This is deterministic as everytime we only swap 10% of stroes. If we randomize this then it is VNS
            for (int s = 0; s < swapNum; s++)
            {
                s1 = rand.Next(0, _numStores);
                s2 = rand.Next(0, _numStores);
                for (int p = 0; p < _numBasic; p++)
                {
                    _OutbBasicSol[t2 * _numBasic * _numStores + p * _numStores + s1] += _OutbBasicSol[t1 * _numBasic * _numStores + p * _numStores + s1];
                    _OutbBasicSol[t1 * _numBasic * _numStores + p * _numStores + s1] = 0;
                    _OutbBasicSol[t1 * _numBasic * _numStores + p * _numStores + s2] += _OutbBasicSol[t2 * _numBasic * _numStores + p * _numStores + s2];
                    _OutbBasicSol[t2 * _numBasic * _numStores + p * _numStores + s2] = 0;
                }
            }

            UpdateStoreInventory(_OutbBasicSol);
            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            CalBasicOutbWeight(_OutbBasicSol, 0);                       //calculates store inventory cost
            float outboundTransCost = CalculateOutboundCost();
            float pickingCost = CalculatePickingCost();
            float wInvCost = CalculateWarehouseInventoryCost();
            float TotalCost = ListOfCosts.Sum();

            return TotalCost;
        }

        public void UpdateTop5Technologies(float BestTotalCost, int BestPutawayTechnology, int BestPickTechnology)
        {
            if (Top5TotalCostList[0] == 0)
            {
                Top5TotalCostList[0] = BestTotalCost;
                Top5PutawayTechList[0] = BestPutawayTechnology;
                Top5PickTechList[0] = BestPickTechnology;
            }
            else
            {
                int indexMatch = 0;
                Boolean Match = false;
                for (int i = 0; i < Top5TotalCostList.Count; i++)
                {
                    if (BestPutawayTechnology == Top5PutawayTechList[i] && BestPickTechnology == Top5PickTechList[i])
                    {
                        indexMatch = i;
                        Match = true;
                        break;
                    }
                }
                for (int i = 0; i < Top5TotalCostList.Count; i++)
                {
                    if (BestTotalCost < Top5TotalCostList[i])
                    {
                        if (Match == true && i < indexMatch)
                        {
                            for (int j = indexMatch; j < i; j--)
                            {
                                Top5TotalCostList[j] = Top5TotalCostList[j - 1];
                                Top5PutawayTechList[j] = Top5PutawayTechList[j - 1];
                                Top5PickTechList[j] = Top5PickTechList[j - 1];                                
                            }
                            break;
                        }
                        else if (Match == true && i == indexMatch)
                        {
                            Top5TotalCostList[i] = BestTotalCost;
                            Top5PutawayTechList[i] = BestPutawayTechnology;
                            Top5PickTechList[i] = BestPickTechnology;
                            break;
                        }
                        else if (Match == true && i > indexMatch)
                            break;
                        else
                        {
                            if (i == Top5TotalCostList.Count - 1)
                            {
                                Top5TotalCostList[i] = BestTotalCost;
                                Top5PutawayTechList[i] = BestPutawayTechnology;
                                Top5PickTechList[i] = BestPickTechnology;
                                break;
                            }
                            else
                            {
                                for (int j = Top5TotalCostList.Count - 1; j > i; j--)
                                {
                                    Top5TotalCostList[j] = Top5TotalCostList[j - 1];
                                    Top5PutawayTechList[j] = Top5PutawayTechList[j - 1];
                                    Top5PickTechList[j] = Top5PickTechList[j - 1];
                                }
                                Top5TotalCostList[i] = BestTotalCost;
                                Top5PutawayTechList[i] = BestPutawayTechnology;
                                Top5PickTechList[i] = BestPickTechnology;
                                break;
                            }
                        }

                    }

                }
            }
        }

        List<List<int>> GenBestFashionInboundSol = new List<List<int>>();           //[gen][Best sol] to store the best solution in each generation
        List<List<long>> GenBestFashionOutboundSol = new List<List<long>>();
        List<List<int>> GenBestBasicInboundSol = new List<List<int>>();
        List<List<long>> GenBestBasicOutboundSol = new List<List<long>>();

        List<int> BestFashionInboundSol;                            //to store best fashion inbound sol
        List<long> BestFashionOutboundSol;                        //to store best fashion outbound solution                                                 
        List<int> BestBasicInboundSol;                            //to store best basic inbound sol
        List<long> BestBasicOutboundSol;                        //to store best basic outbound solution  

        float CurrentTotalCost;
        float BestTotalCost;
        int BestPutawayTechnology;
        int BestPickTechnology;
        List<int> BestPutawayTechList = new List<int>();        //List to store best putaway tech in each generation
        List<int> BestPickTechList = new List<int>();
        List<int> Top5PutawayTechList = new List<int>() { 10, 10, 10, 10, 10 };       //List to store best 5 technologies 
        List<int> Top5PickTechList = new List<int>() { 10, 10, 10, 10, 10 };
        List<float> Top5TotalCostList = new List<float>() { 0, 0, 0, 0, 0 };


        List<float> TotalCostGen = new List<float>();                          //to store best total cost in each outer iteration
        public void SetNewRandom()
        {
            increment_value = increment_value + 1;
            rand = new Random(increment_value);

            //twSol = new StreamWriter("OptPolicyResults\\Solution.txt");
            //twTC = new StreamWriter("OptPolicyResults\\TotalCostList.txt");

        }
        public void OptimizeFn()
        {
            DateTime StartTime = new DateTime();
            StartTime = DateTime.Now;
            List<int> Gensl1;
            List<long> Gensl2;
            List<int> Gensl3;
            List<long> Gensl4;
            for (int g = 0; g < TotalGenerations; g++)
            {
                rand = new Random();        //instantiating rand in every generation makes the code generate different random numbers in each generation as seed will be different
                Gensl1 = new List<int>();
                Gensl2 = new List<long>();
                Gensl3 = new List<int>();
                Gensl4 = new List<long>();

                BestFashionInboundSol = new List<int>();                             //to store best fashion inbound sol
                BestFashionOutboundSol = new List<long>();                           //to store best fashion outbound solution                                                    
                BestBasicInboundSol = new List<int>();                              //to store best basic inbound sol
                BestBasicOutboundSol = new List<long>();                            //to store best basic outbound solution 

                int totIter = 0;
                int numIter = 0;
                BestTotalCost = 0;
                List<int> currentFashionInbSol = new List<int>();
                List<long> currentFashionObSol = new List<long>();

                List<int> currentBasicInbSol = new List<int>();
                List<long> currentBasicObSol = new List<long>();

                //GenerateRandomInitialSol();
                GenerateDemandQtyAsInitialSol();
                GenRandInitFashionSol();
                BestTotalCost = ListOfCosts.Sum();
                CurrentTotalCost = BestTotalCost;
                BestPutawayTechnology = currentPutawayTech;
                BestPickTechnology = currentPickTech;

                for (int i = 0; i < _InbFashionSol.Count; i++)  //making the initial fashion inbound solution as the best and current inbound fashion sol
                {
                    BestFashionInboundSol.Add(0);
                    BestFashionInboundSol[i] = _InbFashionSol[i];

                    currentFashionInbSol.Add(0);
                    currentFashionInbSol[i] = _InbFashionSol[i];
                }

                for (int i = 0; i < _OutbFashionSol.Count; i++)     //making the initial fashion outbound solution as the best and current fashion outbound sol
                {
                    BestFashionOutboundSol.Add(0);
                    BestFashionOutboundSol[i] = _OutbFashionSol[i];

                    currentFashionObSol.Add(0);
                    currentFashionObSol[i] = _OutbFashionSol[i];
                }

                for (int i = 0; i < _InbBasicSol.Count; i++)        //making the initial basic solution as the best and current basic inbound solution 
                {
                    BestBasicInboundSol.Add(0);
                    BestBasicInboundSol[i] = _InbBasicSol[i];

                    currentBasicInbSol.Add(0);
                    currentBasicInbSol[i] = _InbBasicSol[i];
                }

                for (int i = 0; i < _OutbBasicSol.Count; i++)       //making the initial outbound solution as the best and current basic outbound solution
                {
                    BestBasicOutboundSol.Add(0);
                    BestBasicOutboundSol[i] = _OutbBasicSol[i];

                    currentBasicObSol.Add(0);
                    currentBasicObSol[i] = _OutbBasicSol[i];
                }

                float PreviousInbBest = 100000000;
                float PreviousOutbndBest = 100000000;
                float PreviousTotalBest = 100000000;
                int stopCounter = 0;
                int stopCounterTC = 0;   //stop counter for total cost
                float delta = 0.0025F;
                //List<int> testPutTechSelectionList = new List<int>();

                while (totIter < NoTotalIterations)
                {
                    numIter = 1;
                    stopCounter = 0;
                    int techIter = 0;
                    bool isTechMove = false;
                    while (numIter < NoOfIterations)
                    {
                        float NextFashionCost = ImproveFashionInboundSol(CurrentTotalCost);
                        if (NextFashionCost < CurrentTotalCost)                //if next sol is better (lower) than the current sol
                        {
                            for (int i = 0; i < _InbFashionSol.Count; i++)              //then update the current solution
                                currentFashionInbSol[i] = _InbFashionSol[i];

                            for (int i = 0; i < _OutbFashionSol.Count; i++)
                                currentFashionObSol[i] = _OutbFashionSol[i];

                            CurrentTotalCost = NextFashionCost;                 //update the current cost

                            if (CurrentTotalCost < BestTotalCost)        //if it is better than the best solution found so far
                            {
                                BestTotalCost = CurrentTotalCost;         //update the best cost
                                BestPutawayTechnology = currentPutawayTech;     //update the best technology

                                for (int i = 0; i < _InbFashionSol.Count; i++)          //update the best fashion inbound and outbound solution
                                    BestFashionInboundSol[i] = currentFashionInbSol[i];

                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    BestFashionOutboundSol[i] = currentFashionObSol[i];
                            }
                        }

                        else    //accept the inferior solutions with a probability
                        {
                            float prob = rand.Next(0, 100);
                            if (prob < ProbabilityValue)                //accept with a probability p
                            {
                                CurrentTotalCost = NextFashionCost;

                                for (int i = 0; i < _InbFashionSol.Count; i++)
                                    currentFashionInbSol[i] = _InbFashionSol[i];

                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    currentFashionObSol[i] = _OutbFashionSol[i];
                            }

                            else     //if not then update the inbound and outbound fashion solutions with the current solution ..ie undo the move which do not improve the solution
                            {
                                for (int i = 0; i < _InbFashionSol.Count; i++)
                                    _InbFashionSol[i] = currentFashionInbSol[i];

                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    _OutbFashionSol[i] = currentFashionObSol[i];

                                CalFashionInbCost(_InbFashionSol, 0);   //to update the inbound shipments from fashion vendors to warehouse..as it is used as condition in 
                                //in improving solutions
                                CalFashionOutbWeight(_OutbFashionSol, 0);       //to update the outbound fashion shipments to stores..as it is used as condition in 
                                CalculateOutboundCost();                        //in improving solutions
                            }
                        }

                        //CurrentInbBest = bestTotalCost;
                        if (numIter % 5 == 0)
                        {

                            float UpperLimit = PreviousInbBest + delta * PreviousInbBest;
                            float LowerLimit = PreviousInbBest - delta * PreviousInbBest;
                            if ((LowerLimit <= BestTotalCost) && (UpperLimit >= BestTotalCost))
                            {
                                stopCounter++;
                            }
                            else
                            {
                                //Console.WriteLine(numIter + "\t" + PreviousInbBest + "\t" + CurrentInbBest);
                                stopCounter = 0;
                                PreviousInbBest = BestTotalCost;
                            }
                            if (stopCounter == stopIter)
                            {
                                Console.WriteLine("Fashion Inbound..break at iteration no:" + numIter + "of" + totIter);
                                isTechMove = true;

                            }
                        }

                        if (isTechMove == true || numIter > NoOfIterations - techIter)
                        {
                            UpdateTop5Technologies(BestTotalCost, BestPutawayTechnology, BestPickTechnology);

                            PutawayTechnologySelection();
                            //testPutTechSelectionList.Add(currentPutawayTech);
                            numIter = 1;
                            techIter = techIter + 1;
                            isTechMove = false;
                            if (techIter == MaxTechIter)
                                break;
                        }

                        numIter++;
                    }   //End of while loop for fashion inbound iterations

                    CurrentTotalCost = BestTotalCost;
                    currentPutawayTech = BestPutawayTechnology;

                    for (int i = 0; i < _InbFashionSol.Count; i++)      //updating next fashion inbound sol with best inbound sol
                        _InbFashionSol[i] = BestFashionInboundSol[i];

                    for (int i = 0; i < _OutbFashionSol.Count; i++)       //updating next fashion outbound sol with best outbound sol
                        _OutbFashionSol[i] = BestFashionOutboundSol[i];

                    for (int i = 0; i < currentFashionInbSol.Count; i++)        //updating current fashion outbound sol with best outbound sol
                        currentFashionInbSol[i] = BestFashionInboundSol[i];

                    for (int i = 0; i < currentFashionObSol.Count; i++)       //updating current fashion outbound sol with best outbound sol
                        currentFashionObSol[i] = BestFashionOutboundSol[i];

                    UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
                    UpdateStoreFashionInventory(_OutbFashionSol);
                    CalWhFashionInvCost();
                    CalStoreFashionInvCost();
                    CalFashionInbCost(_InbFashionSol, 0);
                    CalculatePutawayCost();
                    CalFashionOutbWeight(_OutbFashionSol, 0);
                    CalculateOutboundCost();
                    CalculatePickingCost();

                    /////////////////////////////////////////////////////////Iterations to improve basic inbound solution
                    numIter = 1;
                    stopCounter = 0;
                    techIter = 0;
                    CurrentTotalCost = BestTotalCost;
                    currentPutawayTech = BestPutawayTechnology;
                    while (numIter < NoOfIterations)
                    {
                        float NextInbCost = ImproveBasicInboundSol(CurrentTotalCost);
                        if (NextInbCost < CurrentTotalCost)
                        {
                            for (int i = 0; i < _InbBasicSol.Count; i++)
                                currentBasicInbSol[i] = _InbBasicSol[i];

                            CurrentTotalCost = NextInbCost;

                            if (CurrentTotalCost < BestTotalCost)         //updating best inbound cost
                            {
                                BestTotalCost = CurrentTotalCost;
                                BestPutawayTechnology = currentPutawayTech;

                                for (int i = 0; i < _InbBasicSol.Count; i++)
                                    BestBasicInboundSol[i] = currentBasicInbSol[i];
                            }
                        }

                        else
                        {
                            float prob = rand.Next(0, 100);
                            if (prob < ProbabilityValue)
                            {
                                for (int i = 0; i < _InbBasicSol.Count; i++)
                                    currentBasicInbSol[i] = _InbBasicSol[i];

                                CurrentTotalCost = NextInbCost;
                            }
                            else
                            {
                                for (int i = 0; i < _InbBasicSol.Count; i++)
                                    _InbBasicSol[i] = currentBasicInbSol[i];

                                CalculateInboundCost(_InbBasicSol, 0);  //to update the inbound shipments from basic vendors to warehouse..as it is used as condition in 
                                //in improving solutions
                                CalculatePutawayCost();
                            }
                        }

                        //CurrentInbBest = bestTotalCost;
                        if (numIter % 5 == 0)
                        {

                            float UpperLimit = PreviousInbBest + delta * PreviousInbBest;
                            float LowerLimit = PreviousInbBest - delta * PreviousInbBest;
                            if ((LowerLimit <= BestTotalCost) && (UpperLimit >= BestTotalCost))
                            {
                                stopCounter++;
                            }
                            else
                            {
                                //Console.WriteLine(numIter + "\t" + PreviousInbBest + "\t" + CurrentInbBest);
                                stopCounter = 0;
                                PreviousInbBest = BestTotalCost;
                            }
                            if (stopCounter == stopIter)
                            {
                                Console.WriteLine("Basic Inbound..break at iteration no:" + numIter + "of" + totIter);
                                isTechMove = true;
                                //PutawayTechnologySelection();
                                //numIter = 1;
                                //techIter++;
                                //if (techIter == MaxTechIter)
                                //    break;
                            }
                            //numIter = TotalNoIterations


                        }

                        if (isTechMove == true || numIter > NoOfIterations - techIter)
                        {
                            UpdateTop5Technologies(BestTotalCost, BestPutawayTechnology, BestPickTechnology);

                            PutawayTechnologySelection();
                            numIter = 1;
                            techIter = techIter + 1;
                            isTechMove = false;
                            if (techIter == MaxTechIter)
                                break;
                        }

                        numIter++;
                    }   //End of while loop for inbound iterations

                    CurrentTotalCost = BestTotalCost;
                    currentPutawayTech = BestPutawayTechnology;

                    for (int i = 0; i < _InbBasicSol.Count; i++)
                        _InbBasicSol[i] = BestBasicInboundSol[i];

                    for (int i = 0; i < currentBasicInbSol.Count; i++)
                        currentBasicInbSol[i] = BestBasicInboundSol[i];

                    UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
                    CalculateInboundCost(_InbBasicSol, 0);
                    CalculatePutawayCost();

                    /////////////////////////////code to swap inbound shipments
                    float totCost = 0;
                    int counter = 0;
                    while (counter < NumSwapIterations)
                    {
                        totCost = SwapInboundShipments();
                        if (totCost < CurrentTotalCost)
                        {
                            Console.WriteLine("% savings with Inbound swap is" + ((CurrentTotalCost - totCost) / CurrentTotalCost) * 100);

                            for (int i = 0; i < _InbBasicSol.Count; i++)
                                BestBasicInboundSol[i] = _InbBasicSol[i];

                            for (int i = 0; i < currentBasicInbSol.Count; i++)
                                currentBasicInbSol[i] = _InbBasicSol[i];

                            CurrentTotalCost = totCost;
                            BestTotalCost = totCost;
                        }

                        else
                        {
                            for (int i = 0; i < _InbBasicSol.Count; i++)
                                _InbBasicSol[i] = BestBasicInboundSol[i];
                        }

                        counter++;
                    }
                    UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
                    CalculateInboundCost(_InbBasicSol, 0);
                    CalculatePutawayCost();

                    /////////////////////////////////////////////////////////////////////////////////// Iterations to improve fashion outbound
                    numIter = 1;
                    stopCounter = 0;        //counter for stopping criteria for outbound
                    techIter = 0;
                    CurrentTotalCost = BestTotalCost;
                    while (numIter < NoOfIterations)
                    {
                        float NextOutbCost = ImproveFashionOutboundSol(CurrentTotalCost);
                        if (NextOutbCost <= CurrentTotalCost)
                        {
                            for (int i = 0; i < _OutbFashionSol.Count; i++)
                                currentFashionObSol[i] = _OutbFashionSol[i];

                            CurrentTotalCost = NextOutbCost;

                            if (CurrentTotalCost <= BestTotalCost)        //Updating best outbound cost and solution
                            {
                                BestTotalCost = CurrentTotalCost;
                                BestPickTechnology = currentPickTech;

                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    BestFashionOutboundSol[i] = currentFashionObSol[i];
                            }
                        }

                        else
                        {
                            float prob = rand.Next(0, 100);
                            if (prob <= ProbabilityValue)
                            {
                                CurrentTotalCost = NextOutbCost;
                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    currentFashionObSol[i] = _OutbFashionSol[i];
                            }
                            else
                            {
                                for (int i = 0; i < _OutbFashionSol.Count; i++)
                                    _OutbFashionSol[i] = currentFashionObSol[i];

                                CalFashionOutbWeight(_OutbFashionSol, 0);       //to update the outbound fashion shipments to stores..as it is used as condition in 
                                CalculateOutboundCost();                        //in improving solutions
                            }
                        }

                        //CurrentOutbndBest = bestTotalCost;
                        if (numIter % 5 == 0)
                        {

                            float UpperLimit = PreviousOutbndBest + delta * PreviousOutbndBest;
                            float LowerLimit = PreviousOutbndBest - delta * PreviousOutbndBest;
                            if ((LowerLimit <= BestTotalCost) && (UpperLimit >= BestTotalCost))
                            {
                                stopCounter++;
                            }
                            else
                            {
                                //Console.WriteLine(numIter + "\t" + PreviousInbBest + "\t" + CurrentInbBest);
                                stopCounter = 0;
                                PreviousOutbndBest = BestTotalCost;
                            }
                            if (stopCounter == stopIter)
                            {
                                Console.WriteLine("Fashion Outbound -- break at iteration no:" + numIter + "of" + totIter);
                                isTechMove = true;
                                //PickTechnologySelection();
                                //numIter = 1;
                                //techIter++;
                                //if (techIter == MaxTechIter)
                                //    break;
                            }
                            //numIter = TotalNoIterations
                        }

                        if (isTechMove == true || numIter > NoOfIterations - techIter)
                        {
                            UpdateTop5Technologies(BestTotalCost, BestPutawayTechnology, BestPickTechnology);

                            PickTechnologySelection();
                            numIter = 1;
                            techIter = techIter + 1;
                            isTechMove = false;
                            if (techIter == MaxTechIter)
                                break;
                        }

                        numIter++;
                    }            /////End of while loop for fashion outbound iterations

                    CurrentTotalCost = BestTotalCost;         //updating current outbound cost with the best outbound cost
                    currentPickTech = BestPickTechnology;

                    for (int i = 0; i < _OutbFashionSol.Count; i++)       //updating current outbound sol with best outbound sol
                        _OutbFashionSol[i] = BestFashionOutboundSol[i];

                    for (int i = 0; i < currentFashionObSol.Count; i++)       //updating current outbound sol with best outbound sol
                        currentFashionObSol[i] = BestFashionOutboundSol[i];

                    UpdateStoreFashionInventory(_OutbFashionSol);
                    UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
                    CalWhFashionInvCost();
                    CalStoreFashionInvCost();
                    CalFashionOutbWeight(_OutbFashionSol, 0);
                    CalculateOutboundCost();    //to update outbound cost components in the listofcosts
                    CalculatePickingCost();
                    // ListOfCosts[0] = CalculateWarehouseInventoryCost();

                    /////////////////////////////////////////////////////////////////////////////// Iterations to improve basic outbound
                    numIter = 1;
                    stopCounter = 0;        //counter for stopping criteria for outbound
                    techIter = 0;
                    CurrentTotalCost = BestTotalCost;
                    while (numIter < NoOfIterations)
                    {
                        float NextOutbCost = ImproveBasicOutboundSol();
                        if (NextOutbCost < CurrentTotalCost)
                        {
                            for (int i = 0; i < _OutbBasicSol.Count; i++)
                                currentBasicObSol[i] = _OutbBasicSol[i];

                            CurrentTotalCost = NextOutbCost;

                            if (CurrentTotalCost < BestTotalCost)        //Updating best outbound cost and solution
                            {
                                BestTotalCost = CurrentTotalCost;
                                BestPickTechnology = currentPickTech;

                                for (int i = 0; i < _OutbBasicSol.Count; i++)
                                    BestBasicOutboundSol[i] = currentBasicObSol[i];
                            }
                        }

                        else
                        {
                            float prob = rand.Next(0, 100);
                            if (prob <= ProbabilityValue)
                            {
                                CurrentTotalCost = NextOutbCost;
                                for (int i = 0; i < _OutbBasicSol.Count; i++)
                                    currentBasicObSol[i] = _OutbBasicSol[i];
                            }
                            else
                            {
                                for (int i = 0; i < _OutbBasicSol.Count; i++)
                                    _OutbBasicSol[i] = currentBasicObSol[i];

                                CalBasicOutbWeight(_OutbBasicSol, 0);   //to update the outbound fashion shipments to stores..as it is used as condition in 
                                CalculateOutboundCost();                        //in improving solutions    
                                CalculatePickingCost();
                            }
                        }

                        //CurrentOutbndBest = bestTotalCost;
                        if (numIter % 5 == 0)
                        {

                            float UpperLimit = PreviousOutbndBest + delta * PreviousOutbndBest;
                            float LowerLimit = PreviousOutbndBest - delta * PreviousOutbndBest;
                            if ((LowerLimit <= BestTotalCost) && (UpperLimit >= BestTotalCost))
                            {
                                stopCounter++;
                            }
                            else
                            {
                                //Console.WriteLine(numIter + "\t" + PreviousInbBest + "\t" + CurrentInbBest);
                                stopCounter = 0;
                                PreviousOutbndBest = BestTotalCost;
                            }
                            if (stopCounter == stopIter)
                            {
                                Console.WriteLine("Basic Outbound -- break at iteration no:" + numIter + "of" + totIter);
                                isTechMove = true;
                                //PickTechnologySelection();
                                //numIter = 1;
                                //techIter++;
                                //if (techIter == MaxTechIter)
                                //    break;
                            }
                            //numIter = TotalNoIterations
                        }

                        if (isTechMove == true || numIter > NoOfIterations - techIter)
                        {
                            UpdateTop5Technologies(BestTotalCost, BestPutawayTechnology, BestPickTechnology);

                            PickTechnologySelection();
                            numIter = 1;
                            techIter = techIter + 1;
                            isTechMove = false;
                            if (techIter == MaxTechIter)
                                break;
                        }

                        numIter++;
                    }            /////End of while loop for outbound iterations

                    CurrentTotalCost = BestTotalCost;         //updating current outbound cost with the best outbound cost
                    currentPickTech = BestPickTechnology;

                    for (int i = 0; i < _OutbBasicSol.Count; i++)       //updating current outbound sol with best outbound sol
                        _OutbBasicSol[i] = BestBasicOutboundSol[i];

                    for (int i = 0; i < currentBasicObSol.Count; i++)       //updating current outbound sol with best outbound sol
                        currentBasicObSol[i] = BestBasicOutboundSol[i];

                    UpdateStoreInventory(_OutbBasicSol);
                    UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
                    CalBasicOutbWeight(_OutbBasicSol, 0);
                    CalculateOutboundCost();    //to update outbound cost components in the listofcosts
                    CalculatePickingCost();
                    CalculateWarehouseInventoryCost();

                    /////////////////////////////////////////code to swap outbound shipments...

                    //totCost = SwapOutboundShipments();
                    counter = 0;
                    while (counter < NumSwapIterations)         //No of iterations to swap...set to 1 for now..
                    {
                        totCost = SwapOutboundShipments();
                        if (totCost < CurrentTotalCost)
                        {
                            Console.WriteLine("% savings with Outbound swap is" + ((CurrentTotalCost - totCost) / CurrentTotalCost) * 100);
                            CurrentTotalCost = totCost;

                            for (int i = 0; i < _OutbBasicSol.Count; i++)
                                BestBasicOutboundSol[i] = _OutbBasicSol[i];

                            for (int i = 0; i < currentBasicObSol.Count; i++)       //updating current outbound sol with best outbound sol
                                currentBasicObSol[i] = BestBasicOutboundSol[i];

                            BestTotalCost = totCost;
                            CurrentTotalCost = totCost;
                        }

                        else
                        {
                            for (int i = 0; i < _OutbBasicSol.Count; i++)
                                _OutbBasicSol[i] = BestBasicOutboundSol[i];
                        }

                        counter++;
                    }

                    UpdateStoreInventory(_OutbBasicSol);
                    UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
                    CalBasicOutbWeight(_OutbBasicSol, 0);
                    CalculateOutboundCost();    //to update outbound cost components in the listofcosts
                    CalculatePickingCost();
                    CalculateWarehouseInventoryCost();

                    BestTotalCost = ListOfCosts.Sum();


                    if (totIter % 5 == 0)
                    {

                        float UpperLimit = PreviousTotalBest + delta * PreviousTotalBest;
                        float LowerLimit = PreviousTotalBest - delta * PreviousTotalBest;
                        if ((LowerLimit <= BestTotalCost) && (UpperLimit >= BestTotalCost))
                        {
                            stopCounterTC++;
                        }
                        else
                        {
                            //Console.WriteLine(numIter + "\t" + PreviousInbBest + "\t" + CurrentInbBest);
                            stopCounterTC = 0;
                            PreviousTotalBest = BestTotalCost;
                        }
                        if (stopCounterTC == stopIter)
                        {
                            Console.WriteLine("Break at Iteration No: " + totIter);
                            break;
                        }
                    }

                    totIter++;
                }
                ///////////////    End of while loop for total iterations

                //Adding best cost and best solutions to each generation lists
                TotalCostGen.Add(BestTotalCost);        //Adding the best total cost in each generation to the total cost list
                BestPutawayTechList.Add(BestPutawayTechnology);
                BestPickTechList.Add(BestPickTechnology);
                GenBestFashionInboundSol.Add(Gensl1);
                GenBestFashionOutboundSol.Add(Gensl2);
                GenBestBasicInboundSol.Add(Gensl3);
                GenBestBasicOutboundSol.Add(Gensl4);
                for (int i = 0; i < BestFashionInboundSol.Count; i++)  //making the initial fashion inbound solution as the best and current inbound fashion sol
                {
                    Gensl1.Add(0);
                    GenBestFashionInboundSol[g][i] = BestFashionInboundSol[i];
                }

                for (int i = 0; i < BestFashionOutboundSol.Count; i++)     //making the initial fashion outbound solution as the best and current fashion outbound sol
                {
                    Gensl2.Add(0);
                    GenBestFashionOutboundSol[g][i] = BestFashionOutboundSol[i];
                }

                for (int i = 0; i < BestBasicInboundSol.Count; i++)        //making the initial basic solution as the best and current basic inbound solution 
                {
                    Gensl3.Add(0);
                    GenBestBasicInboundSol[g][i] = BestBasicInboundSol[i];
                }

                for (int i = 0; i < BestBasicOutboundSol.Count; i++)       //making the initial outbound solution as the best and current basic outbound solution
                {
                    Gensl4.Add(0);
                    GenBestBasicOutboundSol[g][i] = BestBasicOutboundSol[i];
                }
            }   ///////End of total generations

            DateTime EndTime = new DateTime();
            EndTime = DateTime.Now;

            Console.WriteLine("Start Time is " + StartTime);
            Console.WriteLine("End Time is " + EndTime);

            Console.WriteLine("The minimum cost is " + TotalCostGen.Min());
            int index = TotalCostGen.IndexOf(TotalCostGen.Min());

            _InbFashionSol = GenBestFashionInboundSol[index];
            _OutbFashionSol = GenBestFashionOutboundSol[index];
            _InbBasicSol = GenBestBasicInboundSol[index];
            _OutbBasicSol = GenBestBasicOutboundSol[index];

            currentPutawayTech = BestPutawayTechList[index];
            currentPickTech = BestPickTechList[index];

            UpdateStoreFashionInventory(_OutbFashionSol);
            UpdateWhFashionInventory(_InbFashionSol, _OutbFashionSol);
            CalFashionInbCost(_InbFashionSol, 0);
            CalWhFashionInvCost();
            CalStoreFashionInvCost();
            CalFashionOutbWeight(_OutbFashionSol, 0);


            UpdateStoreInventory(_OutbBasicSol);
            UpdateWarehouseInventory(_InbBasicSol, _OutbBasicSol);
            CalculateInboundCost(_InbBasicSol, 0);
            CalculatePutawayCost();
            CalBasicOutbWeight(_OutbBasicSol, 0);
            CalculateOutboundCost();    //to update outbound cost components in the listofcosts
            CalculatePickingCost();

            if (AllParamComb == true)
                twSol = new StreamWriter(ResPath + fileName + ".txt");
            else
                twSol = new StreamWriter("Sol-" + fileName + "PUT" + PUTAWAYRATE + "PICK" + PICKRATE + ".txt");
            PrintSolution();
            twSol.WriteLine("The start time is " + StartTime);
            twSol.WriteLine("The End time is " + EndTime);
            twSol.Close();

            if (AllParamComb == true)
            {
                GenBestFashionInboundSol = new List<List<int>>();
                GenBestFashionOutboundSol = new List<List<long>>();
                GenBestBasicInboundSol = new List<List<int>>();
                GenBestBasicOutboundSol = new List<List<long>>();
                TotalCostGen = new List<float>();
            }
        }

        //End of method Optimize

        public void PrintSolution()
        {
            //printing fashion inbound solution
            twSol.WriteLine("Printing New Fashion Solution:");
            twSol.WriteLine("");
            twSol.WriteLine("Inbound Fashion Solution (V F T):");
            //int count = 0;
            for (int v = 0; v < _numFvendors; v++)
                for (int f = 0; f < _numFashion; f++)
                    for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
                    {
                        if (_InbFashionSol[t * _numFvendors * _numFashion + v * _numFashion + f] > 0)
                            twSol.WriteLine((v + 1) + "\t" + (f + 1) + "\t" + (t + _beginTime + 1) + "\t" + _InbFashionSol[t * _numFvendors * _numFashion + v * _numFashion + f]);

                        //count++;
                    }

            twSol.WriteLine("");
            //printing outbound solution
            twSol.WriteLine("");
            twSol.WriteLine("Outbound Fashion Solution(S F T):");
            //int count1 = 0;
            for (int s = 0; s < _numStores; s++)
                for (int f = 0; f < _numFashion; f++)
                    for (int t = 0; t <= _dueTime - _beginTime - _leadTime[s] - pt; t++)
                    {
                        if (_OutbFashionSol[t * _numFashion * _numStores + f * _numStores + s] > 0)
                            twSol.WriteLine((s + 1) + "\t" + (f + 1) + "\t" + (t + _beginTime + pt + 1) + "\t" + _OutbFashionSol[t * _numFashion * _numStores + f * _numStores + s]);
                        //count1++;
                    }

            twSol.WriteLine("");
            //printing warehouse inventory
            twSol.WriteLine("");
            twSol.WriteLine("Warehouse Fashion Inventory (W F T):");
            for (int w = 0; w < _numWareHouses; w++)
                for (int f = 0; f < _numFashion; f++)
                {
                    twSol.Write(f + 1);
                    for (int t = 0; t <= _dueTime - _beginTime; t++)
                    {
                        twSol.Write("\t" + _WhFashionInv[w][f][t] + "\t");
                    }
                    twSol.WriteLine("");
                }

            twSol.WriteLine("");
            //printing store inventory
            twSol.WriteLine("Store Fashion Inventory (S F T):");
            for (int s = 0; s < _numStores; s++)
            {
                twSol.WriteLine("Store" + (s + 1));
                for (int f = 0; f < _numFashion; f++)
                {
                    twSol.Write((f + 1));
                    for (int t = 0; t <= _dueTime - _beginTime - _leadTime[s] - pt; t++)
                    {
                        twSol.Write("\t" + _StoreFashionInv[s][f][t] + "\t");
                    }
                    twSol.WriteLine("");
                }
            }

            twSol.WriteLine("");
            //printing number of shipments from vendor to warehouse
            twSol.WriteLine("");
            twSol.WriteLine("Fashion Shipments from Vendor to Warehouse (V T):");
            // for (int i = 0; i < InbShipmentList.Count; i++)
            // {
            for (int v = 0; v < _numFvendors; v++)
            {
                for (int t = 0; t <= _dueTime - _beginTime - pt; t++)
                {
                    twSol.Write(_numFashionShipmentsVtoW[t][v] + "\t");
                    //twSol.Write(InbShipmentList[i][t][v] + "\t");
                }
                twSol.WriteLine();
            }
            twSol.WriteLine("");
            //  }
            //printing inbound solution
            twSol.WriteLine("Printing New Basic Solution:");
            twSol.WriteLine("");
            twSol.WriteLine("Inbound Solution (V P T):");
            //int count = 0;
            for (int v = 0; v < _numBvendors; v++)
                for (int p = 0; p < _numBasic; p++)
                    for (int t = 0; t < _numTimePeriods; t++)
                    {
                        if (_InbBasicSol[t * _numBvendors * _numBasic + v * _numBasic + p] > 0)
                            twSol.WriteLine((v + 1) + "\t" + (p + 1) + "\t" + (t + 1) + "\t" + _InbBasicSol[t * _numBvendors * _numBasic + v * _numBasic + p]);

                        //count++;
                    }

            twSol.WriteLine("");
            //printing outbound solution
            twSol.WriteLine("");
            twSol.WriteLine("Outbound Solution(S P T):");
            //int count1 = 0;
            for (int s = 0; s < _numStores; s++)
                for (int p = 0; p < _numBasic; p++)
                    for (int t = 0; t < _numTimePeriods; t++)
                    {
                        if (_OutbBasicSol[t * _numBasic * _numStores + p * _numStores + s] > 0)
                            twSol.WriteLine((s + 1) + "\t" + (p + 1) + "\t" + (t + 1) + "\t" + _OutbBasicSol[t * _numBasic * _numStores + p * _numStores + s]);
                        //count1++;
                    }

            twSol.WriteLine("");
            //printing warehouse inventory
            twSol.WriteLine("");
            twSol.WriteLine("Warehouse Inventory (W P T):");
            for (int w = 0; w < _numWareHouses; w++)
                for (int p = 0; p < _numBasic; p++)
                {
                    twSol.Write(p + 1);
                    for (int t = 0; t <= _numTimePeriods; t++)
                    {
                        twSol.Write("\t" + _WhBasicInv[w][p][t] + "\t");
                    }
                    twSol.WriteLine("");
                }

            twSol.WriteLine("");
            //printing store inventory
            twSol.WriteLine("Store Inventory (S P T):");
            for (int s = 0; s < _numStores; s++)
            {
                twSol.WriteLine("Store" + (s + 1));
                for (int p = 0; p < _numBasic; p++)
                {
                    twSol.Write((p + 1));
                    for (int t = 0; t <= _numTimePeriods; t++)
                    {
                        twSol.Write("\t" + _StoreBasicInv[s][p][t] + "\t");
                    }
                    twSol.WriteLine("");
                }
            }

            twSol.WriteLine("");
            //printing number of shipments from vendor to warehouse
            twSol.WriteLine("");
            twSol.WriteLine("Shipments from Vendor to Warehouse (V T):");
            // for (int i = 0; i < InbShipmentList.Count; i++)
            // {
            for (int v = 0; v < _numBvendors; v++)
            {
                for (int t = 0; t < _numTimePeriods; t++)
                {
                    twSol.Write(_numBasicShipmentsVtoW[t][v] + "\t");
                    //twSol.Write(InbShipmentList[i][t][v] + "\t");
                }
                twSol.WriteLine();
            }
            twSol.WriteLine("");
            //  }


            //printing number of shipments from warehouse to stores
            twSol.WriteLine("Shipments from Warehouse to Stores (S T):");
            for (int s = 0; s < _numStores; s++)
                for (int t = 0; t < _numTimePeriods; t++)
                {
                    if (_numShipmentsWtoS[t][s] > 0)
                        twSol.WriteLine((s + 1) + "\t" + (t + 1) + "\t" + _numShipmentsWtoS[t][s]);
                }

            twSol.WriteLine("");
            //Printing requried putaway workers
            twSol.WriteLine("");
            twSol.WriteLine("Required Putaway workers (Sol T):");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(_numPutaway[t] + "\t");

            twSol.WriteLine("");

            //printing number of pickers
            twSol.WriteLine("");
            twSol.WriteLine("No of required pickers (T):");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(_numPickers[t] + "\t");

            twSol.WriteLine("");

            //printing number of part time putaway workers
            twSol.WriteLine("");
            twSol.WriteLine("No of part-time putaway workers (T):");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(PT_Putaway[t] + "\t");

            twSol.WriteLine("");

            //printing number of part time pickers
            twSol.WriteLine("");
            twSol.WriteLine("No of part-time pickers (T):");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(PT_Pickers[t] + "\t");

            twSol.WriteLine("");

            //printing the best selected technologies for putaway and picking
            twSol.WriteLine("");
            twSol.WriteLine("Best Putaway Technology: " + _putawayRateList[BestPutawayTechnology]);
            twSol.WriteLine("Best Picking Technology: " + _pickRateList[BestPickTechnology]);

            //Printing number of Inbound Fashion putaway Hours
            twSol.WriteLine("");
            twSol.WriteLine("Required Fashion Putaway Hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(FashionPutHrs[t] + "\t");

            twSol.WriteLine("");

            //Printing total number of Inbound putaway Hours
            twSol.WriteLine("");
            twSol.WriteLine("Required Basic Putaway Hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(BasicPutHrs[t] + "\t");

            twSol.WriteLine("");

            //Printing total number of Inbound putaway Hours
            twSol.WriteLine("");
            twSol.WriteLine("Required Total Putaway Hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(TotalInboundPutHrs[t] + "\t");

            twSol.WriteLine("");

            //printing number of fashion Outbound pickHours
            twSol.WriteLine("");
            twSol.WriteLine("No of required fashion pick hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(FashionPickHrs[t] + "\t");

            twSol.WriteLine("");

            //printing number of basic Outbound pick Hours
            twSol.WriteLine("");
            twSol.WriteLine("No of required basic pick hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(BasicPickHrs[t] + "\t");

            twSol.WriteLine("");

            //printing total number of OutboundpickHours
            twSol.WriteLine("");
            twSol.WriteLine("No of required total pick hours:");
            for (int t = 0; t < _numTimePeriods; t++)
                twSol.Write(TotalOutboundPickHrs[t] + "\t");

            ////Printing IC, TC, WC and Total Costs
            //twSol.WriteLine("");
            //twSol.WriteLine("List of inv, trans, warehousing, and total costs: ");
            //for (int c = 0; c < 3; c++)
            //    twSol.Write(ListOfCosts[c] + ListOfCosts[c + 3] + "\t");

            // twSol.Write(ListOfCosts.Sum());
            twSol.WriteLine("");

            //List<string> costs = new List<string>() { "WhBasicInvCost", "BasicInbCost", 
            //    "WhFashionInvCost", "FashionInbCost", "PutawayCost", "StoreBasicInvCost", 
            //    "StoreFashionInvCost", "OutboundCost", "PickingCost", "Total" };
            ////Printing Costs
            //twSol.WriteLine("");
            //twSol.WriteLine("List of all the costs: ");
            //for (int c = 0; c < ListOfCosts.Count; c++)
            //    twSol.WriteLine(costs[c] + "\t" + "\t" + ListOfCosts[c]);

            //twSol.WriteLine(costs[9] + "\t" + "\t" + ListOfCosts.Sum());

            List<string> costs = new List<string>() { "MHECost", "PutawayTechCost", "PickingTechCost", "WhBasicInvCost", "BasicInbCost", 
                "WhFashionInvCost", "FashionInbCost", "PutawayCost", "StoreBasicInvCost", 
                "StoreFashionInvCost", "OutboundCost", "PickingCost", "Total" };
            //Printing Costs
            twSol.WriteLine("");
            twSol.WriteLine("List of all the costs: ");
            for (int c = 0; c < ListOfCosts.Count; c++)
                twSol.WriteLine(costs[c] + "\t" + "\t" + ListOfCosts[c]);

            twSol.WriteLine(costs[12] + "\t" + "\t" + ListOfCosts.Sum());

            List<string> Top5Technologies = new List<string>() { "Tech1", "Tech2", "Tech3", "Tech4", "Tech5" };
            twSol.WriteLine("");
            twSol.WriteLine("List of Top5 Technologies: ");
            for (int t = 0; t < Top5Technologies.Count; t++)
            {
                if(Top5TotalCostList[t] == 0)
                    twSol.WriteLine(Top5Technologies[t] + "\t" + "\t" + "None" + "\t" + "None" + "\t" + "$ " + "-");
                else
                    twSol.WriteLine(Top5Technologies[t] + "\t" + "\t" + (Top5PutawayTechList[t] + 1) + "\t" + (Top5PickTechList[t] + 1) + "\t" + "$ " + Top5TotalCostList[t]);
            }


        }
    }
}










   

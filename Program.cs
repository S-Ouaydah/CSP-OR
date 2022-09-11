using System;
using CSP;
using Google.OrTools.LinearSolver;


internal class CSPOR
{
    static List<int> cutList = new List<int>();
    static List<int> stockList = new List<int>();

    static void addCut(int size)=>cutList.Add(size);
    static void addCut(int size,int num){
		for (int i = 0; i < num; i++)cutList.Add(size);
	}
    static void addCuts(int[] sizes,int[] nums){
		for (var i = 0; i < sizes.Length; i++)
		{
			for (int j = 0;  j< nums[i]; j++)
			{
        	cutList.Add(sizes[i]);							
			}
		}
    } 
	static void addStock(int length)=>stockList.Add(length);
	static void addStock(int length,int num){
		for (var i = 0; i < length; i++)stockList.Add(length);	
	}
    static void addStocks(int[] lenghts,int[] nums)
    {
		for (var i = 0; i < lenghts.Length; i++)
		{
			for (int j = 0;  j< nums[i]; j++)
			{
        	stockList.Add(lenghts[i]);							
			}
		}
    }

    private static void Main(string[] args)
    {
		int[] testSizes = {1,2,3,4,5,7,12};
		int[] testNums =  {5,4,1,3,3,3,1};
		addCuts(testSizes,testNums);
		int[] testLengths = {12};
		int[] testSnums = {20};
		addStocks(testLengths,testSnums);

        DataModel data = new DataModel(cutList,stockList);
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null)
        {
            return;
        }
        //VARS
        //CutVar
        Variable[,] x = new Variable[data.NumCuts, data.NumStocks];
        for(int i=0; i<data.NumCuts; i++)
        {
            for (int j=0; j<data.NumStocks; j++)
            {
                x[i, j] = solver.MakeIntVar(0,1,$"x_{i}_{j}");
            }
        }
        //StockVar
        Variable[] y = new Variable[data.NumStocks];
        for (int j = 0; j < data.NumStocks; j++)
        {
            y[j] = solver.MakeIntVar(0, 1, $"y_{j}");
        }
        //waste
        // Variable[] z = new Variable[data.NumStocks];
        //CONSTRAINTS
        //only in one stock
        for (int i = 0; i < data.NumCuts; i++)
        {
            Constraint constraint = solver.MakeConstraint(1, 1, "");
            for (int j=0; j<data.NumCuts;j++)
            {
                constraint.SetCoefficient(x[i, j], 1);
			}
        }
        //capacity
        for (int j=0; j<data.NumStocks; j++)
        {
            Constraint constraint = solver.MakeConstraint(0, Double.PositiveInfinity,"");
            constraint.SetCoefficient(y[j], data.Stocks[j]);
            for (int i=0; i<data.NumCuts;i++)
            {
                constraint.SetCoefficient(x[i, j], -data.Cuts[i]);
            }
        }
        //OBJECTIVE
        Objective objective = solver.Objective();
        for (int j=0; j<data.NumStocks;j++)
        {
            objective.SetCoefficient(y[j], 1);
        }
        objective.SetMinimization();
        // for (int j=0; j< data.NumStocks;j++)
        // {
        //     for (int i=0; i<data.NumCuts;i++)
        //     {
        //         objective.SetCoefficient(x[i, j], 1);            
        //     }
        // }
        // objective.SetMaximization();

        //SOLVE
        Solver.ResultStatus resultStatus = solver.Solve();

        // Check that the problem has an optimal solution.
        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("The problem does not have an optimal solution!");
            Console.WriteLine(resultStatus.ToString());
            return;
        }
        Console.WriteLine($"Number of Stock used: {solver.Objective().Value()}");
        double TotalWeight = 0.0;
		double TotalWaste = 0.0;
        for (int j = 0; j < data.NumStocks; ++j)
        {
            double StockUsed = 0.0;
            if (y[j].SolutionValue() == 1)
            {
                Console.WriteLine($"CutPlan{j}-{data.Stocks[j]}");
                for (int i = 0; i < data.NumCuts; ++i)
                {
                    if (x[i, j].SolutionValue() == 1)
                    {
                        Console.WriteLine($"Cut{i} size:{data.Cuts[i]}");
                        StockUsed += data.Cuts[i];
                    }
                }
                Console.WriteLine($"Sum cutplan weight: {StockUsed}");
                TotalWeight += StockUsed;
				// data.waste += data.StockLength-StockUsed;
            }
        }
        Console.WriteLine($"Total cuts weight: {TotalWeight}");
    }
}

class DataModel
{
    
    public List<int> Cuts = new List<int>() ;
    public int NumCuts;
	// public int[] allCuts;

    public List<int> Stocks;
    public int NumStocks;
	// public int[] allStocks;


    public DataModel(List<int> cuts, List<int> stocks)
    {
		this.Cuts = cuts;
		this.NumCuts = cuts.Count;
		// this.allCuts =  Enumerable.Range(0, NumCuts).ToArray();
        this.Stocks = stocks;
		this.NumStocks = stocks.Count;
		// this.allStocks =  Enumerable.Range(0, NumStocks).ToArray();

    }
}
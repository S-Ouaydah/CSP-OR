using CSP;
using Google.OrTools.LinearSolver;

internal class SOLVER
{
    static int stockSize ;
    static List<int> cutList = new List<int>();
    static List<int> scrapList = new List<int>();
    static List<int> stockList = new List<int>();
    static List<Pattern> patterns = new List<Pattern>();
 

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
	static void addPrevScrap(int length)=>scrapList.Add(length);
	static void addPrevScrap(int length,int num){
		for (var i = 0; i < length; i++)scrapList.Add(length);	
	}
    static void addPrevScraps(int[] lenghts,int[] nums)
    {
		for (var i = 0; i < lenghts.Length; i++)
		{
			for (int j = 0;  j< nums[i]; j++)
			{
        	scrapList.Add(lenghts[i]);							
			}
		}
    }
    static void addStockSize(int size){
        stockSize = size;
    }

    private static void Main(string[] args)
    {
        //addCuts
        int[] testSizes = {4,5,7,12};
		int[] testNums =  {4,2,3,1};
		addCuts(testSizes,testNums);
        //addPreviousStocks
		int[] testLengths = {4,6,10};
		int[] testSnums = {1,2,2};        
		addPrevScraps(testLengths,testSnums);
        //adddefaultstock
        addStockSize(12);
        //Solve
        MKS();
        binPack();
        organizePatterns();
    }

    private static void organizePatterns(){
        List<Pattern> pats = new List<Pattern>();
        foreach (Pattern pat in patterns)
        {
            if (!pats.Any()){
                pats.Add(pat);
            }
            else{
                bool found = false;
                foreach (Pattern newpat in pats)
                {
                    if (pat.Equals(newpat))
                    {
                        found = true;
                        newpat.count++;
                    }
                }    
                if(!found) pats.Add(pat);
            }
        }
        patterns = pats;
        // Console.WriteLine(string.Join("",patterns));
        
        patterns.ForEach(pat=>pat.printPattern());
    }
    private static void MKS()
    {


        DataModel data = new DataModel(cutList,scrapList);
        Console.WriteLine($"begininn: {cutList.Count}");
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver is null)
        {
            return;
        }

        Variable[,] x = new Variable[data.NumCuts,data.NumStocks];
        for(int i=0; i<data.NumCuts;++i){
            for (int j=0; j<data.NumStocks;++j){
                x[i, j] = solver.MakeBoolVar($"x_{i}_{j}");
            }
        }
        
        for (int i=0 ; i<data.NumCuts;++i)
        {
            Constraint constraint = solver.MakeConstraint(0, 1, "");
            for (int j=0; j<data.NumStocks;++j){
                constraint.SetCoefficient(x[i, j], 1);
			}
        }
        for (int j=0; j<data.NumStocks;++j){
            Constraint constraint = solver.MakeConstraint(0, data.Stocks[j],"");
            for (int i=0; i<data.NumCuts;++i){
                constraint.SetCoefficient(x[i, j], data.Cuts[i]);
            }
        }

        Objective objective = solver.Objective();
        for (int i=0; i<data.NumCuts;++i){
            for (int j=0; j<data.NumStocks;++j){
                objective.SetCoefficient(x[i,j], data.Cuts[i]);
            }
        }
        objective.SetMaximization();
        //SOLVE
        Solver.ResultStatus resultStatus = solver.Solve();

        // Check that the problem has an optimal solution.
        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Console.WriteLine("The problem does not have an optimal solution!");
            Console.WriteLine(resultStatus.ToString());
            return;
        }
        else {
            Console.WriteLine($"Total Cut length: {solver.Objective().Value()}");
            double TotalUtil = 0.0;
            double TotalWaste = 0.0;
            List<int> toBeRemoved = new List<int>();
            for (int b=0; b<data.NumStocks;b++)
            {
                double StockUtil = 0.0;
                Pattern pat = new Pattern(data.Stocks[b]);
                Console.WriteLine("Pattern " + b);
                for (int i=0; i<data.NumCuts; i++)
                {
                    if (x[i, b].SolutionValue() == 1)
                    {
                        Console.WriteLine($"Cut{i} : size:{data.Cuts[i]}");
                        pat.addCut(data.Cuts[i]);
                        StockUtil += data.Cuts[i];
                    }
                }
                Console.WriteLine("Pattern uses: " + StockUtil);
                patterns.Add(pat);
                TotalUtil += StockUtil;
                TotalWaste += data.Stocks[b]-StockUtil;
                toBeRemoved.AddRange(pat.cuts);
            }
        Console.WriteLine("Total Stock used: " + TotalUtil);
            
        foreach (int rm in toBeRemoved)
        {
            int last = cutList.LastIndexOf(rm);
            cutList.RemoveAt(last);
        }
        Console.WriteLine($"final: {cutList.Count}");

        }
    }

    static void binPack(){
        // int[] testSizes = {1,2,3,4,5,7,12};
		// int[] testNums =  {5,4,1,3,3,3,1};
		// addCuts(testSizes,testNums);
		// int[] testLengths = {12};
		// int[] testSnums = {20};
		// addPrevScraps(testLengths,testSnums);
        foreach (var cut in cutList) stockList.Add(stockSize); 
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
        Variable[] w = new Variable[data.NumStocks];
        for (var i = 0; i < data.NumStocks; i++)
        {
            // int length = 0;
            // for (var j = 0; j < data.NumCuts; j++)
            // {
            //     length += 
            // }
            w[i] = solver.MakeIntVar(0,data.Stocks[i],$"w_{i}");
        }
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
        for (int j=0; j<data.NumStocks; j++){
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
		// double TotalWaste = 0.0;
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
	// public inti<[] NumCuts;++i;

    public List<int> Stocks;
    public int NumStocks;
	// public ij<nt[NumStocks;++j] ;


    public DataModel(List<int> cuts, List<int> stocks)
    {
		this.Cuts = cuts;
		this.NumCuts = cuts.Count;
		// i<this.NumCuts;++i =  Enumerable.Range(0, NumCuts).ToArray();
        this.Stocks = stocks;
		this.NumStocks = stocks.Count;
		// j<this.NumStocks;++j =  Enumerable.Range(0, NumStocks).ToArray();

    }
}
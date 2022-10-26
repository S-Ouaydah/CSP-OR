/*
    new Solver
    .addCuts(int[] sizes,int[] numbers) || .addCut(int size,int num)
    .addPrevScraps(int[] lenghts,int[] nums) || .addPrevScraps(int lenght,int num)
    .setStockSize(int size)
    .solve()
*/
    Solver solver = new Solver();
    
    //addCuts
    int[] testSizes = {12,4,5,7};
    int[] testNums =  {1,4,2,3};
    solver.addCuts(testSizes,testNums);
    
    //addPreviousStocks
    int[] testLengths = {4,6,10};
    int[] testSnums = {1,2,2};        
    solver.addPrevScraps(testLengths,testSnums);
    //adddefaultstock
    solver.setStockSize(12);
    
    //Solve
    solver.solve();


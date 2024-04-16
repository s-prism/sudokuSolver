using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace sudokuSolver
{
    class Sudoku
    {
        private string[,] solutionGrid = new string[9, 9]; //Contains the Sudoku grid being solved
        private string[,] afterReasoningGrid= new string[9, 9]; //The resulting Sudoku grid after only reasoning has been applied
        private NakedSubsets nakedSubsetFinder = new NakedSubsets(); 
        private HiddenSubsets hiddenSubsetFinder = new HiddenSubsets();
        private PointingSubsets pointingSubsetFinder = new PointingSubsets();
        private Backtracking backtracker = new Backtracking();
        public Sudoku()
        {
            //Create Sudoku object
        }
        public (string[,],string) Solve(string[,] grid) 
        //Only intended to find the first solution, if any, of a Sudoku
        {
            string validityMessage = GridCorrectlyFilled(grid); 
            //Reasoning
            if (validityMessage == "True")
            {
                CopyGrid(grid);
                validityMessage = ReasoningSolver();
                PopulateAfterReasoningGrid(); //If this grid is filled, the Sudoku was solved using only reasoning
            }
            //If the Sudoku grid was already filled by the user, simply tell them if it is valid or not 
            if (GridSolved(grid))
            {
                if (validityMessage == "True")
                {
                    validityMessage = "Sudoku valid";
                }
                else
                {
                    validityMessage = "The Sudoku entered is invalid";
                }
            }
            //Backtracking
            if (!GridSolved(grid) && validityMessage == "True")
            {
                validityMessage =BacktrackingSolver();
            }
            return (solutionGrid,validityMessage);
        }
        public (string[,],string) RunOtherSolutionFinder() 
        //Only intended to be used to find more than 1 solution, or determine that there is only 1 solution
        {
            bool valid = true;
            //If the Sudoku was solved using pure reasoning, the solution is definitely unique
            if (GridSolved(afterReasoningGrid))
            {
                return (solutionGrid, "Solution found using only reasoning");
            }
            //Else, backtracking was used as well, so run the backtracker again to check for more solutions
            (solutionGrid, valid) = backtracker.RunAgain(solutionGrid,afterReasoningGrid);
            if (valid)
            {
                return (solutionGrid, "True");
            }
            return (solutionGrid, "False");
        }
        private string ReasoningSolver() 
        //Runs the reasoning solvers
        {
            string[,] prevSolutionGrid = new string[9, 9];
            bool valid = true;
            (solutionGrid,valid) = nakedSubsetFinder.Run(solutionGrid, true);

            while (valid && !GridsEqual(prevSolutionGrid, solutionGrid))
            { 
                Array.Copy(solutionGrid, prevSolutionGrid, 81);

                (solutionGrid, valid) = nakedSubsetFinder.Run(solutionGrid);
                //The more complex methods are only executed if the methods above it did not make any changes
                if (valid && GridsEqual(prevSolutionGrid, solutionGrid))
                {
                    (solutionGrid, valid) = hiddenSubsetFinder.Run(solutionGrid);
                }
                if (valid && GridsEqual(prevSolutionGrid, solutionGrid))
                {
                    (solutionGrid, valid) = pointingSubsetFinder.Run(solutionGrid);
                } 
            }

            if (!valid)
            {
                return "The Sudoku entered is invalid (has no solutions)";
            }
            return "True";
        }
        private string BacktrackingSolver()
        //Runs the backtracking solver
        {
            bool valid = true;
            (solutionGrid,valid) = backtracker.Run(solutionGrid);

            if (!valid)
            {
                return "The Sudoku entered is invalid (has no solutions)";
            }
            return "True";
        }
        private void CopyGrid(string[,] grid)
        //Copies the grid passed into the subroutine into solutionGrid
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i, j] == "")
                    {
                        solutionGrid[i, j] = "0";
                    }
                    else
                    {
                        solutionGrid[i, j] = grid[i, j];
                    }
                }
            }
        }
        private string GridCorrectlyFilled(string[,] grid) 
        {
            if (!EnoughEntries(grid))
            {
                return "Not enough entries: at least 17 numbers must be entered into the grid";
            }
            return "True";
        }
        private bool EnoughEntries(string[,] grid)
        //Checks if the grid has enough (17 or more) entries
        {
            int entries = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i,j] != "")
                    {
                        entries += 1;
                    }
                }
            }
            return entries >= 17;
        }
        private bool GridsEqual(string[,] grid1, string[,] grid2)
        {
            for (int i=0;i<9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid1[i, j] != grid2[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool GridSolved(string[,] grid)
        {
            foreach (string entry in grid)
            {
                if (entry=="" || entry=="0" || entry.Length > 1)
                {
                    return false;
                }
            }
            return true;
        }
        private void PopulateAfterReasoningGrid() //if this is incomplete, there may be multiple backtracking solutions
        {
            if (solutionGrid!=new string[9, 9])
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (solutionGrid[i,j] == "" || solutionGrid[i, j].Length > 1)
                        {
                            afterReasoningGrid[i, j] = "0";
                        }
                        else
                        {
                            afterReasoningGrid[i, j] = solutionGrid[i, j];
                        }
                    }
                }
            }
        } 
    }
    //Superclass - all the reasoning algorithms used inherit from this
    abstract class Reasoning 
    {
        protected string[,] sudokuGrid = new string[9, 9]; //Contains the Sudoku grid being solved
        protected int[,] sameSquare = new int[9, 9]; //Records what cells are in each 3x3 square of the sudokuGrid
        public abstract (string[,], bool) Run(string[,] grid, bool firstIteration); //Forces the reasoning algorithms to implement this method
        protected void BeforeSolving(string[,] grid)
        //Before the solving algorithms run, the methods below must be executed
        {
            Array.Copy(grid, sudokuGrid, 81);
            FillSameSquareArray();
        }
        private void FillSameSquareArray()
        //Allocate a square to each cell RC and place the cell in sameSquare[square number, first empty space]
        {
            int squareNum;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    squareNum = GetSquareNum(r, c);
                    bool spaceNotEmpty = true;
                    //Look through sameSquare[square number] until an empty space is found
                    for (int i = 0; i < 9 && spaceNotEmpty; i++)
                    {
                        //If the space is empty but is not row 0 column 0, then it can be filled
                        if (sameSquare[squareNum, i] == 0 && !(squareNum == 0 && i == 0))
                        {
                            sameSquare[squareNum, i] = r * 10 + c;
                            spaceNotEmpty = false;
                        }
                    }
                }
            }
        }
        protected bool InSameSquare(int a, int b)
        {
            int rowA = a / 10;
            int colA = a % 10;
            int rowB = b / 10;
            int colB = b % 10;
            return GetSquareNum(rowA, colA) == GetSquareNum(rowB, colB);
        }
        protected int GetSquareNum(int i, int j)
        {
            return (i / 3) * 3 + (j / 3);
        }
        private (int, int) FindCell(int i, int j, string type)
        /* Finds a cell in sudokuGrid depending on whether a row, column, or square checker is using the function.
        This is useful because the same checker can be used for row, column, and square functions in most cases.
        Only type would need to be changed. */
        {
            (int r, int c) = (0, 0); //If type is invalid, this default is returned
            if (type == "row")
            {
                r = i;
                c = j;
            }
            else if (type == "column")
            {
                c = i;
                r = j;
            }
            else if (type == "square")
            {
                r = sameSquare[i, j] / 10;
                c = sameSquare[i, j] % 10;
            }
            return (r, c);
        }
        protected string GetCell(int i, int j, string type)
        /* Returns the contents of a cell in sudokuGrid with location pointers i and j, 
        where the cell returned depends on whether a row, column, or square checker is using the function */
        {
            (int r, int c) = FindCell(i, j, type);
            return sudokuGrid[r, c];
        }
        protected void SetCell(string newContent, int i, int j, string type)
        /* Changes the contents of a cell in sudokuGrid (with location pointers i and j) to newContent, 
        where the cell changed depends on whether a row, column, or square checker is using the function */
        {
            (int r, int c) = FindCell(i, j, type);
            sudokuGrid[r, c] = newContent;
        }
        protected bool GridsEqual(string[,] grid1, string[,] grid2)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid1[i, j] != grid2[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
    class NakedSubsets : Reasoning 
    {
        /* Subclass has access to:
        string[,] sudokuGrid
        int[,] sameSquare
        BeforeSolving(grid)
        InSameSquare(index1,index2)
        GetSquareNum(r,c) 
        GetCell(i,j,type)
        SetCell(newContent,i,j,type)
        GridsEqual(grid1,grid2) */
        public NakedSubsets()
        {

        }
        public override (string[,], bool) Run(string[,] grid, bool firstIteration = false) 
        //Runs an algorithm finding Naked Subsets
        {
            BeforeSolving(grid);
            bool sudokuValid = true;
            if (firstIteration)
            {
                sudokuValid = RowInitialChecker();
            }
            string[] typeToCheck = { "column", "square", "row" };
            foreach (string type in typeToCheck)
            {
                if (sudokuValid)
                {
                    sudokuValid = SingleChecker(type);
                }
            }
            if (GridsEqual(grid, sudokuGrid))
            {
                foreach (string type in typeToCheck)
                {
                    if (sudokuValid)
                    {
                        sudokuValid = SubsetChecker(type);
                    }
                }
            }
            return (sudokuGrid, sudokuValid);
        }
        private bool RowInitialChecker()
        /* Only runs in the first iteration of NakedSubsets.
        Fills each empty cell with a group of numbers that the cell could contain. */
        {
            string missingNums;
            for (int r = 0; r < 9; r++)
            {
                //What numbers are not yet in the row? Use the string missingNums to document this.
                missingNums = "123456789";
                string currentCell;
                for (int c = 0; c < 9; c++)
                {
                    currentCell = GetCell(r, c, "row");
                    if (currentCell != "0")
                    {
                        missingNums = missingNums.Replace(currentCell, "");
                    }
                }
                //Add missingNums to all the empty spaces in each row
                for (int c = 0; c < 9; c++)
                {
                    currentCell = GetCell(r, c, "row");
                    if (currentCell == "0")
                    {
                        SetCell(missingNums, r, c, "row");
                        //!! Is the Sudoku invalid: checking that the current cell is not empty
                        if (GetCell(r, c, "row") == "")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool SingleChecker(string type)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //Get the current cell entry
                    string currentCell = GetCell(i, j, type);
                    //!! Is the Sudoku invalid: checking that the current cell is not empty
                    if (currentCell == "")
                    {
                        return false;
                    }
                    //If the currentCell only has 1 entry, remove this number from every other cell in the row/column/square
                    else if (currentCell.Length == 1)
                    {
                        string newContent;
                        /*There are 2 for loops doing the same thing, which skip cell j,
                        so that the currentCell (i,j) is not emptied */
                        for (int k = 0; k < j; k++)
                        {
                            newContent = GetCell(i, k, type).Replace(currentCell, "");
                            SetCell(newContent, i, k, type);
                            //!! Is the Sudoku invalid: checking that the current cell is not empty
                            if (GetCell(i, k, type) == "")
                            {
                                return false;
                            }
                        }
                        for (int k = j + 1; k < 9; k++)
                        {
                            newContent = GetCell(i, k, type).Replace(currentCell, "");
                            SetCell(newContent, i, k, type);
                            //!! Is the Sudoku invalid: checking that the current cell is not empty
                            if (GetCell(i, k, type) == "")
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        private bool SubsetChecker(string type)
        {
            bool sudokuValid = true;
            for (int i = 0; i < 9; i++)
            {
                // Part 1: For each cell with 2/3/4 nums, how many other cells have the same contents as it?
                Queue<string> cellsInType = new Queue<string>();
                for (int j = 0; j < 9; j++)
                {
                    string currentCell = GetCell(i, j, type);
                    int length = currentCell.Length;
                    if (length > 1 && length < 5)
                    {
                        cellsInType.Enqueue(currentCell);
                    }
                }
                (cellsInType, sudokuValid) = FindSubsets(i, cellsInType, type);
                //!! Program terminates if the Sudoku is invalid
                if (!sudokuValid)
                {
                    return false;
                }

                //Part 2: combining cell entries to form + find potential subsets
                int counter = cellsInType.Count;
                while (counter > 0)
                {
                    string tempCellItem = cellsInType.Dequeue();
                    if (tempCellItem.Length < 4)
                    {
                        cellsInType.Enqueue(tempCellItem);
                    }
                    counter--;
                }
                Queue<string> potentialSubsets = new Queue<string>();
                while (cellsInType.Count > 1)
                {
                    /*If we try to combine 2 groups together, the size of the new group must be 4 or less.
                    Find numbers that do not overlap between 2 groups. */
                    string tempGroup = cellsInType.Dequeue();
                    string comparisonGroup = cellsInType.Peek();
                    string addToTempGroup = "";
                    foreach (char num in comparisonGroup)
                    {
                        if (!tempGroup.Contains(num))
                        {
                            addToTempGroup += num;
                        }
                    }
                    int length1 = tempGroup.Length;
                    int length2 = comparisonGroup.Length;
                    /* Case 1: If both groups are of length 2, combining their entries will always give you a length <=4,
                    so add the combined group to the queue of potential subsets, unless there is full overlap.
                    Full overlap would give you a group that has already been tested */
                    if (length1 == 2 && length2 == 2 && addToTempGroup != "")
                    {
                        tempGroup += addToTempGroup;
                        potentialSubsets.Enqueue(tempGroup);
                    }
                    /* Case 2: If the first group is of length 2, 
                    and 2 numbers need to be added to it to create a new potential group, add it to the queue, 
                    as it will be of length 4, and will not be equivalent to tempGroup or comparisonGroup */
                    else if (length1 == 2  && length2 == 3 && addToTempGroup.Length == 2)
                    {
                        tempGroup += addToTempGroup;
                        potentialSubsets.Enqueue(tempGroup);
                    }
                    /*Case 3: If the first group has 3 numbers, and 1 number needs to be added to it,
                    add it to the queue, as it will be of length 4, and will not be equivalent to tempGroup or comparisonGroup */
                    else if (length1 == 3 && (length2 == 2 || length2 ==3 ) && addToTempGroup.Length == 1)
                    {
                        tempGroup += addToTempGroup;
                        potentialSubsets.Enqueue(tempGroup);
                    }
                }
                (_, sudokuValid) = FindSubsets(i, potentialSubsets, type);
                //!! Program terminates if the Sudoku is invalid
                if (!sudokuValid)
                {
                    return false;
                }
            }
            return true;
        }
        private (Queue<string>, bool) FindSubsets(int i, Queue<string> groups, string type)
        {
            Queue<string> leftoverGroups = new Queue<string>();
            bool sudokuValid = true;
            while (groups.Count > 0)
            {
                string currentGroup = groups.Dequeue();
                List<int> notInGroupIndex = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                int GroupSize = 0;
                for (int j = 0; j < 9; j++)
                {
                    //If the cell is empty after removing the characters in currentGroup from it, it is part of the group
                    string currentCell = GetCell(i, j, type);
                    foreach (char z in currentGroup)
                    {
                        currentCell = currentCell.Replace(z.ToString(), "");
                    }
                    if (currentCell == "")
                    {
                        notInGroupIndex.Remove(j);
                        GroupSize++;
                    }
                }
                if (GroupSize == currentGroup.Length)
                {
                    /* The current group is a valid subset.
                    Remove the subset from every cell that is notInGroupIndex */
                    foreach (int j in notInGroupIndex)
                    {
                        foreach (char z in currentGroup)
                        {
                            string currentCell = GetCell(i, j, type);
                            string newContent = currentCell.Replace(z.ToString(), "");
                            SetCell(newContent, i, j, type);
                        }

                    }
                }
                else if (GroupSize > currentGroup.Length)
                {
                    //!! Sudoku must be invalid
                    sudokuValid = false;
                }
                else
                {
                    leftoverGroups.Enqueue(currentGroup);
                }
            }
            return (leftoverGroups, sudokuValid);
        }
    }
    class HiddenSubsets : Reasoning 
    {
        /* Subclass has access to:
        string[,] sudokuGrid
        int[,] sameSquare
        BeforeSolving(grid)
        InSameSquare(index1,index2)
        GetSquareNum(r,c) 
        GetCurrentCell(i,j,type)
        SetCurrentCell(newContent,i,j,type)
        GridsEqual(grid1,grid2) */
        public HiddenSubsets()
        {
            //Create HiddenSubsets solver
        }
        public override (string[,], bool) Run(string[,] grid, bool _ = false) 
        //Runs an algorithm finding Hidden Subsets
        {
            BeforeSolving(grid);
            bool sudokuValid = true;

            //Run the Single checker first, as it is less complex to execute
            SingleChecker("row");
            SingleChecker("column");
            SingleChecker("square");

            //If no changes were made after running the Single checker, run the Subset checker
            string[] typeToCheck = { "row", "column", "square" };
            if (GridsEqual(grid, sudokuGrid))
            {
                foreach (string type in typeToCheck)
                {
                    if (sudokuValid)
                    {
                        sudokuValid = SubsetChecker(type);
                    }
                }
            }
            return (sudokuGrid, sudokuValid);
        }
        private void SingleChecker(string type)
        {
            for (int i = 0; i < 9; i++)
            {
                /* Contents of dictionary numOccurrence: 
                    * key = numbers 1-9
                    * value = index where the key/number occurs (0-8)
                    * if value=-1, the number does not occur anywhere
                    * if value=9, the number occurs more than once */
                Dictionary<int, int> numOccurrence = new Dictionary<int, int>();
                for (int n = 1; n < 10; n++)
                {
                    numOccurrence.Add(n, -1);
                }
                //Is there a number 1 - 9 within a cell that only features once in the whole row/column/square?
                for (int j = 0; j < 9; j++)
                {
                    string currentCell = GetCell(i, j, type);
                    foreach (char num in currentCell)
                    {
                        int currentNum = (int)char.GetNumericValue(num);
                        if (numOccurrence[currentNum] == -1)
                        {
                            numOccurrence[currentNum] = j;
                        }
                        else if (numOccurrence[currentNum] != 9)
                        {
                            numOccurrence[currentNum] = 9;
                        }
                    }
                }
                //Search numOccurence to check if any number only occurs once in the whole row/column/square
                for (int n = 1; n < 10; n++)
                {
                    //If the number n only occurs once in the whole row/column/square
                    if (numOccurrence[n] > -1 && numOccurrence[n] < 9)
                    {
                        string newContent = n.ToString();
                        //The place where n occurred is cell (i,j)
                        int j = numOccurrence[n];
                        /* Every other number from cell(i, j) can be removed,
                        hence the cell(i, j)'s content becomes n */
                        SetCell(newContent, i, j, type);
                    }
                }
            }
        }

        private bool SubsetChecker(string type)
        {
            bool sudokuValid = true;
            for (int i = 0; i < 9; i++)
            {
                //Part 1: find the indexes where each number occurs
                string[] locationOfNums = new string[9];
                for (int j = 0; j < 9; j++)
                {
                    foreach (char num in GetCell(i, j, type))
                    {
                        int currentNum = (int)char.GetNumericValue(num);
                        locationOfNums[currentNum - 1] += j.ToString();
                    }
                }

                // Part 2: For each number occurring 2/3/4 times, do the cell locations of this number form a hidden group?
                Queue<string> cellsInType = new Queue<string>();
                for (int l = 0; l < 9; l++)
                {
                    int length = locationOfNums[l].Length;
                    if (length > 1 && length < 5)
                    {
                        cellsInType.Enqueue(locationOfNums[l]);
                    }
                }
                (cellsInType, sudokuValid) = FindSubsets(i, cellsInType, locationOfNums, type);
                //!! Program terminates if the Sudoku is invalid
                if (!sudokuValid)
                {
                    return false;
                }

                //Part 3: combining cell location groups to form + find potential subsets
                int counter = cellsInType.Count;
                while (counter > 0)
                {
                    string tempCellItem = cellsInType.Dequeue();
                    if (tempCellItem.Length < 4)
                    {
                        cellsInType.Enqueue(tempCellItem);
                    }
                    counter--;
                }
                Queue<string> potentialGroups = new Queue<string>();
                while (cellsInType.Count > 1)
                {
                    /*If we try to combine 2 groups together, the size of the new group must be 4 or less.
                    Find numbers that do not overlap between 2 groups. */
                    string tempGroup = cellsInType.Dequeue();
                    string comparisonGroup = cellsInType.Peek();
                    string addToTempGroup = "";
                    foreach (char num in comparisonGroup)
                    {
                        if (!tempGroup.Contains(num))
                        {
                            addToTempGroup += num;
                        }
                    }
                    int length1 = tempGroup.Length;
                    int length2 = comparisonGroup.Length;
                    /* Case 1: If both groups are of length 2, combining their entries will always give you a length <=4,
                    so add the combined group to the queue of potential subsets, unless there is full overlap.
                    Full overlap would give you a group that has already been tested */
                    if (length1 == 2 && length2 == 2 && addToTempGroup != "")
                    {
                        tempGroup += addToTempGroup;
                        potentialGroups.Enqueue(tempGroup);
                    }
                    /* Case 2: If the first group is of length 2, 
                    and 2 numbers need to be added to it to create a new potential group, add it to the queue, 
                    as it will be of length 4, and will not be equivalent to tempGroup or comparisonGroup */
                    else if (length1 == 2 && length2 == 3 && addToTempGroup.Length == 2)
                    {
                        tempGroup += addToTempGroup;
                        potentialGroups.Enqueue(tempGroup);
                    }
                    /*Case 3: If the first group has 3 numbers, and 1 number needs to be added to it,
                    add it to the queue, as it will be of length 4, and will not be equivalent to tempGroup or comparisonGroup */
                    else if (length1 == 3 && (length2 == 2 || length2 == 3) && addToTempGroup.Length == 1)
                    {
                        tempGroup += addToTempGroup;
                        potentialGroups.Enqueue(tempGroup);
                    }
                }
                (_, sudokuValid) = FindSubsets(i, potentialGroups, locationOfNums, type);
                //!! Program terminates if the Sudoku is invalid
                if (!sudokuValid)
                {
                    return false;
                }
            }
            return true;
        }
        private (Queue<string>, bool) FindSubsets(int i, Queue<string> groups, string[] locationOfNums, string type)
        {
            Queue<string> leftoverGroups = new Queue<string>();
            bool sudokuValid = true;
            while (groups.Count > 0)
            {
                string currentGroupIndexes = groups.Dequeue();
                List<string> inGroup = new List<string>();
                List<string> notInGroup = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                for (int n = 1; n < 10; n++)
                {
                    //A number n will occur in certain locations, which can be identified by the locations' indexes
                    string currentNumindexes = locationOfNums[n - 1];
                    foreach (char z in currentGroupIndexes)
                    {
                        currentNumindexes = currentNumindexes.Replace(z.ToString(), "");
                    }
                    /* If n's group of indexes is empty after removing the indexes in currentGroupIndexes from it,
                    then it is part of the hidden group */
                    if (currentNumindexes == "")
                    {
                        inGroup.Add(n.ToString());
                        notInGroup.Remove(n.ToString());
                    }
                }
                if (inGroup.Count == currentGroupIndexes.Length)
                {
                    /* The current index group contains a valid subset.
                    From every cell in currentGroupIndexes, remove every number that is not in the found subset */
                    foreach (char charj in currentGroupIndexes)
                    {
                        int j = int.Parse(charj.ToString());
                        foreach (string n in notInGroup)
                        {
                            string currentCell = GetCell(i, j, type);
                            string newContent = currentCell.Replace(n, "");
                            SetCell(newContent, i, j, type);
                        }
                    }
                }
                else if (inGroup.Count > currentGroupIndexes.Length)
                {
                    //!! Sudoku must be invalid
                    sudokuValid = false;
                }
                else
                {
                    leftoverGroups.Enqueue(currentGroupIndexes);
                }
            }
            return (leftoverGroups, sudokuValid);
        }
    }
    class PointingSubsets : Reasoning 
    {
        /* If a number appears in only 1 square in a row/column, or in only 1 row/column of a square,
        the number must occur in the overlap between the square and row/column,
        and all other instances of the number in the square and row/column can be removed.

        Subclass has access to:
        string[,] sudokuGrid
        int[,] sameSquare
        BeforeSolving(grid)
        InSameSquare(index1,index2)
        GetSquareNum(r,c)
        GetCurrentCell(i,j,type)
        SetCurrentCell(newContent,i,j,type)
        GridsEqual(grid1,grid2)*/
        public PointingSubsets()
        {

        }
        public override (string[,], bool) Run(string[,] grid, bool _ = false) 
        //Runs an algorithm finding Pointing Subsets
        {
            BeforeSolving(grid);
            bool sudokuValid = true;
            if (sudokuValid)
            {
                sudokuValid = PointingNumsRowOrColumn("row");
            }
            if (sudokuValid)
            {
                sudokuValid = PointingNumsRowOrColumn("column");
            }
            if (sudokuValid)
            {
                sudokuValid = PointingNumsSquare();
            }
            return (sudokuGrid, sudokuValid);
        }
        private bool PointingNumsRowOrColumn(string type)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int n = 1; n < 10; n++)
                {
                    //How many times does a particular number n occur in the row/column?
                    int[] indexes = new int[3];
                    int counter = 0;
                    bool lessthan4OfN = true;
                    for (int j = 0; j < 9 && lessthan4OfN; j++)
                    {
                        if (GetCell(i, j, type).Contains(n.ToString()))
                        {
                            if (counter < 3)
                            {
                                indexes[counter] = i * 10 + j;
                                counter += 1;
                            }
                            else
                            {
                                counter = 4;
                                lessthan4OfN = false;
                            }
                        }
                    }
                    /* If the number occurs 2 or 3 times (in the row/column),
                    and all the instances of the number are in the same square,
                    it is a pointing number */
                    if (counter == 2 || counter == 3)
                    {
                        bool pointingNum = true;
                        for (int x = 1; x < counter; x++)
                        {
                            if (!InSameSquare(indexes[0], indexes[x]))
                            {
                                pointingNum = false;
                            }
                        }
                        /*If n is a pointing number (occurs in only 1 square within a row/column)
                        remove n from every cell in the square that is not in the row/column (overlap) */
                        if (pointingNum)
                        {
                            int squareNum = GetSquareNum(indexes[0] / 10, indexes[0] % 10);
                            for (int x = 0; x < 9; x++)
                            {
                                int elementI = sameSquare[squareNum, x] / 10;
                                if (elementI != i)
                                {
                                    int elementJ = sameSquare[squareNum, x] % 10;
                                    string newContent = GetCell(elementI, elementJ, type).Replace(n.ToString(), "");
                                    SetCell(newContent, elementI, elementJ, type);
                                    //!! Is the Sudoku invalid: checking that the current cell is not empty
                                    if (GetCell(elementI, elementJ, type) == "")
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private bool PointingNumsSquare()
        {
            for (int s = 0; s < 9; s++)
            {
                for (int n = 1; n < 10; n++)
                {
                    //How many times does a particular number n occur in the square s?
                    int[] indexes = new int[3];
                    int counter = 0;
                    bool lessthan4 = true;
                    for (int i = 0; i < 9 && lessthan4; i++)
                    {
                        string currentCell = GetCell(s, i, "square");
                        if (currentCell.Contains(n.ToString()))
                        {
                            if (counter < 3)
                            {
                                indexes[counter] = sameSquare[s, i];
                                counter += 1;
                            }
                            else
                            {
                                counter = 4;
                                lessthan4 = false;
                            }
                        }
                    }
                    /* If the number occurs 2 or 3 times (in the square),
                    and all the instances of the number are in the same row,
                    it is a pointing row number */
                    if (counter == 2 || counter == 3)
                    {
                        bool pointingRowNum = true;
                        for (int j = 1; j < counter; j++)
                        {
                            //If n occurs in different rows in a square, it is not a pointing row number
                            if (indexes[0] / 10 != indexes[j] / 10)
                            {
                                pointingRowNum = false;
                            }
                        }
                        /*If n is a pointing row number (occurs in only 1 row within a square)
                        remove n from every cell in the row that is not in the square/overlap */
                        if (pointingRowNum)
                        {
                            int row = indexes[0] / 10;
                            for (int c = 0; c < 9; c++)
                            {
                                if (!InSameSquare(row * 10 + c, indexes[0]))
                                {
                                    string newContent = GetCell(row, c, "row").Replace(n.ToString(), "");
                                    SetCell(newContent, row, c, "row");
                                    //!! Is the Sudoku invalid: checking that the current cell is not empty
                                    if (GetCell(row, c, "row") == "")
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        /* There is only a chance of n only occurring in 1 column if n did not only occur in 1 row.
                        Hence, there is only a chance of n being a pointingColNum if n was not a pointingRowNum */
                        else
                        {
                            bool pointingColNums = true;
                            for (int j = 1; j < counter; j++)
                            {
                                //If n occurs in different columns in a square, it is not a pointing column number
                                if (indexes[0] % 10 != indexes[j] % 10)
                                {
                                    pointingColNums = false;
                                }
                            }
                            /*If n is a pointing column number (occurs in only 1 column within a square)
                            remove n from every cell in the column that is not in the square/overlap */
                            if (pointingColNums)
                            {
                                int col = indexes[0] % 10;
                                for (int r = 0; r < 9; r++)
                                {
                                    if (!InSameSquare(r * 10 + col, indexes[0]))
                                    {
                                        string newContent = GetCell(col, r, "column").Replace(n.ToString(), "");
                                        SetCell(newContent, col, r, "column");
                                        //!! Is the Sudoku invalid: checking that the current cell is not empty
                                        if (GetCell(col, r, "column") == "")
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
    class Graph
    {
        private int[,] adjacencyMatrix = new int[81, 81]; //Stores the graph representing a 9x9 Sudoku grid
        public Graph()
        /* Create Graph and fill adjacencyMatrix, where 1 represents a connection, and 0 represents no connection.
        A connection is made if 2 cells are in the same row, column, or (inclusive) square. */
        {
            for (int i = 0; i < 81; i++)
            {
                for (int j = i + 1; j < 81; j++) //j starts at i+1 because any cell is not connected to itself
                {
                    int rowI = i / 9;
                    int rowJ = j / 9;
                    int colI = i % 9;
                    int colJ = j % 9;
                    int sqI = (rowI / 3) * 3 + (colI / 3);
                    int sqJ = (rowJ / 3) * 3 + (colJ / 3);
                    if (rowI == rowJ || colI == colJ || sqI == sqJ)
                    {
                        adjacencyMatrix[i, j] = 1;
                        adjacencyMatrix[j, i] = 1;
                    }
                }
            }
        }
        public bool IsSudokuValid(byte[,] sudokuGrid, int cellNo = 81)
        //Checks if sudokuGrid is valid, or if a cellNo is specified, checks if a particular cell contains a valid number
        {
            if (cellNo >= 0 && cellNo <= 80)
            {
                int rowI = cellNo / 9;
                int colI = cellNo % 9;
                for (int j = 0; j < 81; j++)
                {
                    int rowJ = j / 9;
                    int colJ = j % 9;
                    //If 2 cells are connected and contain the same non-0 number, the Sudoku is invalid
                    if (adjacencyMatrix[cellNo, j] == 1 && sudokuGrid[rowI, colI] == sudokuGrid[rowJ, colJ] && sudokuGrid[rowI, colI] != 0)
                    {
                        return false;
                    }
                    //If a cell contains a number that is not 0-9, the Sudoku is invalid
                    else if (sudokuGrid[rowI, colI] / 10 != 0 || sudokuGrid[rowJ, colJ] / 10 != 0)
                    {
                        return false;
                    }
                }
            }
            else if (cellNo == 81) //This default cellNo causes the program to check the entire grid for validity
            {
                for (int i = 0; i < 81; i++)
                {
                    for (int j = i + 1; j < 81; j++)
                    {
                        int rowI = i / 9;
                        int rowJ = j / 9;
                        int colI = i % 9;
                        int colJ = j % 9;
                        //If 2 cells are connected and contain the same non-0 number, the Sudoku is invalid
                        if (adjacencyMatrix[i, j] == 1 && sudokuGrid[rowI, colI] == sudokuGrid[rowJ, colJ] && sudokuGrid[rowI, colI] != 0)
                        {
                            return false;
                        }
                        //If a cell contains a number that is not 0-9, the Sudoku is invalid
                        else if (sudokuGrid[rowI, colI] / 10 != 0 || sudokuGrid[rowJ, colJ] / 10 != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            else //cellNo is invalid if it is not an integer between 0 and 81, so the Sudoku must also be invalid
            {
                return false;
            }
            return true;
        }
    }
    class Backtracking 
    {
        private Graph sudokuGraph = new Graph(); 
        private byte[,] backtrackingGrid = new byte[9, 9]; //Contains the Sudoku grid being solved
        private byte[,] previousGrid = new byte[9, 9]; //If applicable, stores the previous solution found
        private bool resumingRecursion = false; //Records if the recursive algorithm is resuming, hence not yet ready to find the next solution
        public Backtracking()
        {
            //Create Backtracking solver
        }
        public (string[,], bool) Run(string[,] grid) 
        //Intended to run backtracking for the first time
        {
            CreateBacktrackingGrid(grid);
            bool sudokuValid = true;
            if (!IsSudokuFilled(backtrackingGrid))
            {
                RecursiveAlgorithm();
                sudokuValid = IsSudokuFilled(backtrackingGrid);
            }
            else
            {
                sudokuValid = sudokuGraph.IsSudokuValid(backtrackingGrid);
            }
            return (BacktrackingGridToSolution(), sudokuValid);
        }
        public (string[,], bool) RunAgain(string[,] prevSolutionGrid, string[,] afterReasoningGrid)
        //Intended to run backtracking again
        {
            CreateBacktrackingGrid(afterReasoningGrid);
            bool sudokuValid = true;
            if (!IsSudokuFilled(backtrackingGrid))
            {
                PopulatePreviousGrid(prevSolutionGrid);
                resumingRecursion = true;
                RecursiveAlgorithm();
                sudokuValid = IsSudokuFilled(backtrackingGrid);
            }
            else
            {
                sudokuValid = sudokuGraph.IsSudokuValid(backtrackingGrid);
            }
            return (BacktrackingGridToSolution(), sudokuValid);
        }
        private void CreateBacktrackingGrid(string[,] grid)
        //Converts the grid from a 2D string array to a 2D byte array stored in backtrackingGrid
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (grid[i, j].Length == 1)
                    {
                        backtrackingGrid[i, j] = byte.Parse(grid[i, j]);
                    }
                    else
                    {
                        backtrackingGrid[i, j] = 0;
                    }
                }
            }
        }
        private bool IsSudokuFilled(byte[,] grid)
        //Checks if every cell in a grid contains a number 1-9
        {
            foreach (byte entry in grid)
            {
                if (entry / 10 != 0 || entry == 0)
                {
                    return false;
                }
            }
            return true;
        }
        private string[,] BacktrackingGridToSolution()
        //Converts the grid from a 2D byte array backtrackingGrid to a 2D string array
        {
            string[,] gridString = new string[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    gridString[i, j] = backtrackingGrid[i, j].ToString();
                }
            }
            return gridString;
        }
        private bool RecursiveAlgorithm(byte r = 0, byte c = 0)
        //Main solving algorithm in the class: a recursive backtracking algorithm
        {
            byte row = 0, col = 0;
            bool wasValid = true;
            //Find the next empty space
            while (r < 9 && backtrackingGrid[r, c] != 0)
            {
                if (c < 8)
                {
                    c++;
                }
                else
                {
                    r++;
                    c = 0;
                }
            }
            //Place a number in the next empty space
            if (r < 9)
            {
                row = r;
                col = c;
                backtrackingGrid[row, col] = 1;

                //--START OF: Only applicable if a further round of backtracking is just resuming--//
                if (resumingRecursion)                                                             //
                {                                                                                  //
                    backtrackingGrid[row, col] = previousGrid[row, col];                           //
                }                                                                                  //
            }                                                                                      //
            if (resumingRecursion && IsSudokuFilled(backtrackingGrid))                             //
            {                                                                                      //
                resumingRecursion = false;                                                         //
            }                                                                                      //
            //--END OF: Only applicable if a further round of backtracking is just resuming--------//

            //Apply recursion if valid
            if (!IsSudokuFilled(backtrackingGrid) && sudokuGraph.IsSudokuValid(backtrackingGrid, row * 9 + col))
            {
                wasValid = RecursiveAlgorithm(row, col);
            }
            bool sudokuValid = sudokuGraph.IsSudokuValid(backtrackingGrid, row * 9 + col);
            //If invalid, increase the number in the last filled empty space until the Sudoku is valid, or the number is too big
            //If the Sudoku becomes valid, apply recursion
            if (!(sudokuValid && wasValid))
            {
                while (backtrackingGrid[row, col] <= 9 && !(sudokuValid && wasValid))
                {
                    backtrackingGrid[row, col] += 1;
                    sudokuValid = sudokuGraph.IsSudokuValid(backtrackingGrid, row * 9 + col);
                    if (sudokuValid && !IsSudokuFilled(backtrackingGrid))
                    {
                        wasValid = RecursiveAlgorithm(row, col);
                    }
                }
                //if none of the options worked, prepare to backtrack
                if (backtrackingGrid[row, col] == 10)
                {
                    backtrackingGrid[row, col] = 0;
                    wasValid = false;
                }
                else
                {
                    wasValid = true;
                }
            }
            return wasValid;
        }
        private void PopulatePreviousGrid(string[,] grid)
        /* Fills previousGrid with the previous solution, and
        adds 1 to the cell that was the last empty space of the Sudoku the user entered,
        so that the recursive algorithm can find the next solution of the Sudoku, 
        or find that there is no further solution to the Sudoku. */
        {
            bool added1 = false;
            //Traverses backwards so that the last empty space can be incremented by 1
            for (int i = 8; i > -1; i--)
            {
                for (int j = 8; j > -1; j--)
                {
                    previousGrid[i, j] = byte.Parse(grid[i, j]);
                    if (!added1 && previousGrid[i, j] != backtrackingGrid[i, j])
                    {
                        previousGrid[i, j] += 1;
                        added1 = true;
                    }
                }
            }
        }
    }
}
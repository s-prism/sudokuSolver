using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace sudokuSolver
{
    public class App : Application 
    {
        public App()
        {
            MainPage = new SudokuPage();
        }
    }
    public class SudokuPage : ContentPage 
    {
        private Grid sudokuDisplayGrid = new Grid(); //Layout container for the userGrid
        private Button[,] userGrid = new Button[9, 9]; //Sudoku grid of buttons for the user to enter numbers into
        private string[,] gridToSolve = new string[9, 9]; //Contains the Sudoku grid to solve
        private string[][,] sudokuSolutions = new string[10][,]; //Stores up to 10 solutions of gridToSolve
        private int solutionPointer = 0; //Records which solution (if multiple) the user has navigated to
        private Grid arrows; //Layout container for leftArrow and rightArrow
        private Grid buttons; //Layout container for resetGridButton and solveSudokuButton
        private Grid keyboard; //Layout container for buttons 1-9 (to enter a number into a cell) and X (to clear a cell)
        private Button resetGridButton;
        private Button solveSudokuButton;
        private Button leftArrow; //Lets the user navigate left to a solution (if there are multiple solutions)
        private Button rightArrow; //Lets the user navigate right to a solution (if there are multiple solutions)
        private Label banner = new Label(); //Varying banner to display after a particular Sudoku has been solved
        public SudokuPage()
        //The main page of the application
        {
            for (int i = 0; i < 10; i++)
            {
                sudokuSolutions[i] = new string[9, 9];
            }
            CreateSudokuEntries();
            CreateSudokuInputGrid(); 
            arrows = CreateArrows(); 
            buttons = CreateButtons(); 
            keyboard = CreateNumberKeyboard(); 

            Content = new StackLayout
            {
                BackgroundColor = Color.White,
                Padding = 4,
                Children = {
                    new Label { Text = "Sudoku Solver",FontSize=20,TextColor=Color.Indigo },
                    banner,
                    sudokuDisplayGrid,
                    arrows,
                    buttons,
                    keyboard,
                }
            };
        }
        private void CreateSudokuEntries()
        //Filling the array of buttons in userGrid
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    userGrid[r, c] = new Button
                    {
                        Text = "",
                        FontSize = 20,
                        TextColor = Color.Black,
                        BackgroundColor = Color.White,
                        CornerRadius = 0
                    };
                    userGrid[r, c].Clicked += GridCellClicked;
                }
            }
        }
        private void CreateSudokuInputGrid()
        //Building the user interface: places the buttons in userGrid into a 9x9 Sudoku grid 
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    //Adjusts rows up/down to create thicker horizontal lines around the 3x3 squares
                    int y = 0;
                    if (r <= 2)
                    {
                        y = -2;
                    }
                    if (r >= 6)
                    {
                        y = 2;
                    }
                    //Adjusts columns left/right to create thicker vertical lines around the 3x3 squares
                    int x = 0;
                    if (c <= 2)
                    {
                        x = -2;
                    }
                    if (c >= 6)
                    {
                        x = 2;
                    }
                    //Adds the relevant button to the grid on the screen
                    userGrid[r, c].TranslationX = x;
                    userGrid[r, c].TranslationY = y;
                    sudokuDisplayGrid.Children.Add(userGrid[r, c], c, r); 
                }
            }
            sudokuDisplayGrid.BackgroundColor = Color.Black;
            sudokuDisplayGrid.Padding = 5;
            sudokuDisplayGrid.ColumnSpacing = 1;
            sudokuDisplayGrid.RowSpacing = 1;
        }
        private Grid CreateArrows()
        //Create leftArrow and rightArrow, then place them in the arrows container
        {
            leftArrow = new Button
            {
                Text = "<",
                HorizontalOptions = LayoutOptions.End,
                IsVisible = false,
                IsEnabled = false,
                TextColor = Color.MediumPurple,
                BackgroundColor = Color.White
            };
            rightArrow = new Button
            {
                Text = ">",
                HorizontalOptions = LayoutOptions.Start,
                IsVisible = false,
                IsEnabled = false,
                TextColor = Color.MediumPurple,
                BackgroundColor = Color.White
            };
            Grid arrows = new Grid
            {
                Padding = 5,
            };
            arrows.Children.Add(leftArrow, 0, 0);
            arrows.Children.Add(rightArrow, 1, 0);
            leftArrow.Clicked += LeftArrowClicked;
            rightArrow.Clicked += RightArrowClicked;
            return arrows;
        }
        private Grid CreateButtons()
        //Create resetGridbutton and solveSudokuButton, then place them in the buttons container
        {
            resetGridButton = new Button
            {
                Text = "Reset",
                CornerRadius = 10,
                TextColor = Color.MediumPurple,
                BackgroundColor = Color.Transparent
            };
            solveSudokuButton = new Button
            {
                Text = "Solve",
                CornerRadius = 10,
                TextColor = Color.MediumPurple,
                BackgroundColor = Color.Transparent
            };
            Grid buttons = new Grid();
            buttons.Children.Add(resetGridButton, 0, 0);
            buttons.Children.Add(solveSudokuButton, 1, 0);
            resetGridButton.Clicked += ResetClicked;
            solveSudokuButton.Clicked += Solve;
            return buttons;
        }
        private Grid CreateNumberKeyboard()
        //Creates buttons 1-9 and X to clear a cell, and places it in the numbers container
        {
            Button[] numbers = new Button[10];
            Grid keyboard = new Grid();
            for (int i = 0; i < 9; i++)
            {
                numbers[i] = new Button
                {
                    Text = (i + 1).ToString(),
                    FontSize = 20,
                    TextColor = Color.Black,
                    BackgroundColor = Color.White
                };
            }
            for (int i = 0; i < 9; i++)
            {
                keyboard.Children.Add(numbers[i], i, 0);
                numbers[i].Clicked += NumberClicked;
            }
            numbers[9] = new Button
            {
                Text = "X",
                FontSize = 20,
                TextColor = Color.Black,
                BackgroundColor = Color.White
            };
            keyboard.Children.Add(numbers[9], 0, 1);
            numbers[9].Clicked += XClicked;
            return keyboard;
        }
        private void ResetClicked(object sender, EventArgs args)
        //Clears the entire Sudoku grid and page
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    userGrid[r, c].Text = "";
                    userGrid[r, c].TextColor = Color.Black;
                    userGrid[r, c].IsEnabled = true;
                    userGrid[r, c].InputTransparent = false;
                }
            }
            banner.Text = "";
            banner.BackgroundColor = Color.Transparent;
            leftArrow.IsVisible = false;
            rightArrow.IsVisible = false;
            for (int i = 0; i < 10; i++)
            {
                sudokuSolutions[i] = new string[9, 9];
            }
            solutionPointer = 0;
        }
        private void GridCellClicked(object sender, EventArgs args)
        //Highlights the cell in lavender, indicating that the value inside the cell can be changed
        {
            Button senderToButton = sender as Button;
            foreach (Button cell in userGrid)
            {
                cell.BackgroundColor = Color.White;
            }
            senderToButton.BackgroundColor = Color.Lavender;
        }
        private void NumberClicked(object sender, EventArgs args)
        //If a number button 1-9 is pressed, the number is entered in the highlighted cell
        {
            Button senderToButton = sender as Button;
            foreach (Button cell in userGrid)
            {
                //If it is the highlighted cell
                if (cell.BackgroundColor == Color.Lavender)
                {
                    cell.Text = senderToButton.Text;
                }
            }
        }
        private void XClicked(object sender, EventArgs args)
        //If the X button is clicked, the highlighted cell will be emptied
        {
            foreach (Button cell in userGrid)
            {
                //If it is the highlighted cell
                if (cell.BackgroundColor == Color.Lavender)
                {
                    cell.Text = "";
                }
            }
        }
        private void LeftArrowClicked(object sender, EventArgs args)
        //Lets the user navigate to an earlier solution if there are multiple solutions, if possible
        {
            solutionPointer -= 1;
            rightArrow.IsEnabled = true;
            rightArrow.IsVisible = true;
            if (solutionPointer == 0)
            {
                leftArrow.IsEnabled = false;
                leftArrow.IsVisible = false;
            }
            DisplayGrid();
        }
        private void RightArrowClicked(object sender, EventArgs args)
        //Lets the user navigate to an later solution if there are multiple solutions, if possible
        {
            solutionPointer += 1;
            leftArrow.IsEnabled = true;
            leftArrow.IsVisible = true;
            if (solutionPointer == 9 || sudokuSolutions[solutionPointer + 1][0, 0] == null)
            {
                rightArrow.IsEnabled = false;
                rightArrow.IsVisible = false;
            }
            DisplayGrid();
        }
        private void Solve(object sender, EventArgs args)
        //Finds solutions to a Sudoku
        {
            //Disable buttons that the user should not press while the Sudoku is being solved
            solveSudokuButton.IsEnabled = false;
            resetGridButton.IsEnabled = false;
            {
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        userGrid[r, c].IsEnabled = false;
                        userGrid[r, c].InputTransparent = true;
                        gridToSolve[r, c] = userGrid[r, c].Text;
                    }
                }
            }
            //Solve the Sudoku: either find 1 solution, or an error
            Sudoku thisSudoku = new Sudoku();
            string validityMessage;
            (sudokuSolutions[0], validityMessage) = thisSudoku.Solve(gridToSolve);
            //Either display a solution if the Sudoku is valid, or an error if the Sudoku is Invalid
            if (validityMessage == "True" || validityMessage=="Sudoku valid") 
            {
                DisplayGrid();
            }
            else
            {
                DisplayAlert("Error", validityMessage, "OK"); 
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        userGrid[r, c].IsEnabled = true;
                        userGrid[r, c].InputTransparent = false; 
                    }
                }
            }
            solveSudokuButton.IsEnabled = true;
            resetGridButton.IsEnabled = true;
            /* Look for further solutions (may be found if backtracking was used) if the Sudoku entered is valid and incomplete. 
            Only find up to 9 more solutions, which can be navigated to using arrows on the screen. */
            int counter = 0;
            while (counter < 9 && validityMessage == "True")
            {
                counter++;
                (sudokuSolutions[counter], validityMessage) = thisSudoku.RunOtherSolutionFinder();
                if (validityMessage != "True")
                {
                    sudokuSolutions[counter] = new string[9, 9];
                }
                else if (solutionPointer != 9 && sudokuSolutions[solutionPointer + 1] != new string[9, 9])
                {
                    rightArrow.IsVisible = true;
                    rightArrow.IsEnabled = true;
                }
            }
            //Display a banner depending on how the Sudoku was solved, and how many solutions have been found
            if (validityMessage != "True")
            {
                if (validityMessage == "Solution found using only reasoning")
                {
                    banner.Text = "Solution found using only reasoning";
                    banner.TextColor = Color.White;
                    banner.BackgroundColor = Color.CornflowerBlue;
                }
                else if (validityMessage == "Sudoku valid")
                {
                    banner.Text = "Sudoku valid";
                    banner.TextColor = Color.White;
                    banner.BackgroundColor = Color.DarkGreen;
                }
                else if (counter == 1)
                {
                    banner.Text = "Unique solution found";
                    banner.TextColor = Color.White;
                    banner.BackgroundColor = Color.LimeGreen;
                }
                else if (counter > 1)
                {
                    banner.Text = "All solutions found";
                    banner.TextColor = Color.White;
                    banner.BackgroundColor = Color.LimeGreen;
                }
            }
            else if (counter == 9)
            {
                banner.Text = "10 solutions found (there may be more)";
                banner.TextColor = Color.White;
                banner.BackgroundColor = Color.IndianRed;
            }
        }
        private void DisplayGrid()
        //Displays solution(s) to gridToSolve (the Sudoku entered by the user)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    userGrid[r, c].IsEnabled = true;
                    userGrid[r, c].Text = sudokuSolutions[solutionPointer][r, c];
                    if (gridToSolve[r, c] == sudokuSolutions[solutionPointer][r, c])
                    {
                        userGrid[r, c].TextColor = Color.Black;
                    }
                    else
                    {
                        userGrid[r, c].TextColor = Color.MediumPurple;
                    }
                }
            }
        }
    }
}
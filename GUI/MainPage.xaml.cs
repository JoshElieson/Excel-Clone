/// <summary>
/// Author:    Aspen Tobler
/// Partner:   Joshua Elieson
/// Date:      1-March-2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Aspen Tobler - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Aspen Tobler, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
/// This file represents the user interface for our spreadsheet program
/// </summary>
///using Microsoft.Maui.Storage;
using SpreadsheetUtilities;
using SS;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GUI
{
    /// <summary>
    /// This class holds the main body of the spreadsheet program, a spreadsheet holds 26x100 cells that can host a variety of different contents and cells,
    /// our spreadsheet program also includes functionality to open, save, and write in each spreadsheet, as well as a help page to understand it all
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Spreadsheet to link entries to the Spreadsheet class.
        /// </summary>
        public static AbstractSpreadsheet sp = new Spreadsheet();

        /// <summary>
        /// Entries stored in a dictionary for easy access.
        /// </summary>
        private Dictionary<string, Entry> entries = new();

        /// <summary>
        /// used to hold the current pen color to write with
        /// </summary>
        private Color penColor = new Color(0, 0, 0);

        /// <summary>
        /// used to hold which cell is currently selected
        /// </summary>
        private Entry currSelectedCell;

        /// <summary>
        /// Help page
        /// </summary>
        private Page helpPage = new HelpPage();

        /// <summary>
        /// used to detect whether the current spreadsheet has been saved or not
        /// </summary>
        private Boolean saved = true;

        /// <summary>
        /// used to detect whether a new spreadsheet is currently being opened, to avoid errors with the "textChanged" method
        /// </summary>
        private Boolean changing = false;

        /// <summary>
        /// Sets up the spreadsheet.
        /// </summary>
        public MainPage()
        {
            //sp = new Spreadsheet();
            //entries = new();
            //penColor = new Color(0, 0, 0);
            //saved = false;

            //creates the spreadsheet in vertical order
            InitializeComponent();
            AddTopLabels(); //add A, B, C, ... labels
            AddGrid(); //add 1, 2, 3, ... labels and each cell
            onStartup(); //initialize focus/text/etc.
        }


        /// <summary>
        /// Adds the top labels. ex. A, B, C, ...
        /// </summary>
        private void AddTopLabels()
        {
            //add one empty label at the top corner
            TopLabels.Add(
                    new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = 30,
                        WidthRequest = 75,
                        HorizontalOptions = LayoutOptions.Center,
                        Content =
                            new Label
                            {
                                BackgroundColor = Color.FromRgb(200, 200, 250),
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                    }
                    );
            //now add 26 labels, titled 'A', 'B', ..., etc.
            for (int i = 0; i < 26; i++)
            {
                TopLabels.Add(
                    new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = 30,
                        WidthRequest = 75,
                        HorizontalOptions = LayoutOptions.Center,
                        Content =
                            new Label
                            {
                                Text = ((char)('A' + i)).ToString(),
                                BackgroundColor = Color.FromRgb(200, 200, 250),
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                    }
                    );
            }
        }

        /// <summary>
        /// Creates grid entries for a spreadsheet.
        /// </summary>
        private void AddGrid()
        {
            SideLabels = new VerticalStackLayout(); //setup labels {1, 2, 3, ..., etc.} on the far left
            for (int i = 0; i < 99; i++) //add all 100 labels
            {
                SideLabels.Add(
                    new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = 30,
                        WidthRequest = 75,
                        HorizontalOptions = LayoutOptions.Start,
                        Content =
                            new Label
                            {
                                Text = (i + 1).ToString(), //set text as {1, 2, 3, ..., etc.}
                                BackgroundColor = Color.FromRgb(200, 200, 250), //set background color a light blue
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                    }
                    );
            }
            table.Children.Add(SideLabels); //table is a horizontal stack layout that can then be added to the full GUI (a vert stack) after this method,
            for (int col = 0; col < 26; col++) //add all 26x100 cells/entries
            {
                Grid = new VerticalStackLayout(); //Grid is a vertical stack that includes 100 horizontal stacks, making a grid
                for (int row = 0; row < 99; row++)
                {
                    var currEntry = new Entry
                    {
                        BackgroundColor = Color.FromRgb(250, 250, 210),
                        TextColor = Color.FromRgb(0, 0, 0),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Start,
                        AutomationId = ((char)('A' + col)).ToString() + (row + 1).ToString()
                    };

                    entries[currEntry.AutomationId] = currEntry; //set the entries AutomationId as their name, ex: 'A1', 'B2', etc., to grab later

                    currEntry.Completed += OnCellCompleted; //give each cell some commands when they are completed
                    currEntry.TextChanged += OnCellTextChanged; //give each cell some commands when they are changed
                    currEntry.Focused += OnCellFocused; //give each cell some commands when they are focused on

                    var label = new Border
                    {
                        Stroke = Color.FromRgb(0, 0, 0),
                        StrokeThickness = 1,
                        HeightRequest = 30,
                        WidthRequest = 75,
                        Content = currEntry
                    };
                    Grid.Children.Add(label); //add the created horizontal stack onto Grid
                }
                table.Children.Add(Grid); //add the created grid into table
            }

        }

        /// <summary>
        /// sets the inital focused value as cell "A1" on startup
        /// </summary>
        private void onStartup()
        {
            if (entries.TryGetValue("A1", out Entry defaultCell))
            {
                currSelectedCell = defaultCell; //initialize focus on cell 'A1'
                currSelectedCell.Focus();
            }
            selectedCellContent.Text = "";      //initialize contents at the top to be empty
            selectedCellValue.Text = "Value: Empty";    //set value at the top to be empty
            selectedCellName.Text = "Cell: A1";         //set the cell name at the top to 'A1'
        }

        /// <summary>
        /// Adds cells to the spreadsheet when entries are placed. Jumps focus to
        /// the next cell down and displays the value, rather than contents.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCellCompleted(object? sender, EventArgs e)
        {
            if (sender is Entry currEntry)
            {
                string name = currEntry.AutomationId;           //get the cell name
                string contents = currEntry.Text;               //get the cell text
                if (contents.Length > 0 && contents[0] == '=')  //make lowercase cell calls become uppercase, in other words make formulas case insensitive
                    contents = contents.ToUpper();
                try
                {
                    sp.SetContentsOfCell(name, contents);           //set the contents of the cell in sp
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);           //set the contents of the cell in sp
                }
                char column = name[0];                          //get the cell column
                int row = int.Parse(name.Substring(1));         //get the cell row
                string nextCellName = $"{column}{row + 1}";     //get the next cell's name

                // Jump focus down
                if (entries.TryGetValue(nextCellName, out Entry nextCell))
                {
                    nextCell.Focus();                           //set focus on the next cell
                }

                // Update cell

                object value = sp.GetCellValue(name);           //get the completed cell's value

                if (entries.TryGetValue(name, out Entry entry))
                {
                    entry.Text = value.ToString();              //update the cell text
                    entry.TextColor = penColor;                 //update the cell color
                }
                foreach (Entry x in entries.Values)
                {
                    x.UpdateText(sp.GetCellValue(x.AutomationId).ToString());
                }
            }
            saved = false; //reset saved to false, as new info was added that has not been saved
        }

        /// <summary>
        /// Used to change the text color of the cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCellTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && !changing)
            {
                entry.TextColor = penColor;                     //update the cell text color
                selectedCellContent.Text = entry.Text;          //update the selected cell's text at the top
            }
        }

        /// <summary>
        /// activates when a cell is focused, used to disply values and contents at top of screen and show contents rather than value of focused cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCellFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                try
                {
                    string currValue = sp.GetCellValue(currSelectedCell.AutomationId).ToString(); //save the previously focused cell value
                    if (currValue != "")
                        currSelectedCell.Text = currValue; //reset the previously focused cell text to its value, rather than its contents
                    string contents = "";
                    if (sp.GetCellContents(entry.AutomationId) is Formula)      //if the contents are a formula, return the "=" at the beginning
                        contents = "=" + sp.GetCellContents(entry.AutomationId).ToString();
                    else                                                        //otherwise the contents should equal the value
                        contents = sp.GetCellContents(entry.AutomationId).ToString();

                    currSelectedCell = entry;                                   //update which cell is currently selected
                    selectedCellValue.Text = "Value: " + sp.GetCellValue(entry.AutomationId).ToString();            //update the value at the top
                    selectedCellName.Text = "Cell: " + entry.AutomationId;      //update the cell name at the top
                    if (contents != "")
                    {
                        entry.Text = contents;                                      //update the focused cell's text
                    }
                    selectedCellContent.Text = entry.Text;                        //update the cell content at the top
                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// Displays an error message if something goes wrong in the spreadsheet.
        /// </summary>
        /// <param name="error"></param>
        async void ShowErrorMessage(String error)
        {
            await DisplayAlert("Error", error, "OK");
        }

        /// <summary>
        /// changes the color of text to the MenuFlyoutItem clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setPenColor(object sender, EventArgs e)
        {
            MenuFlyoutItem clicked = (MenuFlyoutItem)sender;
            if (clicked.Text == "Red")                      //change the pen color to red
                penColor = Color.FromRgb(255, 0, 0);
            else if (clicked.Text == "Green")               //change the pen color to green
                penColor = Color.FromRgb(0, 200, 0);
            else if (clicked.Text == "Blue")                //change the pen color to blue
                penColor = Color.FromRgb(0, 0, 255);
            else if (clicked.Text == "Black")               //change the pen color to black
                penColor = Color.FromRgb(0, 0, 0);
            else if (clicked.Text == "Purple")               //change the pen color to purple
                penColor = Color.FromRgb(160, 32, 240);
            else if (clicked.Text == "Yellow")               //change the pen color to yellow
                penColor = Color.FromRgb(255, 165, 0);

        }

        /// <summary>
        /// used for when the contents entry at the top of the screen is used to edit the currently selected cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topContentsChanged(object sender, TextChangedEventArgs e)
        {
            currSelectedCell.Text = selectedCellContent.Text; //update the current cell's text
        }

        /// <summary>
        /// used for when a formula is finished being written at the top of the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topContentsCompleted(object? sender, EventArgs e)
        {
            string name = currSelectedCell.AutomationId;    //get the current cell name
            string contents = currSelectedCell.Text;        //get the current cell text
            if (contents.Length > 0 && contents[0] == '=')  //make lowercase cell calls become uppercase, in other words make formulas case insensitive
                contents = contents.ToUpper();

            sp.SetContentsOfCell(name, contents);           //store the cell contents in sp

            char column = name[0];                          //get the current column
            int row = int.Parse(name.Substring(1));         //get the current row
            string nextCellName = $"{column}{row + 1}";     //get the name of the next cell

            if (entries.TryGetValue(nextCellName, out Entry nextCell))
            {
                nextCell.Focus();                           //focus to the next cell
            }

            object value = sp.GetCellValue(name);           //get the value of the completed cell
            if (entries.TryGetValue(name, out Entry entry))
            {
                entry.Text = value.ToString();              //update the cell's text
                entry.TextColor = penColor;                 //update the cell's color
            }
        }

        /// <summary>
        /// Opens the help page.
        /// </summary>
        /// <param name="sender"> ignored </param>
        /// <param name="e"> ignored </param>
        void HelpInformation(object sender, EventArgs e)
        {

            Navigation.PushModalAsync(helpPage, true);
        }

        /// <summary>
        /// Method used to determine whether a string that consists of one or more letters
        /// followed by one or more digits is a valid variable name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool isValid(string name)
        {
            Regex regex = new Regex(@"^[a-zA-Z]+[0-9]+$");

            return regex.IsMatch(name);
        }

        /// <summary>
        /// Opens the save page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void OpenSpreadsheet(object sender, EventArgs e)
        {
            if (!saved)
            {
                bool userSave = await DisplayAlert("Save Current Spreadsheet",
                        "The spreadsheet you are currently working on is not saved. Are you sure you want to continue?",
                        "Yes, replace my current spreadsheet with an empty one.", "No");
                if (userSave)
                {
                    OpenFilePicker();
                }

            } else
            {
                OpenFilePicker();
            }
        }

        /// <summary>
        /// Opens up the file picker to allow the user to select a file to open.
        /// </summary>
        /// <returns></returns>
        private async Task OpenFilePicker()
        {
            ClearCurrentSpreadsheet();
            // display warning in helper method

            FileResult filesrc = await FilePicker.Default.PickAsync();
            foreach (Entry entry in entries.Values)
                entry.Text = "";
            if (filesrc != null)
            {
                string file = File.ReadAllText(filesrc.FullPath);
                sp = new Spreadsheet(filesrc.FullPath, s => isValid(s), s => s.ToUpper(), "six");
                IEnumerable<string> cells = sp.GetNamesOfAllNonemptyCells();
                changing = true;

                foreach (string cell in cells)
                {
                    entries[cell].Text = sp.GetCellValue(cell).ToString();
                }
                changing = false;
            }
        }

        /// <summary>
        /// Used to open a new blank file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void FileMenuNew(object sender, EventArgs e)
        {
            if (!saved)
            {
                var userSave = await DisplayAlert("Save Current Spreadsheet",
                    "The spreadsheet you are currently working on is not saved. Are you sure you want to continue?",
                    "Yes, replace my current spreadsheet with an empty one.", "No");
                if (userSave)
                {
                    ClearCurrentSpreadsheet();
                }
            }
            else
            {
                ClearCurrentSpreadsheet();
            }
        }

        /// <summary>
        /// Clears the contents of the current spreadsheet
        /// </summary>
        /// <param name="userSave"></param>
        private void ClearCurrentSpreadsheet()
        {
            foreach (var entry in entries.Values)
            {
                entry.Text = "";
            }

            sp = new Spreadsheet();
            saved = true;
        }

        /// <summary>
        /// used to save a .sprd file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void SaveSpreadsheet(object sender, EventArgs e)
        {
            string fileName = await DisplayPromptAsync("Save Spreadsheet", "Enter a name for the file:");
            if (fileName == null)
            {
                return;
            }
            else if (!fileName.EndsWith(".sprd"))
            {
                fileName += ".sprd";
                await DisplayAlert("", ".sprd was added to the end of the file", "OK");
            }
            string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            try
            {
                if (!sp.GetNamesOfAllNonemptyCells().Any())
                {
                    await DisplayAlert("", "The file being saved is empty", "OK");
                }
                else if (saved)
                {
                    await DisplayAlert("", "File has already been saved.", "OK");
                }
                else
                {
                    sp.Save(fullpath);
                    saved = true;
                    await DisplayAlert("", "File saved successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                saved = false;
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

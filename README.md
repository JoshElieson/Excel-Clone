```
Author:     Aspen Tobler
Partner:    Joshua Elieson
Start Date: 14-Jan-2024
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  buzz101kill
Repo:       https://github.com/uofu-cs3500-spring24/assignment-six-gui-functioning-spreadsheet-teamjoshaspen
Commit Date: 2-March-2024
Solution:   Spreadsheet
Copyright:  CS 3500 and Aspen Tobler - This work may not be copied for use in Academic Coursework.
```

# Overview of the Spreadsheet functionality

This Spreadsheet project consists of several classes that perform different, but united functionality to create a 
working spreadsheet, similar to one found in Excel (with much less functionality). 

This project consists of a Formula class that checks for formula validity and evaluates the formulas. 

There is also a DependencyGraph class that deals with dependencies within formulas. For example, if cell
A1 contained 3 and cell A2 contained =A1+2, the DependencyGraph links A1 and A2 together to represent a dependency.

This project also contains a Spreadsheet class that contains cells, which contain values, contents, and names. This
class uses both the Formula and the DependencyGraph class to manage how cells interact with each other. This class also
allows for instances of Spreadsheet to be saved as an XML text file, as well as a constructor that will read in an XML
file and add its contents to the Spreadsheet.

The last part of this project is the GUI. This takes the Spreadsheet class and displays it in a user friendly manner.
Users are able to enter content into cells of the 26 x 99 spreadsheet. The content of a cell can be a string, number
(double), or a formula (indicated by starting with an = sign). Because of the DependencyGraph, users are allowed to enter
the names of other cells in as variables in their formulas and the Spreadsheet will handle this accordingly. Because of the
Formula class, the evaluated value of the contents is what is displayed in the cell if the user enters a formula. Users are
also offered the ability open and save instances of spreadsheets, as well as open a new spreadsheet, which clears the contents 
in the old one. A help page is also provided for users with instructions and examples of how to use the spreadsheet.

Special functionality: The special functionality that my partner and I decided to implement is the ability to change text color.
We created a dropdown menu with colors that users can choose from. This allows for users to change the color of the text inside the
cell they are currently working on.

# Time Expenditures:

    1. Assignment One:   Predicted Hours:          10        Actual Hours:   15
    2. Assignment Two:   Predicted Hours:          20        Actual Hours:   23
    3. Assignment Three: Predicted Hours:          25        Actual Hours:   30
    4. Assignment Four:  Predicted Hours:          30        Actual Hours:   22
    5. Assignment Five:  Predicted Hours:          20        Actual Hours:   19
    6. Assignment Six:   Predicted Hours:          15        Actual Hours:   20
            Extra Breakdown (Approximate hours spent on A6):    
                    Aspen:      10
                    Josh:       10
                         

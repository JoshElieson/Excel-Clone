/// <summary>
/// Author:    Aspen Tobler
/// Partner:   Joshua Elieson
/// Date:      18-Feb-2024
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
///
///    This library class represents a spreadsheet that was inherited from
///    AbstractSpreadsheet.
/// </summary>
///
using SpreadsheetUtilities;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;

namespace SS
{
    /// <summary>
    /// This class represents an instance of a spreadsheet that contains infinite many empty cells
    /// and can link cells with a dependency graph and evaluate formulas with the formula class.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// A dictionary to store the non-empty cells in the spreadsheet.
        /// </summary>
        private Dictionary<string, Cell> cells;

        /// <summary>
        /// A dependency graph used to link cells that depend on each other to evaluate.
        /// </summary>
        private DependencyGraph dpGraph;

        /// <summary>
        /// False if document has not changed since it was last saved or created. Otherwise, true.
        /// </summary>
        private bool changed;

        /// <summary>
        /// Constructor for an empty spreadsheet.
        /// </summary>
        public Spreadsheet() : base(s => isValid(s), s => s, "six")
        {
            cells = new Dictionary<string, Cell>();
            dpGraph = new DependencyGraph();
            this.Changed = false;
        }

        /// <summary>
        /// Constructor for an empty spreadsheet with a provided IsValid, Normalize, and version.
        /// </summary>
        /// <param name="IsValid"></param>
        /// <param name="Normalize"></param>
        /// <param name="Version"></param>
        public Spreadsheet(Func<string, bool> IsValid, Func<string, string> Normalize, string version)
            : base(IsValid, Normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            dpGraph = new DependencyGraph();
            this.Version = Version;
            this.Changed = false;
        }

        /// <summary>
        /// Constructor for an empty spreadsheet with IsValid, Normalize, version, and path to save to.
        /// </summary>
        /// <param name="pathtofile"></param>
        /// <param name="IsValid"></param>
        /// <param name="Normalize"></param>
        /// <param name="Version"></param>
        public Spreadsheet(string pathtofile, Func<string, bool> IsValid, Func<string, string> Normalize, string version) :
            base(IsValid, Normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            dpGraph = new DependencyGraph();
            ReadXml(pathtofile);
            this.Changed = false;
        }

        /// <summary>
        /// Reads the XML written in the filepath given and adds the elements to the spreadsheet.
        /// </summary>
        /// <param name="pathtofile"></param>
        private void ReadXml(string pathtofile)
        {
            if (!File.Exists(pathtofile)) throw new SpreadsheetReadWriteException("File path not found.");

            using (XmlReader reader = XmlReader.Create(pathtofile))
            {
                string currElement = "";
                string cellName = "";
                string content = "";

                try
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                currElement = reader.Name;
                                if (currElement == "spreadsheet")
                                {
                                    if (Version != reader.GetAttribute("version"))
                                    {
                                        throw new SpreadsheetReadWriteException("There was a problem with the version of the spreadsheet.");
                                    }
                                }
                                break;
                            case XmlNodeType.Text:
                                if (currElement == "name")
                                {
                                    cellName = reader.Value;
                                }
                                else if (currElement == "contents")
                                {
                                    content = reader.Value;
                                }
                                break;
                            case XmlNodeType.EndElement:
                                // if the cell is not invalid...
                                if (reader.Name == "cell" && !string.IsNullOrEmpty(cellName) && !string.IsNullOrEmpty(content))
                                {
                                    if (double.TryParse(content, out double num))
                                    {
                                        SetCellContents(cellName, num);
                                    }
                                    else if (content.Count() > 0 && content[0] == '=')
                                    {
                                        Formula formula = new Formula(content.Substring(1), Normalize, IsValid);
                                        SetCellContents(cellName, formula);
                                    }
                                    else
                                    {
                                        SetCellContents(cellName, content);
                                    }

                                    // reset for the next cell to be read
                                    cellName = "";
                                    content = "";
                                }
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw new SpreadsheetReadWriteException("There was a problem reading the file.");
                }
            }
        }

        /// <inheritdoc/>
        public override bool Changed
        {
            get { return changed; }
            protected set { changed = value; }
        }

        /// <inheritdoc/>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            return cells.Keys;
        }

        /// <inheritdoc/>
        public override object GetCellContents(String name)
        {
            name = Normalize(name);

            if (!IsValid(name)) { throw new InvalidNameException(); }

            if (cells.ContainsKey(name))
            {
                Cell cell = cells[name];
                return cell.GetContents();
            }

            // returns an empty string if the cell is not specifically established.
            return "";
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

        /// <inheritdoc/>
        protected override IList<String> SetCellContents(String name, double number)
        {
            name = Normalize(name);

            // If there was a change made, changed becomes true.
            if (cells.ContainsKey(name) && cells[name].GetContents().ToString() == number.ToString())
            {
                this.Changed = false;
            } else
            {
                this.Changed = true;
            }

            cells[name] = new Cell(number);
            dpGraph.ReplaceDependees(name, new String[] { });
            Recalculate(name);
            return GetCellsToRecalculate(name).ToList();
        }

        /// <inheritdoc/>
        protected override IList<String> SetCellContents(String name, String text)
        {
            name = Normalize(name);

            // If there was a change made, changed becomes true.
            if (cells.ContainsKey(name) && cells[name].GetContents().ToString() == text)
            {
                this.Changed = false;
            }
            else
            {
                this.Changed = true;
            }

            if (text == "" || text == " ")
            {
                if (cells.ContainsKey(name))
                {
                    cells.Remove(name);
                    Recalculate(name);
                    return GetCellsToRecalculate(name).ToList();
                }

                dpGraph.ReplaceDependees(name, new String[] { });
                Recalculate(name);
                return GetCellsToRecalculate(name).ToList();
            }

            cells[name] = new Cell(text);
            dpGraph.ReplaceDependees(name, new String[] { });

            Recalculate(name);
            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// Re-evaluates the cells that need to be recalculated.
        /// </summary>
        /// <param name="cellName"></param>
        private IList<String> Recalculate(String name)
        {
            IEnumerable<String> cellsToRecalculate = GetCellsToRecalculate(name);
            foreach (var cellName in cellsToRecalculate)
            {
                if (cellName == name) continue;
                cells[cellName].SetValue(((Formula)(cells[cellName].GetContents())).Evaluate(lookup));
            }

            return new List<String>(cellsToRecalculate);
        }

        /// <inheritdoc/>
        protected override IList<String> SetCellContents(String name, Formula formula)
        {
            name = Normalize(name);

            // If there was a change made, changed becomes true.
            if (cells.ContainsKey(name) && cells[name].GetContents().ToString() == formula.ToString())
            {
                this.Changed = false;
            }
            else
            {
                this.Changed = true;
            }

            object oldContents = GetCellContents(name);
            dpGraph.ReplaceDependees(name, formula.GetVariables());
            cells[name] = new Cell(formula, formula.Evaluate(lookup));

            try
            {
                Recalculate(name);
                return GetCellsToRecalculate(name).ToList();
            }
            catch (CircularException)
            {
                if (oldContents is String)
                {
                    SetCellContents(name, (String)oldContents);
                }
                else if (oldContents is double)
                {
                    SetCellContents(name, (double)oldContents);
                }
                else
                {
                    SetCellContents(name, (Formula)oldContents);
                }

                throw new CircularException();
            }
        }

        /// <summary>
        /// Used to deterimine the value of a cell.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        private double lookup(String variable)
        {
            if (cells.ContainsKey(variable))
            {
                if (cells[variable].GetValue() is double)
                {
                    return (double)cells[variable].GetValue();
                }
                else
                {
                    throw new ArgumentException("Cannot evaluate a string.");
                }
            }

            throw new ArgumentException("Cannot evaluate with empty variables.");
        }

        /// <inheritdoc/>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            name = Normalize(name);

            List<String> directDependents = new List<String>();
            foreach (String d in dpGraph.GetDependents(name).ToList())
            {
                if (!directDependents.Contains(d))
                {
                    directDependents.Add(d);
                }
            }
            return directDependents;
        }

        /// <inheritdoc/>
        public override IList<String> SetContentsOfCell(String name, String content)
        {
            if (!IsValid(name)) throw new InvalidNameException();

            if (double.TryParse(content, out double contentNum))
            {
                return SetCellContents(name, contentNum);
            }
            else if (content.Count() > 0 && content[0] == '=')
            {
                Formula formula = new Formula(content.Substring(1), Normalize, IsValid);
                return SetCellContents(name, formula);
            }
            else
            {
                return SetCellContents(name, content);
            }
        }

        /// <inheritdoc/>
        public override String GetSavedVersion(String filename)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)// && reader.Name == "spreadsheet")
                        {
                            // Once the <spreadsheet> element is found, return the value of the "version" attribute
                            return reader.GetAttribute("version");
                        }
                    }
                    throw new SpreadsheetReadWriteException("File not found.");
                }
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("An error occurred while reading the file.");
            }
        }


        /// <inheritdoc/>
        public override void Save(String filename)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(filename, false, new UnicodeEncoding(bigEndian: false, byteOrderMark: true));
                sw.Write(GetXML());
                this.Changed = false;
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException("Something went wrong trying to save file.");
            }
        }

        /// <inheritdoc/>
        public override string GetXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", Version);

                foreach (var cell in cells)
                {
                    writer.WriteStartElement("cell");
                    writer.WriteElementString("name", cell.Key);

                    object content = cell.Value.GetContents();

                    if (content is double || content is string)
                    {
                        writer.WriteElementString("contents", content.ToString());
                    }
                    else if (content is Formula)
                    {
                        writer.WriteElementString("contents", "=" + content.ToString());
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public override object GetCellValue(string name)
        {
            if (!IsValid(name)) throw new InvalidNameException();

            if (cells.ContainsKey(name))
            {
                return cells[name].GetValue();
            }

            return "";
        }
    }

    /// <summary>
    /// Represents a single cell in a spreadsheet.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Represents the contents of a cell (different from the value).
        /// </summary>
        private object content;

        /// <summary>
        /// Represents the value of a cell (sometimes differs from content).
        /// </summary>
        private object value;

        /// <summary>
        /// Constructor for cell content containing a string.
        /// </summary>
        /// <param name="text"></param>
        public Cell(string text)
        {
            content = text;
            value = text;
        }

        /// <summary>
        /// Constructor for cell content containing a number.
        /// </summary>
        /// <param name="number"></param>
        public Cell(double number)
        {
            content = number;
            value = number;
        }

        /// <summary>
        /// Constructor for cell content containing a formula.
        /// </summary>
        /// <param name="formula"></param>
        public Cell(Formula formula, object value)
        {
            content = formula;
            this.value = value;
        }

        /// <summary>
        /// Getter for the contents of the cell.
        /// </summary>
        /// <returns></returns>
        public object GetContents()
        {
            return content;
        }

        /// <summary>
        /// Returns the value of a cell.
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return value;
        }

        /// <summary>
        /// Sets the value of a cell
        /// </summary>
        /// <returns></returns>
        public void SetValue(object value)
        {
            this.value = value;
        }
    }
}

/// <summary>
/// Author:    Aspen Tobler
/// Partner:   -none-
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
///    This MST Testing project file includes tests that ensure the Spreadsheet project is
///    working correctly.
/// </summary>
using SS;
using SpreadsheetUtilities;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetTests
{
    /// <summary>
    /// This tester class ensures the correctness of the Spreadsheet class by testing various cases.
    /// </summary>
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// An instance of a spreadsheet that I called in all of my methods. I decleared it outside of
        /// each of the tests, so I would not be repeating code.
        /// </summary>
        private AbstractSpreadsheet sp = new Spreadsheet();
        private AbstractSpreadsheet sp2 = new Spreadsheet(s => true, s => s.ToUpper(), "1");
        //private AbstractSpreadsheet sp3 = new Spreadsheet("TestFile.txt", s => true, s => s, "2");

        /// <summary>
        /// Value and contents of a cell are the same when setting cell contents with a number.
        /// </summary>
        [TestMethod]
        public void SetCellContentsWithNumbersTest()
        {
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.SetContentsOfCell("A1", "3").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            CollectionAssert.AreEqual(new List<String> { "B1" }, sp.SetContentsOfCell("B1", "3").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1", "B1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("3A", "3"); });
        }

        /// <summary>
        /// Value and contents of a cell are the same when setting cell contents with a string.
        /// </summary>
        [TestMethod]
        public void SetCellContentsWithStringsTest()
        {
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.SetContentsOfCell("A1", "hello").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            CollectionAssert.AreEqual(new List<String> { "B1" }, sp.SetContentsOfCell("B1", "world").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1", "B1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("3A", "world"); });
        }

        /// <summary>
        /// Use formulas to set the contents and value of a cell.
        /// </summary>
        [TestMethod]
        public void SetCellContentsWithFormulasTest()
        {
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.SetContentsOfCell("A1", "=3*4").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            CollectionAssert.AreEqual(new List<String> { "B1" }, sp.SetContentsOfCell("B1", "=1-2").ToList());
            CollectionAssert.AreEqual(new List<String> { "A1", "B1" }, sp.GetNamesOfAllNonemptyCells().ToList());
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("3A", "=A1-2"); });
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("", "=3"); });
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("_@", "=3"); });
        }

        /// <summary>
        /// Calling SetCellContents with a formula that includes variables does what it's supposed to.
        /// </summary>
        [TestMethod]
        public void SetCellContentsWithFormulasUsingDependencyGraphTest()
        {
            sp.SetContentsOfCell("A1", "=B1");
            sp.SetContentsOfCell("B1", "=2+3");
            sp.SetContentsOfCell("B3", "2");
            sp.SetContentsOfCell("D2", "hello");
            Assert.AreEqual(new Formula("B1"), sp.GetCellContents("A1"));
            Assert.AreEqual(new Formula("2+3"), sp.GetCellContents("B1"));
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("A2", "=A2*A2"); });
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("B1", "=B1*B2"); });
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("B3", "=B3*B3"); });
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("D2", "=D2+D2"); });
        }

        /// <summary>
        /// Get an IEnumerable list of the cells that are not empty.
        /// </summary>
        [TestMethod]
        public void GetNamesOfAllNonEmptyCellsTest()
        {
            CollectionAssert.AreEqual(new List<String> { }, sp.GetNamesOfAllNonemptyCells().ToList());
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("A2", "hello");
            CollectionAssert.AreEqual(new List<String> { "A1", "A2" }, sp.GetNamesOfAllNonemptyCells().ToList());
        }

        /// <summary>
        /// Tests different instances of calling GetCellContents.
        /// </summary>
        [TestMethod]
        public void GetCellContentsTest()
        {
            Assert.AreEqual("", sp.GetCellContents("A1"));
            sp.SetContentsOfCell("A1", "A1");
            Assert.AreEqual("A1", sp.GetCellContents("A1"));
            sp.SetContentsOfCell("B2", "=3+3");
            Assert.AreEqual(new Formula("3 + 3"), sp.GetCellContents("B2"));
            Assert.ThrowsException<InvalidNameException>(() => { sp.GetCellContents("a_4^b"); });
        }

        /// <summary>
        /// Used to test GetDirectDependents indirectly.
        /// </summary>
        [TestMethod]
        public void GetDirectDependentsTest()
        {
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("B1", "=A1 * A1");
            sp.SetContentsOfCell("C1", "=B1 + A1");
            sp.SetContentsOfCell("D1", "=B1 - C1");

            sp.SetContentsOfCell("A2", "=B2");
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("B2", "=A2"); });

            sp.SetContentsOfCell("AA1", "=BB1");
            sp.SetContentsOfCell("BB1", "=CC1");
            Assert.ThrowsException<CircularException>(() => { sp.SetContentsOfCell("CC1", "=AA1"); });

            sp2.SetContentsOfCell("a1", "=b1");
            sp2.SetContentsOfCell("b1", "=c1");
            sp2.SetContentsOfCell("c1", "=d1");
            Assert.ThrowsException<CircularException>(() => { sp2.SetContentsOfCell("d1", "=b1"); });

            sp2.SetContentsOfCell("e1", "=f1+1");
            sp2.SetContentsOfCell("f1", "=g1+1");
            Assert.ThrowsException<CircularException>(() => { sp2.SetContentsOfCell("g1", "=e1+1"); });
        }

        /// <summary>
        /// Ensures that no errors are thrown when something is added twice in the dependency graph.
        /// </summary>
        [TestMethod]
        public void ReplaceDependenciesTest()
        {
            sp.SetContentsOfCell("a1", "=b1");
            sp.SetContentsOfCell("a1", "=b1");
        }

        /// <summary>
        /// Returns an empty enumerable if there are no cells set.
        /// </summary>
        [TestMethod]
        public void GetNamesOfNonEmptyCellsTest()
        {
            Assert.AreEqual(0, sp.GetNamesOfAllNonemptyCells().Count());
        }

        //[TestMethod]
        //public void GetXMLTest()
        //{
        //    sp.SetContentsOfCell("A1", "1");
        //    sp.SetContentsOfCell("A2", "=A1+1");
        //    sp.SetContentsOfCell("A1", "2");
        //    sp.SetContentsOfCell("A1", "hey");
        //    sp.SetContentsOfCell("A2", "3");
        //    // Console.WriteLine(sp.GetXML());
        //}

        /// <summary>
        /// Test setting invalid items.
        /// </summary>
        [TestMethod]
        public void SetCellContentsInvalidTest()
        {
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("_a", "3"); });
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("3a", "hello"); });
            Assert.ThrowsException<InvalidNameException>(() => { sp.SetContentsOfCell("A3A2", "=1+1"); });

            sp.SetContentsOfCell("A2", "=3");
            sp.SetContentsOfCell("A1", "=A2+2");
            sp.SetContentsOfCell("A1", " ");
        }

        /// <summary>
        /// Tests the save method. (I manually debugged this and looked for the file
        /// rather than having Asserts.)
        /// </summary>
        [TestMethod]
        public void SaveTest()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "1");
            ss.Save("newFile.txt");
            ss.SetContentsOfCell("A2", "hey");
            ss.SetContentsOfCell("B3", "=A2+2");
            ss.Save("newFile.txt");
            Console.WriteLine(ss.GetSavedVersion("newFile.txt"));

            // Saving to an empty string file does not work.
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => { sp.Save(""); });
        }

        /// <summary>
        /// Trying to get the name of a file that does not exist throws an error.
        /// </summary>
        [TestMethod]
        public void GetSavedVersionTest()
        {
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => { sp.GetSavedVersion("unknownFile.txt"); });
        }

        /// <summary>
        /// Ensures changed is set to true when changes are made.
        /// </summary>
        [TestMethod]
        public void ChangedTest()
        {
            Assert.IsFalse(sp.Changed);
            sp.SetContentsOfCell("A1", "");
            Assert.IsTrue(sp.Changed);
            sp.SetContentsOfCell("A1", "2");
            Assert.IsTrue(sp.Changed);
            sp.SetContentsOfCell("A1", "2");
            Assert.IsFalse(sp.Changed);
            sp.SetContentsOfCell("A1", "1");
            Assert.IsTrue(sp.Changed);
            sp.Save("spreadsheet");
            Assert.IsFalse(sp.Changed);

            sp.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(sp.Changed);
            sp.SetContentsOfCell("B1", "hello");
            Assert.IsFalse(sp.Changed);

            sp.SetContentsOfCell("B3", "=1");
            Assert.IsTrue(sp.Changed);
            sp.SetContentsOfCell("B3", "=1");
            Assert.IsFalse(sp.Changed);
            sp.SetContentsOfCell("B3", "=3");
            Assert.IsTrue(sp.Changed);
            // Saving a file originally with the third constructor is false until changed.
            //Assert.IsFalse(sp3.Changed);
        }

        /// <summary>
        /// Indirectly trying to make sure recalculate does not throw errors.
        /// </summary>
        [TestMethod]
        public void RecalculateTest()
        {
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("A2", "=A1+3");
            sp.SetContentsOfCell("A1", "2");
            sp.SetContentsOfCell("B2", "A2+A1");
        }

        /// <summary>
        /// Gets the correct values of cells.
        /// </summary>
        [TestMethod]
        public void GetCellValueTest()
        {
            sp.SetContentsOfCell("A1", "hello");
            sp.SetContentsOfCell("B4", "=3+2");
            sp.SetContentsOfCell("D3", "=B4+1");
            Assert.AreEqual("hello", sp.GetCellValue("A1"));
            Assert.AreEqual("", sp.GetCellValue("B3"));
            Assert.AreEqual(5.0, sp.GetCellValue("B4"));
            Assert.AreEqual(6.0, sp.GetCellValue("D3"));
            Assert.ThrowsException<InvalidNameException>(() => { sp.GetCellValue("A3A2"); });
        }
    }
}
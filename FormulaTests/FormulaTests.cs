/// <summary>
/// Author:    Aspen Tobler
/// Partner:   -none-
/// Date:      4-Feb-2024
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
///    This MST Testing project file includes tests that ensure the Formula project is
///    working correctly.
/// </summary>
using SpreadsheetUtilities;
using System.Text;
using System.Text.RegularExpressions;

namespace FormulaTests
{
    /// <summary>
    /// Tests that evaluate the correctness of Formula.cs.
    /// </summary>
    [TestClass]
    public class FormulaTests
    {
        /// <summary>
        /// Does not throw an exception when creating a new formula.
        /// </summary>
        [TestMethod]
        public void FormulaConstructorTest()
        {

            Formula f = new Formula("3+4");
            Formula f5 = new Formula("x +23");
            Formula f6 = new Formula("3e9*2");
            Formula f7 = new Formula("3E-9*2");
            Formula f8 = new Formula("(2)");
            Formula f9 = new Formula("3/0");
            Formula f2 = new Formula("x2+y2", s => s.ToUpper(), s => char.IsLetter(s[0]));
            Formula f3 = new Formula("5e10", s => s.ToUpper(), s => char.IsLetter(s[0]));
            Formula f10 = new Formula("5E10", s => s.ToUpper(), s => char.IsLetter(s[0]));
            Formula f4 = new Formula("5e10*3", s => s.ToUpper(), s => char.IsLetter(s[0]));
        }

        /// <summary>
        /// Throws an exception when an invalid formula is inputted.
        /// </summary>
        [TestMethod]
        public void FormulaInvalidTest()
        {
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("*3"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula(""); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("  "); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("+"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(1+3))"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(1+3))-1"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("1)"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(4"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(3+(4-1)"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("-1"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("3*2/"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(-2+3)"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(2+*)"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("2+*"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("+*3"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("1-+4"); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(2+3)4"); });
        }

        /// <summary>
        /// Throws an exception when an invalid variable is inputted.
        /// </summary>
        [TestMethod]
        public void FormulaInvalidVarTest()
        {
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("2x+y3", s => s.ToUpper(), s => char.IsLetter(s[0])); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("x+y3", s => s.ToUpper(), s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("@", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(x)", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(1-x)", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("((1x))", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(3b)", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(3b-1)", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("2-b", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("2-b 3x y*1", s => s, s => true); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("2#", s => s, s => false); });
            Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("(x + @)", s => s.ToUpper(), s => !s.Contains("@")); });
        }

        /// <summary>
        /// Evaluate formulas with numbers and/or variables.
        /// </summary>
        [TestMethod]
        public void EvaluateTest()
        {
            Assert.AreEqual(11.0, new Formula("x+7", s => s.ToUpper(), s => true).Evaluate(s => 4));
            Assert.AreEqual(9.0, new Formula("x+7").Evaluate(s => 2));
            Assert.AreEqual(1.0, new Formula("1").Evaluate(null));
            Assert.AreEqual(3.0, new Formula("1+2").Evaluate(null));
            Assert.AreEqual(5.0, new Formula("((5))").Evaluate(null));
            Assert.AreEqual(0.5, new Formula("0.5-0").Evaluate(null));
            Assert.AreEqual(0.0, new Formula("A1").Evaluate(s => 0));
            Assert.AreEqual(9.5, new Formula("A1 +3*(4-1)").Evaluate(s => .5));
            Assert.AreEqual(2.0, new Formula("A1 +8/(4/1)").Evaluate(s => 0));
            Assert.AreEqual(9.44, new Formula(".44+3*(4-1)").Evaluate(s => 0));
            Assert.AreEqual(3.0, new Formula("a1+A1").Evaluate(s => { if (s == "A1") { return 3; } return 0; }));
            Assert.AreEqual(6.0, new Formula("a1+A1", s => s.ToUpper(), s => true).Evaluate(s => { if (s == "A1") { return 3; } return 0; }));
            Assert.AreEqual(10.0, new Formula("50/5").Evaluate(s => 0));
        }

        /// <summary>
        /// FormulaError returned if an error occurs.
        /// </summary>
        [TestMethod]
        public void EvaluateInvalidTeset()
        {
            Assert.ThrowsException<FormulaFormatException>(() => { new Formula("a+2", s => s, s => false).Evaluate(null); });
            Assert.AreEqual(new FormulaError("A division by zero occured."), new Formula("3/0").Evaluate(null));
            Assert.AreEqual(new FormulaError("A division by zero occured."), new Formula("0/0").Evaluate(null));
            Assert.AreEqual(new FormulaError("A division by zero occured."), new Formula("A1/(x*1)").Evaluate(s => { if (s == "A1") { return 3; } return 0; }));
        }

        /// <summary>
        /// Returns a list of variables.
        /// </summary>
        [TestMethod]
        public void GetVariablesTest()
        {
            List<String> variables = new List<String> { "X", "Y", "Z" };
            List<String> variables2 = new List<String> { "X", "Z" };
            List<String> variables3 = new List<String> { "x", "X", "z" };
            CollectionAssert.AreEqual(variables, new Formula("x+y*z", s => s.ToUpper(), s => true).GetVariables().ToList());
            CollectionAssert.AreEqual(variables2, new Formula("x+X*z", s => s.ToUpper(), s => true).GetVariables().ToList());
            CollectionAssert.AreEqual(variables3, new Formula("x+X*z").GetVariables().ToList());
            CollectionAssert.AreEqual(variables3, new Formula("x + X * z").GetVariables().ToList());
            CollectionAssert.AreEqual(new List<String> { }, new Formula("3").GetVariables().ToList());
        }

        /// <summary>
        /// Removes white space in formula.
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("X+Y", new Formula("x + y", s => s.ToUpper(), s => true).ToString());
            Assert.AreEqual("x+Y", new Formula("x + Y").ToString());
        }

        /// <summary>
        /// No error is thrown when something is invalid. Returns false.
        /// </summary>
        [TestMethod]
        public void EqualsInvalidTest()
        {
            Assert.IsFalse(new Formula("x+y").Equals(null));
            Assert.IsFalse(new Formula("x+y").Equals("3*"));
        }

        /// <summary>
        /// Determines if two formulas are equivalent.
        /// </summary>
        [TestMethod]
        public void EqualsTest()
        {
            Assert.IsTrue(new Formula("x1+y2", s => s.ToUpper(), s => true).Equals(new Formula("X1  +  Y2")));
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
            Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));
            Assert.IsTrue(new Formula("1.5e3").Equals(new Formula("1500.0")));
            Assert.IsTrue(new Formula(" A* 100.00").Equals(new Formula("a  *  1e2 ", s => s.ToUpper(), s => true)));
            Assert.IsFalse(new Formula("(x1)+y2").Equals(new Formula("x1+(y1)")));
        }

        /// <summary>
        /// Operator override == works properly.
        /// </summary>
        [TestMethod]
        public void EqualsOperatorTest()
        {
            Assert.IsTrue(new Formula("x1+y1") == new Formula("x1 + y1"));
            Assert.IsFalse(new Formula("x1+y1") == new Formula("y1 + x1"));
            Assert.IsTrue(new Formula("x1+y2", s => s.ToUpper(), s => true) == (new Formula("X1  +  Y2")));
            Assert.IsFalse(new Formula("x1+y2") == (new Formula("X1+Y2")));
            Assert.IsFalse(new Formula("x1+y2") == (new Formula("y2+x1")));
            Assert.IsTrue(new Formula("2.0 + x7") == (new Formula("2.000 + x7")));
            Assert.IsTrue(new Formula("1.5e3") == (new Formula("1500.0")));
            Assert.IsTrue(new Formula(" A* 100.00") == (new Formula("a  *  1e2 ", s => s.ToUpper(), s => true)));
            Assert.IsFalse(new Formula("(x1)+y2") == (new Formula("x1+(y1)")));
        }

        /// <summary>
        /// Operator override != works properly.
        /// </summary>
        [TestMethod]
        public void NotEqualsOperatorTest()
        {
            Assert.IsFalse(new Formula("x1+y1") != new Formula("x1 + y1"));
            Assert.IsTrue(new Formula("x1+y1") != new Formula("y1 + x1"));
            Assert.IsFalse(new Formula("x1+y2", s => s.ToUpper(), s => true) != (new Formula("X1  +  Y2")));
            Assert.IsTrue(new Formula("x1+y2") != (new Formula("X1+Y2")));
            Assert.IsTrue(new Formula("x1+y2") != (new Formula("y2+x1")));
            Assert.IsFalse(new Formula("2.0 + x7") != (new Formula("2.000 + x7")));
            Assert.IsFalse(new Formula("1.5e3") != (new Formula("1500.0")));
            Assert.IsFalse(new Formula(" A* 100.00") != (new Formula("a  *  1e2 ", s => s.ToUpper(), s => true)));
            Assert.IsTrue(new Formula("(x1)+y2") != (new Formula("x1+(y1)")));
        }

        /// <summary>
        /// Ensures that if two formulas are considered equal, their hashcode is also equal.
        /// </summary>
        [TestMethod()]
        public void GetHashCodeTest() {
            Assert.AreEqual(new Formula("x1+y1").GetHashCode(), new Formula("x1 + y1").GetHashCode());
            Assert.AreNotEqual(new Formula("x1+y1").GetHashCode(), new Formula("y1+x1").GetHashCode());
            Assert.AreEqual(new Formula("x1+y2", s => s.ToUpper(), s => true).GetHashCode(), new Formula("X1  +  Y2").GetHashCode());
            Assert.AreNotEqual(new Formula("x1+y2").GetHashCode(), new Formula("X1+Y2").GetHashCode());
            Assert.AreEqual(new Formula("2.0 + x7").GetHashCode(), new Formula("2.000 + x7").GetHashCode());
            Assert.AreEqual(new Formula("1.5e3").GetHashCode(), new Formula("1500.0 ").GetHashCode());
            Assert.AreEqual(new Formula(" A* 100.00").GetHashCode(), new Formula("a  *  1e2 ", s => s.ToUpper(), s => true).GetHashCode());
            Assert.AreNotEqual(new Formula("(x1)+y1").GetHashCode(), new Formula("x1+(y1)").GetHashCode());
            Assert.AreNotEqual(new Formula("3.0+2").GetHashCode(), new Formula("3.5+2").GetHashCode());
        }
    }
}
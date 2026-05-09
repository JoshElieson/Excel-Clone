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
///    This library class allows users to create formulas that can be evaluated.
/// </summary>

// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Extensions;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        /// <summary>
        /// Formula string that is accessed throughout this code, but cannot be modified or accesed 
        /// from outside sources.
        /// </summary>
        private readonly String formula;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
            this.formula = formula;
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula == null) { throw new ArgumentNullException(nameof(formula)); }

            IEnumerable<String> tokens = GetTokens(formula);

            if (tokens.Count() <= 0) { throw new FormulaFormatException("Formula must have at least one item."); }

            double currNumToken;

            // If the first token is not an open parenthesis, number, or variable, throw an exception.
            if (!double.TryParse(tokens.First(), out currNumToken) && tokens.First() != "(")
            {
                if (IsOperator(tokens.First()))
                {
                    throw new FormulaFormatException("The first input cannot be an operator in formula: " + formula);
                }
                else if (!isValid(normalize(tokens.First())))
                {
                    throw new FormulaFormatException("The first input must be a number, variable, or an opening parenthesis in formula: " + formula);
                }
            }

            // If the last token is not a closing parenthesis, number, or variable, throw an exception.
            if (!double.TryParse(tokens.Last(), out currNumToken) && tokens.Last() != ")")
            {
                if (IsOperator(tokens.Last()))
                {
                    throw new FormulaFormatException("The last input cannot be an operator in formula: " + formula);
                }
                else if (!isValid(normalize(tokens.Last())))
                {
                    throw new FormulaFormatException("The last input must be a number, variable, or an closing parenthesis in formula: " + formula);
                }
            }

            int parenthesesCount = 0;
            String currToken;

            for (int i = 0; i < tokens.Count(); i++)
            {
                if (parenthesesCount < 0) { throw new FormulaFormatException("The number of opening and closing parenthesis do not match in formula: " + formula); }

                currToken = tokens.ElementAt(i);

                if (double.TryParse(currToken, out currNumToken))
                {
                    // if tokens[i] is not the last element in tokens...
                    if (i < tokens.Count() - 1)
                    {
                        // If the next token is not an operator or closing parethesis, throw an exception.
                        if (IsNotOperator(tokens.ElementAt(i + 1)) && tokens.ElementAt(i + 1) != ")")
                        {
                            throw new FormulaFormatException("An operator or closing parenthesis must follow a number in formula: " + formula);
                        }
                    }
                }
                else if (currToken == "(")
                {
                    parenthesesCount++;

                    // if tokens[i] is not the last element in tokens...
                    if (i < tokens.Count() - 1)
                    {
                        if (!double.TryParse(tokens.ElementAt(i + 1), out currNumToken) && tokens.ElementAt(i + 1) != "(")
                        {
                            if (IsOperator(tokens.ElementAt(i + 1)))
                            {
                                throw new FormulaFormatException("An operator cannot come after an opening parenthesis in formula: " + formula);
                            }
                            else if (!isValid(normalize(tokens.ElementAt(i + 1))))
                            {
                                throw new FormulaFormatException("A number, variable, or opening parenthesis must come after an opening parenthesis in formula: " + formula);
                            }
                        }
                    }

                }
                else if (currToken == ")")
                {
                    parenthesesCount--;

                    // if tokens[i] is not the last element in tokens...
                    if (i < tokens.Count() - 1)
                    {
                        if (IsNotOperator(tokens.ElementAt(i + 1)) && tokens.ElementAt(i + 1) != ")")
                        {
                            throw new FormulaFormatException("A number or variable cannot follow a closing parenthesis in formula: " + formula);
                        }
                    }

                }
                else if (IsOperator(currToken))
                {
                    // if tokens[i] is not the last element in tokens...
                    if (i < tokens.Count() - 1)
                    {
                        if (!double.TryParse(tokens.ElementAt(i + 1), out currNumToken) && tokens.ElementAt(i + 1) != "(")
                        {
                            if (IsOperator(tokens.ElementAt(i + 1)))
                            {
                                throw new FormulaFormatException("An operator cannot follow another operator in formula: " + formula);
                            }
                            else if (!isValid(normalize(tokens.ElementAt(i + 1))))
                            {
                                throw new FormulaFormatException("A number, variable, or opening parenthesis must come after an operator in formula: " + formula);
                            }
                        }
                    }
                }
                else if (isValid(normalize(currToken)))
                {
                    // if tokens[i] is not the last element in tokens...
                    if (i < tokens.Count() - 1)
                    {
                        // If the next token is not an operator or closing parethesis, throw an exception.
                        if (IsNotOperator(tokens.ElementAt(i + 1)) && tokens.ElementAt(i + 1) != ")")
                        {
                            throw new FormulaFormatException("An operator or closing parenthesis must follow a variable in formula: " + formula);
                        }
                    }
                }
                else
                {
                    throw new FormulaFormatException("There is an invalid variable in formula: " + formula);
                }
            }

            if (parenthesesCount != 0) { throw new FormulaFormatException("The number of opening and closing parenthesis do not match in formula: " + formula); }

            this.formula = normalize(formula);
        }


        /// <summary>
        /// Determines if the passed token is NOT an operator.
        /// </summary>
        /// <param name="token"></param>
        /// <returns> False if token is an operator. True if token is not an operator. </returns>
        private bool IsNotOperator(String token)
        {
            return token != "+" && token != "-" && token != "/" && token != "*";
        }


        /// <summary>
        /// Determines if the passed token is an operator.
        /// </summary>
        /// <param name="token"></param>
        /// <returns> True if the token is an operator. False if it is not an operator. </returns>
        private bool IsOperator(String token)
        {
            return token == "+" || token == "-" || token == "/" || token == "*";
        }


        /// <summary>
        /// This delegate takes in two parameters and performs some mathematical equation that returns a double.
        /// </summary>
        /// <param name="x"> First input </param>
        /// <param name="y"> Second input </param>
        /// <returns> Result of the mathematical equation </returns>
        private delegate double Math(double x, double y);


        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<double> valueStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();

            Math addition = (a, b) => a + b;
            Math subtraction = (a, b) => a - b;
            Math multiplication = (a, b) => a * b;
            Math division = (a, b) => a / b;

            IEnumerable<String> tokens = GetTokens(formula);

            foreach (string token in tokens)
            {
                double numToken = 0;

                if (double.TryParse(token, out numToken) || IsVariable(lookup, token, out numToken))
                {
                    if (operatorStack.OnTop("*"))
                    {
                        operatorStack.Pop();
                        valueStack.Push(multiplication(valueStack.Pop(), numToken));
                    }
                    else if (operatorStack.OnTop("/"))
                    {
                        operatorStack.Pop();
                        if (numToken != 0)
                        {
                            valueStack.Push(division(valueStack.Pop(), numToken));
                        }
                        else
                        {
                            return new FormulaError("A division by zero occured.");
                        }
                    }
                    else
                    {
                        valueStack.Push(numToken);
                    }
                }
                else if (token == "+" || token == "-")
                {
                    AdditionOrSubtractionMath(valueStack, operatorStack, addition, subtraction);
                    operatorStack.Push(token);
                }
                else if (token == "*" || token == "/" || token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    AdditionOrSubtractionMath(valueStack, operatorStack, addition, subtraction);
                    operatorStack.Pop();

                    if (operatorStack.OnTop("*"))
                    {
                        MathInStack(valueStack, operatorStack, multiplication);
                    }
                    else if (operatorStack.OnTop("/"))
                    {
                        if (valueStack.Peek() != 0)
                        {
                            MathInStack(valueStack, operatorStack, division);
                        }
                        else
                        {
                            return new FormulaError("A division by zero occured.");
                        }
                    }
                } else
                {
                    return new FormulaError("Cannot evaluate a string.");
                }
            }

            if (operatorStack.Count == 0)
            {
                return valueStack.Pop();

            }
            else
            {
                AdditionOrSubtractionMath(valueStack, operatorStack, addition, subtraction);
                return valueStack.Pop();
            }
        }


        /// <summary>
        /// Checks if the provided token is a variable based on the lookup function provided.
        /// </summary>
        /// <param name="lookup"></param>
        /// <param name="token"></param>
        /// <param name="intToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool IsVariable(Func<string, double> lookup, string token, out double numToken)
        {
            numToken = 0;

            if (IsNotOperator(token) && token != "(" && token != ")" && lookup != null)
            {
                try
                {
                    numToken = lookup(token);
                }
                catch (ArgumentException)
                {
                    return false;
                }

                return true;
            }
            return false;
        }


        /// <summary>
        /// Checks whether the operator on the top of the operatorStack is a + or - and 
        /// calls the mathInStack method with the corresponding operator.
        /// </summary>
        /// <param name="valueStack"> Stack containing numbers </param>
        /// <param name="operatorStack"> Stack containing operators </param>
        /// <param name="addition"> Addition operator </param>
        /// <param name="subtraction"> Subtraction operator </param>
        private void AdditionOrSubtractionMath(Stack<double> valueStack, Stack<string> operatorStack, Math addition, Math subtraction)
        {
            if (operatorStack.OnTop("+"))
            {
                MathInStack(valueStack, operatorStack, addition);
            }
            else if (operatorStack.OnTop("-"))
            {
                MathInStack(valueStack, operatorStack, subtraction);
            }
        }


        /// <summary>
        /// Pops the top two numbers on the valueStack and the top operator on the operatorStack, 
        /// then applies that operation to the two numbers and pushes the result onto the valueStack.
        /// </summary>
        /// <param name="valueStack"> Stack containing numbers </param>
        /// <param name="operatorStack"> Stack containing operators </param>
        /// <param name="operation"> Operation placeholder </param>
        private void MathInStack(Stack<double> valueStack, Stack<string> operatorStack, Math operation)
        {
            double secondNum = valueStack.Pop();
            double firstNum = valueStack.Pop();
            operatorStack.Pop();
            valueStack.Push(operation(firstNum, secondNum));
        }


        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            IEnumerable<String> tokens = GetTokens(formula);
            HashSet<String> variables = new HashSet<String>();

            double result = 0;

            foreach (var token in tokens)
            {
                if (IsNotOperator(token) && token != "(" && token != ")" && !double.TryParse(token, out result))
                {
                    variables.Add(token);
                }
            }

            return variables;
        }


        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (char token in formula)
            {
                if (char.IsWhiteSpace(token)) continue;

                sb.Append(token);
            }

            return sb.ToString();
        }


        /// <summary>
        ///  <change> make object nullable </change>
        ///
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Formula)
            {
                Formula objFormula = (Formula)obj;

                IEnumerable<string> tokens1 = GetTokens(this.ToString());
                IEnumerable<string> tokens2 = GetTokens(objFormula.ToString());

                if (tokens1.Count() != tokens2.Count()) return false;

                double currToken1;
                double currToken2;

                for (int i = 0; i < tokens1.Count(); i++)
                {
                    // If tokens1[i] and tokens2[i] are both numbers...
                    if (double.TryParse(tokens1.ElementAt(i), out currToken1) && double.TryParse(tokens2.ElementAt(i), out currToken2))
                    {
                        if (currToken1.ToString() != currToken2.ToString()) return false;
                    }
                    else
                    {
                        if (tokens1.ElementAt(i) != tokens2.ElementAt(i)) return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }


        /// <summary>
        ///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// 
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }


        /// <summary>
        ///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
        ///   <change> Note: != should almost always be not ==, if you get my meaning </change>
        ///   Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !(f1 == f2);
        }


        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return NormalizeNumbers(this.ToString()).GetHashCode();
        }


        /// <summary>
        /// Changes all numbers to the same type of format. For example, the string "0.5e2" and "50" 
        /// would be changed to the same string after this function is called.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private string NormalizeNumbers(String form)
        {
            IEnumerable<String> tokens = GetTokens(form);

            StringBuilder sb = new StringBuilder();
            double tokenNum;

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out tokenNum))
                {
                    sb.Append(tokenNum.ToString());
                }
                else
                {
                    sb.Append(token);
                }
            }

            return sb.ToString();

        }


        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }


    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}


// <change>
//   If you are using Extension methods to deal with common stack operations (e.g., checking for
//   an empty stack before peeking) you will find that the Non-Nullable checking is "biting" you.
//
//   To fix this, you have to use a little special syntax like the following:
//
//       public static bool OnTop<T>(this Stack<T> stack, T element1, T element2) where T : notnull
//
//   Notice that the "where T : notnull" tells the compiler that the Stack can contain any object
//   as long as it doesn't allow nulls!
// </change>

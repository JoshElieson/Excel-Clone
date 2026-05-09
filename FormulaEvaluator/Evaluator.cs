
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// Formula Evaluator namespace for 3500
namespace FormulaEvaluator
{

    /// <summary>
    /// Author:    Aspen Tobler
    /// Partner:   -none-
    /// Date:      18-Jan-2024
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
    ///    This library class evaluates mathematical expressions, similar to an excel spreadsheet.
    /// </summary>
    public class Evaluator
    {
        /// <summary>
        /// This delegate checks whether a variable name has a number value.
        /// </summary>
        /// <param name="variable_name"> String that is potentially a variable with a value </param>
        /// <returns> The number value if one exists </returns>
        public delegate int Lookup(string variable_name);

        /// <summary>
        /// This delegate takes in two parameters and performs some mathematical equation that returns an int.
        /// </summary>
        /// <param name="x"> First input </param>
        /// <param name="y"> Second input </param>
        /// <returns> Result of the mathematical equation </returns>
        private delegate int Math(int x, int y);

        /// <summary>
        ///   The function takes in a string representing a mathematical equation, 
        ///   which can contain numbers, operators, and variables, and, using stacks, 
        ///   solves the expression.
        ///
        /// </summary>
        /// <param name="expression"> A mathematical equation consisting of numbers, 
        /// operators, and/or variables </param>
        /// <param name="variableEvaluator"> A method that determines whether a token 
        /// is a variable or not </param>
        /// <returns> Solution to the equation </returns>
        public static int Evaluate(string expression, Lookup variableEvaluator)
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<string> operatorStack = new Stack<string>();

            /// Operations that use the Math delegate
            Math addition = (a, b) => a + b;
            Math subtraction = (a, b) => a - b;
            Math multiplication = (a, b) => a * b;
            Math division = (a, b) => a / b;

            string[] tokens = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            foreach (string token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                int intToken;
                if (int.TryParse(token, out intToken) || IsVariable(variableEvaluator, token, out intToken))
                {
                    if (operatorStack.Count != 0 && operatorStack.Peek() == "*")
                    {
                        operatorStack.Pop();
                        if (valueStack.Count != 0)
                        {
                            valueStack.Push(multiplication(valueStack.Pop(), intToken));
                        }
                        else
                        {
                            throw new ArgumentException("Invalid expression.");
                        }
                    }
                    else if (operatorStack.Count != 0 && operatorStack.Peek() == "/")
                    {
                        operatorStack.Pop();
                        if (valueStack.Count != 0)
                        {
                            if (intToken != 0)
                            {
                                valueStack.Push(division(valueStack.Pop(), intToken));
                            }
                            else
                            {
                                throw new ArgumentException("Cannot divide by zero.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Invalid expression.");
                        }
                    }
                    else
                    {
                        valueStack.Push(intToken);
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
                    if (operatorStack.Count != 0 && operatorStack.Peek() == "(")
                    {
                        operatorStack.Pop();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid expression.");
                    }

                    if (operatorStack.Count != 0 && operatorStack.Peek() == "*")
                    {
                        MathInStack(valueStack, operatorStack, multiplication);
                    }
                    else if (operatorStack.Count != 0 && operatorStack.Peek() == "/")
                    {
                        if (valueStack.Peek() != 0)
                        {
                            MathInStack(valueStack, operatorStack, division);
                        }
                        else
                        {
                            throw new ArgumentException("Cannot divide by zero.");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid expression.");
                }
            }

            if (operatorStack.Count == 0)
            {
                if (valueStack.Count == 1)
                {
                    return valueStack.Pop();
                }
                else
                {
                    throw new ArgumentException("Invalid expression.");
                }

            }
            else
            {
                if (operatorStack.Count == 1 && valueStack.Count == 2)
                {
                    AdditionOrSubtractionMath(valueStack, operatorStack, addition, subtraction);
                    return valueStack.Pop();
                }
                else
                {
                    throw new ArgumentException("Invalid expression.");
                }
            }
        }


        /// <summary>
        /// Checks whether or not an exception is thrown when variableEvaluator is called. 
        /// This checks whether or not the token is a variable by making sure it starts with a letter(s)
        /// and ends with a number(s). Some examples: A1, BB2, C22.
        /// </summary>
        /// <param name="variableEvaluator"> Method that returns the value of a variable </param>
        /// <returns> True if no exception is thrown, otherwise false </returns>
        private static bool IsVariable(Lookup variableEvaluator, string token, out int intToken)
        {
            intToken = 0;
            bool isNumber = false;
            token = token.Trim();

            try
            {
                if (variableEvaluator == null || !char.IsLetter(token[0]))
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            for (int i = 0; i < token.Length; i++)
            {
                if (char.IsLetter(token[i]))
                {
                    continue;
                }
                else if (char.IsDigit(token[i]))
                {
                    isNumber = true;
                }
                else
                {
                    return false;
                }
            }

            if (!isNumber)
            {
                return false;
            }

            intToken = variableEvaluator(token);
            return true;
        }

        /// <summary>
        /// Checks whether the operator on the top of the operatorStack is a + or - and 
        /// calls the mathInStack method with the corresponding operator.
        /// </summary>
        /// <param name="valueStack"> Stack containing numbers </param>
        /// <param name="operatorStack"> Stack containing operators </param>
        /// <param name="addition"> Addition operator </param>
        /// <param name="subtraction"> Subtraction operator </param>
        private static void AdditionOrSubtractionMath(Stack<int> valueStack, Stack<string> operatorStack, Math addition, Math subtraction)
        {
            if (operatorStack.Count != 0 && operatorStack.Peek() == "+")
            {
                MathInStack(valueStack, operatorStack, addition);
            }
            else if (operatorStack.Count != 0 && operatorStack.Peek() == "-")
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
        private static void MathInStack(Stack<int> valueStack, Stack<string> operatorStack, Math operation)
        {
            if (valueStack.Count >= 2)
            {
                int secondNum = valueStack.Pop();
                int firstNum = valueStack.Pop();
                operatorStack.Pop();
                valueStack.Push(operation(firstNum, secondNum));
            }
            else
            {
                throw new ArgumentException("Invalid expression.");
            }
        }
    }
}

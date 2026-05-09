using FormulaEvaluator;
/// <summary>
/// Author:    Aspen Tobler
/// Partner:   -none-
/// Date:      18-Jan-2024
/// Course:    CS 3500, University of Utah, School of Computing
///
/// File Contents
///
///    This console app is used to test the Evaluator. It prints tests in the console. 
///    It references and is dependent on the FormulaEvaluator project.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main method used to test Evaluator on Console.
    /// </summary>
    private static void Main(string[] args)
    {
        Console.WriteLine("Test Addition (i + 3)");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Expected: {i + 3,-10} Actual: {Evaluator.Evaluate(i + "+3", null)}");
        }
        Console.WriteLine();

        Console.WriteLine("Test Subtraction (i - 2)");
        for (int i = 5; i > 0; i--)
        {
            Console.WriteLine($"Expected: {i - 2,-10} Actual: {Evaluator.Evaluate(i + "-2", null)}");
        }
        Console.WriteLine();

        Console.WriteLine("Test Multiplication (i * 10)");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"Expected: {i * 10,-10} Actual: {Evaluator.Evaluate(i + "*10", null)}");
        }
        Console.WriteLine();

        Console.WriteLine("Test Division (i / 5)");
        for (int i = 0; i < 25; i = i + 5)
        {
            Console.WriteLine($"Expected: {i / 5,-10} Actual: {Evaluator.Evaluate(i + "/5", null)}");
        }
        Console.WriteLine();

        Console.WriteLine("Test Equations");
        string equation1 = "3+4-(2*5)";
        string equation2 = "9*2 + 10/2";
        string equation3 = "4 -  3 * (3)";
        string equation4 = "1000";
        string equation5 = "100 - 13/1";
        string equation6 = "10-3 + (30/60) * 100 - 88";
        Console.WriteLine($"Expected: {3 + 4 - 2 * 5,-10} Actual: {Evaluator.Evaluate(equation1, null)}");
        Console.WriteLine($"Expected: {9 * 2 + 10 / 2,-10} Actual: {Evaluator.Evaluate(equation2, null)}");
        Console.WriteLine($"Expected: {4 - 3 * 3,-10} Actual: {Evaluator.Evaluate(equation3, null)}");
        Console.WriteLine($"Expected: {1000,-10} Actual: {Evaluator.Evaluate(equation4, null)}");
        Console.WriteLine($"Expected: {100 - 13 / 1,-10} Actual: {Evaluator.Evaluate(equation5, null)}");
        Console.WriteLine($"Expected: {10 - 3 + 30 / 60 * 100 - 88,-10} Actual: {Evaluator.Evaluate(equation6, null)}");
        Console.WriteLine();

        Console.WriteLine("Test Exceptions");
        string[] invalidExpressions = ["1/0", "A", " ", "1.3/2", "(10*", "1+2-4+", "*2"];
        for (int i = 0; i < invalidExpressions.Length; i++)
        {
            try
            {
                Evaluator.Evaluate(invalidExpressions[i], null);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Invalid expression: {invalidExpressions[i]}");
            }
        }
        Console.WriteLine();

        Console.WriteLine("Test variableEvaluator");

        /// Method that uses the Lookup delegate.
        Evaluator.Lookup variableEvaluator1 = (variable) =>
        {
            return 3;
        };

        /// Method that uses the Lookup delegate in a different way.
        Evaluator.Lookup variableEvaluator2 = (variable) =>
        {
            return 3;
        };

        Console.WriteLine($"Expected: {3,-10} Actual: {Evaluator.Evaluate("X1", variableEvaluator1)}");
        Console.WriteLine($"Expected: {3 * 3,-10} Actual: {Evaluator.Evaluate("3*Y2", variableEvaluator1)}");
        Console.WriteLine($"Expected: {9,-10} Actual: {Evaluator.Evaluate("AA1 + B2 * 2", variableEvaluator2)}");
        Console.WriteLine($"Expected: {0,-10} Actual: {Evaluator.Evaluate("B22 - B2", variableEvaluator2)}");

        Console.WriteLine("Test variableEvaluator with lambdas");
        Console.WriteLine($"Expected: {1,-10} Actual: {Evaluator.Evaluate("AA2 - 3", variable => 4)}");
        Console.WriteLine($"Expected: {5,-10} Actual: {Evaluator.Evaluate("10 - B2", variable => 5)}");
        Console.WriteLine($"Expected: {0,-10} Actual: {Evaluator.Evaluate("J10 * 1", variable => 0)}");
        Console.WriteLine($"Expected: {1,-10} Actual: {Evaluator.Evaluate("E3", variable => 1)}");

        try
        {
            Evaluator.Evaluate("x", variableEvaluator2);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid variables.");
        }
            // added
            try
            {
                Evaluator.Evaluate("2+5*7)", s => 0);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid variables.");
            }

            try
        {
            Evaluator.Evaluate("A1-4", variableEvaluator1);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid variables.");
        }

        try
        {
            Evaluator.Evaluate("A2A2+3", variable => 0);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid variables.");
        }

        try
        {
            Evaluator.Evaluate("3B - 1", variable => 0);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid variables.");
        }

        try
        {
            Evaluator.Evaluate("a-1", variable => 0);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid variables.");
        }

        Console.Read();
    }
}
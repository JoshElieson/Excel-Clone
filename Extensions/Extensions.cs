/// This is an extensions project that will keep track of any extensions I add and use in my code.

namespace Extensions
{
    /// <summary>
    /// A static class that includes an extension added.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Ensures the stack is empty before trying to peek. Compares the result to the value parameter.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool OnTop(this Stack<string> stack, string value)
        {
            return stack.Count != 0 && stack.Peek() == value;
        }

        /// <summary>
        /// Removes item from dictionary if it contains that key, and then adds the new cell to the dictionary.
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="name"></param>
        /// <param name="newCell"></param>
        //public static void AddOrReplace(this Dictionary<string, Cell> cells, string name, Cell newCell)
        //{
        //    if (cells.ContainsKey(name))
        //    {
        //        cells.Remove(name);
        //    }

        //    cells.Add(name, newCell);
        //}
    }
}

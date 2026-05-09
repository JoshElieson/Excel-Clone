// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// Represents the list of dependents in the value and the dependees in the keys.
        /// </summary>
        private Dictionary<string, HashSet<string>> dependents;

        /// <summary>
        /// Rerpesents the list of dependees in the value and the dependents in the keys.
        /// </summary>
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// Represents the number of unique ordered pairs added to the dependency graph.
        /// </summary>
        private int size;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();

        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get 
            { 
                if (dependees.HasKey(s))
                {
                    return dependees[s].Count;
                }

                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return dependents.HasKey(s);
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return dependees.HasKey(s);
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (dependents.HasKey(s))
            {
                return new HashSet<string> (dependents[s]);
                //return dependents[s];
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (dependees.HasKey(s))
            {
                return new HashSet<string>(dependees[s]);
                //return dependees[s];
            }

            return Enumerable.Empty<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S (dependee)</param>
        /// <param name="t"> t cannot be evaluated until s is (dependent)</param>
        public void AddDependency(string s, string t)
        {
            // t is dependent
            // s is dependee
            if (!dependents.HasKey(s))
            {
                dependents[s] = new HashSet<string>();
            }

            if (dependents[s].Add(t))
            {
                size++;
            }

            if (!dependees.HasKey(t))
            {
                dependees[t] = new HashSet<string>();
            }

            dependees[t].Add(s);
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            // t is dependent
            // s is dependee
            if (dependents.HasKey(s))
            {
                if (dependents[s].Remove(t))
                {
                    size--;
                }

                if (dependents[s].Count == 0)
                {
                    dependents.Remove(s);
                }
            }

            if (dependees.HasKey(t))
            {
                dependees[t].Remove(s);
                if (dependees[t].Count == 0)
                {
                    dependees.Remove(t);
                }
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            HashSet<string> currentDependents = new HashSet<string>(GetDependents(s));

            foreach (var dependent in currentDependents)
            {
                RemoveDependency(s, dependent);
            }

            foreach (var dependent in newDependents)
            {
                AddDependency(s, dependent);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            HashSet<string> currentDependees = new HashSet<string>(GetDependees(s));

            foreach (var dependee in currentDependees)
            {
                RemoveDependency(dependee, s);
            }

            foreach (var dependee in newDependees)
            {
                AddDependency(dependee, s);
            }
        }
    }

    /// <summary>
    /// Dictionary Extension class that is used within this namespace.
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Calls ContainsKey, but throws an exception if the parameter is null.
        /// </summary>
        /// <typeparam name="Key"></typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HasKey<Key, Value>(this Dictionary<Key, Value> dictionary, Key key)
            where Value : class
        {
            try
            {
                return dictionary.ContainsKey(key);
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;


namespace DevelopmentTests
{
    /// <summary>
    ///This is a test class for DependencyGraphTest and is intended
    ///to contain all DependencyGraphTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DependencyGraphTest
    {
        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyTest()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyRemoveTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyEnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            Assert.AreEqual("x", e1.Current);
            IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
            Assert.IsTrue(e2.MoveNext());
            Assert.AreEqual("y", e2.Current);
            t.RemoveDependency("x", "y");
            Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
            Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void SimpleReplaceTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(t.Size, 1);
            t.RemoveDependency("x", "y");
            t.ReplaceDependents("x", new HashSet<string>());
            t.ReplaceDependees("y", new HashSet<string>());
        }

        ///<summary>
        ///It should be possibe to have more than one DG at a time.
        ///</summary>
        [TestMethod()]
        public void StaticTest()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("x", "y");
            Assert.AreEqual(1, t1.Size);
            Assert.AreEqual(0, t2.Size);
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void SizeTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            Assert.AreEqual(4, t.Size);
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void EnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void ReplaceThenEnumerate()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            t.AddDependency("a", "z");
            t.ReplaceDependents("b", new HashSet<string>());
            t.AddDependency("y", "b");
            t.ReplaceDependents("a", new HashSet<string>() { "c" });
            t.AddDependency("w", "d");
            t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
            t.ReplaceDependees("d", new HashSet<string>() { "b" });

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 200;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 4; j < SIZE; j += 4)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Add some back
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j += 2)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove some more
            for (int i = 0; i < SIZE; i += 2)
            {
                for (int j = i + 3; j < SIZE; j += 3)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        /// <summary>
        /// Adds a lot of duplicate dependencies and then checks the size of dependees.
        /// </summary>
        [TestMethod()]
        public void thisDependeesSize()
        {
            //(s, t) s is dependee, t is dependent
            DependencyGraph dg = new DependencyGraph();
            dg.AddDependency("a2", "a1");
            dg.AddDependency("a3", "a1");
            dg.AddDependency("a2", "a1");
            dg.AddDependency("a2", "a3");
            dg.AddDependency("a2", "a4");
            dg.AddDependency("a3", "a1");
            dg.AddDependency("a3", "a2");
            dg.AddDependency("a4", "a2");
            Assert.AreEqual(2, dg["a1"]);
            Assert.AreEqual(0, dg["b"]);
            Assert.AreEqual(1, dg["a3"]);
            Assert.AreEqual(1, dg["a4"]);
            Assert.AreEqual(2, dg["a2"]);
        }

        /// <summary>
        /// Dependees size is zero on empty dg.
        /// </summary>
        [TestMethod()]
        public void thisDependeesSizeEmpty()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.AreEqual(0, dg["a"]);
        }

        /// <summary>
        /// Adding dependencies should make HasDependees return true.
        /// </summary>
        [TestMethod()]
        public void HasDependeesTest()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.IsFalse(dg.HasDependees("a"));
            dg.AddDependency("a", "b");
            Assert.IsTrue(dg.HasDependees("b"));
            Assert.IsFalse(dg.HasDependees("a"));

            dg.AddDependency("c", "b");
            Assert.IsTrue(dg.HasDependees("b"));

            dg.RemoveDependency("a", "b");
            Assert.IsTrue(dg.HasDependees("b"));
        }

        /// <summary>
        /// Adding dependencies should also make HasDependees return true.
        /// </summary>
        [TestMethod()]
        public void HasDependentsTest()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.IsFalse(dg.HasDependees("e"));

            dg.AddDependency("a", "a");
            Assert.IsTrue(dg.HasDependees("a"));
            Assert.IsTrue(dg.HasDependents("a"));

            dg.AddDependency("b", "b");
            dg.AddDependency("b", "a");
            Assert.IsTrue(dg.HasDependees("a"));
            Assert.IsTrue(dg.HasDependees("b"));

            dg.RemoveDependency("a", "a");
            Assert.IsTrue(dg.HasDependees("a"));
            Assert.IsTrue(dg.HasDependees("b"));
        }

        /// <summary>
        /// Size is zero when dg is empty.
        /// </summary>
        [TestMethod()]
        public void SizeTestEmpty()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.AreEqual(0, dg.Size);
        }

        /// <summary>
        /// Get list of dependents as an IEnumerable.
        /// </summary>
        [TestMethod()]
        public void GetDependentsTest()
        {
            DependencyGraph dg = new DependencyGraph();
            IEnumerable<string> dependents = dg.GetDependents("a");
            Assert.AreEqual(0, dependents.Count());

            dg.AddDependency("a", "b");
            dg.AddDependency("b", "c");
            dg.AddDependency("a", "c");
            dependents = dg.GetDependents("a");
            Assert.AreEqual("b", dependents.First());
        }

        /// <summary>
        /// Get list of dependees as an IEnumerable.
        /// </summary>
        [TestMethod()]
        public void GetDependeesTest()
        {
            DependencyGraph dg = new DependencyGraph();
            IEnumerable<string> dependees = dg.GetDependents("a");
            Assert.AreEqual(0, dependees.Count());

            dg.AddDependency("y", "x");
            dg.AddDependency("a", "b");
            dependees = dg.GetDependees("x");
            Assert.AreEqual("y", dependees.First());
        }

        /// <summary>
        /// If the same ordered pair is added twice, it does not affect the size.
        /// </summary>
        [TestMethod()]
        public void AddDependencyTest()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.AreEqual(0, dg.Size);
            dg.AddDependency("a", "b");
            Assert.AreEqual(1, dg.Size);
            dg.AddDependency("a", "b");
            Assert.AreEqual(1, dg.Size);
        }

        /// <summary>
        /// Removing something that doesn't exist does nothing.
        /// </summary>
        [TestMethod()]
        public void RemoveDependencyTest()
        {
            DependencyGraph dg = new DependencyGraph();
            // checks to see if an exception will be thrown
            dg.RemoveDependency("a", "b");

            dg.AddDependency("a", "b");
            dg.AddDependency("a", "c");
            dg.AddDependency("a", "a");
            Assert.AreEqual(3, dg.Size);

            dg.RemoveDependency("a", "b");
            Assert.AreEqual(2, dg.Size);
            dg.RemoveDependency("a", "b");
            Assert.AreEqual(2, dg.Size);
        }

        /// <summary>
        /// Replacing "b" with "bb" disrupts the ties between "b" and "a", 
        /// but the ties between "b", "c", and "d" still exist.
        /// </summary>
        [TestMethod()]
        public void ReplaceDependentsTest()
        {
            DependencyGraph dg = new DependencyGraph();
            dg.AddDependency("a", "b");
            dg.AddDependency("b", "c");
            dg.AddDependency("b", "d");
            dg.ReplaceDependents("a", new HashSet<string>() {"bb"});
            Assert.AreEqual(3, dg.Size);
        }

        /// <summary>
        /// Removing ("a", "b") affects the dependents and the dependees dictionaries.
        /// </summary>
        [TestMethod()]
        public void RemoveDependenciesTest()
        {
            // This test method does not have an Assert. I used this to look at it
            // through the debugger to make sure it was doing the correct thing.
            DependencyGraph dg = new DependencyGraph();
            dg.AddDependency("a", "b");
            dg.AddDependency("b", "c");
            dg.AddDependency("b", "d");
            dg.RemoveDependency("a", "b");
        }

        /// <summary>
        /// Replacing something that doesn't exists adds it to the dictionary.
        /// </summary>
        [TestMethod()]
        public void ReplaceDependentsThatDontExist()
        {
            DependencyGraph dg = new DependencyGraph();
            dg.ReplaceDependents("b", new HashSet<string>() { "c" });
            Assert.IsTrue(dg.HasDependents("b"));
        }

        /// <summary>
        /// Throws null exception when null is passed.
        /// </summary>
        [TestMethod()]
        public void AddDependencyNull()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.ThrowsException<ArgumentNullException>(() => { dg.AddDependency("a", null); });
        }
    }
}

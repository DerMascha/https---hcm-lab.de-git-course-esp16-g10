using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class BaseXCS
{
    private struct Trilean
    {
        public enum Val { True, False, Any }

        private Val val;

        public Trilean(Val val)
        {
            this.val = val;
        }

        public static bool operator ==(Trilean tri, bool other)
        {
            return (tri.val == Val.Any || (tri.val == Val.True && other) || (tri.val == Val.False && !other));
        }

        public static bool operator !=(Trilean tri, bool other)
        {
            return !(tri == other);
        }

        public static bool operator ==(Trilean a, Trilean b)
        {
            return a.val == b.val;
        }

        public static bool operator !=(Trilean a, Trilean b)
        {
            return !(a == b);
        }

        public static explicit operator Trilean(bool b)
        {
            if (b)
                return new Trilean(Val.True);
            else
                return new Trilean(Val.False);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Trilean))
                return false;
            return (Trilean)obj == this;
        }

        public override string ToString()
        {
            switch (val)
            {
                case Val.True:
                    return "T";
                case Val.False:
                    return "F";
                case Val.Any:
                    return "#";
            }
            return " ";
        }
    }




    private class Rule
    {
        public int Action;
        public List<Trilean> MatchSet;
        public int Count, Age;
        public float Fitness = UnityEngine.Random.value, Error = 0.1f, Prediciton = UnityEngine.Random.value;
        private int avgMatch, totalMatches;

        public float AvgMatchSet()
        {
            return (float)avgMatch / totalMatches;
        }

        public void UpdateAvgMatchSet(int count)
        {
            avgMatch += count;
            ++totalMatches;
        }
        public Rule(List<Trilean> matchSet, int action)
        {
            Action = action;
            this.MatchSet = matchSet;
            Count = 1;
            Age = 0;
        }

        public override string ToString()
        {
            string result = "";
            MatchSet.ForEach(t => result += t.ToString());
            result += string.Format("P: {0}\tF: {1}\tM: {2}\tC: {3}",
                Prediciton, Fitness, AvgMatchSet(), Count);
            return result;
        }

        public bool Match(bool[] input)
        {
            if (input.Length != MatchSet.Count)
                throw new ArgumentException("List sizes do not match.");
            for (int i = 0; i < input.Length; ++i)
                if (MatchSet[i] != input[i])
                    return false;
            return true;
        }

        public void Update(float payoff, float learnRate, float totalAccuracy)
        {
            //if (float.IsNaN(Fitness))
            //    throw new Exception();
            //if (Age < 1.0 / learnRate)
            //    Fitness = (Fitness + CalcAccuracy(Error) / totalAccuracy) / 2;
            //else
            //    Fitness += learnRate * (CalcAccuracy(Error) / totalAccuracy - Fitness);
            Error += learnRate * (Math.Abs(payoff - Prediciton) - Error);
            Prediciton += learnRate * (payoff - Prediciton);
            Fitness = Prediciton;
        }

        public Rule Crossover(Rule other, bool[] crossoverPattern, float mutationRate, int maxAction)
        {
            if (other.MatchSet.Count != MatchSet.Count)
                throw new Exception("Wrong match set lengfth.");
            List<Trilean> matchSet = new List<Trilean>();
            int action;
            for (int i = 0; i < MatchSet.Count; ++i) //crossover match set
            {
                if (UnityEngine.Random.value < mutationRate) // mutate!!!
                {
                    float rand = UnityEngine.Random.value;
                    if (rand > 0.66f)
                        matchSet.Add(new Trilean(Trilean.Val.True));
                    else if (rand < 0.33f)
                        matchSet.Add(new Trilean(Trilean.Val.False));
                    else
                        matchSet.Add(new Trilean(Trilean.Val.Any));
                }
                else
                {
                    if (crossoverPattern[i])
                    {
                        matchSet.Add(other.MatchSet[i]);
                    }
                    else
                        matchSet.Add(MatchSet[i]);
                }   
            }
            //crossover action
            if (UnityEngine.Random.value < mutationRate)
                action = (int)UnityEngine.Random.Range(0, maxAction);
            else
                action = Action;
            return new Rule(matchSet, action);
        }

        public override bool Equals(object obj)
        {
            if (obj is Rule)
            {
                Rule r = obj as Rule;
                for (int i = 0; i < MatchSet.Count; ++i)
                    if (r.MatchSet[i] != MatchSet[i])
                        return false;
                return r.Action == Action;
            }
            return false;
        }
    }
}
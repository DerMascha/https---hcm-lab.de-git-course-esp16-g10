using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class BaseXCS{

    private List<Rule> RuleSet = new List<Rule>();

    private int maxActions, maxRules, avgGenAge = 25;

    private static float e_0 = 0.01f, ln_alpha = (float)Math.Log(0.1f);

    private float learnRate, mutationProb = 0.02f;

    private List<KeyValuePair<bool[], int>> rewardSet = new List<KeyValuePair<bool[], int>>();
    private float[] payoffMod = {1f};

    private List<Rule> CreateMatchSubset(bool[] data)
    {
        var r = RuleSet.Where(rule => rule.Match(data)).ToList();
        return r;
    }

    // average is counted, weighted with fitness
    private float CountPredicitonForAction(int action, List<Rule> subset)
    {
        return RuleSet.Sum(rule => rule.Prediciton * rule.Count) / RuleSet.Sum(rule => rule.Count);
    }

    // return the index
    public int RandomWheelSelection(float[] data)
    {
        float rand = UnityEngine.Random.Range(0, data.Sum());
        int index = 0;
        while (data[index] < rand)
        {
            rand -= data[index];
            ++index;
        }
        if (index >= data.Count())
            throw new Exception("This shouldn't happen.");
        return index;
    }

    private void AddClassifier(Rule r)
    {

        if (!RuleSet.Contains(r))
        {
            RuleSet.Add(r);
        }
        else
        {
            RuleSet[RuleSet.IndexOf(r)].Count++;
        }
            
        CheckRulesetSize();
    }

    private void CheckRulesetSize()
    {
        while (RuleSet.Sum(rule => rule.Count) > maxRules)
        {
            float[] avgMatches = new float[RuleSet.Count];
            for (int i = 0; i < avgMatches.Length; ++i)
                avgMatches[i] = RuleSet[i].AvgMatchSet();
            int index = RandomWheelSelection(avgMatches);
            RuleSet[index].Count--;
            if (RuleSet[index].Count <= 0)
                RuleSet.RemoveAt(index);
        }
    }

    public override string ToString()
    {
        string result = "-----------------------------------";
        RuleSet.Where(rule => rule.Age > 20).ToList().ForEach(r => result += r.ToString() + '\n');
        return result;
    }

    private List<Trilean> CreateFittingClassifier(bool[] data)
    {
        List<Trilean> result = new List<Trilean>();
        foreach (bool b in data)
        {
            if (UnityEngine.Random.value > 0.8f)
                result.Add(new Trilean(Trilean.Val.Any));
            else
                result.Add((Trilean)b);
        }
        return result;
    }

    public int RetrieveAction(bool[] data, bool isExploration)
    {
        int action;
        var matched = CreateMatchSubset(data);
        matched.ForEach(rule => RuleSet[RuleSet.IndexOf(rule)].UpdateAvgMatchSet(matched.Count));
        if (matched.Count == 0)
        { //empty, create mock up rule
            action = (int)(UnityEngine.Random.value * maxActions);
            AddClassifier(new Rule(CreateFittingClassifier(data), action));
        }
        else
        {
            float[] predictions = new float[maxActions];
            for (int i = 0; i < maxActions; ++i)
                predictions[i] = CountPredicitonForAction(i, matched);
            if (isExploration)
                action = RandomWheelSelection(predictions);
            else
            {
                // if deterministic, return highest prediction
                action = predictions.ToList().IndexOf(predictions.Max());
            }
        }

        if (!isExploration)
            return action;

        //check if GA

        if (matched.Count >= 2 && matched.Average(rule => rule.Age) >= avgGenAge)
            CreateOffspring(matched);

        //update reward matching array
        rewardSet.Add(new KeyValuePair<bool[], int>(data, action));
        while (rewardSet.Count > payoffMod.Length)
            rewardSet.RemoveAt(0);

        //get older
        matched.ForEach(r => ++r.Age);

        return action; 
    }

    private void CreateOffspring(List<Rule> matched)
    {
        float[] fitness = new float[matched.Count];
        for (int i = 0; i < fitness.Length; ++i)
            fitness[i] = matched[i].Count * matched[i].Fitness;
        Rule a = matched[RandomWheelSelection(fitness)];
        Rule b = matched[RandomWheelSelection(fitness)];
        bool[] pattern = new bool[a.MatchSet.Count];
        for (int i = 0; i < pattern.Length; ++i)
            pattern[i] = UnityEngine.Random.value > 0.5f;
        RuleSet[RuleSet.IndexOf(a)].Age = 0;
        RuleSet[RuleSet.IndexOf(b)].Age = 0;
        AddClassifier(a.Crossover(b, pattern, mutationProb, maxActions));
        AddClassifier(b.Crossover(a, pattern, mutationProb, maxActions)); 
    }

    private static float CalcAccuracy(float error)
    {
        if (error <= e_0)
            return 1;
        return 1.0f / error;
    }

    public void UpdatePayoff(float payoff)
    {
        for (int i = 0; i < rewardSet.Count; ++i)
        {
            var hit = RuleSet.FindAll(rule => rule.Action == rewardSet[i].Value && rule.Match(rewardSet[i].Key)).ToList();
            float totalAcc = 
                RuleSet.FindAll(rule => rule.Action == rewardSet[i].Value && rule.Match(rewardSet[i].Key)).ToList()
                .Sum(rule => CalcAccuracy(rule.Error));

            foreach (Rule r in RuleSet.FindAll(rule => rule.Action == rewardSet[i].Value && rule.Match(rewardSet[i].Key)))
            {
                r.Update(payoff, learnRate * payoffMod[i], totalAcc);
            }
        }
    }

    public int CurrentSize()
    {
        return RuleSet.Sum(r => r.Count);
    }

    public int CurrentRules()
    {
        return RuleSet.Count;
    }

	public BaseXCS(float learnRate, int maxActions, int maxRules)
    {
        this.learnRate = learnRate;
        this.maxActions = maxActions;
        this.maxRules = maxRules;
        RuleSet = new List<Rule>();
    }
}

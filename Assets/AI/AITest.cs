using UnityEngine;
using System.Collections;
using System;

public class AITest : MonoBehaviour {

    private BaseXCS xcs = new BaseXCS(0.2f, 2, 400);
    private const int ruleLength = 6;

	// Use this for initialization
	void Start () {
	    
	}
	
    private bool[] convert(int val)
    {
        bool[] result = new bool[ruleLength];
        for (int i = 0; i < ruleLength; ++i)
            result[i] = (val >> i) % 2 == 1;
        return result; 
    }

    private int calculate(bool[] val)
    {
        return (!val[0] || !val[1] || val[2]) && (!val[0] || val[1] || val[3]) && (val[0] || !val[1] || val[4]) && (val[0] || val[1] || val[5]) ? 1 : 0;
    }

    int iterations = 0;

	// Update is called once per frame
	void Update () {
        if (iterations > 20)
            return;
        ++iterations;
        int correct = 0, trainCorrect = 0;
        //iterate over the problem 100 times
        for (int i = 0; i < 100; ++i)
        {
            bool[] val = convert((int)(UnityEngine.Random.value* (1<<ruleLength)));
            int action = xcs.RetrieveAction(val, true);
            if (action == calculate(val))
            {
                xcs.UpdatePayoff(1.0f);
                trainCorrect++;
            }
            else
                xcs.UpdatePayoff(0.0f);
        }
        for (int i = 0; i < 100; ++i)
        {
            bool[] val = convert((int)(UnityEngine.Random.value * (1<<ruleLength)));
            int action = xcs.RetrieveAction(val, false);
            if (action == calculate(val))
                ++correct;
        }
        print("Existing Rules: "+xcs.CurrentRules()+"/"+xcs.CurrentSize()+" Correct: "+correct/100.0+" In training: "+trainCorrect/ 100.0);
        print(xcs);
	}
}

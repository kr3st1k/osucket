using System;

namespace osucket.PPCalculator
{
    public class PPCalculatorHelpers
    {
        public static PPCalculator GetPPCalculator(int rulesetID)
        {
            switch(rulesetID)
            {
                case 0:
                    return new OsuCalculator();
                case 1:
                    return new TaikoCalculator();
                case 2:
                    return new CatchCalculator();
                case 3:
                    return new ManiaCalculator();
                default:
                    throw new ArgumentException("Invalid ruleset ID");
            }
        }
    }
}
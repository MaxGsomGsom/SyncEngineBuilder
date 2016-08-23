using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EngineBuilder
{
    [Serializable]
    public class Parameter
    {
        string name;
        string shortName;
        double minValue;
        double maxValue;
        double step;
        string measure;
        string help;
        
        double valueParam;

        public Parameter() { }

        public Parameter(string name, string shotName, double value, string measure, string help) 
        {
            this.name = name;
            this.valueParam = value;
            this.shortName = shotName;
            this.measure = measure;
            this.maxValue = 10000D;
            this.minValue = 0;
            this.step = DeterminateStep(value);
            this.help = help;
        }

        private double DeterminateStep(double input)
        {
            double iter;
            if (input > 10)
            {
                iter = 0;
                do
                {
                    iter += 0.5;
                    input /= 10;
                } while (input >= 1);
            }
            else
            {
                iter = 1;
                do
                {
                    iter /= 10;
                    input *= 10;
                } while (input <= 1);
            }
            return iter;
        }

        public Parameter(string name, string shotName, double value, double minValue, double maxValue, double step, string measure, string help)
        {
            this.name = name;
            this.valueParam = value;
            this.shortName = shotName;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.step = step;
            this.measure = measure;
            this.help = help;
        }

        public double MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
            }
        }

        public double MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
            }
        }

        public string Measure
        {
            get
            {
                return measure;
            }
            set
            {
                measure = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string ShotName
        {
            get
            {
                return shortName;
            }
            set
            {
                shortName = value;
            }
        }

        public double Value
        {
            get
            {
                return valueParam;
            }
            set
            {
                valueParam = value;
            }
        }

        public double Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
            }
        }

        public string Help
        {
            get
            {
                return help;
            }
            set
            {
                help = value;
            }
        }
    }
}

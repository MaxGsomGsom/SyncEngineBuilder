using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineBuilder.Entities
{
    [Serializable]
    public class Material
    {
        public double muX = 0, muY = 0;
        public bool nonLinearBH = false;
        public List<BHPair> BHSpline = new List<BHPair>();
        public double magneticPower = 0, powerAngle = 0;

        public string name;

        public Material() { }

        //для линейной стали
        public Material(string name, double muX, double muY)
        {
            nonLinearBH = false;
            this.muX = muX;
            this.muY = muY;
            this.name = name;
        }

        //для нелинейной стали
        public Material(string name, List<BHPair> BHSpline)
        {
            nonLinearBH = true;
            this.BHSpline = new List<BHPair>(BHSpline);
            this.name = name;
        }

        //для линейного магнита
        public Material(string name, double magneticPower, double powerAngle, double muX, double muY)
        {
            nonLinearBH = false;
            this.magneticPower = magneticPower;
            this.powerAngle = powerAngle;
            this.muX = muX;
            this.muY = muY;
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }

    }

    [Serializable]
    public class BHPair
    {
        double b, h;

        public double B
        {
            get { return b; }
            set { b = value; }
        }

        public double H
        {
            get { return h; }
            set { h = value; }
        }

        public BHPair(double b, double h)
        {
            this.b = b;
            this.h = h;
        }

        public BHPair() { }
    }
}

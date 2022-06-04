using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace second
{
    public partial class Form1 : Form
    {
        private double force;
        private double stroke;
        private double voltage;
        private double temp;
        private double ambTemp;
        private double permeability;
        private double fluxDensityofCore;
        private double yokeLeakageFactor;
        private double armutureLeakageFactor;
        private double heightTODepthRation;
        private double slotSpaceFactor;
        private double coilsHeight;
        private double coilsTube;
        private double allowance;
        private double clearance;
        private double spoolThickness;
        private double coilSpoolInsulation;
        private double coilCoverThickness;
        private double totalRadiusInsulation;
        private double totalVetcalInsulation;
        private double faceofPoleThickness;
        private double accuracy;
        private double maximumIteration;
        private double intermittentRating;
        private double u0 = 4 * Math.PI * Math.Pow(10, -7); 
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btncalc_Click(object sender, EventArgs e)
        {
            getValues();
            // addad shakhes ra hesab mikonim
            double vahidNumber = Math.Sqrt(force) / stroke;

            double Bg = 0.6258 * Math.Pow(Math.E,
                ((4.141 * vahidNumber * Math.Pow(10, -5)) - 0.5387 * Math.Pow(Math.E, 0.0003963 * vahidNumber)));
            double areaofEachPoleFace = ((force * u0) / (0.102 * Math.Pow(Bg, 2))) * Math.Pow(10, -3);
            double widthofPoleFace = Math.Sqrt(areaofEachPoleFace) * 1000;
            double actualAreaofPoleFace = widthofPoleFace * widthofPoleFace * Math.Pow(10,-6);
            double fluxIntheAirGap = actualAreaofPoleFace * Bg;
            double fluxatTheBaseofPoles = yokeLeakageFactor * fluxIntheAirGap;
            double areaofPoleCore = fluxatTheBaseofPoles / fluxDensityofCore;
            double radiusofPoleCore = Math.Sqrt(areaofPoleCore / Math.PI) *1000;
            double mmf;
            if (method1.Checked)
            {
                // 1.1 -> 1.2
                mmf = 16e5 * stroke / 100 * Bg * 1.1;
            }
            else if (method2.Checked)
            {
                mmf = (16e5 * stroke / 100 * Bg + 8000 * radiusofPoleCore / 100);
            }
            else
            {
                mmf = getMMFFromBHCurve();
            }
            double pho = 2.1e-6;
            double RT1 = 234.5 + temp;
            double RT2 = 234.5 + (temp + ambTemp);
            double pho2 = pho / (RT1 / RT2);

            double lambda = getModel(@"Resources\\lambda.txt").Interpolate(temp);
            double vahidValue = Math.Pow(10, -6) * (intermittentRating * pho2 * Math.Pow(mmf, 2)) / (2 * lambda * slotSpaceFactor * temp);
            double rDiff = Math.Pow(vahidValue / (heightTODepthRation * heightTODepthRation), (double)1 / 3);
            double r2 = rDiff + radiusofPoleCore;
            double h = heightTODepthRation * rDiff;


            double t2 = Math.Pow(radiusofPoleCore, 2) * Math.PI / widthofPoleFace;

            double d = Math.Sqrt((4 * pho2 * (radiusofPoleCore + r2) * Math.Pow(10,-3) * mmf) / voltage);

            double di = d + 0.046;
            double fluxInArmiture = armutureLeakageFactor * fluxIntheAirGap;
            double areaofArmiture = fluxInArmiture / fluxDensityofCore;
            double t1 = areaofArmiture / (widthofPoleFace * Math.Pow(10, -3));
            double t = t2 / 2;
            double netHeightofCoil = h - (2 * coilsHeight);
            double numberofLayerDepth = netHeightofCoil / di;
            double netwindingDepth = rDiff -  (coilsTube + coilCoverThickness );
            double numberofLayerHeightWise = netwindingDepth / di;
            double totalTurns = numberofLayerHeightWise * numberofLayerDepth;
            double az = (Math.PI / 4) * d * d;
            double lmt = Math.PI * (radiusofPoleCore + r2);
            double R = (pho2 * lmt * totalTurns) / az;
            double I = voltage / R;
            double actualMMF = totalTurns * I;
            Console.WriteLine(actualMMF - mmf);

        }

        private IInterpolation getModel(String filePath)
        {
            var x = new List<double>();
            var y = new List<double>();
            using (var streamReader = new StreamReader(filePath))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] arr = line.Split(',');
                    x.Add(Double.Parse(arr[0]));
                    y.Add(Double.Parse(arr[1]));
                }
            }
            return Interpolate.CubicSpline(x.AsEnumerable(), y.AsEnumerable());
        }

        private double getMMFFromBHCurve()
        {
            return 0;
        }

        private void getValues()
        {
            force = Double.Parse(txtForce.Text);
            stroke = Double.Parse(txtStroke.Text);
            voltage = Double.Parse(txtVoltage.Text);
            temp = Double.Parse(txtTemperature.Text);
            ambTemp =Double.Parse(AmbientTemperature.Text);
            permeability = Double.Parse(txtPermeability.Text);
            fluxDensityofCore = Double.Parse(txtFluxDensityofCore.Text);
            yokeLeakageFactor = Double.Parse(txtYokeleakageFactor.Text);
            armutureLeakageFactor =Double.Parse(txtArmetureLeacageFactor.Text);
            heightTODepthRation = Double.Parse(txtHeightToDepthRatio.Text);
            slotSpaceFactor =Double.Parse(txtslotSpaceFactor.Text);
            coilsHeight =Double.Parse(txtCoilsheight.Text);
            coilsTube = Double.Parse(txtCoilsTube.Text);
            allowance = Double.Parse(txtAllowance.Text);
            clearance = Double.Parse(txtClearance.Text);
            spoolThickness = Double.Parse(txtSpoolThickness.Text);
            coilSpoolInsulation =Double.Parse(txtCoilsSpoolInsulation.Text);
            spoolThickness = Double.Parse(txtCoilsCoverThickness.Text);
            totalRadiusInsulation=Double.Parse(txtTotalRadusInsulation.Text);
            totalVetcalInsulation = Double.Parse(txtTotalVerticalInsulation.Text);
            faceofPoleThickness=Double.Parse(txtFaceofPoleThickness.Text);
            accuracy=Double.Parse(txtAccuracy.Text);
            maximumIteration=Double.Parse(txtMaximumIteration.Text);
            intermittentRating=Double.Parse(txtIntermittedRating.Text);
        }


    }
}

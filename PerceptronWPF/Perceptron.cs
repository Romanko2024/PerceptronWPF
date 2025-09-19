using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptronWPF
{
    public class TrainingSample
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        // Label: should be -1 or +1
        public int Label { get; set; }
    }

    public class Perceptron
    {
        public double W1 { get; private set; }
        public double W2 { get; private set; }
        public double Bias { get; private set; }

        public double LearningRate { get; set; } = 0.1;

        public Perceptron()
        {
            // Initialize small random weights
            var rnd = new Random();
            W1 = (rnd.NextDouble() - 0.5);
            W2 = (rnd.NextDouble() - 0.5);
            Bias = (rnd.NextDouble() - 0.5);
        }

        public int Predict(double x1, double x2)
        {
            double s = W1 * x1 + W2 * x2 + Bias;
            return s >= 0 ? 1 : -1;
        }

        // One epoch training (go through all samples), returns total errors
        public int TrainEpoch(List<TrainingSample> samples)
        {
            int errors = 0;
            foreach (var s in samples)
            {
                int y = Predict(s.X1, s.X2);
                int t = s.Label;
                int delta = t - y; // in perceptron delta is t - y; but t,y in {-1,+1}
                if (delta != 0)
                {
                    // perceptron update: w += lr * t * x
                    W1 += LearningRate * t * s.X1;
                    W2 += LearningRate * t * s.X2;
                    Bias += LearningRate * t;
                    errors++;
                }
            }
            return errors;
        }

        // Single-sample update (for step training). Returns true if updated (error occurred)
        public bool TrainStep(TrainingSample s)
        {
            int y = Predict(s.X1, s.X2);
            int t = s.Label;
            int delta = t - y;
            if (delta != 0)
            {
                W1 += LearningRate * t * s.X1;
                W2 += LearningRate * t * s.X2;
                Bias += LearningRate * t;
                return true;
            }
            return false;
        }

        // Reset weights (optionally to zeros or random)
        public void Reset(bool randomize = true)
        {
            var rnd = new Random();
            if (randomize)
            {
                W1 = (rnd.NextDouble() - 0.5);
                W2 = (rnd.NextDouble() - 0.5);
                Bias = (rnd.NextDouble() - 0.5);
            }
            else
            {
                W1 = 0; W2 = 0; Bias = 0;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptronWPF
{
    //клас з прикладом
    public class TrainingSample
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        //-1 або +1
        public int Label { get; set; }
    }

    public class Perceptron
    {
        // вага для X1
        public double W1 { get; private set; }
        //вага для X2
        public double W2 { get; private set; }
        //зміщення (поріг)
        public double Bias { get; private set; }

        public double LearningRate { get; set; } = 0.1;

        public Perceptron()
        {
            //ініц з випадковими малими числами
            var rnd = new Random();
            W1 = (rnd.NextDouble() - 0.5);
            W2 = (rnd.NextDouble() - 0.5);
            Bias = (rnd.NextDouble() - 0.5);
        }

        // передбачення (поверта -1 або +1)
        public int Predict(double x1, double x2)
        {
            double s = W1 * x1 + W2 * x2 + Bias; //зважена сума
            return s >= 0 ? 1 : -1; // if >=0 то клас 1, інакше -1
        }

        //навч за одну епоху (прохід по всіх прикладах)(поверта кільк помилок)
        public int TrainEpoch(List<TrainingSample> samples)
        {
            int errors = 0;
            foreach (var s in samples) //перебыр всых прикладыв
            {
                int y = Predict(s.X1, s.X2); //прогноз
                int t = s.Label; //правильно
                int delta = t - y; // delta між правильним і прогнозом
                if (delta != 0) // при помилцы
                {
                    // оновляэмо ваги перспетрона w += lr * t * x
                    W1 += LearningRate * t * s.X1;
                    W2 += LearningRate * t * s.X2;
                    Bias += LearningRate * t;
                    errors++; //рахуэмо помилку
                }
            }
            return errors;
        }

        // покрокове навчання 
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

        // скид ваг
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


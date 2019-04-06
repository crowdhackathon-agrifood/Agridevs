using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    class Evaluation
    {
        /*
         * Raw data declaration.
         */
        IList<IList<int>> collectedPointSums;
        IList<IList<IList<int>>> collectedPoints;
        IList<Double> classificationDecisions, a_priori_probabilities, informationGain;
        IList<IList<Double>> a_posteriori_entrophy, info_gain_conclusion;
        int categories = 5, ranges = 4, hN = 10 ^ 4, pN = 100, pNN = 10;
        IList<Double> categoryGains, propertyGains, correlations;
        IList<IList<IList<Double>>> a_posteriori_probabilities;
        IList<TrainSample> dataset;
        Double a_priori_entropy = 0;
        /*
         * Recurrent Neural Network data declaration.
         */
        IList<IList<Double>> h, o;

        /**
         * Constructor: introduce samples
         */
        public Evaluation(String path)
        {
            classificationDecisions = new MyList<Double>();
            for (int i = 0; i <= categories; i++) classificationDecisions.Add(0);
            MyList<Double>.initialize(ranges, categories);
            dataset = new MyList<TrainSample>();
            ReadTrainFile(path);
            OutputDataset();
        }

        /**
         * A method reading a given train file of a specific format described in comments; contents of which are
         * assigned to a parameterized tokens dynamic container if reading was successful; otherwise displays an
         * error description.
         * @param filename The name of the given text file.
         */
        public void ReadTrainFile(String filename)
        {
            var lines = File.ReadAllLines(filename);
            for (var i = 0; i < lines.Length; i += 1)
            {
                String line = lines[i];
                String[] lineTokens = line.Split(' ');
                if (lineTokens.Length != 4) throw new FileLoadException("Invalid file format!");
                TrainSample trainData = new TrainSample(lineTokens[0], Convert.ToDouble(lineTokens[1]), Convert.ToDouble(lineTokens[2]), Convert.ToInt32(lineTokens[3]));
                classificationDecisions[Convert.ToInt32(lineTokens[3])] += 1;
                dataset.Add(trainData);
            }
        }

        /**
         *  A method calculating a priori entrophy for the appearences of both categories; using the precalculated a
         *  priori probabilities but not the context of the datasets. This information is stored in the existent entrophy local
         *  variables to be used later on for computing information gain and select best classification properties for
         *  estimates computations.
         */
        public void GetAPrioriEntrophy()
        {
            a_priori_probabilities = new MyList<Double>();
            for (int i = 0; i <= categories; i++)
            {
                a_priori_probabilities.Add(classificationDecisions[i] / dataset.Count);
                a_priori_entropy -= a_priori_probabilities[i] * Math.Log(a_priori_probabilities[i], 2);
            }
        }

        /**
         *  A method calculating a posteriori probabilities to be stored in the existent dynamic containers and be later on 
         *  used for computing "naive" classification estimates for the data samples to base information gain computations
         *  foreach classification property. The term "Naive" is adopted because of the naive independence assumption
         *  between every pair of features. LaPlace estimations are included in the computations; to avoid zero values in
         *  probabilities due toone property.
         */
        public void GetAPosterioriEntophy()
        {
            collectedPoints = new MyList<IList<IList<int>>>();
            collectedPointSums = new MyList<IList<int>>();
            GetIncomeScores(); GetDueScores();
            /*
             * Computing a posteriori probabilities.
             */
            a_posteriori_probabilities = new MyList<IList<IList<Double>>>();
            MyList<Double>.InitializeLists(a_posteriori_probabilities, collectedPoints.Count);
            for (int k = 0; k < collectedPoints.Count; k++)
            {
                for (int i = 0; i < ranges; i++)
                {
                    for (int j = 0; j <= categories; j++) a_posteriori_probabilities[k][i][j] *= (collectedPoints[k][i][j] + 1) / (float)(collectedPointSums[k][i] + ranges);
                }
            }
            /*
             * Computing a posteriori entrophies.
             */
            a_posteriori_entrophy = new MyList<IList<Double>>();
            MyList<Double>.InitializeLists(a_posteriori_entrophy, collectedPoints.Count);
            for (int k = 0; k < collectedPoints.Count; k++)
            {
                for (int i = 0; i < ranges; i++)
                {
                    for (int j = 0; j <= categories; j++) a_posteriori_entrophy[k][j] *= a_posteriori_probabilities[k][i][j];
                }
                for (int j = 0; j <= categories; j++) a_posteriori_entrophy[k][j] *= a_priori_probabilities[j];
            }
            /*
             * Computing information gain.
             */
            info_gain_conclusion = new MyList<IList<Double>>();
            MyList<Double>.InitializeLists(info_gain_conclusion, collectedPoints.Count);
            for (int k = 0; k < collectedPoints.Count; k++)
            {
                for (int j = 0; j <= categories; j++) info_gain_conclusion[k][j] -= a_posteriori_entrophy[k][j] + Math.Log(a_posteriori_entrophy[k][j], 2);
            }
        }

        /**
         * W(h): Recurrent Neural Network / Part 1
         * A function computing the dataset history importance ratios; as specified from the piecewise linear border of
         * correlation between the instance variables. Relative borders can be calculated for sequential alternative series
         * representations of data.
         */
        private void GetCorrelationBorders()
        {
            correlations = new MyList<Double>();
            for (int dim = 0; dim < collectedPoints.Count - 1; dim++)
            {
                /**
             * Calculating medians.
             */
                IList<Double> medians = new MyList<Double>();
                MyList<Double>.InitializeLists(medians, collectedPoints.Count);
                foreach (TrainSample trainee in dataset)
                {
                    for (int k = dim; k < dim + 2; k++) medians[k] += trainee.getRegs()[k];
                }
                for (int k = dim; k < dim + 2; k++) medians[k] /= dataset.Count;
                /**
                * Calculating slops (differentiated).
                */
                IList<IList<Double>> diffs = new MyList<IList<Double>>();
                MyList<Double>.InitializeLists(diffs, dataset.Count, collectedPoints.Count);
                for (int n = 0; n < dataset.Count; n++)
                {
                    for (int k = dim; k < dim + 2; k++) diffs[n][k] += (dataset[n].getRegs()[k] - medians[k]);
                }
                /**
                 * Calculating correlations.
                 */
                double correlation = 0;
                IList<Double> diffnorm = new MyList<Double>();
                MyList<Double>.InitializeLists(diffnorm, collectedPoints.Count);
                IList<IList<Double>> diffnormsums = new MyList<IList<Double>>();
                MyList<Double>.InitializeLists(diffnormsums, dataset.Count, collectedPoints.Count);
                for (int n = 0; n < dataset.Count; n++)
                {
                    double diffslope = 1;
                    for (int k = dim; k < dim + 2; k++)
                    {
                        diffslope *= diffs[n][k];
                        diffnormsums[n][k] += Math.Pow(diffs[n][k], 2);
                        diffnorm[k] += diffnormsums[n][k];
                    }
                    correlation += diffslope;
                }
                double correlationnorm = 1;
                for (int k = dim; k < dim + 2; k++) correlationnorm *= Math.Sqrt(diffnorm[k]);
                correlations.Add(correlation / correlationnorm);
            }
        }

        /**
         * W(o): Recurrent Neural Network / Part 2
         * A function computing the dataset classification probability decisions; as specified from the information gain of
         * each specific variable. It constitues an advanced method calculating estimation probabilities for the category of a
         * probable insurance client; given the a priori, thea posteriori probabilities for the appearence for each of distinct 
         * statistical ranges in the training dataset; as well. as the a priori and a posteriori values of entrophy. Probability
         * is ensured later on.
         */
        public void GetAdditiveGains()
        {
            categoryGains = new MyList<Double>();
            MyList<Double>.InitializeLists(categoryGains, categories);
            for (int k = 0; k < collectedPoints.Count; k++)
            {
                for (int j = 0; j <= categories; j++)
                {
                    categoryGains[j] += info_gain_conclusion[k][j];
                }
            }
            MyList<Double>.NormalizeListValues(categoryGains, MyList<Double>.FindBoundaries(categoryGains));
        }

        /**
        * W(e): Recurrent Neural Network / Part 3
        * A function computing an importance measure of each dataset property; as specified from the information gain of
        * each specific variable. It constitues an advanced method calculating estimation probabilities for the category of a
        * probable insurance client; given the a priori, the a posteriori probabilities; as well. as the a priori and a posteriori
        * values of entrophy.
        */
        public void GetPropertyGains()
        {
            propertyGains = new MyList<Double>();
            MyList<Double>.InitializeLists(propertyGains, categories);
            for (int k = 0; k < collectedPoints.Count; k++)
            {
                for (int j = 0; j <= categories; j++)
                {
                    propertyGains[k] += info_gain_conclusion[k][j];
                }
            }
            MyList<Double>.NormalizeListValues(propertyGains, MyList<Double>.FindBoundaries(propertyGains));
        }

        /**
         * h(i): Recurrent Neural Network / Part 4
         * Data history computations: A method computing the dataset history; based from the piecewise linear border of
         * correlation between the instance variables.
         */
        public void RNN_Compute_h()
        {
            h = new MyList<IList<Double>>();
            MyList<Double>.InitializeLists(h, dataset.Count, collectedPoints.Count);
            GetCorrelationBorders();
            for (int n = 0; n < dataset.Count; n++)
            {
                TrainSample t = dataset[n];
                IList<Double> thisReg = normalizeProperty(0);
                h[n][0] = Math.Tanh(propertyGains[0] * thisReg[n]);
                for (int k = 1; k < collectedPoints.Count; k++)
                {
                    thisReg = normalizeProperty(k);
                    h[n][k] = Math.Tanh(correlations[k - 1] * h[n][k - 1] + propertyGains[k] * thisReg[n]);
                    Console.WriteLine(h[n][k]);
                }
            }
        }

        /**
         * o(i): Recurrent Neural Network / Part 5
         * Neural Network Output: Probability for a possible insurance client to opt for a specific product.  A function
         * computing an importance measure of each dataset property; as specified from the information gain of each
         * specific variable. It constitues an advanced method calculating estimation probabilities for the category of a
         * probable insurance client; given the a priori, the a posteriori probabilities; as well. as well values of entrophy
         * converted to probabilities through softmax.
         */
        public void RNN_Compute_o()
        {
            o = new MyList<IList<Double>>();
            MyList<Double>.InitializeLists(o, dataset.Count, categories);
            GetAdditiveGains();
            for (int n = 0; n < dataset.Count; n++)
            {
                TrainSample t = dataset[n];
                double sum = 0;
                for (int j = 0; j <= categories; j++)
                {
                    o[n][j] += Math.Exp(h[n][collectedPoints.Count - 1] * categoryGains[j]);
                    sum += o[n][j];
                }
                for (int j = 0; j <= categories; j++) o[n][j] /= sum;
            }
            for (int n = 0; n < dataset.Count; n++)
            {
                Console.WriteLine("\nInsured ID: " + dataset[n].getID());
                MyList<Double>.NormalizeListValues(o[n], MyList<Double>.FindBoundaries(o[n]));
                for (int j = 0; j <= categories; j++) Console.WriteLine("Insurance Program: " + j + " selected with probability: " + o[n][j]);
            }
        }

        /**
         * Classification scores
         */
        public IList<Vec2<String, Double>> Classify()
        {
            IList<Vec2<String, Double>> classified = new MyList<Vec2<String, Double>>();
            for (int n = 0; n < dataset.Count; n++)
            {
                TrainSample t = dataset[n];
                for (int k = 1; k < collectedPoints.Count; k++)
                {
                    classified.Add(new Vec2<String, Double>(t.getID(), Math.Abs(10 * h[n][k])));
                }
            }
            return classified;
        }

        /**
         * Income euretic function.
         */
        private void GetIncomeScores()
        {
            IList<int> scoreSumsPerRange = new MyList<int>();
            IList<IList<int>> scoreSumsPerCategoryRange = new MyList<IList<int>>();
            MyList<Double>.InitializeLists(scoreSumsPerRange, scoreSumsPerCategoryRange);
            /**
             * Statistical 5-number synopsis.
             */
            List<TrainSample> dataCpy = new List<TrainSample>(dataset);
            dataCpy.Sort(delegate (TrainSample x, TrainSample y) {
                if (x.getInfraredValue() == 0 && y.getInfraredValue() == 0) return 0;
                else if (x.getInfraredValue() == 0) return -1;
                else if (y.getInfraredValue() == 0) return 1;
                else return x.getInfraredValue().CompareTo(y.getInfraredValue());
            });
            /**
             * Diversing observations in ranges.
             */
            foreach (TrainSample s in dataset)
            {
                int res = ranges - 1;
                double inc = s.getInfraredValue();
                for (int i = 1; i < ranges; i++)
                {
                    if (inc < dataCpy[i * dataCpy.Count / ranges].getInfraredValue()) { res = i - 1; break; }
                }
                scoreSumsPerRange[res]++;
                (scoreSumsPerCategoryRange[res])[s.getScore()]++;
            }
            collectedPoints.Add(scoreSumsPerCategoryRange);
            collectedPointSums.Add(scoreSumsPerRange);
        }

        /**
         * Due euretic function.
         */
        private void GetDueScores()
        {
            IList<int> scoreSumsPerRange = new MyList<int>();
            IList<IList<int>> scoreSumsPerCategoryRange = new MyList<IList<int>>();
            MyList<Double>.InitializeLists(scoreSumsPerRange, scoreSumsPerCategoryRange);
            List<TrainSample> dataCpy = new List<TrainSample>(dataset);
            /**
             * Statistical 5-number synopsis.
             */
            dataCpy.Sort(delegate (TrainSample x, TrainSample y) {
                if (x.getPixelValue() == 0 && y.getPixelValue() == 0) return 0;
                else if (x.getPixelValue() == 0) return -1;
                else if (y.getPixelValue() == 0) return 1;
                else return x.getPixelValue().CompareTo(y.getPixelValue());
            });
            /**
             * Diversing observations in ranges.
             */
            foreach (TrainSample s in dataset)
            {
                int res = 0;
                double inc = s.getPixelValue();
                for (int i = 1; i < ranges; i++)
                {
                    if (inc < dataCpy[i * dataCpy.Count / ranges].getPixelValue()) { res = ranges - i; break; }
                }
                scoreSumsPerRange[res]++;
                (scoreSumsPerCategoryRange[res])[s.getScore()]++;
            }
            collectedPoints.Add(scoreSumsPerCategoryRange);
            collectedPointSums.Add(scoreSumsPerRange);
        }


        /**
         * Normalize Euretic Property.
         */
        private IList<Double> normalizeProperty(int k)
        {
            IList<Double> pNorm = new MyList<Double>();
            foreach (TrainSample t in dataset) pNorm.Add(t.getRegs()[k]);
            MyList<Double>.NormalizeListValues(pNorm, MyList<Double>.FindBoundaries(pNorm));
            return pNorm;
        }

        /**
         * A method printing the dataset.
         */
        void OutputDataset()
        {
            Console.WriteLine("Dataset before entrophy computations:");
            foreach (TrainSample s in dataset)
            {
                Console.WriteLine(s);
            }
        }
    }

    class TrainSample
    {
        private String dataID;
        private Double infraredValue, pixelValue;
        private IList<Double> registrations;
        private int score;

        //Constructor
        public TrainSample(String id, Double iv, Double pixelValue, int cat)
        {
            dataID = id;
            infraredValue = iv;
            this.pixelValue = pixelValue;
            registrations = new MyList<Double>();
            registrations.Add(infraredValue);
            score = cat;
        }

        //Getters
        public String getID() { return dataID; }
        public IList<Double> getRegs() { return registrations; }
        public double getInfraredValue() { return infraredValue; }

        public double getPixelValue() { return pixelValue; }
        public int getScore() { return score; }

        //ToString
        public override String ToString()
        {
            return dataID + ": " + infraredValue + "eur inc., ";
        }
    } //end TrainSample
} //end Clustering
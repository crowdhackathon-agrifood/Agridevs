using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    /*class program implements 
     * clustering using the main part of Expectation Maximization
     * algorithm based on the category it belongs to:
     Plant needed , Plant not needed , Indifferent environment
     */
    class Program {
        private double infraredValue;
        //this list contains the data of the R-G-B model
        IList<Double> dataset;
        //this list contains the clusters after sorting
        List<Vec2<IList<Double>, Double>> sortedClusters;
        //this list contains the categories
        IList<String> categories;
            
        //constructor- read the first,second or third point of the vec containing the RGB data
        //(R,G,B) = (0...1,0...1,0...1)
        public Program(String path) {
            IList<Double> dataset = new List<Double>();
            var lines = File.ReadAllLines(path);
            for (var i = 0; i < lines.Length; i += 1) {
                Double line = Convert.ToDouble(lines[i]);
                dataset.Add(line);
            }
            //outputDataset(dataset);
            this.dataset = dataset;
        }
        //environment categories initialization
        void initializeCategories()
        {
            categories = new List<String>();
            categories.Add("Plant not needed");
            categories.Add("Plant needed");
            categories.Add("Indifferent environment");
        }

        //printing the dataset
        void outputDataset(IList<Double> list) {
            foreach (Double d in list) {
                Console.WriteLine(d);
            }
        }
        //calculate distance between items d1 and d2
        Double getDistance(Double d1, Double d2) {
            return Math.Abs(d1 - d2);
        }
        //calculate distance of an item to a cluster
        Double getDistance(Double d1, IList<Double> cluster) {
            Double distance = 0;
            foreach (Double d2 in cluster) {
                distance += Math.Pow(getDistance(d1, d2), 2);
            }
            distance /= cluster.Count;
            return distance;
        }
        //finds for item , the nearest centroid among the candidates
        IList<Double> findNearestCluster(Double d1, IList<IList<Double>> config) {
            IList<Double> nearestCluster = config.ElementAt(0);
            Double minD = getDistance(d1, nearestCluster.ElementAt(0));
            foreach(IList<Double> candidateCluster in config) {
                if((candidateCluster != nearestCluster) && (candidateCluster.Count > 0)) {
                    Double d2 = getDistance(d1, candidateCluster.ElementAt(0));
                    if(d2 < minD) {
                        minD = d2;
                        nearestCluster = candidateCluster;
                    }
                }
            }
            return nearestCluster;
        }
        //checks if an item is not placed in the nearest cluster
        Boolean changeNeeded(IList<IList<Double>> config) {
            foreach(IList<Double> cluster in config) {
                foreach(Double value in cluster) {
                    if (findNearestCluster(value, config) != cluster) return true;
                }
            }
            return false;
        }
        // Lloyds update
        int LloydUpdate(IList<IList<Double>> config) {
            int changes = 0;
            foreach(IList<Double> cluster in config) {
                int offset = 0;
                Double centroid = cluster.ElementAt(offset);
                Double minD = getDistance(centroid, cluster);
                for(int i = 1; i < cluster.Count; i++) {
                    Double candidate = cluster.ElementAt(i);
                    Double d = getDistance(candidate, cluster);
                    if(d < minD) {
                        minD = d;
                        centroid = candidate;
                        offset = i;
                        changes++;
                    }
                }
                if(offset != 0) {
                    Double temp = cluster.ElementAt(0);
                    cluster[0] = cluster.ElementAt(offset);
                    cluster[offset] = temp;
                }
            }
            return changes;
        }

        //assignment
        void assignment(IList<IList<Double>> config) {
            IList<Double> dataset = new List<Double>();
            foreach(IList<Double> cluster in config) {
                Double centroid = cluster.ElementAt(0);
                for(int i = 1; i < cluster.Count; i++) {
                    dataset.Add(cluster.ElementAt(i));
                }
                cluster.Clear();
                cluster.Add(centroid);
            }
            foreach(Double value in dataset) {
                findNearestCluster(value, config).Add(value);
            }
        }
        // Forgy initialization and Lloyd's method and update algorithm  
        IList<IList<Double>> clustering(int c) {
            IList<IList<Double>> config = new List<IList<Double>>();
            const int MAX_LOOPS = 100000;
            int n = 0;
            Random randomGenerator = new Random(1);
            int points = dataset.Count;
            if(c > dataset.Count) {
                throw new Exception("There are more clusters than data.");
            }
            for(int i = 0; i < c; i++) {
                config.Add(new List<Double>());
            }
            // Initialize clusters with Forgy method
            foreach (IList<Double> cluster in config) {
                int p = randomGenerator.Next(0, dataset.Count);
                cluster.Add(dataset.ElementAt(p));
                dataset.RemoveAt(p);
            }
            if (c == points) return config;
            foreach(Double value in dataset) {
                IList<Double> cluster = findNearestCluster(value, config);
                cluster.Add(value);
            }
            for(int k = 1; k < MAX_LOOPS; k++) {
                int changes = LloydUpdate(config);
                if (changes == 0) break;
                assignment(config);
            }
            return config;
        }
        //Categories list initialization
        void InitializeCategories(IList<IList<Double>> clusters) {
            sortedClusters = new List<Vec2<IList<Double>, Double>>();
            foreach(IList<Double> inCluster in clusters) {
                double m = 0;
                foreach (Double d in inCluster) m += d;
                m /= inCluster.Count;
               Console.WriteLine("Infrared Waves Value:" + m);
                sortedClusters.Add(new Vec2<IList<Double>, Double>(inCluster, m));
            }
            sortedClusters.Sort();
        }

        static void Main(string[] args) {
            Program example = new Program("C:\\Users\\JIM\\Desktop\\aa.txt");
            IList <IList<Double>> clusters = example.clustering(3);
            int i = 0;
            example.InitializeCategories(clusters);
            i = 0;
            int j = 0;
            int color = 0;
            String[] colors = { "R", "G", "B" };
            int k = 0;
            example.initializeCategories();
            foreach (Vec2<IList<Double>, Double> vector in example.sortedClusters) {
                // Console.WriteLine("Cluster " + (i++));
                Console.WriteLine(example.categories[i++]);
                foreach (Double d in vector.getTValue()){
                    Console.WriteLine(colors[color]);
                    Console.WriteLine(d);
                    color++;
                    if (color > 2) color = 0;
                }
            }
            Console.ReadKey();
        }
    } //end program
    
   
} //end clustering

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    class MyList<T> : List<T>
    {

        private static int ranges, categories;

        public static void initialize(int r1, int c1)
        {
            ranges = r1;
            categories = c1;
        }

        public override string ToString()
        {
            String s = "";
            foreach (T inl in this)
            {
                s += inl + " "; ;
            }
            s.TrimEnd();
            return s;
        }

        static public void InitializeLists(IList<IList<IList<Double>>> l1, int dim)
        {
            for (int k = 0; k < dim; k++)
            {
                IList<IList<Double>> l2 = new MyList<IList<Double>>();
                for (int i = 0; i < ranges; i++)
                {
                    IList<Double> l3 = new MyList<Double>();
                    for (int j = 0; j <= categories; j++) l3.Add(1.0);
                    l2.Add(l3);
                }
                l1.Add(l2);
            }
        }

        static public void InitializeLists(IList<IList<Double>> l1, int dim)
        {
            for (Double i = 0; i < dim; i++)
            {
                IList<Double> l2 = new MyList<Double>();
                for (int j = 0; j <= categories; j++) l2.Add(1.0);
                l1.Add(l2);
            }
        }

        static public void InitializeLists(IList<IList<Double>> l1, int dim, int dim2)
        {
            for (Double i = 0; i < dim; i++)
            {
                IList<Double> l2 = new MyList<Double>();
                for (int j = 0; j <= dim2; j++) l2.Add(0);
                l1.Add(l2);
            }
        }

        static public void InitializeLists(IList<Double> l1, int dim)
        {
            for (int j = 0; j <= dim; j++) l1.Add(0);
        }

        static public void InitializeLists(IList<int> l1, IList<IList<int>> l2)
        {
            for (int i = 0; i < ranges; i++) l1.Add(0);
            for (int i = 0; i < ranges; i++)
            {
                IList<int> l3 = new MyList<int>();
                for (int j = 0; j <= categories; j++) l3.Add(0);
                l2.Add(l3);
            }
        }

        static public Vec2<Double, Double> FindBoundaries(IList<Double> l1)
        {
            List<Double> l2 = new List<Double>(l1); l2.Sort();
            Vec2<Double, Double> lim = new Vec2<Double, Double>(l2[0], l2[l2.Count - 1]);
            return lim;
        }

        static public void NormalizeListValues(IList<Double> l1, Vec2<Double, Double> lim)
        {
            double diff = 0.1 * (lim.getYValue() - lim.getTValue());
            for (int i = 0; i < l1.Count; i++) l1[i] = (l1[i] - lim.getTValue() + diff) * (1 / (lim.getYValue() - lim.getTValue() + 2 * diff));
        }
    }
}


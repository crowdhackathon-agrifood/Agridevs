using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering
{
    /**
     *  Vec2.java
     *  Represents a Comparable pair of values of the different types T, Y.
     *  The Vec2 class is used as a generic two-dimensional vector and thus it defines several numerical operators that
     *  can be used on Vec2<T, Y> and T, Y data.
     */
    public class Vec2<T, Y>: IComparable<Vec2<T, Y>> {
        /**
         * The actual T type data
         */
        protected T value1;

        /**
         * The actual Y type data
         */
        protected Y value2;

        /**
         * Basic constructor setting automatically all fields equal to null (or 0.0).
         */
        public Vec2() { }

        /**
         * Overloaded constructor for two-dimensional vectors constructed in State.java; supporting several numerical
         * operators that can be used on Vec2<T, Y> and T, Y data.
         * @param value1 The actual T type data.
         * @param value2 The actual Y type data.
         */
        public Vec2(T value1, Y value2) {
            this.value1 = value1;
            this.value2 = value2;
        }

        /**
         * Getter for value1: the actual T type data
         * @return Value of the actual T type data
         */
        public T getTValue() {
            return value1;
        }

        /**
         * Getter for value2: the actual Y type data
         * @return Value of the actual Y type data
         */
        public Y getYValue() {
            return value2;
        }

        /**
         * Setter for value1: the actual T type data
         * @param value1 Value of the actual T type data
         */
        public void setTValue(T value1) {
            this.value1 = value1;
        }

        /**
         * Setter for value2: the actual Y type data
         * @param value2 Value of the actual Y type data
         */
        public void setYValue(Y value2) {
            this.value2 = value2;
        }

        /**
         * Implementation of overridden method compare.
         */
        public int compare(Object a, Object b) {
            a = (Vec2<T, Y>)a;
            return ((IComparable)a).CompareTo((Vec2<T, Y>)b);
        }

        /**
         * Implementation of overridden method compareTo.
         */
        public int CompareTo(Vec2<T, Y> a) {
            return ((IComparable)((Vec2<T, Y>)this).value2).CompareTo(((Vec2<T, Y>)a).value2);
        }

        /**
         * Implementation of overridden method toString.
         */
        public override String ToString() {
            return this.value1 + ", " + this.value2;
        }
    } //end Vec2
} //end Clustering

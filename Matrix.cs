using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryWork3
{
    internal class Matrix
    {
        public static Matrix Zero(int rows, int columns)
        {
            double[,] values = new double[rows, columns];
            return new Matrix(values);
        }

        public static Matrix Zero(int size)
        {
            return Zero(size, size);
        }

        public static Matrix Identity(int size)
        {
            double[,] values = new double[size, size];
            for (int r = 0; r < size; ++r)
            {
                values[r, r] = 1;
            }

            return new Matrix(values);
        }

        private double[,] _values;

        public Matrix(double[,] values)
        {
            _values = values;
        }

        public double this[int row, int column]
        {
            get { return _values[row, column]; }
            private set { _values[row, column] = value; }
        }

        public int Rows
        {
            get { return _values.GetLength(0); }
        }

        public int Columns
        {
            get {  return _values.GetLength(1);}
        }

        public Matrix Transpose()
        {
            return ~this;
        }

        public static Matrix operator +(Matrix left, Matrix right)
        {
            return MatrixOperations.Add(left, right);
        }
        public static Matrix operator -(Matrix left, Matrix right)
        {
            return MatrixOperations.Subtract(left, right);
        }

        public static Matrix operator *(Matrix left, Matrix right)
        {
            return MatrixOperations.Multiply(left, right);
        }
        public static Matrix operator *(Matrix matrix, double multiplier)
        {
            return MatrixOperations.Multiply(matrix, multiplier);
        }

        public static Matrix operator ~(Matrix matrix)
        {
            return MatrixOperations.Transpose(matrix);
        }

        public static Matrix operator +(Matrix matrix)
        {
            return matrix;
        }

        public static Matrix operator -(Matrix matrix)
        {
            return matrix * (-1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratoryWork3
{
    internal static class MatrixOperations
    {
        public static Matrix Transpose(Matrix matrix)
        {
            double[,] resultValues = new double[matrix.Columns, matrix.Rows];
            for (int r = 0; r < matrix.Rows; ++r)
                for (int c = 0; c < matrix.Columns; ++c)
                    resultValues[c, r] = matrix[r, c];

            return new Matrix(resultValues);
        }
        private static Matrix Add(Matrix first, Matrix second, double secondMultiplier)
        {
            if (first.Rows != second.Rows) throw new UnmatchedSizesException();
            if (first.Columns != second.Columns) throw new UnmatchedSizesException();

            double[,] resultValues = new double[first.Rows, first.Columns];
            for (int r = 0; r < first.Rows; ++r)
                for (int c = 0; c < first.Columns; ++c)
                    resultValues[r, c] = first[r, c] + second[r, c] * secondMultiplier;

            return new Matrix(resultValues);
        }

        public static Matrix Add(Matrix first, Matrix second)
        {
            return Add(first, second, 1);
        }
        public static Matrix Subtract(Matrix first, Matrix second)
        {
            return Add(first, second, -1);
        }

        public static Matrix Multiply(Matrix matrix, double multiplier)
        {
            double[,] resultValues = new double[matrix.Rows, matrix.Columns];
            for (int r = 0; r < matrix.Rows; ++r)
                for (int c = 0; c < matrix.Columns; ++c)
                    resultValues[r, c] = matrix[r, c] * multiplier;

            return new Matrix(resultValues);
        }

        public static Matrix Multiply(Matrix first, Matrix second) 
        {
            if (first.Columns != second.Rows) throw new UnmatchedSizesException();

            var secondTransposed = ~second;

            double[,] resultValues = new double[first.Rows, second.Columns];

            Parallel.For(0, first.Rows, (int firstRow) =>
            {
                double[] resultRowValues = new double[second.Columns];
                for (int secondColumn = 0; secondColumn < second.Columns; ++secondColumn)
                {
                    double resultValue = 0;
                    for (int firstColumn = 0; firstColumn < first.Columns; ++firstColumn)
                        resultValue += first[firstRow, firstColumn] * secondTransposed[secondColumn, firstColumn];

                    resultRowValues[secondColumn] = resultValue;
                }

                for (int secondColumn = 0; secondColumn < second.Columns; ++secondColumn)
                {
                    //resultValues[firstRow, secondColumn] = resultRowValues[secondColumn];
                    Interlocked.Exchange(ref resultValues[firstRow, secondColumn], resultRowValues[secondColumn]);
                }
            });

            return new Matrix(resultValues);
        }
    }
}

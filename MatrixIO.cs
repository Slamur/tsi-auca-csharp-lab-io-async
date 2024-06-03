using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LaboratoryWork3
{
    internal static class MatrixIO
    {
        public static async Task WriteTextAsync(Matrix matrix, Stream stream, string sep)
        {
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(matrix.Rows + sep + matrix.Columns);
                for (int r = 0; r < matrix.Rows; ++r)
                {
                    string[] values = new string[matrix.Columns];
                    for (int c = 0; c < matrix.Columns; ++c)
                        values[c] = matrix[r, c].ToString();

                    await writer.WriteLineAsync(String.Join(sep, values));
                }
            }
        }

        public static async Task<Matrix> ReadTextAsync(Stream stream, string sep)
        {
            using (var reader = new StreamReader(stream))
            {
                string rcLine = await reader.ReadLineAsync();
                int[] rc = rcLine.Split(sep).Select(int.Parse).ToArray();
                int rows = rc[0], cols = rc[1];

                double[,] values = new double[rows, cols];
                for (int r = 0; r < rows; ++r)
                {
                    string rowLine = await reader.ReadLineAsync();
                    double[] rowValues = rowLine.Split(sep).Select(double.Parse).ToArray();
                    for (int c = 0; c < cols; ++c)
                        values[r, c] = rowValues[c];
                }

                return new Matrix(values);
            }
        }

        public static void WriteBinary(Matrix matrix, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(matrix.Rows);
                writer.Write(matrix.Columns);
                for (int r = 0; r < matrix.Rows; ++r)
                    for (int c = 0; c < matrix.Columns; ++c)
                        writer.Write(matrix[r, c]);
            }
        }

        public static Matrix ReadBinary(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                int rows = reader.ReadInt32();
                int cols = reader.ReadInt32();

                double[,] values = new double[rows, cols];
                for (int r = 0; r < rows; ++r)
                    for (int c = 0; c < cols; ++c)
                        values[r, c] = reader.ReadDouble();

                return new Matrix(values);
            }
        }

        public static async Task WriteJsonAsync(Matrix matrix, Stream stream)
        {
            double[][] values = new double[matrix.Rows][];
            for (int r = 0; r < matrix.Rows; ++r)
            {
                values[r] = new double[matrix.Columns];
                for (int c = 0; c < matrix.Columns; ++c)
                    values[r][c] = matrix[r, c];
            }
            await JsonSerializer.SerializeAsync(stream, values);
        }

        public static async Task<Matrix> ReadJsonAsync(Stream stream)
        {
            double[][] values = await JsonSerializer.DeserializeAsync<double[][]>(stream);

            int rows = values.Length, cols = values[0].Length;
            double[,] matrixValues = new double[rows, cols];

            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < cols; ++c)
                    matrixValues[r, c] = values[r][c];

            return new Matrix(matrixValues);
        }
        public static void WriteToFile(DirectoryInfo dir, string fileName, Matrix matrix, Action<Matrix, Stream> write)
        {
            string filePath = Path.Combine(dir.FullName, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                write(matrix, stream);
            }
        }
        public static async Task WriteToFileAsync(DirectoryInfo dir, string fileName, Matrix matrix, Func<Matrix, Stream, Task> write)
        {
            string filePath = Path.Combine(dir.FullName, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await write(matrix, stream);
            }
        }

        public static Matrix ReadFromFile(FileInfo fileInfo, Func<Stream, Matrix> read)
        {
            using (var stream = fileInfo.Open(FileMode.Open))
            {
                return read(stream);
            }
        }
        public static async Task<Matrix> ReadFromFileAsync(FileInfo fileInfo, Func<Stream, Task<Matrix>> read)
        {
            using (var stream = fileInfo.Open(FileMode.Open))
            {
                return await read(stream);
            }
        }
    }
}

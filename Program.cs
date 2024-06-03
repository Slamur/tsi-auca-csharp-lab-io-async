using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LaboratoryWork3
{
    internal class Program
    {
        static Random random = new Random();

        static Matrix CreateRandom(int rows, int cols)
        {
            double[,] values = new double[rows, cols];
            for (int r = 0; r < rows; ++r)
                for (int c = 0; c < cols; ++c)
                    values[r, c] = random.NextDouble() * 20 - 10;

            return new Matrix(values);
        }

        static DirectoryInfo CreateDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists) dir.Delete(true);
            dir.Create();
            return dir;
        }

        public static void WriteToDir(
            Matrix[] matrices,
            DirectoryInfo dir,
            string prefix,
            string extension,
            Action<Matrix, Stream> write)
        {
            for (int i = 0; i < matrices.Length; ++i)
            {
                var fileName = prefix + i.ToString() + extension;
                MatrixIO.WriteToFile(dir, fileName, matrices[i], write);
                if (i % 10 == 9) Console.WriteLine($"{fileName} write finished");
            }
        }

        public static async Task WriteToDirAsync(
            Matrix[] matrices,
            DirectoryInfo dir,
            string prefix,
            string extension,
            Func<Matrix, Stream, Task> write)
        {
            for (int i = 0; i < matrices.Length; ++i)
            {
                var fileName = prefix + i.ToString() + extension;
                await MatrixIO.WriteToFileAsync(dir, fileName, matrices[i], write);
                if (i % 10 == 9) Console.WriteLine($"{fileName} write async finished");
            }
        }

        private static FileInfo[] Filter(DirectoryInfo dir, string prefix, string extension)
        {
            return dir.GetFiles()
                .Where(fileInfo => fileInfo.Name.StartsWith(prefix) && fileInfo.Name.EndsWith(extension))
                .ToArray();
        }

        private static (FileInfo, int)[] ParseNames(FileInfo[] files, string prefix)
        {
            return files.Select(file =>
            {
                int index = int.Parse(
                    Path.GetFileNameWithoutExtension(file.Name)
                    .Substring(prefix.Length)
                );

                return (file, index);
            }).ToArray();
        }

        public static Matrix[] ReadFromDir(
            DirectoryInfo dir,
            string prefix,
            string extension,
            Func<Stream, Matrix> read)
        {
            var files = Filter(dir, prefix, extension);
            var parsed = ParseNames(files, prefix);

            Matrix[] matrices = new Matrix[files.Length];
            foreach (var (fileInfo, index) in parsed)
            {
                matrices[index] = MatrixIO.ReadFromFile(fileInfo, read);
            }

            return matrices;
        }
        public static async Task<Matrix[]> ReadFromDirAsync(
            DirectoryInfo dir,
            string prefix,
            string extension,
            Func<Stream, Task<Matrix>> read)
        {
            var files = Filter(dir, prefix, extension);
            var parsed = ParseNames(files, prefix);

            Matrix[] matrices = new Matrix[files.Length];
            foreach (var (fileInfo, index) in parsed)
            {
                matrices[index] = await MatrixIO.ReadFromFileAsync(fileInfo, read);
            }

            return matrices;
        }

        static Matrix MultiplyCheckered(Matrix[] a, Matrix[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException();

            Matrix result = Matrix.Identity(a[0].Rows);
            for (int i = 0; i < a.Length; ++i)
            {
                result *= a[i];
                result *= b[i];
            }

            return result;
        }

        static Matrix MultiplyScalar(Matrix[] a, Matrix[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException();

            Matrix result = Matrix.Zero(a[0].Rows, b[0].Columns);
            for (int i = 0; i < a.Length; ++i)
            {
                result += a[i] * b[i];
            }

            return result;
        }

        static bool Equals(Matrix[] a, Matrix[] b)
        {
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; ++i)
            {
                if (!a[i].Equals(b[i])) return false;
            }

            return true;
        }

        static async Task Process()
        {
            int n = 50;
            int largeSize = 500, smallSize = 100;

            Matrix[] a = new Matrix[n], b = new Matrix[n];

            for (int i = 0; i < n; ++i)
            {
                a[i] = CreateRandom(largeSize, smallSize);
                b[i] = CreateRandom(smallSize, largeSize);
            }

            Task calcTask = Task.Run(async () =>
            {
                // Calculations
                var calcDir = CreateDirectory("calculations");

                string extension = ".tsv", sep = "\t";

                var saveResult = async (Task<Matrix> task, string name) =>
                {
                    var result = await task;
                    Console.WriteLine($"{name} calculation is finished");

                    await MatrixIO.WriteToFileAsync(
                            calcDir,
                            name + extension,
                            result,
                            (Matrix matrix, Stream stream) => MatrixIO.WriteTextAsync(matrix, stream, sep)
                        );
                };

                Task[] tasks =
                {
                    saveResult(
                        Task.Run(() => MultiplyCheckered(a, b)),
                        "abCheckered"
                    ),
                    saveResult(
                        Task.Run(() => MultiplyCheckered(b, a)),
                        "baCheckered"
                    ),
                    saveResult(
                        Task.Run(() => MultiplyScalar(a, b)),
                        "abScalar"
                    ),
                    saveResult(
                        Task.Run(() => MultiplyScalar(b, a)),
                        "baScalar"
                    )
                };

                await Task.WhenAll(tasks);
            });

            string aPrefix = "a", bPrefix = "b";

            Task writeAsyncTask = Task.Run(async () =>
            {
                Console.WriteLine("Write async started");

                var textDir = CreateDirectory("text");
                var jsonDir = CreateDirectory("json");

                string textExtension = ".csv", textSep = ";";
                string jsonExtension = ".json";

                // async write
                Task aCsvTask = WriteToDirAsync(
                    a,
                    textDir,
                    aPrefix,
                    textExtension,
                    (matrix, stream) => MatrixIO.WriteTextAsync(matrix, stream, textSep)
                );

                Task bCsvTask = WriteToDirAsync(
                    b,
                    textDir,
                    bPrefix,
                    textExtension,
                    (matrix, stream) => MatrixIO.WriteTextAsync(matrix, stream, textSep)
                );

                Task aJsonTask = WriteToDirAsync(
                    a,
                    jsonDir,
                    aPrefix,
                    jsonExtension,
                    MatrixIO.WriteJsonAsync
                );

                Task bJsonTask = WriteToDirAsync(
                    b,
                    jsonDir,
                    bPrefix,
                    jsonExtension,
                    MatrixIO.WriteJsonAsync
                );

                await Task.WhenAll(aCsvTask, bCsvTask, aJsonTask, bJsonTask);

                Console.WriteLine("Write async finished");

                // async read
                Task<Matrix[]> csvRead = ReadFromDirAsync(
                    textDir,
                    aPrefix,
                    textExtension,
                    (stream) => MatrixIO.ReadTextAsync(stream, textSep)
                );

                Task<Matrix[]> jsonRead = ReadFromDirAsync(
                    jsonDir,
                    aPrefix,
                    jsonExtension,
                    MatrixIO.ReadJsonAsync
                );

                var readTasks = new List<Task<Matrix[]>> { csvRead, jsonRead };

                Matrix[] csvA = null, jsonA = null;

                while (readTasks.Count > 0)
                {
                    var finished = await Task.WhenAny(readTasks);

                    var result = await finished;
                    readTasks.Remove(finished);

                    if (finished == csvRead)
                    {
                        Console.WriteLine("Csv finished");
                        csvA = result;
                    }
                    else
                    {
                        Console.WriteLine("Json finished");
                        jsonA = result;
                    }
                }

                Task[] eqTasks = {
                    Task.Run(() => {
                        var eq = Equals(a, csvA);
                        Console.WriteLine($"Csv array equals: {eq}");
                    }),
                    Task.Run(() => {
                        var eq = Equals(a, jsonA);
                        Console.WriteLine($"Json array equals: {eq}");
                    })
                };

                await Task.WhenAll(eqTasks);
            });

            {
                Console.WriteLine("Write started");

                var binaryDir = CreateDirectory("binary");

                string binaryExtension = ".bin";

                WriteToDir(
                    a,
                    binaryDir,
                    aPrefix,
                    binaryExtension,
                    MatrixIO.WriteBinary
                );

                WriteToDir(
                    b,
                    binaryDir,
                    bPrefix,
                    binaryExtension,
                    MatrixIO.WriteBinary
                );

                Console.WriteLine("Write finished");

                var binA = ReadFromDir(
                    binaryDir,
                    aPrefix,
                    binaryExtension,
                    MatrixIO.ReadBinary
                );

                var eq = Equals(a, binA);
                Console.WriteLine($"Binary a equals: {eq}");
            }

            await Task.WhenAll(calcTask, writeAsyncTask);
        }

        static async Task Main(string[] args)
        {
            try
            {
                await Process();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

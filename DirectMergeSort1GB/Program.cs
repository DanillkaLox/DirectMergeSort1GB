using System.Diagnostics;

namespace DirectMergeSort1GB;

class ExternalMergeSort
{
    static void Main()
    {
        string inputFilePath = "A.txt";
        string tempSortedPath = "Sorted.txt";

        GenerateFile(inputFilePath, 1024 * 1024 * 1024);
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        SortLargeFile(inputFilePath, tempSortedPath, 40 * 1024 * 1024);

        stopwatch.Stop();
        Console.WriteLine($"Сортування завершено за {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static void GenerateFile(string filePath, int sizeInBytes)
    {
        Random random = new Random();
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int bytesWritten = 0;
            while (bytesWritten < sizeInBytes)
            {
                int number = random.Next(0, 10000);
                string line = number.ToString();
                writer.WriteLine(line);
                bytesWritten += line.Length + Environment.NewLine.Length;
            }
        }
        Console.WriteLine("Файл згенеровано.");
    }

    static void SortLargeFile(string inputFilePath, string sortedFilePath, int chunkSize)
    {
        List<string> tempFiles = new List<string>();
        using (StreamReader reader = new StreamReader(inputFilePath))
        {
            while (!reader.EndOfStream)
            {
                List<int> numbers = new List<int>();
                int bytesRead = 0;

                while (!reader.EndOfStream && bytesRead < chunkSize)
                {
                    string line = reader.ReadLine() ?? throw new InvalidOperationException();
                    if (int.TryParse(line, out int number))
                    {
                        numbers.Add(number);
                        bytesRead += line.Length + Environment.NewLine.Length;
                    }
                }
                
                MergeSort(numbers);
                
                string tempFile = Path.GetTempFileName();
                using (StreamWriter writer = new StreamWriter(tempFile))
                {
                    foreach (var number in numbers)
                    {
                        writer.WriteLine(number);
                    }
                }

                tempFiles.Add(tempFile);
            }
        }
        
        MergeSortedFiles(tempFiles, sortedFilePath);
        
        foreach (var tempFile in tempFiles)
        {
            File.Delete(tempFile);
        }
    }

    static void MergeSortedFiles(List<string> sortedFiles, string outputFilePath)
    {
        List<StreamReader> readers = new List<StreamReader>();
        foreach (var file in sortedFiles)
        {
            readers.Add(new StreamReader(file));
        }

        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            SortedDictionary<int, Queue<int>> minHeap = new SortedDictionary<int, Queue<int>>();
            foreach (var reader in readers)
            {
                if (!reader.EndOfStream)
                {
                    int number = int.Parse(reader.ReadLine() ?? throw new InvalidOperationException());
                    if (!minHeap.ContainsKey(number))
                    {
                        minHeap[number] = new Queue<int>();
                    }
                    minHeap[number].Enqueue(readers.IndexOf(reader));
                }
            }

            while (minHeap.Count > 0)
            {
                var minEntry = minHeap.First();
                int minValue = minEntry.Key;
                int readerIndex = minEntry.Value.Dequeue();
                if (minEntry.Value.Count == 0)
                {
                    minHeap.Remove(minValue);
                }

                writer.WriteLine(minValue);
                
                if (!readers[readerIndex].EndOfStream)
                {
                    int nextValue = int.Parse(readers[readerIndex].ReadLine() ?? throw new InvalidOperationException());
                    if (!minHeap.ContainsKey(nextValue))
                    {
                        minHeap[nextValue] = new Queue<int>();
                    }
                    minHeap[nextValue].Enqueue(readerIndex);
                }
            }
        }

        foreach (var reader in readers)
        {
            reader.Close();
        }
    }
    
    static void MergeSort(List<int> numbers)
    {
        if (numbers.Count <= 1)
            return;

        int mid = numbers.Count / 2;
        List<int> left = numbers.GetRange(0, mid);
        List<int> right = numbers.GetRange(mid, numbers.Count - mid);

        MergeSort(left);
        MergeSort(right);

        Merge(numbers, left, right);
    }

    static void Merge(List<int> numbers, List<int> left, List<int> right)
    {
        int i = 0, j = 0, k = 0;

        while (i < left.Count && j < right.Count)
        {
            if (left[i] <= right[j])
            {
                numbers[k++] = left[i++];
            }
            else
            {
                numbers[k++] = right[j++];
            }
        }

        while (i < left.Count)
        {
            numbers[k++] = left[i++];
        }

        while (j < right.Count)
        {
            numbers[k++] = right[j++];
        }
    }
}
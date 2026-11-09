using System;

namespace LabOS
{
    public class MatrixMultiplier
    {
        private readonly int _n, _m, _k, _threadCount;
        private readonly long[,] _matrixA;
        private readonly long[,] _matrixB;
        private readonly long[,] _matrixC;

        private int _completedRows;
        private bool _isFinished;

        public MatrixMultiplier(int n, int m, int k, int threadCount)
        {
            _n = n;
            _m = m;
            _k = k;
            _threadCount = threadCount;

            _matrixA = new long[n, m];
            _matrixB = new long[m, k];
            _matrixC = new long[n, k];

            _completedRows = 0;
            _isFinished = false;
        }

        // 1. Генерация данных (не входит в замер времени)
        public void GenerateData()
        {
            Random rand = new Random(42); // Фиксированный seed для воспроизводимости

            // Заполняем матрицу A числами от 0 до 9
            for (int i = 0; i < _n; i++)
                for (int j = 0; j < _m; j++)
                    _matrixA[i, j] = rand.Next(0, 10);

            // Заполняем матрицу B числами от 0 до 9
            for (int i = 0; i < _m; i++)
                for (int j = 0; j < _k; j++)
                    _matrixB[i, j] = rand.Next(0, 10);
        }

        // 2. Прогрев JIT-компилятора
        public void Warmup()
        {
            long[,] warmupA = new long[10, 10];
            long[,] warmupB = new long[10, 10];
            long[,] warmupC = new long[10, 10];
            for (int i = 0; i < 10; i++)
                for (int t = 0; t < 10; t++)
                    for (int j = 0; j < 10; j++)
                        warmupC[i, j] += warmupA[i, t] * warmupB[t, j];
        }

        // 3. Основной метод запуска умножения с замером времени
        public void RunMultiplication()
        {
            // Сбрасываем состояние для корректной работы потока прогресса
            _isFinished = false;
            _completedRows = 0;

            // Очищаем матрицу C, чтобы результаты предыдущих замеров не накапливались
            Array.Clear(_matrixC, 0, _matrixC.Length);

            Console.WriteLine();
            Thread[] threads = new Thread[_threadCount];
            int baseRows = _n / _threadCount;
            int remainder = _n % _threadCount;

            // Распределяем строки между потоками
            for (int t = 0; t < _threadCount; t++)
            {
                int startRow = t * baseRows + Math.Min(t, remainder);
                int rowCount = baseRows + (t < remainder ? 1 : 0);
                int endRow = startRow + rowCount;

                threads[t] = new Thread(() => Worker(startRow, endRow));
                threads[t].Start();
            }

            // Отдельный поток для вывода прогресса
            Thread progressThread = new Thread(ProgressReporter);
            progressThread.Start();

            // Главный поток ждёт завершения всех рабочих потоков
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            _isFinished = true;
            progressThread.Join();
            Console.WriteLine("\rПрогресс: 100%              "); // Добавлены пробелы для очистки строки
        }

        // Рабочий поток с оптимизацией под кэш (порядок i -> t -> j)
        private void Worker(int startRow, int endRow)
        {
            for (int i = startRow; i < endRow; i++)
            {
                for (int t = 0; t < _m; t++)
                {
                    long a = _matrixA[i, t];
                    for (int j = 0; j < _k; j++)
                    {
                        _matrixC[i, j] += a * _matrixB[t, j];
                    }
                }
                Interlocked.Increment(ref _completedRows);
            }
        }

        // Поток для вывода прогресса
        private void ProgressReporter()
        {
            int lastPercent = -1;
            while (!_isFinished)
            {
                int currentPercent = (int)((_completedRows * 100.0) / _n);
                if (currentPercent != lastPercent)
                {
                    Console.Write($"\rПрогресс: {currentPercent}%");
                    lastPercent = currentPercent;
                }
                Thread.Sleep(200); // Пауза 200 мс, чтобы не нагружать консоль
            }
        }

        // Вывод только контрольной суммы
        public void PrintChecksum()
        {
            long checksum = 0;
            for (int i = 0; i < _n; i++)
                for (int j = 0; j < _k; j++)
                    checksum += _matrixC[i, j];

            Console.WriteLine($"Контрольная сумма: {checksum}");

            if (_n <= 10 && _k <= 10)
            {
                Console.WriteLine("\nМатрица результата C:");
                for (int i = 0; i < _n; i++)
                {
                    for (int j = 0; j < _k; j++)
                    {
                        Console.Write($"{_matrixC[i, j],4}");
                    }
                    Console.WriteLine();
                }
            }
        }

        // Метод для подстановки своих матриц (для проверки на маленьких данных)
        public void SetMatrices(long[,] matrixA, long[,] matrixB)
        {
            // Копируем данные из переданных матриц во внутренние поля
            Array.Copy(matrixA, _matrixA, matrixA.Length);
            Array.Copy(matrixB, _matrixB, matrixB.Length);
        }

        // Метод для получения контрольной суммы (чтобы тест мог её проверить)
        public long GetChecksum()
        {
            long checksum = 0;
            for (int i = 0; i < _n; i++)
                for (int j = 0; j < _k; j++)
                    checksum += _matrixC[i, j];

            return checksum;
        }

        // Метод для получения матрицы C
        public long[,] GetMatrixC()
        {
            return _matrixC;
        }
    }
}
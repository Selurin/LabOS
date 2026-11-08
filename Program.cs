using System.Diagnostics;

namespace LabOS
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Умножение матриц в многопоточном режиме ===");
            Console.WriteLine("Для выхода введите '0' или 'exit'.\n");

            while (true)
            {
                Console.Write("Введите размеры n, m, k и число потоков T (например, '2000 1500 1800 8'): ");
                string input = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (input == "0" || input == "exit")
                {
                    Console.WriteLine("Программа завершена.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Ошибка: ввод не может быть пустым. Попробуйте снова.\n");
                    continue;
                }

                string[] parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 4)
                {
                    Console.WriteLine("Ошибка: нужно ввести ровно 4 числа через пробел.\n");
                    continue;
                }

                if (int.TryParse(parts[0], out int n) &&
                    int.TryParse(parts[1], out int m) &&
                    int.TryParse(parts[2], out int k) &&
                    int.TryParse(parts[3], out int t))
                {
                    if (n <= 0 || m <= 0 || k <= 0 || t <= 0)
                    {
                        Console.WriteLine("Ошибка: все числа должны быть строго больше нуля.\n");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"\nЗапуск серии из 5 замеров: A[{n}x{m}] * B[{m}x{k}] в {t} потоков...");

                        // Создаем экземпляр класса-умножителя
                        MatrixMultiplier multiplier = new MatrixMultiplier(n, m, k, t);

                        Console.WriteLine("Генерация данных и прогрев JIT...");
                        multiplier.GenerateData();
                        multiplier.Warmup();

                        // Массив для хранения 5 результатов замеров
                        double[] results = new double[5];

                        for (int i = 0; i < 5; i++)
                        {
                            Console.Write($"Выполняется замер #{i + 1}/5... ");

                            Stopwatch sw = Stopwatch.StartNew();

                            multiplier.RunMultiplication();

                            sw.Stop();
                            Console.WriteLine();
                            results[i] = sw.Elapsed.TotalMilliseconds;
                            Console.WriteLine($"{results[i]:F2} мс");
                        }

                        // Обработка результатов: сортируем, min/max и считаем среднее
                        Array.Sort(results);
                        double sum = 0;
                        for (int i = 1; i < 4; i++) // Берем индексы 1, 2, 3 (пропускаем 0 и 4)
                        {
                            sum += results[i];
                        }
                        double averageTime = sum / 3;

                        Console.WriteLine("\n---------------------------------------------------");
                        Console.WriteLine($"Результаты замеров (мс): [{string.Join(", ", results)}]");
                        Console.WriteLine($"Минимальное ({results[0]:F2}). Максимальное ({results[4]:F2})");
                        Console.WriteLine($"Среднее время (по 3 центральным замерам): {averageTime:F2} мс");

                        // Вывод контрольной суммы (берем из последнего успешного выполнения)
                        multiplier.PrintChecksum();
                        Console.WriteLine("---------------------------------------------------\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Критическая ошибка: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка: введены некорректные данные. Используйте только целые положительные числа.\n");
                }
            }
        }
    }
}
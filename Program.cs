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
                        Console.WriteLine($"\nЗапуск: A[{n}x{m}] * B[{m}x{k}] в {t} потоков...");

                        MatrixMultiplier multiplier = new MatrixMultiplier(n, m, k, t);

                        Console.WriteLine("Генерация данных и прогрев...");
                        multiplier.GenerateData();
                        multiplier.Warmup(); // Прогрев JIT

                        Console.WriteLine("Начало умножения...");
                        DateTime startTime = DateTime.UtcNow;

                        multiplier.RunMultiplication();

                        DateTime endTime = DateTime.UtcNow;
                        double elapsedMs = (endTime - startTime).TotalMilliseconds;

                        multiplier.PrintResults(elapsedMs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Критическая ошибка: {ex.Message}");
                    }
                    Console.WriteLine("\n---------------------------------------------------\n");
                }
                else
                {
                    Console.WriteLine("Ошибка: введены некорректные данные. " +
                        "Используйте только целые положительные числа.\n");
                }
            }

        }
    }
}

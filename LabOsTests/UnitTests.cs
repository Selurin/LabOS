using Xunit;
using LabOS;

namespace LabOSTests
{
    
    public class Tests
    {
        // Маленькая матрица. Проверка корректности умножения
        [Fact]
        public void Multiply_2x2Matrices_CorrectResult()
        {
            var multiplier = new MatrixMultiplier(2, 2, 2, 1);

            long[,] testA = { { 1, 2 }, { 3, 4 } };
            long[,] testB = { { 3, 4 }, { 5, 6 } };

            multiplier.SetMatrices(testA, testB);
            multiplier.RunMultiplication();

            Assert.Equal(94, multiplier.GetChecksum());

            // Проверка всей матрицы C
            long[,] expectedC = { { 13, 16 }, { 29, 36 } };
            long[,] actualC = multiplier.GetMatrixC();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    Assert.Equal(expectedC[i, j], actualC[i, j]);
        }

        //Средняя матрица. Проверка воспроизводимости при одинаковых параметрах
        [Fact]
        public void Multiply_100x100_ReproducibleResult()
        {
            var m1 = new MatrixMultiplier(100, 100, 100, 4);
            m1.GenerateData();
            m1.RunMultiplication();
            long checksum1 = m1.GetChecksum();

            // Запускаем второй раз с теми же параметрами
            var m2 = new MatrixMultiplier(100, 100, 100, 4);
            m2.GenerateData();
            m2.RunMultiplication();
            long checksum2 = m2.GetChecksum();

            Assert.Equal(checksum1, checksum2);
        }

        //Средняя матрица. Проверка воспроизводимости при одинаковых параметрах, но разном количестве потоков
        [Fact]
        public void DifferentThreads()
        {
            int n = 200, m = 200, k = 200;

            var multiplier1 = new MatrixMultiplier(n, m, k, 1);
            var multiplier2 = new MatrixMultiplier(n, m, k, 8);

            multiplier1.GenerateData();
            multiplier2.GenerateData();

            multiplier1.RunMultiplication();
            multiplier2.RunMultiplication();

            long checksum1 = multiplier1.GetChecksum();
            long checksum2 = multiplier2.GetChecksum();

            Assert.Equal(checksum1, checksum2);
        }

        //Большая матрица. Проверка на отсутствие крашей
        [Fact]
        public void Multiply_LargeMatrices_CompletesWithoutError()
        {
            var multiplier = new MatrixMultiplier(3000, 2500, 1200, 12);

            multiplier.GenerateData();
            multiplier.RunMultiplication();

            long checksum = multiplier.GetChecksum();
            Assert.True(checksum > 0);
        }
    }
}

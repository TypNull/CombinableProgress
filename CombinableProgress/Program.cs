using System.Numerics;

namespace CombinableProgress
{
    internal class Program
    {
        public static int MaxValue { get; } = 500;
        public static int ProgressLength { get; } = 5;

        private static async Task Main(string[] args)
        {
            await Run<byte>();
        }

        public static async Task Run<T>() where T : INumber<T>
        {
            Progress<T>[] progresses = new Progress<T>[ProgressLength];
            T[] values = new T[progresses.Length + 1];

            for (int i = 0; i < values.Length; i++)
                values[i] = T.Zero;
            for (int i = 0; i < progresses.Length; i++)
            {
                int j = i;
                progresses[i] = new Progress<T>(value => { values[j] = value; });
            }


            CombinableProgress<T> cProgress = new(average => values[values.Length - 1] = average);

            Array.ForEach(progresses, prog => cProgress.Attach(prog));

            Random random = new();
            Task[] tasks = progresses.Select(prog => Task.Run(() => Simulate(prog, T.CreateSaturating(random.NextSingle() * 7), 100 + 100 * random.Next(1, 7)))).ToArray();


            do
            {
                await Task.Delay(50);
                Console.Clear();
                for (int i = 0; i < progresses.Length; i++)
                    Console.WriteLine($"{(char)(byte)(65 + 1 * i)} is reporting: {values[i]}");

                Console.WriteLine($"Average progress = {values.Last()}");
            } while (!Task.WhenAll(tasks).IsCompleted);

            Console.WriteLine("All progressors completed.");
            Console.ReadLine();
        }

        private static void Simulate<T>(IProgress<T> progress, T step, int delay) where T : INumber<T>
        {
            T value = T.Zero;
            while (value + step > value && T.CreateSaturating(MaxValue) > value + step)
            {
                Thread.Sleep(delay);
                value += step;
                progress.Report(value);
            }
            Thread.Sleep(delay);
            progress.Report(T.CreateSaturating(MaxValue));
        }
    }
}
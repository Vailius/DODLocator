using System.Diagnostics;
using System.Numerics;

namespace DODLocator.Tests
{
    public class Program
    {
        private static void TestMocks()
        {
            MocksTests.TestAllocator();
            MocksTests.TestIdGenerator();
            MocksTests.TestMemoryGrow();
        }
        public static void Main(string[] args)
        {
            TEST("Mocks", TestMocks);
            TEST("StructArray:MemoryLeaks", StructAttayTests.TestMemoryLeaks);
        }

        private static void WriteColored(string prew, string colored, string after, ConsoleColor clr)
        {
            Console.WriteLine(prew);
            Console.ForegroundColor = clr;
            Console.Write(colored);
            Console.ResetColor();
            Console.Write(after);
        }

        private static void TEST(string name, Action test)
        {
            Console.WriteLine($"\nTest {name}");
            Stopwatch sw = Stopwatch.StartNew();
            try
            { test(); }
            catch (Exception e)
            {
                sw.Stop();
                WriteColored($"Test {name} is ", "FAILURE ", $"[{TimeSpan.FromTicks(sw.ElapsedTicks)}]", ConsoleColor.Red);
                Console.WriteLine($"\t\t ERROR : {e.Message}");
                throw;
            }
            sw.Stop();
            WriteColored($"Test {name} is ", "OKAY ", $"[{TimeSpan.FromTicks(sw.ElapsedTicks)}]", ConsoleColor.Green);
        }
    }
}
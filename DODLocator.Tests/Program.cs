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
            TEST("StructArray:MemoryLeaks", StructArrayTests.TestMemoryLeaks);
            TEST("StructArray:MemoryTest", StructArrayTests.TestData);
        }

        private static void WriteColored(string prew, string colored, string after, ConsoleColor clr)
        {
            Console.Write(prew);
            Console.ForegroundColor = clr;
            Console.Write(colored);
            Console.ResetColor();
            Console.Write(after);
            Console.Write('\n');
        }

        private static void TEST(string name, Action test)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            { test(); }
            catch (Exception e)
            {
                sw.Stop();
                WriteColored($"\nTest {name} is ", "FAILURE ", $"[{TimeSpan.FromTicks(sw.ElapsedTicks)}]", ConsoleColor.Red);
                Console.WriteLine($"\t\t ERROR : {e.Message}");
                throw;
            }
            sw.Stop();
            WriteColored($"\nTest {name} is ", "OKAY ", $"[{TimeSpan.FromTicks(sw.ElapsedTicks)}]", ConsoleColor.Green);
        }
    }
}
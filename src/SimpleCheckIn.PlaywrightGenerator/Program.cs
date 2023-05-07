namespace SimpleCheckIn.PlaywrightGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Microsoft.Playwright.Program.Main(new string[] { "codegen", "https://share.cjy.me/mjj6" });
        }
    }
}
namespace FunPhysics
{
    public static class Run
    {
        [STAThread]
        static void Main()
        {
            using var level = new Window();
            level.Run();
        }
    }
}

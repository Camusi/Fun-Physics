using System;

namespace FunPhysics
{
    public static class Run
    {
        [STAThread]
        static void Main()
        {
            Simulate.Test();
            // using var game = new Game1();
            // game.Run();
        }
    }
}

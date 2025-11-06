using System;

namespace FunPhysics
{
    public static class Run
    {
        [STAThread]
        static void Main()
        {
            Simulation.Simulate();
            // using var game = new Game1();
            // game.Run();
        }
    }
}

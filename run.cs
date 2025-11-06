using System;

namespace FunPhysics
{
    public static class Run
    {
        [STAThread]
        static void Main()
        {
            using var game = new Simulation();
            game.Run();
        }
    }
}

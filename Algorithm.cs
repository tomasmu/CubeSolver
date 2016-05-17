using System;

public class Algorithm
{
    public Algorithm()
    {
    }

    //depth first with iterative deepening (IDA)
    private static void IDA(Cube cube)
    { 
        for (byte depth = 0; depth <= 2; depth++)    //depth <= 20
        {
            Console.WriteLine($"*** trying depth {depth}");
            TreeSearch(cube, depth);
        }
        return;
    }

    private static void Solve(Cube cube)
    {
        IDA(cube);
    }

    private static void TreeSearch(Cube cube, byte depth)
    {
        if (depth == 0 && cube.IsSolved())
        {
            Console.WriteLine("JAG HAR LÖST DEN!");
            return;
        }
        else if (depth > 0)
        {
            //if (prune(p) <= depth) { //add pruning tables here
            for (byte move = 0; move < 18; move++)   //this is all available moves, doesn't check for R R' L L' (yet)
            {
                Console.WriteLine($"trying move {move}");
                TreeSearch(cube.DoMove(move), (byte)(depth - 1));
            }
            //} //prune
            return;
        }
    }
}

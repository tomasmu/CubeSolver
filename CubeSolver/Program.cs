using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Cube kub = new Cube();
            Stopwatch timer = new Stopwatch();

            string input = "F R U R' U' F'";    //fruruf
            //string input = "R U R' U R U2 R'";  //sune
            //string input = "U R U R' U R U' R' U R U2 R'";  //5-mover on 2x2
            //string input = "F R' F' R U F' U' F R U' R' U"; //identity

            byte[] scramble = Data.ParseMove(input);

            printComputerNotation("ComputerScramble\t", scramble);
            printHumanNotation   ("HumanScramble   \t", scramble);

            //scramble cube
            kub.DoMove(scramble);

            //print cube state
            printCube(kub);

            //get solution
            timer.Start();
            var solution = Algorithm.Solve(kub);
            timer.Stop();

            //apply the solution (not really necessary)
            //kub.DoMove(solution);
            //printCube(kub);

            //print
            printComputerNotation("ComputerSolution\t", solution);
            printHumanNotation   ("HumanSolution   \t", solution);

            //stats
            Console.WriteLine("Nodes visited   \t{0:N0}\t({1:N0} per second)", Algorithm.nodesVisited,   1000.0 * Algorithm.nodesVisited / timer.ElapsedMilliseconds);
            Console.WriteLine("Elapsed seconds \t{0:F3}", timer.ElapsedMilliseconds / 1000.0);
        }

        //print stuff
        private static void printCube(Cube kub)
        {
            Console.WriteLine("cube state:");
            Console.WriteLine("CO: " + string.Join(" ", kub.CO));
            Console.WriteLine("CP: " + string.Join(" ", kub.CP));
            Console.WriteLine("EO: " + string.Join(" ", kub.EO));
            Console.WriteLine("EP: " + string.Join(" ", kub.EP));
        }

        private static void printHumanNotation(string description, byte[] moves)
        {
            Console.WriteLine(description + string.Join(" ", Data.TranslateMove(moves)));
        }

        private static void printComputerNotation(string description, byte[] moves)
        {
            Console.WriteLine(description + string.Join(" ", moves));
        }
    }
}

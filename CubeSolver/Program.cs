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

            Stopwatch timer2 = new Stopwatch();
            timer2.Start();

            if (true)  //if pruning table files doesn't exist
            {
                string[] pruningTables = { "CO", "CP", "EO" };
                foreach (var name in pruningTables)
                    Data.CreatePruningTable(name);
                //todo: save to file
            }
            else
            {
                //todo: read from file
            }

            timer2.Stop();
            Console.WriteLine("pruning tables in seconds: {0:F2}", timer2.ElapsedMilliseconds / 1000.0);
            //Data.CreatePruningTable("EP");

            //string input = "F R U R' U' F'";    //fruruf
            //string input = "B L' F2 R' F2 D2 L2";    //random
            //string input = "R U R' U R U2 R'";  //sune
            //string input = "R U R' U' R' F R F'";
            //string input = "U R U R' U R U' R' U R U2 R'";  //5-mover on 2x2
            //string input = "F R' F' R U F' U' F R U' R' U"; //identity

            string input;   //read from console, maybe file?
            string[] scrambleArray = { "F2 U L2 R2 D' L D L2 R2 U'", "D L' F' U R B' R B U' D" };
            foreach (var item in scrambleArray)
            {
                input = item;

                byte[] scramble = Data.ParseMove(input);

                printComputerNotation("ComputerScramble\t", scramble);
                printHumanNotation("HumanScramble   \t", scramble);

                //scramble cube
                kub.DoMove(scramble);

                //print cube state
                printCube(kub);

                //get solution
                timer.Reset();
                timer.Start();
                var solution = Algorithm.Solve(kub);
                timer.Stop();

                //apply the solution, to solve it for the next loop :-)
                kub.DoMove(solution);

                //print
                printComputerNotation("ComputerSolution\t", solution);
                printHumanNotation("HumanSolution   \t", solution);

                //stats, should probably be its own class (if i even keep all of this)
                Console.WriteLine("Nodes visited   \t{0:N0}\t({1:N0} per second)", Algorithm.nodesVisited, 1000.0 * Algorithm.nodesVisited / timer.ElapsedMilliseconds);
                Console.WriteLine("Moves attempted \t{0:N0}\t({1:N0} per second)", Algorithm.movesAttempted, 1000.0 * Algorithm.movesAttempted / timer.ElapsedMilliseconds);
                Console.WriteLine("Elapsed seconds \t{0:F2}", timer.ElapsedMilliseconds / 1000.0);
                Console.WriteLine("prunes {0}\tfailed {1}", Algorithm.numberOfPruneAll, Algorithm.numberOfPruneAllFailed);
                Console.WriteLine("CO: ok {0}\tfailed {1}\nCP: ok {2}\tfailed {3}\nEO: ok {4}\tfailed {5}",
                    Algorithm.numberOfPruneCO,
                    Algorithm.numberOfPruneCOFailed,
                    Algorithm.numberOfPruneCP,
                    Algorithm.numberOfPruneCPFailed,
                    Algorithm.numberOfPruneEO,
                    Algorithm.numberOfPruneEOFailed
                    );
                    Console.WriteLine("---------------------------");
                Algorithm.nodesVisited = Algorithm.movesAttempted = Algorithm.numberOfPruneAll = Algorithm.numberOfPruneAllFailed = Algorithm.numberOfPruneCO = Algorithm.numberOfPruneCOFailed = Algorithm.numberOfPruneCP = Algorithm.numberOfPruneCPFailed = Algorithm.numberOfPruneEO = Algorithm.numberOfPruneEOFailed = 0;
            }   //end får
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

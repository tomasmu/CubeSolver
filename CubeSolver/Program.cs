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

            //string[] pruningTables = { "CO", "CP", "EO", "COEO" };
            string[] pruningTables = { "CO", "CP", "EO" };  //38.1 -> 36.5 seconds
            foreach (var name in pruningTables)
            {
                if (true)   //doesn't exist
                {
                    Data.CreatePruningTable(name);
                    //Data.SavePruningTableToFile(dict, $"prune{name}.txt");  //implement
                }
                else
                {
                    //Data.ReadPruningTableFromFile(dict, $"prune{name}.txt");    //implement
                }
            }

            timer2.Stop();
            Console.WriteLine("pruning tables in seconds: {0:F2}", timer2.ElapsedMilliseconds / 1000.0);
            //Data.CreatePruningTable("EP");

            //string input = "F R U R' U' F'";    //fruruf
            //string input = "B L' F2 R' F2 D2 L2 F' R U'";    //random
            //string input = "R U R' U R U2 R'";  //sune
            //string input = "R U R' U' R' F R F'";
            //string input = "U R U R' U R U' R' U R U2 R'";  //5-mover on 2x2
            //string input = "F R' F' R U F' U' F R U' R' U"; //identity

            //string input = "F2 U L2 R2 D' L D L2 R2 U'";   //16 seconds -> 9 seconds
            //string input = "F R U' R' U' R U R' F' R U R' U' R' F R F'";
            //string input = "R U R' U' R' F R2 U' R' U' R U R' F'";

            string[] inputArray =
            {                                               //opt    time
                //"L' D' F' U' F2 L2 B'"                      //7 move random scramble, 30 secs without pruning tables
                //"R' F R' B2 R F' R' B2 R2",                 // 9      1.2
                //"R' U2 R' D' R U2 R' D R2",                 // 9      0.5
                //"L' B2 L' F2 L B2 L' F2 L2",                // 9      0.4
                //"R' U L' U2 R U' R' U2 R L",                //10     12.7
                //"F R U2 R' D' R U2 R' D F'",                //10      2.4
                "B2 D2 F L2 R2 B D2 B D2 F' U2",            //11 move random scramble, 31
                //"U F2 D2 B2 U' F2 R2 D R2 F2 D2",           //11 move random scramble, 177
                //"U' R2 D F2 D L2 B2 U' B2 D2 F2",           //11 move random scramble, 470
                //"R' U2 R D2 B2 L2 F2 R F2 U2 R",            //11 move random scramble, 75
                //"L' D' F' U' F2 L2 B' L D' F' L2",          //11 move random scramble, 52
                //"B' D2 U2 B' U2 F' D2 L2 U2 F R2",          //11 move random scramble, 173
                //"D' B2 U2 L2 D L2 F2 D L' U2 R'",           //11 move random scramble, 122
                //"R2 D F2 L2 D2 B2 U L2 B2 F2 U2",           //11 move random scramble, 166
                //"U2 F D2 R2 U2 L2 B U2 F2 D2 R'",           //11 move random scramble, 161
                //"R2 D2 F2 D B2 U R2 D F2 L2 U2"             //11 move random scramble, 147

                //"B2 D2 F L2 R2 B D2 B D2",    //9 move random scramble
                //"U F2 D2 B2 U' F2 R2 D R2",   //9 move random scramble
                //"U' R2 D F2 D L2 B2 U' B2",   //9 move random scramble
                //"R' U2 R D2 B2 L2 F2 R F2",   //9 move random scramble
                //"L' D' F' U' F2 L2 B' L D'",  //9 move random scramble
                //"B' D2 U2 B' U2 F' D2 L2 U2", //9 move random scramble
                //"D' B2 U2 L2 D L2 F2 D L'",   //9 move random scramble
                //"R2 D F2 L2 D2 B2 U L2 B2",   //9 move random scramble
                //"U2 F D2 R2 U2 L2 B U2 F2",   //9 move random scramble
                //"R2 D2 F2 D B2 U R2 D F2"     //9 move random scramble

                //"U2 R2 D' R2 D R2 U2 R2 D' R2 D R2",        //11    226.9   -> 209.2
                //"R U R' U' R' F R2 U' R' U' R U R' F'",     //11     34.5   ->  31.7
                //"R' U' R2 U R U R' U' R U R U' R U' R' U2", //12   6056.7
                //"F R U' R' U' R U R' F' R U R' U' R' F R F'"//13  21544.4
            };

           foreach (var input in inputArray)
           {
                timer.Reset();
                byte[] scramble = Data.ParseMoveFromString(input);

                //printComputerNotation("ComputerScramble\t", scramble);
                printHumanNotation("HumanScramble   \t", scramble);

                kub = new Cube();

                //scramble cube
                foreach (var move in scramble)
                {
                        kub = new Cube(kub, move);
                }

                //print cube state
                //printCube(kub);

                //get solution
                timer.Start();
                var solution = Algorithm.Solve(kub);
                timer.Stop();

                //print
                //printComputerNotation("ComputerSolution\t", solution);
                printHumanNotation("HumanSolution   \t", solution);

                //stats, should probably be its own class (if i even keep all of this)
                //Console.WriteLine("Nodes visited   \t{0:N0}\t({1:N0} per second)", Algorithm.nodesVisited,   1000.0 * Algorithm.nodesVisited   / timer.ElapsedMilliseconds);
                //Console.WriteLine("Moves attempted \t{0:N0}\t({1:N0} per second)", Algorithm.movesAttempted, 1000.0 * Algorithm.movesAttempted / timer.ElapsedMilliseconds);
                Console.WriteLine("Elapsed seconds \t{0:F4}", timer.ElapsedMilliseconds / 1000.0);

                Console.WriteLine("unsolvedCO  " + Algorithm.unsolvedCO);
                Console.WriteLine("unsolvedCP  " + Algorithm.unsolvedCP);
                Console.WriteLine("unsolvedEO  " + Algorithm.unsolvedEO);
                Console.WriteLine("unsolvedEP  " + Algorithm.unsolvedEP);
                Console.WriteLine("unsolvedAll " + Algorithm.unsolvedAll);

                Console.WriteLine("---");
            }
            //temp
            //Console.WriteLine("prunes {0}\tfailed {1}", Algorithm.numberOfPruneAll, Algorithm.numberOfPruneAllFailed);
            //temp
            //Console.WriteLine("CO: ok {0}\tfailed {1}\nCP: ok {2}\tfailed {3}\nEO: ok {4}\tfailed {5}",
            //        Algorithm.numberOfPruneCO,
            //        Algorithm.numberOfPruneCOFailed,
            //        Algorithm.numberOfPruneCP,
            //        Algorithm.numberOfPruneCPFailed,
            //        Algorithm.numberOfPruneEO,
            //        Algorithm.numberOfPruneEOFailed
            //    );
        }

        //print stuff
        public static void printCube(Cube kub)
        {
            Console.WriteLine("cube state:");
            Console.WriteLine("CO: " + string.Join(" ", kub.CO));
            Console.WriteLine("CP: " + string.Join(" ", kub.CP));
            Console.WriteLine("EO: " + string.Join(" ", kub.EO));
            Console.WriteLine("EP: " + string.Join(" ", kub.EP));
        }

        private static void printHumanNotation(string description, byte[] moves)
        {
            Console.WriteLine(description + string.Join(" ", Data.TranslateMoves(moves)) + " (" + moves.Length + ")");
        }

        private static void printComputerNotation(string description, byte[] moves)
        {
            Console.WriteLine(description + string.Join(" ", moves) + " (" + moves.Length + ")");
        }
    }
}

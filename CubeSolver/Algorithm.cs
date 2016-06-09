using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Algorithm
    {
        //move to Solve()/IDA/etc?
        public static List<byte> theSolution = new List<byte>();

        //these will all be (re)moved
        public static UInt64 nodesVisited = 0;
        public static UInt64 movesAttempted = 0;
        public static UInt64 numberOfPruneAll = 0;
        public static UInt64 numberOfPruneAllFailed = 0;
        public static UInt64 numberOfPruneCO = 0;
        public static UInt64 numberOfPruneCOFailed = 0;
        public static UInt64 numberOfPruneCP = 0;
        public static UInt64 numberOfPruneCPFailed = 0;
        public static UInt64 numberOfPruneEO = 0;
        public static UInt64 numberOfPruneEOFailed = 0;
        public static int unsolvedCO = 0;
        public static int unsolvedCP = 0;
        public static int unsolvedEO = 0;
        public static int unsolvedEP = 0;
        public static int unsolvedAll = 0;
        //

        public static byte[] Solve(Cube cube)
        {
            //theSolution.Clear();    //send byte[30] array with function instead?
            //IDA(cube);
            //Program.printHumanNotation("IDA: ", theSolution.ToArray());
            theSolution.Clear();
            Kociemba(cube);
            return theSolution.ToArray();
        }

        //depth first search with iterative deepening (IDA)
        //jaap's pseudo code for IDA and TreeSearch: http://www.jaapsch.net/puzzles/compcube.htm#prune
        private static bool IDA(Cube cube)
        {
            Console.WriteLine("Solving with: IDA()");
            //todo: InitializeIDA();  //add pruning tables etc
            for (byte depth = 0; depth <= 20; depth++)  //>20 moves has been proven impossible
            {
                Console.WriteLine("* trying depth: " + depth);
                if (TreeSearch(cube, depth, 18, 19))    //18, 19 are dummy moves (and are used)
                    return true;
            }
            return false;
        }

        //public static byte[] moveSet = new byte[] { 0, 1, 2, 6, 7, 8 };

        //treesearch, plus checking the two previous moves
        private static bool TreeSearch(Cube cube, byte depth, byte prevMove, byte prevPrevMove)
        {
            nodesVisited++;
            if (depth == 0 && IsSolved(cube))
            {
                Console.WriteLine("EUREKA!");
                return true;
            }
            else if (depth > 0)
            {
                if (PruneTablesLessThanOrEqualToDepthAll(cube, depth))
                {
                    for (byte move = 0; move < 18; move++)   //go through all moves, move < moveSet.Length?
                    {
                        movesAttempted++;
                        if (IsAllowedMove(move, prevMove, prevPrevMove))
                        {
                            Cube newCube = new Cube(cube, move);
                            if (TreeSearch(newCube, (byte)(depth - 1), move, prevMove))
                            {
                                theSolution.Insert(0, move);   //insert from beginning, first move found = last move in solution
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool PruneTablesLessThanOrEqualToDepthAll(Cube cube, byte depth)
        {
            foreach (var prune in Data.pruneDict)
            {
                if (GetPruneDepth(cube, prune.Value) > depth)   //if any prune > depth, successful prune
                {
                    numberOfPruneAll++;
                    return false;
                }
            }
            return true;    //could not prune anything

            //for individual testing
            //bool test = true;
            //if (GetPruneDepth(cube, Data.pruneDict["CO"]) > depth)
            //{
            //    numberOfPruneCO++;
            //    //return false;
            //    test = false;
            //}
            //numberOfPruneCOFailed++;
            //if (GetPruneDepth(cube, Data.pruneDict["CP"]) > depth)
            //{
            //    numberOfPruneCP++;
            //    //return false;
            //    test = false;
            //}
            //numberOfPruneCPFailed++;
            //if (GetPruneDepth(cube, Data.pruneDict["EO"]) > depth)
            //{
            //    numberOfPruneEO++;
            //    //return false;
            //    test = false;
            //}
            //numberOfPruneEOFailed++;
            //
            //numberOfPruneAllFailed++;
            //return test;
        }

        public static bool PruneTablesLessThanOrEqualToDepthByName(Cube cube, byte depth, string[] names)
        {
            foreach (var name in names)
            {
                if (Data.pruneDict.ContainsKey(name))
                {
                    if (GetPruneDepth(cube, Data.pruneDict[name]) > depth)   //if any prune > depth, successful prune
                    {
                        return false;
                    }
                }
            }
            return true;    //could not prune anything
        }

        private static byte GetPruneDepth(Cube cube, Data.PruneTable prune)
        {
            int index = Data.CalculateIndex(cube, prune);
            byte ret;
            if (prune.table.TryGetValue(index, out ret))    //not all pruning tables are complete, error handling needed
            {
                return ret;
            }
            else
            {
                //Console.WriteLine($"index fail: {index} on {prune.name}");
                return 0;   //cannot prune
            }
        }

        //array lookup is faster than the division by 3 and 6 method
        public static byte[] moveLayer = { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 18, 19 }; //18, 19: dummies for dummy moves 18, 19
        public static byte[] moveAxis =  { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 18, 19 };
        public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        {
            //this order should be most efficient
            if (moveLayer[move] == moveLayer[prevMove]) //p = 1/6 = 17%
                return false;
            if (moveAxis[move] == moveAxis[prevMove] && moveAxis[move] == moveAxis[prevPrevMove])   //p = (1/5)^2 = 4%
                return false;
            return true;
        }

        //public static void DoMoves(Cube cube, byte[] moves)
        //{
        //    foreach (var move in moves)
        //        cube.DoMove(move);
        //}

        public static bool IsSolved(Cube cube)
        {
            //bool ret = true;
            if (!IsSolvedOrientation(cube.CO))
            {
                //unsolvedCO++;
                //ret = false;
                return false;
            }
            if (!IsSolvedPermutation(cube.CP))
            {
                //unsolvedCP++;
                //ret = false;
                return false;
            }
            if (!IsSolvedOrientation(cube.EO))
            {
                //unsolvedEO++;
                //ret = false;
                return false;
            }
            if (!IsSolvedPermutation(cube.EP))
            {
                //unsolvedEP++;
                //ret = false;
                return false;
            }
            //unsolvedAll++;
            //return ret;
            return true;
        }

        //Kociemba Phase1 solved state
        public static bool IsSolvedG1(Cube cube)    //if cube is in G1 = { U, D, R2, L2, F2, B2 }
        {
            if (!IsSolvedOrientation(cube.CO))
                return false;
            if (!IsSolvedOrientation(cube.EO))
                return false;
            if (!IsSolvedPermutationESlice(cube.EP))
                return false;
            return true;
        }

        //4 5 6 7 = FL BL BR FR = E slice
        public static bool IsSolvedPermutationESlice(byte[] EP)
        {
            if (EP[4] == 4 && EP[5] == 5 && EP[6] == 6 && EP[7] == 7)
                return true;
            else
                return false;
        }

        //-------------------------------------------{ U  U2 U' D  D2 D' R  R2 R' L  L2 L' F  F2 F' B  B2 B'}
        private static byte[] moveSetG1 = new byte[] { 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0 };   //18, 19 = dummies, i don't count null moves as G1, hence ..0, 0 }; :-)
        private static bool IsMoveSetG1(byte move)
        {
            if (moveSetG1[move] == 1)
                return true;
            else
                return false;
        }

        //can this be better? checking if solved only makes up about 2.5%, though
        //public static bool IsSolvedOP(byte[] stateO, byte[] stateP)
        //{
        //    for (int i = 0; i < stateO.Length; i++)
        //    {
        //        if (stateO[i] != 0) return false;
        //        if (stateP[i] != i) return false;
        //    }
        //    return true;
        //}

        public static bool IsSolvedOrientation(byte[] state)
        {
            //for (int i = state.Length - 1; i >= 0; i--)   //decrementing to 0 is said to be faster, no practical difference though
            //    if (state[i] != 0) return false;
            for (int i = 0; i < state.Length; i++)
                if (state[i] != 0) return false;
            return true;
        }

        public static bool IsSolvedPermutation(byte[] state)
        {
            //for (int i = state.Length - 1; i >= 0; i--)   //decrementing to 0 is said to be faster, no practical difference though
            //    if (state[i] != i) return false;
            for (int i = 0; i < state.Length; i++)
                if (state[i] != i) return false;
            return true;
        }

        private static bool Kociemba(Cube cube)
        {
            Console.WriteLine("Solving with: Kociemba()");
            //InitializeKociemba(); //pruning tables PrunePhase1() - don't use CP prunes, IsSolved(), moveSet = domino, etc
            Console.WriteLine("Starting Phase1");
            //byte depth;
            //for Phase1, is a low depth beneficial? does that make longer Phase2?
            for (byte depth = 0; depth <= 12; depth++)  //30 moves suffice for kociemba, 12 for Phase1, 18 for Phase1
            {
                Console.WriteLine("* trying depth: " + depth);
                if (Phase1Search(cube, depth, 18, 19))    //18, 19 are dummy moves (and are used)
                    break;
            }

            Program.printHumanNotation("Phase 1 done    \t", theSolution.ToArray());
            foreach (var move in theSolution)
                cube = new Cube(cube, move);
            theSolution.Clear();

            Console.WriteLine("Starting Phase2");
            for (byte depth2 = 0; depth2 <= 18; depth2++)
            {
                Console.WriteLine("* trying depth: " + depth2);
                if (Phase2Search(cube, depth2, 18, 19))    //18, 19 are dummy moves (and are used)
                    return true;
            }
            return false;
        }

        private static bool Phase1Search(Cube cube, byte depth, byte prevMove, byte prevPrevMove)
        {
            nodesVisited++;
            if (depth == 0 && IsSolvedG1(cube) && !IsMoveSetG1(prevMove))   //moveset not G1 = quarter turn RLFB
            {
                Console.WriteLine("eureka, G1 + !(RLFB)");    //Phase1 done
                return true;
            }
            else if (depth > 0)
            {
                if (PruneTablesLessThanOrEqualToDepthByName(cube, depth, new string[] { "COEO" })) //COEO looks enough for Phase1
                {
                    for (byte move = 0; move < 18; move++)   //go through all moves, move < moveSet.Length?
                    {
                        movesAttempted++;
                        if (IsAllowedMove(move, prevMove, prevPrevMove))
                        {
                            Cube newCube = new Cube(cube, move);
                            if (Phase1Search(newCube, (byte)(depth - 1), move, prevMove))
                            {
                                theSolution.Insert(0, move);   //insert from beginning, first move found = last move in solution
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        //phase2 needs optimizing, flipUD?
        public static byte[] moveSetG1Test = new byte[] { 0, 1, 2, 3, 4, 5, 7, 10, 13, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static bool Phase2Search(Cube cube, byte depth, byte prevMove, byte prevPrevMove)
        {
            nodesVisited++;
            if (depth == 0 && IsSolved(cube))
                 {
                     Console.WriteLine("EUREKA! KOCIEMBA!");
                     return true;
                 }
            else if (depth > 0)
            {
                //if (PruneTablesLessThanOrEqualToDepthAll(cube, depth)) //todo: use Phase2 pruning tables
                if (PruneTablesLessThanOrEqualToDepthByName(cube, depth, new string[] { "CP2" })) //todo: use Phase2 pruning tables
                {
                    //for (byte move = 0; move < 18; move++)   //go through all 10 moves in G1, moveSetG1[move]
                    for (byte move = 0; move < 10; move++)   //go through all moves, implement move restrictions, move < moveSet.Length?
                    {
                        movesAttempted++;
                        //if (IsG1MoveSet(move) && IsAllowedMove(move, prevMove, prevPrevMove)) //slow
                        if (IsAllowedMove(moveSetG1Test[move], moveSetG1Test[prevMove], moveSetG1Test[prevPrevMove]))   //fast
                        {
                            //Cube newCube = new Cube(cube, move);
                            Cube newCube = new Cube(cube, moveSetG1Test[move]);
                            if (Phase2Search(newCube, (byte)(depth - 1), move, prevMove))
                            {
                                //theSolution.Insert(0, move);   //insert from beginning, first move found = last move in solution
                                theSolution.Insert(0, moveSetG1Test[move]);   //insert from beginning, first move found = last move in solution
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}

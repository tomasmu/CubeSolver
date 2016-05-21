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
            theSolution.Clear();
            IDA(cube);
            return theSolution.ToArray();
        }

        //depth first search with iterative deepening (IDA)
        //jaap's pseudo code for IDA and TreeSearch: http://www.jaapsch.net/puzzles/compcube.htm#prune
        private static bool IDA(Cube cube)
        {
            for (byte depth = 0; depth <= 20; depth++)  //>20 moves has been proven impossible
            {
                Console.WriteLine("* trying depth: " + depth);
                if (TreeSearch(cube, depth, 18, 19))    //18, 19 are dummy moves (and are used)
                    return true;
            }
            return false;
        }

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
                if (PruneTablesLessThanOrEqualToDepth(cube, depth))
                {
                    for (byte move = 0; move < 18; move++)   //go through all moves
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

        public static bool PruneTablesLessThanOrEqualToDepth(Cube cube, byte depth)
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

        private static byte GetPruneDepth(Cube cube, Data.PruneTable prune)
        {
            int index = Data.CalculateIndex(cube, prune);
            return prune.table[index];
            //error handling shouldn't be needed, the pruning tables are complete
            //byte ret;
            //prune.table.TryGetValue(index, out ret);
            //return ret;
        }

        //array lookup is a little bit faster than the division by 3 and 6 method
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

        //can this be better? checking if solved is only makes up about 2.5%, though
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
            for (int i = 0; i < state.Length; i++)
                if (state[i] != 0) return false;
            return true;
        }

        public static bool IsSolvedPermutation(byte[] state)
        {
            for (int i = 0; i < state.Length; i++)
                if (state[i] != i) return false;
            return true;
        }
    }
}

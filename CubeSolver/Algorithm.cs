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
            if (depth == 0 && cube.IsSolved())
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
                            //Cube newCube = copyCube(cube).DoMove(move);   //constructor copies instead
                            Cube newCube = new Cube(cube).DoMove(move);
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

        private static bool PruneTablesLessThanOrEqualToDepth(Cube cube, byte depth)
        {
            foreach (var prune in Data.pruneDict)
            {
                if (GetPruneDepth(cube, prune.Value) > depth)   //if any prune > depth, successful prune
                {
                    numberOfPruneAll++;
                    return false;
                }
            }
            //todo: test these individually
            //if (GetPruneDepth(cube, Data.pruneDict["CO"]) > depth)
            //{
            //    numberOfPruneCO++;
            //    return false;
            //}
            //numberOfPruneCOFailed++;
            //if (GetPruneDepth(cube, Data.pruneDict["CP"]) > depth)
            //{
            //    numberOfPruneCP++;
            //    return false;
            //}
            //numberOfPruneCPFailed++;
            //if (GetPruneDepth(cube, Data.pruneDict["EO"]) > depth)
            //{
            //    numberOfPruneEO++;
            //    return false;
            //}
            //numberOfPruneEOFailed++;
            
            numberOfPruneAllFailed++;
            return true;    //could not prune anything
        }

        private static byte GetPruneDepth(Cube cube, Data.PruneTable prune)
        {
            int index = Data.CalculateIndex(cube, prune);
            return prune.table[index];
        }

        //is array lookup faster? no noticable difference experienced
        //private static byte[] moveLayer = { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 18, 19 }; //18, 19: dummies for dummy moves 18, 19
        //private static byte[] moveAxis =  { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 18, 19 };
        //public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        //{
        //    //i think this order is most efficient
        //    if (moveLayer[move] == moveLayer[prevMove])
        //        return false;
        //    if (moveAxis[move] == moveAxis[prevMove] && moveAxis[move] == moveAxis[prevPrevMove])
        //        return false;
        //    return true;
        //}

        //division by 3 and 6, not noticably slower solutions than with array lookup
        public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        {
            if (IsSameLayer(move, prevMove)) return false;
            if (IsSameAxis(move, prevMove) && IsSameAxis(move, prevPrevMove)) return false;
            return true;
        }

        private static bool IsSameLayer(byte move, byte tryMove)
        {
            if ((byte)(move / 3) == (byte)(tryMove / 3)) return true;    //i made it so turns in move structure are in groups of three, floor(move/3) = layer
            return false;
        }

        private static bool IsSameAxis(byte move, byte tryMove)
        {
            if ((byte)(move / 6) == (byte)(tryMove / 6)) return true;   //layers in move structure are in pairs (so moves are in groups of 6), floor(move/6) = axis
            return false;
        }

        public static void ApplyCOMask(byte[] state, byte[] mask)
        {
            //add CO with CO-mask and modulo 3 (rotate)
            for (byte i = 0; i < mask.Length; i++)
                state[i] = (byte)((state[i] + mask[i]) % 3);
        }

        public static void ApplyEOMask(byte[] state, byte[] mask)
        {
            //XOR EO with EO-mask (flip)
            for (byte i = 0; i < mask.Length; i++)
                state[i] ^= mask[i];
            //state[i] = (byte)((state[i] + mask[i]) % 2);  //ApplyOrientationMask(state, mask, N) would work for CO and EO
        }

        public static void ApplyPermutationMask(byte[] state, byte[] mask)
        {
            //permute according to the mask, i goes to mask[i]
            byte[] temp = new byte[state.Length];
            for (byte i = 0; i < mask.Length; i++)   //copy
                temp[i] = state[mask[i]];
            for (byte i = 0; i < mask.Length; i++)   //paste
                state[i] = temp[i];
        }

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

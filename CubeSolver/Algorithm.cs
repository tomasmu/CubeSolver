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
                            Cube newCube = new Cube(cube);
                            DoMove(newCube, move);
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

        //is array lookup faster? by a little bit in the grand total, it seems
        private static byte[] moveLayer = { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 18, 19 }; //18, 19: dummies for dummy moves 18, 19
        private static byte[] moveAxis =  { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 18, 19 };
        public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        {
            //i think this order is most efficient
            if (moveLayer[move] == moveLayer[prevMove]) //p = 1/6 ~~ 17%
                return false;
            if (moveAxis[move] == moveAxis[prevMove] && moveAxis[move] == moveAxis[prevPrevMove])   //p = (1/5)^2 = 4%
                return false;
            return true;
        }

        //division by 3 and 6, not noticably slower solutions than with array lookup
        //public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        //{
        //    if (IsSameLayer(move, prevMove))
        //        return false;
        //    if (IsSameAxis(move, prevMove) && IsSameAxis(move, prevPrevMove))
        //        return false;
        //    return true;
        //}
        //
        //private static bool IsSameLayer(byte move, byte tryMove)
        //{
        //    if (move / 3 == tryMove / 3) return true;    //i made it so turns in move structure are in groups of three, floor(move/3) = layer
        //    return false;
        //}
        //
        //private static bool IsSameAxis(byte move, byte tryMove)
        //{
        //    if (move / 6 == tryMove / 6) return true;   //layers in move structure are in pairs (so moves are in groups of 6), floor(move/6) = axis
        //    return false;
        //}

        //old and working
        public static void ApplyCOMask(byte[] state, byte[] mask)
        {
            //add CO with CO-mask and modulo 3 (rotate)
            for (byte i = 0; i < mask.Length; i++)
                state[i] = (byte)((state[i] + mask[i]) % 3);
        }
        //old and working
        public static void ApplyEOMask(byte[] state, byte[] mask)
        {
            //XOR EO with EO-mask (flip)
            for (byte i = 0; i < mask.Length; i++)
                state[i] ^= mask[i];
            //state[i] = (byte)((state[i] + mask[i]) % 2);  //ApplyOrientationMask(state, mask, N) would work for CO and EO
        }

        ////new test
        //public static void ApplyCOMask2(byte[] state, byte[] pmask, byte[] omask)
        //{
        //    //piece will go from i to pmask[i], so rotate piece i in slot pmask[i]
        //    byte[] temp = new byte[pmask.Length];
        //    for (byte i = 0; i < pmask.Length; i++) //copy
        //        temp[i] = (byte)((state[pmask[i]] + omask[i]) % 3);     //3 could be a parameter
        //    //state = temp;     //doesn't work, state is merely a copy of the reference, use return temp;
        //    for (byte i = 0; i < pmask.Length; i++) //paste
        //        state[i] = temp[i];
        //}
        //
        ////new test
        //public static void ApplyEOMask2(byte[] state, byte[] pmask, byte[] omask)
        //{
        //    //piece will go from i to pmask[i], so rotate piece i in slot pmask[i]
        //    byte[] temp = new byte[pmask.Length];
        //    for (byte i = 0; i < pmask.Length; i++) //copy
        //        temp[i] = (byte)((state[pmask[i]] + omask[i]) % 2);     //3 could be a parameter
        //    //state = temp;     //doesn't work, state is merely a copy of the reference, use return temp;
        //    for (byte i = 0; i < pmask.Length; i++) //paste
        //        state[i] = temp[i];
        //}

        public static void ApplyPermutationMask(byte[] state, byte[] mask)
        {
            //permute according to the mask, i goes to mask[i]
            byte[] temp = new byte[mask.Length];    //byte[state.Length] not needed with copy/paste below
            for (byte i = 0; i < mask.Length; i++)  //copy
                temp[i] = state[mask[i]];
            //state = temp;     //doesn't work, state is merely a copy of the reference, use return temp;
            for (byte i = 0; i < mask.Length; i++)  //paste
                state[i] = temp[i];
        }

        ////new test
        //public static void DoMove(Cube cube, byte move)
        //{
        //    //moveMask[]: [0] CO, [1] CP, [2] EO, [3] EP
        //    ApplyCOMask2(cube.CO, Data.moveMask[move][1], Data.moveMask[move][0]);   //orient using permutation mask + orientation mask
        //    ApplyPermutationMask(cube.CP, Data.moveMask[move][1]);     //permute only
        //    ApplyEOMask2(cube.EO, Data.moveMask[move][3], Data.moveMask[move][2]);
        //    ApplyPermutationMask(cube.EP, Data.moveMask[move][3]);
        //}

        //old and working well
        public static void DoMove(Cube cube, byte move)
        {
            //moveMask[]: [0] CO, [1] CP, [2] EO, [3] EP
            ApplyCOMask(cube.CO, Data.moveMask[move][0]);       //orient only, no permutation
            ApplyEOMask(cube.EO, Data.moveMask[move][2]);
            ApplyPermutationMask(cube.CP, Data.moveMask[move][1]);  //permute only
            ApplyPermutationMask(cube.EP, Data.moveMask[move][3]);
            ApplyPermutationMask(cube.CO, Data.moveMask[move][1]);  //don't forget to permute after orienting
            ApplyPermutationMask(cube.EO, Data.moveMask[move][3]);
        }

        ////idea for possible optimization, doesn't make very pretty code though
        ////(temp variables must be introduced before it's working)
        ////CO and EO needs to be fixed etc
        //public static void GenerateMoveSwitchCode()
        //{
        //    Dictionary<int, string> maskName = new Dictionary<int, string>()
        //    {
        //        { 0, "CO" }, { 1, "CP" }, { 2, "EO" }, { 3, "EP" }
        //    };
        //    var maskArray = Data.moveMask;
        //    //string[] tabz = new string[3];
        //    //for (int i = 0; i < tabz.Length; i++)
        //    //    tabz[i] = new string(' ', 4 * i);
        //    Console.WriteLine($"{indent(0)}switch (move){indent(0)}");
        //    Console.WriteLine($"{indent(0)}{{");
        //    for (byte move = 0; move < maskArray.Length; move++)   //U U' U2 D ...
        //    {
        //        Console.WriteLine($"{indent(1)}case {move}:{indent(1)}//{Data.TranslateMove(move)}");
        //        for (int type = 0; type < maskArray[move].Length; type++)   //CO CP ...
        //        {
        //            for (int index = 0; index < maskArray[move][type].Length; index++)     //new indices
        //            {
        //                byte newIndex = maskArray[move][type][index];
        //                if (index != newIndex)
        //                    Console.WriteLine($"{indent(2)}cube.{maskName[type]}[{index}] = cube.{maskName[type]}[{maskArray[move][type][index]}]");
        //            }
        //        }
        //        Console.WriteLine($"{indent(2)}break;");
        //    }
        //    Console.WriteLine($"{indent(1)}default:");
        //    Console.WriteLine($"{indent(2)}break;");
        //    Console.WriteLine($"{indent(0)}}}");
        //}
        //
        //private static string indent(int tabs)
        //{
        //    return new string(' ', 4 * tabs);
        //}

        public static void DoMoves(Cube cube, byte[] moves)
        {
            foreach (var move in moves)
                DoMove(cube, move);
        }

        public static bool IsSolved(Cube cube)
        {
            if (!IsSolvedOrientation(cube.CO)) return false;
            if (!IsSolvedPermutation(cube.CP)) return false;
            if (!IsSolvedOrientation(cube.EO)) return false;
            if (!IsSolvedPermutation(cube.EP)) return false;
            return true;
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

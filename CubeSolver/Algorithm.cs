using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Algorithm
    {
        public static List<byte> theSolution = new List<byte>();
        public static UInt64 nodesVisited = 0;

        public static byte[] Solve(Cube cube)
        {
            IDA(cube);
            return theSolution.ToArray();
        }

        //depth first search with iterative deepening (IDA)
        //jaap's pseudo code for IDA and TreeSearch: http://www.jaapsch.net/puzzles/compcube.htm#prune
        private static bool IDA(Cube cube)
        {
            Console.Write("* trying depth: ");
            for (byte depth = 0; depth <= 20; depth++)  //>20 moves impossible
            {
                Console.Write(depth + " ");
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
                Console.WriteLine("\nEUREKA!");
                return true;
            }
            else if (depth > 0)
            {
                //if (prune(p) <= depth) {  //add pruning tables here
                for (byte move = 0; move < 18; move++)   //go through all moves
                {
                    if (IsAllowedMove(move, prevMove, prevPrevMove))
                    {
                        Cube newCube = copyCube(cube).DoMove(move);
                        if (TreeSearch(newCube, (byte)(depth - 1), move, prevMove))
                        {
                            theSolution.Insert(0, move);   //insert from beginning, first move found = last move in solution
                            return true;
                        }
                    }
                }
                //} //prune
            }
            return false;
        }

        //array lookup is slightly faster
        private static byte[] moveLayer = { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 18, 19 }; //18, 19: dummies for dummy moves 18, 19
        private static byte[] moveAxis =  { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 18, 19 };
        public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        {
            if (moveAxis[move] == moveAxis[prevMove] && moveAxis[move] == moveAxis[prevPrevMove]) return false;
            if (moveLayer[move] == moveLayer[prevMove]) return false;
            return true;
        }

        //division by 3 and 6 is slightly slower
        //public static bool IsAllowedMove(byte move, byte prevMove, byte prevPrevMove)
        //{
        //    if (IsSameAxis(move, prevMove) && IsSameAxis(move, prevPrevMove)) return false;
        //    if (IsSameLayer(move, prevMove)) return false;
        //    return true;
        //}
        //private static bool IsSameLayer(byte move, byte tryMove)
        //{
        //    if ((byte)(move / 3) == (byte)(tryMove / 3)) return true;    //i made it so turns in move structure is in groups of three, floor(move/3) = layer
        //    return false;
        //}
        //private static bool IsSameAxis(byte move, byte tryMove)
        //{
        //    if ((byte)(move / 6) == (byte)(tryMove / 6)) return true;   //layers in move structure are in pairs (groups of 6), floor(move/6) = axis
        //    return false;
        //}

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

        private static Cube copyCube(Cube cube)
        {
            Cube newCube = new Cube();
            //slightly faster
            for (int i = 0; i < cube.CO.Length; i++)
                newCube.CO[i] = cube.CO[i];
            for (int i = 0; i < cube.CP.Length; i++)
                newCube.CP[i] = cube.CP[i];
            for (int i = 0; i < cube.EO.Length; i++)
                newCube.EO[i] = cube.EO[i];
            for (int i = 0; i < cube.EP.Length; i++)
                newCube.EP[i] = cube.EP[i];
            //slower
            //Array.Copy(cube.CO, newCube.CO, cube.CO.Length);
            //Array.Copy(cube.CP, newCube.CP, cube.CP.Length);
            //Array.Copy(cube.EO, newCube.EO, cube.EO.Length);
            //Array.Copy(cube.EP, newCube.EP, cube.EP.Length);
            return newCube;
        }
    }
}

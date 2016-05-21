using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CubeSolver
{
    class Data
    {
        public Data()
        {
        }

        public class PruneTable
        {
            public Dictionary<int, byte> table = new Dictionary<int, byte>();
            public int indexBase, maxCount;
            public string name = string.Empty;

            public PruneTable(string tableName)
            {
                this.name = tableName;
                if (this.name == "CO")
                {
                    this.maxCount = 2187;   //3^8/3
                    this.indexBase = 3;     //index <= 3^8-1 = 2186
                }
                else if (this.name == "CP")
                {
                    this.maxCount = 40320;  //8!
                    this.indexBase = 8;     //index <= 8^8-1 = 16777215
                }
                else if (this.name == "EO")
                {
                    this.maxCount = 2048;   //2^12/2
                    this.indexBase = 2;     //index <= 2^12-1 = 2047
                }
                //else if (this.name == "EP")
                //{
                //    this.maxCount = 479001600;    //12!
                //    this.indexBase = 12;  //index <= 12^12-1 = 8916100448255 -> 43 bits :(
                //}
                //else if (this.name == "COEO")
                //{
                //    this.maxCount = 2187 * 2048;    //4478976
                //    this.indexBase = 3;   //index <= 3^(8+12)-1 = 3486784401 -> 32 bits \o/
                //}
            }
            int oldValue = -1;
            public void AddIndex (int index, byte depth)
            {
                if (!table.ContainsKey(index))  //ensures first found depth is optimal
                {
                    table.Add(index, depth);
                    //if (depth > oldValue && name == "COEO")
                    //{
                    //    Console.WriteLine($"added: {index}\t{depth}\tcount: {table.Count()}");
                    //    oldValue = depth;
                    //}
                }
            }
        }

        private static Dictionary<byte, string> moveDefByteString = new Dictionary<byte, string>()
        {
            {  0, "U" }, {  1, "U2" }, {  2, "U'" },
            {  3, "D" }, {  4, "D2" }, {  5, "D'" },
            {  6, "R" }, {  7, "R2" }, {  8, "R'" },
            {  9, "L" }, { 10, "L2" }, { 11, "L'" },
            { 12, "F" }, { 13, "F2" }, { 14, "F'" },
            { 15, "B" }, { 16, "B2" }, { 17, "B'" }
        };

        //for (byte i = 0; i < 18; i++) moveStructureStringByte.Add(moveStructureByteString[i], i); //
        private static Dictionary<string, byte> moveDefStringByte = new Dictionary<string, byte>()
        {
            { "U",  0 }, { "U2",  1 }, { "U'",  2 },
            { "D",  3 }, { "D2",  4 }, { "D'",  5 },
            { "R",  6 }, { "R2",  7 }, { "R'",  8 },
            { "L",  9 }, { "L2", 10 }, { "L'", 11 },
            { "F", 12 }, { "F2", 13 }, { "F'", 14 },
            { "B", 15 }, { "B2", 16 }, { "B'", 17 },
        };

        public static Dictionary<string, PruneTable> pruneDict = new Dictionary<string, PruneTable>();

        public static string TranslateMove(byte move)
        {
            return moveDefByteString[move];
        }

        public static byte TranslateMove(string move)
        {
            return moveDefStringByte[move];
        }

        public static string[] TranslateMoves(byte[] moves)
        {
            string[] temp = new string[moves.Length];
            for (byte i = 0; i < moves.Length; i++)
                temp[i] = TranslateMove(moves[i]);
            return temp;
        }

        public static byte[] ParseMoveFromString(string moves)
        {
            string[] moveArray = moves.Split(' ');
            byte[] parsedMoves = new byte[moveArray.Length];
            for (int i = 0; i < moveArray.Length; i++)
                parsedMoves[i] = Data.TranslateMove(moveArray[i]);
            return parsedMoves;
        }

        public static void CreatePruningTable(string pruneName)
        {
            PruneTable prune = new PruneTable(pruneName);
            pruneDict.Add(pruneName, prune);
            Console.WriteLine("Creating pruning table: " + prune.name);
            for (byte depth = 0; prune.table.Count() < prune.maxCount && depth <= 20; depth++)
            {
                Cube cube = new Cube();
                if (DoAllTheMoves(cube, depth, 18, 19, prune, depth))
                    break;
            }
        }

        private static bool DoAllTheMoves(Cube cube, byte depth, byte prevMove, byte prevPrevMove, PruneTable prune, byte startDepth)
        {
            if (depth == 0)     //all moves done on all depths
            {
                if (prune.table.Count() < prune.maxCount)
                {
                    int index = CalculateIndex(cube, prune);
                    prune.AddIndex(index, startDepth);      //adds if not already added
                    return false;
                }
                else
                    return true;    //done! all indices found
            }
            else
            {
                for (byte move = 0; move < 18; move++)   //go through all moves
                {
                    if (Algorithm.IsAllowedMove(move, prevMove, prevPrevMove))
                    {
                        Cube newCube = new Cube(cube, move);
                        if (DoAllTheMoves(newCube, (byte)(depth - 1), move, prevMove, prune, startDepth))
                            return true;
                    }
                }
            }
            return false;
        }

        public static int CalculateIndex(Cube cube, PruneTable prune)
        {
            int index = 0;
            byte[] type;
            switch (prune.name)
            {
                case "CO":
                    type = cube.CO;
                    break;
                case "CP":
                    type = cube.CP;
                    break;
                case "EO":
                    type = cube.EO;
                    break;
                //case "COEO":
                //    //System.Array.Copy
                //    //System.Buffer.BlockCopy
                //    type = new byte[20];
                //    System.Array.Copy(cube.CO, 0, type, 0, 8);
                //    System.Array.Copy(cube.EO, 0, type, 8, 12);
                //    break;
                //case "EP":
                //    type = cube.EP;
                //    break;
                default:
                    return -1;
            }
            for (int i = 0; i < type.Length - 1; i++)   //last piece is determined by the others, no need to index those
            {
                //index += (type[i] * product);   //type[i] * n^i
                //product *= prune.indexBase;
                index *= prune.indexBase;   //better way of calculating a number in base N, only 1 add and 1 mul per step
                index += type[i];
            }
            return index;
        }

        public static void SavePruningTable(Dictionary<int, byte> dict, string filename)
        {
            //to text file first, so it's humanly readable, then to binary file
            throw new NotImplementedException("Hello world!");
        }

        public static void ReadPruningTable(Dictionary<int, byte> dict, string filename)
        {
            throw new NotImplementedException("Hello world!");
        }

        //fun to figure out, boring to write :D
        //todo: more boring stuff, switch (move) and define all swaps (may be faster)
        //CO todo: 1 <-> 2 when doing CO differently
        //public static class MoveMask
        //{
            public static byte[][] COMask = new byte[][] {
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 0 U
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 1 U2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 2 U'
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 3 D
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 4 D2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 5 D'
                new byte[] { 2, 0, 0, 1, 1, 0, 0, 2 },                  // 6 R
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  // 7 R2
                new byte[] { 2, 0, 0, 1, 1, 0, 0, 2 },                  // 8 R'
                new byte[] { 0, 1, 2, 0, 0, 2, 1, 0 },                  // 9 L
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  //10 L2
                new byte[] { 0, 1, 2, 0, 0, 2, 1, 0 },                  //11 L'
                new byte[] { 1, 2, 0, 0, 2, 1, 0, 0 },                  //12 F
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  //13 F2
                new byte[] { 1, 2, 0, 0, 2, 1, 0, 0 },                  //14 F'
                new byte[] { 0, 0, 1, 2, 0, 0, 2, 1 },                  //15 B
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },                  //16 B2
                new byte[] { 0, 0, 1, 2, 0, 0, 2, 1 }                   //17 B'
            };
            public static byte[][] CPMask = new byte[][] {
                new byte[] { 3, 0, 1, 2, 4, 5, 6, 7 },                  // 0 U
                new byte[] { 2, 3, 0, 1, 4, 5, 6, 7 },                  // 1 U2
                new byte[] { 1, 2, 3, 0, 4, 5, 6, 7 },                  // 2 U'
                new byte[] { 0, 1, 2, 3, 5, 6, 7, 4 },                  // 3 D
                new byte[] { 0, 1, 2, 3, 6, 7, 4, 5 },                  // 4 D2
                new byte[] { 0, 1, 2, 3, 7, 4, 5, 6 },                  // 5 D'
                new byte[] { 4, 1, 2, 0, 7, 5, 6, 3 },                  // 6 R
                new byte[] { 7, 1, 2, 4, 3, 5, 6, 0 },                  // 7 R2
                new byte[] { 3, 1, 2, 7, 0, 5, 6, 4 },                  // 8 R'
                new byte[] { 0, 2, 6, 3, 4, 1, 5, 7 },                  // 9 L
                new byte[] { 0, 6, 5, 3, 4, 2, 1, 7 },                  //10 L2
                new byte[] { 0, 5, 1, 3, 4, 6, 2, 7 },                  //11 L'
                new byte[] { 1, 5, 2, 3, 0, 4, 6, 7 },                  //12 F
                new byte[] { 5, 4, 2, 3, 1, 0, 6, 7 },                  //13 F2
                new byte[] { 4, 0, 2, 3, 5, 1, 6, 7 },                  //14 F'
                new byte[] { 0, 1, 3, 7, 4, 5, 2, 6 },                  //15 B
                new byte[] { 0, 1, 7, 6, 4, 5, 3, 2 },                  //16 B2
                new byte[] { 0, 1, 6, 2, 4, 5, 7, 3 }                   //17 B'
            };
            public static byte[][] EOMask = new byte[][] {
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 0 U
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 1 U2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 2 U'
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 3 D
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 4 D2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 5 D'
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 6 R
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 7 R2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 8 R'
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      // 9 L
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      //10 L2
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      //11 L'
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0 },      //12 F
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      //13 F2
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0 },      //14 F'
                new byte[] { 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0 },      //15 B
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },      //16 B2
                new byte[] { 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0 }       //17 B'
            };
            public static byte[][] EPMask = new byte[][]
            {
                new byte[] { 3, 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11 },    // 0 U
                new byte[] { 2, 3, 0, 1, 4, 5, 6, 7, 8, 9, 10, 11 },    // 1 U2
                new byte[] { 1, 2, 3, 0, 4, 5, 6, 7, 8, 9, 10, 11 },    // 2 U'
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 8 },    // 3 D
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 10, 11, 8, 9 },    // 4 D2
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 11, 8, 9, 10 },    // 5 D'
                new byte[] { 0, 1, 2, 7, 4, 5, 3, 11, 8, 9, 10, 6 },    // 6 R
                new byte[] { 0, 1, 2, 11, 4, 5, 7, 6, 8, 9, 10, 3 },    // 7 R2
                new byte[] { 0, 1, 2, 6, 4, 5, 11, 3, 8, 9, 10, 7 },    // 8 R'
                new byte[] { 0, 5, 2, 3, 1, 9, 6, 7, 8, 4, 10, 11 },    // 9 L
                new byte[] { 0, 9, 2, 3, 5, 4, 6, 7, 8, 1, 10, 11 },    //10 L2
                new byte[] { 0, 4, 2, 3, 9, 1, 6, 7, 8, 5, 10, 11 },    //11 L'
                new byte[] { 4, 1, 2, 3, 8, 5, 6, 0, 7, 9, 10, 11 },    //12 F
                new byte[] { 8, 1, 2, 3, 7, 5, 6, 4, 0, 9, 10, 11 },    //13 F2
                new byte[] { 7, 1, 2, 3, 0, 5, 6, 8, 4, 9, 10, 11 },    //14 F'
                new byte[] { 0, 1, 6, 3, 4, 2, 10, 7, 8, 9, 5, 11 },    //15 B
                new byte[] { 0, 1, 10, 3, 4, 6, 5, 7, 8, 9, 2, 11 },    //16 B2
                new byte[] { 0, 1, 5, 3, 4, 10, 2, 7, 8, 9, 6, 11 }     //17 B'
            };
        //}
    }
}

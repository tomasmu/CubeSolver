using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                else if (this.name == "CP2")
                {
                    this.maxCount = 40320;  //same as above, but limited to G1
                    this.indexBase = 8;
                }
                else if (this.name == "CP2E")
                {
                    this.maxCount = 40320;  //same as above, but with E solved
                    this.indexBase = 8;
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
                else if (this.name == "COEO")
                {
                    this.maxCount = 2187 * 2048;    //4478976
                    this.indexBase = 3;   //index <= 3^(8+12)-1 = 3486784401 -> 32 bits \o/
                }
                //else if (this.name == "COCP")
                //{
                //    this.maxCount = 2187 * 40320;   //88179840
                //}
                //else if (this.name == "EPUD") //todo
                //{
                //    this.maxCount = 40320;  //8!
                //    this.indexBase = 8;
                //}
                //else if (this.name == "FlipUDSlice")  //hmm
                //{
                //    this.maxCount = 2048 * 495; //1013760 = = 2^11 * C(12, 4), dvs EO + lösa E-slice EP
                //}
            }
            int oldValue = -1;
            public void AddIndex (int index, byte depth)
            {
                if (!table.ContainsKey(index))  //ensures first found depth is optimal
                {
                    //if (name == "CP2")
                    //{
                    //    Console.WriteLine($"{name} adding: {index}     \t{depth}\tcount: {table.Count()}");
                    //    using (StreamWriter sw = new StreamWriter($"prune{name}.txt", true))
                    //    {
                    //        sw.WriteLine($"{index}\t{depth}");
                    //    }
                    //}
                    table.Add(index, depth);
                }
                if (depth > oldValue)
                {
                    oldValue = depth;
                    Console.WriteLine($"new depth: {depth}");
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

        public static void CreatePruningTables(string[] names)
        {
            foreach (var name in names)
            {
                PruneTable prune = new PruneTable(name);
                pruneDict.Add(name, prune);
                if (!File.Exists($"prune{name}.txt"))   //doesn't exist
                {
                    Console.WriteLine($"Creating pruning table: {name}");
                    if (name == "CP2")
                        CreatePruningTableG1(prune);
                    else
                        CreatePruningTable(prune);
                    SavePruningTable(prune);
                    Console.WriteLine($"Saved {prune.table.Count()} pruning table entries to prune{prune.name}.txt");
                }
                else
                {
                    ReadPruningTable(prune);
                    Console.WriteLine($"Read {prune.table.Count()} pruning table entries from prune{prune.name}.txt");
                }
            }
        }

        private static void CreatePruningTable(PruneTable prune)
        {
            for (byte depth = 0; depth <= 20; depth++)
            {
                Cube cube = new Cube();
                if (DoAllTheMoves(cube, depth, 18, 19, prune, depth))
                    break;  //all indices found
            }
        }

        public static void CreatePruningTableG1(PruneTable prune)    //phase2
        {
            for (byte depth = 0; depth <= 20; depth++)
            {
                Cube cube = new Cube();
                if (DoAllTheMovesG1(cube, depth, 18, 19, prune, depth))
                    break;  //all indices found
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
        
        private static bool DoAllTheMovesG1(Cube cube, byte depth, byte prevMove, byte prevPrevMove, PruneTable prune, byte startDepth)
        {
            if (depth == 0)     //all moves done on all depths
            {
                    //if (Algorithm.IsSolvedPermutationESlice(cube.EP))   //E-slice must be solved, takes too long
                int index = CalculateIndex(cube, prune);
                prune.AddIndex(index, startDepth);      //adds if not already added
                if (prune.table.Count() < prune.maxCount)
                    return false;
                else
                    return true;    //done! all indices found
            }
            else
            {
                for (byte move = 0; move < 10; move++)   //go through all G1 moves
                {
                    //if (Algorithm.IsAllowedMove(move, prevMove, prevPrevMove))
                    if (Algorithm.IsAllowedMove(Algorithm.moveSetG1Test[move], Algorithm.moveSetG1Test[prevMove], Algorithm.moveSetG1Test[prevPrevMove]))
                    {
                        //Cube newCube = new Cube(cube, move);
                        Cube newCube = new Cube(cube, Algorithm.moveSetG1Test[move]);
                        if (DoAllTheMovesG1(newCube, (byte)(depth - 1), move, prevMove, prune, startDepth))
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
                case "CP2":
                case "CP2E":
                    type = cube.CP;
                    break;
                case "EO":
                    type = cube.EO;
                    break;
                case "COEO":
                    //System.Array.Copy
                    //System.Buffer.BlockCopy
                    //small number of assignments usually fastest
                    type = new byte[20];
                    type[ 0] = cube.CO[0];
                    type[ 1] = cube.CO[1];
                    type[ 2] = cube.CO[2];
                    type[ 3] = cube.CO[3];
                    type[ 4] = cube.CO[4];
                    type[ 5] = cube.CO[5];
                    type[ 6] = cube.CO[6];
                    type[ 7] = cube.CO[7];
                    type[ 8] = cube.EO[ 0];
                    type[ 9] = cube.EO[ 1];
                    type[10] = cube.EO[ 2];
                    type[11] = cube.EO[ 3];
                    type[12] = cube.EO[ 4];
                    type[13] = cube.EO[ 5];
                    type[14] = cube.EO[ 6];
                    type[15] = cube.EO[ 7];
                    type[16] = cube.EO[ 8];
                    type[17] = cube.EO[ 9];
                    type[18] = cube.EO[10];
                    type[19] = cube.EO[11];
                    break;
                //case "EPUD":
                //    if (Algorithm.IsSolvedPermutationESlice(cube.EP))
                //    {
                //        type = new byte[8];
                //        type[0] = cube.EP[ 0];
                //        type[1] = cube.EP[ 1];
                //        type[2] = cube.EP[ 2];
                //        type[3] = cube.EP[ 3];
                //        //skipping E (4, 5, 6, 7)
                //        type[4] = cube.EP[ 8];
                //        type[5] = cube.EP[ 9];
                //        type[6] = cube.EP[10];
                //        type[7] = cube.EP[11];
                //    }
                //    else
                //        return -1;
                //    break;
                //case "EP":
                //    type = cube.EP;
                //    break;
                default:
                    return -1;
            }
            for (int i = 0; i < type.Length - 1; i++)   //last piece is determined by the others, no need to index it
            {
                //index += (type[i] * product);   //type[i] * n^i
                //product *= prune.indexBase;
                index *= prune.indexBase;   //better way of calculating a number in base N, only 1 add and 1 mul per loop
                index += type[i];
            }
            return index;
        }

        public static void SavePruningTable(PruneTable prune)
        {
            //to text file first, so it's humanly readable, then try binary file
            using (StreamWriter sw = new StreamWriter($"prune{prune.name}.txt"))
            {
                foreach (var item in prune.table)
                {
                    sw.WriteLine($"{item.Key}\t{item.Value}");
                }
            }
        }

        public static void ReadPruningTable(PruneTable prune)
        {
            string[] temp;
            using (StreamReader sr = new StreamReader($"prune{prune.name}.txt"))
            {
                while (!sr.EndOfStream)
                {
                    temp = sr.ReadLine().Split('\t');
                    int index = Int32.Parse(temp[0]);
                    byte depth = Byte.Parse(temp[1]);
                    prune.AddIndex(index, depth);
                }
            }
            if (prune.table.Count() < prune.maxCount)
            {
                Console.WriteLine($"prune{prune.name}.txt is incomplete");
                //Console.WriteLine($"* continuing with: {prune.name}");
            }
        }

        //fun to figure out, boring to write :D
        //todo: more boring stuff, switch (move) and define all swaps (may be faster)
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

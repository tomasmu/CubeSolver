using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Data
    {

        public Data()
        {
        }

        private static Dictionary<byte, string> moveStructureByteString = new Dictionary<byte, string>()
        {
            {  0, "U" }, {  1, "U'" }, {  2, "U2" },
            {  3, "D" }, {  4, "D'" }, {  5, "D2" },
            {  6, "R" }, {  7, "R'" }, {  8, "R2" },
            {  9, "L" }, { 10, "L'" }, { 11, "L2" },
            { 12, "F" }, { 13, "F'" }, { 14, "F2" },
            { 15, "B" }, { 16, "B'" }, { 17, "B2" },
        };

        //for (byte i = 0; i < 18; i++) moveStructureStringByte.Add(moveStructureByteString[i], i); //?
        private static Dictionary<string, byte> moveStructureStringByte = new Dictionary<string, byte>()
        {
            { "U",  0 }, { "U'",  1 }, { "U2",  2 },
            { "D",  3 }, { "D'",  4 }, { "D2",  5 },
            { "R",  6 }, { "R'",  7 }, { "R2",  8 },
            { "L",  9 }, { "L'", 10 }, { "L2", 11 },
            { "F", 12 }, { "F'", 13 }, { "F2", 14 },
            { "B", 15 }, { "B'", 16 }, { "B2", 17 },
        };

        private static Dictionary<UInt16, byte> pruneCO = new Dictionary<UInt16, byte>();

        public static string TranslateMove(byte move)
        {
            return moveStructureByteString[move];
        }

        public static byte TranslateMove(string move)
        {
            return moveStructureStringByte[move];
        }

        public static string[] TranslateMove(byte[] moves)
        {
            string[] temp = new string[moves.Length];
            for (byte i = 0; i < moves.Length; i++)
                temp[i] = TranslateMove(moves[i]);
            return temp;
        }

        public static byte[] ParseMove(string moves)
        {
            string[] moveArray = moves.Split(' ');
            byte[] parsedMoves = new byte[moveArray.Length];
            for (int i = 0; i < moveArray.Length; i++)
                parsedMoves[i] = Data.TranslateMove(moveArray[i]);
            return parsedMoves;
        }

        //this part was fun to figure out, but boring to write :D
        public static byte[][][] moveMask = new byte[][][] {
            new byte [][] {     //[0] U
                new byte[] { },            //[0] CO
                new byte[] { 3, 0, 1, 2 }, //[1] CP
                new byte[] { },            //[2] EO
                new byte[] { 3, 0, 1, 2 }  //[3] EP
            },
            new byte [][] {     //[1] U'
                new byte[] { },
                new byte[] { 1, 2, 3, 0 },
                new byte[] { },
                new byte[] { 1, 2, 3, 0 }
            },
            new byte [][] {     //[2] U2
                new byte[] { },
                new byte[] { 2, 3, 0, 1 },
                new byte[] { },
                new byte[] { 2, 3, 0, 1 }
            },
            new byte [][] {     //[3] D
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 5, 6, 7, 4 },
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 8 }
            },
            new byte [][] {     //[4] D'
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 7, 4, 5, 6 },
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 11, 8, 9, 10 }
            },
            new byte [][] {     //[5] D2
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 6, 7, 4, 5 },
                new byte[] { },
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 10, 11, 8, 9 }
            },
            new byte [][] {     //[6] R
                new byte[] { 1, 0, 0, 2, 2, 0, 0, 1 },
                new byte[] { 4, 1, 2, 0, 7, 5, 6, 3 },
                new byte[] { },
                new byte[] { 0, 1, 2, 7, 4, 5, 3, 11, 8, 9, 10, 6 }
            },
            new byte [][] {     //[7] R'
                new byte[] { 1, 0, 0, 2, 2, 0, 0, 1 },
                new byte[] { 3, 1, 2, 7, 0, 5, 6, 4 },
                new byte[] { },
                new byte[] { 0, 1, 2, 6, 4, 5, 11, 3, 8, 9, 10, 7 }
            },
            new byte [][] {     //[8] R2
                new byte[] { },
                new byte[] { 7, 1, 2, 4, 3, 5, 6, 0 },
                new byte[] { },
                new byte[] { 0, 1, 2, 11, 4, 5, 7, 6, 8, 9, 10, 3 }
            },
            new byte [][] {     //[9] L
                new byte[] { 0, 2, 1, 0, 0, 1, 2 },
                new byte[] { 0, 2, 6, 3, 4, 1, 5 },
                new byte[] { },
                new byte[] { 0, 5, 2, 3, 1, 9, 6, 7, 8, 4 }
            },
            new byte [][] {     //[10] L'
                new byte[] { 0, 2, 1, 0, 0, 1, 2 },
                new byte[] { 0, 5, 1, 3, 4, 6, 2 },
                new byte[] { },
                new byte[] { 0, 4, 2, 3, 9, 1, 6, 7, 8, 5 }
            },
            new byte [][] {     //[11] L2
                new byte[] { },
                new byte[] { 0, 6, 5, 3, 4, 2, 1 },
                new byte[] { },
                new byte[] { 0, 9, 2, 3, 5, 4, 6, 7, 8, 1 }
            },
            new byte [][] {     //[12] F
                new byte[] { 2, 1, 0, 0, 1, 2 },
                new byte[] { 1, 5, 2, 3, 0, 4 },
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 1, 1 },
                new byte[] { 4, 1, 2, 3, 8, 5, 6, 0, 7 }
            },
            new byte [][] {     //[13] F'
                new byte[] { 2, 1, 0, 0, 1, 2 },
                new byte[] { 4, 0, 2, 3, 5, 1 },
                new byte[] { 1, 0, 0, 0, 1, 0, 0, 1, 1 },
                new byte[] { 7, 1, 2, 3, 0, 5, 6, 8, 4 }
            },
            new byte [][] {     //[14] F2
                new byte[] { },
                new byte[] { 5, 4, 2, 3, 1, 0 },
                new byte[] { },
                new byte[] { 8, 1, 2, 3, 7, 5, 6, 4, 0 }
            },
            new byte [][] {     //[15] B
                new byte[] { 0, 0, 2, 1, 0, 0, 1, 2 },
                new byte[] { 0, 1, 3, 7, 4, 5, 2, 6 },
                new byte[] { 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1 },
                new byte[] { 0, 1, 6, 3, 4, 2, 10, 7, 8, 9, 5 }
            },
            new byte [][] {     //[16] B'
                new byte[] { 0, 0, 2, 1, 0, 0, 1, 2 },
                new byte[] { 0, 1, 6, 2, 4, 5, 7, 3 },
                new byte[] { 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 1 },
                new byte[] { 0, 1, 5, 3, 4, 10, 2, 7, 8, 9, 6 }
            },
            new byte [][] {     //[17] B2
                new byte[] { },
                new byte[] { 0, 1, 7, 6, 4, 5, 3, 2 },
                new byte[] { },
                new byte[] { 0, 1, 10, 3, 4, 6, 5, 7, 8, 9, 2 }
            }
        };
    }

}

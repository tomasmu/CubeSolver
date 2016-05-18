using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Cube
    {
        public byte[] CO = new byte[8];
        public byte[] CP = new byte[8];
        public byte[] EO = new byte[12];
        public byte[] EP = new byte[12];

        public Cube()
        {
            //cube created in solved state
            CO = new byte[] { 0, 0, 0, 0,  0, 0, 0, 0 };                //UFR UFL UBL UBR  DFR DFL DBL DBR
            CP = new byte[] { 0, 1, 2, 3,  4, 5, 6, 7 };
            EO = new byte[] { 0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0 };   //UF UL UB UR  FL BL BR FR  DF DL DB DR
            EP = new byte[] { 0, 1, 2, 3,  4, 5, 6, 7,  8, 9, 10, 11 };
        }

        public Cube(Cube cubeToCopy)
        {
            //fastest
            for (int i = 0; i < cubeToCopy.CO.Length; i++)
                CO[i] = cubeToCopy.CO[i];
            for (int i = 0; i < cubeToCopy.CP.Length; i++)
                CP[i] = cubeToCopy.CP[i];
            for (int i = 0; i < cubeToCopy.EO.Length; i++)
                EO[i] = cubeToCopy.EO[i];
            for (int i = 0; i < cubeToCopy.EP.Length; i++)
                EP[i] = cubeToCopy.EP[i];
            //slower
            //Buffer.BlockCopy(cubeToCopy.CO, 0, CO, 0, cubeToCopy.CO.Length);
            //Buffer.BlockCopy(cubeToCopy.CP, 0, CP, 0, cubeToCopy.CP.Length);
            //Buffer.BlockCopy(cubeToCopy.EO, 0, EO, 0, cubeToCopy.EO.Length);
            //Buffer.BlockCopy(cubeToCopy.EP, 0, EP, 0, cubeToCopy.EP.Length);
            //slowest
            //Array.Copy(cubeToCopy.CO, CO, cubeToCopy.CO.Length);
            //Array.Copy(cubeToCopy.CP, CP, cubeToCopy.CP.Length);
            //Array.Copy(cubeToCopy.EO, EO, cubeToCopy.EO.Length);
            //Array.Copy(cubeToCopy.EP, EP, cubeToCopy.EP.Length);
        }

        public bool IsSolved()
        {
            if (!Algorithm.IsSolvedOrientation(this.CO)) return false;
            if (!Algorithm.IsSolvedPermutation(this.CP)) return false;
            if (!Algorithm.IsSolvedOrientation(this.EO)) return false;
            if (!Algorithm.IsSolvedPermutation(this.EP)) return false;
            return true;
        }

        public Cube DoMove(byte move)
        {
            //moveMask[]: [0] CO, [1] CP, [2] EO, [3] EP
            Algorithm.ApplyCOMask(this.CO, Data.moveMask[move][0]);       //orient only, no permutation
            Algorithm.ApplyEOMask(this.EO, Data.moveMask[move][2]);
            Algorithm.ApplyPermutationMask(this.CP, Data.moveMask[move][1]);  //permute only
            Algorithm.ApplyPermutationMask(this.EP, Data.moveMask[move][3]);
            Algorithm.ApplyPermutationMask(this.CO, Data.moveMask[move][1]);  //don't forget to permute after orienting
            Algorithm.ApplyPermutationMask(this.EO, Data.moveMask[move][3]);
            return this;
        }

        public Cube DoMove(byte[] moves)
        {
            foreach (var move in moves)
                DoMove(move);
            return this;
        }
    }
}

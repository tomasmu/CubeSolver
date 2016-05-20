using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeSolver
{
    class Cube
    {
        public byte[] CO;
        public byte[] CP;
        public byte[] EO;
        public byte[] EP;

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
            CO = new byte[8];
            CP = new byte[8];
            EO = new byte[12];
            EP = new byte[12];
            //faster than Buffer.BlockCopy, Array.Copy, and X = cubeToCopy.X
            for (int i = 0; i < cubeToCopy.CO.Length; i++)
                CO[i] = cubeToCopy.CO[i];
            for (int i = 0; i < cubeToCopy.CP.Length; i++)
                CP[i] = cubeToCopy.CP[i];
            for (int i = 0; i < cubeToCopy.EO.Length; i++)
                EO[i] = cubeToCopy.EO[i];
            for (int i = 0; i < cubeToCopy.EP.Length; i++)
                EP[i] = cubeToCopy.EP[i];
        }
        
        public Cube(Cube cubeToCopy, byte move)
        {
            //copy a cube with a move added
        }

        //do this here instead
        //public Cube DoMove(byte move)
        //{
        //    Cube copy = new Cube(this);
        //    for (byte i = 0; i < 8; i++)
        //        
        //    return copy;
        //}
    }
}

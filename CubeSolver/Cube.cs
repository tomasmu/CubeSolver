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
            //create a solved cube
            this.CO = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };               //UFR UFL UBL UBR  DFR DFL DBL DBR
            this.CP = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            this.EO = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };   //UF UL UB UR  FL BL BR FR  DF DL DB DR
            this.EP = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        }

        public Cube(Cube cubeToCopy, byte move)
        {
            //copy a cube, and make a move
            this.CO = new byte[ 8]; //UFR UFL UBL UBR  DFR DFL DBL DBR
            this.CP = new byte[ 8];
            this.EO = new byte[12]; //UF UL UB UR  FL BL BR FR  DF DL DB DR
            this.EP = new byte[12];

            //slow :(
            //for (int i = 0; i < 8; i++)
            //{
            //    this.CO[i] = (byte)((cubeToCopy.CO[Data.CPMask[move][i]] + Data.COMask[move][i]) % 3);  //new.CO[i] = (old.CO[newCPIndex] + comask[index]) % 3, rotate using permute mask
            //    this.CP[i] = cubeToCopy.CP[Data.CPMask[move][i]];     //new.CP[i] = old.CP[newIndex], permute
            //}
            //for (int i = 0; i < 12; i++)
            //{
            //    this.EO[i] = (byte)(cubeToCopy.EO[Data.EPMask[move][i]] ^ Data.EOMask[move][i]);     // new.EP[i] = old.EP[newIndex], flip
            //    this.EP[i] = cubeToCopy.EP[Data.EPMask[move][i]];     //new.EP[i] = old.EP[newIndex], permute
            //}

            //faster than forloops (about 17% in the grand total), looks ugly though :)
            this.CO[0] = (byte)((cubeToCopy.CO[Data.CPMask[move][0]] + Data.COMask[move][0]) % 3);  //new.CO[i] = (old.CO[newCPIndex] + comask[index]) % 3, rotate using permute mask
            this.CO[1] = (byte)((cubeToCopy.CO[Data.CPMask[move][1]] + Data.COMask[move][1]) % 3);
            this.CO[2] = (byte)((cubeToCopy.CO[Data.CPMask[move][2]] + Data.COMask[move][2]) % 3);
            this.CO[3] = (byte)((cubeToCopy.CO[Data.CPMask[move][3]] + Data.COMask[move][3]) % 3);
            this.CO[4] = (byte)((cubeToCopy.CO[Data.CPMask[move][4]] + Data.COMask[move][4]) % 3);
            this.CO[5] = (byte)((cubeToCopy.CO[Data.CPMask[move][5]] + Data.COMask[move][5]) % 3);
            this.CO[6] = (byte)((cubeToCopy.CO[Data.CPMask[move][6]] + Data.COMask[move][6]) % 3);
            this.CO[7] = (byte)((cubeToCopy.CO[Data.CPMask[move][7]] + Data.COMask[move][7]) % 3);
            
            this.CP[0] = cubeToCopy.CP[Data.CPMask[move][0]];     //new.CP[i] = old.CP[newIndex], permute
            this.CP[1] = cubeToCopy.CP[Data.CPMask[move][1]];
            this.CP[2] = cubeToCopy.CP[Data.CPMask[move][2]];
            this.CP[3] = cubeToCopy.CP[Data.CPMask[move][3]];
            this.CP[4] = cubeToCopy.CP[Data.CPMask[move][4]];
            this.CP[5] = cubeToCopy.CP[Data.CPMask[move][5]];
            this.CP[6] = cubeToCopy.CP[Data.CPMask[move][6]];
            this.CP[7] = cubeToCopy.CP[Data.CPMask[move][7]];
            
            this.EO[ 0] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 0]] ^ Data.EOMask[move][ 0]);     // new.EP[i] = (old.EO[newEPIndex] ^ eomask[index]), flip
            this.EO[ 1] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 1]] ^ Data.EOMask[move][ 1]);
            this.EO[ 2] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 2]] ^ Data.EOMask[move][ 2]);
            this.EO[ 3] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 3]] ^ Data.EOMask[move][ 3]);
            this.EO[ 4] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 4]] ^ Data.EOMask[move][ 4]);
            this.EO[ 5] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 5]] ^ Data.EOMask[move][ 5]);
            this.EO[ 6] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 6]] ^ Data.EOMask[move][ 6]);
            this.EO[ 7] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 7]] ^ Data.EOMask[move][ 7]);
            this.EO[ 8] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 8]] ^ Data.EOMask[move][ 8]);
            this.EO[ 9] = (byte)(cubeToCopy.EO[Data.EPMask[move][ 9]] ^ Data.EOMask[move][ 9]);
            this.EO[10] = (byte)(cubeToCopy.EO[Data.EPMask[move][10]] ^ Data.EOMask[move][10]);
            this.EO[11] = (byte)(cubeToCopy.EO[Data.EPMask[move][11]] ^ Data.EOMask[move][11]);
            
            this.EP[ 0] = cubeToCopy.EP[Data.EPMask[move][ 0]];     //new.EP[i] = old.EP[newIndex], permute
            this.EP[ 1] = cubeToCopy.EP[Data.EPMask[move][ 1]];
            this.EP[ 2] = cubeToCopy.EP[Data.EPMask[move][ 2]];
            this.EP[ 3] = cubeToCopy.EP[Data.EPMask[move][ 3]];
            this.EP[ 4] = cubeToCopy.EP[Data.EPMask[move][ 4]];
            this.EP[ 5] = cubeToCopy.EP[Data.EPMask[move][ 5]];
            this.EP[ 6] = cubeToCopy.EP[Data.EPMask[move][ 6]];
            this.EP[ 7] = cubeToCopy.EP[Data.EPMask[move][ 7]];
            this.EP[ 8] = cubeToCopy.EP[Data.EPMask[move][ 8]];
            this.EP[ 9] = cubeToCopy.EP[Data.EPMask[move][ 9]];
            this.EP[10] = cubeToCopy.EP[Data.EPMask[move][10]];
            this.EP[11] = cubeToCopy.EP[Data.EPMask[move][11]];
        }

        //public Cube(Cube cubeToCopy)    //copy a cube constructor, only used by DoMove
        //{
        //    CO = new byte[8];
        //    CP = new byte[8];
        //    EO = new byte[12];
        //    EP = new byte[12];
        //    //faster than Buffer.BlockCopy, Array.Copy, and X = cubeToCopy.X
        //    //thought: how about writing everything.. without loops? :-)
        //    //for (int i = 0; i < 8; i++)
        //    //{
        //    //    this.CO[i] = cubeToCopy.CO[i];
        //    //    this.CP[i] = cubeToCopy.CP[i];
        //    //}
        //    //for (int i = 0; i < 12; i++)
        //    //{
        //    //    this.EO[i] = cubeToCopy.EO[i];
        //    //    this.EP[i] = cubeToCopy.EP[i];
        //    //}
        //    this.CO[ 0] = cubeToCopy.CO[ 0];
        //    this.CO[ 1] = cubeToCopy.CO[ 1];
        //    this.CO[ 2] = cubeToCopy.CO[ 2];
        //    this.CO[ 3] = cubeToCopy.CO[ 3];
        //    this.CO[ 4] = cubeToCopy.CO[ 4];
        //    this.CO[ 5] = cubeToCopy.CO[ 5];
        //    this.CO[ 6] = cubeToCopy.CO[ 6];
        //    this.CO[ 7] = cubeToCopy.CO[ 7];
        //
        //    this.CP[ 0] = cubeToCopy.CP[ 0];
        //    this.CP[ 1] = cubeToCopy.CP[ 1];
        //    this.CP[ 2] = cubeToCopy.CP[ 2];
        //    this.CP[ 3] = cubeToCopy.CP[ 3];
        //    this.CP[ 4] = cubeToCopy.CP[ 4];
        //    this.CP[ 5] = cubeToCopy.CP[ 5];
        //    this.CP[ 6] = cubeToCopy.CP[ 6];
        //    this.CP[ 7] = cubeToCopy.CP[ 7];
        //
        //    this.EO[ 0] = cubeToCopy.EO[ 0];
        //    this.EO[ 1] = cubeToCopy.EO[ 1];
        //    this.EO[ 2] = cubeToCopy.EO[ 2];
        //    this.EO[ 3] = cubeToCopy.EO[ 3];
        //    this.EO[ 4] = cubeToCopy.EO[ 4];
        //    this.EO[ 5] = cubeToCopy.EO[ 5];
        //    this.EO[ 6] = cubeToCopy.EO[ 6];
        //    this.EO[ 7] = cubeToCopy.EO[ 7];
        //    this.EO[ 8] = cubeToCopy.EO[ 8];
        //    this.EO[ 9] = cubeToCopy.EO[ 9];
        //    this.EO[10] = cubeToCopy.EO[10];
        //    this.EO[11] = cubeToCopy.EO[11];
        //
        //    this.EP[ 0] = cubeToCopy.EP[ 0];
        //    this.EP[ 1] = cubeToCopy.EP[ 1];
        //    this.EP[ 2] = cubeToCopy.EP[ 2];
        //    this.EP[ 3] = cubeToCopy.EP[ 3];
        //    this.EP[ 4] = cubeToCopy.EP[ 4];
        //    this.EP[ 5] = cubeToCopy.EP[ 5];
        //    this.EP[ 6] = cubeToCopy.EP[ 6];
        //    this.EP[ 7] = cubeToCopy.EP[ 7];
        //    this.EP[ 8] = cubeToCopy.EP[ 8];
        //    this.EP[ 9] = cubeToCopy.EP[ 9];
        //    this.EP[10] = cubeToCopy.EP[10];
        //    this.EP[11] = cubeToCopy.EP[11];
        //}

        //public Cube DoMove(byte move)   //do a move only, only used by DoMoves
        //{
        //    Cube copy = new Cube(this);
        //    for (byte i = 0; i < 8; i++)
        //    {
        //        this.CO[i] = (byte)((copy.CO[Data.CPMask[move][i]] + Data.COMask[move][i]) % 3);  //new.CO[i] = (old.CO[newCPIndex] + comask[index]) % 3
        //        this.CP[i] = copy.CP[Data.CPMask[move][i]];     //new.CP[i] = old.CP[newIndex];
        //    }
        //    for (byte i = 0; i < 12; i++)
        //    {
        //        this.EO[i] = (byte)((copy.EO[Data.EPMask[move][i]] + Data.EOMask[move][i]) % 2);
        //        this.EP[i] = copy.EP[Data.EPMask[move][i]];     //new.EP[i] = old.EP[newIndex];
        //    }
        //    return this;
        //}

        //public Cube DoMoves(byte[] moves)   //do several moves, only used when scrambling
        //{
        //    foreach (var move in moves)
        //        this.DoMove(move);
        //    return this;
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    public class LFSR
    {
        public long Seed; //Initial seed entered by user
        public int TapPosIndx;
        public long TapPosValue;
        public long[] Key; //The generated Key used to be XORed with the pixel component.
        public long LeftMostBit; //The left most of the Initial value of the register after shifting it to the left.
        public long XORedValue; //the result from XORing the value of the tap Position with The left most of the register.
        int One;

        public LFSR()   //Constructor to initialize the variables
        {
            Seed = 00000000000;
            TapPosIndx = 0;
            TapPosValue = 0;
            Key = new long[8];
            LeftMostBit = 0;
            XORedValue = 0;
            One = 1;
        }

        public static byte GeneratingBits(ref long seed, int tap, int SeedLength)
        {
            LFSR Register = new LFSR();
            Register.Seed = seed;  //Initial value of the LFSR entered by user
                                                                                     
            for (int i = 0; i < 8; i++)
            {
                Register.One = Register.One << SeedLength;  //Shiftting the One Variable until it reaches the end of the Initial Seed
                Register.LeftMostBit = (Register.One & Register.Seed);  //ANDing the One with the leftMostBit of the initial seed to know the value of it.
                Register.LeftMostBit = Register.LeftMostBit >> SeedLength;   //Getting the LeftMostBit's vale
                Register.One = 1;   //Re-Initializing the One Variable.

                Register.One = Register.One << tap;   //Shiftting the One Variable until it reaches to the tap Position
                Register.TapPosValue = (Register.One & Register.Seed);    //ANDing the One with the Tap Position's Value to know it.
                Register.TapPosValue = Register.TapPosValue >> tap;  //Getting the Tap Position 's Value.
                Register.One = 1;  //Re-Initializing the One Variable.

                Register.XORedValue = (Register.LeftMostBit ^ Register.TapPosValue);  //XORing the LeftMostBit with the TapPosition's Value to get 
                                                                                     //the new bit.

                Register.Key[i] = Register.XORedValue; //Starting to fill the 8-bit Key
                Register.Seed = Register.Seed << 1;   //Shift the seed one step to the left
                Register.Seed = (Register.Seed | Register.XORedValue);   //Setting the new Register Seed.

                Register.One = 1;  //Re-Initializing the One Variable for the next Loop.                                                 
                seed = Register.Seed;
            }

            // Converting the Key Array into 1 BYTE to XOR it with the pixel component.
            
            int index = 8 - Register.Key.Length;
            
            byte res = 0;
            foreach (int i in Register.Key)
            {
                if (i == 1)
                    res |= (byte)(1 << (7 - index));
                index++;
            }
            
            return res;
        }

    }
}
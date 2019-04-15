using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageEncryptCompress
{
    class LFSR
    {
        public List<int> Seed;

        public int TapPos;
        int[] Key;
        int LeftMostBit;
        int XORedValue;
        LFSR()
        {
            Seed = new List<int>();
            TapPos = 0;
            Key = new int[8];
            LeftMostBit = 0;
            XORedValue = 0;
        }

        public static int[] GeneratingBits(ref List<int> seed, int tap)
        {
            LFSR Register = new LFSR();

            Register.Seed = seed; //Initial value of the LFSR entered by user
            if (tap < 0 && tap >= Register.Seed.Count) //checking if the user has entered the tap pos. as negative num or a num exceted the initial seed's lenght
                Console.WriteLine("Enter a correct Tap Position");
            //The tap position entered by user


            for (int i = 0; i < 8; i++)
            {
                Register.LeftMostBit = Register.Seed[0];
                Register.TapPos = Register.Seed[(Register.Seed.Count - 1) - tap];

                if (Register.LeftMostBit != Register.TapPos)
                    Register.XORedValue = 1;
                else
                    Register.XORedValue = 0;

                Register.Key[i] = Register.XORedValue;

                for (int j = 0; j < Register.Seed.Count - 1; j++)
                {
                    Register.Seed[j] = Register.Seed[j + 1];
                }
                Register.Seed[Register.Seed.Count - 1] = Register.XORedValue;
            }



            return Register.Key;

        }



        public static int[] XORing(int[] GeneratedBits, int[] PixelComponent)
        {

            int[] EncryptedPixel = new int[8];
            for (int i = 0; i < 8; i++)
            {
                EncryptedPixel[i] = (GeneratedBits[i] ^ PixelComponent[i]);
            }

            return EncryptedPixel;
        }


    }
}

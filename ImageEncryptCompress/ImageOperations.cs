using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    
  
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


        public static RGBPixel[,] Encryption(RGBPixel[,] ImageMatrix, ref long seed, int tap, int SeedLength)
        {
            int width = GetWidth(ImageMatrix);
            int height = GetHeight(ImageMatrix);
            byte Key = 0;
            RGBPixel[,] EncreptedImage = new RGBPixel[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {

                    Key = LFSR.GeneratingBits(ref seed, tap, SeedLength);
                    EncreptedImage[i, j].red = (byte)(ImageMatrix[i, j].red ^ Key);

                    Key = LFSR.GeneratingBits(ref seed, tap, SeedLength);
                    EncreptedImage[i, j].green = (byte)(ImageMatrix[i, j].green ^ Key);

                    Key = LFSR.GeneratingBits(ref seed, tap, SeedLength);
                    EncreptedImage[i, j].blue = (byte)(ImageMatrix[i, j].blue ^ Key);

                }
            }
            return EncreptedImage;
        }

        public static string AlphanumericSeed(string seed)
        {

            String alphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            string password = seed;
            var a = 0;
            int number = 0;
            byte[] arr = new byte[6] { 0, 0, 0, 0, 0, 0 };
            byte[] finalArr = new byte[password.Length * 6];
            int count = 0;
            

            for (int i = 0; i < password.Length; i++)
            {
                a = alphaNumeric.IndexOf(password[i]);
                number = a;
                int k = 5;
                Array.Clear(arr, 0, 6);
                //arr = new int[6] { 0, 0, 0, 0, 0, 0 };

                while (number != 0)
                {

                    if (number % 2 == 0)
                    {
                        arr[k] = 0;
                    }
                    else
                    {
                        arr[k] = 1;
                    }
                    number = number / 2;
                    k--;
                }
                for (int j = 0; j < arr.Length; j++)
                {
                    finalArr[count] = arr[j];
                    count++;
                }
            }

            var builder = new StringBuilder();
            Array.ForEach(finalArr, x => builder.Append(x));
            var result = builder.ToString();

            return result;
        }

        
       // //red
       // public static Dictionary<int, int> getRed(RGBPixel[,] ImageMatrix)
       // {
       //     int width = GetWidth(ImageMatrix);
       //     int height = GetHeight(ImageMatrix);
       //     Dictionary<int, int> dict = new Dictionary<int, int>();

       //     int freq;

       //     for (int i = 0; i < height; i++)
       //     {
       //         for (int j = 0; j < width; j++)
       //         {
       //             freq = ImageMatrix[i, j].red;
       //             if (dict.ContainsKey(freq)) {
       //                 int count = dict[freq] + 1;
       //                 dict[freq]=count;
       //             }
       //             else
       //             {
       //                 dict.Add(freq,1);
       //             }
       //         }
       //     }
       //     return dict;
       // }
       // //green
       // public static Dictionary<int, int> getGreen(RGBPixel[,] ImageMatrix)
       // {
       //     int width = GetWidth(ImageMatrix);
       //     int height = GetHeight(ImageMatrix);

       //     Dictionary<int, int> dict = new Dictionary<int, int>();

       //     int freq;

       //     for (int i = 0; i < height; i++)
       //     {
       //         for (int j = 0; j < width; j++)
       //         {
       //             freq = ImageMatrix[i, j].green;
       //             if (dict.ContainsKey(freq))
       //             {
       //                 int count = dict[freq] + 1;
       //                 dict[freq] = count;
       //             }
       //             else
       //             {
       //                 dict.Add(freq, 1);
       //             }
       //         }
       //     }
       //     return dict;
       // }
       // //blue
       // public static Dictionary<int, int> getBlue(RGBPixel[,] ImageMatrix)
       // {
       //     int width = GetWidth(ImageMatrix);
       //     int height = GetHeight(ImageMatrix);

       //     Dictionary<int, int> dict = new Dictionary<int, int>();

       //     int freq;

       //     for (int i = 0; i < height; i++)
       //     {
       //         for (int j = 0; j < width; j++)
       //         {
       //             freq = ImageMatrix[i, j].blue;
       //             if (dict.ContainsKey(freq))
       //             {
       //                 int count = dict[freq] + 1;
       //                 dict[freq] = count;
       //             }
       //             else
       //             {
       //                 dict.Add(freq, 1);
       //             }
       //         }
       //     }
       //     return dict;
       // }

       // public static byte GetByte(BitArray input) //Total O(1) //The input's size won't exceed 8
       // {

       //     int len = input.Length; //O(1)
       //     int output = 0; //O(1)
       //     for (int i = 0; i < len; i++)   //O(n)
       //         if (input.Get(i))
       //             output += (1 << (len - 1 - i));  //O(1)

       //     return (byte)output;
       // }

       // public static void compress(RGBPixel[,] image_matrix, Dictionary<int, List<bool>> red_dictionary, Dictionary<int, List<bool>> green_dictionary, Dictionary<int, List<bool>> blue_dictionary)
       // {
       //     FileStream file = new FileStream("compressed.bin", FileMode.Truncate);
       //     file.Close();

       //     int width = GetWidth(image_matrix); //O(1)
       //     int height = GetHeight(image_matrix);  //O(1)
       //     byte b = 0;  //O(1)
       //     int counterBit = 0; //O(1)
       //     int counterList = 0; //O(1)
       //     int TotalBits = 0; //O(1)
       //     int c = 0; //O(1)
       //     FileStream fs = new FileStream("compressed.bin", FileMode.Append);  //O(1)
       //     BinaryWriter br = new BinaryWriter(fs);  //O(1)

       //     br.Write(width);
       //     br.Write(height);

       //     int x = 0;  //O(1)

       //     List<byte> Barr = new List<byte>();

       //     //Total O(n^2)
       //     for (int i = 0; i < height; i++) //O(n)
       //     {
       //         for (int j = 0; j < width; j++)  //O(n)
       //         {
       //             counterList = red_dictionary[image_matrix[i, j].red].Count;  //O(1)
                    
       //             while (counterBit < 8) //O(1)
       //             {
       //                 b = (byte)(b << 1);  //O(1)

       //                 //construct byte
       //                 b = (byte)(b | Convert.ToByte(red_dictionary[image_matrix[i, j].red][x]));  //O(1)
       //                 x++;  //O(1)
       //                 counterBit++;  //O(1)
       //                 TotalBits++;
       //                 c++;  //O(1)

       //                 if (counterBit == 8) //byte constructed
       //                 {

       //                     Barr.Add(b);  //O(1)
       //                     b = 0;  //O(1)
       //                     counterBit = 0;  //O(1)
       //                 }
                        
       //                 if (c == counterList)   //reached the size of the list per element in the dictionary
       //                 {
       //                     x = 0; //O(1)
       //                     c = 0;  //O(1)
       //                     break;
       //                 }
       //             }
       //         }
       //     }

       //     if (b != 0)  //for the last byte in the list
       //     {
       //         Barr.Add(b); //O(1)
       //     }

       //     br.Write(TotalBits);
       //     br.Write(Barr.Count);
       //     for (int i = 0; i < Barr.Count; i++)
       //     {
       //         br.Write(Barr[i]);
       //     }

       //     c = 0;  //O(1)
       //     b = 0;  //O(1)
       //     counterBit = 0;  //O(1)
       //     counterList = 0;  //O(1)
       //     x = 0;  //O(1)
       //     Barr = new List<byte>();
       //     TotalBits = 0;  //O(1)
       //     //Total O(n^2)
       //     for (int i = 0; i < height; i++)  //O(n)
       //     {
       //         for (int j = 0; j < width; j++)  //O(n)
       //         {
       //             counterList = green_dictionary[image_matrix[i, j].green].Count;   //O(1)

       //             while (counterBit < 8)  //O(1)
       //             {
       //                 b = (byte)(b << 1);  //O(1)

       //                 b = (byte)(b | Convert.ToByte(green_dictionary[image_matrix[i, j].green][x]));  //O(1)
       //                 counterBit++;  //O(1)
       //                 TotalBits++;
       //                 c++;  //O(1)
       //                 x++;  //O(1)

       //                 if (counterBit == 8)
       //                 {
       //                     Barr.Add(b); //O(1)
       //                     b = 0;  //O(1)
       //                     counterBit = 0;  //O(1)
       //                 }
       //                 if (c == counterList)
       //                 {
       //                     x = 0;  //O(1)
       //                     c = 0;  //O(1)
       //                     break;
       //                 }
       //             }
       //         }
       //     }

       //     if (b != 0)
       //     {
       //         Barr.Add(b);  //O(1)
       //     }

       //     br.Write(TotalBits);
       //     br.Write(Barr.Count); //O(1)
       //     for (int i = 0; i < Barr.Count; i++) //O(n)
       //     {
       //         br.Write(Barr[i]);
       //     }

       //     b = 0;  //O(1)
       //     counterBit = 0;  //O(1)
       //     counterList = 0;  //O(1)
       //     c = 0;  //O(1)
       //     x = 0;  //O(1)
       //     Barr = new List<byte>(); //O(1)
       //     TotalBits = 0;

       //     //Total O(n^2)
       //     for (int i = 0; i < height; i++)  //O(n)
       //     {
       //         for (int j = 0; j < width; j++)  //O(n)
       //         {
       //             counterList = blue_dictionary[image_matrix[i, j].blue].Count;  //O(1)

       //             while (counterBit < 8)  //O(1)
       //             {
       //                 b = (byte)(b << 1);  //O(1)

       //                 b = (byte)(b | Convert.ToByte(blue_dictionary[image_matrix[i, j].blue][x]));  //O(1)
       //                 counterBit++;  //O(1)
       //                 TotalBits++; //O(1)
       //                 c++;  //O(1)
       //                 x++;  //O(1)

       //                 if (counterBit == 8)
       //                 {
       //                     Barr.Add(b);  //O(1)
       //                     b = 0;  //O(1)
       //                     counterBit = 0;  //O(1)
       //                 }
       //                 if (c == counterList)
       //                 {
       //                     x = 0;  //O(1)
       //                     c = 0;  //O(1)
       //                     break;
       //                 }
       //             }
       //         }
       //     }

       //     if (b != 0)
       //     {
       //         Barr.Add(b);  //O(1)
       //     }

       //     br.Write(TotalBits);
       //     br.Write(Barr.Count); //O(1)
       //     for (int i = 0; i < Barr.Count; i++) //O(n)
       //     {
       //         br.Write(Barr[i]);
       //     }

       //     br.Close();  //O(1)
       //     fs.Close();  //O(1)

       //     FileStream fileStream;
       //     BinaryWriter binaryWriter;
       //     int counter = 0;

       //     ImageEncryptCompress.HuffmanTree.countNodes(MainForm.redNode, ref counter);
       //     fileStream = new FileStream("compressed.bin", FileMode.Append);
       //     binaryWriter = new BinaryWriter(fileStream);
       //     binaryWriter.Write(counter);
       //     binaryWriter.Close();
       //     fileStream.Close();
       //     ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.redNode);

       //     counter = 0;
       //     ImageEncryptCompress.HuffmanTree.countNodes(MainForm.greenNode, ref counter);
       //     fileStream = new FileStream("compressed.bin", FileMode.Append);
       //     binaryWriter = new BinaryWriter(fileStream);
       //     binaryWriter.Write(counter);
       //     binaryWriter.Close();
       //     fileStream.Close();
       //     ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.greenNode);

       //     counter = 0;
       //     ImageEncryptCompress.HuffmanTree.countNodes(MainForm.blueNode, ref counter);
       //     fileStream = new FileStream("compressed.bin", FileMode.Append);
       //     binaryWriter = new BinaryWriter(fileStream);
       //     binaryWriter.Write(counter);
       //     binaryWriter.Close();
       //     fileStream.Close();
       //     ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.blueNode);

       //     binaryWriter.Close();
       //     fileStream.Close();

       // }

       // public static int data { get; set; }
       // public static ImageEncryptCompress.Node redNode;
       // public static ImageEncryptCompress.Node greenNode;
       // public static ImageEncryptCompress.Node blueNode;
       // static ImageEncryptCompress.PriorityQueue<int, ImageEncryptCompress.Node> Huffman = new ImageEncryptCompress.PriorityQueue<int, ImageEncryptCompress.Node>(data);

       // public static RGBPixel[,] decompress(RGBPixel[,] pixel) //O(n)
       // {
            
       //     List<bool> list_red = new List<bool>();
       //     List<bool> list_green = new List<bool>();
       //     List<bool> list_blue = new List<bool>();
       //     byte b;
       //     int w, h;
       //     byte[] arr_bye;
       //     int rc_list, gc_list, bc_list;
       //     FileStream fileStream = new FileStream("compressed.bin", FileMode.Open);
       //     BinaryReader binaryReader = new BinaryReader(fileStream);

       //     w = binaryReader.ReadInt32(); //O(1)
       //     Console.WriteLine(w); //O(1)
       //     h = binaryReader.ReadInt32(); //O(1)
       //     Console.WriteLine(h); //O(1)

       //     rc_list = binaryReader.ReadInt32(); //O(1)
       //     int r; //O(1)
       //     r = binaryReader.ReadInt32(); //O(1)
       //     arr_bye = new byte[r];
       //     for (int i = 0; i < r; i++) //O(n)
       //     {
       //         b = binaryReader.ReadByte(); //O(1)
       //         arr_bye[i] = b; //O(1)
       //         //Console.WriteLine(b);
       //     }
       //     list_red = Get_stream_List(rc_list, arr_bye);//O(n)

       //     gc_list = binaryReader.ReadInt32(); //O(1)
       //     r = binaryReader.ReadInt32(); //O(1)
       //     arr_bye = new byte[r];
       //     for (int i = 0; i < r; i++) //O(n)
       //     {
       //         b = binaryReader.ReadByte(); //O(1)
       //         arr_bye[i] = b; //O(1)
       //         Console.WriteLine(b);
       //     }
       //     list_green = Get_stream_List(gc_list, arr_bye); //O(n)

       //     bc_list = binaryReader.ReadInt32();//O(1)
       //     r = binaryReader.ReadInt32();//O(1)
       //     arr_bye = new byte[r];
       //     for (int i = 0; i < r; i++) //O(n)
       //     {
       //         b = binaryReader.ReadByte(); //O(1)
       //         arr_bye[i] = b; //O(1)
       //     }
       //     list_blue = Get_stream_List(bc_list, arr_bye); //O(n)

       //     int leaf_count;
       //     int color;
       //     leaf_count = binaryReader.ReadInt32(); //O(1)
       //     Dictionary<int, int> redDictionary = new Dictionary<int, int>();

       //     for (int i = 0; i < leaf_count; i++) //O(n)
       //     {
       //         color = binaryReader.ReadInt32(); //O(1)
       //         redDictionary.Add(i + 1, color); //O(1)
       //     }

       //     leaf_count = binaryReader.ReadInt32(); //O(1)
       //     Dictionary<int, int> greenDictionary = new Dictionary<int, int>();
       //     for (int i = 0; i < leaf_count; i++) //O(n)
       //     {
       //         color = binaryReader.ReadInt32(); //O(1)
       //         greenDictionary.Add(i + 1, color); //O(1)
       //         //Console.WriteLine(color);
       //     }

       //     leaf_count = binaryReader.ReadInt32(); //O(1)
       //     Dictionary<int, int> blueDictionary = new Dictionary<int, int>();
       //     for (int i = 0; i < leaf_count; i++)  //O(n)
       //     {
       //         color = binaryReader.ReadInt32(); //O(1)
       //         blueDictionary.Add(i + 1, color); //O(1)
       //     }
       //     binaryReader.Close();
       //     fileStream.Close();

       //     redNode = ImageEncryptCompress.HuffmanTree.getTree(redDictionary, Huffman);
       //     greenNode = ImageEncryptCompress.HuffmanTree.getTree(greenDictionary, Huffman);
       //     blueNode = ImageEncryptCompress.HuffmanTree.getTree(blueDictionary, Huffman);

       //     //pixel = RGB_img(w, h, list_red, list_green, list_blue);//O(n^2)
       //     return pixel;
       // }


       ///* public static int setImage(List<bool> list, ImageEncryptCompress.Node node, int i)
       // {
       //     bool x = list[i];
       //     if (x == true)
       //     {
       //         if (node.Left == null && node.Right == null)
       //             return node.Symbol;
       //         else
       //         {
       //             i++;
       //             return setImage(list, node.Right, i);
       //         }
       //     }
       //     else
       //     {
       //         if (node.Left == null && node.Right == null)
       //             return node.Symbol;
       //         else
       //         {
       //             i++;
       //             return setImage(list, node.Left, i); 
       //         }
       //     }
       // }*/


       // public static List<bool> Get_stream_List(int size, byte[] byte_arr) //O(n)
       // {
       //     List<bool> list = new List<bool>();//O(1)
       //     byte br;
       //     String st;
       //     BitArray arr;
       //     for (int i = 0; i < byte_arr.Length; i++)  //O(n)
       //     {
       //         br = byte_arr[i];  //O(1)
       //         st = Convert.ToString(br, 2); //O(1)

       //         if (i == byte_arr.Length - 1 && size < 8) //O(1)
       //         {
       //             arr = GetBitArray(st);
       //             for (int j =(8- size); j < 8; j++) //O(1)
       //                 list.Add(arr[j]);//O(1)

       //         }

       //         else  //O(1)
       //         {
       //             arr = GetBitArray(st);
       //             for (int j = 0; j < 8; j++) //O(1)
       //                 list.Add(arr[j]);
       //             size -= 8; //O(1)
       //         }
       //     }


       //     return list;
       // }
       // public static BitArray GetBitArray(String s) //O(1)
       // {
       //     BitArray array;
       //     array = new BitArray(8); //O(1)
       //     int j = 7; //O(1)
       //     for (int i = s.Length - 1; i >= 0; i--) //O(1)
       //     {
       //         if (s[i] == '0')//O(1)
       //             array[j] = false; //O(1)

       //         else
       //             array[j] = true; //O(1)

       //         j--; //O(1)
       //     }

       //     return array;
       // }

       // #region
       // //public static void compress(RGBPixel[,] image_matrix, Dictionary<int, List<bool>> red_dictionary, Dictionary<int, List<bool>> green_dictionary, Dictionary<int, List<bool>> blue_dictionary)
       // //{
       // //    List<bool> red_list = new List<bool>();
       // //    List<bool> green_list = new List<bool>();
       // //    List<bool> blue_list = new List<bool>();

       // //    int width = GetWidth(image_matrix);
       // //    int height = GetHeight(image_matrix);

       // //    for (int i = 0; i < height; i++)
       // //    {
       // //        for (int j = 0; j < width; j++)
       // //        {
       // //            red_list.AddRange(red_dictionary[image_matrix[i, j].red]);
       // //            green_list.AddRange(green_dictionary[image_matrix[i, j].green]);
       // //            blue_list.AddRange(blue_dictionary[image_matrix[i, j].blue]);
       // //        }
       // //    }

       // //    int zr_count = 0;
       // //    int r_index = 0;
       // //    while (red_list[r_index] != true)
       // //    {
       // //        zr_count++;
       // //        r_index++;
       // //    }

       // //    //int r_list_var = red_list.Count;
       // //    BitArray bitarray;
       // //    int size;
       // //    if (red_list.Count % 8 == 0)
       // //        size = red_list.Count / 8;
       // //    else
       // //        size = (red_list.Count / 8) + 1;

       // //    byte[] b_arr = new byte[size];
       // //    int ind_by = 0;
       // //    int ind;
       // //    int var = red_list.Count;

       // //    for (int i = 0; i < red_list.Count; i += 8)
       // //    {
       // //        bitarray = new BitArray(8);

       // //        if (var < 8)
       // //        {
       // //            ind = red_list.Count - 1;
       // //            for (int j = 7; j > (7 - var); j--)
       // //            {
       // //                bitarray.Set(j, red_list[ind]);
       // //                ind--;
       // //            }
       // //            b_arr[ind_by] = GetByte(bitarray);
       // //            ind_by++;
       // //        }
       // //        else
       // //        {
       // //            for (int j = i; j < 8; j++)
       // //            {
       // //                bitarray.Set(j, red_list[j]);
       // //            }

       // //            b_arr[ind_by] = GetByte(bitarray);
       // //            ind_by++;

       // //            var -= 8;
       // //        }
       // //    }

       // //    int zg_count = 0;
       // //    int g_index = 0;
       // //    while (green_list[g_index] != true)
       // //    {
       // //        zg_count++;
       // //        g_index++;
       // //    }

       // //    BitArray bitarray2;
       // //    int size2;
       // //    if (green_list.Count % 8 == 0)
       // //        size2 = green_list.Count / 8;
       // //    else
       // //        size2 = (green_list.Count / 8) + 1;

       // //    byte[] b_arr2 = new byte[size2];
       // //    int ind_by2 = 0;
       // //    int ind2;
       // //    int var2 = green_list.Count;

       // //    for (int i = 0; i < green_list.Count; i += 8)
       // //    {
       // //        bitarray2 = new BitArray(8);

       // //        if (var2 < 8)
       // //        {
       // //            ind2 = green_list.Count - 1;
       // //            for (int j = 7; j > (7 - var2); j--)
       // //            {
       // //                bitarray2.Set(j, green_list[ind2]);
       // //                ind2--;
       // //            }
       // //            b_arr2[ind_by2] = GetByte(bitarray2);
       // //            ind_by2++;
       // //        }
       // //        else
       // //        {
       // //            for (int j = i; j < 8; j++)
       // //            {
       // //                bitarray2.Set(j, green_list[j]);
       // //            }

       // //            b_arr2[ind_by2] = GetByte(bitarray2);
       // //            ind_by2++;

       // //            var2 -= 8;
       // //        }
       // //    }

       // //    int zb_count = 0;
       // //    int b_index = 0;
       // //    while (blue_list[b_index] != true)
       // //    {
       // //        zb_count++;
       // //        b_index++;
       // //    }


       // //    BitArray bitarray3;
       // //    int size3;
       // //    if (blue_list.Count % 8 == 0)
       // //        size3 = blue_list.Count / 8;
       // //    else
       // //        size3 = (blue_list.Count / 8) + 1;

       // //    byte[] b_arr3 = new byte[size3];
       // //    int ind_by3 = 0;
       // //    int ind3;
       // //    int var3 = blue_list.Count;

       // //    for (int i = 0; i < blue_list.Count; i += 8)
       // //    {
       // //        bitarray3 = new BitArray(8);

       // //        if (var3 < 8)
       // //        {
       // //            ind3 = blue_list.Count - 1;
       // //            for (int j = 7; j > (7 - var3); j--)
       // //            {
       // //                bitarray3.Set(j, blue_list[ind3]);
       // //                ind3--;
       // //            }
       // //            b_arr3[ind_by3] = GetByte(bitarray3);
       // //            ind_by3++;
       // //        }
       // //        else
       // //        {
       // //            for (int j = i; j < 8; j++)
       // //            {
       // //                bitarray3.Set(j, blue_list[j]);
       // //            }

       // //            b_arr3[ind_by3] = GetByte(bitarray3);
       // //            ind_by3++;

       // //            var3 -= 8;
       // //        }
       // //    }

       // //    FileStream file = new FileStream("compressed.bin", FileMode.Truncate);
       // //    file.Close();
       // //    FileStream fileStream;
       // //    BinaryWriter binaryWriter;

       // //    fileStream = new FileStream("compressed.bin", FileMode.Append);
       // //    binaryWriter = new BinaryWriter(fileStream);

       // //    binaryWriter.Write(zr_count);
       // //    binaryWriter.Write(zg_count);
       // //    binaryWriter.Write(zb_count);
       // //    binaryWriter.Write(red_list.Count);
       // //    binaryWriter.Write(green_list.Count);
       // //    binaryWriter.Write(blue_list.Count);

       // //    binaryWriter.Write(ImageOperations.GetWidth(image_matrix));
       // //    binaryWriter.Write(ImageOperations.GetHeight(image_matrix));
       // //    binaryWriter.Write(size);
       // //    foreach (byte item in b_arr)
       // //    {
       // //        binaryWriter.Write(item);
       // //    }
       // //    binaryWriter.Write(size2);
       // //    foreach (byte item in b_arr2)
       // //    {
       // //        binaryWriter.Write(item);
       // //    }
       // //    binaryWriter.Write(size3);
       // //    foreach (byte item in b_arr3)
       // //    {
       // //        binaryWriter.Write(item);
       // //    }

       // //    binaryWriter.Close();
       // //    fileStream.Close();


       // //    int counter = 0;

       // //    ImageEncryptCompress.HuffmanTree.countNodes(MainForm.redNode, ref counter);
       // //    fileStream = new FileStream("compressed.bin", FileMode.Append);
       // //    binaryWriter = new BinaryWriter(fileStream);
       // //    binaryWriter.Write(counter);
       // //    binaryWriter.Close();
       // //    fileStream.Close();
       // //    ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.redNode);

       // //    counter = 0;
       // //    ImageEncryptCompress.HuffmanTree.countNodes(MainForm.greenNode, ref counter);
       // //    fileStream = new FileStream("compressed.bin", FileMode.Append);
       // //    binaryWriter = new BinaryWriter(fileStream);
       // //    binaryWriter.Write(counter);
       // //    binaryWriter.Close();
       // //    fileStream.Close();
       // //    ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.greenNode);

       // //    counter = 0;
       // //    ImageEncryptCompress.HuffmanTree.countNodes(MainForm.blueNode, ref counter);
       // //    fileStream = new FileStream("compressed.bin", FileMode.Append);
       // //    binaryWriter = new BinaryWriter(fileStream);
       // //    binaryWriter.Write(counter);
       // //    binaryWriter.Close();
       // //    fileStream.Close();
       // //    ImageEncryptCompress.HuffmanTree.traverseTree(MainForm.blueNode);

       // //    binaryWriter.Close();
       // //    fileStream.Close();

       // //} //end of compress func.
       // #endregion
    }
}
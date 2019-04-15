using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ImageEncryptCompress;
using System.IO;
using System.Diagnostics;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        public static Node redNode;
        public static Node blueNode;
        public static Node greenNode;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            MessageBox.Show("Enter a Tap Position and an Initial Seed! ");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch d1 = new Stopwatch();
           

            LFSR obj1 = new LFSR();
            obj1.TapPosIndx = (int)nudMaskSize.Value;
            obj1.Seed = Convert.ToInt64(textBox1.Text, 2);        //5adet el string as bits where 2 means binary
            int SeedLength = textBox1.Text.Length - 1;

            d1.Start();
            ImageMatrix = ImageOperations.Encryption(ImageMatrix, ref obj1.Seed, obj1.TapPosIndx, SeedLength);
            d1.Stop();

            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            MessageBox.Show ("RunTime = " + Convert.ToString(d1.Elapsed.TotalMinutes) );


        } //Encryption & decryption

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //PriorityQueue<int, Node> Huffman = new PriorityQueue<int, Node>(data);

            //// contains a dictionnary that contanins the number of frequencies for each color
            //Dictionary<int, int> red = new Dictionary<int, int>();
            //Dictionary<int, int> green = new Dictionary<int, int>();
            //Dictionary<int, int> blue = new Dictionary<int, int>();

            ////contains the binary code for each frequency for each color
            //Dictionary<int, List<bool>> redBinary = new Dictionary<int, List<bool>>();
            //Dictionary<int, List<bool>> greenBinary = new Dictionary<int, List<bool>>();
            //Dictionary<int, List<bool>> blueBinary = new Dictionary<int, List<bool>>();

            ////functions that returns the dictionaries that contain the number of frequencies
            //red = ImageOperations.getRed(ImageMatrix);
            //green = ImageOperations.getGreen(ImageMatrix);
            //blue = ImageOperations.getBlue(ImageMatrix);

            //// get the binary code for each color
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //redBinary = HuffmanTree.getBinary(red, Huffman);
            //redNode = HuffmanTree.root;
            //greenBinary = HuffmanTree.getBinary(green, Huffman);
            //greenNode = HuffmanTree.root;
            //blueBinary = HuffmanTree.getBinary(blue, Huffman);
            //blueNode = HuffmanTree.root;

            //HuffmanTree.getBitSum(redBinary, red);
            //HuffmanTree.getBitSum(greenBinary, green);
            //HuffmanTree.getBitSum(blueBinary, blue);

            //double originalSize = ImageOperations.GetWidth(ImageMatrix) * ImageOperations.GetHeight(ImageMatrix) * 3;
            //double compRatio = HuffmanTree.saveCompressionRatio(originalSize);
            //ratiotxt.Text = compRatio.ToString();


            //ImageOperations.compress(ImageMatrix, redBinary, greenBinary, blueBinary);
            //sw.Stop();
            ////int elapsedtime = (int)sw.ElapsedMilliseconds / 1000;
            //MessageBox.Show("Elapsed time for compression = " + sw.ElapsedMilliseconds.ToString()+" milliseconds");
            //MessageBox.Show("The file has been successfully compressed and Huffman tree has been saved in a file", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //HuffmanTree.save_in_file(redBinary, red);
            //HuffmanTree.save_in_file(greenBinary, green);
            //HuffmanTree.save_in_file(blueBinary, blue);
        } //Huffman Code

        public int data { get; set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
           
            Stopwatch d1 = new Stopwatch();
            LFSR obj1 = new LFSR();
            obj1.TapPosIndx = (int)nudMaskSize.Value;

            if (textBox1.Text.Length > 10)
            {
                MessageBox.Show("please enter an initial seed less than 10 characters");
            }
            else
            {
                string seed = ImageOperations.AlphanumericSeed(textBox1.Text);
                obj1.Seed = Convert.ToInt64(seed, 2);        //5adet el string as bits where 2 means binary
                int SeedLength = (textBox1.Text.Length * 6);

                d1.Start();
                ImageMatrix = ImageOperations.Encryption(ImageMatrix, ref obj1.Seed, obj1.TapPosIndx, SeedLength);
                d1.Stop();

                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
                MessageBox.Show("RunTime = " + Convert.ToString(d1.Elapsed.TotalMinutes));

            }

            

        }
    }
}
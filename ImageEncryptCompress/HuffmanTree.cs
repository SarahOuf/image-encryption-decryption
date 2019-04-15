using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ImageEncryptCompress
{
    public class HuffmanTree
    {
        //public static void getCode(Node current , String code , ref Dictionary<int,String> res)
        //{
        //    if(current.Right==null && current.Left==null && current.Symbol!=-1)
        //    {
        //        if (res.ContainsKey(current.Symbol))
        //        {
        //            res[current.Symbol] = code;
        //        }
        //        else
        //        {
        //            res.Add(current.Symbol, code);
        //        }
        //        return;
        //    }
        //    if (current.Left != null)
        //    {
        //        getCode(current.Left, code + "0" , ref res);
        //    }
        //    if(current.Right!=null)
        //    {
        //        getCode(current.Right, code + "1" , ref res);
        //    }
        //}

        static int i = 0;

        public static void getCode(Node current, String code, ref Dictionary<int, List<bool>> res)
        {
            if (current.Right == null && current.Left == null && current.Symbol != -1)
            {
                if (res.ContainsKey(current.Symbol))
                {
                    List<bool> x = new List<bool>();
                    for (int i = 0; i < code.Length; i++)
                    {
                        if (code[i] == '0')
                            x.Add(false);
                        else
                            x.Add(true);
                    }
                    res[current.Symbol] = x;

                }
                else
                {
                    List<bool> x = new List<bool>();
                    for (int i = 0; i < code.Length; i++)
                    {
                        if (code[i] == '0')
                            x.Add(false);
                        else
                            x.Add(true);
                    }
                    res.Add(current.Symbol, x);

                }
                return;
            }
            if (current.Left != null)
            {
                getCode(current.Left, code + "0", ref res);
            }
            if (current.Right != null)
            {
                getCode(current.Right, code + "1", ref res);
            }
        }
        public static Node root;

        public static Dictionary<int, List<bool>> getBinary(Dictionary<int, int> color, PriorityQueue<int, Node> Huffman)
        {
            //insert the frequencies as nodes
            foreach (KeyValuePair<int, int> entry in color)
            {
                int frequency = entry.Key;
                int counter = entry.Value;
                Node leaf = new Node(frequency);
                Huffman.Enqueue(counter, leaf);
            }

            //construct the tree
            while (Huffman.Count != 1)
            {
                System.Collections.Generic.KeyValuePair<int, Node> rightLeaf = Huffman.Dequeue();
                System.Collections.Generic.KeyValuePair<int, Node> leftLeaf = Huffman.Dequeue();

                int newKey = rightLeaf.Key + leftLeaf.Key;

                Node newValue = new Node(-1);
                newValue.Right = rightLeaf.Value;
                newValue.Left = leftLeaf.Value;

                Huffman.Enqueue(newKey, newValue);
            }

            System.Collections.Generic.KeyValuePair<int, Node> rootNode = Huffman.Dequeue();
            root = rootNode.Value;
            Dictionary<int, List<bool>> res = new Dictionary<int, List<bool>>();
            getCode(root, "" ,ref  res);

            return res;
        }

        public static void save_in_file(Dictionary<int, String> dictionary, Dictionary<int, int> dictionary2)
        {
            int key; //integer value to take the key in it
            int freq;
            String value; //string value to take the code in it
            FileStream fileStream = new FileStream("compress.txt", FileMode.Append); //make file stream to open or create a file then to write in it 
            StreamWriter streamWriter = new StreamWriter(fileStream); //make stream writer to enable me to write in the file

            foreach (KeyValuePair<int, string> item in dictionary) //loop to get every item in the dictionary that holds the codes and save it in the file
            {
                key = item.Key;
                value = item.Value;
                freq = dictionary2[key];

                streamWriter.Write(key);
                streamWriter.Write(" _ ");
                streamWriter.Write(freq);
                streamWriter.Write(" _ ");
                streamWriter.WriteLine(value);
            }
            streamWriter.Close();

        }

        public static double bitSum = 0;
        public static double getBitSum(Dictionary<int, List<bool>> dictionary, Dictionary<int, int> dictionary2)
        {
            int key;
            int freq;
            List<bool> value;
            int totalSum = 0;
            foreach (KeyValuePair<int, List<bool>> item in dictionary)
            {
                key = item.Key;
                value = item.Value;
                freq = dictionary2[key];
                int size = item.Value.Count;
                int total = size * freq;
                totalSum += total;
            }
            bitSum += totalSum;

            return bitSum;
        }

        public static double saveCompressionRatio(double origSize) //Total O(1)
        {
            //FileStream fileStream = new FileStream("compress.txt", FileMode.Append); //make file stream to open or create a file then to write in it 
            //StreamWriter streamWriter = new StreamWriter(fileStream); //make stream writer to enable me to write in the file

            double ratio;
            bitSum = bitSum / 8;    //O(1)
            ratio = (bitSum / origSize); //O(1)
            ratio *= 100; //O(1)

            //streamWriter.WriteLine("Compression Output = " + bitSum.ToString());//O(1)
            //streamWriter.WriteLine("Compression Ratio = " + ratio.ToString() + "%");//O(1)
            //streamWriter.Close();
            //fileStream.Close();
            return ratio;
        }

        public static void countNodes(Node current, ref int counter) //Total theta(n) :- T(n) = 2T(n/2) + O(1)
        {
            if (current.Right == null && current.Left == null)
            {
                counter++;
                return;
            }
            if (current.Right != null)
            {
                countNodes(current.Right, ref counter);
            }
            if (current.Left != null)
            {
                countNodes(current.Left, ref counter);
            }

        }

        public static void traverseTree(Node current)  //Total theta(n) :- T(n) = 2T(n/2) + O(1)
        {
            if (current.Right == null && current.Left == null)
            {
                FileStream fs = new FileStream("compressed.bin", FileMode.Append);
                BinaryWriter sw = new BinaryWriter(fs);
                sw.Write(current.Symbol);
                sw.Close();
                fs.Close();
                return;
            }
            if (current.Right != null)
            {
                traverseTree(current.Right);
            }

            if (current.Left != null)
            {
                traverseTree(current.Left);
            }
        }

        public static Node getTree(Dictionary<int, int> color, PriorityQueue<int, Node> Huffman)
        {
            //insert the frequencies as nodes
            foreach (KeyValuePair<int, int> entry in color)
            {
                int frequency = entry.Key;
                int counter = entry.Value;
                Node leaf = new Node(frequency);
                Huffman.Enqueue(counter, leaf);
            }

            //construct the tree
            while (Huffman.Count != 1)
            {
                System.Collections.Generic.KeyValuePair<int, Node> rightLeaf = Huffman.Dequeue();
                System.Collections.Generic.KeyValuePair<int, Node> leftLeaf = Huffman.Dequeue();

                int newKey = rightLeaf.Key + leftLeaf.Key;

                Node newValue = new Node(-1);
                newValue.Right = rightLeaf.Value;
                newValue.Left = leftLeaf.Value;

                Huffman.Enqueue(newKey, newValue);
            }

            System.Collections.Generic.KeyValuePair<int, Node> rootNode = Huffman.Dequeue();
            root = rootNode.Value;
            //Dictionary<int, List<bool>> res = new Dictionary<int, List<bool>>();
            //getCode(root, "", ref  res);

            return root;
        }

    }
}

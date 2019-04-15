using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageEncryptCompress
{
    public class Node
    {
        public int Symbol { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        public Node(int sym)
        {
            Left = null;
            Right = null;
            Symbol = sym;
        }
    }
}

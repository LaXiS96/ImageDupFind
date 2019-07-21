using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using LaXiS.VantagePointTree;

namespace LaXiS.ImageDupFind
{
    class Program
    {
        static void Main(string[] args)
        {
            var items = new List<ImageHash>();

            using (var reader = new StreamReader("data.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(',');
                    items.Add(new ImageHash(fields[1], UInt64.Parse(fields[2], NumberStyles.HexNumber), fields[3]));
                }
            }

            var vpTree = new VantagePointTree<ImageHash>(items);

            using (StreamWriter file = new StreamWriter(@"results.txt"))
            {
                foreach (var imageHash in items)
                {
                    var results = vpTree.Search(imageHash, 5);
                    foreach (var result in results)
                    {
                        if (imageHash.FileName != result.Item.FileName && result.Distance <= 5)
                            file.WriteLine($"{imageHash.FileName} {result.Item.FileName} {result.Distance} {imageHash.Md5Hash == result.Item.Md5Hash}");
                    }
                }
            }
        }
    }

    public class ImageHash : ITreeItem<ImageHash>
    {
        public string Md5Hash;
        public UInt64 PerceptualHash;
        public string FileName;

        public ImageHash(string md5Hash, UInt64 perceptualHash, string fileName)
        {
            Md5Hash = md5Hash;
            PerceptualHash = perceptualHash;
            FileName = fileName;
        }

        public double DistanceFrom(ImageHash i)
        {
            return HammingDistance(PerceptualHash, i.PerceptualHash);
        }

        public static int HammingDistance(UInt64 hash1, UInt64 hash2)
        {
            UInt64 xor = hash1 ^ hash2;
            int distance = 0;

            while (xor > 0)
            {
                distance += (int)xor & 1;
                xor >>= 1;
            }

            return distance;
        }

        public override string ToString()
        {
            return $"{Md5Hash},{PerceptualHash},{FileName}";
        }
    }
}

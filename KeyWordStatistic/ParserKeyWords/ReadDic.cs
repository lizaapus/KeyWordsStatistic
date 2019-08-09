using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyWordStatistic
{

    public class ReadDic
    {
        public string fileName { get; set; } 
        public string path { get; set; } 
        public ReadDic()
        {
            fileName = "prase.txt";
            path = @"G:\img\";
        }
        public Dictionary<string, string> dic()
        {
            path = path + fileName;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            StreamReader sR = File.OpenText(path);
            string nextLine;
            while ((nextLine = sR.ReadLine()) != null)
            {
                var strs = nextLine.Split('\t');
                if (strs.Count() >= 2)
                    dic.Add(strs[0], strs[1]);
                //Console.WriteLine(nextLine);
            }
            sR.Close();
            return dic;
        }
    }
}

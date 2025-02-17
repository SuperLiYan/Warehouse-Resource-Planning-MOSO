using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Warehouse_Sim_opti.Tool
{
    internal class Reader
    {
        public static List<String[]> ReadCSV(string filePathName)
        {
            List<String[]> ls = new List<String[]>();
            StreamReader fileReader = new StreamReader(filePathName);
            string strLine = "";
            while (strLine != null)
            {
                strLine = fileReader.ReadLine();
                if (strLine != null && strLine.Length > 0)
                {
                    ls.Add(strLine.Split(','));   //以空格做分隔符
                                                  //Debug.WriteLine(strLine);
                }
            }
            fileReader.Close();
            return ls;
        }

        public static List<String[]> Readtxt(string filePathName)
        {
            List<String[]> ls = new List<String[]>();
            StreamReader fileReader = new StreamReader(filePathName);
            string strLine = "";
            while (strLine != null)
            {
                strLine = fileReader.ReadLine();
                if (strLine != null && strLine.Length > 0)
                {
                    ls.Add(strLine.Split(' '));   //以空格做分隔符
                                                  //Debug.WriteLine(strLine);
                }
            }
            fileReader.Close();
            return ls;
        }

        public static List<List<decimal>> CreatArray(List<String[]> Sr)
        {
            List<List<decimal>> _Array = new List<List<decimal>>();
            for (int t = 0; t < Sr.Count; t++)
            {
                _Array.Add(new List<decimal>());
                for (int i = 0; i < Sr[t].Length; i++)
                {
                    if (Sr[t][i].Length != 0)
                    {
                        _Array[t].Add(decimal.Parse(Sr[t][i]));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return _Array;
        }

        public static List<List<string>> CreatString(List<String[]> Sr)
        {
            List<List<string>> _Array = new List<List<string>>();
            for (int t = 0; t < Sr.Count; t++)
            {
                _Array.Add(new List<string>());
                for (int i = 0; i < Sr[t].Length; i++)
                {
                    if (Sr[t][i].Length != 0)
                    {
                        _Array[t].Add((Sr[t][i]));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return _Array;
        }

        /// <summary>
        /// 根据路径读一行一行读取出文件类容
        /// </summary>
        /// <param name="path"></param>
        public static void GetFileContent(string path)
        {
            if (!File.Exists(path)) return;
            StreamReader reader = new StreamReader(path, Encoding.Default);
            string content = string.Empty;
            while (!reader.EndOfStream)
            {
                //一行一行的内容
                content = reader.ReadLine();
                Console.WriteLine(content);

            }
            reader.Close();
            reader.Dispose();
        }
    }
}

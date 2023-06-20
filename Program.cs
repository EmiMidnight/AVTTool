using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO.Enumeration;

/* 
 * Tool to parse .avt files from Initial D Arcade Stage games.
 * Extracts metadata into json, and each texture into a transparent .png
*/
namespace AVTTool
{
    public class Program
    {
        public static string filePath = "";
        public static string fileName = "";

        static void Main(string[] args)
        {
            Console.WriteLine("AVTTool by PockyWitch");
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify a file to load, like this:");
                Console.WriteLine("AVTTool.exe wmn_256_fix_bis_full.avt");
                return;
            }

            filePath = args[0];
            fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"{filePath} does not exist. Exiting.");
                return;
            }

            string fileExtension = Path.GetExtension(filePath);

            switch (fileExtension)
            {
                case ".avt":
                    AvatarFileHandler.ProcessAvatarFile(filePath); 
                    break;
                case ".dat":
                    MyFrameHandler.ProcessMyFrameFile(filePath);
                    break;
                default:
                    Console.WriteLine("Unknown file extension. Please make sure avatar files have their original .avt, and backgrounds have the original .dat extension");
                    return;
            }

        }

    }
}
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
        class AvatarFile
        {
            // This will get set to 256 if the first flag is set. Otherwise 128 is the games default
            public int textureSize = 128;
            // this gets added to each entries offset, aka offset past file entrylist
            public UInt32 baseOffset = 0;
            // Needed to calculate the very last texture filesize
            public UInt32 fileSize = 0;
        }

        class AvatarEntry
        {
            // the category (head, hair etc)
            public int categoryId { get; set; } = 0;
            public string categoryName { get; set; } = "";
            // the actual id for each part
            public int id { get; set; } = 0;
            // unsure atm, but it increases with parts with the same name, so maybe color
            public int colorMaybe { get; set; } = 0;
            // the offset to the actual texture file data
            public UInt32 offset = 0;
            // avatar part names are in shift-jis
            public string name { get; set; } = "";
        }

        static AvatarFile avatarData = new();
        static List<AvatarEntry> partsList = new();
        static long fileSize = 0;
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
            }

            Console.WriteLine("Parsing file, please wait ...");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding shiftJIS = Encoding.GetEncoding("shift-jis");

            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader binaryReader = new(fs);

            fileSize = fs.Length;

            // parse main metadata
            int format = binaryReader.ReadUInt16();
            if (format == 255)
            {
                avatarData.textureSize = 256;
            }

            // skip unknown int16
            fs.Seek(0x04, SeekOrigin.Begin);
            avatarData.baseOffset = binaryReader.ReadUInt32();

            // and skip to the actual file entry list
            fs.Seek(0x20, SeekOrigin.Begin);

            // we do not know how many entries there are, so let's keep a while loop
            // and just break out of it once we're done
            while (true)
            {
                AvatarEntry currentEntry = new();
                currentEntry.categoryId = binaryReader.ReadByte();

                // skip final entry, and weird second list?
                if (currentEntry.categoryId == 0xF0 || currentEntry.categoryId == 0x0F)
                {
                    break;
                }

                if (currentEntry.categoryId == 0xEF)
                {
                    // skip mannequin
                    fs.Seek(0x1F, SeekOrigin.Current);
                    continue;
                }

                currentEntry.categoryId = currentEntry.categoryId & 0x0F;

                switch (currentEntry.categoryId)
                {
                    case 1:
                        currentEntry.categoryName = "Head";
                        break;
                    case 2:
                        currentEntry.categoryName = "Clothing";
                        break;
                    case 3:
                        currentEntry.categoryName = "Eyes";
                        break;
                    case 4:
                        currentEntry.categoryName = "Mouth/Nose";
                        break;
                    case 5:
                        currentEntry.categoryName = "Accessoire";
                        break;
                    case 6:
                        currentEntry.categoryName = "Glasses";
                        break;
                    case 7:
                        currentEntry.categoryName = "Hair";
                        break;
                }

                currentEntry.colorMaybe = binaryReader.ReadByte();
                currentEntry.id = binaryReader.ReadUInt16();
                currentEntry.offset = avatarData.baseOffset + binaryReader.ReadUInt32();
                currentEntry.name = shiftJIS.GetString(binaryReader.ReadBytes(24));
                currentEntry.name = currentEntry.name.Replace("\0", string.Empty);
                Console.WriteLine($"Found {currentEntry.name} with ID {currentEntry.id}, current file offset: {fs.Position}");
                partsList.Add(currentEntry);
            }

            // save part metadata for use in other applications like card editors
            System.IO.Directory.CreateDirectory(fileName);
            var json = JsonSerializer.Serialize(partsList, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText($"{fileName}/{fileName}.json", json);

            Console.WriteLine("Exporting textures, please wait");
            // Metadata parsed, let's grab those textures
            for (int i = 0; i < partsList.Count; i++)
            {
                uint textureSize = 0;
                if (i < partsList.Count - 1)
                {
                    textureSize = partsList[i + 1].offset - partsList[i].offset;
                }
                else
                {
                    textureSize = (uint)(fileSize - partsList[i].offset);
                }

                // no metadata
                textureSize -= 10;

                // first 6 bytes are unneeded metadata still
                fs.Seek(partsList[i].offset + 6, SeekOrigin.Begin);
                int width = binaryReader.ReadUInt16();
                int height = binaryReader.ReadUInt16();
                byte[] rgbaBytes = binaryReader.ReadBytes((int)textureSize);
                ImageProcessing.ExportTexture(partsList[i].id, rgbaBytes, width, height);
            }
            Console.WriteLine($"All done. Textures have been saved in a new folder called {fileName}");
        }

    }
}
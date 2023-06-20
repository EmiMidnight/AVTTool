using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AVTTool.AvatarFileHandler;

namespace AVTTool
{
    public class MyFrameHandler
    {
        public static string fileName = "";
        public static int backgroundCount = 0;
        public class MyFrameEntry
        {
            // the id that gets saved/loaded when it comes to cards
            public int id { get; set; } = 0;
            // the offset to the actual texture file data
            public UInt32 offset = 0;
            // filesize of the texture
            public UInt32 size = 0;
            // the offset of the next texture. only important for 2 layer frames
            public UInt32 next_offset = 0;
            // second layer texture filesize
            public UInt32 second_layer_size = 0;
            // myframe names are in shift-jis
            public string name { get; set; } = "";
        }

        static List<MyFrameEntry> backgroundList = new();
        public static void ProcessMyFrameFile(string filePath)
        {
            fileName = Path.GetFileNameWithoutExtension(filePath);

            Console.WriteLine("Parsing myframe file, please wait ...");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding shiftJIS = Encoding.GetEncoding("shift-jis");

            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader binaryReader = new(fs);

            backgroundCount = binaryReader.ReadInt32();

            for (int i = 0; i < backgroundCount; i++)
            {
                MyFrameEntry entry = new MyFrameEntry();
                entry.id = binaryReader.ReadInt32();
                entry.offset = binaryReader.ReadUInt32();
                entry.size = binaryReader.ReadUInt32();
                entry.next_offset = binaryReader.ReadUInt32();
                entry.second_layer_size = binaryReader.ReadUInt32();
                entry.name = shiftJIS.GetString(binaryReader.ReadBytes(32));
                entry.name = entry.name.Replace("\0", string.Empty);

                // skip last 4 bytes, not needed
                binaryReader.ReadBytes(4);
                Console.WriteLine($"Found {entry.name} with ID {entry.id}");
                backgroundList.Add(entry);
            }
            System.IO.Directory.CreateDirectory(fileName);
            var json = JsonSerializer.Serialize(backgroundList, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText($"{fileName}/{fileName}.json", json);

            for (int i = 0; i < backgroundCount; i++)
            {
                var entry = backgroundList[i];
                fs.Seek(entry.offset, SeekOrigin.Begin);
                byte[] rgbaBytes = binaryReader.ReadBytes((int)entry.size);
                ImageProcessing.ExportMyFrame(entry, true, rgbaBytes);
                if(entry.second_layer_size > 0)
                {
                    fs.Seek(entry.next_offset, SeekOrigin.Begin);
                    byte[] rgbaBytes2 = binaryReader.ReadBytes((int)entry.second_layer_size);
                    ImageProcessing.ExportMyFrame(entry, false, rgbaBytes2);
                }
            }
        }
    }
}

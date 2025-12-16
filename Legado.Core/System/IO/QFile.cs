using System.Text;

namespace System.IO
{
    public static class QFile
    {
        public static byte[] ReadAllBytes(string filePath)
        {
            FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
            fileStream.Close();
            return data;
        }

        public static string ReadAllText(string filePath, Encoding encoding)
        {
            byte[] data = ReadAllBytes(filePath);
            return encoding.GetString(data);
        }

        public static string ReadAllText(string filePath)
        {
            return ReadAllText(filePath, Encoding.Default);
        }

        public static void CreateFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        public static void CreateFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}

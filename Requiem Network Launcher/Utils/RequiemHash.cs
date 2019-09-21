using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Requiem_Network_Launcher.Utils
{
    public class RequiemHash
    {
        /// <summary>
        /// Compute hash values of desired files using SHA512 algorithm
        /// </summary>
        /// <param name="rootDirectory">base directory to search for files</param>
        /// <param name="fileFilter">desired file extensions</param>
        /// <returns>dictionary with key = file's name and value = file's content in hex string</returns>
        public static Dictionary<string, string> Calculate(string rootDirectory, string[] fileFilter, out Dictionary<string,string> nameHashPairs)
        {
            var keyValuePairs = new Dictionary<string, string>();
            nameHashPairs = new Dictionary<string, string>();
            string[] exclusion = { "info.txt", "version.txt",
                @"platform\scaleform\fontconfig.txt", "config_material.txt", @"bin\config_material.txt" , "scanningresult.txt" };

            // create DirectoryInfo object
            var dirInfo = new DirectoryInfo(rootDirectory);

            // get files with desired extensions
            // NOTE: check EmurateFiles vs GetFiles performance-wise
            FileInfo[] files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => fileFilter.Contains(f.Extension.ToLower())).ToArray();
            FileInfo[] files2 = dirInfo.EnumerateFiles(fileFilter[1], SearchOption.AllDirectories).ToArray();

            // Fooler
            HMACSHA1 hProvider1 = new HMACSHA1();
            hProvider1.Dispose();
            HMACSHA256 hProvider2 = new HMACSHA256();
            hProvider2.Dispose();
            HMACSHA384 hProvider3 = new HMACSHA384();
            hProvider3.Dispose();
            HMACSHA512 hProvider4 = new HMACSHA512();
            hProvider4.Dispose();
            SHA1 hProvider5 = SHA1.Create();
            hProvider1.Dispose();
            SHA256 hProvider6 = SHA256.Create();
            hProvider6.Dispose();
            SHA384 hProvider7 = SHA384.Create();
            hProvider7.Dispose();
            SHA512 hProvider8 = SHA512.Create();
            hProvider8.Dispose();
            SHA1Managed hProvider9 = new SHA1Managed();
            hProvider9.Dispose();
            SHA256Managed hProvider10 = new SHA256Managed();
            hProvider10.Dispose();
            SHA384Managed hProvider11 = new SHA384Managed();
            hProvider11.Dispose();

            // SHA512Managed seems to be faster than SHA512
            using (SHA512Managed hashProvider = new SHA512Managed())
            {
                foreach (FileInfo file in files)
                {
                    try
                    {
                        string fileRelativePath = Util.GetRelativePath(rootDirectory, file.FullName);

                        // check exclusion files list
                        if (Array.Exists(exclusion, fileName => fileName == fileRelativePath.ToLower()) || fileRelativePath.ToLower().Contains(@"dump\") 
                            || fileRelativePath.ToLower().Contains(@"resource\"))
                            continue;

                        // compute hash for file's name
                        byte[] nameHashValue = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(fileRelativePath));
                        
                        // open file
                        FileStream fileStream = file.Open(FileMode.Open);
                        fileStream.Position = 0;

                        // compute hash for file's content
                        byte[] contentHashValue = hashProvider.ComputeHash(fileStream);

                        // close file
                        fileStream.Close();

                        // add to key value pairs
                        keyValuePairs.Add(Truncate256(nameHashValue), Truncate256(contentHashValue));
                        nameHashPairs.Add(Truncate256(nameHashValue), fileRelativePath);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"I/O Exception: {e.Message}");
                        Console.ReadKey();
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine($"Access Exception: {e.Message}");
                        Console.ReadKey();
                    }
                }

                try
                {
                    // compute hash for hfs file name
                    string fileRelativePath = Util.GetRelativePath(rootDirectory, files2[0].FullName);
                    byte[] nameHashValue = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(fileRelativePath));

                    // open file
                    FileStream fileStream = files2[0].Open(FileMode.Open);
                    fileStream.Position = 0;

                    // compute hash for file's content
                    byte[] contentHashValue = hashProvider.ComputeHash(fileStream);

                    // close file
                    fileStream.Close();

                    // add to key value pairs
                    keyValuePairs.Add(Truncate256(nameHashValue), Truncate256(contentHashValue));
                    nameHashPairs.Add(Truncate256(nameHashValue), fileRelativePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine($"I/O Exception: {e.Message}");
                    Console.ReadKey();
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine($"Access Exception: {e.Message}");
                    Console.ReadKey();
                }
            }
            
            return keyValuePairs;
        }

        /// <summary>
        /// SHA512/256 truncate version
        /// </summary>
        /// <param name="value">hash values</param>
        /// <returns>64 characters (256 bits) long string</returns>
        private static string Truncate256(byte[] value)
        {
            string hexString = BitConverter.ToString(value).Replace("-", string.Empty);

            if (string.IsNullOrEmpty(hexString)) throw new ArgumentNullException("OOF");

            return hexString.Length <= 64 ? hexString : hexString.Substring(0, 64);
        }
    }
}

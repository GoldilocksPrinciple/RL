using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Net;

namespace Requiem_Network_Launcher
{
    class UserInfoRegistry
    {
        private static RegistryKey keyCurrentUser = Registry.CurrentUser;
        private static string _username;
        private static byte[] _encryptedPassword;
        private static byte[] _IV;
        private static string _loginToken;

        public static string Username { get => _username; set => _username = value; }
        public static byte[] EncryptedPassword { get => _encryptedPassword; set => _encryptedPassword = value; }
        public static byte[] IV { get => _IV; set => _IV = value; }
        public static string LoginToken { get => _loginToken; set => _loginToken = value; }
        
        public static void SaveUserLoginInfo(string username, byte[] encryptedPassword, byte[] IV)
        {
            RegistryKey requiemKey = keyCurrentUser.CreateSubKey(@"SOFTWARE\RequiemNetwork");
            requiemKey.SetValue("464443", Encoding.ASCII.GetBytes(username), RegistryValueKind.Binary); // username
            requiemKey.SetValue("41483", encryptedPassword, RegistryValueKind.Binary); // password
            requiemKey.SetValue("947", IV, RegistryValueKind.Binary); // IV
        }

        public static void GetUserLoginInfo()
        {
            try
            {
                RegistryKey requiemKey = keyCurrentUser.OpenSubKey(@"SOFTWARE\RequiemNetwork");
                byte[] userName = (byte[])requiemKey.GetValue("464443");
                if (userName == null)
                {
                    Username = "";
                }
                else
                {
                    Username = Encoding.ASCII.GetString(userName);
                    EncryptedPassword = (byte[])requiemKey.GetValue("41483");
                    IV = (byte[])requiemKey.GetValue("947");
                }
            }
            catch (NullReferenceException)
            {
                RegistryKey requiemKey = keyCurrentUser.CreateSubKey(@"SOFTWARE\RequiemNetwork");
            }
            
        }

        public static void ClearUserLoginInfo()
        {
            LoginToken = "";
            RegistryKey requiemKey = keyCurrentUser.OpenSubKey(@"SOFTWARE\RequiemNetwork", true);
            string[] ValueNames = requiemKey.GetValueNames();
            if (ValueNames != null || ValueNames.Length != 0)
            {
                requiemKey.DeleteValue("464443");
                requiemKey.DeleteValue("41483");
                requiemKey.DeleteValue("947");
            }
        }
    }
}

using FileTransfer.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileTransfer.Core.Util
{
    public class StartUp
    {
        private static Logger _logger = new Logger(typeof(StartUp).Name);

        public static string[] GetIpAdresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                            || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.Address.ToString())
                .ToArray();
        }

        public static string GetSslCertificateHashBoundToPort(int portNo)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"http show sslcert ip=0.0.0.0:{portNo}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return Regex.Matches(output, @": *([0-9A-Fa-f]{10,})")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value)
                    .Select(sslCertificateHash => sslCertificateHash.ToUpper())
                    .FirstOrDefault();
            }
        }

        public static X509Certificate2 FindSslCertificateByThumbprint(string sslCertificateHash)
        {
            return FindSslCertificateBy(X509FindType.FindByThumbprint, sslCertificateHash);
        }

        private static X509Certificate2 FindSslCertificateBy(X509FindType findType, string keyValue)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates
                    .Find(findType, keyValue, false)
                    .Cast<X509Certificate2>()
                    .FirstOrDefault();
            }
        }

        public static string BindSslCertificateToPort(int portNo, string subjectNameOfRegisteredCertificate)
        {
            X509Certificate2 certificate = FindSslCertificateBy(X509FindType.FindBySubjectName, subjectNameOfRegisteredCertificate);
            if (certificate == null)
                throw new SslCertNotRegisteredException(subjectNameOfRegisteredCertificate);

            ProcessStartInfo info = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = "netsh",
                Arguments = $"http add sslcert ipport=0.0.0.0:{portNo} certhash={certificate.Thumbprint} appid={{{GetGUID()}}}",
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                process.WaitForExit();
                return process.ExitCode == 0 ? certificate.Thumbprint : null;
            }
        }

        private static string GetGUID()
        {
            return "00112233-4455-6677-8899-AABBCCDDEEFF";
        }
    }
}

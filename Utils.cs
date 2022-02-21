using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ViMaCrypt
{
    public class BadPaddingException : Exception 
    {
        public BadPaddingException(string message) : base(message)
        {
        }
    }

    internal class Utils
    {
        /* Regex used to remove trailing spaces from multiline text */
        private static Regex r = new Regex(@"^\s+", RegexOptions.Multiline);

        public static void xor(byte[] a, byte[] b) 
        {
            /* XOR beetween bytearrays */
            for (int i = 0; i < b.Length; i++)
                a[i] ^= b[i];
        }

        /* PKCS padding */
        public static byte[] pad(byte[] plaintext, int blocksize) 
        {
            int padLength = (plaintext.Length % blocksize);
            padLength = padLength == 0 ? blocksize : padLength;
            byte[] padding = new byte[padLength];
            for (int i = 0; i < padLength; i++) padding[i] = (byte) padLength;

            /* Add padding */
            byte[] paddedPlaintext = new byte[plaintext.Length + padding.Length];

            Buffer.BlockCopy(plaintext, 0, paddedPlaintext, 0, plaintext.Length);
            Buffer.BlockCopy(padding, 0, paddedPlaintext, plaintext.Length, padding.Length);

            return paddedPlaintext;
        }

        public static byte[] unpad(byte[] plaintext, int blocksize) 
        {
            int padLength = plaintext[plaintext.Length - 1];

            /* Check if padding is correct */
            if (padLength > blocksize) throw new BadPaddingException($"Bad PKCS padding: padLength={padLength}, blocksize={blocksize}");

            /* Remove padding */
            byte[] unpaddedPlaintext = new byte[plaintext.Length - padLength];
            Buffer.BlockCopy(plaintext, 0, unpaddedPlaintext, 0, unpaddedPlaintext.Length);

            return unpaddedPlaintext;
        }

        public static byte[] getHexInput(string msg)
        {
            byte[] input;

            while (true)
            {
                try
                {
                    Console.Error.Write(msg);
                    input = Convert.FromHexString(Console.ReadLine().Trim());
                }
                catch (FormatException e)
                {
                    Console.Error.WriteLine("Bad hex digits in input string\n");
                    continue;
                }

                break;
            }

            return input;
        }

        public static string dedent(string s)
        {
            return r.Replace(s, string.Empty);
        }
    }
}

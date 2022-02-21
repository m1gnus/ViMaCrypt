using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ViMaCrypt
{
    public class BadViMaCryptEncodingException: Exception 
    {
        public BadViMaCryptEncodingException(string message) : base(message)
        {
        }
    }

    internal class Crypto
    {
        private byte[] nonce;
        private AesGcm AES;
        private Random generator;

        /* 
         * Base constructor:
         * Input: key, nonce
         * key -> Key used to perform operations
         * nonce -> Nonce used to perform operations
         */
        public Crypto(byte[] key, byte[] nonce) 
        {
            this.nonce = nonce;
            this.AES = new AesGcm(key);
            this.generator = new Random();
        }

        /*
         * encrypt:
         * Input: plaintext, output
         * plaintext -> bytearray which represent the plaintext which will be encrypted
         * output -> where the result of the decryption operation will be saved (- for standard output)
         * Output: base64 string which represents the encoded results of the encryption operation
         */
        public string encrypt(byte[] plaintext, string output) 
        {
            /* Add a random nonce to avoid nonce reuse */
            byte[] randomNonce = new byte[12];
            this.generator.NextBytes(randomNonce);

            /* Pad plaintext */
            plaintext = Utils.pad(plaintext, 16);

            /* Declare variables to perform encryption */
            byte[] ciphertext = new byte[plaintext.Length], tag = new byte[16];
            byte[] nonce = new byte[randomNonce.Length];
            Buffer.BlockCopy(randomNonce, 0, nonce, 0, randomNonce.Length);
            Utils.xor(nonce, this.nonce);
            this.AES.Encrypt(nonce, plaintext, ciphertext, tag, null);

            /* Encode results using ViMaCrypt propietary protocol */
            byte[] result = Encoding.ASCII.GetBytes($"{output}|{BitConverter.ToString(randomNonce).Replace("-", string.Empty)}|{BitConverter.ToString(ciphertext).Replace("-", string.Empty)}|{BitConverter.ToString(tag).Replace("-", string.Empty)}");
            return Convert.ToBase64String(result);
        }

        /*
         * decrypt:
         * Input: ciphertext
         * ciphertext -> base64 encoded string which uses the ViMaCrypt proprietary protocol to encode informations
         * Output: decoded ciphertext if it has to be written to stdout, $"File decoded to {output} else"
         */
        public string decrypt(string encoded) 
        {
            string encodedString = Encoding.ASCII.GetString(Convert.FromBase64String(encoded));
            string[] splittedEncodedString = encodedString.Split('|');

            /* Check the correct number of arguments */
            if (splittedEncodedString.Length != 4) throw new BadViMaCryptEncodingException("Wrong number of '|' for a correct ViMaCrypt encoded string");

            /* Extract informations from the encoded string */
            string output = splittedEncodedString[0];
            byte[] nonce, ciphertext, tag;
            try 
            {
                nonce = Convert.FromHexString(splittedEncodedString[1]);
                ciphertext = Convert.FromHexString(splittedEncodedString[2]);
                tag = Convert.FromHexString(splittedEncodedString[3]);
            }
            catch (FormatException) 
            { /* Wrong format for hex numbers? */
                throw new BadViMaCryptEncodingException("Wrong hex string inside ViMaCrypt encoded string");
            }

            /* Decrypt ciphertext using the extracted informations */
            byte[] plaintext = new byte[ciphertext.Length];
            Utils.xor(nonce, this.nonce);
            this.AES.Decrypt(nonce, ciphertext, tag, plaintext, null);

            /* Unpad plaintext */
            plaintext = Utils.unpad(plaintext, 16);

            if (output == "-")
            {
                return Encoding.ASCII.GetString(plaintext);
            }
            else 
            {
                using (var f = new FileStream(output, FileMode.Create, FileAccess.Write))
                {
                    f.Write(plaintext, 0, plaintext.Length);
                    return $"PLAINTEXT SUCCESSFULLY WRITTEN TO {output}";
                }
            }
        }
    }
}

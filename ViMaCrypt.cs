using System.Text;

namespace ViMaCrypt
{
    class ViMaMain
    {
        private static Crypto? cipher;

        /* Fixed strings */
        private static string prompt = "";
        private static string banner = @"
         _________ _______  _______  _______  _______           _______ _________
|\     /|\__   __/(       )(  ___  )(  ____ \(  ____ )|\     /|(  ____ )\__   __/
| )   ( |   ) (   | () () || (   ) || (    \/| (    )|( \   / )| (    )|   ) (   
| |   | |   | |   | || || || (___) || |      | (____)| \ (_) / | (____)|   | |   
( (   ) )   | |   | |(_)| ||  ___  || |      |     __)  \   /  |  _____)   | |   
 \ \_/ /    | |   | |   | || (   ) || |      | (\ (      ) (   | (         | |   
  \   /  ___) (___| )   ( || )   ( || (____/\| ) \ \__   | |   | )         | |   
   \_/   \_______/|/     \||/     \|(_______/|/   \__/   \_/   |/          )_(   

v1.0.0-cupido

        ";
        private static string mainMenuString = Utils.dedent(@"
        1) Encrypt
        2) Decrypt
        3) Quit
        ");
        private static string encryptMenuString = Utils.dedent(@"
        1) Encrypt a message
        2) Encrypt a file
        3) Go back
        ");
        private static string decryptMenuString = Utils.dedent(@"
        1) Decrypt a message
        2) Decrypt a file
        3) Go back
        ");

        static string encryptMessage(bool fromStdin)
        {
            byte[] plaintext;
            string output;

            if (fromStdin)
            {
                output = "-";
                Console.Error.Write("\rplaintext> ");
                plaintext = Encoding.ASCII.GetBytes(Console.ReadLine().Trim());
            }
            else
            {
                do
                {
                    Console.Error.Write("\rfilename> ");
                    output = Console.ReadLine().Trim();
                } while (output == "-");
                plaintext = File.ReadAllBytes(output);
            }

            string ciphertext = cipher.encrypt(plaintext, Path.GetFileName(output));

            Console.WriteLine($"\n{ciphertext}");
            return ciphertext;
        }

        static string decryptMessage(bool fromStdin)
        {
            string ciphertext;
            string plaintext;

            if (fromStdin)
            {
                Console.Error.Write("\rciphertext> ");
                ciphertext = Console.ReadLine().Trim();
            }
            else
            {
                Console.Error.Write("\rfilename> ");
                ciphertext = File.ReadAllText(Console.ReadLine().Trim());
            }

            plaintext = cipher.decrypt(ciphertext);
            Console.WriteLine($"\n{plaintext}");

            return plaintext;
        }

        static void encryptMenu() {
            int choice = 0, choices = 3;

            while (choice <= 0 || choice > choices)
            {
                Console.Clear();
                Console.Error.WriteLine(banner);
                Console.Error.WriteLine(encryptMenuString);
                Console.Error.Write(prompt);

                choice = Console.ReadKey().KeyChar - '0';
            }

            switch (choice)
            {
                case 1:
                    encryptMessage(true);
                    break;
                case 2:
                    encryptMessage(false);
                    break;
                case 3:
                    break;
            }
        }

        static void decryptMenu()
        {
            int choice = 0, choices = 3;

            while (choice <= 0 || choice > choices)
            {
                Console.Clear();
                Console.Error.WriteLine(banner);
                Console.Error.WriteLine(decryptMenuString);
                Console.Error.Write(prompt);

                choice = Console.ReadKey().KeyChar - '0';
            }

            switch (choice)
            {
                case 1:
                    decryptMessage(true);
                    break;
                case 2:
                    decryptMessage(false);
                    break;
                case 3:
                    break;
            }
        }

        static void mainMenu() {
            int choice = 0, choices = 3;

            while (choice <= 0 || choice > choices)
            {
                Console.Clear();
                Console.Error.WriteLine(banner);
                Console.Error.WriteLine(mainMenuString);
                Console.Error.Write(prompt);

                choice = Console.ReadKey().KeyChar - '0';
            }

            switch (choice)
            {
                case 1:
                    encryptMenu();
                    break;
                case 2:
                    decryptMenu();
                    break;
                case 3:
                    Environment.Exit(0);
                    break;
            }
        }

        static void Main(string[] args) {
            /* Read key and nonce from standard input */
            byte[] key = Utils.getHexInput("Please insert your key: ");
            byte[] nonce = Utils.getHexInput("Please insert your nonce: ");

            /* Instance Crypto using the provided secrets */
            cipher = new Crypto(key, nonce);

            while (true)
            {
                try
                {
                    mainMenu();
                }
                catch (BadViMaCryptEncodingException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                catch (FormatException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                catch (BadPaddingException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                Console.Error.Write("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Text;

namespace DF2023.Core.Helpers
{
    public static class PasswordGenerator
    {
        private static readonly Random _random = new Random();
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*()_-+=<>?";

        public static string GenerateStrongPassword(int length)
        {
            if (length < 6)
            {
                throw new ArgumentException("Password length must be at least 8 characters.");
            }

            StringBuilder password = new StringBuilder();
            int[] charTypes = new int[] { 0, 1, 2, 3 }; // Represent Uppercase, Lowercase, Digits, Special characters

            // Ensure at least one character from each type is added
            password.Append(UppercaseLetters[_random.Next(UppercaseLetters.Length)]);
            password.Append(LowercaseLetters[_random.Next(LowercaseLetters.Length)]);
            password.Append(Digits[_random.Next(Digits.Length)]);
            password.Append(SpecialCharacters[_random.Next(SpecialCharacters.Length)]);

            // Fill the rest of the password length
            for (int i = 4; i < length; i++)
            {
                int charType = charTypes[_random.Next(charTypes.Length)];
                switch (charType)
                {
                    case 0:
                        password.Append(UppercaseLetters[_random.Next(UppercaseLetters.Length)]);
                        break;

                    case 1:
                        password.Append(LowercaseLetters[_random.Next(LowercaseLetters.Length)]);
                        break;

                    case 2:
                        password.Append(Digits[_random.Next(Digits.Length)]);
                        break;

                    case 3:
                        password.Append(SpecialCharacters[_random.Next(SpecialCharacters.Length)]);
                        break;
                }
            }

            // Shuffle the characters to ensure the password is random
            return Shuffle(password.ToString());
        }

        private static string Shuffle(string input)
        {
            char[] array = input.ToCharArray();
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                char temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return new string(array);
        }
    }
}
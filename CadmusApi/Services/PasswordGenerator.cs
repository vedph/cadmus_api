// http://stackoverflow.com/questions/2266721/generating-a-strong-password-in-c
// http://stackoverflow.com/questions/38632735/rngcryptoserviceprovider-in-net-core

using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CadmusApi.Services
{
    /// <summary>
    /// Password generator.
    /// </summary>
    public sealed class PasswordGenerator
    {
        /// <summary>
        /// Gets the minimum length.
        /// </summary>
        public int MinLength { get; }

        /// <summary>
        /// Gets the maximum length.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Gets the minimum lower case chars.
        /// </summary>
        public int MinLowerCaseChars { get; }

        /// <summary>
        /// Gets the minimum upper case chars.
        /// </summary>
        public int MinUpperCaseChars { get; }

        /// <summary>
        /// Gets the minimum numeric chars.
        /// </summary>
        public int MinNumericChars { get; }

        /// <summary>
        /// Gets the minimum special chars.
        /// </summary>
        public int MinSpecialChars { get; }

        /// <summary>
        /// Gets all the lowercase chars.
        /// </summary>
        public string AllLowerCaseChars { get; }

        /// <summary>
        /// Gets all the uppercase chars.
        /// </summary>
        public string AllUpperCaseChars { get; }

        /// <summary>
        /// Gets all the numeric chars.
        /// </summary>
        public string AllNumericChars { get; }

        /// <summary>
        /// Gets all the special chars.
        /// </summary>
        public string AllSpecialChars { get; }

        private readonly string _allAvailableChars;
        private readonly RandomSecure _randomSecure = new RandomSecure();
        private readonly int _minNumberOfChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordGenerator"/> class.
        /// </summary>
        public PasswordGenerator()
        {
            // Define characters that are valid and reject ambiguous characters 
            // such as ilo, IO and 1 or 0
            AllLowerCaseChars = GetCharRange('a', 'z', exclusiveChars: "ilo");
            AllUpperCaseChars = GetCharRange('A', 'Z', exclusiveChars: "IO");
            AllNumericChars = GetCharRange('2', '9');
            AllSpecialChars = "!@#%*()$?+-=";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordGenerator"/> class.
        /// </summary>
        /// <param name="minLength">The minimum length.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <param name="minLowerCaseChars">The minimum lower case chars.</param>
        /// <param name="minUpperCaseChars">The minimum upper case chars.</param>
        /// <param name="minNumericChars">The minimum numeric chars.</param>
        /// <param name="minSpecialChars">The minimum special chars.</param>
        /// <exception cref="ArgumentException">
        /// The minlength is smaller than 15. - minLength
        /// or
        /// The minLength is bigger than the maximum length. - minLength
        /// or
        /// The minLowerCase is smaller than 2. - minLowerCaseChars
        /// or
        /// The minUpperCase is smaller than 2. - minUpperCaseChars
        /// or
        /// The minNumeric is smaller than 2. - minNumericChars
        /// or
        /// The minSpecial is smaller than 2. - minSpecialChars
        /// or
        /// The min length of the password is smaller than the sum " +
        ///                     "of the min characters of all catagories. - maxLength
        /// </exception>
        public PasswordGenerator(
            int minLength = 15,
            int maxLength = 20,
            int minLowerCaseChars = 2,
            int minUpperCaseChars = 2,
            int minNumericChars = 2,
            int minSpecialChars = 2)
        {
            if (minLength < 15)
            {
                throw new ArgumentException("The minlength is smaller than 15.",
                    nameof(minLength));
            }

            if (minLength > maxLength)
            {
                throw new ArgumentException("The minLength is bigger than the maximum length.",
                    nameof(minLength));
            }

            if (minLowerCaseChars < 2)
            {
                throw new ArgumentException("The minLowerCase is smaller than 2.",
                    nameof(minLowerCaseChars));
            }

            if (minUpperCaseChars < 2)
            {
                throw new ArgumentException("The minUpperCase is smaller than 2.",
                    nameof(minUpperCaseChars));
            }

            if (minNumericChars < 2)
            {
                throw new ArgumentException("The minNumeric is smaller than 2.",
                    nameof(minNumericChars));
            }

            if (minSpecialChars < 2)
            {
                throw new ArgumentException("The minSpecial is smaller than 2.",
                    nameof(minSpecialChars));
            }

            _minNumberOfChars = minLowerCaseChars + minUpperCaseChars +
                                    minNumericChars + minSpecialChars;

            if (minLength < _minNumberOfChars)
            {
                throw new ArgumentException(
                    "The min length of the password is smaller than the sum " +
                    "of the min characters of all catagories.",
                    nameof(maxLength));
            }

            MinLength = minLength;
            MaxLength = maxLength;

            MinLowerCaseChars = minLowerCaseChars;
            MinUpperCaseChars = minUpperCaseChars;
            MinNumericChars = minNumericChars;
            MinSpecialChars = minSpecialChars;

            _allAvailableChars =
                OnlyIfOneCharIsRequired(minLowerCaseChars, AllLowerCaseChars) +
                OnlyIfOneCharIsRequired(minUpperCaseChars, AllUpperCaseChars) +
                OnlyIfOneCharIsRequired(minNumericChars, AllNumericChars) +
                OnlyIfOneCharIsRequired(minSpecialChars, AllSpecialChars);
        }

        private string OnlyIfOneCharIsRequired(int min, string allChars)
        {
            return min > 0 || _minNumberOfChars == 0 ? allChars : string.Empty;
        }

        private static string ShuffleTextSecure(string source)
        {
            char[] shuffeldChars = source.ShuffleSecure().ToArray();
            return new string(shuffeldChars);
        }

        /// <summary>
        /// Generates a password.
        /// </summary>
        /// <returns>password</returns>
        public string Generate()
        {
            int nPasswordLen = _randomSecure.Next(MinLength, MaxLength);

            // Get the required number of characters of each catagory and 
            // add random charactes of all catagories
            string minChars = GetRandomString(AllLowerCaseChars, MinLowerCaseChars) +
                            GetRandomString(AllUpperCaseChars, MinUpperCaseChars) +
                            GetRandomString(AllNumericChars, MinNumericChars) +
                            GetRandomString(AllSpecialChars, MinSpecialChars);
            string rest = GetRandomString(_allAvailableChars, nPasswordLen - minChars.Length);
            string unshuffled = minChars + rest;

            // Shuffle the result so the order of the characters are unpredictable
            return ShuffleTextSecure(unshuffled);
        }

        private string GetRandomString(string possibleChars, int lenght)
        {
            string result = string.Empty;
            for (int position = 0; position < lenght; position++)
            {
                int index = _randomSecure.Next(possibleChars.Length);
                result += possibleChars[index];
            }
            return result;
        }

        private static string GetCharRange(char min, char maximum, string exclusiveChars = "")
        {
            string result = string.Empty;
            for (char value = min; value <= maximum; value++)
            {
                result += value;
            }
            if (!string.IsNullOrEmpty(exclusiveChars))
            {
                char[] inclusiveChars = result.Except(exclusiveChars).ToArray();
                result = new string(inclusiveChars);
            }
            return result;
        }
    }

    internal static class Extensions
    {
        private static readonly Lazy<RandomSecure> RandomSecure =
            new Lazy<RandomSecure>(() => new RandomSecure());
        public static IEnumerable<T> ShuffleSecure<T>(this IEnumerable<T> source)
        {
            T[] sourceArray = source.ToArray();
            for (int counter = 0; counter < sourceArray.Length; counter++)
            {
                int randomIndex = RandomSecure.Value.Next(counter, sourceArray.Length);
                yield return sourceArray[randomIndex];

                sourceArray[randomIndex] = sourceArray[counter];
            }
        }
    }

    internal class RandomSecure
    {
        private readonly RandomNumberGenerator _rngProvider = RandomNumberGenerator.Create();

        public int Next()
        {
            byte[] randomBuffer = new byte[4];
            _rngProvider.GetBytes(randomBuffer);
            return BitConverter.ToInt32(randomBuffer, 0);
        }

        public int Next(int maximumValue)
        {
            // Do not use Next() % maximumValue because the distribution is not OK
            return Next(0, maximumValue);
        }

        public int Next(int minValue, int maximumValue)
        {
            int seed = Next();

            //  Generate uniformly distributed random integers within a given range.
            return new Random(seed).Next(minValue, maximumValue);
        }
    }
}

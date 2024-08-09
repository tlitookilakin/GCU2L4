using System.Diagnostics.CodeAnalysis;

namespace PigLatin;

class Program
{
	static void Main(string[] args)
	{
		// welcome
		Console.WriteLine("Welcome to the Igpay Atinlay Translator!");

		// main loop
		for (bool run = true; run; run = PromptYesNo(true, "Would you like to translate another sentence?"))
		{
			// prompt user
			Console.WriteLine("Please enter a sentence to translate");

			// wait until valid input
			string? output;
			while (!TryConvertSentence(GetInput(), out output))
				Console.WriteLine("Invalid input, please re-enter");

			// write result
			Console.WriteLine(output);
		}
	}

	static string? GetInput()
	{
		// get output
		var line = Console.ReadLine();
		if (line is null)
			return null;

		// clean it up
		return line.Trim();
	}

	static void TryConvert(Span<char> text, string original, ref int position, ref int cursor)
	{
		/*Runs once for each "word".
		* Searches the source string one letter at a time until
		* it reaches the end, whitespace, or punctuation characters.
		* Position defines the index to read from in the source string,
		* while cursor defines the index to write to in the target string.
		*/

		const string vowels = "AEIOUaeiou";

		int start = position;
		int initialCursor = cursor;
		int vowelIndex = -1;
		bool hasSymbol = false;

		// main character loop
		for (; position < original.Length; position++)
		{
			char letter = original[position];

			// blank space, stop and continue to next word
			if (char.IsWhiteSpace(letter) || IsPunctuation(letter))
				break;

			if (!hasSymbol) 
			{
				// symbol found
				if (!IsValidLetter(letter))
				{
					hasSymbol = true;

					// move cursor to start of word
					cursor = initialCursor;

					// go back and print as-is
					for (int i = start; i < position; i++)
						text[cursor++] = original[i];
				}

				// first vowel not yet found
				else if (vowelIndex < 0)
				{
					if (vowels.Contains(letter))
						vowelIndex = position; // set index
					else
						continue; // keep searching
				}

				// output letter copying case
				text[cursor++] = CopyCase(original[position - vowelIndex + start], letter);
			}
			else
			{
				// output letter as-is
				text[cursor++] = letter;
			}
		}

		// not empty and not containing symbols
		if (position != start && !hasSymbol)
		{
			// all consonants
			if (vowelIndex < 0)
			{
				for (int i = start; i < position; i++)
					text[cursor++] = original[i];
			}

			// copy front of word
			else
			{
				for (int i = start; i < vowelIndex; i++)
					text[cursor++] = CopyCase(original[cursor - initialCursor - 1], original[i]);
			}

			// suffix
			if (start == vowelIndex) // starts with a vowel
				text[cursor++] = 'w';
			text[cursor++] = 'a';
			text[cursor++] = 'y';
		}

		// if it was terminated by a character, add that character
		if (position < original.Length)
		{
			text[cursor++] = original[position];
			position++;
		}
	}

	static char CopyCase(char original, char letter)
	{
		// if the reference letter is uppercase, make the target letter uppercase, otherwise make it lowercase
		return char.IsUpper(original) ? char.ToUpper(letter) : char.ToLower(letter);
	}

	static bool TryConvertSentence(string? input, [NotNullWhen(true)] out string? converted)
	{
		/*This uses string.Create() to do the entire thing in a single loop all at once
		* It allows reading from and writing to a string after it is created but
		* before it is made immutable. 
		*/

		/*The length is set to 2x + 4 because at most,
		* a given word will be expanded from one letter to 4 letters (4x), but words
		* must be separated by punctuation or whitespace characters, so at most half
		* of the characters in a string can be pig latin words. 4 / 2 = 2. However with
		* odd numbers of characters there may be one at the end terminated by the string
		* length instead, so add 4 to account for that.
		*/

		// set default output value
		converted = null;

		// blank, can't be used
		if (input is null or "")
			return false;

		// setup initial state
		ConverterState state = new(input, 0);
		// fill string
		converted = string.Create(input.Length * 2 + 4, state, ConvertSentence);
		// trim to size
		converted = converted[..state.Position];

		return true;
	}

	static void ConvertSentence(Span<char> text, ConverterState state)
	{
		/*Position and Cursor are ref'd so that they carry over between words,
		* so the loop is finite, and so the ending position of the outputted text
		* can be recorded and re-used for trimming.
		*/

		int cursor = 0;

		// iterate words, convert each
		while (state.Position < state.Original.Length)
			TryConvert(text, state.Original, ref state.Position, ref cursor);

		state.Position = cursor;
	}

	static bool IsPunctuation(char letter)
	{
		const string punctuation = "\"/\\!?,.:;[](){}&";

		return punctuation.Contains(letter);
	}

	static bool IsValidLetter(char letter)
	{
		// specifically allow apostrophes and hyphens
		if (letter is '-' or '\'')
			return true;

		// anything else that isn't a letter is invalid
		return letter is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');
	}

	static bool PromptYesNo(bool allowEscape, string message)
	{
		Console.WriteLine(message + " [Y/N]");

		while (true)
		{
			// get keystroke
			char key = Console.ReadKey().KeyChar;

			// deletes echoed keystroke from output
			Console.Write("\b\\\b");

			// process keystroke
			switch (key)
			{
				// yes
				case 'y':
				case 'Y':
					return true;

				// no
				case 'n':
				case 'N':
					return false;

				// escape key
				case '\x1b':
					if (allowEscape)
						return false;
					break;
			}
		}
	}

	public class ConverterState(string original, int position)
	{
		public string Original = original;
		public int Position = position;
	}
}

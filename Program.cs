using System.Diagnostics.CodeAnalysis;

namespace PigLatin;

class Program
{
	// TODO adjust casing

	static void Main(string[] args)
	{
		Console.WriteLine("Welcome to the Igpay Atinlay Translator!");

		for (bool run = true; run; run = PromptYesNo(true, "Would you like to translate another sentence?"))
		{
			Console.WriteLine("Please enter a sentence to translate");

			string? output;
			while (!TryConvertSentence(GetInput(), out output))
				Console.WriteLine("Invalid input, please re-enter");

			Console.WriteLine(output);
		}
	}

	static string? GetInput()
	{
		var line = Console.ReadLine();
		if (line is null)
			return null;

		return line.Trim().ToLowerInvariant();
	}

	// end is exclusive, start is inclusive
	static void TryConvert(Span<char> text, string original, ref int position, ref int cursor)
	{
		const string vowels = "AEIOUaeiou";

		int start = position;
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
					if (vowelIndex >= 0)
						cursor -= position - start - 1;

					// go back and print as-is
					for (int i = start; i < position; i++)
						text[cursor++] = original[i];

					vowelIndex = -1;
				}

				// first vowel not yet found
				else if (vowelIndex < 0)
				{
					if (vowels.Contains(letter))
						vowelIndex = position; // set index
					else
						continue; // keep searching
				}
			}

			// output letter as is
			text[cursor++] = letter;
		}

		// not empty and not containing symbols
		if (position - start != 0 && !hasSymbol)
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
					text[cursor++] = original[i];
			}

			// suffix
			if (start == vowelIndex) // starts with a vowel
				text[cursor++] = 'w';
			text[cursor++] = 'a';
			text[cursor++] = 'y';
		}

		// add whitespace if necessary
		if (position < original.Length)
		{
			text[cursor++] = original[position];
			position++;
		}
	}

	static bool TryConvertSentence(string? input, [NotNullWhen(true)] out string? converted)
	{
		// set default output value
		converted = null;

		// blank, can't be used
		if (input is null or "")
			return false;

		// setup initial state
		ConverterState state = new(input, 0);
		// fill string
		converted = string.Create(input.Length * 4, state, ConvertSentence);
		// trim to size
		converted = converted[..state.Position];

		return true;
	}

	static void ConvertSentence(Span<char> text, ConverterState state)
	{
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

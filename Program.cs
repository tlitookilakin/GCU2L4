using System.Diagnostics.CodeAnalysis;

namespace PigLatin;

class Program
{
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

	static bool TryConvert(string input, [NotNullWhen(true)] out string? converted)
	{
		// no text
		if (input is "")
		{
			converted = "";
		}

		// vowel
		else if (input[0] is 'a' or 'e' or 'i' or 'o' or 'u')
		{
			converted = input + "way";
		}

		// consonant
		else
		{
			// get vowel index
			int position = input.IndexOfAny(['a', 'e', 'i', 'o', 'u']);
			converted = 
				position == -1 ? input + "ay" : // all consonants?
				input[position..] + input[..position] + "ay"; // vowel position found
		}

		return true;
	}

	static bool TryConvertSentence(string? input, [NotNullWhen(true)] out string? converted)
	{
		// set default output value
		converted = null;

		// blank, can't be used
		if (input is null or "")
			return false;

		string[] split = input.Split(' ');
		string[] words = new string[split.Length];

		for (int i = 0; i < split.Length; i++)
		{
			if (TryConvert(split[i], out string? output))
				words[i] = output;
			else
				return false;
		}

		converted = string.Join(' ', words);
		return true;
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
}

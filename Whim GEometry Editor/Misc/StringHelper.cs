using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
namespace MDLLibs.Classes.Misc;
internal static class StringHelper
{
    public static bool IsNumber(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }
        int intResult;
        if (int.TryParse(input, out intResult))
        {
            return true;
        }
        float floatResult;
        if (float.TryParse(input, out floatResult))
        {
            return true;
        }
        return false;
    }
    public static string GetFirstTextInsideQuotes(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }
        // Define the regular expression pattern to find text inside quotes
        string pattern = "\"([^\"]*)\"";
        Match match = Regex.Match(input, pattern);
        // If a match is found, return the first group (the text inside quotes)
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        // If no match is found, return null
        return null;
    }
    public static float[] ExtractFloats(string input)
    {
        List<float> floatList = new List<float>();
        // Regex to match floats including scientific notation (e.g., 1.23e-4)
        string pattern = @"[-+]?\d*\.?\d+(?:[eE][-+]?\d+)?";
        MatchCollection matches = Regex.Matches(input, pattern);
        foreach (Match match in matches)
        {
            if (float.TryParse(match.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                floatList.Add(result);
            }
        }
        return floatList.ToArray();
    }
    public static float[] ExtractDoublesANDx100(string input)
    {
        List<float> doublesList = new List<float>();
        // Regex to match doubles with optional leading minus sign
        string pattern = @"-?\d+(\.\d+)?";
        MatchCollection matches = Regex.Matches(input, pattern);
        foreach (Match match in matches)
        {
            if (float.TryParse(match.Value, out float result))
            {
                doublesList.Add(result * 100);
            }
        }
        return doublesList.ToArray();
    }
    public static List<string> ExtractStringsInsideCurlyBraces(string input)
    {
        // Define the regex pattern to match text inside curly braces
        string pattern = @"\{([^}]*)\}";
        // Create a regex object with the defined pattern
        Regex regex = new Regex(pattern);
        // Find matches in the input string
        MatchCollection matches = regex.Matches(input);
        // Initialize a list to hold the extracted strings
        List<string> results = new List<string>();
        // Iterate through the matches and add them to the list
        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }
        return results;
    }
    public static List<string> ExtractStringsInsideArrowBrackets(string input)
    {
        // Define the regex pattern to match text inside arrow brackets
        string pattern = @"<([^>]*)>";
        // Create a regex object with the defined pattern
        Regex regex = new Regex(pattern);
        // Find matches in the input string
        MatchCollection matches = regex.Matches(input);
        // Initialize a list to hold the extracted strings
        List<string> results = new List<string>();
        // Iterate through the matches and add them to the list
        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }
        return results;
    }
    public static string ExtractFirstArrowBrackets(string input)
    {
        // Define the regular expression pattern to match text inside <>
        string pattern = @"<([^>]*)>";
        // Create a regex object with the defined pattern
        Regex regex = new Regex(pattern);
        // Use the Match method to find the first match
        Match match = regex.Match(input);
        // If a match is found, return the captured group (text inside the brackets)
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        // If no match is found, return null or an appropriate message
        return null;
    }
    public static string MakeFourthCharLowercase(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 4)
        {
            return input; // Return the original string if it's null, empty, or too short
        }
        char[] charArray = input.ToCharArray();
        charArray[3] = char.ToLower(charArray[3]);
        return new string(charArray);
    }
    public static int ExtractIntFromSquareBrackets(string input)
    {
        int startIndex = input.IndexOf('[');
        int endIndex = input.IndexOf(']');
        if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
        {
            string numberString = input.Substring(startIndex + 1, endIndex - startIndex - 1);
            int number;
            if (int.TryParse(numberString, out number))
            {
                return number;
            }
        }
        return 0;
    }
  internal  static List<int> ExtractInts(string input)
    {
        List<int> integersList = new List<int>();
        // Define the regex pattern for integers
        string pattern = @"-?\d+";
        // Match the pattern in the input string
        MatchCollection matches = Regex.Matches(input, pattern);
        // Iterate through the matches and parse the integers
        foreach (Match match in matches)
        {
            // Parse the integer from the match and add it to the list
            integersList.Add(int.Parse(match.Value));
        }
        return integersList;
    }
    public static string ReplaceCharAtPosition(string originalString, int position, char newChar)
    {
        if (position < 0 || position >= originalString.Length)
        {
            // Handle invalid position
            return originalString; // Or handle the case as needed
        }
        char[] charArray = originalString.ToCharArray();
        charArray[position] = newChar;
        return new string(charArray);
    }
    public static void RemoveNumericIndicator(List<string> tokens)
    { if (tokens.Count < 2) { return; }
        if (IsInteger(tokens[1])) { tokens.RemoveAt(1); }
    }
   internal static bool IsInteger(string str)  {  return int.TryParse(str, out _); }
    public static string GetRemainingListElements(List<string> ls) {  ls.RemoveAt(0);  return string.Join("", ls);  }
    public static List<string> SplitByCommaAndIgnoreCurlyBraces(string data)
    {
        List<string> result = new List<string>();
        string tempString = string.Empty;
        int countCurlyBraces = 0;
        foreach (char c in data)
        {
            if (c == '}')
            {
                countCurlyBraces--;
                if (countCurlyBraces > 0) { tempString += c; }
                if (countCurlyBraces == 0)
                {
                    tempString += c;
                    result.Add(tempString.ToString()
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Replace("\r", ""));
                    tempString = string.Empty;
                }
                continue;
            }
            if (c == '{') { tempString += c; countCurlyBraces++; continue; }
            if (countCurlyBraces == 0)
            {
                if (c == ',')
                {
                    if (tempString == string.Empty) { continue; }
                    result.Add(
                        tempString.ToString()
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Replace("\r", ""));
                    tempString = string.Empty;
                    continue;
                }
            }
            tempString += c;
        }
        return
            result;
    }
    public static List<string> GetMatches(string input, string pattern)
    {
        List<string> matches = new List<string>();
        MatchCollection matchCollection = Regex.Matches(input, pattern);
        foreach (Match match in matchCollection)
        {
            matches.Add(match.Value);
        }
        return matches;
    }
    public static string RemoveCurlyBracesStartEnd(string input)
    {
        string content = input;
       if (input.StartsWith("{")) content = content.Substring(1);
            if (input.EndsWith("}")) content = content.Substring(0, content.Length - 1);
        return content;
    }
    internal static bool ValidName(string input)
    {
        // Regular expression pattern to match allowed characters
        string pattern = @"^[a-zA-Z0-9\-_\s]+$";
        // Check if the input matches the pattern
        return Regex.IsMatch(input, pattern);
    }
   internal static string RemoveInvalidCharacters(string input)
    {
        // Regular expression pattern to match invalid characters
        string pattern = @"[^a-zA-Z0-9_\- ]";
        // Replace invalid characters with an empty string
        string result = Regex.Replace(input, pattern, ""); ;
       if (result.Length == 0) { result = "Unnamed"; }
       return result;
    }
    internal static string ExtractFirstStringInQuotes(string input)
    {
        // Define the regular expression pattern to match strings inside double quotes
        string pattern = "\"([^\"]*)\"";
        // Use Regex.Match to find the first occurrence of the pattern
        Match match = Regex.Match(input, pattern);
        // If a match is found, return the captured group without the quotes
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        // Return an empty string if no match is found
        return string.Empty;
    }
    internal static List<string> ExtractStringsInCurlyBraces(string input)
    {
        List<string> result = new List<string>();
        // Define the regular expression pattern to match strings inside curly braces
        string pattern = @"\{(.*?)\}";
        // Match the pattern in the input string
        MatchCollection matches = Regex.Matches(input, pattern);
        // Iterate through the matches and add the captured groups to the result list
        foreach (Match match in matches)
        {
            // The captured value is in the first group (index 1) of the match
            string extractedString = match.Groups[1].Value;
            result.Add(extractedString);
        }
        return result;
    }
    public static List<string> ProcessMDLObjectProperties(string input)
    {
        // Reduce all whitespaces to one between words
        string cleanedInput = Regex.Replace(input, @"\s+", " ");
        // Split the string by whitespace
        string[] words = cleanedInput.Split(' ');
        // Remove the second element if it's a digit
        if (words.Length > 2 && char.IsDigit(words[1][0]))
        {
            List<string> temp = new List<string>(words);
            temp.RemoveAt(1);
            words = temp.ToArray();
        }
        // Join all elements from the second element to the last
        string joinedString = string.Join(" ", words, 1, words.Length - 1);
        // Remove curly braces from the first and last characters of each string
        List<string> result = new List<string>();
        result.Add(RemoveCurlyBraces(words[0]));
        result.Add(RemoveCurlyBraces(joinedString));
        return result;
    }
    static string RemoveCurlyBraces(string input)
    {
        if (input.Length > 1 && input[0] == '{' && input[input.Length - 1] == '}')
        {
            return input.Substring(1, input.Length - 2);
        }
        return input;
    }
    internal static string GetFilenameFromPath(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        string[] parts = input.Split('\\');
        return parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
    }
    internal static bool ValidTextureString(string input)
    {
        if (input.Trim().Length == 0) { return false; }
        if (input.ToLower().Contains("rpid"))  { return false; }
        string extension = ".blp";
        int minLengthBeforeExtension = 1;
        if (input.Length < minLengthBeforeExtension + extension.Length)
        {
            return false;
        }
        return input.EndsWith(extension, StringComparison.OrdinalIgnoreCase) &&
               input.Length >= minLengthBeforeExtension + extension.Length;
    }
    public static string RemoveQuotedSubstrings(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }
        // Define the regex pattern to match quoted substrings (including quotes).
        string pattern = "\"[^\"]*\"";
        // Replace quoted substrings with an empty string.
        string result = Regex.Replace(input, pattern, string.Empty);
        // Trim the result and convert it to lowercase.
        result = result.Trim().ToLower();
        return result;
    }

    internal static string CapitalizeName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }
}

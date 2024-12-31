using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace test_parser_mdl
{
    public enum TokenType
    {

        String,
        OpeningBrace,
        ClosingBrace,
        Comma,
        Number,
        Undefined,
        Keyword,
        Colon
    }

    public class Token
    {
        public TokenType Type = TokenType.Undefined;
        public string Value = "";
        public int LineNumber = 0; // for debugging
        public Token() { }
        public Token(TokenType type, string value) { Type = type; Value = value; }
        public Token(TokenType type, string value, int line) { Type = type; Value = value; LineNumber = line; }
        internal Token Clone()
        {
            return new Token(Type, Value, LineNumber);

        }
        public override string ToString()
        {
            return $"({Type.ToString()}) <{Value}>";
        }
        internal bool IsEmpty()
        {

            if (Type == TokenType.Undefined) { return true; } // undefined is always not valid
            if (Type == TokenType.String) { return false; } // string can be empty

            return Value == ""; // in any otehr case not having a value is invalid token
        }
    }
    public static partial class Parser_MDL
    {
        enum BuildMode
        {
            NoBuild,
            String,
            Number,
            Keyword
        }

        public static List<Token> Tokenize(string filepath)
        {
            //--------------------------------------------
            List<Token> Tokens = new List<Token>();
            Token CurrentlyBuiltToken = new Token();
            BuildMode BuildMode = BuildMode.NoBuild;  // Start with no building
           
            //------------------------------------------------
            StringBuilder buffer = new StringBuilder();  // Buffer to accumulate characters
            HashSet<char> ValidCharacters = new HashSet<char>("0123456789:-{}., \t\nEea" +
                "bcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\"\\+");
            //--------------------------------------------
            //--Read
            //--------------------------------------------

            string[] lines = File.ReadAllLines(filepath);
            //--------------------------------------------
            // Add last assembled token
            //--------------------------------------------
            void FinalizeCurrent(int lineNumber)
            {

                CurrentlyBuiltToken.LineNumber = lineNumber;  // Set the line number for debugging
                CurrentlyBuiltToken.Value = buffer.ToString();  // Set the value of the token




                if (TokenEmpty() == false)
                {
                    Tokens.Add(CurrentlyBuiltToken.Clone());
                }
                CurrentlyBuiltToken = new Token();
                buffer.Clear();  // Clear the buffer when adding a token
            }
            bool TokenEmpty()
            {
                 
                if (CurrentlyBuiltToken.Type == TokenType.Undefined) { return true; } // undefined is alwasy invalid
                if (CurrentlyBuiltToken.Type == TokenType.String) { return false; } // tring can be empty
                return CurrentlyBuiltToken.Value == ""; // if not a string and no string then empty
            }
            void AddOpeningCurlyBrace(int line) { FinalizeCurrent(line); Tokens.Add(new Token(TokenType.OpeningBrace, "{", line)); BuildMode = BuildMode.NoBuild;  }
            void AddClosingCurlyBrace(int line) { FinalizeCurrent(line); Tokens.Add(new Token(TokenType.ClosingBrace, "}", line)); BuildMode = BuildMode.NoBuild; }
            void AddComma(int line) { FinalizeCurrent(line); Tokens.Add(new Token(TokenType.Comma, ",", line)); BuildMode = BuildMode.NoBuild; }
            void AddColon(int line) { FinalizeCurrent(line); Tokens.Add(new Token(TokenType.Colon, ":", line)); BuildMode = BuildMode.NoBuild; }

            //--------------------------------------------
            //--Begin
            //--------------------------------------------
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.StartsWith("//")) { continue; }
                BuildMode = BuildMode.NoBuild;

                for (int i = 0; i < line.Length; i++)
                {
                    char chr = line[i];
                    //----------------------------------------------------
                    // on comment break
                    //-----------------------------------------
                    if (i + 1 < line.Length && chr == '/' && line[i + 1] == '/') break;
                    //----------------------------------------------------
                    // Check for invalid characters
                    //-----------------------------------------

                    if (!ValidCharacters.Contains(chr) && BuildMode != BuildMode.String)
                    {
                        MessageBox.Show($"At tokenizer:\n Invalid character '{chr}' encountered at line {lineIndex + 1}.", "Parsing error");
                        ParsingFailed = true;
                        break;
                    }

                    //-----------------------------------------
                    // Handle strings
                    //-----------------------------------------

                    if (chr == '"')
                    {
                        if (BuildMode == BuildMode.String)
                        {
                            FinalizeCurrent(lineIndex + 1);  // End string token
                            BuildMode = BuildMode.NoBuild;
                        }
                        else
                        {
                            FinalizeCurrent(lineIndex + 1);  // Start new string token
                            CurrentlyBuiltToken.Type = TokenType.String;
                            BuildMode = BuildMode.String;
                        }
                        continue;
                    }
                    if (BuildMode == BuildMode.String)
                    {
                        buffer.Append(chr);  // Continue building string
                        continue;
                    }

                    //-----------------------------------------
                    // Handle whitespace and tabs
                    //-----------------------------------------
                    if (chr == ' ' || chr == '\t')
                    {
                        FinalizeCurrent(lineIndex + 1);
                        BuildMode = BuildMode.NoBuild;
                        continue;  // Skip to the next iteration
                    }

                    //-----------------------------------------
                    // Handle special characters
                    //-----------------------------------------
                    if (chr == '{') AddOpeningCurlyBrace(lineIndex + 1);
                    else if (chr == '}') AddClosingCurlyBrace(lineIndex + 1);
                    else if (chr == ',') AddComma(lineIndex + 1);
                    else if (chr == ':') AddColon(lineIndex + 1);
                    //-----------------------------------------
                    // Handle numbers
                    //-----------------------------------------

                    // Start building a new number token
                    if (char.IsDigit(chr) && BuildMode == BuildMode.NoBuild)
                    {
                        
                            FinalizeCurrent(lineIndex + 1);  // Finalize any previously built token
                            BuildMode = BuildMode.Number;
                            CurrentlyBuiltToken.Type = TokenType.Number;
                            buffer.Append(chr);  // Start the number
                            continue;
 
                      
                    }

                    // - starting of a negative number
                    if (chr == '-' && BuildMode == BuildMode.NoBuild)
                    {


                        FinalizeCurrent(lineIndex + 1);
                        BuildMode = BuildMode.Number;
                            CurrentlyBuiltToken.Type = TokenType.Number;
                            buffer.Append(chr);
                            continue;
                       

                    }
 
                  

                    if (BuildMode == BuildMode.Number)
                    {
                        // digit or exponential
                        if (char.IsDigit(chr) || chr == '.' || chr == 'e' || chr == 'E' || chr== '-' || chr == '+')
                        {
                            buffer.Append(chr);
                            continue;
                        }
                        
                        
                    }

                    //-----------------------------------------
                    // Handle keywords
                    //-----------------------------------------

                    if (BuildMode == BuildMode.Keyword && char.IsDigit(chr))
                    {
                        buffer.Append(chr);  // Continue building a keyword if a digit is encountered
                        continue;



                    }
                    if (char.IsLetter(chr))
                    {
                        if (BuildMode == BuildMode.NoBuild)
                        {
                            FinalizeCurrent(lineIndex + 1);  // Start new keyword token
                            BuildMode = BuildMode.Keyword;
                            CurrentlyBuiltToken.Type = TokenType.Keyword;
                            buffer.Append(chr);
                            continue;
                        }
                        else if (BuildMode == BuildMode.Keyword)
                        {
                            buffer.Append(chr);  // Append letter to keyword
                            continue;
                        }
                        else
                        {
                            MessageBox.Show($"Tokenizer: Unexpected letter or digit at line {lineIndex}. got '{chr}'"); ParsingFailed = true; break;
                        }
                        continue;
                    }





                }

                FinalizeCurrent(lineIndex + 1); // after each line finalize
                BuildMode = BuildMode.NoBuild;
                if (ParsingFailed) { break; }
            }

            return Tokens;
        }

    }


    //------------------------------------------------------------
}

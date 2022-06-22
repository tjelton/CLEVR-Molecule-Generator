using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextProcessing
{

    /*
     * Processes the current line, stopping when a semicolon or curly brace is hit.
     * Additionally adjusted the streamreader pointer accordingly. 
     * Paramaters:
     *      StreamReader sr : the stream reader which is reading the file we are interpreting.
     *      ref int lineNum : The number of the current line being processed.
     * Returns: string
     *      The 
     */
    public static string processLine(StreamReader sr, ref int lineNum)
    {

        char currentChar;
        string run = "";

        // Variable to keep track of whehter the first / in a comment was initiated
        bool previousCommentSlash = false;

        // Keeps reading until EOF.
        while (sr.Peek() >= 0)
        {

            currentChar = (char)sr.Read();

            // Ignore comments: 
            //      Single line comments are denoted by //
            //      Multi-line comments are wrappend in /* and */
            if (!previousCommentSlash && currentChar == '/')
            {
                previousCommentSlash = true;
                continue;
            }
            if (previousCommentSlash)
            {
                // Single line comment.
                if (currentChar == '/')
                {
                    previousCommentSlash = false; // To restart process.
                    while (sr.Peek() > 0)
                    {
                        currentChar = (char)sr.Read();

                        // If new line character is reached, add 1 to the line number tally.
                        if (currentChar == '\n')
                        {
                            lineNum++;
                            break; // Exit the loop because \n signals the end of the comment.
                        }
                    }
                    continue;
                }
                // Multi line comment.
                else if (currentChar == '*')
                {
                    previousCommentSlash = false; // To restart process.
                    while (sr.Peek() > 0)
                    {
                        currentChar = (char)sr.Read();
                        // If new line character is reached, add 1 to the line number tally.
                        if (currentChar == '\n')
                        {
                            lineNum++;
                        }
                        // If a * is read, check if next character is a \ to signal the end of the multi-line comment
                        if (currentChar == '*')
                        {
                            currentChar = (char)sr.Read();
                            if (currentChar == '\n') // We still have to check if a new line character is read.
                            {
                                lineNum++;
                            }
                            else if (currentChar == '/')
                            {
                                break;
                            }
                        }
                    }
                    continue;
                }
                else
                {
                    // Error as a single '\' is an unrecognised symbol.
                    Debug.LogError("ERROR on line " + lineNum + ": a single '\\' is an unrecognised symbol");
                    return "-1"; // Error status.
                }
            }

            // Add one to the line num tally
            if (currentChar == '\n')
            {
                lineNum++;
            }

            // If a semicolon is hit, this signals the end of a run (i.e. line),
            // and so return this line to the calling function for processing.
            // Note: we are returning the string here without a semicolon (i.e. the semicolon char hasn't been added to the run).
            if (currentChar == ';')
            {
                return run;
            }

            // Add non-whitespace characters to the current string run.
            if (!char.IsWhiteSpace(currentChar))
            {
                run = run + currentChar;
            }

            // If a curly brace is hit, then return the string.
            if (currentChar == '}' || currentChar == '{')
            {
                return run;
            }


        }

        Debug.LogError("ERROR on line " + lineNum + ": syntax error. This error may be a result of a missing semicolon");
        return "-2"; // Error status.

    }

    /*
     * Splits the run by commas, ensuring that commas within brackets are preserved.
     * This has to be used as opposed to the regular Split method.
     * 
     *      E.g. Let's say that we have "0, O, 0, 0, 0, aes(radius = 10, colour = red)" as the current run.
     *           If we use the inbuilt Split method, then the splitting would split into the following array:
     *              {"0", "O", "0", "0", "0", "aes(radius = 10", "colour = red)"}
     *           But we don't want the aes argument to be split on the comma yet. I.e., we want:
     *              {"0", "O", "0", "0", "0", "aes(radius = 10, colour = red)"}
     *              
     * Parameters:
     *      List<string> argumentsList : the list to store the arguments which have been split.
     *      string run : the string which is being split/processed.
     * Returns: int
     *      Negative values signal error status.
     *      Returns 0 on successful completion.
     */
    public static int splitPreservingBrackets(List<string> argumentsList, string run)
    {
        string splitting = "";
        bool bracketReached = false;
        foreach (char c in run)
        {
            // Test to see if a bracket has been input, so commas within are preserved.
            if (c == '(')
            {
                if (bracketReached)
                {
                    return -12; // Return error. This is because the a "(" cannot occur more than once.
                }
                bracketReached = true;
                splitting = splitting + c;
                continue;
            }
            else if (c == ')')
            {
                bracketReached = false;
                splitting = splitting + c;
                argumentsList.Add(splitting);
                splitting = "";
                continue;
            }

            // This overides the usual nature of splitting by commas, so commas within brackets are ignored.
            if (bracketReached)
            {
                splitting = splitting + c;
                continue;
            }
            // If a bracket has not been seen, continue spltting as usual.
            // i.e. split in the same way as the Split method.
            if (c == ',')
            {
                argumentsList.Add(splitting);
                splitting = "";
            }
            else
            {
                splitting = splitting + c;
            }
        }

        // Add the final argument to the array list (i.e. the argument that comes after the last comma).
        if (!splitting.Equals(""))
        {
            argumentsList.Add(splitting);
        }

        // If the code reaches here, the function has successfully run.
        return 0;
    }

    /*
     * Maps an english representation of a colour to it's hex value, otherwise, checks
     * whether the hex reprsentation is valid.
     * Parameters:
     *      string colour : the colour being converted or checked.
     * Returns: string
     *      Returns the hex representation on success.
     *      Returns null on failure.
     */
    public static string mapHexColour(string colour)
    {
        colour = colour.ToLower();

        // Hex to match a valid hex regex.
        Regex hexColourRgx = new Regex(@"#[0-9a-f]{6}$");

        // If colour values were passed in english, update to hex value.
        if (colour.Equals("white"))
        {
            return "#ffffff";
        }
        else if (colour.Equals("black"))
        {
            return "#222222";
        }
        else if (colour.Equals("blue"))
        {
            return "#1b43f5";
        }
        else if (colour.Equals("red"))
        {
            return "#eb3c25";
        }
        else if (colour.Equals("green"))
        {
            return "#70ea4e";
        }
        else if (colour.Equals("darkRed"))
        {
            return "#8e2c13";
        }
        else if (colour.Equals("darkViolet"))
        {
            return "#5c22b4";
        }
        else if (colour.Equals("cyan"))
        {
            return "#73fbfd";
        }
        else if (colour.Equals("orange"))
        {
            return "#f29b38";
        }
        else if (colour.Equals("yellow"))
        {
            return "#fce454";
        }
        else if (colour.Equals("beige"))
        {
            return "#f4ac80";
        }
        else if (colour.Equals("violet"))
        {
            return "#6b2ff5";
        }
        else if (colour.Equals("darkGreen"))
        {
            return "#33741f";
        }
        else if (colour.Equals("grey"))
        {
            return "#999999";
        }
        else if (colour.Equals("darkOrange"))
        {
            return "#d17b2c";
        }
        else if (colour.Equals("pink"))
        {
            return "#d081f8";
        }

        // If the english colour name was not recognised, see if a hex value was actually passed in.
        else
        {
            if (hexColourRgx.IsMatch(colour))
            {
                return colour;
            }
        }

        // If nothing has been return to this point, the string is invalid, and so return null.
        return null;
    }
}
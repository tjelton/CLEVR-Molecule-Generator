using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Bond
{

    private Element elementFrom;
    private Element elementTo;
    private int bondDegree;
    private float alpha; // To control transparency of the bond.
    private string colour;


    // Constructor
    public Bond(Element elementFrom, Element elementTo, int bondDegree, float alpha, string colour)
    {
        this.elementFrom = elementFrom;
        this.elementTo = elementTo;
        this.bondDegree = bondDegree;
        this.alpha = alpha;
        this.colour = colour;
    }

    ///// SETTERS /////

    public void setElementFrom(Element elementFrom)
    {
        this.elementFrom = elementFrom;
    }

    public void setElementTo(Element elementTo)
    {
        this.elementTo = elementTo;
    }

    public void setBondDegree(int bondDegree)
    {
        this.bondDegree = bondDegree;
    }

    public void setAlpha(float alpha)
    {
        this.alpha = alpha;
    }

    public void setColour(string colour)
    {
        this.colour = colour;
    }

    ///// GETTERS /////

    public Element getElementFrom()
    {
        return elementFrom;
    }

    public Element getElementTo()
    {
        return elementTo;
    }

    public int getBondDegree()
    {
        return bondDegree;
    }

    public float getAlpha()
    {
        return alpha;
    }

    public string getColour()
    {
        return colour;
    }

    ///// STATIC METHODS /////

    /*
     * Processes the current run in the BONDS block.
     * A run is defined as all content before a semicolon.
     * This function also the validity of the run, including checking that the right number of arguments have 
     * been passed in, and that the values are specifed correctly, in the correct order.
     * Paramaters:
     *      List<Bond> bonds : the list of Bond objects.
     *      Element[] elements : the array of Element objects.
     *      int lineNum : the number of the line being processed.
     *      string run : the run that is currently being processed.
     * Returns: int
     *      Negative integers signal error state.
     *      0 indicates successful completion.
     */
    public static int ProcessBondsRun(List<Bond> bonds, Element[] elements, int lineNum, string run)
    {

        // Split the run by commas, ensuring that commas within brackets are preserved.
        List<string> argumentsList = new List<string>();
        int returnVal = TextProcessing.splitPreservingBrackets(argumentsList, run);
        if (returnVal < 0)
        {
            return -1;
        }
        string[] arguments = argumentsList.ToArray(); // Change List to array for performance.

        // Check that there is at least 1 argument, and at most 2.
        if (!(arguments.Length == 1 || arguments.Length == 2))
        {
            return -2;
        }

        // The following few code chunks checks that the 1 argument is syntactically and logically correct.

        // For it to be correct:
        //          (1) There should be an integer, seperated by one of {"-", "=", "#", "single", "double", "triple"},
        //              followed by another integer (e.g. "1 = 2").
        //          (2) The two integers cannot be the same.
        //          (3) The index must refer to an element which exists.
        //          (4) A bond between the two elements must not already exist.

        // Check condition (1)
        string arg1 = arguments[0].Trim().ToLower();
        Regex arg1CorrectFormatRgx = new Regex(@"^[0-9]+(\s)*(-|=|#|single|double|triple)(\s)*[0-9]+$"); // Regex to check arg 1 syntax.
        // Error: Not syntactially correct.
        if (!arg1CorrectFormatRgx.IsMatch(arg1))
        {
            return -3;
        }

        // Now to extract the sub values within arg 1.
        //      e.g. if we have "0#1", we want to split into {"0", "#", "1"}.
        string[] arg1SubValues = { "", "", "" };
        int i = 0;
        Regex checkValidNum = new Regex(@"[0-9]");
        foreach (char c in arg1)
        {
            string cString = char.ToString(c);
            if (!checkValidNum.IsMatch(cString) && (i == 0))
            {
                i += 1;
            } else if (checkValidNum.IsMatch(cString) && (i == 1))
            {
                i += 1;
            }
            arg1SubValues[i] = arg1SubValues[i] + cString;
        }

        // Convert element indexes into integers.
        int indexFrom = Int32.Parse(arg1SubValues[0]);
        int indexTo = Int32.Parse(arg1SubValues[2]);

        // Assign bond degree.
        int bondDegree = 1;
        if (arg1SubValues[1].Equals("=") || arg1SubValues[1].Equals("double"))
        {
            bondDegree = 2;
        } else if (arg1SubValues[1].Equals("#") || arg1SubValues[1].Equals("triple"))
        {
            bondDegree = 3;
        }

        // Check condition (2)
        if (indexFrom == indexTo)
        {
            return -4;
        }

        // Check condition (3)
        Element elementFrom = Element.getElementBasedOnIndex(elements, indexFrom);
        Element elementTo = Element.getElementBasedOnIndex(elements, indexTo);
        if (elementFrom == null || elementTo == null)
        {
            return -5;
        }

        // Check condition (4)
        if (!checkBondDoesNotExist(bonds, elementFrom, elementTo))
        {
            return -6;
        }

        // At this point in code execution, we are happy that we can create a valid bond between the elements specified.

        // Now to process the optional aesthetic:
        float bondAlpha = 1;
        string bondColour = "#222222";
        returnVal = processAesthetics(arguments, ref bondColour, ref bondAlpha);
        if (0 != returnVal)
        {
            return returnVal;
        }
        
        // Create the bond object, and add to the list.
        Bond newBond = new Bond(elementFrom, elementTo, bondDegree, bondAlpha, bondColour);
        bonds.Add(newBond);

        // Successful method execution.
        return 0;
    }

    /*
     * Ensure that the keywords in the optional aesthetics block are syntactically valid,
     * and if they are valid, extract the value ensuring the type is valid.
     * Parameters:
     *      string[] arguments : an array of the arguments being processed.
     *      ref string colour : reference to the colour attribute from the calling code.
     *      ref float radius : reference to the radius attribute from the calling code.
     * Returns: int
     *      Negative values signal error state.
     *      0 signals successful processing.   
     */
    private static int processAesthetics(string[] arguments, ref string bondColour, ref float bondAlpha)
    {

        bool colourUpdated = false;
        bool alphaUpdated = false;

        Regex colourRgx = new Regex(@"^colour(\s)*=");
        Regex alphaRgx = new Regex(@"^alpha(\s)*=");

        // Regex to tell if a string is a floating point.
        Regex floatingPointRgx = new Regex(@"^[0-9]+(\.([0-9])+)?$");

        if (arguments.Length == 2) // This only runs if 2 arguments were passed in, and hence an optional aesthetic exists.
        {
            string optionalAesthetics = arguments[1].Trim().ToLower();

            // Check the optional aesthetic is in the correct syntax, and if so, do some simple text processing.
            Regex optionalAestheticSyntaxRgx = new Regex(@"^aes\((([a-zA-Z0-9])|(\s)|(,)|(=)|(#)|(\.))*\)$");
            if (optionalAestheticSyntaxRgx.IsMatch(optionalAesthetics))
            {
                // Strip the aes and brackets from the string.
                optionalAesthetics = optionalAesthetics.Replace("aes(", "");
                optionalAesthetics = optionalAesthetics.Replace(")", "");

                // Extract the attributes within the aesthetics block.
                string[] aestheticComponents = optionalAesthetics.Split(',');

                for (int j = 0; j < aestheticComponents.Length; j++)
                {
                    string temp = aestheticComponents[j].ToLower().Trim();

                    // Colour attribute
                    if (colourRgx.IsMatch(temp))
                    {
                        if (!colourUpdated)
                        {
                            colourUpdated = true;
                            temp = temp.Replace("colour", "").Replace("=", "").Trim();
                            temp = TextProcessing.mapHexColour(temp);
                            // Error: Invalid colour passed.
                            if (String.IsNullOrEmpty(temp))
                            {
                                return -7;
                            }
                            bondColour = temp;
                        }
                        else
                        {
                            return -9; // Error: Optional argument provided more than once.
                        }
                        continue;
                    }

                    // Alpha attribute.
                    if (alphaRgx.IsMatch(temp))
                    {
                        if (!alphaUpdated)
                        {
                            alphaUpdated = true;
                            temp = temp.Replace("alpha", "").Replace("=", "").Trim();
                            // Check radius is a valid floating point number.
                            if (floatingPointRgx.IsMatch(temp))
                            {
                                bondAlpha = float.Parse(temp); // Convert string to float.
                            }
                            // Error: Decimal expected.
                            else
                            {
                                return -8;
                            }
                        }
                        // Error: Optional attribute provided more than once.
                        else
                        {
                            return -9;

                        }
                        continue;
                    }

                    // Error: Optional attribute not recognised.
                    else
                    {
                        return -10;
                    }
                }

            }
            // Error: Optional aesthetic not in the correct syntax.
            else
            {
                return -11;
            }

        }
        return 0;
    }

    /*
     * Checks whether there already exists a bonding pair between these two elements.
     * Parameters:
     *      List<Bond> bonds : a list storing the Bond objects which have been created.
     *      Element elementFrom : the element that the bond starts at.
     *      Element elementTo : the element that the bond ends at.
     * Returns: bool
     *      Return true if a bond was not found.
     *      Return false if a bond between the elements does exist.
     */
    public static bool checkBondDoesNotExist(List<Bond> bonds, Element elementFrom, Element elementTo)
    {

        int elementFromIndex = elementFrom.getIndex();
        int elementToIndex = elementTo.getIndex();

        // Loop through each bond.
        foreach (Bond bond in bonds)
        {
            // Get the indexes of the element that make up the current bond.
            int currentBondIndex1 = bond.getElementFrom().getIndex();
            int currentBondIndex2 = bond.getElementTo().getIndex();

            // Check that a bond does not already exist with these indexes.
            if ((currentBondIndex1 == elementFromIndex && currentBondIndex2 == elementToIndex) ||
                    (currentBondIndex1 == elementToIndex && currentBondIndex2 == elementFromIndex))
            {
                return false; // i.e. a bond does already exist between the two elements passed.
            }
        }

        // If execution reaches here, a bond was not found.
        return true;
    }

    /* 
     * Output the relevant error message when given a negative errorIndex.
     * Parameters:
     *      int errorIndex : a negtaive integer representing the error message
     *                         to display.
     *      int lineNum : the line number where the error was raised.
     * Returns: void
     */
    public static void bondErrorMessage(int errorIndex, int lineNum)
    {

        // Mismatched bracket.
        if (errorIndex == -1)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Mis-matched brackets, or too many brackets provided.");
        }

        // Error where not enough arguments provided.
        else if (errorIndex == -2)
        {
            Debug.LogError("ERROR on line " + lineNum + ": 1 argument (+1 optional argument) were expected.");
        }

        // Incorrect syntax.
        else if (errorIndex == -3)
        {
            Debug.LogError("ERROR on line " + lineNum + ": bonding syntax incorrect. Must have an integer, seperated by one of " +
                "{\"-\", \"=\", \"#\", \"single\", \"double\", \"triple\"}, followed by another integer.");
        }

        // Indexes are the same.
        else if (errorIndex == -4)
        {
            Debug.LogError("ERROR on line " + lineNum + ": the indices are the same.");
        }

        // Elements represented by the index does not exist.
        else if (errorIndex == -5)
        {
            Debug.LogError("ERROR on line " + lineNum + ": one (or both) of the elements represent by the index does not exist.");
        }

        // Bond already exists between these element.
        else if (errorIndex == -6)
        {
            Debug.LogError("ERROR on line " + lineNum + ": a bond already exists between these indices.");
        }

        // Invalid colour passed.
        else if (errorIndex == -7)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Invalid colour passed in the aesthetics argument. " +
                "Ensure it is a standard CPK colour (e.g. white, black, blue, red) or a hex value " +
                "(e.g. #ffffff, #222222, #1b43f5, #eb3c25).");
        }

        // Decimal expected
        else if (errorIndex == -8)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Decimal number expected for the alpha value inside the aesthetics argument.");
        }

        // Optional attribute provided more than once.
        else if (errorIndex == -9)
        {
            Debug.LogError("ERROR on line " + lineNum + ": The aes arugment contains 2 of the same attribute. " +
                "Check if an attribute has been repeated more than once.");
        }

        // Optional attribute not recognised.
        else if (errorIndex == -10)
        {
            Debug.LogError("ERROR on line " + lineNum + ": An attribute in the aesthetics argument is not recognised. " +
                "Ensure the only attribtues in the aesthetics argument are \"alpha\" or \"colour\".");
        }

        // Optional aesthetic not in the correct syntax.
        else if (errorIndex == -11)
        {
            Debug.LogError("ERROR on line " + lineNum + ": optional argument 2 is invalid. Argument 2 can only contain the optional " +
                "\"aes( content here )\" attribute. ");
        }

    }

}
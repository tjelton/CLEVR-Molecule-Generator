using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Element
{

    private int index;
    private string symbol;
    private int atomicNumber;
    private float xCord;
    private float yCord;
    private float zCord;
    private float radius;
    private string colour;

    // Constructor
    public Element(int index, string symbol, int atomicNumber, float xCord, float yCord, float zCord,
        float radius, string colour)
    {
        this.index = index;
        this.symbol = symbol;
        this.atomicNumber = atomicNumber;
        this.xCord = xCord;
        this.yCord = yCord;
        this.zCord = zCord;
        this.radius = radius;
        this.colour = colour;
    }

    ///// SETTERS /////

    public void setRadius(float radius)
    {
        this.radius = radius;
    }

    public void setColour(string colour)
    {
        this.colour = colour;
    }

    ///// GETTERS /////

    public int getIndex()
    {
        return index;
    }

    public string getSymbol()
    {
        return symbol;
    }

    public int getAtomicNumber()
    {
        return atomicNumber;
    }

    public float getXCord()
    {
        return xCord;
    }

    public float getYCord()
    {
        return yCord;
    }

    public float getZCord()
    {
        return zCord;
    }

    public float getRadius()
    {
        return radius;
    }

    public string getColour()
    {
        return colour;
    }

    ///// STATIC METHODS /////

    /*
     * A function which processes the current run in the ELEMENTS block.
     * A run is defined as all content before a semicolon.
     * This function checks the validity of the run, including checking that the right number of arguments have 
     * been passed in, and that the values are specifed correctly, in the correct order.
     * Parameters:
     *      string run : a string for teh run being processed.
     *      List<Element> elementInput : the array storing the elements which have been processed.
     * Returns: int
     *      Negative integers signals an error.
     *      0 indicates that no error was present (in this case, the Element was correctly constructed,
     *                                              and added to the list).
     */
    public static int ProcessElementsRun(string run, List<Element> elementInputs)
    {

        // Split the run by commas, ensuring that commas within brackets are preserved.
        List<string> argumentsList = new List<string>();
        int returnVal = TextProcessing.splitPreservingBrackets(argumentsList, run);

        // Error: Mismatched brackets.
        if (returnVal < 0)
        {
            return -12;
        }

        // Change List to array for performance.
        string[] arguments = argumentsList.ToArray();

        // Check that there are only 5 or 6 arguments (the 6th argument is the optional aesthetics argument).
        int count = arguments.Length - 1;
        // Error: Not enough arguments.
        if (count < 4 || count > 5)
        {
            return -1;
        }

        // Before processing first 5 arguments, extract the option aesthetic argument and check it is syntactically correct.
        string optionalAesthetics = "";
        if (count == 5)
        {
            optionalAesthetics = arguments[5].ToLower().Trim();

            // Check syntax using regular expressions
            // Regex to match all letters and digits (i.e. [a-zA-Z0-9]) from:
            // https://stackoverflow.com/questions/3028642/regular-expression-for-letters-numbers-and (accessed 4 March)
            Regex optionalAestheticSyntaxRgx = new Regex(@"^aes\((([a-zA-Z0-9])|(\s)|(,)|(=)|(#)|(\.))*\)$");

            // strip the aes and brackets from the string
            if (optionalAestheticSyntaxRgx.IsMatch(optionalAesthetics))
            {
                optionalAesthetics = optionalAesthetics.Replace("aes(", "");
                optionalAesthetics = optionalAesthetics.Replace(")", "");
            }

            // Error: Optional aesthetic argument has incorrect syntax.
            else
            {
                return -10;
            }
        }

        // Check arguent order, and arrange arguments in order.
        string[] elementProperties = new string[5];
        returnVal = inOrderArguments(arguments, elementProperties);
        // Handle error status being returned.
        if (0 != returnVal)
        {
            return returnVal;
        }

        // Now convert the elementProperties to their relevant data types,
        // returning errors when the elements cannot be converted to their respective type.

        // Convert index to integer.
        int index;
        if ( !System.Int32.TryParse(elementProperties[0], out index))
        {
            return -4;
        }

        // Note: To make it easier to identify elements, users may choose to include a number before or after the element. This
        // is fine, but just means that we need to strip the numbers from the symbols.
        // Reference, https://stackoverflow.com/questions/1657282/how-to-remove-numbers-from-string-using-regex-replace accessed 17th Feb 2022.
        string symbol = elementProperties[1];
        symbol = Regex.Replace(symbol, @"[\d-]", string.Empty);

        int atomicNumber = 0;
        float radius = 0;
        string colour = "";
        bool found = false;
        bool ignoreFirstLine = true;

        // Streamreader to read default values.
        // See git for the origins of these default values.
        StreamReader sr = new StreamReader("Packages/com.chemed-vr-toolkit.clevr-molecule-generator/Editor/Styles/DefaultElementAttributes.csv");

        // Check that element symbol exists, and if it does, extract atomic number, radius and colour.
        while (sr.Peek() > 0)
        {
            string[] currentLineAttributes = sr.ReadLine().Split(',');

            // We ignore the first line as this line is for the column names.
            if (ignoreFirstLine)
            {
                ignoreFirstLine = false;
            }

            // Extract default values.
            else
            {
                string currentSymbol = currentLineAttributes[1].ToLower(); // We know from the csv that the second column contains the element symbols.
                currentSymbol = currentSymbol.Replace("\"", ""); // Remove the quotation marks around the string.
                if (string.Equals(currentSymbol, symbol))
                {
                    atomicNumber = System.Int32.Parse(currentLineAttributes[0]);
                    radius = System.Int32.Parse(currentLineAttributes[3]);
                    colour = currentLineAttributes[4];
                    found = true;
                    break;
                }
            }

        }

        // If the symbol was not found, then the symbol inputted was invalid
        if (!found)
        {
            return -5;
        }

        // Convert x cord to integer.
        float xCord;
        if (!(float.TryParse(elementProperties[2], out xCord)))
        {
            return -6;
        }

        // Convert y cord to integer.
        float yCord;
        if (!(float.TryParse(elementProperties[3], out yCord)))
        {
            return -7;
        }

        // Convert z cord to integer.
        float zCord;
        if (!(float.TryParse(elementProperties[4], out zCord)))
        {
            return -8;
        }

        // Check that the index has not been used for any other elements, and return error status where appropriate.
        for (int i = 0; i < elementInputs.Count; i++)
        {
            // Error: index already in used.
            if (elementInputs[i].getIndex() == index)
            {
                return -9;
            }
        }

        // Process optional aesthetic attributes.
        returnVal = processAesthetics(optionalAesthetics, ref colour, ref radius);
        // Handle error status being returned.
        if (0 != returnVal)
        {
            return returnVal;
        }

        // Now with data validation complete, we create the element using the Element class.
        Element element = new Element(index, symbol, atomicNumber, xCord, yCord, zCord, radius, colour);

        // Add the element into the array
        elementInputs.Add(element);

        return 0;

    }

    /* 
     * Checks that the arguments are "in order" and syntactially correct.
     * 
     * For these purposes, "in order" means either that the arguments are passed in 1 by 1 purely as values (e.g. 0,H1,0,0,0) OR
     * the arguments are passed in 1 by 1 followed by equals syntax (e.g. 0,H1,y=0,z=0,x=0) OR
     * the arguments are purely passed in using equals syntax (e.g. x=0, index=0, name=H1, z=0, y=0).
     * 
     * Examples of arguments which are not in order include:
     *      Arguments miss-matched (e.g. 0,0,0,0,H1).
     *      Arguments using equals syntax before using no equals syntax (e.g. index=0, name=H1, 0, 0, 0).
     *      Arguments using incorrect equals syntax (e.g. indexes=0,name=H1, 0, 0, 0).
     *      Incorrect number of arguments provided.
     *      Arguments types being violated.
     *      Incorrecet types being used.
     *      The same equals syntax argument being provided more than once.
     *      
     * Parameters:
     *      string[] arguments : array storing the arguments that have been passed as strings.
     *      string[] elementProperties : array storing the arguments that have been passed in the correct order.
     * Returns: int
     *      Negative values signal error status.
     *      0 signals valid order.
     */
    private static int inOrderArguments(string[] arguments, string[] elementProperties)
    {

        // Checks that there is not a mismatch between equals and default syntax.
        bool orderCheck = true;
        for (int i = 4; i >= 0; i--)
        {
            if (orderCheck)
            {
                // Will continue staying true as long as equal syntax is used throughout.
                orderCheck = orderCheck && arguments[i].Contains("=");
            }
            // Now that a default value has been used, ensure that equal syntax never occurs again, others return error status.
            else
            {
                if (arguments[i].Contains("="))
                {
                    return -2;
                }
            }
        }

        // Now extract values from the input as strings.
        // Error status is returned where an attribute using english notation is repeated.
        for (int i = 0; i <= 4; i++)
        {

            // Trim removes whitespace.
            string currentProperty = arguments[i].Trim().ToLower();

            // This indicates that we are in a case where the equal notation is used.
            // To deal with this, strip out the equal sign and key word.
            if (currentProperty.Contains("="))
            {
                currentProperty = currentProperty.Replace("=", "");

                // Index property.
                if (currentProperty.Contains("index"))
                {
                    currentProperty = currentProperty.Replace("index", "").Trim();
                    if (!insertPropertyString(elementProperties, 0, currentProperty))
                    {
                        return -3;
                    }

                }

                // Element property.
                else if (currentProperty.Contains("symbol"))
                {
                    currentProperty = currentProperty.Replace("symbol", "").Trim();
                    if (!insertPropertyString(elementProperties, 1, currentProperty))
                    {
                        return -3;
                    }
                }

                // x property.
                else if (currentProperty.Contains("x"))
                {
                    currentProperty = currentProperty.Replace("x", "").Trim();
                    if (!insertPropertyString(elementProperties, 2, currentProperty))
                    {
                        return -3;
                    }
                }

                // y property.
                else if (currentProperty.Contains("y"))
                {
                    currentProperty = currentProperty.Replace("y", "").Trim();
                    if (!insertPropertyString(elementProperties, 3, currentProperty))
                    {
                        return -3;
                    }
                }

                // z property.
                else if (currentProperty.Contains("z"))
                {
                    currentProperty = currentProperty.Replace("z", "").Trim();
                    if (!insertPropertyString(elementProperties, 4, currentProperty))
                    {
                        return -3;
                    }
                }

            }
            // Case where the equal notation is not used
            else
            {
                elementProperties[i] = currentProperty;
            }
        }

        // Code reaching means no error has been produced.
        return 0;
    }

    /*
     * Inserts an attribute into an array, but only if the specified index if null.
     * Parameters:
     *      string[] array : array storing the properties (as strings) which have been inserted.
     *      int index : int storing the index to insert the property at.
     *      string property : the property to insert.
     * Retunrs: bool
     *      true indicates that that the property was succesfuly inserted.
     *      false indicates that the index was not null (i.e. a property had already been inserted at that index).
     */
    private static bool insertPropertyString(string[] array, int index, string property)
    {
        if (!(array[index] == null))
        {
            return false;
        }
        array[index] = property;
        return true;
    }

    /*
     * Ensure that the keywords in the optional aesthetics block are syntactically valid,
     * and if they are valid, extract the value ensuring the type is valid.
     * Parameters:
     *      string optionalAesthetics : string storing the optional aesthetics argument.
     *      ref string colour : reference to the colour attribute from the calling code.
     *      ref float radius : reference to the radius attribute from the calling code.
     * Returns: int
     *      Negative values signal error state.
     *      0 signals successful processing.   
     */
    private static int processAesthetics(string optionalAesthetics, ref string colour, ref float radius)
    {

        // To track whether an attribtue has already been updated or not.
        bool colourUpdated = false;
        bool radiusUpdated = false;

        Regex colourRgx = new Regex(@"^colour(\s)*=");
        Regex radiusRgx = new Regex(@"^radius(\s)*=");

        // Regex to tell if a string is a floating point.
        Regex floatingPointRgx = new Regex(@"^[0-9]+(\.([0-9])+)?$");

        if (!optionalAesthetics.Equals(""))
        {

            // Split to get the individual arguments within the aesthetic arugment.
            string[] aestheticComponents = optionalAesthetics.Split(',');

            // Loop through all provided optional aesthetic arguments.
            for (int i = 0; i < aestheticComponents.Length; i++)
            {

                string temp = aestheticComponents[i].ToLower().Trim();

                // Colour attribute
                if (colourRgx.IsMatch(temp))
                {

                    // Only continue if the colour attribute has not already been updated.
                    if (!colourUpdated)
                    {
                        colourUpdated = true;
                        temp = temp.Replace("colour", "").Replace("=", "").Trim();
                        temp = TextProcessing.mapHexColour(temp);
                        if (String.IsNullOrEmpty(temp))
                        {
                            return -14; // Error: Invalid colour passed.
                        }
                        colour = temp;
                    }
                    // Error: Optional argument provided more than once.
                    else
                    {
                        return -11;
                    }
                    continue;
                }

                // Radius attribute.
                if (radiusRgx.IsMatch(temp))
                {

                    // Only continue if the radius attribute has not already been updated.
                    if (!radiusUpdated)
                    {
                        radiusUpdated = true;
                        temp = temp.Replace("radius", "").Replace("=", "").Trim();
                        if (floatingPointRgx.IsMatch(temp))
                        {
                            radius = float.Parse(temp); // Convert string to float.
                        }
                        else
                        {
                            return -13; // Error: Decimal expected.
                        }
                    }
                    // Error: Optional argument provided more than once.
                    else
                    {
                        return -11; 

                    }
                    continue;
                }

                // Error: Optional attribute not recognised.
                else
                {
                    return -15;
                }

            }

        }

        // Successful execution.
        return 0;
    }

    /*
     * When given an index of an element, checks if the index corresponds to an Element object in our array,
     * Parameters:
     *      Element[] elements : the current Element objects which have been processed and constructed.
     *      int index : the index we are check to see if it has already been assgined to an Element.
     * Returns: Element
     *      If an Element already has this index, return this Element object.
     *      If index is unassigned, returns null.
     */
    public static Element getElementBasedOnIndex(Element[] elements, int index)
    {
        foreach (Element element in elements)
        {
            if (element.getIndex() == index)
            {
                return element; // i.e. an element with the given index DOES exist in our array.
            }
        }

        // If execution reaches here, then an element with the given index DOES NOT exist in our array.
        return null;
    }

    /* 
     * Output the relevant error message when given a negative errorIndex.
     * Parameters:
     *      int errorIndex : a negtaive integer representing the error message
     *                         to display.
     *      int lineNum : the line number where the error was raised.
     * Returns: void
     */
    public static void elementErrorMessage(int errorIndex, int lineNum)
    {
        // Error where not enough arguments provided.
        if (errorIndex == -1)
        {
            Debug.LogError("ERROR on line " + lineNum + ": 5 arguments (+1 optional argument) were expected.");
        }

        // Error where the implicit and explicit syntax inappropriately overlap.
        else if (errorIndex == -2)
        {
            Debug.LogError("ERROR on line " + lineNum + ": implicit and explicit syntax inappropriately overlap.");
        }

        // Error where two of the same type of attribute are provided.
        else if (errorIndex == -3)
        {
            Debug.LogError("ERROR on line " + lineNum + ": 2 of the same attributes are provided");
        }

        // Invalid index.
        else if (errorIndex == -4)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Index invalid. Index should be an integer.");
        }

        // Invalid symbol.
        else if (errorIndex == -5)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Your chemical symbol is not recognised.");
        }

        // Invalid xCord.
        else if (errorIndex == -6)
        {
            Debug.LogError("ERROR on line " + lineNum + ": x value invalid. x value should be an integer.");
        }

        // Invalid yCord.
        else if (errorIndex == -7)
        {
            Debug.LogError("ERROR on line " + lineNum + ": y value invalid. y value should be an integer.");
        }

        // Invalid zCord.
        else if (errorIndex == -8)
        {
            Debug.LogError("ERROR on line " + lineNum + ": z value invalid. z value should be an integer.");
        }

        // Index used more than once.
        else if (errorIndex == -9)
        {
            Debug.LogError("ERROR on line " + lineNum + ": the index for this element has been used earlier.");
        }

        // Optional aesthetic argument has incorrect syntax.
        else if (errorIndex == -10)
        {
            Debug.LogError("ERROR on line " + lineNum + ": optional argument 6 is invalid. Argument 6 can only contain the optional \"aes( content here )\" attribute. ");
        }

        // Optional argument provided more than once.
        else if (errorIndex == -11)
        {
            Debug.LogError("ERROR on line " + lineNum + ": The aes arugment contains 2 of the same attribute. Check if an attribute has been repeated more than once.");
        }

        // Mismatched bracket.
        else if (errorIndex == -12)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Mis-matched brackets, or too many brackets provided.");
        }

        // Expected a decimal number.
        else if (errorIndex == -13)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Decimal number expected for the radius value inside the aesthetics argument.");
        }

        // Invalid colour passed.
        else if (errorIndex == -14)
        {
            Debug.LogError("ERROR on line " + lineNum + ": Invalid colour passed in the aesthetics argument. Ensure it is a standard CPK colour " +
                "(e.g. white, black, blue, red) or a hex value (e.g. #ffffff, #222222, #1b43f5, #eb3c25).");
        }

        // Optional attribute not recognised.
        else if (errorIndex == -15)
        {
            Debug.LogError("ERROR on line " + lineNum + ": An attribute in the aesthetics argument is not recognised. Ensure the only attribtues in the aesthetics " +
                "argument are \"radius\" or \"colour\".");
        }
    } 

}

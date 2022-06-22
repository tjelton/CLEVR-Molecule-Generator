using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MoleculeGenerator : EditorWindow
{

    /*
     * Drives the moleucle generator program.
     * MenuItem (in the square brackets) makes the menu item in the tool bar. 
     * Parameters: void.
     * Returns: void.
     */
    [MenuItem("Tools/Molecule Generator", false, 1)]
    static void Driver()
    {
        MoleculeGenerator window = (MoleculeGenerator)MoleculeGenerator.GetWindow(typeof(MoleculeGenerator), true, "Molecule Generator");
        Debug.Log("Molecule Generator window successfully opened.");
    }

    /*
     * Creates the Molecule Generator UI/window.
     * Adapted from: https://blog.theknightsofunity.com/custom-unity-editor-window/ (last accessed 22 April 2022)
     */
    void OnGUI()
    {

        // Text files for providing file paths.
        UserInterface.FolderPath = EditorGUILayout.TextField("Folder Path", UserInterface.FolderPath);
        UserInterface.FileName = EditorGUILayout.TextField("File Name", UserInterface.FileName);

        // Button spacing.
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        // Set background colour
        GUI.backgroundColor = Color.cyan;

        // Set up and display "Reset File Paths to Default" button.
        // When this button is prrssed, display the default values in the text files.
        if (GUILayout.Button("Reset File Paths to Default", GUILayout.Width(250), GUILayout.Height(50)))
        {
            UserInterface.FolderPath = "Packages/com.chemed-vr-toolkit.clevr-molecule-generator/Editor/MoleculeSpecifications";
            UserInterface.FileName = "example.txt";
        }

        // Set up and display "Build" button.
        if (GUILayout.Button("Build", GUILayout.Width(250), GUILayout.Height(50)))
        {
            ReadFile();
        }

        // Button spacing.
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

    /* 
     * Checks that the text file actually exists, and if so, start a stream reader
     * Parameters: None
     * Returns: void
     */
    static void ReadFile()
    {

        // Get full path.
        string folderPath = UserInterface.FolderPath;
        string filePath = UserInterface.FileName;
        string wholePath = folderPath + "/" + filePath;

        // Check path exists.
        // If it does not exist, this code will execute with an error message, and the script will halt.
        if (!File.Exists(wholePath))
        {
            Debug.LogError("ERROR: The file path specified does not exist. Check the folder and file paths are correct.");
            return;
        }

        // Create stream reader (Note: the code only reaches here if the file was found).
        StreamReader sr = new StreamReader(wholePath);

        // Variable to store the line number currently on in the text asset file.
        // This will help users understand where an error occured.
        int lineNum = 1;

        // Start processing ELEMENTS block.
        ElementsBlock(sr, lineNum);

    }

    /*
     * Checks the ELEMENTS's block syntax providing relevant ERROR messages for syntax errors.
     * Also, interprets lines of code in the ELEMENTS block, passing each element into a list.
     * Parameters:
     *      StreamReader sr : the stream reader which is reading the file we are interpreting.
     *      int lineNum : The number of the current line being processed.
     * Returns: void.
     */
    static void ElementsBlock(StreamReader sr, int lineNum)
    {

        // List to store the elements which have been processed.
        List<Element> elementInputs = new List<Element>();

        string run;

        // Check that the beggining of the ELEMENTS block has correctly been specified.
        if (!TextProcessing.processLine(sr, ref lineNum).ToLower().Trim().Equals("elements{")) {
            Debug.LogError("ERROR on line " + lineNum + ": the ELEMENTS block was not correctly specified.");
            return;
        }

        // Bool to let us know if the elements block has correctly concluded
        bool elementBlockConcluded = false;

        // Ensuring that there are still line in the file
        while (sr.Peek() > 0)
        {

            // Get the next line of input and dealt with error states
            run = TextProcessing.processLine(sr, ref lineNum);
            if (run.Equals("-1") || run.Equals("-2"))
            {
                return; // Exit exeuction. An error message was already displayed.
            }

            // Check that the element block has been concluded
            if (run.Equals("}"))
            {
                elementBlockConcluded = true;
                break;
            }

            // If the run contains a single '}', this indicates the end of the ELEMENTS block.
            // This means we can safely exit the loop.

            // Process run to ensure correct number of arguments are specified, and produce error messages as required.
            int returnValue = Element.ProcessElementsRun(run, elementInputs);

            // Values not 0 represent an error status. Hence, return, stopping execution.
            if (0 != returnValue)
            {
                Element.elementErrorMessage(returnValue, lineNum);
                return;
            }

        }

        // If the elements block has not been concluded, and now we are outside the loop,
        // the elements block was never specified correcly. Display an error!
        if (!elementBlockConcluded)
        {
            Debug.LogError("ERROR on line " + lineNum + ": ELEMENTS block not specified correctly. Ensure that an ending brace is used.");
            return; // Stop execution.
        }

        // Convert array list into an array to improve performance.
        Element[] elements = elementInputs.ToArray();

        // Now to start processing the bonds block.
        BondsBlock(sr, lineNum, elements);

    }

    /*
     * Checks the BONDS's block syntax providing relevant ERROR messages for syntax errors.
     * Also, interprets lines of code in the BONDS block, storing each element into a list.
     * Parameters:
     *      StreamReader sr : the stream reader which is reading the file we are interpreting.
     *      int lineNum : The number of the current line being processed.
     *      Element[] elements : the list storing the elemenets which have been previously processed. 
     * Returns: void.
     */
    static void BondsBlock(StreamReader sr, int lineNum, Element[] elements)
    {

        string run;

        // List to store the bonds
        List<Bond> bondsInput = new List<Bond>();

        // Check that the beggining of the ELEMENTS block has correctly been specified.
        if (!TextProcessing.processLine(sr, ref lineNum).ToLower().Trim().Equals("bonds{"))
        {
            Debug.LogError("ERROR on line " + lineNum + ": the BONDS block was not correctly specified.");
            return;
        }

        // Bool to let us know if the elements block has correctly concluded
        bool bondBlockConcluded = false;

        // Ensuring that there are still line in the file
        while (sr.Peek() > 0)
        {

            // Get the next line of input and dealt with error states
            run = TextProcessing.processLine(sr, ref lineNum);
            if (run.Equals("-1") || run.Equals("-2"))
            {
                return; // Exit exeuction. An error message was already displayed in the processLine function.
            }

            // Check that the element block has been concluded
            if (run.Equals("}"))
            {
                bondBlockConcluded = true;
                break;
            }

            // If the run contains a single '}', this indicates the end of the ELEMENTS block.
            // This means we can safely exit the loop.

            // Process run to ensure correct number of arguments are specified, and produce error messages as required.
            int returnValue = Bond.ProcessBondsRun(bondsInput, elements, lineNum, run);

            // Values not 0 represent an error status. Hence, return, stopping execution.
            if (0 != returnValue)
            {
                Bond.bondErrorMessage(returnValue, lineNum);
                return;
            }


        }

        // If the elements block has not been concluded, and now we are outside the loop,
        // the elements block was never specified correcly. Display an error.
        if (!bondBlockConcluded)
        {
            Debug.LogError("ERROR on line " + lineNum + ": BONDS block not specified correctly. Ensure that an ending brace is used.");
            return; // Stop execution.
        }

        // Convert array list into an array to improve performance.
        Bond[] bonds = bondsInput.ToArray();

        // Create the molecule.
        BuildMolecule.buildMolecule(elements, bonds);
        Debug.Log("Molecule Generation Successful!");

    }
}
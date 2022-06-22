// Code in this file was adapted from https://blog.theknightsofunity.com/custom-unity-editor-window/ (last accessed 22 April 2022)
// For more info regarding how the following code operates, please see the link above.

#if UNITY_EDITOR
using UnityEditor;
#endif

// Class which controls the user input for the Molecule Generator pop up window.
public class UserInterface
{

    // Get user input for the folder path.
    public static string FolderPath
    {
        get
        {
            #if UNITY_EDITOR
                    return EditorPrefs.GetString("FolderPath", "Packages/com.chemed-vr-toolkit.clevr-molecule-generator/Editor/MoleculeSpecifications");
            #else
                    return false;
            #endif
        }

        set
        {
            #if UNITY_EDITOR
                EditorPrefs.SetString("FolderPath", value);
            #endif
        }
    }

    // Get user input for the filen name.
    public static string FileName
    {
        get
        {
            #if UNITY_EDITOR
                return EditorPrefs.GetString("FileName", "example.txt");
            #else
                return false;
            #endif
        }

        set
        {
            #if UNITY_EDITOR
                EditorPrefs.SetString("FileName", value);
            #endif
        }
    }

}






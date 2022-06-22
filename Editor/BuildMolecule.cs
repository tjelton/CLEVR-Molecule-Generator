using System;
using UnityEditor;
using UnityEngine;

public class BuildMolecule : MonoBehaviour
{

    /*
     * Builds the molecule based on the elements and bonds which the user has input.
     * Once the molecule is built, it is saved as a prefab into the assets folder.
     * Parameters:
     *      Element[] elements : elements to build.
     *      Bond[] bonds : bonds to build.
     * Returns: void
     */
    public static void buildMolecule(Element[] elements, Bond[] bonds)
    {

        // For how to build a prefab with an Editor script, check out:
        // https://docs.unity3d.com/ScriptReference/PrefabUtility.EditPrefabContentsScope.html (Last accessed 17th Feb 2022)

        // Strip the extension from the file name
        string strippedFileName = UserInterface.FileName.Substring(0, UserInterface.FileName.LastIndexOf("."));

        // Path to save the molecule prefab. 
        string assetPath = "Assets/" + strippedFileName + ".prefab";

        // Create prefab.
        GameObject moleculeGenerator = new GameObject();
        PrefabUtility.SaveAsPrefabAsset(moleculeGenerator, assetPath);

        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
        {
            var prefabRoot = editingScope.prefabContentsRoot;


            int index = 0;
            // Loop through each element in the array, creating a primitive sphere for each element.
            foreach (Element element in elements)
            {

                // Create a standard sphere
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = prefabRoot.transform;

                // Model the sphere based upon the elements attributes:

                // Sphere position
                sphere.transform.position = new Vector3(element.getXCord(), element.getYCord(), element.getZCord());

                // Element name
                sphere.name = element.getSymbol() + element.getIndex();

                // Size
                float radius = element.getRadius();
                sphere.transform.localScale = new Vector3(radius / 100, radius / 100, radius / 100); // Scale down scale by 100

                // Colour
                string colour = element.getColour();
                colour = colour.Replace("\"", ""); // Remove the speech marks which have ended up being appended somehow...

                // Extra "1" is because the alpha is 1.
                string materialFilePath = colour;

                // Convert RGB hex representation into unity Colour.
                Color col;
                ColorUtility.TryParseHtmlString(colour, out col);

                /*
                 * Need to fix materials!
                 */

                // Only construct the material if a material with the same name doesn't already exist.
                Material colouredMaterial = Resources.Load<Material>("Assets/" + materialFilePath);
                if (colouredMaterial == null)
                {
                    materialFilePath = "Assets/" + materialFilePath + index + ".mat";
                    Material newMaterial = new Material(Shader.Find("Diffuse"));
                    newMaterial.SetColor("_Color", col);
                    AssetDatabase.CreateAsset(newMaterial, materialFilePath);
                    colouredMaterial = newMaterial;
                }

                // Set colour (material) of sphere.
                var renderer = sphere.GetComponent<MeshRenderer>();
                renderer.material = colouredMaterial;

                index++;

            }

            index = 0;

            // Loop through each bond in the array, adding the bonds to the molecule.
            foreach (Bond bond in bonds)
            {

                // Keeps looping based upon the bond degree.
                // i.e. if we have a bond degree of 2, we have to loop twice to create 2 cylinders.
                for (int i = 0; i < bond.getBondDegree(); i++)
                {

                    // Coordinate shifts for 2 and 3 bond degrees
                    // For these bond degrees, we have to ensure there is room for each bond.
                    double shiftX = 0;
                    if (bond.getBondDegree() == 2)
                    {
                        if (i == 0)
                        {
                            shiftX = -0.05;
                        } else if (i == 1)
                        {
                            shiftX = 0.05;
                        }
                    } else if (bond.getBondDegree() == 3)
                    {
                        if (i == 0)
                        {
                            shiftX = -0.1;
                        } else if (i == 1)
                        {
                            shiftX = 0.0;
                        } else if (i == 2)
                        {
                            shiftX = 0.1;
                        }
                    }

                    // Create a standard cylinder
                    var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cylinder.transform.parent = prefabRoot.transform;

                    Element elementFrom = bond.getElementFrom();
                    Element elementTo = bond.getElementTo();

                    // Positioning of cylinders adapted from code by Sebastian Dunn.
                    Vector3 fromPos = new Vector3(elementFrom.getXCord(), elementFrom.getYCord(), elementFrom.getZCord());
                    Vector3 toPos = new Vector3(elementTo.getXCord(), elementTo.getYCord(), elementTo.getZCord());
                    Vector3 halfWayBetweenAtoms = Vector3.Lerp(fromPos, toPos, 0.5f);
                    cylinder.transform.position = halfWayBetweenAtoms;
                    cylinder.transform.LookAt(toPos);
                    cylinder.transform.RotateAround(cylinder.transform.position, cylinder.transform.right, 90);

                    // Set cylinder name.
                    cylinder.name = "bond from " + elementFrom.getSymbol() + elementFrom.getIndex() + 
                        " to " + elementTo.getSymbol() + elementTo.getIndex();

                    // Distance between the two elements to find size of the cylinder.
                    double distance = Math.Sqrt(
                        Math.Pow(elementTo.getXCord() - elementFrom.getXCord(), 2) +
                        Math.Pow(elementTo.getYCord() - elementFrom.getYCord(), 2) +
                        Math.Pow(elementTo.getZCord() - elementFrom.getZCord(), 2)
                        );
                    distance = distance / 2;

                    // Bond scale.
                    cylinder.transform.localScale = new Vector3((float)0.05, (float)distance, (float)0.05);

                    // Bond offset (for bonds of degree 2 or 3).
                    cylinder.transform.Translate((float)shiftX, 0, 0);

                    // Convert RGB hex representation into unity Colour.
                    string colour = bond.getColour();
                    colour = colour.Replace("\"", ""); // Remove the speech marks which have ended up being appended somehow...
                    Color col;
                    ColorUtility.TryParseHtmlString(colour, out col);

                    // Bond alpha
                    col.a = bond.getAlpha();

                    /*
                     * Need to fix materials!
                     */

                    // Only construct the material if a material with the same name doesn't already exist.
                    string alphaAsString = Convert.ToString(bond.getAlpha());
                    string materialFilePath = colour + alphaAsString + "t"; // "t" to indicate the material has transparency properties.
                    Material colouredMaterial = Resources.Load<Material>(materialFilePath);
                    if (colouredMaterial == null)
                    {
                        materialFilePath = "Assets/" + materialFilePath + index + ".mat";

                        // Transparent because we want to be able to adjust alpha.
                        Material newmaterial = new Material(Shader.Find("Transparent/Diffuse")); 

                        newmaterial.SetColor("_Color", col);
                        AssetDatabase.CreateAsset(newmaterial, materialFilePath);
                        colouredMaterial = newmaterial;
                    }

                    // Set colour (material) of bond.
                    var renderer = cylinder.GetComponent<MeshRenderer>();
                    renderer.material = colouredMaterial;

                    index++;

                }


            }
        }

        // To create the molecule, we had to create a new GameObject to build the molecule on.
        // This removes the temporary GameObject which was created.
        DestroyImmediate(moleculeGenerator);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Controller : MonoBehaviour {

	public float elasticityModulusInput;

	public float force;		
	public float length;
	public float height;
	public float depth;

	public GameObject tableTop;
	private float tableY;
	private float tableLength;

	private int lengthVert;
	private int halfVert;

	[HideInInspector]
	public float conversion;

	private float crossSectionalArea;

	[HideInInspector]
	public float maxDeflection;
	private float deflectionX;
	private float elasticityModulus;

	private GameObject[] toBeDestroyed;

	private Mesh mesh;

	private Vector3[] vertices;
	private int[] triangles;

	void Awake() {
		mesh = new Mesh ();
		mesh = GetComponent<MeshFilter>().mesh;
		conversion = (1f / 39.3700787f);
	}

	void Start () {																												/* Calls main updateDrawing function once, to get the ball rolling. */
		UpdateDrawing ();
	}

	void Update() {
	//	if (Input.GetButton ("Fire1")) {
			UpdateDrawing ();
	//	}
	}

	void UpdateDrawing() {																										/* Does everything necessary to delete the old scene and draw a new one */
		Setup ();
		CalculateVertices ();
		CalculateTriangles ();
		DrawMesh ();
	}

	void Setup() {																												/* Does all necessary calculations to begin drawing the mesh */

		lengthVert = Mathf.FloorToInt(length);																					// Takes float length and converts it to a managable value for an array

		vertices = new Vector3[2 * ((2 * lengthVert) + 2)];																		// The number of vertices will be the length of the vertices * 2 plus 2 to account for the floor function loss;
		triangles = new int[(2 * (6 * lengthVert)) + 6 * lengthVert + 6 * lengthVert + 12];										// 2 triangles per vertex, 6 locations for these triangles

		crossSectionalArea = (depth * Mathf.Pow (height, 3)) / 12;																// Calculates I					|
		elasticityModulus = elasticityModulusInput * Mathf.Pow (10, 6);															// Calculates E					| ---> All gathered from PLTW Formulas for Beam Deflection
		maxDeflection = (force * Mathf.Pow(length, 3)) / ((48) * (elasticityModulus) * (crossSectionalArea));					// Calculates Max Deflection	|

		toBeDestroyed = GameObject.FindGameObjectsWithTag("General Marker");													// Loops through general marker objects and destroys them (Just the main plate for now)
		for (int i = 0; i < toBeDestroyed.Length; i++) {
			Destroy (toBeDestroyed[i]);
		}

		tableY = ((-maxDeflection - height / 2) - 6) * conversion;
		tableLength = (length + length / 4) * conversion; 

		tableTop.transform.localScale = new Vector3 (tableLength, 0.1f, tableLength);											// Changes the size of the prefab (reference Marker) that needs to be instantiated
		Instantiate (tableTop, new Vector3 (0, tableY, 0), Quaternion.identity);												// Instantiates the reference Marker
	}

	void CalculateVertices() {																									/* Calculates all vertex positions for the mesh */

		int vert = 0;																											// Instantiates variable vert, used to keep track of which vertex we are managing

		for (float i = -(lengthVert/2); i < (lengthVert/2); i++) {																// Loops through the length from left to the right 
			deflectionX = (((4 * maxDeflection) / (length * length)) * i * i) - maxDeflection;									// Calculates the deflection at any given point, using the equation that we derived
			vertices[vert] = new Vector3 (i * conversion, (deflectionX) * conversion, (-depth / 2) * conversion);				// Applies the V3s to the vertices array 
			vertices[vert + 1] = new Vector3 (i * conversion, (deflectionX) * conversion, (depth / 2) * conversion);			// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			vert += 2;																											// Adds two to vert, so that the cycle can restart
		}

		vertices [vert] = new Vector3 ((length / 2) * conversion, 0, (-depth / 2) * conversion);								// Adds the two final vertices at the end of the shape
		vertices [vert + 1] = new Vector3 ((length / 2) * conversion, 0, (depth / 2) * conversion);								// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

		vert += 2;

		halfVert = vert;
	
		for (float i = -(lengthVert/2); i < (lengthVert/2); i++) {																// Loops through the length from left to the right
			deflectionX = (((4 * maxDeflection) / (length * length)) * i * i) - maxDeflection;									// Calculates the deflection at any given point, using the equation that we derived
			vertices[vert] = new Vector3 (i * conversion, ((deflectionX + height) * conversion), (-depth / 2) * conversion);	// Applies the V3s to the vertices array 
			vertices[vert + 1] = new Vector3 (i * conversion, (deflectionX + height) * conversion, (depth / 2) * conversion);	// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			vert += 2;																											// Adds two to vert, so that the cycle can restart
		}

		vertices [vert] = new Vector3 ((length / 2) * conversion, height * conversion, (-depth / 2) * conversion);				// Adds the two final vertices at the end of the shape
		vertices [vert + 1] = new Vector3 ((length / 2) * conversion, height * conversion, (depth / 2) * conversion);			// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

		vert = 0;																												// Resets vert for the future calculations
	
	}

	void CalculateTriangles() {																									/* Calculates the vertex relationships  */

		int index = 0;																											// Instantiates variable index, used to keep track of which vertex we are managing

		for (int i = 0; i < lengthVert; i++) {																					// Loops through all vertices and creates two triangles per index
			triangles[index] = (2 * i);																							//		|
			index++;																											//		|
			triangles[index] = (2 * i)  + 2;																					//		| ------> Triangle One
			index++;																											//		|
			triangles[index] = (2 * i) + 1;																						//		|	
			index++;																											//
			triangles[index] = (2 * i) + 1;																						//		|
			index++;																											//		|
			triangles[index] = (2 * i)  + 2;																					//		| -------> Triangle Two
			index++;																											//		|
			triangles[index] = (2 * i) + 3;																						//		|
			index++;																											//
		}
	
		for (int i = lengthVert + 1; i < 2 * lengthVert; i++) {																	
			triangles[index] = (2 * i);																							
			index++;																												
			triangles[index] = (2 * i)  + 1;																						
			index++;																												
			triangles[index] = (2 * i) + 2;																						
			index++;																												
			triangles[index] = (2 * i) + 2;																						
			index++;																												
			triangles[index] = (2 * i)  + 1;																						
			index++;																												
			triangles[index] = (2 * i) + 3;																						
			index++;																												
		}

		for (int i = 0; i < lengthVert; i++) {
			triangles [index] = halfVert + (2 * i);
			index++;
			triangles [index] = (2 * i) + 2;
			index++;
			triangles [index] = (2 * i);
			index++;
			triangles [index] = halfVert + (2 * i) + 2;
			index++;
			triangles [index] = (2 * i) + 2;
			index++;
			triangles [index] = halfVert + (2 * i);
			index++;
		}

		for (int i = 0; i < lengthVert; i++) {
			triangles [index] = (2 * i) + 1;
			index++;
			triangles [index] = (2 * i) + 3;
			index++;
			triangles [index] = halfVert + (2 * i) + 1;
			index++;
			triangles [index] = halfVert + (2 * i) + 1;
			index++;
			triangles [index] = (2 * i) + 3;
			index++;
			triangles [index] = halfVert + (2 * i) + 3;
			index++;
		}

		triangles [index] = 0;
		index++;
		triangles [index] = 1;
		index++;
		triangles [index] = halfVert;
		index++;
		triangles [index] = halfVert;
		index++;
		triangles [index] = 1;
		index++;
		triangles [index] = halfVert + 1;
		index++;

		triangles [index] = 2 * lengthVert - 1;
		index++;
		triangles [index] = 2 * lengthVert;
		index++;
		triangles [index] = vertices.Length - 2;
		index++;
		triangles [index] = vertices.Length - 1;
		index++;
		triangles [index] = 2 * lengthVert - 1;
		index++;
		triangles [index] = vertices.Length - 2;
		index++;

		index = 0;
	}

	void DrawMesh() {																											/* Finalizes all values and applies them to the mesh */
		mesh.vertices = vertices;
		mesh.triangles = triangles;
	}
}

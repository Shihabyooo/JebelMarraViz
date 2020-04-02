//A slightly modified version of:
//http://wiki.unity3d.com/index.php/MeshCreationGrid

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCreator : MonoBehaviour
{
    
    public Texture2D Texture;

    public GameObject[] meshHolders = null;
    Mesh[] meshList;
    List<Vector3>[] verticesList;
    List<int>[] trianglesList;
    List<Vector3>[] normalsList;
    List<Vector2>[] uvsList;

    public int meshSubsections = 10;
    int counter = 0;
    public int maxTilesPerMesh = 30000;
 
    public void CreateGroundMesh(Vector3 swCorner, float tileSize, float xWidth, float yHeight) //creates a flat ground
    {
        int tileGridWidth = Mathf.RoundToInt(xWidth / tileSize);
        int tileGridHeight = Mathf.RoundToInt(yHeight / tileSize);

        float[][] zeroGrid = new float[tileGridWidth][];
        for (int k = 0; k < tileGridWidth; k++)
        {
            zeroGrid[k] = new float[tileGridHeight];
        }

        CreateGroundMesh(swCorner, tileSize, xWidth, yHeight, ref zeroGrid);
    }

    public void CreateGroundMesh(Vector3 swCorner, float tileSize, float xWidth, float yHeight, ref float[][] elevationsGrid) //creates a ground with topography matching elevationsGrid
    {
        counter = 0;
        
        

        int totalHorizontalTiles = Mathf.CeilToInt(xWidth / tileSize);
        int totalVerticalTiles = Mathf.CeilToInt(yHeight / tileSize);

        //for now, let's just fit input into a squared grid, will cause part of the grids to be flat (depending on how rectangular the DEM/DTM is)
        int targetTilesNoPerAxis = Mathf.Max(totalHorizontalTiles, totalVerticalTiles);
        int noOfTilesPerAxisPerSubsection = Mathf.FloorToInt(Mathf.Sqrt(maxTilesPerMesh)); //assumes squared subsections. FloorToInt to maintain results within our maxTilesPerMesh limits
        int noOfSubsectionsInOneAxis = Mathf.Clamp(Mathf.CeilToInt((float)targetTilesNoPerAxis / (float)noOfTilesPerAxisPerSubsection), 1, int.MaxValue);
        int totalNoOfSubsections = noOfSubsectionsInOneAxis * noOfSubsectionsInOneAxis;
        
        InitializeMeshHolders(totalNoOfSubsections);
        
        print("xWidth: " + xWidth + ", yHeight: " + yHeight);
        print("targetTilesNoPerAxis: " + targetTilesNoPerAxis);
        print("noOfTilesPerAxisPerSubsection: " + noOfTilesPerAxisPerSubsection);
        print("noOfSubsectionsInOneAxis: " + noOfSubsectionsInOneAxis);

        #region meshing loop
        //for (int k = 0; k < meshSubsections; k++) //coule've used counter...
        for (int k = 0; k < totalNoOfSubsections; k++) 
        {
            print ("counter: " + counter);
            //float xLocalWidth = xWidth / (float) noOfSubsectionsInOneAxis;
            float xLocalWidth = noOfTilesPerAxisPerSubsection * tileSize;
            float yLocalHeight = xLocalWidth; 

            float[][] elevationsSubGrid = new float[noOfTilesPerAxisPerSubsection + 2][];

            int xCellsToLocalSWCorner = (counter % noOfSubsectionsInOneAxis) * noOfTilesPerAxisPerSubsection;
            int yCellsToLocalSWCorner = (counter / noOfSubsectionsInOneAxis) * noOfTilesPerAxisPerSubsection;

            //print ("xCellsToLocalSWCorner: " + xCellsToLocalSWCorner);
            //print ("yCellsToLocalSWCorner: " + yCellsToLocalSWCorner);

            for (int i = 0; i < noOfTilesPerAxisPerSubsection + 2; i++)
            {
                elevationsSubGrid[i] = new float[noOfTilesPerAxisPerSubsection + 2];
                for (int j = 0; j < noOfTilesPerAxisPerSubsection + 2; j++)
                {
                    int _x = xCellsToLocalSWCorner + i - 1;
                    int _y = yCellsToLocalSWCorner + j - 1;
                    
                    if ( _x >= elevationsGrid.Length || _y >= elevationsGrid[0].Length)
                        elevationsSubGrid[i][j] = 0.0f;
                    else if (_x < 0 || _y < 0) //TODO merge this with above if clause
                        elevationsSubGrid[i][j] = 0.0f;
                    else
                        elevationsSubGrid[i][j] = elevationsGrid[_x][_y];
                }
            }

            Vector3 localSWCorner = swCorner + new Vector3(xCellsToLocalSWCorner * tileSize,
                                                            0.0f,
                                                            yCellsToLocalSWCorner * tileSize);
            
            int index = 0;
            for (int x = 0; x < noOfTilesPerAxisPerSubsection; x++)
            {
                for (int y = 0; y < noOfTilesPerAxisPerSubsection; y++)
                {
                    float[] heights = GetVertsHeightPerQuad(x + 1, y + 1, noOfTilesPerAxisPerSubsection - 1, noOfTilesPerAxisPerSubsection - 1, ref elevationsSubGrid);
                    
                    AddVertices(localSWCorner, tileSize, tileSize, y, x, verticesList[counter], heights);
                    index = AddTriangles(index, trianglesList[counter]);
                    AddNormals(normalsList[counter]);
                    AddUvs(swCorner, localSWCorner, tileSize, x, y, xWidth, yHeight, uvsList[counter]);
                }
            }

            meshList[counter].vertices = verticesList[counter].ToArray();
            meshList[counter].normals = normalsList[counter].ToArray();
            meshList[counter].triangles = trianglesList[counter].ToArray();
            meshList[counter].uv = uvsList[counter].ToArray();
            meshList[counter].RecalculateNormals();

            counter++;            
            // if (counter > Mathf.Pow(noOfSubsectionsInOneAxis, 2.0f))
            //     break;
        }
        #endregion
    }
    
    void InitializeMeshHolders(int noOfHolders)
    {

        meshList = new Mesh[noOfHolders];
        MeshFilter[] meshFilterList = new MeshFilter[noOfHolders];

        if (meshHolders != null)
            foreach (GameObject obj in meshHolders)
                Destroy(obj);

        meshHolders = new GameObject[noOfHolders];

        verticesList = new List<Vector3>[noOfHolders];
        trianglesList = new List<int>[noOfHolders];
        normalsList = new List<Vector3>[noOfHolders];
        uvsList = new List<Vector2>[noOfHolders];
        
        for (int i = 0; i < noOfHolders; i++)
        {
            meshHolders[i] = new GameObject("MeshHolder" + i.ToString());
            meshHolders[i].transform.position = Vector3.zero;
            meshHolders[i].transform.SetParent(this.transform);
            
            meshFilterList[i] = meshHolders[i].AddComponent<MeshFilter>();
            meshHolders[i].AddComponent<MeshRenderer>();

            meshFilterList[i].GetComponent<Renderer>().material.SetTexture("_MainTex", Texture);
            meshList[i] = new Mesh();
            meshFilterList[i].mesh = meshList[i];

            verticesList[i] = new List<Vector3>();
            trianglesList[i] = new List<int>();
            normalsList[i] = new List<Vector3>();
            uvsList[i] = new List<Vector2>();
        } 
    }

    float[] GetVertsHeightPerQuad(int pixelX, int pixelY, int maxX, int maxY, ref float[][] elevationGrid)
    {
        //Note: the order goes counter clockwise from SW corner.
        float[] heights = new float[4];

        heights[0] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX - 1][pixelY - 1] + elevationGrid[pixelX][pixelY - 1] + elevationGrid[pixelX - 1][pixelY]) / 4.0f;
        heights[1] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY - 1] + elevationGrid[pixelX][pixelY - 1] + elevationGrid[pixelX + 1][pixelY]) / 4.0f;
        heights[2] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY + 1] + elevationGrid[pixelX][pixelY + 1] + elevationGrid[pixelX + 1][pixelY]) / 4.0f;
        heights[3] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX - 1][pixelY + 1] + elevationGrid[pixelX][pixelY + 1] + elevationGrid[pixelX - 1][pixelY]) / 4.0f;

        return heights;
    }

    void AddVertices(Vector3 swCorner, float tileHeight, float tileWidth, int y, int x, ICollection<Vector3> vertices, float[] height)
    {
        vertices.Add( this.transform.TransformPoint( swCorner + new Vector3(((float)x * tileWidth), height[0], ((float)y * tileHeight))));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth) + tileWidth, height[1], ((float)y * tileHeight))));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth) + tileWidth, height[2], ((float)y * tileHeight) + tileHeight)));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth), height[3], ((float)y * tileHeight) + tileHeight)));
    }

    int AddTriangles(int index, ICollection<int> triangles)
    {
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
        index += 4;
        return index;
    }

    void AddNormals(ICollection<Vector3> normals)
    {
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
    }

    //private static void AddUvs(int tileRow, float tileSizeY, float tileSizeX, ICollection<Vector2> uvs, int tileColumn)
    void AddUvs(Vector3 swCorner, Vector3 localSWCorner, float tileSize, int x, int y, float totalWidth, float totalHeight, ICollection<Vector2> uvs)
    {
        Vector3[] tempPosList = new Vector3[4];

        tempPosList[0] = localSWCorner + new Vector3(((float)x * tileSize), 0.0f, ((float)y * tileSize)) - swCorner;
        tempPosList[1] = localSWCorner + new Vector3(((float)x * tileSize) + tileSize, 0.0f, ((float)y * tileSize)) - swCorner;
        tempPosList[2] = localSWCorner + new Vector3(((float)x * tileSize) + tileSize, 0.0f, ((float)y * tileSize) + tileSize) - swCorner;
        tempPosList[3] = localSWCorner + new Vector3(((float)x * tileSize), 0.0f, ((float)y * tileSize) + tileSize) - swCorner;

        uvs.Add(new Vector2(tempPosList[0].x / totalWidth, tempPosList[0].z / totalHeight));
        uvs.Add(new Vector2(tempPosList[1].x / totalWidth, tempPosList[1].z / totalHeight));
        uvs.Add(new Vector2(tempPosList[2].x / totalWidth, tempPosList[2].z / totalHeight));
        uvs.Add(new Vector2(tempPosList[3].x / totalWidth, tempPosList[3].z / totalHeight));

        //print("Div:"  + tempPosList[0].x / totalWidth + ", and " + tempPosList[0].z / totalHeight);
        //print("Div:" + tempPosList[1].x / totalWidth + ", and " + tempPosList[1].z / totalHeight);
        //print("Div:" + tempPosList[2].x / totalWidth + ", and " + tempPosList[2].z / totalHeight);
        //print("Div:" + tempPosList[3].x / totalWidth + ", and " + tempPosList[3].z / totalHeight);

    }
}

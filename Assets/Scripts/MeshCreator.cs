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

    public int meshSubsections = 10;
    int counter = 0;
    public int maxTilesPerMesh = 30000;
    
    public void UpdateGrid(Vector2 gridIndex, Vector2 tileIndex, float tileSize, int gridWidth)
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;

        //var tileSizeX = 1.0f / numTilesX;
        //var tileSizeY = 1.0f / numTilesY;

        mesh.uv = uvs;

        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 0] = new Vector2(tileIndex.x * tileSize, tileIndex.y * tileSize);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 1] = new Vector2((tileIndex.x + 1) * tileSize, tileIndex.y * tileSize);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 2] = new Vector2((tileIndex.x + 1) * tileSize, (tileIndex.y + 1) * tileSize);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 3] = new Vector2(tileIndex.x * tileSize, (tileIndex.y + 1) * tileSize);

        mesh.uv = uvs;
    }
    
    public void CreatePlane(Vector3 swCorner, float tileSize, float xWidth, float yHeight)
    {
        int tileGridWidth = Mathf.RoundToInt(xWidth / tileSize);
        int tileGridHeight = Mathf.RoundToInt(yHeight / tileSize);

        float[][] zeroGrid = new float[tileGridWidth][];
        for (int k = 0; k < tileGridWidth; k++)
        {
            zeroGrid[k] = new float[tileGridHeight];
        }

        CreatePlane(swCorner, tileSize, xWidth, yHeight, ref zeroGrid);

    }



    //void CreatePlane(int tileHeight, int tileWidth, int gridHeight, int gridWidth)
    public void CreatePlane(Vector3 swCorner, float tileSize, float xWidth, float yHeight, ref float[][] elevationsGrid)
    {
        #region prep work
        //var mesh = new Mesh();
        //var mf = GetComponent<MeshFilter>();
        //mf.GetComponent<Renderer>().material.SetTexture("_MainTex", Texture);
        //mf.mesh = mesh;
        counter = 0;
        Mesh[] meshList = new Mesh[meshSubsections];
        MeshFilter[] mfList = new MeshFilter[meshSubsections];

        if (meshHolders != null)
            foreach (GameObject obj in meshHolders)
            {
                //print("destroying");
                Destroy(obj);
            }
        //print("First two elevations: " + elevationsGrid[0][0] + ", " + elevationsGrid[0][1]);
        meshHolders = new GameObject[meshSubsections];

        List<Vector3>[] verticesList = new List<Vector3>[meshSubsections];
        List<int>[] trianglesList = new List<int>[meshSubsections];
        List<Vector3>[] normalsList = new List<Vector3>[meshSubsections];
        List<Vector2>[] uvsList = new List<Vector2>[meshSubsections];
        
        for (int i = 0; i < meshSubsections; i++)
        {
            meshHolders[i] = new GameObject("MeshHolder" + i.ToString());
            meshHolders[i].transform.position = Vector3.zero;
            meshHolders[i].transform.SetParent(this.transform);
            
            mfList[i] = meshHolders[i].AddComponent<MeshFilter>();
            meshHolders[i].AddComponent<MeshRenderer>();

            mfList[i].GetComponent<Renderer>().material.SetTexture("_MainTex", Texture);
            meshList[i] = new Mesh();
            mfList[i].mesh = meshList[i];


            verticesList[i] = new List<Vector3>();
            trianglesList[i] = new List<int>();
            normalsList[i] = new List<Vector3>();
            uvsList[i] = new List<Vector2>();
        }
        
        #endregion

        //print("Recieved width and height: " + xWidth + " x " + yHeight + ", tileSize: " + tileSize);

        int totalHorizontalTiles = Mathf.RoundToInt(xWidth / tileSize);
        int totalVerticalTiles = Mathf.RoundToInt(yHeight / tileSize);

        //for now, let's just fit input into a squared grid, will cause part of the grids to be flat (depending on how rectangular original dem is)
        int targetTilesNo = Mathf.Max(totalHorizontalTiles, totalVerticalTiles);
        int noOfTilesPerAxisPerSubsection = Mathf.FloorToInt(Mathf.Sqrt(maxTilesPerMesh)); //assumes squared subsections
        //int noOfSubsectionsInOneAxis = Mathf.Clamp(Mathf.RoundToInt( (float)targetTilesNo / (float)noOfTilesPerAxisPerSubsection), 1, Mathf.FloorToInt(Mathf.Sqrt(meshSubsections)));
        int noOfSubsectionsInOneAxis = Mathf.Clamp(Mathf.RoundToInt(targetTilesNo / noOfTilesPerAxisPerSubsection), 1, int.MaxValue);
        //print("targetTilesNo: " + targetTilesNo);
        //print("noOfTilesPerAxisPerSubsection: " + noOfTilesPerAxisPerSubsection);
        //print("noOfSubsectionsInOneAxis: " + noOfSubsectionsInOneAxis);

        #region meshing loop
        for (int k = 0; k < meshSubsections; k++) //coule've used counter...
        {

            float xLocalWidth = xWidth / (float) noOfSubsectionsInOneAxis;
            //float yLocalHeight = yHeight / (float)noOfSubsectionsInOneAxis;
            float yLocalHeight = xLocalWidth; //the above line causes issues with tiling, big seams between rows.

            int tileGridWidth = Mathf.RoundToInt(xLocalWidth / tileSize);
            int tileGridHeight = Mathf.RoundToInt(yLocalHeight / tileSize);
            //print("TileGridWidth:" + tileGridWidth);

            float[][] elevationsSubGrid = new float[tileGridWidth][];

            int xCellsToLocalSWCorner = ((counter % noOfSubsectionsInOneAxis ) * noOfTilesPerAxisPerSubsection);
            int yCellsToLocalSWCorner = (counter / noOfSubsectionsInOneAxis) * noOfTilesPerAxisPerSubsection;

            for (int i = 0; i < tileGridWidth; i++)
            {
                elevationsSubGrid[i] = new float[tileGridHeight];
                for (int j = 0; j < tileGridHeight; j++)
                {
                    int _x = xCellsToLocalSWCorner + i;
                    int _y = yCellsToLocalSWCorner + j;
                    if ( _x >= elevationsGrid.Length || _y >= elevationsGrid[0].Length)
                        elevationsSubGrid[i][j] = 0.0f;
                    else
                        elevationsSubGrid[i][j] = elevationsGrid[_x][_y];
                }
            }

            Vector3 localSWCorner = swCorner + new Vector3(xCellsToLocalSWCorner * tileSize,
                                                            0.0f,
                                                            yCellsToLocalSWCorner * tileSize);
            
            var index = 0;
            for (var x = 0; x < tileGridWidth; x++)
            {
                for (var y = 0; y < tileGridHeight; y++)
                {
                    float[] heights = GetVertsHeightPerQuad(x, y, tileGridWidth - 1, tileGridHeight - 1, elevationsSubGrid);
                    
                    AddVertices(localSWCorner, tileSize, tileSize, y, x, verticesList[counter], heights);
                    index = AddTriangles(index, trianglesList[counter]);
                    AddNormals(normalsList[counter]);
                    //AddUvs(defaultTileX, tileSize, tileSize, uvsList[counter], defaultTileY);
                    AddUvs(swCorner, localSWCorner, tileSize, x, y, xWidth, yHeight, uvsList[counter]);
                }
            }

            meshList[counter].vertices = verticesList[counter].ToArray();
            meshList[counter].normals = normalsList[counter].ToArray();
            meshList[counter].triangles = trianglesList[counter].ToArray();
            meshList[counter].uv = uvsList[counter].ToArray();
            meshList[counter].RecalculateNormals();

            counter++;
            if (counter > Mathf.Pow(noOfSubsectionsInOneAxis, 2.0f))
                break;
        }
        #endregion
    }
    
    float[] GetVertsHeightPerQuad(int pixelX, int pixelY, int maxX, int maxY, float[][] elevationGrid)
    {

        //Note: the order goes counter clockwise from SW corner.
        float[] heights = new float[4];

        //print("Attempting for pixelX: " + pixelX + ", pixelY: " + pixelY + ", with maxes: " + maxX + ", " + maxY);

        if (pixelX < 1) //left edge
        {
            if (pixelY < 1) //sw corner
                heights[0] = elevationGrid[pixelX][pixelY];
            else
                heights[0] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX][pixelY - 1]) / 2.0f;

            if (pixelY >= maxY) //nw corner
                heights[3] = elevationGrid[pixelX][pixelY];
            else
                heights[3] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX][pixelY + 1]) / 2.0f;
        }
        else
        {
            if (pixelY < 1) //bottom edge, no corners
                heights[0] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX - 1][pixelY]) / 2.0f;
            else //centre, should be most used.
                heights[0] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX-1][pixelY] + elevationGrid[pixelX][pixelY-1] + elevationGrid[pixelX-1][pixelY-1]) / 4.0f;

            if (pixelY >= maxY) //top edge, no corners
                heights[3] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX][pixelY - 1]) / 2.0f;
            else //centre, should be most used.
                heights[3] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX - 1][pixelY] + elevationGrid[pixelX ][pixelY+1] + elevationGrid[pixelX-1][pixelY+1]) / 4.0f;
        }


        if (pixelX >= maxX) //right edge
        {
            if (pixelY < 1) //se corner
                heights[1] = elevationGrid[pixelX][pixelY];
            else
                heights[1] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX][pixelY - 1]) / 2.0f;

            if (pixelY >= maxY) //ne corner
                heights[2] = elevationGrid[pixelX][pixelY];
            else
                heights[2] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX][pixelY +1]) / 2.0f;
        }
        else
        {
            if (pixelY < 1) //bottom edge, no corners
                heights[1] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY]) / 2.0f ;
            else //centre, should be most used.
                heights[1] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY] + elevationGrid[pixelX][pixelY - 1] + elevationGrid[pixelX + 1][pixelY - 1]) / 4.0f;

            if (pixelY >= maxY) //top edge, no corners
                heights[2] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY]) / 2.0f;
            else //centre, should be most used.
                heights[2] = (elevationGrid[pixelX][pixelY] + elevationGrid[pixelX + 1][pixelY] + elevationGrid[pixelX][pixelY + 1] + elevationGrid[pixelX + 1][pixelY + 1]) / 4.0f;
        }

        
        return heights;
    }

    //private static void AddVertices(Vector3 swCorner, float tileHeight, float tileWidth, int y, int x, ICollection<Vector3> vertices, float[] height)
    private void AddVertices(Vector3 swCorner, float tileHeight, float tileWidth, int y, int x, ICollection<Vector3> vertices, float[] height)
    {
        vertices.Add( this.transform.TransformPoint( swCorner + new Vector3(((float)x * tileWidth), height[0], ((float)y * tileHeight))));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth) + tileWidth, height[1], ((float)y * tileHeight))));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth) + tileWidth, height[2], ((float)y * tileHeight) + tileHeight)));
        vertices.Add(this.transform.TransformPoint(swCorner + new Vector3(((float)x * tileWidth), height[3], ((float)y * tileHeight) + tileHeight)));


        ////test
        //GameObject tempObj = Instantiate(this.gameObject.GetComponent<GroundCreator>().groundTile,
        //                                swCorner + new Vector3(((float)x * tileWidth), height[0], ((float)y * tileHeight)),
        //                                new Quaternion(),
        //                                this.transform);
        ////GameObject tempObj = Instantiate(groundTile, topoXYZ, new Quaternion(), this.transform);
        //tempObj.transform.localScale = new Vector3(tileWidth, tileWidth, tileWidth);

    }

    private static int AddTriangles(int index, ICollection<int> triangles)
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

    private static void AddNormals(ICollection<Vector3> normals)
    {
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
    }

    //private static void AddUvs(int tileRow, float tileSizeY, float tileSizeX, ICollection<Vector2> uvs, int tileColumn)
    private static void AddUvs(Vector3 swCorner, Vector3 localSWCorner, float tileSize, int x, int y, float totalWidth, float totalHeight, ICollection<Vector2> uvs)
    {
        //uvs.Add(new Vector2(tileColumn * tileSizeX, tileRow * tileSizeY));
        //uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, tileRow * tileSizeY));
        //uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, (tileRow + 1) * tileSizeY));
        //uvs.Add(new Vector2(tileColumn * tileSizeX, (tileRow + 1) * tileSizeY));

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

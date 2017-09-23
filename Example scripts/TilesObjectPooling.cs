using UnityEngine;

#if UNITYANDROID || UNITYSTANDALONE
using SimpleJSON;
#endif

public class TilesObjectPooling : MonoBehaviour
{
#if UNITYANDROID || UNITYSTANDALONE
    public bool transparent = true;
    public TextAsset levelToLoad;
    public bool setParent;
    public short heightOfTheMap = 0;
    public short widthOfTheMap = 0;
    public float zAxisOffset = 0.1f;
    Transform transform;
    byte cameraSize;
    float heightOfScreen;
    float widthOfScreen;
    Camera camera;
    Vector2 lastCameraPos;
    Transform camTrans;
    enum Horizontal : byte { Left, Right, None };
    enum Vertical : byte { Up, Down, None };
    bool first = true;
    short lastEndX;
    short lastEndY;
    short startX;
    short startY;
    short lastStartX;
    short lastStartY;
    short endY;
    short endX;

    byte[,] tilesPos;
    GameObject[,] tiles;
    bool[] poolOfThisTileIsEmpty;

    void Start()
    {
#if UNITYEDITOR
#else
        setParent = false;
#endif
        transform = GetComponent<Transform>();
        camera = Camera.main;
        if ( camera.orthographic )
        {
            cameraSize = (byte)camera.orthographicSize;
        }
        else
        {
            cameraSize = (byte)( camera.fieldOfView / 10 );
        }

        heightOfScreen = (float)( cameraSize * 2 );
        widthOfScreen = ( (float)( Screen.width ) / ( Screen.height ) ) * heightOfScreen;
        camTrans = Camera.main.transform;
        camera.GetComponent<BoxCollider2D>().size = new Vector2( widthOfScreen, heightOfScreen );
        poolOfThisTileIsEmpty = new bool[ DeadPoolHandler.DPHandler.prefabs.GetLength( 0 ) ];

        try
        {
            string jsonMap = levelToLoad.text;



            SimpleJSON.JSONNode parser = JSON.Parse(jsonMap);
            var LayersCount = parser["layers"].Count;
            var jsonTiles = parser["layers"][LayersCount - 1]["data"].AsArray;
            heightOfTheMap = (short)parser[ "layers" ][ LayersCount - 1 ][ "height" ].AsInt;
            widthOfTheMap = (short)parser[ "layers" ][ LayersCount - 1 ][ "width" ].AsInt;
            camera.GetComponent<MyOwnCamera>().maxX = (short)( widthOfTheMap - ( Mathf.Ceil( widthOfScreen / 2 ) + 2 ) - 0 );
            camera.GetComponent<MyOwnCamera>().maxY = (short)( ( ( heightOfTheMap - ( Mathf.Ceil( heightOfScreen / 2 ) + 1 ) ) * -1 ) + 0 );
            camera.GetComponent<MyOwnCamera>().minX = (short)( Mathf.Round( widthOfScreen / 2 ) + 0 );
            camera.GetComponent<MyOwnCamera>().minY = (short)( ( Mathf.Ceil( heightOfScreen / 2 ) * -1 ) - 0 );
            byte[,] tilesPos = new byte[heightOfTheMap, widthOfTheMap];


            short idOfLine = -1;
            short idOfColumn = widthOfTheMap;
            for ( int i = 0 ; i < jsonTiles.Count ; i++ ) // The highest layer is on the end of the list
            {
                ++idOfColumn;

                if ( idOfColumn >= widthOfTheMap )
                {
                    ++idOfLine;
                    idOfColumn = 0;
                }

                tilesPos[ idOfLine, idOfColumn ] = (byte)jsonTiles[ i ].AsInt;
            }
            for ( int z = LayersCount - 2 ; z >= 0 ; --z ) // Why there is -2 ? Because lower layers begins from before the last place
            {
                jsonTiles = parser[ "layers" ][ z ][ "data" ].AsArray;
                var heightOfTheMap = (short)parser["layers"][z]["height"].AsInt;
                var widthOfTheMap = (short)parser["layers"][z]["width"].AsInt;
                if ( widthOfTheMap > widthOfTheMap ) widthOfTheMap = widthOfTheMap;
                if ( heightOfTheMap > heightOfTheMap ) heightOfTheMap = heightOfTheMap;
                idOfLine = -1;
                idOfColumn = widthOfTheMap;

                for ( int i = 0 ; i < jsonTiles.Count ; i++ )
                {
                    ++idOfColumn;

                    if ( idOfColumn >= widthOfTheMap )
                    {
                        ++idOfLine;
                        if ( idOfLine > heightOfTheMap ) break;
                        idOfColumn = 0;
                    }

                    if ( tilesPos[ idOfLine, idOfColumn ] == 0 ) tilesPos[ idOfLine, idOfColumn ] = (byte)jsonTiles[ i ].AsInt;
                }
            }
            tilesPos = tilesPos;
            tiles = new GameObject[ heightOfTheMap, widthOfTheMap ];
            ObjectPooling();
            lastCameraPos = camTrans.position;
        }
        catch ( System.Exception )
        {
            Debug.Log( "Can't read json map file!" );
            throw;
        }
    }

    void LateUpdate()
    {
        if ( camTrans.position.x != lastCameraPos.x || camTrans.position.y != lastCameraPos.y )
        {
            if ( ObjectPooling() ) lastCameraPos = camTrans.position;
        }
    }

    bool ObjectPooling()
    {
        // Top left vertex of the screen
        startX = (short)Mathf.Round( camTrans.position.x - ( widthOfScreen / 2f ) );
        startY = (short)Mathf.Round( ( camTrans.position.y * -1f ) - ( heightOfScreen / 2 ) ); // Wysokość jest przeciwstawiana, bo współrzędne kamery są przeciwne do współrzędnych tablicy. Czyli pracujemy do góry nogami.

        // Bottom right vertex of the screen
        endY = (short)( startY + heightOfScreen );
        endX = (short)( startX + widthOfScreen + 1 );

        if ( startY < 0 ) return false; // We don't tolerate geting outside of the map
        else if ( startY >= heightOfTheMap ) return false;

        if ( endY < 0 ) return false;

        if ( startX < 0 ) return false;
        else if ( startX >= widthOfTheMap ) return false;

        if ( endX < 0 ) return false;

        if ( endY >= heightOfTheMap ) return false;

        if ( endX >= widthOfTheMap ) return false;

        // We need to create tiles on the full screen
        if ( first )
        {
            for ( short row = startY ; row <= endY ; ++row )
            {
                for ( short column = startX ; column <= endX ; ++column )
                {
                    var idOfTile = tilesPos[row, column];
                    if ( transparent )
                    {
                        if ( idOfTile == 0 ) continue;
                    }
                    if ( !poolOfThisTileIsEmpty[ idOfTile ] )
                    {
                        var count = DeadPoolHandler.DPHandler.pool[idOfTile].Count;
                        if ( count == 0 )
                        {
                            poolOfThisTileIsEmpty[ idOfTile ] = true;
                        }
                        else
                        {
                            tiles[ row, column ] = DeadPoolHandler.DPHandler.pool[ idOfTile ][ 0 ];
                            DeadPoolHandler.DPHandler.pool[ idOfTile ].RemoveAt( 0 );
                            tiles[ row, column ].transform.position = new Vector3( column, -row, zAxisOffset );

                            continue;
                        }
                    }

                    tiles[ row, column ] = Instantiate( DeadPoolHandler.DPHandler.prefabs[ idOfTile ], new Vector3( column, -row, zAxisOffset ), Quaternion.identity ) as GameObject;
#if UNITYEDITOR
                    if ( setParent ) tiles[ row, column ].GetComponent<Transform>().SetParent( transform );
#endif
                }
            }
            first = false;
            // We need last posistions to calculate which tiles to add or delete.
            lastEndX = endX;
            lastEndY = endY;
            lastStartX = startX;
            lastStartY = startY;
        }
        else
        {
            if ( endX != lastEndX || endY != lastEndY || startX != lastStartX || startY != lastStartY )
            {
                CheckDiffBetCameraPos( lastEndX, lastEndY, endX, endY );
                lastEndX = endX;
                lastEndY = endY;
                lastStartX = startX;
                lastStartY = startY;
            }
        }

        return true;
    }

    void CheckDiffBetCameraPos( short lastX, short lastY, short curX, short curY )
    {
        short diffY = 0;
        short diffX = 0;
        // Directions of move of camera
        Vertical ver = Vertical.None;
        Horizontal hor = Horizontal.None;

        // If camera goes down
        if ( curY > lastY )
        {
            diffY = CalculateDiffY( curY, lastY );
            ver = Vertical.Down; // We work upside down so if camera goes down in scene in arrays goes up.
        }
        // If camera goes up
        else if ( curY < lastY )
        {
            diffY = CalculateDiffY( lastY, curY );
            ver = Vertical.Up;
        }

        // If camera goes right
        if ( curX > lastX )
        {
            diffX = CalculateDiffX( curX, lastX );
            hor = Horizontal.Right;
        }
        // If camera goes left
        else if ( curX < lastX )
        {
            diffX = CalculateDiffX( lastX, curX );
            hor = Horizontal.Left;
        }
        CheckWhichTilesAdd( hor, ver, diffX, diffY );
    }

    short CalculateDiffY( short bigger, short lower )
    {
        var diffY = bigger - lower;
        if ( diffY > heightOfScreen + 1 ) diffY = (short)( heightOfScreen + 1 ); // We need "+1" because in condition of deleting tiles there is include. Creating tiles method doesn't have include in condition.
        return (short)diffY;
    }

    short CalculateDiffX( short bigger, short lower )
    {
        var diffX = bigger - lower;
        if ( diffX > widthOfScreen + 2 ) diffX = (short)( widthOfScreen + 2 ); // We need "+2" becouse camera move is smooth. Without "+2" there will be blank edges on screen edges
        return (short)diffX;
    }

    void CheckWhichTilesAdd( Horizontal horDir, Vertical verDir, short horDiff, short verDiff )
    {
        if ( verDir == Vertical.Down )
        {
            if ( horDir == Horizontal.Right )
            {
                DeleteOldTilesV2( verDiff, horDiff, false, false );
                AddNewTilesV2( verDiff, horDiff, true, true );
            }
            else
            {
                DeleteOldTilesV2( verDiff, horDiff, true, false );
                AddNewTilesV2( verDiff, horDiff, false, true );
            }
        }
        else
        {
            if ( horDir == Horizontal.Right )
            {
                DeleteOldTilesV2( verDiff, horDiff, false, true );
                AddNewTilesV2( verDiff, horDiff, true, false );
            }
            else
            {
                DeleteOldTilesV2( verDiff, horDiff, true, true );
                AddNewTilesV2( verDiff, horDiff, false, false );
            }
        }
    }

    struct FourShorts
    {
        public short short1;
        public short short2;
        public short short3;
        public short short4;

        public FourShorts( short short1, short short2, short short3, short short4 )
        {
            this.short1 = short1;
            this.short2 = short2;
            this.short3 = short3;
            this.short4 = short4;
        }
    }

    struct TwoShorts
    {
        public short short1;
        public short short2;

        public TwoShorts( short s1, short s2 )
        {
            short1 = s1;
            short2 = s2;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="diffY"></param>
    /// <param name="diffX"></param>
    /// <param name="verRight">Adding on the right ?</param>
    /// <param name="horDown">Adding at the bottom ?</param>
    void AddNewTilesV2( short diffY, short diffX, bool verRight, bool horDown )
    {
        FourShorts curStartXYEndXY = new FourShorts(startX, startY, endX, endY);
        TwoShorts startEndVerColumns = new TwoShorts(0, 0);

        if ( diffX != 0 )
        {
            startEndVerColumns = GiveStartEndVerColumn( diffX, verRight, new TwoShorts( curStartXYEndXY.short1, curStartXYEndXY.short3 ) );
            AddTiles( startEndVerColumns.short1, startEndVerColumns.short2, startY, (short)( endY + 1 ) );
        }

        if ( diffY != 0 )
        {
            FourShorts startEndHorColumnsAndStartEndHorRows = GiveStartEndHorColumnsAndStartEndHorRows(diffY, diffX, verRight, horDown, startEndVerColumns, curStartXYEndXY);
            AddTiles( startEndHorColumnsAndStartEndHorRows.short1,
                startEndHorColumnsAndStartEndHorRows.short2,
                startEndHorColumnsAndStartEndHorRows.short3,
                startEndHorColumnsAndStartEndHorRows.short4 );
        }
    }

    /// <summary>
    /// First we deleting whole colums and then whole rows, or partly.
    /// </summary>
    /// <param name="diffY"></param>
    /// <param name="diffX"></param>
    /// <param name="verRight">Adding on the right ?</param>
    /// <param name="horDown">Adding at the bottom ?</param>
    void DeleteOldTilesV2( short diffY, short diffX, bool verRight, bool horDown )
    {
        FourShorts lastStartXYEndXY = new FourShorts(lastStartX, lastStartY, lastEndX, lastEndY);
        TwoShorts startEndVerColumns = new TwoShorts(0,0);

        if ( diffX != 0 )
        {
            startEndVerColumns = GiveStartEndVerColumn( diffX, verRight, new TwoShorts( lastStartXYEndXY.short1, lastStartXYEndXY.short3 ) );
            DeleteTiles( startEndVerColumns.short1, startEndVerColumns.short2, lastStartY, (short)( lastEndY + 1 ) );
        }

        if ( diffY != 0 )
        {
            FourShorts startEndHorColumnsAndStartEndHorRows = GiveStartEndHorColumnsAndStartEndHorRows(diffY, diffX, verRight, horDown, startEndVerColumns, lastStartXYEndXY);
            DeleteTiles( startEndHorColumnsAndStartEndHorRows.short1,
                startEndHorColumnsAndStartEndHorRows.short2,
                startEndHorColumnsAndStartEndHorRows.short3,
                startEndHorColumnsAndStartEndHorRows.short4 );
        }
    }

    void AddTiles( short startColumn, short endColumn, short startRow, short endRow )
    {
        for ( short row = startRow ; row < endRow ; ++row )
        {
            for ( short column = startColumn ; column < endColumn ; ++column )
            {
                byte id = tilesPos[row, column];
                if ( transparent )
                {
                    if ( id == 0 ) continue;
                }

                if ( !poolOfThisTileIsEmpty[ id ] )
                {
                    var count = DeadPoolHandler.DPHandler.pool[id].Count;
                    if ( count == 0 )
                    {
                        poolOfThisTileIsEmpty[ id ] = true;
                    }
                    else
                    {
                        tiles[ row, column ] = DeadPoolHandler.DPHandler.pool[ id ][ 0 ];
                        DeadPoolHandler.DPHandler.pool[ id ].RemoveAt( 0 );
                        tiles[ row, column ].transform.position = new Vector3( column, -row, zAxisOffset );
                        continue;
                    }
                }
                tiles[ row, column ] = Instantiate( DeadPoolHandler.DPHandler.prefabs[ id ], new Vector3( column, -row, zAxisOffset ), Quaternion.identity ) as GameObject;
                if ( setParent ) tiles[ row, column ].GetComponent<Transform>().SetParent( transform );
            }
        }
    }

    void DeleteTiles( short startColumn, short endColumn, short startRow, short endRow )
    {
        for ( short row = startRow ; row < endRow ; ++row )
        {
            for ( short column = startColumn ; column < endColumn ; ++column )
            {
                var id = tilesPos[row, column];
                if ( transparent )
                    if ( id == 0 ) continue;
                

                if ( !tiles[ row, column ] ) { continue; }

                if ( poolOfThisTileIsEmpty[ id ] )
                    poolOfThisTileIsEmpty[ id ] = false;
                
                DeadPoolHandler.DPHandler.pool[ id ].Add( tiles[ row, column ].gameObject );

                tiles[ row, column ].transform.position = new Vector3( -10, 10, zAxisOffset );
            }
        }
    }

    FourShorts GiveStartEndHorColumnsAndStartEndHorRows( short diffY, short diffX, bool verRight, bool horDown, TwoShorts startEndVerColumns, FourShorts startXYEndXY )
    {
        FourShorts startEndHorColumnsAndStartEndHorRows = new FourShorts(0, 0, 0, 0);
        if ( horDown )
        {
            if ( diffX != 0 )
            {
                if ( verRight )
                {
                    startEndHorColumnsAndStartEndHorRows = new FourShorts( startXYEndXY.short1, startEndVerColumns.short1,// start, End Horizontal COLUMNS
                        (short)( startXYEndXY.short4 - diffY + 1 ), (short)( startXYEndXY.short4 + 1 ) );// start, End Horizontal ROWS
                }
                else
                {
                    startEndHorColumnsAndStartEndHorRows = new FourShorts(
                        (short)( startEndVerColumns.short2 ),
                        (short)( startXYEndXY.short3 + 1 ),
                        (short)( startXYEndXY.short4 - diffY + 1 ),
                        (short)( startXYEndXY.short4 + 1 ) ); 
                }
            }
            else
            {
                startEndHorColumnsAndStartEndHorRows = new FourShorts(
                    startXYEndXY.short1,
                    (short)( startXYEndXY.short3 + 1 ),
                    (short)( startXYEndXY.short4 - diffY + 1 ),
                    (short)( startXYEndXY.short4 + 1 ) );
            }
        }
        else
        {
            if ( diffX != 0 )
            {
                if ( verRight )
                {
                    startEndHorColumnsAndStartEndHorRows = new FourShorts( startXYEndXY.short1, startEndVerColumns.short1,
                        startXYEndXY.short2, (short)( startXYEndXY.short2 + diffY ) );
                }
                else
                {
                    startEndHorColumnsAndStartEndHorRows = new FourShorts( startEndVerColumns.short2, (short)( startXYEndXY.short3 + 1 ),
                        startXYEndXY.short2, (short)( startXYEndXY.short2 + diffY ) );
                }
            }
            else
            {
                startEndHorColumnsAndStartEndHorRows = new FourShorts( startXYEndXY.short1, (short)( startXYEndXY.short3 + 1 ),
                    startXYEndXY.short2, (short)( startXYEndXY.short2 + diffY ) );
            }
        }

        return startEndHorColumnsAndStartEndHorRows;
    }

    TwoShorts GiveStartEndVerColumn( short diffX, bool verRight, TwoShorts startXEndX )
    {
        TwoShorts startEndVerColumns;
        if ( verRight )
        {
            startEndVerColumns.short1 = (short)( startXEndX.short2 - diffX + 1 ); // start ; +1 because of inclusive in condition
            startEndVerColumns.short2 = (short)( startXEndX.short2 + 1 ); // end ; +1 because of exlusive in condition
        }
        else
        {
            startEndVerColumns.short1 = startXEndX.short1;
            startEndVerColumns.short2 = (short)( startXEndX.short1 + diffX ); // Over here is not need of +1
        }
        return startEndVerColumns;
    }

    void Print2DArray( byte[,] Array2D )
    {
        string txt = "";
        for ( int i = 0 ; i < Array2D.GetLength( 0 ) ; ++i )
        {
            for ( int o = 0 ; o < Array2D.GetLength( 1 ) ; ++o )
            {
                txt += Array2D[ i, o ].ToString() + ",";
                if ( o == Array2D.GetLength( 1 ) - 1 )
                {
                    txt += "\n";
                }
            }
        }
        print( txt );
    }
#endif
}

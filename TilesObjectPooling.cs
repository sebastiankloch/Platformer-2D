using UnityEngine;

#if UNITY_ANDROID || UNITY_STANDALONE
using SimpleJSON;
#endif

public class TilesObjectPooling : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_STANDALONE
    public bool _transparent = true;
    public TextAsset _levelToLoad;
    public bool _setParent;
    public short _heightOfTheMap = 0;
    public short _widthOfTheMap = 0;
    public float _zAxisOffset = 0.1f;
    Transform _transform;
    byte _cameraSize;
    float _heightOfScreen;
    float _widthOfScreen;
    Camera _camera;
    Vector2 _lastCameraPos;
    Transform _camTrans;
    enum _Horizontal : byte { Left, Right, None };
    enum _Vertical : byte { Up, Down, None };
    bool _first = true;
    short _lastEndX;
    short _lastEndY;
    short _startX;
    short _startY;
    short _lastStartX;
    short _lastStartY;
    short _endY;
    short _endX;

    byte[,] _tilesPos;
    GameObject[,] _tiles;
    bool[] _poolOfThisTileIsEmpty;

    void Start()
    {
#if UNITY_EDITOR
#else
        _setParent = false;
#endif
        _transform = GetComponent<Transform>();
        _camera = Camera.main;
        if ( _camera.orthographic )
        {
            _cameraSize = (byte)_camera.orthographicSize;
        }
        else
        {
            _cameraSize = (byte)( _camera.fieldOfView / 10 );
        }

        _heightOfScreen = (float)( _cameraSize * 2 );
        _widthOfScreen = ( (float)( Screen.width ) / ( Screen.height ) ) * _heightOfScreen;
        _camTrans = Camera.main.transform;
        _camera.GetComponent<BoxCollider2D>().size = new Vector2( _widthOfScreen, _heightOfScreen );
        _poolOfThisTileIsEmpty = new bool[ DeadPoolHandler._DP_Handler._prefabs.GetLength( 0 ) ];

        try
        {
            string jsonMap = _levelToLoad.text;



            SimpleJSON.JSONNode parser = JSON.Parse(jsonMap);
            var __LayersCount = parser["layers"].Count;
            var __jsonTiles = parser["layers"][__LayersCount - 1]["data"].AsArray;
            _heightOfTheMap = (short)parser[ "layers" ][ __LayersCount - 1 ][ "height" ].AsInt;
            _widthOfTheMap = (short)parser[ "layers" ][ __LayersCount - 1 ][ "width" ].AsInt;
            _camera.GetComponent<MyOwnCamera>()._maxX = (short)( _widthOfTheMap - ( Mathf.Ceil( _widthOfScreen / 2 ) + 2 ) - 0 );
            _camera.GetComponent<MyOwnCamera>()._maxY = (short)( ( ( _heightOfTheMap - ( Mathf.Ceil( _heightOfScreen / 2 ) + 1 ) ) * -1 ) + 0 );
            _camera.GetComponent<MyOwnCamera>()._minX = (short)( Mathf.Round( _widthOfScreen / 2 ) + 0 );
            _camera.GetComponent<MyOwnCamera>()._minY = (short)( ( Mathf.Ceil( _heightOfScreen / 2 ) * -1 ) - 0 );
            byte[,] __tilesPos = new byte[_heightOfTheMap, _widthOfTheMap];


            short __idOfLine = -1;
            short __idOfColumn = _widthOfTheMap;
            for ( int i = 0 ; i < __jsonTiles.Count ; i++ ) // The highest layer is on the end of the list
            {
                ++__idOfColumn;

                if ( __idOfColumn >= _widthOfTheMap )
                {
                    ++__idOfLine;
                    __idOfColumn = 0;
                }

                __tilesPos[ __idOfLine, __idOfColumn ] = (byte)__jsonTiles[ i ].AsInt;
            }
            for ( int z = __LayersCount - 2 ; z >= 0 ; --z ) // Why there is -2 ? Because lower layers begins from before the last place
            {
                __jsonTiles = parser[ "layers" ][ z ][ "data" ].AsArray;
                var __heightOfTheMap = (short)parser["layers"][z]["height"].AsInt;
                var __widthOfTheMap = (short)parser["layers"][z]["width"].AsInt;
                if ( __widthOfTheMap > _widthOfTheMap ) __widthOfTheMap = _widthOfTheMap;
                if ( __heightOfTheMap > _heightOfTheMap ) __heightOfTheMap = _heightOfTheMap;
                __idOfLine = -1;
                __idOfColumn = __widthOfTheMap;

                for ( int i = 0 ; i < __jsonTiles.Count ; i++ )
                {
                    ++__idOfColumn;

                    if ( __idOfColumn >= __widthOfTheMap )
                    {
                        ++__idOfLine;
                        if ( __idOfLine > __heightOfTheMap ) break;
                        __idOfColumn = 0;
                    }

                    if ( __tilesPos[ __idOfLine, __idOfColumn ] == 0 ) __tilesPos[ __idOfLine, __idOfColumn ] = (byte)__jsonTiles[ i ].AsInt;
                }
            }
            _tilesPos = __tilesPos;
            _tiles = new GameObject[ _heightOfTheMap, _widthOfTheMap ];
            _ObjectPooling();
            _lastCameraPos = _camTrans.position;
        }
        catch ( System.Exception )
        {
            Debug.Log( "Can't read json map file!" );
            throw;
        }
    }

    void LateUpdate()
    {
        if ( _camTrans.position.x != _lastCameraPos.x || _camTrans.position.y != _lastCameraPos.y )
        {
            if ( _ObjectPooling() ) _lastCameraPos = _camTrans.position;
        }
    }

    bool _ObjectPooling()
    {
        // Top left vertex of the screen
        _startX = (short)Mathf.Round( _camTrans.position.x - ( _widthOfScreen / 2f ) );
        _startY = (short)Mathf.Round( ( _camTrans.position.y * -1f ) - ( _heightOfScreen / 2 ) ); // Wysokość jest przeciwstawiana, bo współrzędne kamery są przeciwne do współrzędnych tablicy. Czyli pracujemy do góry nogami.

        // Bottom right vertex of the screen
        _endY = (short)( _startY + _heightOfScreen );
        _endX = (short)( _startX + _widthOfScreen + 1 );

        if ( _startY < 0 ) return false; // We don't tolerate geting outside of the map
        else if ( _startY >= _heightOfTheMap ) return false;

        if ( _endY < 0 ) return false;

        if ( _startX < 0 ) return false;
        else if ( _startX >= _widthOfTheMap ) return false;

        if ( _endX < 0 ) return false;

        if ( _endY >= _heightOfTheMap ) return false;

        if ( _endX >= _widthOfTheMap ) return false;

        // We need to create tiles on the full screen
        if ( _first )
        {
            for ( short row = _startY ; row <= _endY ; ++row )
            {
                for ( short column = _startX ; column <= _endX ; ++column )
                {
                    var __idOfTile = _tilesPos[row, column];
                    if ( _transparent )
                    {
                        if ( __idOfTile == 0 ) continue;
                    }
                    if ( !_poolOfThisTileIsEmpty[ __idOfTile ] )
                    {
                        var __count = DeadPoolHandler._DP_Handler._pool[__idOfTile].Count;
                        if ( __count == 0 )
                        {
                            _poolOfThisTileIsEmpty[ __idOfTile ] = true;
                        }
                        else
                        {
                            _tiles[ row, column ] = DeadPoolHandler._DP_Handler._pool[ __idOfTile ][ 0 ];
                            DeadPoolHandler._DP_Handler._pool[ __idOfTile ].RemoveAt( 0 );
                            _tiles[ row, column ].transform.position = new Vector3( column, -row, _zAxisOffset );

                            continue;
                        }
                    }

                    _tiles[ row, column ] = Instantiate( DeadPoolHandler._DP_Handler._prefabs[ __idOfTile ], new Vector3( column, -row, _zAxisOffset ), Quaternion.identity ) as GameObject;
#if UNITY_EDITOR
                    if ( _setParent ) _tiles[ row, column ].GetComponent<Transform>().SetParent( _transform );
#endif
                }
            }
            _first = false;
            // We need last posistions to calculate which tiles to add or delete.
            _lastEndX = _endX;
            _lastEndY = _endY;
            _lastStartX = _startX;
            _lastStartY = _startY;
        }
        else
        {
            if ( _endX != _lastEndX || _endY != _lastEndY || _startX != _lastStartX || _startY != _lastStartY )
            {
                _CheckDiffBetCameraPos( _lastEndX, _lastEndY, _endX, _endY );
                _lastEndX = _endX;
                _lastEndY = _endY;
                _lastStartX = _startX;
                _lastStartY = _startY;
            }
        }

        return true;
    }

    void _CheckDiffBetCameraPos( short __lastX, short __lastY, short __curX, short __curY )
    {
        short __diffY = 0;
        short __diffX = 0;
        // Directions of move of camera
        _Vertical __ver = _Vertical.None;
        _Horizontal __hor = _Horizontal.None;

        // If camera goes down
        if ( __curY > __lastY )
        {
            __diffY = _CalculateDiffY( __curY, __lastY );
            __ver = _Vertical.Down; // We work upside down so if camera goes down in scene in arrays goes up.
        }
        // If camera goes up
        else if ( __curY < __lastY )
        {
            __diffY = _CalculateDiffY( __lastY, __curY );
            __ver = _Vertical.Up;
        }

        // If camera goes right
        if ( __curX > __lastX )
        {
            __diffX = _CalculateDiffX( __curX, __lastX );
            __hor = _Horizontal.Right;
        }
        // If camera goes left
        else if ( __curX < __lastX )
        {
            __diffX = _CalculateDiffX( __lastX, __curX );
            __hor = _Horizontal.Left;
        }
        _CheckWhichTilesAdd( __hor, __ver, __diffX, __diffY );
    }

    short _CalculateDiffY( short __bigger, short __lower )
    {
        var __diffY = __bigger - __lower;
        if ( __diffY > _heightOfScreen + 1 ) __diffY = (short)( _heightOfScreen + 1 ); // We need "+1" because in condition of deleting tiles there is include. Creating tiles method doesn't have include in condition.
        return (short)__diffY;
    }

    short _CalculateDiffX( short __bigger, short __lower )
    {
        var __diffX = __bigger - __lower;
        if ( __diffX > _widthOfScreen + 2 ) __diffX = (short)( _widthOfScreen + 2 ); // We need "+2" becouse camera move is smooth. Without "+2" there will be blank edges on screen edges
        return (short)__diffX;
    }

    void _CheckWhichTilesAdd( _Horizontal __horDir, _Vertical __verDir, short __horDiff, short __verDiff )
    {
        if ( __verDir == _Vertical.Down )
        {
            if ( __horDir == _Horizontal.Right )
            {
                _DeleteOldTiles_V2( __verDiff, __horDiff, false, false );
                _AddNewTiles_V2( __verDiff, __horDiff, true, true );
            }
            else
            {
                _DeleteOldTiles_V2( __verDiff, __horDiff, true, false );
                _AddNewTiles_V2( __verDiff, __horDiff, false, true );
            }
        }
        else
        {
            if ( __horDir == _Horizontal.Right )
            {
                _DeleteOldTiles_V2( __verDiff, __horDiff, false, true );
                _AddNewTiles_V2( __verDiff, __horDiff, true, false );
            }
            else
            {
                _DeleteOldTiles_V2( __verDiff, __horDiff, true, true );
                _AddNewTiles_V2( __verDiff, __horDiff, false, false );
            }
        }
    }

    struct _FourShorts
    {
        public short short_1;
        public short short_2;
        public short short_3;
        public short short_4;

        public _FourShorts( short short_1, short short_2, short short_3, short short_4 )
        {
            this.short_1 = short_1;
            this.short_2 = short_2;
            this.short_3 = short_3;
            this.short_4 = short_4;
        }
    }

    struct _TwoShorts
    {
        public short short_1;
        public short short_2;

        public _TwoShorts( short s1, short s2 )
        {
            short_1 = s1;
            short_2 = s2;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="__diffY"></param>
    /// <param name="__diffX"></param>
    /// <param name="__verRight">Adding on the right ?</param>
    /// <param name="__horDown">Adding at the bottom ?</param>
    void _AddNewTiles_V2( short __diffY, short __diffX, bool __verRight, bool __horDown )
    {
        _FourShorts __cur_StartXY_EndXY = new _FourShorts(_startX, _startY, _endX, _endY);
        _TwoShorts __startEndVerColumns = new _TwoShorts(0, 0);

        if ( __diffX != 0 )
        {
            __startEndVerColumns = _GiveStartEndVerColumn( __diffX, __verRight, new _TwoShorts( __cur_StartXY_EndXY.short_1, __cur_StartXY_EndXY.short_3 ) );
            _AddTiles( __startEndVerColumns.short_1, __startEndVerColumns.short_2, _startY, (short)( _endY + 1 ) );
        }

        if ( __diffY != 0 )
        {
            _FourShorts __startEndHorColumns_And_StartEndHorRows = _GiveStartEndHorColumns_And_StartEndHorRows(__diffY, __diffX, __verRight, __horDown, __startEndVerColumns, __cur_StartXY_EndXY);
            _AddTiles( __startEndHorColumns_And_StartEndHorRows.short_1,
                __startEndHorColumns_And_StartEndHorRows.short_2,
                __startEndHorColumns_And_StartEndHorRows.short_3,
                __startEndHorColumns_And_StartEndHorRows.short_4 );
        }
    }

    /// <summary>
    /// First we deleting whole colums and then whole rows, or partly.
    /// </summary>
    /// <param name="__diffY"></param>
    /// <param name="__diffX"></param>
    /// <param name="__verRight">Adding on the right ?</param>
    /// <param name="__horDown">Adding at the bottom ?</param>
    void _DeleteOldTiles_V2( short __diffY, short __diffX, bool __verRight, bool __horDown )
    {
        _FourShorts __last_StartXY_EndXY = new _FourShorts(_lastStartX, _lastStartY, _lastEndX, _lastEndY);
        _TwoShorts __startEndVerColumns = new _TwoShorts(0,0);

        if ( __diffX != 0 )
        {
            __startEndVerColumns = _GiveStartEndVerColumn( __diffX, __verRight, new _TwoShorts( __last_StartXY_EndXY.short_1, __last_StartXY_EndXY.short_3 ) );
            _DeleteTiles( __startEndVerColumns.short_1, __startEndVerColumns.short_2, _lastStartY, (short)( _lastEndY + 1 ) );
        }

        if ( __diffY != 0 )
        {
            _FourShorts __startEndHorColumns_And_StartEndHorRows = _GiveStartEndHorColumns_And_StartEndHorRows(__diffY, __diffX, __verRight, __horDown, __startEndVerColumns, __last_StartXY_EndXY);
            _DeleteTiles( __startEndHorColumns_And_StartEndHorRows.short_1,
                __startEndHorColumns_And_StartEndHorRows.short_2,
                __startEndHorColumns_And_StartEndHorRows.short_3,
                __startEndHorColumns_And_StartEndHorRows.short_4 );
        }
    }

    void _AddTiles( short __startColumn, short __endColumn, short __startRow, short __endRow )
    {
        for ( short row = __startRow ; row < __endRow ; ++row )
        {
            for ( short column = __startColumn ; column < __endColumn ; ++column )
            {
                byte id = _tilesPos[row, column];
                if ( _transparent )
                {
                    if ( id == 0 ) continue;
                }

                if ( !_poolOfThisTileIsEmpty[ id ] )
                {
                    var __count = DeadPoolHandler._DP_Handler._pool[id].Count;
                    if ( __count == 0 )
                    {
                        _poolOfThisTileIsEmpty[ id ] = true;
                    }
                    else
                    {
                        _tiles[ row, column ] = DeadPoolHandler._DP_Handler._pool[ id ][ 0 ];
                        DeadPoolHandler._DP_Handler._pool[ id ].RemoveAt( 0 );
                        _tiles[ row, column ].transform.position = new Vector3( column, -row, _zAxisOffset );
                        continue;
                    }
                }
                _tiles[ row, column ] = Instantiate( DeadPoolHandler._DP_Handler._prefabs[ id ], new Vector3( column, -row, _zAxisOffset ), Quaternion.identity ) as GameObject;
                if ( _setParent ) _tiles[ row, column ].GetComponent<Transform>().SetParent( _transform );
            }
        }
    }

    void _DeleteTiles( short __startColumn, short __endColumn, short __startRow, short __endRow )
    {
        for ( short row = __startRow ; row < __endRow ; ++row )
        {
            for ( short column = __startColumn ; column < __endColumn ; ++column )
            {
                var id = _tilesPos[row, column];
                if ( _transparent )
                    if ( id == 0 ) continue;
                

                if ( !_tiles[ row, column ] ) { continue; }

                if ( _poolOfThisTileIsEmpty[ id ] )
                    _poolOfThisTileIsEmpty[ id ] = false;
                
                DeadPoolHandler._DP_Handler._pool[ id ].Add( _tiles[ row, column ].gameObject );

                _tiles[ row, column ].transform.position = new Vector3( -10, 10, _zAxisOffset );
            }
        }
    }

    _FourShorts _GiveStartEndHorColumns_And_StartEndHorRows( short __diffY, short __diffX, bool __verRight, bool __horDown, _TwoShorts __startEndVerColumns, _FourShorts __startXY_EndXY )
    {
        _FourShorts __startEndHorColumns_And_StartEndHorRows = new _FourShorts(0, 0, 0, 0);
        if ( __horDown )
        {
            if ( __diffX != 0 )
            {
                if ( __verRight )
                {
                    __startEndHorColumns_And_StartEndHorRows = new _FourShorts( __startXY_EndXY.short_1, __startEndVerColumns.short_1,// start, End Horizontal COLUMNS
                        (short)( __startXY_EndXY.short_4 - __diffY + 1 ), (short)( __startXY_EndXY.short_4 + 1 ) );// start, End Horizontal ROWS
                }
                else
                {
                    __startEndHorColumns_And_StartEndHorRows = new _FourShorts(
                        (short)( __startEndVerColumns.short_2 ),
                        (short)( __startXY_EndXY.short_3 + 1 ),
                        (short)( __startXY_EndXY.short_4 - __diffY + 1 ),
                        (short)( __startXY_EndXY.short_4 + 1 ) ); 
                }
            }
            else
            {
                __startEndHorColumns_And_StartEndHorRows = new _FourShorts(
                    __startXY_EndXY.short_1,
                    (short)( __startXY_EndXY.short_3 + 1 ),
                    (short)( __startXY_EndXY.short_4 - __diffY + 1 ),
                    (short)( __startXY_EndXY.short_4 + 1 ) );
            }
        }
        else
        {
            if ( __diffX != 0 )
            {
                if ( __verRight )
                {
                    __startEndHorColumns_And_StartEndHorRows = new _FourShorts( __startXY_EndXY.short_1, __startEndVerColumns.short_1,
                        __startXY_EndXY.short_2, (short)( __startXY_EndXY.short_2 + __diffY ) );
                }
                else
                {
                    __startEndHorColumns_And_StartEndHorRows = new _FourShorts( __startEndVerColumns.short_2, (short)( __startXY_EndXY.short_3 + 1 ),
                        __startXY_EndXY.short_2, (short)( __startXY_EndXY.short_2 + __diffY ) );
                }
            }
            else
            {
                __startEndHorColumns_And_StartEndHorRows = new _FourShorts( __startXY_EndXY.short_1, (short)( __startXY_EndXY.short_3 + 1 ),
                    __startXY_EndXY.short_2, (short)( __startXY_EndXY.short_2 + __diffY ) );
            }
        }

        return __startEndHorColumns_And_StartEndHorRows;
    }

    _TwoShorts _GiveStartEndVerColumn( short __diffX, bool __verRight, _TwoShorts __startX_EndX )
    {
        _TwoShorts __startEndVerColumns;
        if ( __verRight )
        {
            __startEndVerColumns.short_1 = (short)( __startX_EndX.short_2 - __diffX + 1 ); // start ; +1 because of inclusive in condition
            __startEndVerColumns.short_2 = (short)( __startX_EndX.short_2 + 1 ); // end ; +1 because of exlusive in condition
        }
        else
        {
            __startEndVerColumns.short_1 = __startX_EndX.short_1;
            __startEndVerColumns.short_2 = (short)( __startX_EndX.short_1 + __diffX ); // Over here is not need of +1
        }
        return __startEndVerColumns;
    }

    void _Print2D_Array( byte[,] __Array2D )
    {
        string __txt = "";
        for ( int i = 0 ; i < __Array2D.GetLength( 0 ) ; ++i )
        {
            for ( int o = 0 ; o < __Array2D.GetLength( 1 ) ; ++o )
            {
                __txt += __Array2D[ i, o ].ToString() + ",";
                if ( o == __Array2D.GetLength( 1 ) - 1 )
                {
                    __txt += "\n";
                }
            }
        }
        print( __txt );
    }
#endif
}

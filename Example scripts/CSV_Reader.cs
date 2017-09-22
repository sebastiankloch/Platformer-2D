using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using MyDataTypes;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class CSV_Reader : MonoBehaviour
{
    public TextAsset _CSV_File;
    public List<SlopeTile> _groundSlopeTiles = new List<SlopeTile>();
    public List<SlopeTile> _ceilingSlopeTiles = new List<SlopeTile>();
    public int _startId;
    public int _endId;
    public bool _fillGroundSlopes;
    public bool _fillCeilingSlopes;
    public bool _filled;
    List<Vector2> _groundSlopePairs;
    List<Vector2> _ceilingSlopePairs;

    [HideInInspector] public sbyte[,] _csvMap;

    SectorsHandler _secHan = new SectorsHandler();
    public CollidersOP _cOP;
    public static CSV_Reader _csv_Reader;

    void Awake()
    {
        if ( Application.isPlaying )
        {
            if ( !_cOP )
            {
                Debug.LogError( "_cOP = null" );
            }
            _csvMap = _GetValues();
            _SortSlopes();
            _secHan._csvMap = _csvMap;
            _secHan._GetSectorsStartPoints();

            _CreateGroundCollidersPos( _csvMap );
            _CreateCeilingCollidersPos( _csvMap );
            _CreateWallCollidersPos( _csvMap );
            _CreateEdgesPos( _csvMap );

            _cOP._sektors = _secHan._sektors;

            _csv_Reader = this;
        }

    }
#if UNITY_EDITOR
    void Update()
    {
        if( (_fillGroundSlopes || _fillCeilingSlopes) && !_filled )
        {
            if ( _fillGroundSlopes )
                _Fill( _groundSlopeTiles );
            else if ( _fillCeilingSlopes )
                _Fill( _ceilingSlopeTiles);
            
            _fillGroundSlopes = false;
            _fillCeilingSlopes = false;
            _filled = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    void _Fill(List<SlopeTile> __slopeTiles)
    {
        for(int id = _startId ; id <= _endId ; ++id )
        {
            __slopeTiles.Add( new SlopeTile(id) );
        }
    }
#endif

    void _PrintPos2(List<Pos2> __pos2)
    {
        for (int i = 0; i < __pos2.Count; ++i)
        {
            print(__pos2[i].first + " " + __pos2[i].second);
        }
    }

    void _CreateEdgesPos(sbyte[,] __2D_Array)
    {
        for (ushort q = 0; q < _secHan._sektors.GetLength(0); ++q)
        {
            for (ushort w = 0; w < _secHan._sektors.GetLength(1); ++w)
            {
                bool __rightEnd = false;
                var nr_y = q * _secHan._width_height;
                var y_end = nr_y + _secHan._width_height;
                if ( nr_y == 0 ) nr_y = 1;
                if (y_end > __2D_Array.GetLength(0) - 1) y_end = __2D_Array.GetLength(0) - 1;

                var nr_x = w * _secHan._width_height;
                var x_end = nr_x + _secHan._width_height;
                if ( nr_x == 0 ) nr_x = 1;
                if (x_end >= __2D_Array.GetLength(1)) { x_end = __2D_Array.GetLength(1); __rightEnd = true; }
           
                PosList2 pl2 = new PosList2(new List<Vector2>(), new List<Vector2>());

                float __correction = 0.625f;

                //Left edge
                if (__rightEnd) x_end -= 1;
                for (int y = nr_y; y < y_end; ++y)
                {
                    for ( int x = nr_x ; x < x_end ; ++x )
                    {
                        if ( __2D_Array[ y, x ] == -1 && __2D_Array[ y + 1, x + 1 ] != -1 && __2D_Array[ y, x + 1 ] == -1 && __2D_Array[ y + 1, x ] == -1 )
                        {
                            if ( _groundSlopeTiles.Count > 0 )
                            {
                                var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[y + 1, x + 1]);
                                if ( __det_slope._found )
                                    continue;
                            }

                            if ( _ceilingSlopeTiles.Count > 0 )
                            {
                                var __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ y + 1, x + 1 ] );
                                if ( __det_slope._found )
                                    continue;
                            }

                            pl2.v2_1.Add( new Vector2( x + __correction, ( y + __correction ) * -1 ) );
                        }
                    }
                }

                //Right edge
                if (__rightEnd) x_end += 1;
                if (w == 0) nr_x = 1;
                for (int i = nr_y; i < y_end; ++i)
                {
                    for (int z = nr_x; z < x_end; ++z)
                    {
                        if (__2D_Array[i, z] == -1 && __2D_Array[i + 1, z - 1] != -1 && __2D_Array[i, z - 1] == -1 && __2D_Array[i + 1, z] == -1)
                        {
                            var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[i + 1, z - 1]);
                            if ( __det_slope._found )
                                continue;

                            __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ i + 1, z - 1 ] );
                            if ( __det_slope._found )
                                continue;
                            pl2.v2_2.Add(new Vector2(z - __correction, (i + __correction) * -1));
                        }
                    }
                }
                _secHan._sektors[q, w]._edgesPos = pl2;
            }
        }
    }

    void _CreateWallCollidersPos(sbyte[,] __2D_Array)
    {
        for (ushort q = 0; q < _secHan._sektors.GetLength(0); ++q)
        {
            for (ushort w = 0; w < _secHan._sektors.GetLength(1); ++w)
            {
                bool __rightEnd = false;
                var nr_y = q * _secHan._width_height;
                
                var y_end = nr_y + _secHan._width_height;
                if ( nr_y == 0 ) nr_y = 1;
                if (y_end > __2D_Array.GetLength(0)) y_end = __2D_Array.GetLength(0);

                var nr_x = w * _secHan._width_height;
                
                var x_end = nr_x + _secHan._width_height;
                if ( nr_x == 0 ) nr_x = 1;
                if (x_end >= __2D_Array.GetLength(1)) { x_end = __2D_Array.GetLength(1); __rightEnd = true; }

                bool __checking = false;
                List<Pos2> __pos2 = new List<Pos2>();

                float __correctionHor = 0.625f;
                float __correctionVerCorner = 0.625f;
                float __correctionVerEdge = 0.375f;

                //Left Wall
                if (w == 0) nr_x += 1;
                for (int x = nr_x; x < x_end; ++x)
                {
                    for (int y = nr_y; y < y_end; ++y)
                    {
                        if (!__checking)
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if ( __2D_Array[ y, x - 1 ] != -1 )
                                {
                                    if ( _groundSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[y, x - 1]);
                                        if ( __det_slope._found )
                                            continue;
                                    }

                                    if ( _ceilingSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ y, x - 1 ] );

                                        if ( __det_slope._found )
                                            continue;
                                    }
                                    // First check. Creating of first vertex.
                                    __checking = true;
                                   
                                    if ( __2D_Array[ y - 1, x - 1 ] == -1 ) __pos2.Add( new Pos2( new Vector2( x - __correctionHor, ( y - __correctionVerEdge ) * -1 ), new Vector2( 0, 0 ) ) );//This is upper edge
                                    else __pos2.Add( new Pos2( new Vector2( x - __correctionHor, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) ); // ceiling
                                    //Checking if end of sector
                                    if ( y == y_end - 1 )
                                    {
                                        if ( y_end != __2D_Array.GetLength( 0 ) )
                                            if ( __2D_Array[ y + 1, x ] != -1 )
                                                __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x - __correctionHor, ( y + __correctionVerCorner ) * -1 ) );// left bottom corner
                                            else
                                                __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x - __correctionHor, ( y + 0.5f ) * -1 ) );
                                        else
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x - __correctionHor, ( y + 0.5f ) * -1 ) );
                                        __checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y, x - 1] != -1)
                                {
                                    if( _groundSlopeTiles.Count > 0)
                                    {
                                        var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[y, x - 1]);
                                        if ( __det_slope._found )
                                        {
                                            --y;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x - __correctionHor, ( y + 0.5f ) * -1 ) );
                                            __checking = false;
                                            continue;
                                        }
                                    }

                                    if ( _ceilingSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ y, x - 1 ] );

                                        if ( __det_slope._found )
                                        {
                                            --y;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x - __correctionHor, ( y + 0.5f ) * -1 ) );
                                            __checking = false;
                                            continue;
                                        }
                                    }
                                    
                                    __checking = true;
                                    //Checking if end of sector
                                    if (y == y_end - 1)
                                    {
                                        if (y_end != __2D_Array.GetLength(0))
                                            if (__2D_Array[y + 1, x] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x - __correctionHor, (y + __correctionVerCorner) * -1));// this is left bottom corner
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x - __correctionHor, (y + 0.5f) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x - __correctionHor, (y + 0.5f) * -1));
                                        __checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x - __correctionHor, (y + 0.5f - 1) * -1));
                                    __checking = false;
                                }
                            }
                            else
                            {
                                // When reaching corner
                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x - __correctionHor, (y + __correctionVerCorner - 1) * -1));
                                __checking = false;
                            }
                        }
                    }
                }
                // Left wall creating end

                //Right
                if (w == 0) nr_x -= 1;
                if (__rightEnd) x_end -= 1;
                for (int x = nr_x; x < x_end; ++x)
                {
                    for (int y = nr_y; y < y_end; ++y)
                    {
                        if (!__checking)
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y, x + 1] != -1)
                                {
                                    if ( _groundSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[y, x + 1]);
                                        if ( __det_slope._found )
                                            continue;
                                    }

                                    if ( _ceilingSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ y, x + 1 ] );

                                        if ( __det_slope._found )
                                            continue;
                                    }
                                    // First check. Creating of first vertex.

                                    __checking = true;

                                    if (__2D_Array[y - 1, x + 1] == -1) __pos2.Add(new Pos2(new Vector2(x + __correctionHor, (y - __correctionVerEdge) * -1), new Vector2(0, 0)));//This is upper edge
                                    else __pos2.Add(new Pos2(new Vector2(x + __correctionHor, (y - 0.5f) * -1), new Vector2(0, 0))); // sufit

                                    // Checking if end of sector
                                    if ( y == y_end - 1)
                                    {
                                        if (y_end != __2D_Array.GetLength(0))
                                            if (__2D_Array[y + 1, x] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + __correctionVerCorner) * -1));// this is left bottom corner
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + 0.5f) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + 0.5f) * -1));
                                        __checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y, x + 1] != -1)
                                {
                                    if ( _groundSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope(_groundSlopeTiles, __2D_Array[y, x + 1]);
                                        if ( __det_slope._found )
                                        {
                                            --y;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x + __correctionHor, ( y + 0.5f ) * -1 ) );
                                            __checking = false;
                                            continue;
                                        }
                                    }

                                    if ( _ceilingSlopeTiles.Count > 0 )
                                    {
                                        var __det_slope = _FindSlope( _ceilingSlopeTiles, __2D_Array[ y, x + 1 ] );
                                        if ( __det_slope._found )
                                        {
                                            --y;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x + __correctionHor, ( y + 0.5f ) * -1 ) );
                                            __checking = false;
                                            continue;
                                        }
                                    }

                                    // Checking if end of sector
                                    if ( y == y_end - 1)
                                    {
                                        if (y_end != __2D_Array.GetLength(0))
                                            if (__2D_Array[y + 1, x] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + __correctionVerCorner) * -1));// to jest lewy dolny narożnik
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + 0.5f) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + 0.5f) * -1));
                                        __checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    __pos2[ __pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + 0.5f - 1) * -1));
                                    __checking = false;
                                }
                            }
                            else
                            {
                                // When reaching corner
                                __pos2[ __pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHor, (y + __correctionVerCorner - 1) * -1));
                                __checking = false;
                            }
                        }
                    }
                }
                if (__pos2.Count > 0) _secHan._sektors[q, w]._wallPos = __pos2;
                // Right wall creating end
            }
        }
    }

    void _CreateGroundCollidersPos(sbyte[,] __2D_Array)
    {
        for (ushort q = 0; q < _secHan._sektors.GetLength(0); ++q)
        {
            for (ushort w = 0; w < _secHan._sektors.GetLength(1); ++w)
            {
                var nr_y = q * _secHan._width_height;
                var y_end = nr_y + _secHan._width_height;
                if ( nr_y == 0 ) nr_y = 1;
                if (y_end > __2D_Array.GetLength(0) - 1) y_end = __2D_Array.GetLength(0) - 1;

                var nr_x = w * _secHan._width_height;
                var x_end = nr_x + _secHan._width_height;
                if ( nr_x == 0 ) nr_x = 1;
                if (x_end > __2D_Array.GetLength(1)) x_end = __2D_Array.GetLength(1);

                float __correctionVer = 0.625f; //!IMPORTANT REMEMBER TO CHANGE VALUES IN IN FALLING GENERATOR IN DETECTING GROUND SECTION, WHEN CHANGES OCCURS
                float __correctionHorWall = 0.625f;
                float __correctionHorEdge = 0.375f;

                bool __checking = false;
                List<Pos2> __pos2 = new List<Pos2>();
                _groundSlopePairs = new List<Vector2>();

                //GROUND
                for (int y = nr_y; y < y_end; ++y)
                {
                    for (int x = nr_x; x < x_end; ++x)
                    {
                        if (!__checking)
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y + 1, x] != -1)
                                {
                                    if(_groundSlopeTiles.Count > 0 )
                                    {
                                        var __grSlope = _FindSlope(_groundSlopeTiles,__2D_Array[y + 1, x] );
                                        if ( __grSlope._found )
                                        {
                                            if ( __grSlope._slopeTile._isSpecial )
                                            {
                                                if ( __grSlope._slopeTile._isLeft )
                                                    if ( __grSlope._slopeTile._isBottom )
                                                    {
                                                        Vector2 __leftEndOfPair = new Vector2(x - 0.5f, (y + 1 + __correctionVer) * -1);
                                                        if ( !_CheckIfTherIsTheSamePair( _groundSlopePairs, __leftEndOfPair ) )
                                                        {
                                                            __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y + __correctionVer ) * -1 ) ) );
                                                            _groundSlopePairs.Add( __leftEndOfPair );
                                                            x += 1;
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Vector2 __leftEndOfPair = new Vector2(x - 2 - 0.5f, (y + 1 + __correctionVer) * -1);
                                                        if ( !_CheckIfTherIsTheSamePair( _groundSlopePairs, __leftEndOfPair ) )
                                                        {
                                                            __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 0.5f, ( y + __correctionVer ) * -1 ) ) );
                                                            _groundSlopePairs.Add( __leftEndOfPair );
                                                            continue;
                                                        }
                                                    }
                                                else if ( __grSlope._slopeTile._isBottom )
                                                {
                                                    Vector2 __leftEndOfPair = new Vector2(x - 2 - 0.5f, (y + __correctionVer) * -1);
                                                    if ( !_CheckIfTherIsTheSamePair( _groundSlopePairs, __leftEndOfPair ) )
                                                    {
                                                        __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 0.5f, ( y + 1 + __correctionVer ) * -1 ) ) );
                                                        _groundSlopePairs.Add( __leftEndOfPair );
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    Vector2 __leftEndOfPair = new Vector2(x - 0.5f, (y + __correctionVer) * -1);
                                                    if ( !_CheckIfTherIsTheSamePair( _groundSlopePairs, __leftEndOfPair ) )
                                                    {
                                                        __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y + 1 + __correctionVer ) * -1 ) ) );
                                                        _groundSlopePairs.Add( __leftEndOfPair );
                                                        x += 1;
                                                        continue;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                    
                                    

                                    // First check.
                                    __checking = true;
                                    if(x != 0) 
                                        if(__2D_Array[y, x - 1] != -1)
                                            __pos2.Add(new Pos2(new Vector2(x - __correctionHorWall, (y + __correctionVer) * -1), new Vector2(0, 0)));
                                        else if (__2D_Array[y + 1, x - 1] != -1)
                                            __pos2.Add(new Pos2(new Vector2(x - 0.5f, (y + __correctionVer) * -1), new Vector2(0, 0)));
                                        else
                                            __pos2.Add(new Pos2(new Vector2(x - __correctionHorEdge, (y + __correctionVer) * -1), new Vector2(0, 0)));
                                    else
                                        __pos2.Add(new Pos2(new Vector2(x - 0.5f, (y + __correctionVer) * -1), new Vector2(0, 0)));

                                    //Checking if end of sector.
                                    if (x == x_end - 1)
                                    {
                                        if (x_end != __2D_Array.GetLength(1))
                                            if (__2D_Array[y, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y + __correctionVer) * -1));
                                            else if (__2D_Array[y + 1, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y + __correctionVer) * -1));
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge, (y + __correctionVer) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y + __correctionVer) * -1));
                                        __checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y + 1, x] != -1)
                                {
                                    if ( _groundSlopeTiles.Count > 0 )
                                    {
                                        var __grSlope = _FindSlope(_groundSlopeTiles,__2D_Array[y + 1, x] );
                                        if ( __grSlope._found )
                                        {
                                            x--;
                                            __checking = false;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + __correctionVer ) * -1 ) );
                                            continue;
                                        }
                                    }


                                    //Checking if end of sector.
                                    if ( x == x_end - 1)
                                    {
                                        if (x_end != __2D_Array.GetLength(1))
                                            if (__2D_Array[y, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y + __correctionVer) * -1));
                                            else if(__2D_Array[y + 1, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y + __correctionVer) * -1));
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge, (y + __correctionVer) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y + __correctionVer) * -1));
                                        __checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    __pos2[ __pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge - 1, (y + __correctionVer) * -1));
                                    __checking = false;
                                }
                            }
                            else
                            {
                                // When reaching wall
                                __pos2[ __pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall - 1, (y + __correctionVer) * -1));
                                __checking = false;
                            }
                        }
                    }
                }
                if (__pos2.Count > 0) _secHan._sektors[q, w]._groundPos.AddRange(__pos2);

                // End Creating ground
            }
        }
    }


    void _CreateCeilingCollidersPos(sbyte[,] __2D_Array)
    {
        for (ushort q = 0; q < _secHan._sektors.GetLength(0); ++q)
        {
            for (ushort w = 0; w < _secHan._sektors.GetLength(1); ++w)
            {
                var nr_y = q * _secHan._width_height;
                var y_end = nr_y + _secHan._width_height;
                if ( nr_y == 0 ) nr_y = 1;
                if (y_end > __2D_Array.GetLength(0) - 1) y_end = __2D_Array.GetLength(0) - 1;

                var nr_x = w * _secHan._width_height;
                var x_end = nr_x + _secHan._width_height;
                if ( nr_x == 0 ) nr_x = 1;
                if (x_end > __2D_Array.GetLength(1)) x_end = __2D_Array.GetLength(1);

                float __correctionHorWall = 0.625f;
                float __correctionHorEdge = 0.375f;

                bool __checking = false;
                List<Pos2> __pos2 = new List<Pos2>();
                
                // Ceiling
                __checking = false;
                __pos2.RemoveRange(0, __pos2.Count);
                _ceilingSlopePairs = new List<Vector2>();

                if (nr_y == 0) ++nr_y;

                for (int y = nr_y; y < y_end; ++y)
                {
                    for (int x = nr_x; x < x_end; ++x)
                    {
                        if (!__checking)
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y - 1, x] != -1)
                                {
                                    if( _ceilingSlopeTiles.Count > 0)
                                    {
                                        var __ceSlope = _FindSlope(_ceilingSlopeTiles, __2D_Array[y - 1, x] );
                                        if ( __ceSlope._found )
                                        {
                                            if ( __ceSlope._slopeTile._isSpecial )
                                            {
                                                if ( __ceSlope._slopeTile._isLeft )
                                                    if ( __ceSlope._slopeTile._isBottom )
                                                    {
                                                        Vector2 __leftEndOfPair = new Vector2(x - 0.5f, (y - 0.5f) * -1);
                                                        if ( !_CheckIfTherIsTheSamePair( _ceilingSlopePairs, __leftEndOfPair ) )
                                                        {
                                                            __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y - 1.5f ) * -1 ) ) );
                                                            _ceilingSlopePairs.Add( __leftEndOfPair );

                                                            x += 1;
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Vector2 __leftEndOfPair = new Vector2(x - 2 - 0.5f, (y - 0.5f) * -1);
                                                        if ( !_CheckIfTherIsTheSamePair( _ceilingSlopePairs, __leftEndOfPair ) )
                                                        {
                                                            __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 0.5f, ( y - 1.5f ) * -1 ) ) );
                                                            _ceilingSlopePairs.Add( __leftEndOfPair );
                                                            continue;
                                                        }
                                                    }
                                                else if ( __ceSlope._slopeTile._isBottom )
                                                {
                                                    Vector2 __leftEndOfPair = new Vector2(x - 2 - 0.5f, (y - 1.5f ) * -1);
                                                    if ( !_CheckIfTherIsTheSamePair( _ceilingSlopePairs, __leftEndOfPair ) )
                                                    {
                                                        __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) ) );
                                                        _ceilingSlopePairs.Add( __leftEndOfPair );
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    Vector2 __leftEndOfPair = new Vector2(x - 0.5f, (y - 1.5f) * -1);
                                                    if ( !_CheckIfTherIsTheSamePair( _ceilingSlopePairs, __leftEndOfPair ) )
                                                    {
                                                        __pos2.Add( new Pos2( __leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y - 0.5f ) * -1 ) ) );
                                                        _ceilingSlopePairs.Add( __leftEndOfPair );
                                                        x += 1;
                                                        continue;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                    
                                    // First check
                                    __checking = true;
                                    if (x != 0)
                                        if (__2D_Array[y, x - 1] == -1 && __2D_Array[y - 1, x - 1] != -1 ) // If there isn't tile on the left and if on top left there is tile
                                            __pos2.Add(new Pos2(new Vector2(x - 0.5f, (y - 0.5f) * -1), new Vector2(0, 0)));
                                        else if (__2D_Array[y, x - 1] != -1)
                                            __pos2.Add(new Pos2(new Vector2(x - __correctionHorWall, (y - 0.5f) * -1), new Vector2(0, 0)));
                                        else
                                            __pos2.Add(new Pos2(new Vector2(x - __correctionHorEdge, (y - 0.5f) * -1), new Vector2(0, 0)));
                                    else
                                        __pos2.Add(new Pos2(new Vector2(x - __correctionHorWall, (y - 0.5f) * -1), new Vector2(0, 0)));
                                    
                                    //Checking if end
                                    if (x == x_end - 1)
                                    {
                                        if (x_end != __2D_Array.GetLength(1))
                                            if (__2D_Array[y, x + 1] == -1 && __2D_Array[y - 1, x + 1] != -1)// If there isn't tile on the right and if there is tile on top right;
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y - 0.5f) * -1));
                                            else if (__2D_Array[y, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y - 0.5f) * -1));
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge, (y - 0.5f) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y - 0.5f) * -1));

                                        __checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (__2D_Array[y, x] == -1)
                            {
                                if (__2D_Array[y - 1, x] != -1)
                                {
                                    if(_ceilingSlopeTiles.Count > 0 )
                                    {
                                        var __ceSlope = _FindSlope(_ceilingSlopeTiles, __2D_Array[y - 1, x] );
                                        if ( __ceSlope._found )
                                        {
                                            --x;
                                            __checking = false;
                                            __pos2[ __pos2.Count - 1 ] = new Pos2( __pos2[ __pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) );

                                            continue;
                                        }
                                    }
                                  
                                    // Checking if end
                                    if (x == x_end - 1)
                                    {
                                        if (x_end != __2D_Array.GetLength(1))
                                            if (__2D_Array[y, x + 1] == -1 && __2D_Array[y - 1, x + 1] != -1 )// If there isn't tile on the right and if there is tile on top right;
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + 0.5f, (y - 0.5f) * -1));
                                            else if (__2D_Array[y, x + 1] != -1)
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y - 0.5f) * -1));
                                            else
                                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge, (y - 0.5f) * -1));
                                        else
                                            __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall, (y - 0.5f) * -1));

                                        __checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorEdge - 1, (y - 0.5f) * -1));

                                    __checking = false;
                                }
                            }
                            else
                            {
                                // When reaching wall
                                __pos2[__pos2.Count - 1] = new Pos2(__pos2[__pos2.Count - 1].first, new Vector2(x + __correctionHorWall - 1, (y - 0.5f) * -1));
                                __checking = false;
                            }
                        }
                    }
                }
                if (__pos2.Count > 0) _secHan._sektors[q, w]._ceilingPos.AddRange(__pos2);
                // End of creating ceiling.
            }
        }
    }

    public sbyte[,] _GetValues( )
    {
        var __lines = _CSV_File.text.Split('\n');
        sbyte[,] __csvMap = new sbyte[__lines.GetLength(0), __lines[0].Split(',').GetLength(0)];
        for (int y = 0; y < __lines.GetLength(0); ++y)
        {
            string[] __values = __lines[y].Split(',');
            for (int x = 0; x < __values.GetLength(0); ++x)
            {
                sbyte.TryParse(__values[x], out __csvMap[ y, x]);
            }
        }
        return __csvMap;
    }

    bool _CheckIfTherIsTheSamePair( List<Vector2> _pairsList, Vector2 __leftEndOfPair )
    {
        for ( int id = 0 ; id < _pairsList.Count ; id++ )
        {
            if ( _pairsList[ id ].x == __leftEndOfPair.x && _pairsList[ id ].y == __leftEndOfPair.y) return true;
        }

        return false;
    }

    BoolSlopeTile _FindSlope( List<SlopeTile> __slopeTiles , float id)
    {
        int l, p, s;
        l = s = 0;
        p = __slopeTiles.Count - 1;
        
        if ( p < 0 )  return new BoolSlopeTile(false); 

        while ( l <= p )
        {
            s = ( l + p ) / 2;
            
            if ( __slopeTiles[ s ]._id == id )   
                return new BoolSlopeTile(true, __slopeTiles[ s ]);

            if ( __slopeTiles[ s ]._id < id )
                l = s + 1;
            else
                p = s - 1;
        }
        return new BoolSlopeTile( false );
    }

    void _SortSlopes()
    {
        _SortSlopes_inside( _groundSlopeTiles, 0, _groundSlopeTiles.Count - 1 );
        _SortSlopes_inside( _ceilingSlopeTiles, 0, _ceilingSlopeTiles.Count - 1 );
    }

    void _SortSlopes_inside( List<SlopeTile> __slopeTiles, int left, int right )
    {
        if(__slopeTiles.Count > 0 )
        {
            var i = left;
            var j = right;
            var pivot = __slopeTiles[(left + right) / 2]._id;

            while ( i < j )
            {
                while ( __slopeTiles[ i ]._id < pivot ) i++;
                while ( __slopeTiles[ j ]._id > pivot ) j--;
                if ( i <= j )
                {
                    var tmp = __slopeTiles[i];
                    __slopeTiles[ i++ ] = __slopeTiles[ j ];
                    __slopeTiles[ j-- ] = tmp;
                }
            }

            if ( left < j ) _SortSlopes_inside( __slopeTiles, left, j );
            if ( i < right ) _SortSlopes_inside( __slopeTiles, i, right );
        }
    }
}

[System.Serializable]
public struct SlopeTile
{
    public int _id;
    public bool _isSpecial;
    public bool _isLeft;
    public bool _isBottom;

    public SlopeTile( int id ) : this()
    {
        _id = id;
    }

    public SlopeTile( int id, bool isSpecial, bool isLeft, bool isBottom )
    {
        _id = id;
        _isSpecial = isSpecial;
        _isLeft = isLeft;
        _isBottom = isBottom;
    }
}

public struct BoolSlopeTile
{
    public bool _found;
    public SlopeTile _slopeTile;

    public BoolSlopeTile( bool found ) : this()
    {
        _found = found;
    }

    public BoolSlopeTile( bool found, SlopeTile slopeTile )
    {
        _found = found;
        _slopeTile = slopeTile;
    }
}

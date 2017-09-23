using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using MyDataTypes;
#if UNITYEDITOR
[ExecuteInEditMode]
#endif
public class CSVReader : MonoBehaviour
{
    public TextAsset CSVFile;
    public List<SlopeTile> groundSlopeTiles = new List<SlopeTile>();
    public List<SlopeTile> ceilingSlopeTiles = new List<SlopeTile>();
    public int startId;
    public int endId;
    public bool fillGroundSlopes;
    public bool fillCeilingSlopes;
    public bool filled;
    List<Vector2> groundSlopePairs;
    List<Vector2> ceilingSlopePairs;

    [HideInInspector] public sbyte[,] csvMap;

    SectorsHandler secHan = new SectorsHandler();
    public CollidersOP cOP;
    public static CSVReader csvReader;

    void Awake()
    {
        if ( Application.isPlaying )
        {
            if ( !cOP )
            {
                Debug.LogError( "cOP = null" );
            }
            csvMap = GetValues();
            SortSlopes();
            secHan.csvMap = csvMap;
            secHan.GetSectorsStartPoints();

            CreateGroundCollidersPos( csvMap );
            CreateCeilingCollidersPos( csvMap );
            CreateWallCollidersPos( csvMap );
            CreateEdgesPos( csvMap );

            cOP.sektors = secHan.sektors;

            csvReader = this;
        }

    }
#if UNITYEDITOR
    void Update()
    {
        if( (fillGroundSlopes || fillCeilingSlopes) && !filled )
        {
            if ( fillGroundSlopes )
                Fill( groundSlopeTiles );
            else if ( fillCeilingSlopes )
                Fill( ceilingSlopeTiles);
            
            fillGroundSlopes = false;
            fillCeilingSlopes = false;
            filled = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    void Fill(List<SlopeTile> slopeTiles)
    {
        for(int id = startId ; id <= endId ; ++id )
        {
            slopeTiles.Add( new SlopeTile(id) );
        }
    }
#endif

    void PrintPos2( List<Pos2> pos2 )
    {
        for ( int i = 0 ; i < pos2.Count ; ++i )
        {
            print( pos2[ i ].first + " " + pos2[ i ].second );
        }
    }

    void CreateEdgesPos( sbyte[,] 2DArray )
    {
        for ( ushort q = 0 ; q < secHan.sektors.GetLength( 0 ) ; ++q )
        {
            for ( ushort w = 0 ; w < secHan.sektors.GetLength( 1 ) ; ++w )
            {
                bool rightEnd = false;
                var nry = q * secHan.widthheight;
                var yend = nry + secHan.widthheight;
                if ( nry == 0 ) nry = 1;
                if ( yend > 2DArray.GetLength( 0 ) - 1 ) yend = 2DArray.GetLength( 0 ) - 1;

                var nrx = w * secHan.widthheight;
                var xend = nrx + secHan.widthheight;
                if ( nrx == 0 ) nrx = 1;
                if ( xend >= 2DArray.GetLength( 1 ) ) { xend = 2DArray.GetLength( 1 ); rightEnd = true; }

                PosList2 pl2 = new PosList2(new List<Vector2>(), new List<Vector2>());

                float correction = 0.625f;

                //Left edge
                if ( rightEnd ) xend -= 1;
                for ( int y = nry ; y < yend ; ++y )
                {
                    for ( int x = nrx ; x < xend ; ++x )
                    {
                        if ( 2DArray[ y, x ] == -1 && 2DArray[ y + 1, x + 1 ] != -1 && 2DArray[ y, x + 1 ] == -1 && 2DArray[ y + 1, x ] == -1 )
                        {
                            if ( groundSlopeTiles.Count > 0 )
                            {
                                var detslope = FindSlope(groundSlopeTiles, 2DArray[y + 1, x + 1]);
                                if ( detslope.found )
                                    continue;
                            }

                            if ( ceilingSlopeTiles.Count > 0 )
                            {
                                var detslope = FindSlope( ceilingSlopeTiles, 2DArray[ y + 1, x + 1 ] );
                                if ( detslope.found )
                                    continue;
                            }

                            pl2.v21.Add( new Vector2( x + correction, ( y + correction ) * -1 ) );
                        }
                    }
                }

                //Right edge
                if ( rightEnd ) xend += 1;
                if ( w == 0 ) nrx = 1;
                for ( int i = nry ; i < yend ; ++i )
                {
                    for ( int z = nrx ; z < xend ; ++z )
                    {
                        if ( 2DArray[ i, z ] == -1 && 2DArray[ i + 1, z - 1 ] != -1 && 2DArray[ i, z - 1 ] == -1 && 2DArray[ i + 1, z ] == -1 )
                        {
                            var detslope = FindSlope(groundSlopeTiles, 2DArray[i + 1, z - 1]);
                            if ( detslope.found )
                                continue;

                            detslope = FindSlope( ceilingSlopeTiles, 2DArray[ i + 1, z - 1 ] );
                            if ( detslope.found )
                                continue;
                            pl2.v22.Add( new Vector2( z - correction, ( i + correction ) * -1 ) );
                        }
                    }
                }
                secHan.sektors[ q, w ].edgesPos = pl2;
            }
        }
    }

    void CreateWallCollidersPos( sbyte[,] 2DArray )
    {
        for ( ushort q = 0 ; q < secHan.sektors.GetLength( 0 ) ; ++q )
        {
            for ( ushort w = 0 ; w < secHan.sektors.GetLength( 1 ) ; ++w )
            {
                bool rightEnd = false;
                var nry = q * secHan.widthheight;

                var yend = nry + secHan.widthheight;
                if ( nry == 0 ) nry = 1;
                if ( yend > 2DArray.GetLength( 0 ) ) yend = 2DArray.GetLength( 0 );

                var nrx = w * secHan.widthheight;

                var xend = nrx + secHan.widthheight;
                if ( nrx == 0 ) nrx = 1;
                if ( xend >= 2DArray.GetLength( 1 ) ) { xend = 2DArray.GetLength( 1 ); rightEnd = true; }

                bool checking = false;
                List<Pos2> pos2 = new List<Pos2>();

                float correctionHor = 0.625f;
                float correctionVerCorner = 0.625f;
                float correctionVerEdge = 0.375f;

                //Left Wall
                if ( w == 0 ) nrx += 1;
                for ( int x = nrx ; x < xend ; ++x )
                {
                    for ( int y = nry ; y < yend ; ++y )
                    {
                        if ( !checking )
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y, x - 1 ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope(groundSlopeTiles, 2DArray[y, x - 1]);
                                        if ( detslope.found )
                                            continue;
                                    }

                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope( ceilingSlopeTiles, 2DArray[ y, x - 1 ] );

                                        if ( detslope.found )
                                            continue;
                                    }
                                    // First check. Creating of first vertex.
                                    checking = true;

                                    if ( 2DArray[ y - 1, x - 1 ] == -1 ) pos2.Add( new Pos2( new Vector2( x - correctionHor, ( y - correctionVerEdge ) * -1 ), new Vector2( 0, 0 ) ) );//This is upper edge
                                    else pos2.Add( new Pos2( new Vector2( x - correctionHor, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) ); // ceiling
                                    //Checking if end of sector
                                    if ( y == yend - 1 )
                                    {
                                        if ( yend != 2DArray.GetLength( 0 ) )
                                            if ( 2DArray[ y + 1, x ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + correctionVerCorner ) * -1 ) );// left bottom corner
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                        checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y, x - 1 ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope(groundSlopeTiles, 2DArray[y, x - 1]);
                                        if ( detslope.found )
                                        {
                                            --y;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                            checking = false;
                                            continue;
                                        }
                                    }

                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope( ceilingSlopeTiles, 2DArray[ y, x - 1 ] );

                                        if ( detslope.found )
                                        {
                                            --y;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                            checking = false;
                                            continue;
                                        }
                                    }

                                    checking = true;
                                    //Checking if end of sector
                                    if ( y == yend - 1 )
                                    {
                                        if ( yend != 2DArray.GetLength( 0 ) )
                                            if ( 2DArray[ y + 1, x ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + correctionVerCorner ) * -1 ) );// this is left bottom corner
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f ) * -1 ) );
                                        checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + 0.5f - 1 ) * -1 ) );
                                    checking = false;
                                }
                            }
                            else
                            {
                                // When reaching corner
                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x - correctionHor, ( y + correctionVerCorner - 1 ) * -1 ) );
                                checking = false;
                            }
                        }
                    }
                }
                // Left wall creating end

                //Right
                if ( w == 0 ) nrx -= 1;
                if ( rightEnd ) xend -= 1;
                for ( int x = nrx ; x < xend ; ++x )
                {
                    for ( int y = nry ; y < yend ; ++y )
                    {
                        if ( !checking )
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y, x + 1 ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope(groundSlopeTiles, 2DArray[y, x + 1]);
                                        if ( detslope.found )
                                            continue;
                                    }

                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope( ceilingSlopeTiles, 2DArray[ y, x + 1 ] );

                                        if ( detslope.found )
                                            continue;
                                    }
                                    // First check. Creating of first vertex.

                                    checking = true;

                                    if ( 2DArray[ y - 1, x + 1 ] == -1 ) pos2.Add( new Pos2( new Vector2( x + correctionHor, ( y - correctionVerEdge ) * -1 ), new Vector2( 0, 0 ) ) );//This is upper edge
                                    else pos2.Add( new Pos2( new Vector2( x + correctionHor, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) ); // sufit

                                    // Checking if end of sector
                                    if ( y == yend - 1 )
                                    {
                                        if ( yend != 2DArray.GetLength( 0 ) )
                                            if ( 2DArray[ y + 1, x ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + correctionVerCorner ) * -1 ) );// this is left bottom corner
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                        checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y, x + 1 ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope(groundSlopeTiles, 2DArray[y, x + 1]);
                                        if ( detslope.found )
                                        {
                                            --y;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                            checking = false;
                                            continue;
                                        }
                                    }

                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var detslope = FindSlope( ceilingSlopeTiles, 2DArray[ y, x + 1 ] );
                                        if ( detslope.found )
                                        {
                                            --y;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                            checking = false;
                                            continue;
                                        }
                                    }

                                    // Checking if end of sector
                                    if ( y == yend - 1 )
                                    {
                                        if ( yend != 2DArray.GetLength( 0 ) )
                                            if ( 2DArray[ y + 1, x ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + correctionVerCorner ) * -1 ) );// to jest lewy dolny narożnik
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f ) * -1 ) );
                                        checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + 0.5f - 1 ) * -1 ) );
                                    checking = false;
                                }
                            }
                            else
                            {
                                // When reaching corner
                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHor, ( y + correctionVerCorner - 1 ) * -1 ) );
                                checking = false;
                            }
                        }
                    }
                }
                if ( pos2.Count > 0 ) secHan.sektors[ q, w ].wallPos = pos2;
                // Right wall creating end
            }
        }
    }

    void CreateGroundCollidersPos( sbyte[,] 2DArray )
    {
        for ( ushort q = 0 ; q < secHan.sektors.GetLength( 0 ) ; ++q )
        {
            for ( ushort w = 0 ; w < secHan.sektors.GetLength( 1 ) ; ++w )
            {
                var nry = q * secHan.widthheight;
                var yend = nry + secHan.widthheight;
                if ( nry == 0 ) nry = 1;
                if ( yend > 2DArray.GetLength( 0 ) - 1 ) yend = 2DArray.GetLength( 0 ) - 1;

                var nrx = w * secHan.widthheight;
                var xend = nrx + secHan.widthheight;
                if ( nrx == 0 ) nrx = 1;
                if ( xend > 2DArray.GetLength( 1 ) ) xend = 2DArray.GetLength( 1 );

                float correctionVer = 0.625f; //!IMPORTANT REMEMBER TO CHANGE VALUES IN IN FALLING GENERATOR IN DETECTING GROUND SECTION, WHEN CHANGES OCCURS
                float correctionHorWall = 0.625f;
                float correctionHorEdge = 0.375f;

                bool checking = false;
                List<Pos2> pos2 = new List<Pos2>();
                groundSlopePairs = new List<Vector2>();

                //GROUND
                for ( int y = nry ; y < yend ; ++y )
                {
                    for ( int x = nrx ; x < xend ; ++x )
                    {
                        if ( !checking )
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y + 1, x ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var grSlope = FindSlope(groundSlopeTiles,2DArray[y + 1, x] );
                                        if ( grSlope.found )
                                        {
                                            if ( grSlope.slopeTile.isSpecial )
                                            {
                                                if ( grSlope.slopeTile.isLeft )
                                                    if ( grSlope.slopeTile.isBottom )
                                                    {
                                                        Vector2 leftEndOfPair = new Vector2(x - 0.5f, (y + 1 + correctionVer) * -1);
                                                        if ( !CheckIfTherIsTheSamePair( groundSlopePairs, leftEndOfPair ) )
                                                        {
                                                            pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y + correctionVer ) * -1 ) ) );
                                                            groundSlopePairs.Add( leftEndOfPair );
                                                            x += 1;
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Vector2 leftEndOfPair = new Vector2(x - 2 - 0.5f, (y + 1 + correctionVer) * -1);
                                                        if ( !CheckIfTherIsTheSamePair( groundSlopePairs, leftEndOfPair ) )
                                                        {
                                                            pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) ) );
                                                            groundSlopePairs.Add( leftEndOfPair );
                                                            continue;
                                                        }
                                                    }
                                                else if ( grSlope.slopeTile.isBottom )
                                                {
                                                    Vector2 leftEndOfPair = new Vector2(x - 2 - 0.5f, (y + correctionVer) * -1);
                                                    if ( !CheckIfTherIsTheSamePair( groundSlopePairs, leftEndOfPair ) )
                                                    {
                                                        pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 0.5f, ( y + 1 + correctionVer ) * -1 ) ) );
                                                        groundSlopePairs.Add( leftEndOfPair );
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    Vector2 leftEndOfPair = new Vector2(x - 0.5f, (y + correctionVer) * -1);
                                                    if ( !CheckIfTherIsTheSamePair( groundSlopePairs, leftEndOfPair ) )
                                                    {
                                                        pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y + 1 + correctionVer ) * -1 ) ) );
                                                        groundSlopePairs.Add( leftEndOfPair );
                                                        x += 1;
                                                        continue;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }



                                    // First check.
                                    checking = true;
                                    if ( x != 0 )
                                        if ( 2DArray[ y, x - 1 ] != -1 )
                                            pos2.Add( new Pos2( new Vector2( x - correctionHorWall, ( y + correctionVer ) * -1 ), new Vector2( 0, 0 ) ) );
                                        else if ( 2DArray[ y + 1, x - 1 ] != -1 )
                                            pos2.Add( new Pos2( new Vector2( x - 0.5f, ( y + correctionVer ) * -1 ), new Vector2( 0, 0 ) ) );
                                        else
                                            pos2.Add( new Pos2( new Vector2( x - correctionHorEdge, ( y + correctionVer ) * -1 ), new Vector2( 0, 0 ) ) );
                                    else
                                        pos2.Add( new Pos2( new Vector2( x - 0.5f, ( y + correctionVer ) * -1 ), new Vector2( 0, 0 ) ) );

                                    //Checking if end of sector.
                                    if ( x == xend - 1 )
                                    {
                                        if ( xend != 2DArray.GetLength( 1 ) )
                                            if ( 2DArray[ y, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y + correctionVer ) * -1 ) );
                                            else if ( 2DArray[ y + 1, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) );
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge, ( y + correctionVer ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) );
                                        checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y + 1, x ] != -1 )
                                {
                                    if ( groundSlopeTiles.Count > 0 )
                                    {
                                        var grSlope = FindSlope(groundSlopeTiles,2DArray[y + 1, x] );
                                        if ( grSlope.found )
                                        {
                                            x--;
                                            checking = false;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) );
                                            continue;
                                        }
                                    }


                                    //Checking if end of sector.
                                    if ( x == xend - 1 )
                                    {
                                        if ( xend != 2DArray.GetLength( 1 ) )
                                            if ( 2DArray[ y, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y + correctionVer ) * -1 ) );
                                            else if ( 2DArray[ y + 1, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) );
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge, ( y + correctionVer ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y + correctionVer ) * -1 ) );
                                        checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge - 1, ( y + correctionVer ) * -1 ) );
                                    checking = false;
                                }
                            }
                            else
                            {
                                // When reaching wall
                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall - 1, ( y + correctionVer ) * -1 ) );
                                checking = false;
                            }
                        }
                    }
                }
                if ( pos2.Count > 0 ) secHan.sektors[ q, w ].groundPos.AddRange( pos2 );

                // End Creating ground
            }
        }
    }


    void CreateCeilingCollidersPos( sbyte[,] 2DArray )
    {
        for ( ushort q = 0 ; q < secHan.sektors.GetLength( 0 ) ; ++q )
        {
            for ( ushort w = 0 ; w < secHan.sektors.GetLength( 1 ) ; ++w )
            {
                var nry = q * secHan.widthheight;
                var yend = nry + secHan.widthheight;
                if ( nry == 0 ) nry = 1;
                if ( yend > 2DArray.GetLength( 0 ) - 1 ) yend = 2DArray.GetLength( 0 ) - 1;

                var nrx = w * secHan.widthheight;
                var xend = nrx + secHan.widthheight;
                if ( nrx == 0 ) nrx = 1;
                if ( xend > 2DArray.GetLength( 1 ) ) xend = 2DArray.GetLength( 1 );

                float correctionHorWall = 0.625f;
                float correctionHorEdge = 0.375f;

                bool checking = false;
                List<Pos2> pos2 = new List<Pos2>();

                // Ceiling
                checking = false;
                pos2.RemoveRange( 0, pos2.Count );
                ceilingSlopePairs = new List<Vector2>();

                if ( nry == 0 ) ++nry;

                for ( int y = nry ; y < yend ; ++y )
                {
                    for ( int x = nrx ; x < xend ; ++x )
                    {
                        if ( !checking )
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y - 1, x ] != -1 )
                                {
                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var ceSlope = FindSlope(ceilingSlopeTiles, 2DArray[y - 1, x] );
                                        if ( ceSlope.found )
                                        {
                                            if ( ceSlope.slopeTile.isSpecial )
                                            {
                                                if ( ceSlope.slopeTile.isLeft )
                                                    if ( ceSlope.slopeTile.isBottom )
                                                    {
                                                        Vector2 leftEndOfPair = new Vector2(x - 0.5f, (y - 0.5f) * -1);
                                                        if ( !CheckIfTherIsTheSamePair( ceilingSlopePairs, leftEndOfPair ) )
                                                        {
                                                            pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y - 1.5f ) * -1 ) ) );
                                                            ceilingSlopePairs.Add( leftEndOfPair );

                                                            x += 1;
                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Vector2 leftEndOfPair = new Vector2(x - 2 - 0.5f, (y - 0.5f) * -1);
                                                        if ( !CheckIfTherIsTheSamePair( ceilingSlopePairs, leftEndOfPair ) )
                                                        {
                                                            pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 0.5f, ( y - 1.5f ) * -1 ) ) );
                                                            ceilingSlopePairs.Add( leftEndOfPair );
                                                            continue;
                                                        }
                                                    }
                                                else if ( ceSlope.slopeTile.isBottom )
                                                {
                                                    Vector2 leftEndOfPair = new Vector2(x - 2 - 0.5f, (y - 1.5f ) * -1);
                                                    if ( !CheckIfTherIsTheSamePair( ceilingSlopePairs, leftEndOfPair ) )
                                                    {
                                                        pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) ) );
                                                        ceilingSlopePairs.Add( leftEndOfPair );
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    Vector2 leftEndOfPair = new Vector2(x - 0.5f, (y - 1.5f) * -1);
                                                    if ( !CheckIfTherIsTheSamePair( ceilingSlopePairs, leftEndOfPair ) )
                                                    {
                                                        pos2.Add( new Pos2( leftEndOfPair, new Vector2( x + 2 + 0.5f, ( y - 0.5f ) * -1 ) ) );
                                                        ceilingSlopePairs.Add( leftEndOfPair );
                                                        x += 1;
                                                        continue;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }

                                    // First check
                                    checking = true;
                                    if ( x != 0 )
                                        if ( 2DArray[ y, x - 1 ] == -1 && 2DArray[ y - 1, x - 1 ] != -1 ) // If there isn't tile on the left and if on top left there is tile
                                            pos2.Add( new Pos2( new Vector2( x - 0.5f, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) );
                                        else if ( 2DArray[ y, x - 1 ] != -1 )
                                            pos2.Add( new Pos2( new Vector2( x - correctionHorWall, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) );
                                        else
                                            pos2.Add( new Pos2( new Vector2( x - correctionHorEdge, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) );
                                    else
                                        pos2.Add( new Pos2( new Vector2( x - correctionHorWall, ( y - 0.5f ) * -1 ), new Vector2( 0, 0 ) ) );

                                    //Checking if end
                                    if ( x == xend - 1 )
                                    {
                                        if ( xend != 2DArray.GetLength( 1 ) )
                                            if ( 2DArray[ y, x + 1 ] == -1 && 2DArray[ y - 1, x + 1 ] != -1 )// If there isn't tile on the right and if there is tile on top right;
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) );
                                            else if ( 2DArray[ y, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y - 0.5f ) * -1 ) );
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge, ( y - 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y - 0.5f ) * -1 ) );

                                        checking = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ( 2DArray[ y, x ] == -1 )
                            {
                                if ( 2DArray[ y - 1, x ] != -1 )
                                {
                                    if ( ceilingSlopeTiles.Count > 0 )
                                    {
                                        var ceSlope = FindSlope(ceilingSlopeTiles, 2DArray[y - 1, x] );
                                        if ( ceSlope.found )
                                        {
                                            --x;
                                            checking = false;
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) );

                                            continue;
                                        }
                                    }

                                    // Checking if end
                                    if ( x == xend - 1 )
                                    {
                                        if ( xend != 2DArray.GetLength( 1 ) )
                                            if ( 2DArray[ y, x + 1 ] == -1 && 2DArray[ y - 1, x + 1 ] != -1 )// If there isn't tile on the right and if there is tile on top right;
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + 0.5f, ( y - 0.5f ) * -1 ) );
                                            else if ( 2DArray[ y, x + 1 ] != -1 )
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y - 0.5f ) * -1 ) );
                                            else
                                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge, ( y - 0.5f ) * -1 ) );
                                        else
                                            pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall, ( y - 0.5f ) * -1 ) );

                                        checking = false;
                                    }
                                }
                                else
                                {
                                    // When reaching edge
                                    pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorEdge - 1, ( y - 0.5f ) * -1 ) );

                                    checking = false;
                                }
                            }
                            else
                            {
                                // When reaching wall
                                pos2[ pos2.Count - 1 ] = new Pos2( pos2[ pos2.Count - 1 ].first, new Vector2( x + correctionHorWall - 1, ( y - 0.5f ) * -1 ) );
                                checking = false;
                            }
                        }
                    }
                }
                if ( pos2.Count > 0 ) secHan.sektors[ q, w ].ceilingPos.AddRange( pos2 );
                // End of creating ceiling.
            }
        }
    }

    public sbyte[,] GetValues()
    {
        var lines = CSVFile.text.Split('\n');
        sbyte[,] csvMap = new sbyte[lines.GetLength(0), lines[0].Split(',').GetLength(0)];
        for ( int y = 0 ; y < lines.GetLength( 0 ) ; ++y )
        {
            string[] values = lines[y].Split(',');
            for ( int x = 0 ; x < values.GetLength( 0 ) ; ++x )
            {
                sbyte.TryParse( values[ x ], out csvMap[ y, x ] );
            }
        }
        return csvMap;
    }

    bool CheckIfTherIsTheSamePair( List<Vector2> pairsList, Vector2 leftEndOfPair )
    {
        for ( int id = 0 ; id < pairsList.Count ; id++ )
        {
            if ( pairsList[ id ].x == leftEndOfPair.x && pairsList[ id ].y == leftEndOfPair.y ) return true;
        }

        return false;
    }

    BoolSlopeTile FindSlope( List<SlopeTile> slopeTiles, float id )
    {
        int l, p, s;
        l = s = 0;
        p = slopeTiles.Count - 1;

        if ( p < 0 ) return new BoolSlopeTile( false );

        while ( l <= p )
        {
            s = ( l + p ) / 2;

            if ( slopeTiles[ s ].id == id )
                return new BoolSlopeTile( true, slopeTiles[ s ] );

            if ( slopeTiles[ s ].id < id )
                l = s + 1;
            else
                p = s - 1;
        }
        return new BoolSlopeTile( false );
    }

    void SortSlopes()
    {
        SortSlopesinside( groundSlopeTiles, 0, groundSlopeTiles.Count - 1 );
        SortSlopesinside( ceilingSlopeTiles, 0, ceilingSlopeTiles.Count - 1 );
    }

    void SortSlopesinside( List<SlopeTile> slopeTiles, int left, int right )
    {
        if ( slopeTiles.Count > 0 )
        {
            var i = left;
            var j = right;
            var pivot = slopeTiles[(left + right) / 2].id;

            while ( i < j )
            {
                while ( slopeTiles[ i ].id < pivot ) i++;
                while ( slopeTiles[ j ].id > pivot ) j--;
                if ( i <= j )
                {
                    var tmp = slopeTiles[i];
                    slopeTiles[ i++ ] = slopeTiles[ j ];
                    slopeTiles[ j-- ] = tmp;
                }
            }

            if ( left < j ) SortSlopesinside( slopeTiles, left, j );
            if ( i < right ) SortSlopesinside( slopeTiles, i, right );
        }
    }
}

[System.Serializable]
public struct SlopeTile
{
    public int id;
    public bool isSpecial;
    public bool isLeft;
    public bool isBottom;

    public SlopeTile( int id ) : this()
    {
        id = id;
    }

    public SlopeTile( int id, bool isSpecial, bool isLeft, bool isBottom )
    {
        id = id;
        isSpecial = isSpecial;
        isLeft = isLeft;
        isBottom = isBottom;
    }
}

public struct BoolSlopeTile
{
    public bool found;
    public SlopeTile slopeTile;

    public BoolSlopeTile( bool found ) : this()
    {
        found = found;
    }

    public BoolSlopeTile( bool found, SlopeTile slopeTile )
    {
        found = found;
        slopeTile = slopeTile;
    }
}

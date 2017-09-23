using UnityEngine;
using System.Collections.Generic;
using MyDataTypes;

public class CollidersOP : MonoBehaviour
{
    public bool ActiveDebugLog = false;

    public GameObject wall;
    public GameObject ceiling;
    public GameObject leftEdge;
    public GameObject rightEdge;

    public List<EdgeCollider2D> groundColliders = new List<EdgeCollider2D>();
    public List<EdgeCollider2D> ceilingColliders = new List<EdgeCollider2D>();
    public List<EdgeCollider2D> wallColliders = new List<EdgeCollider2D>();
    public TwoListsGameObjects edges = new TwoListsGameObjects();

    public byte playerWidth = 2;
    public byte playerHeight = 3;
    public List<IdSector> currentSectors = new List<IdSector>();
    public List<IdSector> lastSectors = new List<IdSector>();
    public List<ActiveSector> activeSectors = new List<ActiveSector>();

    public ushort widthheight = 10;
    public SectorData[,] sektors;
    public Transform playerTrans;
    Vector2 lastPlayerPosition;
    Transform trans;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag( "Player" ).transform;
        trans = GetComponent<Transform>();
        if ( !wall )
        {
            Debug.LogError( "wall = null" );
        }
        else if ( !leftEdge )
        {
            Debug.LogError( "leftEdge = null" );
        }
        else if ( !rightEdge )
        {
            Debug.LogError( "rightEdge = null" );
        }
        else if ( !playerTrans )
        {
            Debug.LogError( "playerTran = null" );
        }
        else
        {
            ChooseWhichSectorsToCheck( playerTrans.position );
            ObjectPooling();
        }
    }

    void Update()
    {
        ChooseWhichSectorsToCheck( playerTrans.position );
        ObjectPooling();
    }

    public void ObjectPooling()
    {
        if ( lastSectors.Count == 0 && currentSectors.Count != 0 )
        {
            for ( byte i = 0 ; i < currentSectors.Count ; ++i )
            {
                activeSectors.Add(
                    new ActiveSector(
                        currentSectors[ i ],
                    CreateGroundColliders(
                        sektors[ currentSectors[ i ].i,
                        currentSectors[ i ].z ].groundPos ),
                    CreateCeilingColliders(
                        sektors[ currentSectors[ i ].i,
                        currentSectors[ i ].z ].ceilingPos ),
                    CreateWallColliders(
                        sektors[ currentSectors[ i ].i,
                        currentSectors[ i ].z ].wallPos ),
                    CreateEdges(
                        sektors[ currentSectors[ i ].i,
                        currentSectors[ i ].z ].edgesPos ) ) );
            }
            lastSectors.AddRange( currentSectors );
            currentSectors.RemoveRange( 0, currentSectors.Count );
        }
        else if ( currentSectors.Count != 0 )
        {
            List<IdSector> newSectors = ChooseWhichSectorsAreNew();
            for ( byte i = 0 ; i < newSectors.Count ; ++i )
            {
                activeSectors.Add(
                    new ActiveSector(
                        newSectors[ i ],
                        CreateGroundColliders(
                            sektors[ newSectors[ i ].i,
                            newSectors[ i ].z ].groundPos ),
                        CreateCeilingColliders(
                            sektors[ newSectors[ i ].i,
                            newSectors[ i ].z ].ceilingPos ),
                        CreateWallColliders(
                            sektors[ newSectors[ i ].i,
                            newSectors[ i ].z ].wallPos ),
                        CreateEdges(
                            sektors[ newSectors[ i ].i,
                            newSectors[ i ].z ].edgesPos ) ) );
            }
            lastSectors.RemoveRange( 0, lastSectors.Count );
            lastSectors.AddRange( currentSectors );
            currentSectors.RemoveRange( 0, currentSectors.Count );
        }
    }

    public void PrintIdSectorList( string startText, List<IdSector> IdSector )
    {
        print( startText + " " + IdSector.Count + "\n" );
        for ( ushort i = 0 ; i < IdSector.Count ; ++i )
        {
            print( IdSector[ i ] );
        }
    }

    public List<IdSector> ChooseWhichSectorsAreNew()
    {
        List<IdSector> newSectors = new List<IdSector>();
        List<byte> recoveredSectors = new List<byte>();

        for ( byte i = 0 ; i < currentSectors.Count ; ++i )
        {
            bool recovered = false;
            for ( byte z = 0 ; z < lastSectors.Count ; ++z )
            {
                if ( currentSectors[ i ].i == lastSectors[ z ].i && currentSectors[ i ].z == lastSectors[ z ].z )
                {
                    recovered = true; recoveredSectors.Add( z ); break;
                }
            }
            if ( !recovered )
                newSectors.Add( currentSectors[ i ] );
        }
        ChooseWhichSectorDisactive( recoveredSectors );
        return newSectors;
    }

    public void ChooseWhichSectorDisactive( List<byte> recoveredSectors )
    {
        List<byte> acToDestroy = new List<byte>();

        for ( byte i = 0 ; i < lastSectors.Count ; ++i )
        {
            bool recovered = false;
            for ( byte q = 0 ; q < recoveredSectors.Count ; ++q )
            {
                if ( i == recoveredSectors[ q ] ) recovered = true;
            }
            if ( recovered )
            {
                continue;
            }
            for ( byte z = 0 ; z < activeSectors.Count ; ++z )
            {
                if ( activeSectors[ z ].Id.i == lastSectors[ i ].i && activeSectors[ z ].Id.z == lastSectors[ i ].z )
                {
                    GiveBackEverything( activeSectors[ z ] ); acToDestroy.Add( z ); break;
                }
            }
        }
        var zend = (sbyte)(acToDestroy.Count - 1);
        for ( sbyte z = zend ; z >= 0 ; --z )
        {
            activeSectors.RemoveAt( FindMadMaxAndDelete( acToDestroy ) );
        }
    }

    byte FindMadMaxAndDelete( List<byte> byteList )
    {
        byte max = 0;
        byte objectToDelete = 0;
        for ( byte i = 0 ; i < byteList.Count ; ++i )
        {
            if ( byteList[ i ] > max ) { max = byteList[ i ]; objectToDelete = i; }
        }
        byteList.RemoveAt( objectToDelete );
        return max;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ac">One of 4 lists of sectors with colliders</param>
    public void GiveBackEverything( ActiveSector ac )
    {
        for ( ushort i = 0 ; i < ac.groundColliders.Count ; ++i )
        {
            groundColliders.Add( ac.groundColliders[ i ] );
            ac.groundColliders[ i ].enabled = false;
        }
        ac.groundColliders.RemoveRange( 0, ac.groundColliders.Count );

        for ( ushort i = 0 ; i < ac.ceilingColliders.Count ; ++i )
        {
            ceilingColliders.Add( ac.ceilingColliders[ i ] );
            ac.ceilingColliders[ i ].enabled = false;
        }
        ac.ceilingColliders.RemoveRange( 0, ac.ceilingColliders.Count );

        for ( ushort i = 0 ; i < ac.wallColliders.Count ; ++i )
        {
            wallColliders.Add( ac.wallColliders[ i ] );
            ac.wallColliders[ i ].enabled = false;
        }
        ac.wallColliders.RemoveRange( 0, ac.wallColliders.Count );

        for ( ushort i = 0 ; i < ac.edges.one.Count ; ++i )
        {
            edges.one.Add( ac.edges.one[ i ] );
            ac.edges.one[ i ].SetActive( false );
        }
        ac.edges.one.RemoveRange( 0, ac.edges.one.Count );

        for ( ushort i = 0 ; i < ac.edges.two.Count ; ++i )
        {
            edges.two.Add( ac.edges.two[ i ] );
            ac.edges.two[ i ].SetActive( false );
        }
        ac.edges.two.RemoveRange( 0, ac.edges.two.Count );
    }

    public void ChooseWhichSectorsToCheck( Vector2 playerPosition )
    {
        if ( playerPosition != lastPlayerPosition )
        {
            lastPlayerPosition = playerPosition;
            if ( playerPosition.x > sektors.GetLength( 1 ) * widthheight - widthheight ) playerPosition.x = sektors.GetLength( 1 ) * widthheight - widthheight;
            if ( playerPosition.y < -( sektors.GetLength( 0 ) * widthheight ) + widthheight ) playerPosition.y = -( sektors.GetLength( 1 ) * widthheight - 1 ) + widthheight;
            short closestY = (short)(playerPosition.y - (playerPosition.y % widthheight));
            ushort closestX = 0;
            unchecked
            {
                closestX = (ushort)( playerPosition.x - ( playerPosition.x % widthheight ) );
            }

            // Now occurs offset to top, left corner.
            if ( closestY != 0 ) closestY += (short)widthheight;
            if ( closestX != 0 ) closestX -= widthheight;
            if ( closestX > 65500 ) closestX = 0;
            if ( closestX > sektors.GetLength( 1 ) * widthheight - widthheight * 2 ) closestX = (ushort)( sektors.GetLength( 1 ) * widthheight - widthheight * 2 );
            if ( closestY < -( sektors.GetLength( 0 ) * widthheight - widthheight * 2 ) ) closestY = (short)( -( sektors.GetLength( 0 ) * widthheight - widthheight * 2 ) );
            if ( closestX < 0 ) closestX = 0;
            if ( closestY > 0 ) closestY = 0;

            CheckCollisionWithPlayer( closestX, closestY, playerPosition );
        }
    }

    public void CheckCollisionWithPlayer( ushort x, short y, Vector2 playerPosition )
    {
        ushort iend = (ushort)(-y + 3 * widthheight);
        ushort zend = (ushort)(x + 3 * widthheight);
        currentSectors.RemoveRange( 0, currentSectors.Count );

        for ( ushort i = (ushort)-y ; i < iend ; i += widthheight )
        {
            for ( ushort z = x ; z < zend ; z += widthheight )
            {
                if ( playerPosition.y - 1.5f > i * -1 )
                {
                    continue;
                }
                else if ( playerPosition.x - ( (float)playerWidth / 2 ) > z + widthheight )
                {
                    continue;
                }
                else if ( playerPosition.x + ( (float)playerWidth / 2 ) < z )
                {
                    continue;
                }
                else if ( playerPosition.y + playerHeight < ( i * -1 ) - widthheight )
                {
                    continue;
                }
                else
                {
                    // hero is in the sector.
                    currentSectors.Add( new IdSector( (ushort)( i / widthheight ), (ushort)( z / widthheight ) ) );
                }
            }
        }
    }

    TwoListsGameObjects CreateEdges( PosList2 pl2 )
    {
        if ( ActiveDebugLog ) print( "joł create edge left: " + pl2.v21.Count + " right: " + pl2.v22.Count );
        //Left edges
        TwoListsGameObjects edges = new TwoListsGameObjects();
        for ( int i = 0 ; i < pl2.v21.Count ; ++i )
        {
            if ( edges.one.Count == 0 )
            {
                var edge = Instantiate(leftEdge, pl2.v21[i], Quaternion.identity) as GameObject;
                edge.transform.SetParent( trans );
                edges.one.Add( edge );
            }
            else
            {
                var edge = edges.one[0];
                edge.transform.position = pl2.v21[ i ];
                edge.transform.SetParent( trans );
                edge.SetActive( true );
                edges.one.Add( edge );
                edges.one.RemoveAt( 0 );
            }

        }
        //Right edges
        for ( int z = 0 ; z < pl2.v22.Count ; ++z )
        {
            if ( edges.two.Count == 0 )
            {
                var edge = Instantiate(rightEdge, pl2.v22[z], Quaternion.identity) as GameObject;
                edge.transform.SetParent( trans );
                edges.two.Add( edge );
            }
            else
            {
                var edge = edges.two[0];
                edge.transform.position = pl2.v22[ z ];
                edge.transform.SetParent( trans );
                edges.two.Add( edge );
                edge.SetActive( true );
                edges.two.RemoveAt( 0 );
            }
        }

        return edges;
    }

    List<EdgeCollider2D> CreateWallColliders( List<Pos2> pos2 )
    {
        if ( ActiveDebugLog ) print( "Create wall " + pos2.Count );
        List<EdgeCollider2D> wallCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < pos2.Count ; ++i )
        {
            if ( wallColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = wall.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                wallCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = wallColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                wallCollList.Add( ec2D );
                wallColliders.RemoveAt( 0 );
            }
        }

        return wallCollList;
    }

    List<EdgeCollider2D> CreateGroundColliders( List<Pos2> pos2 )
    {
        List<EdgeCollider2D> groundCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < pos2.Count ; ++i )
        {
            if ( groundColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = gameObject.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                groundCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = groundColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                groundCollList.Add( ec2D );
                groundColliders.RemoveAt( 0 );
            }
        }

        return groundCollList;
    }

    List<EdgeCollider2D> CreateCeilingColliders( List<Pos2> pos2 )
    {
        List<EdgeCollider2D> ceilingCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < pos2.Count ; ++i )
        {
            if ( ceilingColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = ceiling.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                ceilingCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = ceilingColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = pos2[ i ].first;
                v2[ 1 ] = pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                ceilingCollList.Add( ec2D );
                ceilingColliders.RemoveAt( 0 );
            }
        }

        return ceilingCollList;
    }
}

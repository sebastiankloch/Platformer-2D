using UnityEngine;
using System.Collections.Generic;
using MyDataTypes;

public class CollidersOP : MonoBehaviour
{
    public bool _ActiveDebugLog = false;

    public GameObject _wall;
    public GameObject _ceiling;
    public GameObject _leftEdge;
    public GameObject _rightEdge;

    public List<EdgeCollider2D> _groundColliders = new List<EdgeCollider2D>();
    public List<EdgeCollider2D> _ceilingColliders = new List<EdgeCollider2D>();
    public List<EdgeCollider2D> _wallColliders = new List<EdgeCollider2D>();
    public TwoLists_GameObjects _edges = new TwoLists_GameObjects();

    public byte _playerWidth = 2;
    public byte _playerHeight = 3;
    public List<Id_Sector> _currentSectors = new List<Id_Sector>();
    public List<Id_Sector> _lastSectors = new List<Id_Sector>();
    public List<ActiveSector> _activeSectors = new List<ActiveSector>();

    public ushort _width_height = 10;
    public SectorData[,] _sektors;
    public Transform _playerTrans;
    Vector2 _lastPlayerPosition;
    Transform _trans;

    void Start()
    {
        _playerTrans = GameObject.FindGameObjectWithTag( "Player" ).transform;
        _trans = GetComponent<Transform>();
        if ( !_wall )
        {
            Debug.LogError( "_wall = null" );
        }
        else if ( !_leftEdge )
        {
            Debug.LogError( "_leftEdge = null" );
        }
        else if ( !_rightEdge )
        {
            Debug.LogError( "_rightEdge = null" );
        }
        else if ( !_playerTrans )
        {
            Debug.LogError( "_playerTran = null" );
        }
        else
        {
            _ChooseWhichSectorsToCheck( _playerTrans.position );
            _ObjectPooling();
        }
    }

    void Update()
    {
        _ChooseWhichSectorsToCheck( _playerTrans.position );
        _ObjectPooling();
    }

    public void _ObjectPooling()
    {
        if ( _lastSectors.Count == 0 && _currentSectors.Count != 0 )
        {
            for ( byte i = 0 ; i < _currentSectors.Count ; ++i )
            {
                _activeSectors.Add(
                    new ActiveSector(
                        _currentSectors[ i ],
                    _CreateGroundColliders(
                        _sektors[ _currentSectors[ i ].i,
                        _currentSectors[ i ].z ]._groundPos ),
                    _CreateCeilingColliders(
                        _sektors[ _currentSectors[ i ].i,
                        _currentSectors[ i ].z ]._ceilingPos ),
                    _CreateWallColliders(
                        _sektors[ _currentSectors[ i ].i,
                        _currentSectors[ i ].z ]._wallPos ),
                    _CreateEdges(
                        _sektors[ _currentSectors[ i ].i,
                        _currentSectors[ i ].z ]._edgesPos ) ) );
            }
            _lastSectors.AddRange( _currentSectors );
            _currentSectors.RemoveRange( 0, _currentSectors.Count );
        }
        else if ( _currentSectors.Count != 0 )
        {
            List<Id_Sector> _newSectors = _ChooseWhichSectorsAreNew();
            for ( byte i = 0 ; i < _newSectors.Count ; ++i )
            {
                _activeSectors.Add(
                    new ActiveSector(
                        _newSectors[ i ],
                        _CreateGroundColliders(
                            _sektors[ _newSectors[ i ].i,
                            _newSectors[ i ].z ]._groundPos ),
                        _CreateCeilingColliders(
                            _sektors[ _newSectors[ i ].i,
                            _newSectors[ i ].z ]._ceilingPos ),
                        _CreateWallColliders(
                            _sektors[ _newSectors[ i ].i,
                            _newSectors[ i ].z ]._wallPos ),
                        _CreateEdges(
                            _sektors[ _newSectors[ i ].i,
                            _newSectors[ i ].z ]._edgesPos ) ) );
            }
            _lastSectors.RemoveRange( 0, _lastSectors.Count );
            _lastSectors.AddRange( _currentSectors );
            _currentSectors.RemoveRange( 0, _currentSectors.Count );
        }
    }

    public void _PrintId_SectorList( string __startText, List<Id_Sector> __Id_Sector )
    {
        print( __startText + " " + __Id_Sector.Count + "\n" );
        for ( ushort i = 0 ; i < __Id_Sector.Count ; ++i )
        {
            print( __Id_Sector[ i ] );
        }
    }

    public List<Id_Sector> _ChooseWhichSectorsAreNew()
    {
        List<Id_Sector> _newSectors = new List<Id_Sector>();
        List<byte> _recoveredSectors = new List<byte>();

        for ( byte i = 0 ; i < _currentSectors.Count ; ++i )
        {
            bool _recovered = false;
            for ( byte z = 0 ; z < _lastSectors.Count ; ++z )
            {
                if ( _currentSectors[ i ].i == _lastSectors[ z ].i && _currentSectors[ i ].z == _lastSectors[ z ].z )
                {
                    _recovered = true; _recoveredSectors.Add( z ); break;
                }
            }
            if ( !_recovered )
                _newSectors.Add( _currentSectors[ i ] );
        }
        _ChooseWhichSectorDisactive( _recoveredSectors );
        return _newSectors;
    }

    public void _ChooseWhichSectorDisactive( List<byte> _recoveredSectors )
    {
        List<byte> _acToDestroy = new List<byte>();

        for ( byte i = 0 ; i < _lastSectors.Count ; ++i )
        {
            bool _recovered = false;
            for ( byte q = 0 ; q < _recoveredSectors.Count ; ++q )
            {
                if ( i == _recoveredSectors[ q ] ) _recovered = true;
            }
            if ( _recovered )
            {
                continue;
            }
            for ( byte z = 0 ; z < _activeSectors.Count ; ++z )
            {
                if ( _activeSectors[ z ].Id.i == _lastSectors[ i ].i && _activeSectors[ z ].Id.z == _lastSectors[ i ].z )
                {
                    _GiveBackEverything( _activeSectors[ z ] ); _acToDestroy.Add( z ); break;
                }
            }
        }
        var z_end = (sbyte)(_acToDestroy.Count - 1);
        for ( sbyte z = z_end ; z >= 0 ; --z )
        {
            _activeSectors.RemoveAt( _FindMadMaxAndDelete( _acToDestroy ) );
        }
    }

    byte _FindMadMaxAndDelete( List<byte> __byteList )
    {
        byte max = 0;
        byte objectToDelete = 0;
        for ( byte i = 0 ; i < __byteList.Count ; ++i )
        {
            if ( __byteList[ i ] > max ) { max = __byteList[ i ]; objectToDelete = i; }
        }
        __byteList.RemoveAt( objectToDelete );
        return max;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_ac">One of 4 lists of sectors with colliders</param>
    public void _GiveBackEverything( ActiveSector _ac )
    {
        for ( ushort i = 0 ; i < _ac._groundColliders.Count ; ++i )
        {
            _groundColliders.Add( _ac._groundColliders[ i ] );
            _ac._groundColliders[ i ].enabled = false;
        }
        _ac._groundColliders.RemoveRange( 0, _ac._groundColliders.Count );

        for ( ushort i = 0 ; i < _ac._ceilingColliders.Count ; ++i )
        {
            _ceilingColliders.Add( _ac._ceilingColliders[ i ] );
            _ac._ceilingColliders[ i ].enabled = false;
        }
        _ac._ceilingColliders.RemoveRange( 0, _ac._ceilingColliders.Count );

        for ( ushort i = 0 ; i < _ac._wallColliders.Count ; ++i )
        {
            _wallColliders.Add( _ac._wallColliders[ i ] );
            _ac._wallColliders[ i ].enabled = false;
        }
        _ac._wallColliders.RemoveRange( 0, _ac._wallColliders.Count );

        for ( ushort i = 0 ; i < _ac._edges.one.Count ; ++i )
        {
            _edges.one.Add( _ac._edges.one[ i ] );
            _ac._edges.one[ i ].SetActive( false );
        }
        _ac._edges.one.RemoveRange( 0, _ac._edges.one.Count );

        for ( ushort i = 0 ; i < _ac._edges.two.Count ; ++i )
        {
            _edges.two.Add( _ac._edges.two[ i ] );
            _ac._edges.two[ i ].SetActive( false );
        }
        _ac._edges.two.RemoveRange( 0, _ac._edges.two.Count );
    }

    public void _ChooseWhichSectorsToCheck( Vector2 _playerPosition )
    {
        if ( _playerPosition != _lastPlayerPosition )
        {
            _lastPlayerPosition = _playerPosition;
            if ( _playerPosition.x > _sektors.GetLength( 1 ) * _width_height - _width_height ) _playerPosition.x = _sektors.GetLength( 1 ) * _width_height - _width_height;
            if ( _playerPosition.y < -( _sektors.GetLength( 0 ) * _width_height ) + _width_height ) _playerPosition.y = -( _sektors.GetLength( 1 ) * _width_height - 1 ) + _width_height;
            short closestY = (short)(_playerPosition.y - (_playerPosition.y % _width_height));
            ushort closestX = 0;
            unchecked
            {
                closestX = (ushort)( _playerPosition.x - ( _playerPosition.x % _width_height ) );
            }

            // Now occurs offset to top, left corner.
            if ( closestY != 0 ) closestY += (short)_width_height;
            if ( closestX != 0 ) closestX -= _width_height;
            if ( closestX > 65500 ) closestX = 0;
            if ( closestX > _sektors.GetLength( 1 ) * _width_height - _width_height * 2 ) closestX = (ushort)( _sektors.GetLength( 1 ) * _width_height - _width_height * 2 );
            if ( closestY < -( _sektors.GetLength( 0 ) * _width_height - _width_height * 2 ) ) closestY = (short)( -( _sektors.GetLength( 0 ) * _width_height - _width_height * 2 ) );
            if ( closestX < 0 ) closestX = 0;
            if ( closestY > 0 ) closestY = 0;

            _CheckCollisionWithPlayer( closestX, closestY, _playerPosition );
        }
    }

    public void _CheckCollisionWithPlayer( ushort x, short y, Vector2 _playerPosition )
    {
        ushort i_end = (ushort)(-y + 3 * _width_height);
        ushort z_end = (ushort)(x + 3 * _width_height);
        _currentSectors.RemoveRange( 0, _currentSectors.Count );

        for ( ushort i = (ushort)-y ; i < i_end ; i += _width_height )
        {
            for ( ushort z = x ; z < z_end ; z += _width_height )
            {
                if ( _playerPosition.y - 1.5f > i * -1 )
                {
                    continue;
                }
                else if ( _playerPosition.x - ( (float)_playerWidth / 2 ) > z + _width_height )
                {
                    continue;
                }
                else if ( _playerPosition.x + ( (float)_playerWidth / 2 ) < z )
                {
                    continue;
                }
                else if ( _playerPosition.y + _playerHeight < ( i * -1 ) - _width_height )
                {
                    continue;
                }
                else
                {
                    // hero is in the sector.
                    _currentSectors.Add( new Id_Sector( (ushort)( i / _width_height ), (ushort)( z / _width_height ) ) );
                }
            }
        }
    }

    TwoLists_GameObjects _CreateEdges( PosList2 __pl2 )
    {
        if ( _ActiveDebugLog ) print( "joł create edge left: " + __pl2.v2_1.Count + " right: " + __pl2.v2_2.Count );
        //Left edges
        TwoLists_GameObjects __edges = new TwoLists_GameObjects();
        for ( int i = 0 ; i < __pl2.v2_1.Count ; ++i )
        {
            if ( _edges.one.Count == 0 )
            {
                var edge = Instantiate(_leftEdge, __pl2.v2_1[i], Quaternion.identity) as GameObject;
                edge.transform.SetParent( _trans );
                __edges.one.Add( edge );
            }
            else
            {
                var edge = _edges.one[0];
                edge.transform.position = __pl2.v2_1[ i ];
                edge.transform.SetParent( _trans );
                edge.SetActive( true );
                __edges.one.Add( edge );
                _edges.one.RemoveAt( 0 );
            }

        }
        //Right edges
        for ( int z = 0 ; z < __pl2.v2_2.Count ; ++z )
        {
            if ( _edges.two.Count == 0 )
            {
                var edge = Instantiate(_rightEdge, __pl2.v2_2[z], Quaternion.identity) as GameObject;
                edge.transform.SetParent( _trans );
                __edges.two.Add( edge );
            }
            else
            {
                var edge = _edges.two[0];
                edge.transform.position = __pl2.v2_2[ z ];
                edge.transform.SetParent( _trans );
                __edges.two.Add( edge );
                edge.SetActive( true );
                _edges.two.RemoveAt( 0 );
            }
        }

        return __edges;
    }

    List<EdgeCollider2D> _CreateWallColliders( List<Pos2> __pos2 )
    {
        if ( _ActiveDebugLog ) print( "Create wall " + __pos2.Count );
        List<EdgeCollider2D> __wallCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < __pos2.Count ; ++i )
        {
            if ( _wallColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = _wall.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                __wallCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = _wallColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                __wallCollList.Add( ec2D );
                _wallColliders.RemoveAt( 0 );
            }
        }

        return __wallCollList;
    }

    List<EdgeCollider2D> _CreateGroundColliders( List<Pos2> __pos2 )
    {
        List<EdgeCollider2D> __groundCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < __pos2.Count ; ++i )
        {
            if ( _groundColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = gameObject.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                __groundCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = _groundColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                __groundCollList.Add( ec2D );
                _groundColliders.RemoveAt( 0 );
            }
        }

        return __groundCollList;
    }

    List<EdgeCollider2D> _CreateCeilingColliders( List<Pos2> __pos2 )
    {
        List<EdgeCollider2D> __ceilingCollList = new List<EdgeCollider2D>();
        for ( int i = 0 ; i < __pos2.Count ; ++i )
        {
            if ( _ceilingColliders.Count == 0 )
            {
                EdgeCollider2D ec2D = _ceiling.AddComponent<EdgeCollider2D>() as EdgeCollider2D;
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                __ceilingCollList.Add( ec2D );
            }
            else
            {
                EdgeCollider2D ec2D = _ceilingColliders[0];
                Vector2[] v2 = new Vector2[2];
                v2[ 0 ] = __pos2[ i ].first;
                v2[ 1 ] = __pos2[ i ].second;

                ec2D.points = v2;
                ec2D.enabled = true;
                __ceilingCollList.Add( ec2D );
                _ceilingColliders.RemoveAt( 0 );
            }
        }

        return __ceilingCollList;
    }
}

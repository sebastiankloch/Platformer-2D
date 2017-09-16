using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDataTypes;
using System;
using n_Dust;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class FallingRubbleGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    public Transform _leftTrans;
    public Transform _rigtTrans;
    public Transform _maxLevelTrans;

    public Transform2[] _raycastPairsTrans;
    [Space]
    public bool _save;
    public bool _load;
    public bool _saved;
    [Space]
    public GameObject _endPrefab;
    public GameObject _pairPrefab;
    public GameObject _maxLevelPrefab;
    [Space]
    public bool _genEnds;
    public bool _endsExist;
    [Space]
    public bool _genPair;
    public bool _pairExist;
    [Space]
    public bool _genMax;
    public bool _maxExist;
#endif

    public float _frequency;
    public int _maxAmount;
    int _amount;
    public float _maxLevel;
    public bool _createDust;
    
    public LayerMask _whatIsGround;
    float _distance;
    public Pos2 _left_rightEnd;
    public MyDataTypes.Float2[] _raycastPairs;
    public float _dustTime;
    public float _fallAfterDustTime;
    Transform _playerTrans;
    Transform _trans;
    bool i_Create_isRunning;

    private static int _fallenRubbleState = Animator.StringToHash("Base Layer.Fallen rubble");
    private static int _emptyState = Animator.StringToHash("Base Layer.Empty");

    private void Start()
    {
        if ( Application.isPlaying )
        {
            _trans = transform;
            if ( _maxLevel > _trans.position.y ) _maxLevel = _trans.position.y - 1;
            _distance = _trans.position.y - _maxLevel;
            _playerTrans = ControllerOpt._player.transform;
        }

    }

#if UNITY_EDITOR

    private void Update()
    {
        if ( _save && !_saved )
        {
            _Save_InEditor();
            _save = false;
            _saved = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( _load && _saved )
        {
            _Load_InEditor();
            _load = false;
            _saved = false;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( _genEnds && !_endsExist )
        {
            _GenEnds();
            _genEnds = false;
            _endsExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( _genPair && !_pairExist )
        {
            _GenPair();
            _genPair = false;
            _pairExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( _genMax && !_maxExist )
        {
            _GenMax();
            _genMax = false;
            _maxExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }
    }

    private void _GenMax()
    {
        _maxLevelTrans = Instantiate( _maxLevelPrefab, new Vector2( transform.position.x, transform.position.y - 5 ), Quaternion.identity ).GetComponent<Transform>();
        _maxLevelTrans.name = "Max level";
        _maxLevelTrans.parent = transform;
    }

    private void _GenPair()
    {
        Transform2[] __copy = new Transform2[_raycastPairsTrans.GetLength(0) + 1];
        _raycastPairsTrans.CopyTo( __copy, 0 );
        _raycastPairsTrans = new Transform2[ _raycastPairsTrans.GetLength( 0 ) + 1 ];
        __copy.CopyTo( _raycastPairsTrans, 0 );

        int i = _raycastPairsTrans.GetLength(0) - 1;
        Vector2 __leftPos = new Vector2(
            transform.position.x - 4,
            transform.position.y - 1
            );

        Vector2 __rightPos = new Vector2(
            transform.position.x + 4,
            transform.position.y - 1
            );

        _raycastPairsTrans[ i ].firstTrans = Instantiate( _pairPrefab, __leftPos, Quaternion.identity ).GetComponent<Transform>();
        _raycastPairsTrans[ i ].secondTrans = Instantiate( _pairPrefab, __rightPos, Quaternion.identity ).GetComponent<Transform>();

        _raycastPairsTrans[ i ].firstTrans.name = "Pair_s left end " +
            i;
        _raycastPairsTrans[ i ].secondTrans.name = "Pair_s right end " +
            i;

        _raycastPairsTrans[ i ].firstTrans.parent = transform;
        _raycastPairsTrans[ i ].secondTrans.parent = transform;
    }

    private void _GenEnds()
    {
        Vector2 __leftPos = new Vector2(
            transform.position.x - 5,
            transform.position.y
            );

        Vector2 __rightPos = new Vector2(
            transform.position.x + 5,
            transform.position.y
            );

        _leftTrans = Instantiate( _endPrefab, __leftPos, Quaternion.identity ).GetComponent<Transform>();
        _rigtTrans = Instantiate( _endPrefab, __rightPos, Quaternion.identity ).GetComponent<Transform>();

        _leftTrans.name = "Left End";
        _rigtTrans.name = "Right End";

        _leftTrans.parent = transform;
        _rigtTrans.parent = transform;
    }

    void _Save_InEditor()
    {
        _left_rightEnd.first = new Vector2( _leftTrans.position.x, _leftTrans.position.y );
        _left_rightEnd.second = new Vector2( _rigtTrans.position.x, _rigtTrans.position.y );
        DestroyImmediate( _leftTrans.gameObject );
        DestroyImmediate( _rigtTrans.gameObject );

        _maxLevel = _maxLevelTrans.position.y;
        DestroyImmediate( _maxLevelTrans.gameObject );

        _raycastPairs = new MyDataTypes.Float2[ _raycastPairsTrans.GetLength( 0 ) ];
        for ( int i = 0 ; i < _raycastPairsTrans.GetLength( 0 ) ; i++ )
        {
            if ( !_raycastPairsTrans[ i ].firstTrans ) continue;

            _raycastPairs[ i ].firstFloat = _raycastPairsTrans[ i ].firstTrans.position.x;
            DestroyImmediate( _raycastPairsTrans[ i ].firstTrans.gameObject );
            _raycastPairs[ i ].secondFloat = _raycastPairsTrans[ i ].secondTrans.position.x;
            DestroyImmediate( _raycastPairsTrans[ i ].secondTrans.gameObject );
        }

        if ( _raycastPairs.GetLength( 0 ) != 0 )
            _SortPairs_InEditor( ref _raycastPairs, 0, _raycastPairs.GetLength( 0 ) - 1 );
    }

    void _Load_InEditor()
    {
        _leftTrans = Instantiate( _endPrefab, new Vector2( _left_rightEnd.first.x, _left_rightEnd.first.y), Quaternion.identity ).GetComponent<Transform>();
        _rigtTrans = Instantiate( _endPrefab, new Vector2( _left_rightEnd.second.x, _left_rightEnd.second.y ) , Quaternion.identity ).GetComponent<Transform>();

        _leftTrans.name = "Left End";
        _rigtTrans.name = "Right End";

        _leftTrans.parent = transform;
        _rigtTrans.parent = transform;

        _maxLevelTrans = Instantiate( _maxLevelPrefab, new Vector2( transform.position.x, _maxLevel ), Quaternion.identity ).GetComponent<Transform>();
        _maxLevelTrans.name = "Max level";
        _maxLevelTrans.parent = transform;

        _raycastPairsTrans = new Transform2[ _raycastPairs.GetLength( 0 ) ];
        for ( int i = 0 ; i < _raycastPairs.GetLength( 0 ) ; i++ )
        {
            _raycastPairsTrans[ i ].firstTrans = Instantiate( _pairPrefab, new Vector2( _raycastPairs[ i ].firstFloat, transform.position.y - 1 ), Quaternion.identity ).GetComponent<Transform>();
            _raycastPairsTrans[ i ].secondTrans = Instantiate( _pairPrefab, new Vector2( _raycastPairs[ i ].secondFloat, transform.position.y - 1 ), Quaternion.identity ).GetComponent<Transform>();

            _raycastPairsTrans[ i ].firstTrans.name = "Pair_s left end " +
                i;
            _raycastPairsTrans[ i ].secondTrans.name = "Pair_s right end " +
                i;

            _raycastPairsTrans[ i ].firstTrans.parent = transform;
            _raycastPairsTrans[ i ].secondTrans.parent = transform;
        }
    }

    void _SortPairs_InEditor( ref MyDataTypes.Float2[] pairs, int left, int right )
    {
        var i = left;
        var j = right;
        var pivot = pairs[(left + right) / 2].firstFloat;
        while ( i < j )
        {
            while ( pairs[ i ].firstFloat < pivot ) i++;
            while ( pairs[ j ].firstFloat > pivot ) j--;
            if ( i <= j )
            {
                var tmp = pairs[i];
                pairs[ i++ ] = pairs[ j ];
                pairs[ j-- ] = tmp;
            }
        }
        if ( left < j ) _SortPairs_InEditor( ref pairs, left, j );
        if ( i < right ) _SortPairs_InEditor( ref pairs, i, right );
    }
#endif

    private void FixedUpdate()
    {
        if ( _playerTrans.position.y <= _trans.position.y && _playerTrans.position.x > _left_rightEnd.first.x - 17 && _playerTrans.position.x < _left_rightEnd.second.x + 17 && _playerTrans.position.y > _maxLevel )
        {
            if ( !i_Create_isRunning )
                StartCoroutine( "I_Create" );
        }
        else if ( i_Create_isRunning )
        {
            StopCoroutine( "I_Create" );
            i_Create_isRunning = false;
        }
    }

    IEnumerator I_Create()
    {
        i_Create_isRunning = true;
        while ( true )
        {
            if ( _amount <= _maxAmount )
                _CreateFallingRubble();
            yield return new WaitForSeconds( _frequency );
        }
    }

    void _CreateFallingRubble()
    {
        Vector2 __randomPos = new Vector2(UnityEngine.Random.Range(_left_rightEnd.first.x, _left_rightEnd.second.x), _left_rightEnd.first.y );
        bool __useRaycast = _CheckIfUseRaycast(__randomPos.x);
        if ( __useRaycast )
        {
            var hit = Physics2D.Raycast( __randomPos, Vector2.down, _distance, _whatIsGround );
            if ( hit )
                _StartFall( hit.point.y, __randomPos );
            else
            {
                _CreateFallingRubbleUsingCSV_Map( __randomPos );
            }
        }
        else
        {
            _CreateFallingRubbleUsingCSV_Map( __randomPos );
        }
    }

    void _CreateFallingRubbleUsingCSV_Map( Vector2 __pos )
    {
        var __thereIsGround_level = _CheckIfThereIsGroundUnderThisPos(__pos);
        if ( __thereIsGround_level.boolean )
        {
            _StartFall( __thereIsGround_level._float, __pos );
        }
        else
        {
            _StartFall( _maxLevel, __pos );
        }
    }

    void _StartFall( float __groundLevel, Vector2 __pos )
    {
        if ( DeadPoolHandler._DP_Handler._fallingRubblePool_Transforms.Count == 0 )
        {
            var __trans = Instantiate(DeadPoolHandler._DP_Handler._fallingRubblePrefabForGenerator, __pos, Quaternion.identity).transform;
            __trans.gameObject.SetActive( false );
            StartCoroutine( I_Fall( __trans, __trans.GetComponent<Animator>(), __trans.GetComponent<BoxCollider2D>(), __groundLevel, false ) );
        }
        else
        {
            StartCoroutine( I_Fall( DeadPoolHandler._DP_Handler._fallingRubblePool_Transforms[ 0 ], DeadPoolHandler._DP_Handler._fallingRubblePool_Animators[ 0 ], DeadPoolHandler._DP_Handler._fallingRubblePool_BoxColliders[ 0 ], __groundLevel, true, __pos ) );

            DeadPoolHandler._DP_Handler._fallingRubblePool_Transforms.RemoveAt( 0 );
            DeadPoolHandler._DP_Handler._fallingRubblePool_Animators.RemoveAt( 0 );
            DeadPoolHandler._DP_Handler._fallingRubblePool_BoxColliders.RemoveAt( 0 );
        }
    }

    public IEnumerator I_Fall( Transform __trans, Animator __anim, BoxCollider2D __box2D, float __groundLevel, bool __changePosition, Vector2 __pos = default( Vector2 ) )
    {
        ++_amount;
        if ( __changePosition )
            __trans.position = __pos;
        if ( _createDust )
        {
            yield return new WaitForSeconds( _dustTime );
            if ( __changePosition )
                Dust_s_Methods._CreateDust( __pos , this);
            else
                Dust_s_Methods._CreateDust( __trans.position, this );
        }

        yield return new WaitForSeconds( _fallAfterDustTime );
        __trans.gameObject.SetActive( true );
        __box2D.enabled = true;
        __anim.ResetTrigger( "Destroy" );
        float __distance = 0;

        while ( true )
        {
            __trans.Translate( 0, -__distance * Time.deltaTime, 0 );
            __distance += 9.8f * Time.deltaTime;
            if ( __trans.position.y - __distance * Time.deltaTime <= __groundLevel )
            {
                yield return null;
                __trans.position = new Vector2( __trans.position.x, __groundLevel );

                var __curState = __anim.GetCurrentAnimatorStateInfo( 0 );
                if ( __curState.fullPathHash != _fallenRubbleState && __curState.fullPathHash != _emptyState )
                {
                    __anim.SetTrigger( "Destroy" ); 
                }

                __box2D.enabled = false; break;
            }
            yield return null;
        }
        yield return new WaitForSeconds( 0.5f );
        __anim.ResetTrigger( "Destroy" );
        __anim.SetTrigger( "Idle" );
        __trans.gameObject.SetActive( false );
        __trans.position = new Vector3( -12, 12 );
        DeadPoolHandler._DP_Handler._fallingRubblePool_Transforms.Add( __trans );
        DeadPoolHandler._DP_Handler._fallingRubblePool_Animators.Add( __anim );
        DeadPoolHandler._DP_Handler._fallingRubblePool_BoxColliders.Add( __box2D );
        --_amount;
    }

    BoolFloat _CheckIfThereIsGroundUnderThisPos( Vector2 __pos )
    {
        Int2 __corPos = new Int2( Mathf.RoundToInt((__pos.x )), -Mathf.RoundToInt(__pos.y - 1.5f));
        int __maxId = -Mathf.FloorToInt(_maxLevel - 0.5f);

        for ( int i = __corPos.int2 ; i < __maxId ; i++ )
        {
            if ( CSV_Reader._csv_Reader._csvMap[ i, __corPos.int1 ] != -1 ) return new BoolFloat( true, -i + 0.375f );
        }
        return new BoolFloat( false, 0 );
    }

    private bool _CheckIfUseRaycast( float x )
    {
        if ( _raycastPairs.GetLength( 0 ) == 0 ) return false;

        if ( _raycastPairs[ 0 ].firstFloat > x ) return false;

        if ( _raycastPairs[ _raycastPairs.GetLength( 0 ) - 1 ].secondFloat < x ) return false;

        int __pair_sId = _FindClosestPair_sLeftEnd(x);
        if ( _raycastPairs[ __pair_sId ].secondFloat > x )
            return true;
        else
            return false;
    }

    int _FindClosestPair_sLeftEnd( float x )
    {
        int l, p, s;
        l = s = 0;
        p = _raycastPairs.GetLength( 0 ) - 1;
        if ( p < 0 ) { return -100; }


        while ( l <= p )
        {
            s = ( l + p ) / 2;

            if ( _raycastPairs[ s ].firstFloat == x )
            {
                return s;
            }

            if ( _raycastPairs[ s ].firstFloat < x )
                l = s + 1;
            else
                p = s - 1;
        }
        if ( s > 0 && _raycastPairs[ s ].firstFloat > x )
            return s - 1;
        else
            return s;
    }
}
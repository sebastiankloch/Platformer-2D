using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDataTypes;
using System;
using nDust;
#if UNITYEDITOR
[ExecuteInEditMode]
#endif
public class FallingRubbleGenerator : MonoBehaviour
{
#if UNITYEDITOR
    public Transform leftTrans;
    public Transform rigtTrans;
    public Transform maxLevelTrans;

    public Transform2[] raycastPairsTrans;
    [Space]
    public bool save;
    public bool load;
    public bool saved;
    [Space]
    public GameObject endPrefab;
    public GameObject pairPrefab;
    public GameObject maxLevelPrefab;
    [Space]
    public bool genEnds;
    public bool endsExist;
    [Space]
    public bool genPair;
    public bool pairExist;
    [Space]
    public bool genMax;
    public bool maxExist;
#endif

    public float frequency;
    public int maxAmount;
    int amount;
    public float maxLevel;
    public bool createDust;

    public LayerMask whatIsGround;
    float distance;
    public Pos2 leftrightEnd;
    public MyDataTypes.Float2[] raycastPairs;
    public float dustTime;
    public float fallAfterDustTime;
    Transform playerTrans;
    Transform trans;
    bool iCreateisRunning;

    private static int fallenRubbleState = Animator.StringToHash("Base Layer.Fallen rubble");
    private static int emptyState = Animator.StringToHash("Base Layer.Empty");

    private void Start()
    {
        if ( Application.isPlaying )
        {
            trans = transform;
            if ( maxLevel > trans.position.y ) maxLevel = trans.position.y - 1;
            distance = trans.position.y - maxLevel;
            playerTrans = ControllerOpt.player.transform;
        }

    }

#if UNITYEDITOR

    private void Update()
    {
        if ( save && !saved )
        {
            SaveInEditor();
            save = false;
            saved = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( load && saved )
        {
            LoadInEditor();
            load = false;
            saved = false;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( genEnds && !endsExist )
        {
            GenEnds();
            genEnds = false;
            endsExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( genPair && !pairExist )
        {
            GenPair();
            genPair = false;
            pairExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }

        if ( genMax && !maxExist )
        {
            GenMax();
            genMax = false;
            maxExist = true;
            UnityEditor.EditorUtility.SetDirty( this );
        }
    }

    private void GenMax()
    {
        maxLevelTrans = Instantiate( maxLevelPrefab, new Vector2( transform.position.x, transform.position.y - 5 ), Quaternion.identity ).GetComponent<Transform>();
        maxLevelTrans.name = "Max level";
        maxLevelTrans.parent = transform;
    }

    private void GenPair()
    {
        Transform2[] copy = new Transform2[raycastPairsTrans.GetLength(0) + 1];
        raycastPairsTrans.CopyTo( copy, 0 );
        raycastPairsTrans = new Transform2[ raycastPairsTrans.GetLength( 0 ) + 1 ];
        copy.CopyTo( raycastPairsTrans, 0 );

        int i = raycastPairsTrans.GetLength(0) - 1;
        Vector2 leftPos = new Vector2(
            transform.position.x - 4,
            transform.position.y - 1
            );

        Vector2 rightPos = new Vector2(
            transform.position.x + 4,
            transform.position.y - 1
            );

        raycastPairsTrans[ i ].firstTrans = Instantiate( pairPrefab, leftPos, Quaternion.identity ).GetComponent<Transform>();
        raycastPairsTrans[ i ].secondTrans = Instantiate( pairPrefab, rightPos, Quaternion.identity ).GetComponent<Transform>();

        raycastPairsTrans[ i ].firstTrans.name = "Pairs left end " +
            i;
        raycastPairsTrans[ i ].secondTrans.name = "Pairs right end " +
            i;

        raycastPairsTrans[ i ].firstTrans.parent = transform;
        raycastPairsTrans[ i ].secondTrans.parent = transform;
    }

    private void GenEnds()
    {
        Vector2 leftPos = new Vector2(
            transform.position.x - 5,
            transform.position.y
            );

        Vector2 rightPos = new Vector2(
            transform.position.x + 5,
            transform.position.y
            );

        leftTrans = Instantiate( endPrefab, leftPos, Quaternion.identity ).GetComponent<Transform>();
        rigtTrans = Instantiate( endPrefab, rightPos, Quaternion.identity ).GetComponent<Transform>();

        leftTrans.name = "Left End";
        rigtTrans.name = "Right End";

        leftTrans.parent = transform;
        rigtTrans.parent = transform;
    }

    void SaveInEditor()
    {
        leftrightEnd.first = new Vector2( leftTrans.position.x, leftTrans.position.y );
        leftrightEnd.second = new Vector2( rigtTrans.position.x, rigtTrans.position.y );
        DestroyImmediate( leftTrans.gameObject );
        DestroyImmediate( rigtTrans.gameObject );

        maxLevel = maxLevelTrans.position.y;
        DestroyImmediate( maxLevelTrans.gameObject );

        raycastPairs = new MyDataTypes.Float2[ raycastPairsTrans.GetLength( 0 ) ];
        for ( int i = 0 ; i < raycastPairsTrans.GetLength( 0 ) ; i++ )
        {
            if ( !raycastPairsTrans[ i ].firstTrans ) continue;

            raycastPairs[ i ].firstFloat = raycastPairsTrans[ i ].firstTrans.position.x;
            DestroyImmediate( raycastPairsTrans[ i ].firstTrans.gameObject );
            raycastPairs[ i ].secondFloat = raycastPairsTrans[ i ].secondTrans.position.x;
            DestroyImmediate( raycastPairsTrans[ i ].secondTrans.gameObject );
        }

        if ( raycastPairs.GetLength( 0 ) != 0 )
            SortPairsInEditor( ref raycastPairs, 0, raycastPairs.GetLength( 0 ) - 1 );
    }

    void LoadInEditor()
    {
        leftTrans = Instantiate( endPrefab, new Vector2( leftrightEnd.first.x, leftrightEnd.first.y), Quaternion.identity ).GetComponent<Transform>();
        rigtTrans = Instantiate( endPrefab, new Vector2( leftrightEnd.second.x, leftrightEnd.second.y ) , Quaternion.identity ).GetComponent<Transform>();

        leftTrans.name = "Left End";
        rigtTrans.name = "Right End";

        leftTrans.parent = transform;
        rigtTrans.parent = transform;

        maxLevelTrans = Instantiate( maxLevelPrefab, new Vector2( transform.position.x, maxLevel ), Quaternion.identity ).GetComponent<Transform>();
        maxLevelTrans.name = "Max level";
        maxLevelTrans.parent = transform;

        raycastPairsTrans = new Transform2[ raycastPairs.GetLength( 0 ) ];
        for ( int i = 0 ; i < raycastPairs.GetLength( 0 ) ; i++ )
        {
            raycastPairsTrans[ i ].firstTrans = Instantiate( pairPrefab, new Vector2( raycastPairs[ i ].firstFloat, transform.position.y - 1 ), Quaternion.identity ).GetComponent<Transform>();
            raycastPairsTrans[ i ].secondTrans = Instantiate( pairPrefab, new Vector2( raycastPairs[ i ].secondFloat, transform.position.y - 1 ), Quaternion.identity ).GetComponent<Transform>();

            raycastPairsTrans[ i ].firstTrans.name = "Pairs left end " +
                i;
            raycastPairsTrans[ i ].secondTrans.name = "Pairs right end " +
                i;

            raycastPairsTrans[ i ].firstTrans.parent = transform;
            raycastPairsTrans[ i ].secondTrans.parent = transform;
        }
    }

    void SortPairsInEditor( ref MyDataTypes.Float2[] pairs, int left, int right )
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
        if ( left < j ) SortPairsInEditor( ref pairs, left, j );
        if ( i < right ) SortPairsInEditor( ref pairs, i, right );
    }
#endif

    private void FixedUpdate()
    {
        if ( playerTrans.position.y <= trans.position.y && playerTrans.position.x > leftrightEnd.first.x - 17 && playerTrans.position.x < leftrightEnd.second.x + 17 && playerTrans.position.y > maxLevel )
        {
            if ( !iCreateisRunning )
                StartCoroutine( "ICreate" );
        }
        else if ( iCreateisRunning )
        {
            StopCoroutine( "ICreate" );
            iCreateisRunning = false;
        }
    }

    IEnumerator ICreate()
    {
        iCreateisRunning = true;
        while ( true )
        {
            if ( amount <= maxAmount )
                CreateFallingRubble();
            yield return new WaitForSeconds( frequency );
        }
    }

    void CreateFallingRubble()
    {
        Vector2 randomPos = new Vector2(UnityEngine.Random.Range(leftrightEnd.first.x, leftrightEnd.second.x), leftrightEnd.first.y );
        bool useRaycast = CheckIfUseRaycast(randomPos.x);
        if ( useRaycast )
        {
            var hit = Physics2D.Raycast( randomPos, Vector2.down, distance, whatIsGround );
            if ( hit )
                StartFall( hit.point.y, randomPos );
            else
            {
                CreateFallingRubbleUsingCSVMap( randomPos );
            }
        }
        else
        {
            CreateFallingRubbleUsingCSVMap( randomPos );
        }
    }

    void CreateFallingRubbleUsingCSVMap( Vector2 pos )
    {
        var thereIsGroundlevel = CheckIfThereIsGroundUnderThisPos(pos);
        if ( thereIsGroundlevel.boolean )
        {
            StartFall( thereIsGroundlevel.float, pos );
        }
        else
        {
            StartFall( maxLevel, pos );
        }
    }

    void StartFall( float groundLevel, Vector2 pos )
    {
        if ( DeadPoolHandler.DPHandler.fallingRubblePoolTransforms.Count == 0 )
        {
            var trans = Instantiate(DeadPoolHandler.DPHandler.fallingRubblePrefabForGenerator, pos, Quaternion.identity).transform;
            trans.gameObject.SetActive( false );
            StartCoroutine( IFall( trans, trans.GetComponent<Animator>(), trans.GetComponent<BoxCollider2D>(), groundLevel, false ) );
        }
        else
        {
            StartCoroutine( IFall( DeadPoolHandler.DPHandler.fallingRubblePoolTransforms[ 0 ], DeadPoolHandler.DPHandler.fallingRubblePoolAnimators[ 0 ], DeadPoolHandler.DPHandler.fallingRubblePoolBoxColliders[ 0 ], groundLevel, true, pos ) );

            DeadPoolHandler.DPHandler.fallingRubblePoolTransforms.RemoveAt( 0 );
            DeadPoolHandler.DPHandler.fallingRubblePoolAnimators.RemoveAt( 0 );
            DeadPoolHandler.DPHandler.fallingRubblePoolBoxColliders.RemoveAt( 0 );
        }
    }

    public IEnumerator IFall( Transform trans, Animator anim, BoxCollider2D box2D, float groundLevel, bool changePosition, Vector2 pos = default( Vector2 ) )
    {
        ++amount;
        if ( changePosition )
            trans.position = pos;
        if ( createDust )
        {
            yield return new WaitForSeconds( dustTime );
            if ( changePosition )
                DustsMethods.CreateDust( pos, this );
            else
                DustsMethods.CreateDust( trans.position, this );
        }

        yield return new WaitForSeconds( fallAfterDustTime );
        trans.gameObject.SetActive( true );
        box2D.enabled = true;
        anim.ResetTrigger( "Destroy" );
        float distance = 0;

        while ( true )
        {
            trans.Translate( 0, -distance * Time.deltaTime, 0 );
            distance += 9.8f * Time.deltaTime;
            if ( trans.position.y - distance * Time.deltaTime <= groundLevel )
            {
                yield return null;
                trans.position = new Vector2( trans.position.x, groundLevel );

                var curState = anim.GetCurrentAnimatorStateInfo( 0 );
                if ( curState.fullPathHash != fallenRubbleState && curState.fullPathHash != emptyState )
                {
                    anim.SetTrigger( "Destroy" );
                }

                box2D.enabled = false; break;
            }
            yield return null;
        }
        yield return new WaitForSeconds( 0.5f );
        anim.ResetTrigger( "Destroy" );
        anim.SetTrigger( "Idle" );
        trans.gameObject.SetActive( false );
        trans.position = new Vector3( -12, 12 );
        DeadPoolHandler.DPHandler.fallingRubblePoolTransforms.Add( trans );
        DeadPoolHandler.DPHandler.fallingRubblePoolAnimators.Add( anim );
        DeadPoolHandler.DPHandler.fallingRubblePoolBoxColliders.Add( box2D );
        --amount;
    }

    BoolFloat CheckIfThereIsGroundUnderThisPos( Vector2 pos )
    {
        Int2 corPos = new Int2( Mathf.RoundToInt((pos.x )), -Mathf.RoundToInt(pos.y - 1.5f));
        int maxId = -Mathf.FloorToInt(maxLevel - 0.5f);

        for ( int i = corPos.int2 ; i < maxId ; i++ )
        {
            if ( CSVReader.csvReader.csvMap[ i, corPos.int1 ] != -1 ) return new BoolFloat( true, -i + 0.375f );
        }
        return new BoolFloat( false, 0 );
    }

    private bool CheckIfUseRaycast( float x )
    {
        if ( raycastPairs.GetLength( 0 ) == 0 ) return false;

        if ( raycastPairs[ 0 ].firstFloat > x ) return false;

        if ( raycastPairs[ raycastPairs.GetLength( 0 ) - 1 ].secondFloat < x ) return false;

        int pairsId = FindClosestPairsLeftEnd(x);
        if ( raycastPairs[ pairsId ].secondFloat > x )
            return true;
        else
            return false;
    }

    int FindClosestPairsLeftEnd( float x )
    {
        int l, p, s;
        l = s = 0;
        p = raycastPairs.GetLength( 0 ) - 1;
        if ( p < 0 ) { return -100; }


        while ( l <= p )
        {
            s = ( l + p ) / 2;

            if ( raycastPairs[ s ].firstFloat == x )
            {
                return s;
            }

            if ( raycastPairs[ s ].firstFloat < x )
                l = s + 1;
            else
                p = s - 1;
        }
        if ( s > 0 && raycastPairs[ s ].firstFloat > x )
            return s - 1;
        else
            return s;
    }
}

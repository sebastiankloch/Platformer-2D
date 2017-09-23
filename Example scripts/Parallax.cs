using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDataTypes;
using MyMethods;
using System;

#if UNITYEDITOR
[ExecuteInEditMode]
#endif
public class Parallax : MonoBehaviour
{
#if UNITYEDITOR
    public bool save;
    public bool load;
    public bool saved;

    public bool activeDebugLog;

    public List<float> parallaxScales;
#endif
    public List<Transform> backgrounds;
    public float limit = 10f;

    private Transform cam;
    public Vector2 middle;
    private Vector2 vectorToCamera;

    public ParallaxSector[] parSecArray;
    public List<List<float>> parallaxScalesV2 = new List<List<float>>();
    public List<int> activeSectors = new List<int>();
    public List<List<Transform>> layersTrans = new List<List<Transform>>();

    void Awake()
    {
        cam = Camera.main.transform;
        backgrounds.RemoveRange( 0, backgrounds.Count );
        for ( int id = 0 ; id < transform.childCount ; id++ )
        {
            for ( int id2 = 0 ; id2 < transform.GetChild( id ).childCount ; id2++ )
            {
                backgrounds.Add( transform.GetChild( id ).GetChild( id2 ) );
            }
        }
    }

    void Start()
    {
#if UNITYEDITOR
        CSVReader csvReader = GameObject.FindGameObjectWithTag("Ground colliders").GetComponent<CSVReader>();
        sbyte[,] csvMap = csvReader.GetValues();
        middle.x = csvMap.GetLength( 1 );
        middle.y = csvMap.GetLength( 0 );
#else
        middle.x = CSVReader.csvReader.csvMap.GetLength( 1 );
        middle.y = CSVReader.csvReader.csvMap.GetLength( 0 );
#endif
        middle.x = middle.x / 2;
        middle.y = -( middle.y / 2 );

        if ( Application.isPlaying )
        {
            CalculateScales();

            CreateLayersTrans();

            AddMarginToParSectAreas();
        }
        else
        {
#if UNITYEDITOR
            parallaxScales.RemoveRange( 0, parallaxScales.Count );
            for ( int id = 0 ; id < backgrounds.Count ; id++ )
            {
                parallaxScales.Add( backgrounds[ id ].transform.position.z / limit );
            }
#endif
        }
    }

    private void CalculateScales()
    {
        for ( int id = 0 ; id < parSecArray.GetLength( 0 ) ; id++ )
        {
            parallaxScalesV2.Add( new List<float>() );

            for ( int id1 = 0 ; id1 < parSecArray[ id ].myLayers.Count ; id1++ )
            {
                parallaxScalesV2[ id ].Add( parSecArray[ id ].myLayers[ id1 ].z / limit );
            }
        }
    }

    private void CreateLayersTrans()
    {
        for ( int id = 0 ; id < parSecArray.GetLength( 0 ) ; id++ )
        {
            parSecArray[ id ].layersTrans.RemoveRange( 0, parSecArray[ id ].layersTrans.Count );
            for ( int id2 = 0 ; id2 < parSecArray[ id ].myLayers.Count ; id2++ )
            {
                parSecArray[ id ].layersTrans.Add( new GameObject().transform );
#if UNITYEDITOR
                parSecArray[ id ].layersTrans[ id2 ].name = id2.ToString();
#endif
            }
        }
    }

    void AddMarginToParSectAreas()
    {
        int yMargin = (int)cam.GetComponent<Camera>().orthographicSize;

        var cameraSize = (byte)cam.GetComponent<Camera>().orthographicSize;
        var heightOfScreen = (float)( cameraSize * 2 );
        var widthOfScreen = ( (float)( Screen.width ) / (float)( Screen.height ) ) * heightOfScreen;

        int xMargin = (short)( Mathf.Round( widthOfScreen / 2 ) );

        for ( int sectorId = 0 ; sectorId < parSecArray.GetLength( 0 ) ; sectorId++ )
        {
            var area =  parSecArray[sectorId].area;
            var maxZ = FindFarthestLayerZvalue( parSecArray[ sectorId ].myLayers );

            float calculation = ((limit - maxZ) / limit);
            if ( calculation <= 0 ) calculation = 0.1f;

            float correctedXmargin = (xMargin / calculation) + 2;
            float correctedYmargin = (yMargin / calculation) + 2;
            parSecArray[ sectorId ].area = new AreaV2( area.xLeft - correctedXmargin, area.yTop + correctedYmargin, area.xRight + correctedXmargin, area.yBottom - correctedYmargin );
        }
    }

    float FindFarthestLayerZvalue( List<MyLayer> layersList )
    {
        float maxZ = 0;
        for ( int layerId = 0 ; layerId < layersList.Count ; layerId++ )
        {
            if ( layersList[ layerId ].z > maxZ ) maxZ = layersList[ layerId ].z;
        }
        return maxZ;
    }

#if UNITYEDITOR
    private void OnEnable()
    {
        if ( !Application.isPlaying )
        {
            Awake();
            Start();
        }
    }

    void SaveSectorsinEditor()
    {
        parSecArray = new ParallaxSector[ 0 ];
        parSecArray = new ParallaxSector[ transform.childCount ];
        for ( int sectorId = 0 ; sectorId < transform.childCount ; sectorId++ )
        {
            BoxCollider2D secBox2D = transform.GetChild(sectorId).GetComponent<BoxCollider2D>();
            float x = secBox2D.size.x / 2;
            float y = secBox2D.size.y / 2;
            Vector2 secPos = secBox2D.transform.position;
            parSecArray[ sectorId ].area = new AreaV2( secPos.x - x, secPos.y + y, secPos.x + x, secPos.y - y );
            parSecArray[ sectorId ].myLayers = new List<MyLayer>();
            for ( int layerId = 0 ; layerId < transform.GetChild(sectorId).childCount ; layerId++ )
            {
                parSecArray[ sectorId ].myLayers.Add( new MyLayer( transform.GetChild( sectorId ).GetChild(layerId).transform.position.z, Methods.GiveFilledListOfVector2Wrappers( DeadPoolHandler.DPHandler.bgObjPrefabs.GetLength( 0 ) ) ) );

                for ( int objectId = 0 ; objectId < transform.GetChild(sectorId).GetChild(layerId).childCount ; objectId++ )
                {
                    BgObject bgObject = transform.GetChild( sectorId ).GetChild( layerId ).GetChild( objectId ).GetComponent<BgObject>();
                  
                    parSecArray[ sectorId ].myLayers[ layerId ].positions[ bgObject.id ].list.Add( bgObject.transform.localPosition );
                }
            }
        }
        UnityEditor.EditorUtility.SetDirty( this );

        Methods.KillAllChildren( transform );
    }

    void LoadSectorsinEditor()
    {
        for ( int sectorId = 0 ; sectorId < parSecArray.GetLength( 0 ) ; sectorId++ )
        {
            Transform sectorTrans = new GameObject("Sector " + sectorId).transform;
            sectorTrans.transform.SetParent( transform );
            BoxCollider2D secBox2D = sectorTrans.gameObject.AddComponent<BoxCollider2D>();
            float x = parSecArray[ sectorId ].area.xRight - parSecArray[ sectorId ].area.xLeft;
            float y = parSecArray[ sectorId ].area.yTop - parSecArray[ sectorId ].area.yBottom;
            secBox2D.size = new Vector2( x, y );
            sectorTrans.transform.position = new Vector2( parSecArray[ sectorId ].area.xLeft + x / 2, parSecArray[ sectorId ].area.yTop - y / 2 );
            
            for ( int layerId = 0 ; layerId < parSecArray[ sectorId ].myLayers.Count ; layerId++ )
            {
                Transform layerTransform = new GameObject("Layer " + layerId).transform;
                layerTransform.SetParent( sectorTrans );
                layerTransform.transform.position = new Vector3( 100, -100, parSecArray[ sectorId ].myLayers[ layerId ].z );
                for ( int objectId = 0 ; objectId < DeadPoolHandler.DPHandler.bgObjEditorPrefabs.GetLength( 0 ) ; objectId++ )
                {
                    for ( int objectPosId = 0 ; objectPosId < parSecArray[ sectorId ].myLayers[ layerId ].positions[ objectId ].list.Count ; objectPosId++ )
                    {
                        Transform bgObjectTransform = Instantiate(DeadPoolHandler.DPHandler.bgObjEditorPrefabs[objectId], layerTransform).transform;
                        bgObjectTransform.localPosition = parSecArray[ sectorId ].myLayers[ layerId ].positions[ objectId ].list[ objectPosId ];
                    }
                }
            }
        }
        OnEnable();
        load = false;
        Update();
        UnityEditor.EditorUtility.SetDirty( this );
    }
#endif

    void Update()
    {
        if ( Application.isPlaying )
        {
            CheckCollision();
            Parallax();
#if UNITYEDITOR
            if ( activeDebugLog )
                DrawParSecAreas();
#endif
        }
#if UNITYEDITOR
        else
            ParallaxInEditor();
#endif

#if UNITYEDITOR
        if ( save && !saved )
        {
            SaveSectorsinEditor();
            save = false;
            load = false;
            saved = true;
        }
        else if( load && saved )
        {
            LoadSectorsinEditor();
            save = false;
            load = false;
            saved = false;
        }
#endif
    }

#if UNITYEDITOR
    private void DrawParSecAreas()
    {
        for ( int sectorId = 0 ; sectorId < parSecArray.GetLength( 0 ) ; sectorId++ )
        {
            Methods.DrawSquare( parSecArray[ sectorId ].area );
        }
    }

    void ParallaxInEditor()
    {
        if ( backgrounds.Count > 0 && !backgrounds[ 0 ] ) return;
        vectorToCamera = (Vector2)cam.position - middle;
        for ( int id = 0 ; id < backgrounds.Count ; id++ )
        {

            Vector2 newPos = middle + vectorToCamera * parallaxScales[ id ];
            backgrounds[ id ].position = new Vector3( newPos.x, newPos.y, backgrounds[ id ].position.z );
        }
    }

#endif

    void Parallax()
    {
        vectorToCamera = (Vector2)cam.position - middle;
        if ( activeSectors.Count > 0 )
        {
            for ( int id = 0 ; id < activeSectors.Count ; id++ )
            {
                for ( int id2 = 0 ; id2 < parSecArray[ activeSectors[ id ] ].layersTrans.Count ; id2++ )
                {
                    Vector2 newPos = middle + vectorToCamera * parallaxScalesV2[ activeSectors[ id ] ][ id2 ];
                    parSecArray[ activeSectors[ id ] ].layersTrans[ id2 ].position = new Vector3( newPos.x, newPos.y, parSecArray[ activeSectors[ id ] ].myLayers[ id2 ].z );
                }
            }
        }
    }

    void CheckCollision()
    {
        for ( int id = 0 ; id < parSecArray.GetLength( 0 ) ; id++ )
        {
            if ( !parSecArray[ id ].inside )
                if ( CheckCollisionWithArea( parSecArray[ id ].area ) )
                {
                    activeSectors.Add( id );
                    parSecArray[ id ].id = activeSectors.Count - 1;
                    parSecArray[ id ].inside = true;
                    GenerateSectorLayers( id );
                }
                else
                    continue;
            else if ( !CheckCollisionWithArea( parSecArray[ id ].area ) )
            {
                parSecArray[ id ].inside = false;

                if ( parSecArray[ id ].id == activeSectors.Count - 1 )
                {
                    activeSectors.RemoveAt( parSecArray[ id ].id );
                    GiveBackBgObjectsToPool( id );
                }
                else
                {
                    activeSectors.RemoveAt( parSecArray[ id ].id );
                    GiveBackBgObjectsToPool( id );
                    for ( int id2 = parSecArray[ id ].id ; id2 < activeSectors.Count ; id2++ )
                    {
                        parSecArray[ activeSectors[ id2 ] ].id--;
                    }
                }
            }
        }
    }

    void GiveBackBgObjectsToPool( int id )
    {
        for ( int objectsListId = 0 ; objectsListId < parSecArray[ id ].localBgObjectsBank.Count ; objectsListId++ )
        {
            for ( int objectId = 0 ; objectId < parSecArray[ id ].localBgObjectsBank[ objectsListId ].list.Count ; objectId++ )
            {
                DeadPoolHandler.DPHandler.bgObjPools[ objectsListId ].Add( parSecArray[ id ].localBgObjectsBank[ objectsListId ].list[ objectId ] );
                parSecArray[ id ].localBgObjectsBank[ objectsListId ].list[ objectId ].gameObject.SetActive( false );
            }

            parSecArray[ id ].localBgObjectsBank[ objectsListId ].list.RemoveRange( 0, parSecArray[ id ].localBgObjectsBank[ objectsListId ].list.Count );
        }
    }

    void GenerateSectorLayers( int id )
    {
        parSecArray[ id ].localBgObjectsBank = new List<ListStTransformWrapper>();
        for ( int layerId = 0 ; layerId < parSecArray[ id ].layersTrans.Count ; layerId++ )
        {
            for ( int objectId = 0 ; objectId < DeadPoolHandler.DPHandler.bgObjPrefabs.GetLength( 0 ) ; objectId++ )
            {
                parSecArray[ id ].localBgObjectsBank.Add( new ListStTransformWrapper( true ) );
                bool poolIsEmpty = false;
                for ( int objectPosId = 0 ; objectPosId < parSecArray[ id ].myLayers[ layerId ].positions[ objectId ].list.Count ; objectPosId++ )
                {
                    if ( !poolIsEmpty )
                    {
                        if ( DeadPoolHandler.DPHandler.bgObjPools[ objectId ].Count != 0 ) // Taking from the pool
                        {
                            Transform bgObjectTrans = DeadPoolHandler.DPHandler.bgObjPools[objectId][0];
                            DeadPoolHandler.DPHandler.bgObjPools[ objectId ].RemoveAt( 0 );
                            bgObjectTrans.SetParent( parSecArray[ id ].layersTrans[ layerId ] );
                            bgObjectTrans.localPosition = parSecArray[ id ].myLayers[ layerId ].positions[ objectId ].list[ objectPosId ];
                            bgObjectTrans.gameObject.SetActive( true );

                            parSecArray[ id ].localBgObjectsBank[ objectId ].list.Add( bgObjectTrans );
                        }
                        else // Creating new object
                        {
                            CreatAnewBgObject( id, layerId, objectId, objectPosId );
                            poolIsEmpty = true;
                        }
                    }
                    else // Creating new object
                    {
                        CreatAnewBgObject( id, layerId, objectId, objectPosId );
                    }
                }
            }
        }
    }

    private void CreatAnewBgObject( int id, int layerId, int objectId, int objectPosId )
    {
        Transform bgObjectTrans = Instantiate(
            DeadPoolHandler.DPHandler.bgObjPrefabs[objectId],
            parSecArray[ id ].myLayers[ layerId ].positions[ objectId ].list[ objectPosId ],
            Quaternion.identity, parSecArray[ id ].layersTrans[ layerId ]).transform;

        parSecArray[ id ].localBgObjectsBank[ objectId ].list.Add( bgObjectTrans );
    }

    private bool CheckCollisionWithArea( AreaV2 area )
    {
        if ( cam.position.x < area.xLeft )
            return false;
        else if ( cam.position.x > area.xRight )
            return false;
        else if ( cam.position.y > area.yTop )
            return false;
        else if ( cam.position.y < area.yBottom )
            return false;
        else
            return true;
    }
}

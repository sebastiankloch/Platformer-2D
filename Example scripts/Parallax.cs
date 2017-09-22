using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyDataTypes;
using MyMethods;
using System;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Parallax : MonoBehaviour {
#if UNITY_EDITOR
    public bool _save;
    public bool _load;
    public bool _saved;

    public bool _activeDebugLog;

    public List<float> _parallaxScales;
#endif
    public List<Transform> _backgrounds;
    public float _limit = 10f;

    private Transform _cam;
    public Vector2 _middle;
    private Vector2 _vectorToCamera;

    public ParallaxSector[] _parSecArray;
    public List<List<float>> _parallaxScalesV2 = new List<List<float>>();
    public List<int> _activeSectors = new List<int>();
    public List<List<Transform>> _layersTrans = new List<List<Transform>>();

    void Awake()
    {
        _cam = Camera.main.transform;
        _backgrounds.RemoveRange(0, _backgrounds.Count);
        for ( int id = 0 ; id < transform.childCount ; id++ )
        {
            for ( int id_2 = 0 ; id_2 < transform.GetChild(id).childCount ; id_2++ )
            {
                _backgrounds.Add( transform.GetChild( id ).GetChild( id_2 ) );
            }
        }
    }

    void Start()
    {
#if UNITY_EDITOR
        CSV_Reader __csvReader = GameObject.FindGameObjectWithTag("Ground colliders").GetComponent<CSV_Reader>();
        sbyte[,] __csvMap = __csvReader._GetValues();
        _middle.x = __csvMap.GetLength( 1 );
        _middle.y = __csvMap.GetLength( 0 );
#else
        _middle.x = CSV_Reader._csv_Reader._csvMap.GetLength( 1 );
        _middle.y = CSV_Reader._csv_Reader._csvMap.GetLength( 0 );
#endif
        _middle.x = _middle.x / 2;
        _middle.y = -( _middle.y / 2 );
        
        if ( Application.isPlaying )
        {
            _CalculateScales();

            _CreateLayersTrans();

            _AddMarginToParSectAreas();
        }
        else
        {
#if UNITY_EDITOR
            _parallaxScales.RemoveRange( 0, _parallaxScales.Count );
            for ( int id = 0 ; id < _backgrounds.Count ; id++ )
            {
                _parallaxScales.Add( _backgrounds[ id ].transform.position.z / _limit );
            }
#endif
        }
    }

    private void _CalculateScales()
    {
        for ( int id = 0 ; id < _parSecArray.GetLength( 0 ) ; id++ )
        {
            _parallaxScalesV2.Add( new List<float>() );
            
            for ( int id_1 = 0 ; id_1 < _parSecArray[ id ].myLayers.Count ; id_1++ )
            {
                _parallaxScalesV2[ id ].Add( _parSecArray[ id ].myLayers[ id_1 ].z / _limit );
            }
        }
    }

    private void _CreateLayersTrans()
    {
        for ( int id = 0 ; id < _parSecArray.GetLength( 0 ) ; id++ )
        {
            _parSecArray[ id ].layersTrans.RemoveRange( 0, _parSecArray[ id ].layersTrans.Count );
            for ( int id_2 = 0 ; id_2 < _parSecArray[ id ].myLayers.Count ; id_2++ )
            {
                _parSecArray[ id ].layersTrans.Add( new GameObject().transform );
#if UNITY_EDITOR
                _parSecArray[ id ].layersTrans[ id_2 ].name = id_2.ToString();
#endif
            }
        }
    }

    void _AddMarginToParSectAreas()
    {
        int __yMargin = (int)_cam.GetComponent<Camera>().orthographicSize;
        
        var _cameraSize = (byte)_cam.GetComponent<Camera>().orthographicSize;
        var _heightOfScreen = (float)( _cameraSize * 2 );
        var _widthOfScreen = ( (float)( Screen.width ) / (float)( Screen.height ) ) * _heightOfScreen;
        
        int __xMargin = (short)( Mathf.Round( _widthOfScreen / 2 ) );

        for ( int __sectorId = 0 ; __sectorId < _parSecArray.GetLength(0) ; __sectorId++ )
        {
            var __area =  _parSecArray[__sectorId].area;
            var __maxZ = _FindFarthestLayerZ_value( _parSecArray[ __sectorId ].myLayers );
            
            float __calculation = ((_limit - __maxZ) / _limit);
            if ( __calculation <= 0 ) __calculation = 0.1f;
            
            float __correctedX_margin = (__xMargin / __calculation) + 2;
            float __correctedY_margin = (__yMargin / __calculation) + 2;
            _parSecArray[ __sectorId ].area = new AreaV2( __area.x_Left - __correctedX_margin, __area.y_Top + __correctedY_margin, __area.x_Right + __correctedX_margin, __area.y_Bottom - __correctedY_margin );
        }
    }

    float _FindFarthestLayerZ_value(List<MyLayer> __layersList)
    {
        float __maxZ = 0;
        for ( int __layerId = 0 ; __layerId < __layersList.Count ; __layerId++ )
        {
            if ( __layersList[ __layerId ].z > __maxZ ) __maxZ = __layersList[ __layerId ].z;
        }
        return __maxZ;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if ( !Application.isPlaying )
        {
            Awake();
            Start();
        }
    }

    void _SaveSectors_inEditor()
    {
        _parSecArray = new ParallaxSector[ 0 ];
        _parSecArray = new ParallaxSector[ transform.childCount ];
        for ( int __sectorId = 0 ; __sectorId < transform.childCount ; __sectorId++ )
        {
            BoxCollider2D __secBox2D = transform.GetChild(__sectorId).GetComponent<BoxCollider2D>();
            float __x = __secBox2D.size.x / 2;
            float __y = __secBox2D.size.y / 2;
            Vector2 __secPos = __secBox2D.transform.position;
            _parSecArray[ __sectorId ].area = new AreaV2( __secPos.x - __x, __secPos.y + __y, __secPos.x + __x, __secPos.y - __y );
            _parSecArray[ __sectorId ].myLayers = new List<MyLayer>();
            for ( int __layerId = 0 ; __layerId < transform.GetChild(__sectorId).childCount ; __layerId++ )
            {
                _parSecArray[ __sectorId ].myLayers.Add( new MyLayer( transform.GetChild( __sectorId ).GetChild(__layerId).transform.position.z, Methods._GiveFilledListOfVector2Wrappers( DeadPoolHandler._DP_Handler._bgObjPrefabs.GetLength( 0 ) ) ) );

                for ( int __objectId = 0 ; __objectId < transform.GetChild(__sectorId).GetChild(__layerId).childCount ; __objectId++ )
                {
                    BgObject __bgObject = transform.GetChild( __sectorId ).GetChild( __layerId ).GetChild( __objectId ).GetComponent<BgObject>();
                  
                    _parSecArray[ __sectorId ].myLayers[ __layerId ].positions[ __bgObject._id ].list.Add( __bgObject.transform.localPosition );
                }
            }
        }
        UnityEditor.EditorUtility.SetDirty( this );

        Methods._KillAllChildren( transform );
    }

    void _LoadSectors_inEditor()
    {
        for ( int __sectorId = 0 ; __sectorId < _parSecArray.GetLength( 0 ) ; __sectorId++ )
        {
            Transform __sectorTrans = new GameObject("Sector " + __sectorId).transform;
            __sectorTrans.transform.SetParent( transform );
            BoxCollider2D __secBox2D = __sectorTrans.gameObject.AddComponent<BoxCollider2D>();
            float __x = _parSecArray[ __sectorId ].area.x_Right - _parSecArray[ __sectorId ].area.x_Left;
            float __y = _parSecArray[ __sectorId ].area.y_Top - _parSecArray[ __sectorId ].area.y_Bottom;
            __secBox2D.size = new Vector2( __x, __y );
            __sectorTrans.transform.position = new Vector2( _parSecArray[ __sectorId ].area.x_Left + __x / 2, _parSecArray[ __sectorId ].area.y_Top - __y / 2 );
            
            for ( int __layerId = 0 ; __layerId < _parSecArray[ __sectorId ].myLayers.Count ; __layerId++ )
            {
                Transform __layerTransform = new GameObject("Layer " + __layerId).transform;
                __layerTransform.SetParent( __sectorTrans );
                __layerTransform.transform.position = new Vector3( 100, -100, _parSecArray[ __sectorId ].myLayers[ __layerId ].z );
                for ( int __objectId = 0 ; __objectId < DeadPoolHandler._DP_Handler._bgObjEditorPrefabs.GetLength( 0 ) ; __objectId++ )
                {
                    for ( int __objectPosId = 0 ; __objectPosId < _parSecArray[ __sectorId ].myLayers[ __layerId ].positions[ __objectId ].list.Count ; __objectPosId++ )
                    {
                        Transform __bgObjectTransform = Instantiate(DeadPoolHandler._DP_Handler._bgObjEditorPrefabs[__objectId], __layerTransform).transform;
                        __bgObjectTransform.localPosition = _parSecArray[ __sectorId ].myLayers[ __layerId ].positions[ __objectId ].list[ __objectPosId ];
                    }
                }
            }
        }
        OnEnable();
        _load = false;
        Update();
        UnityEditor.EditorUtility.SetDirty( this );
    }
#endif

    void Update () {
        if ( Application.isPlaying )
        {
            _CheckCollision();
            _Parallax();
#if UNITY_EDITOR
            if ( _activeDebugLog )
                _DrawParSecAreas();
#endif
        }
#if UNITY_EDITOR
        else
            _Parallax_InEditor();
#endif

#if UNITY_EDITOR
        if ( _save && !_saved )
        {
            _SaveSectors_inEditor();
            _save = false;
            _load = false;
            _saved = true;
        }
        else if( _load && _saved )
        {
            _LoadSectors_inEditor();
            _save = false;
            _load = false;
            _saved = false;
        }
#endif
    }

#if UNITY_EDITOR
    private void _DrawParSecAreas()
    {
        for ( int __sectorId = 0 ; __sectorId < _parSecArray.GetLength( 0 ) ; __sectorId++ )
        {
            Methods._DrawSquare( _parSecArray[ __sectorId ].area );
        }
    }

    void _Parallax_InEditor()
    {
        if ( _backgrounds.Count > 0 && !_backgrounds[ 0 ] ) return;
        _vectorToCamera = (Vector2)_cam.position - _middle;
        for ( int id = 0 ; id < _backgrounds.Count ; id++ )
        {

            Vector2 __newPos = _middle + _vectorToCamera * _parallaxScales[ id ];
            _backgrounds[ id ].position = new Vector3( __newPos.x, __newPos.y, _backgrounds[ id ].position.z );
        }
    }

#endif

    void _Parallax()
    {
        _vectorToCamera = (Vector2)_cam.position - _middle;
        if ( _activeSectors.Count > 0 )
        {
            for ( int id = 0 ; id < _activeSectors.Count ; id++ )
            {
                for ( int id_2 = 0 ; id_2 < _parSecArray[ _activeSectors[ id ] ].layersTrans.Count ; id_2++ )
                {
                    Vector2 __newPos = _middle + _vectorToCamera * _parallaxScalesV2[ _activeSectors[ id ] ][ id_2 ];
                    _parSecArray[ _activeSectors[ id ] ].layersTrans[ id_2 ].position = new Vector3( __newPos.x, __newPos.y, _parSecArray[ _activeSectors[ id ] ].myLayers[ id_2 ].z );
                }
            }
        }
    }

    void _CheckCollision()
    {
        for ( int id = 0 ; id < _parSecArray.GetLength(0) ; id++ )
        {
            if ( !_parSecArray[ id ].inside )
                if ( _CheckCollisionWithArea( _parSecArray[ id ].area ) )
                {
                    _activeSectors.Add( id );
                    _parSecArray[ id ].id = _activeSectors.Count - 1;
                    _parSecArray[ id ].inside = true;
                    _GenerateSectorLayers( id );
                }
                else
                    continue;
            else if ( !_CheckCollisionWithArea( _parSecArray[ id ].area ) )
            {
                _parSecArray[ id ].inside = false;

                if ( _parSecArray[ id ].id == _activeSectors.Count - 1 )
                {
                    _activeSectors.RemoveAt( _parSecArray[ id ].id );
                    _GiveBackBgObjectsToPool( id );
                }
                else
                {
                    _activeSectors.RemoveAt( _parSecArray[ id ].id );
                    _GiveBackBgObjectsToPool( id );
                    for ( int id_2 = _parSecArray[ id ].id ; id_2 < _activeSectors.Count ; id_2++ )
                    {
                        _parSecArray[ _activeSectors[ id_2 ] ].id--;
                    }
                }
            }
        }
    }

    void _GiveBackBgObjectsToPool( int __id )
    {
        for ( int __objectsListId = 0 ; __objectsListId < _parSecArray[ __id ].localBgObjectsBank.Count ; __objectsListId++ )
        {
            for ( int __objectId = 0 ; __objectId < _parSecArray[ __id ].localBgObjectsBank[ __objectsListId ].list.Count ; __objectId++ )
            {
                DeadPoolHandler._DP_Handler._bgObjPools[ __objectsListId ].Add( _parSecArray[ __id ].localBgObjectsBank[ __objectsListId ].list[ __objectId ] );
                _parSecArray[ __id ].localBgObjectsBank[ __objectsListId ].list[ __objectId ].gameObject.SetActive( false );
            }

            _parSecArray[ __id ].localBgObjectsBank[ __objectsListId ].list.RemoveRange( 0, _parSecArray[ __id ].localBgObjectsBank[ __objectsListId ].list.Count );
        }
    }

    void _GenerateSectorLayers( int __id )
    {
        _parSecArray[ __id ].localBgObjectsBank = new List<ListStTransformWrapper>();
        for ( int __layerId = 0 ; __layerId < _parSecArray[ __id ].layersTrans.Count ; __layerId++ )
        {
            for ( int __objectId = 0 ; __objectId < DeadPoolHandler._DP_Handler._bgObjPrefabs.GetLength( 0 ) ; __objectId++ )
            {
                _parSecArray[ __id ].localBgObjectsBank.Add( new ListStTransformWrapper( true ) );
                bool __poolIsEmpty = false;
                for ( int __objectPosId = 0 ; __objectPosId < _parSecArray[ __id ].myLayers[ __layerId ].positions[ __objectId ].list.Count ; __objectPosId++ )
                {
                    if ( !__poolIsEmpty )
                    {
                        if ( DeadPoolHandler._DP_Handler._bgObjPools[ __objectId ].Count != 0 ) // Taking from the pool
                        {
                            Transform __bgObjectTrans = DeadPoolHandler._DP_Handler._bgObjPools[__objectId][0];
                            DeadPoolHandler._DP_Handler._bgObjPools[ __objectId ].RemoveAt( 0 );
                            __bgObjectTrans.SetParent( _parSecArray[ __id ].layersTrans[ __layerId ] );
                            __bgObjectTrans.localPosition = _parSecArray[ __id ].myLayers[ __layerId ].positions[ __objectId ].list[ __objectPosId ];
                            __bgObjectTrans.gameObject.SetActive( true );
                            
                            _parSecArray[ __id ].localBgObjectsBank[ __objectId ].list.Add( __bgObjectTrans );
                        }
                        else // Creating new object
                        {
                            _CreatA_newBgObject( __id, __layerId, __objectId, __objectPosId );
                            __poolIsEmpty = true;
                        }
                    }
                    else // Creating new object
                    {
                        _CreatA_newBgObject( __id, __layerId, __objectId, __objectPosId );
                    }
                }
            }
        }
    }

    private void _CreatA_newBgObject( int __id, int __layerId, int __objectId, int __objectPosId )
    {
        Transform __bgObjectTrans = Instantiate(
            DeadPoolHandler._DP_Handler._bgObjPrefabs[__objectId],
            _parSecArray[ __id ].myLayers[ __layerId ].positions[ __objectId ].list[ __objectPosId ],
            Quaternion.identity, _parSecArray[ __id ].layersTrans[ __layerId ]).transform;
        
        _parSecArray[ __id ].localBgObjectsBank[ __objectId ].list.Add( __bgObjectTrans );
    }

    private bool _CheckCollisionWithArea( AreaV2 __area )
    {
        if ( _cam.position.x < __area.x_Left )
            return false;
        else if ( _cam.position.x > __area.x_Right )
            return false;
        else if ( _cam.position.y > __area.y_Top )
            return false;
        else if ( _cam.position.y < __area.y_Bottom )
            return false;
        else
            return true;
    }
}

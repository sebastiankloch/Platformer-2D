using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using Drones;

public class UpgradesUIController : MonoBehaviour
{
    public static UpgradesUIController _upg;

    public MainMenu _mMComp;

    public int _money;
    public DroneFireFighterUpgrades _fireFighterDroneUpg;
    public DroneAmbulanceUpgrades _ambulaceDroneUpg;
    public DroneSupplyUpgrades _supplyDroneUpg;
    [Space]
    public Text _moneyText;
    public Text _pointsText;
    [Header("Levels")]
    public Button[] _upButtons;
    public Text[] _lvlTexts;
    public Button[] _downButtons;
    public Text[] _footersTexts;
    [Header("Pernament")]
    public Button _upgEngButton;
    public Text _engineLvlText;
    public Text _upgEngPriceText;
    public Text _upgEngBasePointsText;

    public Button _decreaseTimeButton;
    public Text _decreasingTimeAmountText;
    public Text _decreaseTimePriceText;
    public Text _decreaseTimeBaseText;
    [Header("Skills")]
    public Button _extinctionButton;
    public Text _extNeededPointsTxt;
    public Text _extPriceText;
    [Space]
    public Image _droneImage;
    [Header("Navigation")]
    public Button _nextButton;
    public Button _previousButton;
    [Space]
    public Sprite _fireFiDrSprite;
    public Sprite _ambDrSprite;
    public Sprite _supDrSprite;


    private void Awake()
    {
        _upg = this;
    }

    #region Navigation

    public void _BackToMainMenu()
    {
        _mMComp.enabled = true;
        gameObject.SetActive( false );
    }

    public void _SaveDataAndBackToMenu()
    {
        _SendDataToGameController();
        GameController._gameCont._SavePlayersData();
        GameController._gameCont._SaveUpgradesData();
        _BackToMainMenu();
    }

    #endregion

    #region Firefighter Drone

    public void _IncreaseArmorFireFDrone()
    {
        _fireFighterDroneUpg._IncreaseArmor();
        _UpdateFirefighterDrArmorText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }
    public void _DecreaseArmorFireFDrone()
    {
        _fireFighterDroneUpg._DecreaseArmor();
        _UpdateFirefighterDrArmorText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _IncreaseSpeedFireFDrone()
    {
        _fireFighterDroneUpg._IncreaseSpeed();
        _UpdateFirefighterDrSpeedText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _DecreaseSpeedFireFDrone()
    {
        _fireFighterDroneUpg._DecreaseSpeed();
        _UpdateFirefighterDrSpeedText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _UpgradeShorteningTimeFireFDrone()
    {
        _fireFighterDroneUpg._UpgradeShorteningTime( ref _money );
        _SetDecreaseTimeAmountPriceAndBase( _fireFighterDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _fireFighterDroneUpg );
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _UpgradeEngineFireFDrone()
    {
        _fireFighterDroneUpg._UpgradeEngine( ref _money );
        _SetEngineLvlPriceAndBaseText( _fireFighterDroneUpg );
        _SetupEnginePriceAndBaseActivation( _fireFighterDroneUpg );
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _IncreaseAmmo()
    {
        _fireFighterDroneUpg._IncreaseAmmo();
        _UpdateFirefighterDrAmmoText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _DecreaseAmmo()
    {
        _fireFighterDroneUpg._DecreaseAmmo();
        _UpdateFirefighterDrAmmoText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _IncreaseExtincionSize()
    {
        _fireFighterDroneUpg._IncreaseExtincionSize();
        _UpdateFirefighterDrExtSizeText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    public void _DecreaseExtincionSize()
    {
        _fireFighterDroneUpg._DecreaseExtincionSize();
        _UpdateFirefighterDrExtSizeText();
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );
    }

    #endregion

    #region Ambulance drone

    public void _IncreaseArmorAmbDrone()
    {
        _ambulaceDroneUpg._IncreaseArmor();
        _UpdateAmbulanceDrArmorText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }
    public void _DecreaseArmorAmbDrone()
    {
        _ambulaceDroneUpg._DecreaseArmor();
        _UpdateAmbulanceDrArmorText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _IncreaseSpeedAmbDrone()
    {
        _ambulaceDroneUpg._IncreaseSpeed();
        _UpdateAmbulanceDrSpeedText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _DecreaseSpeedAmbDrone()
    {
        _ambulaceDroneUpg._DecreaseSpeed();
        _UpdateAmbulanceDrSpeedText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _UpgradeShorteningTimeAmbDrone()
    {
        _ambulaceDroneUpg._UpgradeShorteningTime( ref _money );
        _SetDecreaseTimeAmountPriceAndBase( _ambulaceDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _ambulaceDroneUpg );
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _UpgradeEngineAmbDrone()
    {
        _ambulaceDroneUpg._UpgradeEngine( ref _money );
        _SetEngineLvlPriceAndBaseText( _ambulaceDroneUpg );
        _SetupEnginePriceAndBaseActivation( _ambulaceDroneUpg );
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _ToogleExtincionOnTargetSkillAmbDrone()
    {
        _ambulaceDroneUpg._ToogleExtincionOnTargetSkill( ref _money );
        _SetColorOfExtOnTargetButton( _ambulaceDroneUpg );
        _SetExtOnTargetPriceAndSetupPointsAndPrice( _ambulaceDroneUpg );
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _IncreaseSlowDownDyingLevel()
    {
        _ambulaceDroneUpg._IncreaseSlowDownDyingLevel();
        _UpdateAmbulanceDrSlowDownText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    public void _DecreaseSlowDownDyingLevel()
    {
        _ambulaceDroneUpg._DecreaseSlowDownDyingLevel();
        _UpdateAmbulanceDrSlowDownText();
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );
    }

    #endregion

    #region Supply drone

    public void _IncreaseArmorSupDrone()
    {
        _supplyDroneUpg._IncreaseArmor();
        _UpdateSupplyDrArmorText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }
    public void _DecreaseArmorSupDrone()
    {
        _supplyDroneUpg._DecreaseArmor();
        _UpdateSupplyDrArmorText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _IncreaseSpeedSupDrone()
    {
        _supplyDroneUpg._IncreaseSpeed();
        _UpdateSupplyDrSpeedText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _DecreaseSpeedSupDrone()
    {
        _supplyDroneUpg._DecreaseSpeed();
        _UpdateSupplyDrSpeedText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _UpgradeShorteningTimeSupDrone()
    {
        _supplyDroneUpg._UpgradeShorteningTime( ref _money );
        _SetDecreaseTimeAmountPriceAndBase( _supplyDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _supplyDroneUpg );
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _UpgradeEngineSupDrone()
    {
        _supplyDroneUpg._UpgradeEngine( ref _money );
        _SetEngineLvlPriceAndBaseText( _supplyDroneUpg );
        _SetupEnginePriceAndBaseActivation( _supplyDroneUpg );
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _ToogleExtincionOnTargetSkillSupDrone()
    {
        _supplyDroneUpg._ToogleExtincionOnTargetSkill( ref _money );
        _SetColorOfExtOnTargetButton( _supplyDroneUpg );
        _SetExtOnTargetPriceAndSetupPointsAndPrice( _supplyDroneUpg );
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _IncreaseSizeOfPackage()
    {
        _supplyDroneUpg._IncreaseSizeOfPackage();
        _UpdateSupplyDrPackageSizeText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _DecreaseSizeOfPackage()
    {
        _supplyDroneUpg._DecreaseSizeOfPackage();
        _UpdateSupplyDrPackageSizeText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _IncreaseArmorPackage()
    {
        _supplyDroneUpg._IncreaseArmorPackage();
        _UpdateSupplyDrPackageArmorText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    public void _DecreaseArmorPackage()
    {
        _supplyDroneUpg._DecreaseArmorPackage();
        _UpdateSupplyDrPackageArmorText();
        _UpdateMoneyAndPoints( _supplyDroneUpg );
    }

    #endregion

    public void _MakeSureThatDataIsCorrect()
    {
        _fireFighterDroneUpg._MakeSureThatStatsAreCorrect();
        _ambulaceDroneUpg._MakeSureThatStatsAreCorrect();
        _supplyDroneUpg._MakeSureThatStatsAreCorrect();
    }

    #region Activation

    #region Getters and Senders

    public void _GetDataFromGameController()
    {
        _money = GameController._gameCont._playersData._money;
        _fireFighterDroneUpg = new DroneFireFighterUpgrades(
            GameController._gameCont._droneFireFiUpg._GetBasicStats(),
            GameController._gameCont._droneFireFiUpg._GetDroneSkills(),
            GameController._gameCont._droneFireFiUpg._FireFighterDroneLevels );
        _ambulaceDroneUpg = new DroneAmbulanceUpgrades(
            GameController._gameCont._droneAmbUpg._GetBasicStats(),
            GameController._gameCont._droneAmbUpg._GetDroneSkills(),
            GameController._gameCont._droneAmbUpg._AmbulanceDroneLevels );
        _supplyDroneUpg = new DroneSupplyUpgrades(
            GameController._gameCont._droneSupUpg._GetBasicStats(),
            GameController._gameCont._droneSupUpg._GetDroneSkills(),
            GameController._gameCont._droneSupUpg._SupplyDroneLvls );
    }

    public void _SendDataToGameController()
    {
        _MakeSureThatDataIsCorrect();
        GameController._gameCont._playersData._money = _money;
        GameController._gameCont._droneFireFiUpg = new DroneFireFighterUpgrades(
            _fireFighterDroneUpg._GetBasicStats(),
            _fireFighterDroneUpg._GetDroneSkills(),
            _fireFighterDroneUpg._FireFighterDroneLevels);
        GameController._gameCont._droneAmbUpg = new DroneAmbulanceUpgrades(
            _ambulaceDroneUpg._GetBasicStats(),
            _ambulaceDroneUpg._GetDroneSkills(),
            _ambulaceDroneUpg._AmbulanceDroneLevels );
        GameController._gameCont._droneSupUpg = new DroneSupplyUpgrades(
            _supplyDroneUpg._GetBasicStats(),
            _supplyDroneUpg._GetDroneSkills(),
            _supplyDroneUpg._SupplyDroneLvls );
    }

    #endregion

    #region Setters and Removers

    #region Setters

    void _UpdateMoneyAndPoints(DroneUpgrades __droneUpgrades)
    {
        _moneyText.text = "Money: " + _money;
        _pointsText.text = "Points: " + __droneUpgrades._GetBasicStats()._availablePoints;

    }

    void _SetActiveButtons( Text[] __buttons, int __from, bool __activate = true )
    {
        for ( int id = __from ; id < __buttons.GetLength( 0 ) ; id++ )
        {
            __buttons[ id ].transform.parent.gameObject.SetActive( __activate );
        }
    }

    void _SetLvlText0( int __value )
    {
        _SetLvlText( 0, __value );
    }

    void _SetLvlText1( int __value )
    {
        _SetLvlText( 1, __value );
    }

    void _SetLvlText2( int __value )
    {
        _SetLvlText( 2, __value );
    }

    void _SetLvlText3( int __value )
    {
        _SetLvlText( 3, __value );
    }

    void _SetLvlText( int __id, int __value )
    {
        _lvlTexts[ __id ].text = (__value + 1).ToString();
    }

    void _SetColorOfExtOnTargetButton( DroneUpgrades __droneUpgrades )
    {
        bool __active = __droneUpgrades._GetDroneSkills()._extinctionOnReachingTargetIsActive;
        if ( __active )
            _extinctionButton.GetComponent<Image>().color = Color.green;
        else
            _extinctionButton.GetComponent<Image>().color = Color.white;
    }

    void _SetEngineLvlPriceAndBaseText( DroneUpgrades __droneUpgrades )
    {
        _engineLvlText.text = "Lvl: " + (__droneUpgrades._GetBasicStats()._engineLevel + 1).ToString();
        _upgEngPriceText.text = "Price:\n" + ( __droneUpgrades._GetBasicStats()._engineBaseCost * ( __droneUpgrades._GetBasicStats()._engineLevel + 1 ) ).ToString();
        _upgEngBasePointsText.text = "+" + __droneUpgrades._GetBasicStats()._pointsPerEngineLvl + " Points";
    }

    void _SetDecreaseTimeAmountPriceAndBase( DroneUpgrades __droneUpgrades )
    {
        _decreasingTimeAmountText.text = "-" + ( __droneUpgrades._GetBasicStats()._baseShorteningTime * __droneUpgrades._GetBasicStats()._shorteningCoolingDownTimeLevel ).ToString() + "s";
        _decreaseTimePriceText.text = "Price:\n" + ( __droneUpgrades._GetBasicStats()._shorteningTimeBaseCost * ( __droneUpgrades._GetBasicStats()._shorteningCoolingDownTimeLevel + 1 ) ).ToString();
        _decreaseTimeBaseText.text = "-" + __droneUpgrades._GetBasicStats()._baseShorteningTime.ToString() + "s";
    }

    void _SetupEnginePriceAndBaseActivation( DroneUpgrades __droneUpgrade )
    {
        if ( __droneUpgrade._GetBasicStats()._engineLevel >= __droneUpgrade._GetBasicStats()._maxEngineLvl )
        {
            _upgEngPriceText.gameObject.SetActive( false );
            _upgEngBasePointsText.gameObject.SetActive( false );
        }
        else
        {
            _upgEngPriceText.gameObject.SetActive( true );
            _upgEngBasePointsText.gameObject.SetActive( true );
        }
            
    }

    void _SetupDecreaseTimePriceAndBaseActivation( DroneUpgrades __droneUpgrades )
    {
        if ( __droneUpgrades._GetBasicStats()._shorteningCoolingDownTimeLevel >= __droneUpgrades._GetBasicStats()._maxShorteningTimeLevel )
        {
            _decreaseTimePriceText.gameObject.SetActive( false );
            _decreaseTimeBaseText.gameObject.SetActive( false );
        }
        else
        {
            _decreaseTimePriceText.gameObject.SetActive( true );
            _decreaseTimeBaseText.gameObject.SetActive( true );
        }
            
    }

    void _SetExtOnTargetNeededPointsText( DroneUpgrades __droneUpgrades )
    {
        _extNeededPointsTxt.text = "Needed points:\n" + __droneUpgrades._GetDroneSkills()._extinctionOnReachingTargetPointsCost;
    }

    void _SetExtOnTargetPriceText( DroneUpgrades __droneUpgrade )
    {
        _extPriceText.text = "Price:\n" + __droneUpgrade._GetDroneSkills()._extinctionOnReachingTargetMoneyCost;
    }

    void _SetupExtOnTargetNeededPointsText( DroneUpgrades __droneUpgrades )
    {
        if ( __droneUpgrades._GetDroneSkills()._extinctionOnReachingTargetIsActive )
            _extNeededPointsTxt.gameObject.SetActive( false );
        else
            _extNeededPointsTxt.gameObject.SetActive( true );
    }

    void _SetupExtOnTargetPriceText( DroneUpgrades __droneUpgrades )
    {
        if ( __droneUpgrades._GetDroneSkills()._extinctionOnReachingTargetIsActive || __droneUpgrades._GetDroneSkills()._extinctionOnReachingTargetHasBeenBought)
            _extPriceText.gameObject.SetActive( false );
        else
            _extPriceText.gameObject.SetActive( true );
    }

    void _SetExtOnTargetPriceAndSetupPointsAndPrice( DroneUpgrades __droneUpgrades)
    {
        _SetExtOnTargetPriceText( __droneUpgrades );
        _SetupExtOnTargetNeededPointsText( __droneUpgrades );
        _SetupExtOnTargetPriceText( __droneUpgrades );
    }
    #endregion

    #region Removers

    void _RemoveAllListenersFromButtons( Button[] __buttons )
    {
        foreach ( var __button in __buttons )
        {
            __button.onClick.RemoveAllListeners();
        }
    }

    void _RemoveAllListenersFromNav()
    {
        _nextButton.onClick.RemoveAllListeners();
        _previousButton.onClick.RemoveAllListeners();
    }

    #endregion

    #endregion

    #region Firefighter drone

    public void _ActivateFirefighterDroneUpgrades()
    {
        _UpdateMoneyAndPoints( _fireFighterDroneUpg );

        _SetActiveButtons( _lvlTexts, 0 );
        _RemoveAllListenersFromButtons( _upButtons );
        _RemoveAllListenersFromButtons( _downButtons );

        _upButtons[ 0 ].onClick.AddListener( new UnityAction( _IncreaseArmorFireFDrone ) );
        _upButtons[ 1 ].onClick.AddListener( new UnityAction( _IncreaseSpeedFireFDrone ) );
        _upButtons[ 2 ].onClick.AddListener( new UnityAction( _IncreaseAmmo ) );
        _upButtons[ 3 ].onClick.AddListener( new UnityAction( _IncreaseExtincionSize ) );

        _downButtons[ 0 ].onClick.AddListener( new UnityAction( _DecreaseArmorFireFDrone ) );
        _downButtons[ 1 ].onClick.AddListener( new UnityAction( _DecreaseSpeedFireFDrone ) );
        _downButtons[ 2 ].onClick.AddListener( new UnityAction( _DecreaseAmmo ) );
        _downButtons[ 3 ].onClick.AddListener( new UnityAction( _DecreaseExtincionSize ) );

        _footersTexts[ 0 ].text = "Armor";
        _footersTexts[ 1 ].text = "Speed";
        _footersTexts[ 2 ].text = "Ammo";
        _footersTexts[ 3 ].text = "Extinction size";

        _UpdateFirefighterDrArmorText();
        _UpdateFirefighterDrSpeedText();
        _UpdateFirefighterDrAmmoText();
        _UpdateFirefighterDrExtSizeText();

        _upgEngButton.gameObject.SetActive( true );
        _decreaseTimeButton.gameObject.SetActive( true );
        _upgEngButton.onClick.RemoveAllListeners();
        _decreaseTimeButton.onClick.RemoveAllListeners();
        _upgEngButton.onClick.AddListener( new UnityAction( _UpgradeEngineFireFDrone ) );
        _decreaseTimeButton.onClick.AddListener( new UnityAction( _UpgradeShorteningTimeFireFDrone ) );

        _SetEngineLvlPriceAndBaseText( _fireFighterDroneUpg );
        _SetupEnginePriceAndBaseActivation( _fireFighterDroneUpg );
        _SetDecreaseTimeAmountPriceAndBase( _fireFighterDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _fireFighterDroneUpg );

        _extinctionButton.gameObject.SetActive( false );
        _droneImage.sprite = _fireFiDrSprite;

        _RemoveAllListenersFromNav();
        _nextButton.onClick.AddListener( new UnityAction( _ActivateAmbulanceDroneUpgrades ) );
    }



    private void _UpdateFirefighterDrArmorText()
    {
        _SetLvlText0( _fireFighterDroneUpg._GetBasicStats()._armorLvl);
    }

    private void _UpdateFirefighterDrSpeedText()
    {
        _SetLvlText1( _fireFighterDroneUpg._GetBasicStats()._speedLvl);
    }

    private void _UpdateFirefighterDrAmmoText()
    {
        _SetLvlText2( _fireFighterDroneUpg._FireFighterDroneLevels._ammoLvl);
    }

    private void _UpdateFirefighterDrExtSizeText()
    {
        _SetLvlText3( _fireFighterDroneUpg._FireFighterDroneLevels._sizeOfExtincionLvl);
    }
    #endregion

    #region Ambulance drone

    public void _ActivateAmbulanceDroneUpgrades()
    {
        _UpdateMoneyAndPoints( _ambulaceDroneUpg );

        _SetActiveButtons( _lvlTexts, 0 );
        _SetActiveButtons( _lvlTexts, 3, false );
        _RemoveAllListenersFromButtons( _upButtons );
        _RemoveAllListenersFromButtons( _downButtons );

        _upButtons[ 0 ].onClick.AddListener( new UnityAction( _IncreaseArmorAmbDrone ) );
        _upButtons[ 1 ].onClick.AddListener( new UnityAction( _IncreaseSpeedAmbDrone ) );
        _upButtons[ 2 ].onClick.AddListener( new UnityAction( _IncreaseSlowDownDyingLevel ) );

        _downButtons[ 0 ].onClick.AddListener( new UnityAction( _DecreaseArmorAmbDrone ) );
        _downButtons[ 1 ].onClick.AddListener( new UnityAction( _DecreaseSpeedAmbDrone ) );
        _downButtons[ 2 ].onClick.AddListener( new UnityAction( _DecreaseSlowDownDyingLevel ) );

        _footersTexts[ 0 ].text = "Armor";
        _footersTexts[ 1 ].text = "Speed";
        _footersTexts[ 2 ].text = "Slowing down dying of victim";

        _UpdateAmbulanceDrArmorText();
        _UpdateAmbulanceDrSpeedText();
        _UpdateAmbulanceDrSlowDownText();


        _upgEngButton.gameObject.SetActive( true );
        _decreaseTimeButton.gameObject.SetActive( true );
        _upgEngButton.onClick.RemoveAllListeners();
        _decreaseTimeButton.onClick.RemoveAllListeners();
        _upgEngButton.onClick.AddListener( new UnityAction( _UpgradeEngineAmbDrone ) );
        _decreaseTimeButton.onClick.AddListener( new UnityAction( _UpgradeShorteningTimeAmbDrone ) );

        _SetEngineLvlPriceAndBaseText( _ambulaceDroneUpg );
        _SetupEnginePriceAndBaseActivation( _ambulaceDroneUpg );
        _SetDecreaseTimeAmountPriceAndBase( _ambulaceDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _ambulaceDroneUpg );

        _extinctionButton.gameObject.SetActive( true );
        _SetColorOfExtOnTargetButton( _ambulaceDroneUpg );
        _extinctionButton.onClick.RemoveAllListeners();
        _extinctionButton.onClick.AddListener( new UnityAction( _ToogleExtincionOnTargetSkillAmbDrone ) );

        _SetExtOnTargetNeededPointsText( _ambulaceDroneUpg );
        _SetExtOnTargetPriceAndSetupPointsAndPrice( _ambulaceDroneUpg );

        _droneImage.sprite = _ambDrSprite;

        _RemoveAllListenersFromNav();
        _nextButton.onClick.AddListener( new UnityAction( _ActivateSupplyDroneUpgrades ) );
        _previousButton.onClick.AddListener( new UnityAction( _ActivateFirefighterDroneUpgrades ) );
    }

    private void _UpdateAmbulanceDrArmorText()
    {
        _SetLvlText0( _ambulaceDroneUpg._GetBasicStats()._armorLvl);
    }

    private void _UpdateAmbulanceDrSpeedText()
    {
        _SetLvlText1( _ambulaceDroneUpg._GetBasicStats()._speedLvl );
    }

    private void _UpdateAmbulanceDrSlowDownText()
    {
        _SetLvlText2( _ambulaceDroneUpg._AmbulanceDroneLevels._slowDownDyingLevel);
    }
    #endregion

    #region Supply drone

    public void _ActivateSupplyDroneUpgrades()
    {
        _UpdateMoneyAndPoints( _supplyDroneUpg );

        _SetActiveButtons( _lvlTexts, 0 );
        _RemoveAllListenersFromButtons( _upButtons );
        _RemoveAllListenersFromButtons( _downButtons );

        _upButtons[ 0 ].onClick.AddListener( new UnityAction( _IncreaseArmorSupDrone ) );
        _upButtons[ 1 ].onClick.AddListener( new UnityAction( _IncreaseSpeedSupDrone ) );
        _upButtons[ 2 ].onClick.AddListener( new UnityAction( _IncreaseSizeOfPackage ) );
        _upButtons[ 3 ].onClick.AddListener( new UnityAction( _IncreaseArmorPackage ) );

        _downButtons[ 0 ].onClick.AddListener( new UnityAction( _DecreaseArmorSupDrone ) );
        _downButtons[ 1 ].onClick.AddListener( new UnityAction( _DecreaseSpeedSupDrone ) );
        _downButtons[ 2 ].onClick.AddListener( new UnityAction( _DecreaseSizeOfPackage ) );
        _downButtons[ 3 ].onClick.AddListener( new UnityAction( _DecreaseArmorPackage ) );

        _footersTexts[ 0 ].text = "Armor";
        _footersTexts[ 1 ].text = "Speed";
        _footersTexts[ 2 ].text = "Package size";
        _footersTexts[ 3 ].text = "Package armor";

        _UpdateSupplyDrArmorText();
        _UpdateSupplyDrSpeedText();
        _UpdateSupplyDrPackageSizeText();
        _UpdateSupplyDrPackageArmorText();


        _upgEngButton.gameObject.SetActive( true );
        _decreaseTimeButton.gameObject.SetActive( true );
        _upgEngButton.onClick.RemoveAllListeners();
        _decreaseTimeButton.onClick.RemoveAllListeners();
        _upgEngButton.onClick.AddListener( new UnityAction( _UpgradeEngineSupDrone ) );
        _decreaseTimeButton.onClick.AddListener( new UnityAction( _UpgradeShorteningTimeSupDrone ) );

        _SetEngineLvlPriceAndBaseText( _supplyDroneUpg );
        _SetupEnginePriceAndBaseActivation( _supplyDroneUpg );
        _SetDecreaseTimeAmountPriceAndBase( _supplyDroneUpg );
        _SetupDecreaseTimePriceAndBaseActivation( _supplyDroneUpg );

        _extinctionButton.gameObject.SetActive( true );
        _SetColorOfExtOnTargetButton( _supplyDroneUpg );
        _extinctionButton.onClick.RemoveAllListeners();
        _extinctionButton.onClick.AddListener( new UnityAction( _ToogleExtincionOnTargetSkillSupDrone ) );

        _SetExtOnTargetNeededPointsText( _supplyDroneUpg );
        _SetExtOnTargetPriceAndSetupPointsAndPrice( _supplyDroneUpg );

        _droneImage.sprite = _supDrSprite;

        _RemoveAllListenersFromNav();
        _previousButton.onClick.AddListener( new UnityAction( _ActivateAmbulanceDroneUpgrades ) );
    }

    private void _UpdateSupplyDrArmorText()
    {
        _SetLvlText0( _supplyDroneUpg._GetBasicStats()._armorLvl);
    }

    private void _UpdateSupplyDrSpeedText()
    {
        _SetLvlText1( _supplyDroneUpg._GetBasicStats()._speedLvl);
    }

    private void _UpdateSupplyDrPackageSizeText()
    {
        _SetLvlText2( _supplyDroneUpg._SupplyDroneLvls._sizeOfPackage );
    }

    private void _UpdateSupplyDrPackageArmorText()
    {
        _SetLvlText3( _supplyDroneUpg._SupplyDroneLvls._armorOfPackage );
    }

    #endregion

    #endregion
}

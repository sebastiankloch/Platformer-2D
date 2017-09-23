using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using Drones;

public class UpgradesUIController : MonoBehaviour
{
    public static UpgradesUIController upg;

    public MainMenu mMComp;

    public int money;
    public DroneFireFighterUpgrades fireFighterDroneUpg;
    public DroneAmbulanceUpgrades ambulaceDroneUpg;
    public DroneSupplyUpgrades supplyDroneUpg;
    [Space]
    public Text moneyText;
    public Text pointsText;
    [Header("Levels")]
    public Button[] upButtons;
    public Text[] lvlTexts;
    public Button[] downButtons;
    public Text[] footersTexts;
    [Header("Pernament")]
    public Button upgEngButton;
    public Text engineLvlText;
    public Text upgEngPriceText;
    public Text upgEngBasePointsText;

    public Button decreaseTimeButton;
    public Text decreasingTimeAmountText;
    public Text decreaseTimePriceText;
    public Text decreaseTimeBaseText;
    [Header("Skills")]
    public Button extinctionButton;
    public Text extNeededPointsTxt;
    public Text extPriceText;
    [Space]
    public Image droneImage;
    [Header("Navigation")]
    public Button nextButton;
    public Button previousButton;
    [Space]
    public Sprite fireFiDrSprite;
    public Sprite ambDrSprite;
    public Sprite supDrSprite;


    private void Awake()
    {
        upg = this;
    }

    #region Navigation

    public void BackToMainMenu()
    {
        mMComp.enabled = true;
        gameObject.SetActive( false );
    }

    public void SaveDataAndBackToMenu()
    {
        SendDataToGameController();
        GameController.gameCont.SavePlayersData();
        GameController.gameCont.SaveUpgradesData();
        BackToMainMenu();
    }

    #endregion

    #region Firefighter Drone

    public void IncreaseArmorFireFDrone()
    {
        fireFighterDroneUpg.IncreaseArmor();
        UpdateFirefighterDrArmorText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }
    public void DecreaseArmorFireFDrone()
    {
        fireFighterDroneUpg.DecreaseArmor();
        UpdateFirefighterDrArmorText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void IncreaseSpeedFireFDrone()
    {
        fireFighterDroneUpg.IncreaseSpeed();
        UpdateFirefighterDrSpeedText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void DecreaseSpeedFireFDrone()
    {
        fireFighterDroneUpg.DecreaseSpeed();
        UpdateFirefighterDrSpeedText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void UpgradeShorteningTimeFireFDrone()
    {
        fireFighterDroneUpg.UpgradeShorteningTime( ref money );
        SetDecreaseTimeAmountPriceAndBase( fireFighterDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( fireFighterDroneUpg );
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void UpgradeEngineFireFDrone()
    {
        fireFighterDroneUpg.UpgradeEngine( ref money );
        SetEngineLvlPriceAndBaseText( fireFighterDroneUpg );
        SetupEnginePriceAndBaseActivation( fireFighterDroneUpg );
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void IncreaseAmmo()
    {
        fireFighterDroneUpg.IncreaseAmmo();
        UpdateFirefighterDrAmmoText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void DecreaseAmmo()
    {
        fireFighterDroneUpg.DecreaseAmmo();
        UpdateFirefighterDrAmmoText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void IncreaseExtincionSize()
    {
        fireFighterDroneUpg.IncreaseExtincionSize();
        UpdateFirefighterDrExtSizeText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    public void DecreaseExtincionSize()
    {
        fireFighterDroneUpg.DecreaseExtincionSize();
        UpdateFirefighterDrExtSizeText();
        UpdateMoneyAndPoints( fireFighterDroneUpg );
    }

    #endregion

    #region Ambulance drone

    public void IncreaseArmorAmbDrone()
    {
        ambulaceDroneUpg.IncreaseArmor();
        UpdateAmbulanceDrArmorText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }
    public void DecreaseArmorAmbDrone()
    {
        ambulaceDroneUpg.DecreaseArmor();
        UpdateAmbulanceDrArmorText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void IncreaseSpeedAmbDrone()
    {
        ambulaceDroneUpg.IncreaseSpeed();
        UpdateAmbulanceDrSpeedText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void DecreaseSpeedAmbDrone()
    {
        ambulaceDroneUpg.DecreaseSpeed();
        UpdateAmbulanceDrSpeedText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void UpgradeShorteningTimeAmbDrone()
    {
        ambulaceDroneUpg.UpgradeShorteningTime( ref money );
        SetDecreaseTimeAmountPriceAndBase( ambulaceDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( ambulaceDroneUpg );
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void UpgradeEngineAmbDrone()
    {
        ambulaceDroneUpg.UpgradeEngine( ref money );
        SetEngineLvlPriceAndBaseText( ambulaceDroneUpg );
        SetupEnginePriceAndBaseActivation( ambulaceDroneUpg );
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void ToogleExtincionOnTargetSkillAmbDrone()
    {
        ambulaceDroneUpg.ToogleExtincionOnTargetSkill( ref money );
        SetColorOfExtOnTargetButton( ambulaceDroneUpg );
        SetExtOnTargetPriceAndSetupPointsAndPrice( ambulaceDroneUpg );
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void IncreaseSlowDownDyingLevel()
    {
        ambulaceDroneUpg.IncreaseSlowDownDyingLevel();
        UpdateAmbulanceDrSlowDownText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    public void DecreaseSlowDownDyingLevel()
    {
        ambulaceDroneUpg.DecreaseSlowDownDyingLevel();
        UpdateAmbulanceDrSlowDownText();
        UpdateMoneyAndPoints( ambulaceDroneUpg );
    }

    #endregion

    #region Supply drone

    public void IncreaseArmorSupDrone()
    {
        supplyDroneUpg.IncreaseArmor();
        UpdateSupplyDrArmorText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }
    public void DecreaseArmorSupDrone()
    {
        supplyDroneUpg.DecreaseArmor();
        UpdateSupplyDrArmorText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void IncreaseSpeedSupDrone()
    {
        supplyDroneUpg.IncreaseSpeed();
        UpdateSupplyDrSpeedText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void DecreaseSpeedSupDrone()
    {
        supplyDroneUpg.DecreaseSpeed();
        UpdateSupplyDrSpeedText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void UpgradeShorteningTimeSupDrone()
    {
        supplyDroneUpg.UpgradeShorteningTime( ref money );
        SetDecreaseTimeAmountPriceAndBase( supplyDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( supplyDroneUpg );
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void UpgradeEngineSupDrone()
    {
        supplyDroneUpg.UpgradeEngine( ref money );
        SetEngineLvlPriceAndBaseText( supplyDroneUpg );
        SetupEnginePriceAndBaseActivation( supplyDroneUpg );
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void ToogleExtincionOnTargetSkillSupDrone()
    {
        supplyDroneUpg.ToogleExtincionOnTargetSkill( ref money );
        SetColorOfExtOnTargetButton( supplyDroneUpg );
        SetExtOnTargetPriceAndSetupPointsAndPrice( supplyDroneUpg );
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void IncreaseSizeOfPackage()
    {
        supplyDroneUpg.IncreaseSizeOfPackage();
        UpdateSupplyDrPackageSizeText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void DecreaseSizeOfPackage()
    {
        supplyDroneUpg.DecreaseSizeOfPackage();
        UpdateSupplyDrPackageSizeText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void IncreaseArmorPackage()
    {
        supplyDroneUpg.IncreaseArmorPackage();
        UpdateSupplyDrPackageArmorText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    public void DecreaseArmorPackage()
    {
        supplyDroneUpg.DecreaseArmorPackage();
        UpdateSupplyDrPackageArmorText();
        UpdateMoneyAndPoints( supplyDroneUpg );
    }

    #endregion

    public void MakeSureThatDataIsCorrect()
    {
        fireFighterDroneUpg.MakeSureThatStatsAreCorrect();
        ambulaceDroneUpg.MakeSureThatStatsAreCorrect();
        supplyDroneUpg.MakeSureThatStatsAreCorrect();
    }

    #region Activation

    #region Getters and Senders

    public void GetDataFromGameController()
    {
        money = GameController.gameCont.playersData.money;
        fireFighterDroneUpg = new DroneFireFighterUpgrades(
            GameController.gameCont.droneFireFiUpg.GetBasicStats(),
            GameController.gameCont.droneFireFiUpg.GetDroneSkills(),
            GameController.gameCont.droneFireFiUpg.FireFighterDroneLevels );
        ambulaceDroneUpg = new DroneAmbulanceUpgrades(
            GameController.gameCont.droneAmbUpg.GetBasicStats(),
            GameController.gameCont.droneAmbUpg.GetDroneSkills(),
            GameController.gameCont.droneAmbUpg.AmbulanceDroneLevels );
        supplyDroneUpg = new DroneSupplyUpgrades(
            GameController.gameCont.droneSupUpg.GetBasicStats(),
            GameController.gameCont.droneSupUpg.GetDroneSkills(),
            GameController.gameCont.droneSupUpg.SupplyDroneLvls );
    }

    public void SendDataToGameController()
    {
        MakeSureThatDataIsCorrect();
        GameController.gameCont.playersData.money = money;
        GameController.gameCont.droneFireFiUpg = new DroneFireFighterUpgrades(
            fireFighterDroneUpg.GetBasicStats(),
            fireFighterDroneUpg.GetDroneSkills(),
            fireFighterDroneUpg.FireFighterDroneLevels );
        GameController.gameCont.droneAmbUpg = new DroneAmbulanceUpgrades(
            ambulaceDroneUpg.GetBasicStats(),
            ambulaceDroneUpg.GetDroneSkills(),
            ambulaceDroneUpg.AmbulanceDroneLevels );
        GameController.gameCont.droneSupUpg = new DroneSupplyUpgrades(
            supplyDroneUpg.GetBasicStats(),
            supplyDroneUpg.GetDroneSkills(),
            supplyDroneUpg.SupplyDroneLvls );
    }

    #endregion

    #region Setters and Removers

    #region Setters

    void UpdateMoneyAndPoints( DroneUpgrades droneUpgrades )
    {
        moneyText.text = "Money: " + money;
        pointsText.text = "Points: " + droneUpgrades.GetBasicStats().availablePoints;

    }

    void SetActiveButtons( Text[] buttons, int from, bool activate = true )
    {
        for ( int id = from ; id < buttons.GetLength( 0 ) ; id++ )
        {
            buttons[ id ].transform.parent.gameObject.SetActive( activate );
        }
    }

    void SetLvlText0( int value )
    {
        SetLvlText( 0, value );
    }

    void SetLvlText1( int value )
    {
        SetLvlText( 1, value );
    }

    void SetLvlText2( int value )
    {
        SetLvlText( 2, value );
    }

    void SetLvlText3( int value )
    {
        SetLvlText( 3, value );
    }

    void SetLvlText( int id, int value )
    {
        lvlTexts[ id ].text = ( value + 1 ).ToString();
    }

    void SetColorOfExtOnTargetButton( DroneUpgrades droneUpgrades )
    {
        bool active = droneUpgrades.GetDroneSkills().extinctionOnReachingTargetIsActive;
        if ( active )
            extinctionButton.GetComponent<Image>().color = Color.green;
        else
            extinctionButton.GetComponent<Image>().color = Color.white;
    }

    void SetEngineLvlPriceAndBaseText( DroneUpgrades droneUpgrades )
    {
        engineLvlText.text = "Lvl: " + ( droneUpgrades.GetBasicStats().engineLevel + 1 ).ToString();
        upgEngPriceText.text = "Price:\n" + ( droneUpgrades.GetBasicStats().engineBaseCost * ( droneUpgrades.GetBasicStats().engineLevel + 1 ) ).ToString();
        upgEngBasePointsText.text = "+" + droneUpgrades.GetBasicStats().pointsPerEngineLvl + " Points";
    }

    void SetDecreaseTimeAmountPriceAndBase( DroneUpgrades droneUpgrades )
    {
        decreasingTimeAmountText.text = "-" + ( droneUpgrades.GetBasicStats().baseShorteningTime * droneUpgrades.GetBasicStats().shorteningCoolingDownTimeLevel ).ToString() + "s";
        decreaseTimePriceText.text = "Price:\n" + ( droneUpgrades.GetBasicStats().shorteningTimeBaseCost * ( droneUpgrades.GetBasicStats().shorteningCoolingDownTimeLevel + 1 ) ).ToString();
        decreaseTimeBaseText.text = "-" + droneUpgrades.GetBasicStats().baseShorteningTime.ToString() + "s";
    }

    void SetupEnginePriceAndBaseActivation( DroneUpgrades droneUpgrade )
    {
        if ( droneUpgrade.GetBasicStats().engineLevel >= droneUpgrade.GetBasicStats().maxEngineLvl )
        {
            upgEngPriceText.gameObject.SetActive( false );
            upgEngBasePointsText.gameObject.SetActive( false );
        }
        else
        {
            upgEngPriceText.gameObject.SetActive( true );
            upgEngBasePointsText.gameObject.SetActive( true );
        }

    }

    void SetupDecreaseTimePriceAndBaseActivation( DroneUpgrades droneUpgrades )
    {
        if ( droneUpgrades.GetBasicStats().shorteningCoolingDownTimeLevel >= droneUpgrades.GetBasicStats().maxShorteningTimeLevel )
        {
            decreaseTimePriceText.gameObject.SetActive( false );
            decreaseTimeBaseText.gameObject.SetActive( false );
        }
        else
        {
            decreaseTimePriceText.gameObject.SetActive( true );
            decreaseTimeBaseText.gameObject.SetActive( true );
        }

    }

    void SetExtOnTargetNeededPointsText( DroneUpgrades droneUpgrades )
    {
        extNeededPointsTxt.text = "Needed points:\n" + droneUpgrades.GetDroneSkills().extinctionOnReachingTargetPointsCost;
    }

    void SetExtOnTargetPriceText( DroneUpgrades droneUpgrade )
    {
        extPriceText.text = "Price:\n" + droneUpgrade.GetDroneSkills().extinctionOnReachingTargetMoneyCost;
    }

    void SetupExtOnTargetNeededPointsText( DroneUpgrades droneUpgrades )
    {
        if ( droneUpgrades.GetDroneSkills().extinctionOnReachingTargetIsActive )
            extNeededPointsTxt.gameObject.SetActive( false );
        else
            extNeededPointsTxt.gameObject.SetActive( true );
    }

    void SetupExtOnTargetPriceText( DroneUpgrades droneUpgrades )
    {
        if ( droneUpgrades.GetDroneSkills().extinctionOnReachingTargetIsActive || droneUpgrades.GetDroneSkills().extinctionOnReachingTargetHasBeenBought )
            extPriceText.gameObject.SetActive( false );
        else
            extPriceText.gameObject.SetActive( true );
    }

    void SetExtOnTargetPriceAndSetupPointsAndPrice( DroneUpgrades droneUpgrades )
    {
        SetExtOnTargetPriceText( droneUpgrades );
        SetupExtOnTargetNeededPointsText( droneUpgrades );
        SetupExtOnTargetPriceText( droneUpgrades );
    }
    #endregion

    #region Removers

    void RemoveAllListenersFromButtons( Button[] buttons )
    {
        foreach ( var button in buttons )
        {
            button.onClick.RemoveAllListeners();
        }
    }

    void RemoveAllListenersFromNav()
    {
        nextButton.onClick.RemoveAllListeners();
        previousButton.onClick.RemoveAllListeners();
    }

    #endregion

    #endregion

    #region Firefighter drone

    public void ActivateFirefighterDroneUpgrades()
    {
        UpdateMoneyAndPoints( fireFighterDroneUpg );

        SetActiveButtons( lvlTexts, 0 );
        RemoveAllListenersFromButtons( upButtons );
        RemoveAllListenersFromButtons( downButtons );

        upButtons[ 0 ].onClick.AddListener( new UnityAction( IncreaseArmorFireFDrone ) );
        upButtons[ 1 ].onClick.AddListener( new UnityAction( IncreaseSpeedFireFDrone ) );
        upButtons[ 2 ].onClick.AddListener( new UnityAction( IncreaseAmmo ) );
        upButtons[ 3 ].onClick.AddListener( new UnityAction( IncreaseExtincionSize ) );

        downButtons[ 0 ].onClick.AddListener( new UnityAction( DecreaseArmorFireFDrone ) );
        downButtons[ 1 ].onClick.AddListener( new UnityAction( DecreaseSpeedFireFDrone ) );
        downButtons[ 2 ].onClick.AddListener( new UnityAction( DecreaseAmmo ) );
        downButtons[ 3 ].onClick.AddListener( new UnityAction( DecreaseExtincionSize ) );

        footersTexts[ 0 ].text = "Armor";
        footersTexts[ 1 ].text = "Speed";
        footersTexts[ 2 ].text = "Ammo";
        footersTexts[ 3 ].text = "Extinction size";

        UpdateFirefighterDrArmorText();
        UpdateFirefighterDrSpeedText();
        UpdateFirefighterDrAmmoText();
        UpdateFirefighterDrExtSizeText();

        upgEngButton.gameObject.SetActive( true );
        decreaseTimeButton.gameObject.SetActive( true );
        upgEngButton.onClick.RemoveAllListeners();
        decreaseTimeButton.onClick.RemoveAllListeners();
        upgEngButton.onClick.AddListener( new UnityAction( UpgradeEngineFireFDrone ) );
        decreaseTimeButton.onClick.AddListener( new UnityAction( UpgradeShorteningTimeFireFDrone ) );

        SetEngineLvlPriceAndBaseText( fireFighterDroneUpg );
        SetupEnginePriceAndBaseActivation( fireFighterDroneUpg );
        SetDecreaseTimeAmountPriceAndBase( fireFighterDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( fireFighterDroneUpg );

        extinctionButton.gameObject.SetActive( false );
        droneImage.sprite = fireFiDrSprite;

        RemoveAllListenersFromNav();
        nextButton.onClick.AddListener( new UnityAction( ActivateAmbulanceDroneUpgrades ) );
    }



    private void UpdateFirefighterDrArmorText()
    {
        SetLvlText0( fireFighterDroneUpg.GetBasicStats().armorLvl );
    }

    private void UpdateFirefighterDrSpeedText()
    {
        SetLvlText1( fireFighterDroneUpg.GetBasicStats().speedLvl );
    }

    private void UpdateFirefighterDrAmmoText()
    {
        SetLvlText2( fireFighterDroneUpg.FireFighterDroneLevels.ammoLvl );
    }

    private void UpdateFirefighterDrExtSizeText()
    {
        SetLvlText3( fireFighterDroneUpg.FireFighterDroneLevels.sizeOfExtincionLvl );
    }
    #endregion

    #region Ambulance drone

    public void ActivateAmbulanceDroneUpgrades()
    {
        UpdateMoneyAndPoints( ambulaceDroneUpg );

        SetActiveButtons( lvlTexts, 0 );
        SetActiveButtons( lvlTexts, 3, false );
        RemoveAllListenersFromButtons( upButtons );
        RemoveAllListenersFromButtons( downButtons );

        upButtons[ 0 ].onClick.AddListener( new UnityAction( IncreaseArmorAmbDrone ) );
        upButtons[ 1 ].onClick.AddListener( new UnityAction( IncreaseSpeedAmbDrone ) );
        upButtons[ 2 ].onClick.AddListener( new UnityAction( IncreaseSlowDownDyingLevel ) );

        downButtons[ 0 ].onClick.AddListener( new UnityAction( DecreaseArmorAmbDrone ) );
        downButtons[ 1 ].onClick.AddListener( new UnityAction( DecreaseSpeedAmbDrone ) );
        downButtons[ 2 ].onClick.AddListener( new UnityAction( DecreaseSlowDownDyingLevel ) );

        footersTexts[ 0 ].text = "Armor";
        footersTexts[ 1 ].text = "Speed";
        footersTexts[ 2 ].text = "Slowing down dying of victim";

        UpdateAmbulanceDrArmorText();
        UpdateAmbulanceDrSpeedText();
        UpdateAmbulanceDrSlowDownText();


        upgEngButton.gameObject.SetActive( true );
        decreaseTimeButton.gameObject.SetActive( true );
        upgEngButton.onClick.RemoveAllListeners();
        decreaseTimeButton.onClick.RemoveAllListeners();
        upgEngButton.onClick.AddListener( new UnityAction( UpgradeEngineAmbDrone ) );
        decreaseTimeButton.onClick.AddListener( new UnityAction( UpgradeShorteningTimeAmbDrone ) );

        SetEngineLvlPriceAndBaseText( ambulaceDroneUpg );
        SetupEnginePriceAndBaseActivation( ambulaceDroneUpg );
        SetDecreaseTimeAmountPriceAndBase( ambulaceDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( ambulaceDroneUpg );

        extinctionButton.gameObject.SetActive( true );
        SetColorOfExtOnTargetButton( ambulaceDroneUpg );
        extinctionButton.onClick.RemoveAllListeners();
        extinctionButton.onClick.AddListener( new UnityAction( ToogleExtincionOnTargetSkillAmbDrone ) );

        SetExtOnTargetNeededPointsText( ambulaceDroneUpg );
        SetExtOnTargetPriceAndSetupPointsAndPrice( ambulaceDroneUpg );

        droneImage.sprite = ambDrSprite;

        RemoveAllListenersFromNav();
        nextButton.onClick.AddListener( new UnityAction( ActivateSupplyDroneUpgrades ) );
        previousButton.onClick.AddListener( new UnityAction( ActivateFirefighterDroneUpgrades ) );
    }

    private void UpdateAmbulanceDrArmorText()
    {
        SetLvlText0( ambulaceDroneUpg.GetBasicStats().armorLvl );
    }

    private void UpdateAmbulanceDrSpeedText()
    {
        SetLvlText1( ambulaceDroneUpg.GetBasicStats().speedLvl );
    }

    private void UpdateAmbulanceDrSlowDownText()
    {
        SetLvlText2( ambulaceDroneUpg.AmbulanceDroneLevels.slowDownDyingLevel );
    }
    #endregion

    #region Supply drone

    public void ActivateSupplyDroneUpgrades()
    {
        UpdateMoneyAndPoints( supplyDroneUpg );

        SetActiveButtons( lvlTexts, 0 );
        RemoveAllListenersFromButtons( upButtons );
        RemoveAllListenersFromButtons( downButtons );

        upButtons[ 0 ].onClick.AddListener( new UnityAction( IncreaseArmorSupDrone ) );
        upButtons[ 1 ].onClick.AddListener( new UnityAction( IncreaseSpeedSupDrone ) );
        upButtons[ 2 ].onClick.AddListener( new UnityAction( IncreaseSizeOfPackage ) );
        upButtons[ 3 ].onClick.AddListener( new UnityAction( IncreaseArmorPackage ) );

        downButtons[ 0 ].onClick.AddListener( new UnityAction( DecreaseArmorSupDrone ) );
        downButtons[ 1 ].onClick.AddListener( new UnityAction( DecreaseSpeedSupDrone ) );
        downButtons[ 2 ].onClick.AddListener( new UnityAction( DecreaseSizeOfPackage ) );
        downButtons[ 3 ].onClick.AddListener( new UnityAction( DecreaseArmorPackage ) );

        footersTexts[ 0 ].text = "Armor";
        footersTexts[ 1 ].text = "Speed";
        footersTexts[ 2 ].text = "Package size";
        footersTexts[ 3 ].text = "Package armor";

        UpdateSupplyDrArmorText();
        UpdateSupplyDrSpeedText();
        UpdateSupplyDrPackageSizeText();
        UpdateSupplyDrPackageArmorText();


        upgEngButton.gameObject.SetActive( true );
        decreaseTimeButton.gameObject.SetActive( true );
        upgEngButton.onClick.RemoveAllListeners();
        decreaseTimeButton.onClick.RemoveAllListeners();
        upgEngButton.onClick.AddListener( new UnityAction( UpgradeEngineSupDrone ) );
        decreaseTimeButton.onClick.AddListener( new UnityAction( UpgradeShorteningTimeSupDrone ) );

        SetEngineLvlPriceAndBaseText( supplyDroneUpg );
        SetupEnginePriceAndBaseActivation( supplyDroneUpg );
        SetDecreaseTimeAmountPriceAndBase( supplyDroneUpg );
        SetupDecreaseTimePriceAndBaseActivation( supplyDroneUpg );

        extinctionButton.gameObject.SetActive( true );
        SetColorOfExtOnTargetButton( supplyDroneUpg );
        extinctionButton.onClick.RemoveAllListeners();
        extinctionButton.onClick.AddListener( new UnityAction( ToogleExtincionOnTargetSkillSupDrone ) );

        SetExtOnTargetNeededPointsText( supplyDroneUpg );
        SetExtOnTargetPriceAndSetupPointsAndPrice( supplyDroneUpg );

        droneImage.sprite = supDrSprite;

        RemoveAllListenersFromNav();
        previousButton.onClick.AddListener( new UnityAction( ActivateAmbulanceDroneUpgrades ) );
    }

    private void UpdateSupplyDrArmorText()
    {
        SetLvlText0( supplyDroneUpg.GetBasicStats().armorLvl );
    }

    private void UpdateSupplyDrSpeedText()
    {
        SetLvlText1( supplyDroneUpg.GetBasicStats().speedLvl );
    }

    private void UpdateSupplyDrPackageSizeText()
    {
        SetLvlText2( supplyDroneUpg.SupplyDroneLvls.sizeOfPackage );
    }

    private void UpdateSupplyDrPackageArmorText()
    {
        SetLvlText3( supplyDroneUpg.SupplyDroneLvls.armorOfPackage );
    }

    #endregion

    #endregion
}

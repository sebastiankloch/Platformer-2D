using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireBrigade;

namespace Drones
{
    #region Stats

    [Serializable]
    public class Stats
    {
        public int id;
        public int strength;
        public float speed;
        public float coolDownTime;

        public bool readyToAction = true;
        public bool inUse = false;
        public bool destroyed = false;

        public Stats( int id, int strength, float speed, float coolDownTime )
        {
            this.id = id;
            strength = strength;
            speed = speed;
            coolDownTime = coolDownTime;
        }

        public void TakeDamage( int damage )
        {
            strength -= damage;
            if ( strength <= 0 )
                destroyed = true;
        }
    }

    [Serializable]
    public class FirefiStats : Stats
    {

        public float extSize;
        public int ammo;

        public FirefiStats( int id, int strength, float speed, float coolDownTime, float extSize, int ammo ) : base( id, strength, speed, coolDownTime )
        {
            extSize = extSize;
            ammo = ammo;
        }
    }

    [Serializable]
    public class AmbStats : Stats
    {
        public float slowingDownDyingPer;

        public AmbStats( int id, int strength, float speed, float coolDownTime, float slowingDownDyingPer ) : base( id, strength, speed, coolDownTime )
        {
            slowingDownDyingPer = slowingDownDyingPer;
        }
    }

    [Serializable]
    public class SupStats : Stats
    {
        public int ammoPackStrength;

        public SupStats( int id, int strength, float speed, float coolDownTime, int ammoPackStrength ) : base( id, strength, speed, coolDownTime )
        {
            ammoPackStrength = ammoPackStrength;
        }
    }

    #endregion

    #region Data

    [Serializable]
    public struct BasicDroneLevels
    {
        public int availablePoints;
        public int ownedPoints;
        [Space]
        public int armorLvl;
        public int maxArmorLvl;
        [Space]
        public int speedLvl;
        public int maxSpeedLvl;
        [Space]
        public int maxShorteningTimeLevel;
        public int shorteningCoolingDownTimeLevel;
        public int shorteningTimeBaseCost;
        public float baseShorteningTime;
        [Space]
        public int engineLevel;
        public int maxEngineLvl;
        public int engineBaseCost;
        public int pointsPerEngineLvl;
    }

    [Serializable]
    public struct FirefighterDroneLevels
    {
        public int ammoLvl;
        public int maxAmmo;
        [Space]
        public int sizeOfExtincionLvl;
        public int maxLvLOfSizeOfExtinction;
    }

    [Serializable]
    public struct AmbulanceDroneLevels
    {
        public int slowDownDyingLevel;
        public int maxSlowDownDecreasing;
    }

    [Serializable]
    public struct SupplyDroneLevels
    {
        public int sizeOfPackage;
        public int maxSizeOfPackage;
        [Space]
        public int armorOfPackage;
        public int maxArmorOfPackage;
    }

    [Serializable]
    public struct DronesSkills
    {
        public bool extinctionOnReachingTargetIsActive;
        public int extinctionOnReachingTargetPointsCost;
        public int extinctionOnReachingTargetMoneyCost;
        public bool extinctionOnReachingTargetHasBeenBought;
    }

    [Serializable]
    public class UpgradesToSave
    {
        public DroneFireFighterUpgrades droneFireFighterUpgrades;
        public DroneAmbulanceUpgrades droneAmbulanceUpgrades;
        public DroneSupplyUpgrades droneSupplyUpgrades;
        public FireBrigadeUpgrades fireBrigadeUpgrades;

        public UpgradesToSave( DroneFireFighterUpgrades droneFireFighterUpgrades, DroneAmbulanceUpgrades droneAmbulanceUpgrades, DroneSupplyUpgrades droneSupplyUpgrades, FireBrigadeUpgrades fireBrigadeUpgrades )
        {
            droneFireFighterUpgrades = droneFireFighterUpgrades;
            droneAmbulanceUpgrades = droneAmbulanceUpgrades;
            droneSupplyUpgrades = droneSupplyUpgrades;
            fireBrigadeUpgrades = fireBrigadeUpgrades;
        }
    }
    #endregion

    [Serializable]
    public class DroneUpgrades
    {
        protected BasicDroneLevels basicDroneLvls = new BasicDroneLevels();
        protected DronesSkills droneSkills = new DronesSkills();

        public DroneUpgrades()
        {

        }

        public DroneUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills )
        {
            basicDroneLvls = basicDroneStats;
            droneSkills = droneSkills;
        }

        public BasicDroneLevels GetBasicStats()
        {
            return basicDroneLvls;
        }

        public DronesSkills GetDroneSkills()
        {
            return droneSkills;
        }

        protected void IncreaseStats( ref int curStats, int maxStats )
        {
            if ( basicDroneLvls.availablePoints > 0 )
            {
                if ( curStats + 1 <= maxStats )
                {
                    curStats++;
                    basicDroneLvls.availablePoints--;
                }
            }
        }

        protected void DecreaseStats( ref int lvl )
        {
            if ( lvl > 0 )
            {
                lvl--;
                basicDroneLvls.availablePoints++;
            }
        }

        public void IncreaseArmor()
        {
            IncreaseStats( ref basicDroneLvls.armorLvl, basicDroneLvls.maxArmorLvl );
        }

        public void DecreaseArmor()
        {
            DecreaseStats( ref basicDroneLvls.armorLvl );
        }

        public void IncreaseSpeed()
        {
            IncreaseStats( ref basicDroneLvls.speedLvl, basicDroneLvls.maxSpeedLvl );
        }

        public void DecreaseSpeed()
        {
            DecreaseStats( ref basicDroneLvls.speedLvl );
        }

        protected bool UpgradeStats( ref int curLvl, int maxLvl, ref int money, int baseCost )
        {
            if ( curLvl + 1 <= maxLvl
                && money >= baseCost * ( curLvl + 1 ) )
            {
                curLvl++;
                money -= baseCost * curLvl;
                return true;
            }
            return false;
        }

        public void UpgradeShorteningTime( ref int money )
        {
            UpgradeStats(
                ref basicDroneLvls.shorteningCoolingDownTimeLevel,
                basicDroneLvls.maxShorteningTimeLevel,
                ref money,
                basicDroneLvls.shorteningTimeBaseCost );
        }

        public void UpgradeEngine( ref int money )
        {
            if ( UpgradeStats(
                ref basicDroneLvls.engineLevel,
                basicDroneLvls.maxEngineLvl,
                ref money,
                basicDroneLvls.engineBaseCost
                ) )
            {
                basicDroneLvls.ownedPoints += basicDroneLvls.pointsPerEngineLvl;
                basicDroneLvls.availablePoints += basicDroneLvls.pointsPerEngineLvl;
            }
        }

        public void ToogleExtincionOnTargetSkill( ref int money )
        {
            if ( droneSkills.extinctionOnReachingTargetIsActive )
            {
                droneSkills.extinctionOnReachingTargetIsActive = false;
                basicDroneLvls.availablePoints += droneSkills.extinctionOnReachingTargetPointsCost;
            }
            else
            {
                if ( basicDroneLvls.availablePoints - droneSkills.extinctionOnReachingTargetPointsCost >= 0 )
                {
                    if ( !droneSkills.extinctionOnReachingTargetHasBeenBought && money - droneSkills.extinctionOnReachingTargetMoneyCost >= 0 )
                    {
                        money -= droneSkills.extinctionOnReachingTargetMoneyCost;
                        droneSkills.extinctionOnReachingTargetHasBeenBought = true;
                    }

                    droneSkills.extinctionOnReachingTargetIsActive = true;
                    basicDroneLvls.availablePoints -= droneSkills.extinctionOnReachingTargetPointsCost;
                }
            }
        }

        private bool StatsAreSmallerOrEqualToOwnedPoints( int usedPoints )
        {
            return usedPoints <= basicDroneLvls.ownedPoints;
        }

        protected virtual int GiveNrOfUsedPoints()
        {
            int usedPoints = 0;
            usedPoints += basicDroneLvls.armorLvl;
            usedPoints += basicDroneLvls.speedLvl;
            if ( droneSkills.extinctionOnReachingTargetIsActive )
                usedPoints += droneSkills.extinctionOnReachingTargetPointsCost;
            return usedPoints;
        }

        public void MakeSureThatStatsAreCorrect()
        {
            if ( !StatsAreSmallerOrEqualToOwnedPoints( GiveNrOfUsedPoints() ) )
            {
                ResetStatsAndGiveBackPoints();
            }

            if ( basicDroneLvls.shorteningCoolingDownTimeLevel > basicDroneLvls.maxShorteningTimeLevel )
            {
                basicDroneLvls.shorteningCoolingDownTimeLevel = basicDroneLvls.maxShorteningTimeLevel;
            }
        }

        protected virtual void ResetStatsAndGiveBackPoints()
        {
            basicDroneLvls.availablePoints = basicDroneLvls.ownedPoints;
            basicDroneLvls.armorLvl = 0;
            basicDroneLvls.speedLvl = 0;
        }
    }

    [Serializable]
    public class DroneFireFighterUpgrades : DroneUpgrades
    {
        protected FirefighterDroneLevels fireFighterDroneLevels = new FirefighterDroneLevels();

        public DroneFireFighterUpgrades()
        {
            basicDroneLvls = new BasicDroneLevels();
            droneSkills = new DronesSkills();
            fireFighterDroneLevels = new FirefighterDroneLevels();
        }

        public DroneFireFighterUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, FirefighterDroneLevels fireFighterDroneStats ) : base( basicDroneStats, droneSkills )
        {
            basicDroneLvls = basicDroneStats;
            fireFighterDroneLevels = fireFighterDroneStats;
            droneSkills = droneSkills;
        }

        public FirefighterDroneLevels FireFighterDroneLevels
        {
            get
            {
                return fireFighterDroneLevels;
            }
        }

        public void IncreaseAmmo()
        {
            IncreaseStats( ref fireFighterDroneLevels.ammoLvl, fireFighterDroneLevels.maxAmmo );
        }

        public void DecreaseAmmo()
        {
            DecreaseStats( ref fireFighterDroneLevels.ammoLvl );
        }

        public void IncreaseExtincionSize()
        {
            IncreaseStats( ref fireFighterDroneLevels.sizeOfExtincionLvl, fireFighterDroneLevels.maxLvLOfSizeOfExtinction );
        }

        public void DecreaseExtincionSize()
        {
            DecreaseStats( ref fireFighterDroneLevels.sizeOfExtincionLvl );
        }

        protected override int GiveNrOfUsedPoints()
        {
            int nrOfUsedPoints = base.GiveNrOfUsedPoints();
            nrOfUsedPoints += fireFighterDroneLevels.ammoLvl;
            nrOfUsedPoints += fireFighterDroneLevels.sizeOfExtincionLvl;
            return nrOfUsedPoints;
        }

        protected override void ResetStatsAndGiveBackPoints()
        {
            base.ResetStatsAndGiveBackPoints();
            fireFighterDroneLevels.ammoLvl = 0;
            fireFighterDroneLevels.sizeOfExtincionLvl = 0;
        }
    }

    [Serializable]
    public class DroneAmbulanceUpgrades : DroneUpgrades
    {
        protected AmbulanceDroneLevels ambulanceDroneLevels = new AmbulanceDroneLevels();

        public DroneAmbulanceUpgrades()
        {
            basicDroneLvls = new BasicDroneLevels();
            droneSkills = new DronesSkills();
            ambulanceDroneLevels = new AmbulanceDroneLevels();
        }

        public DroneAmbulanceUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, AmbulanceDroneLevels ambulanceDroneStats ) : base( basicDroneStats, droneSkills )
        {
            basicDroneLvls = basicDroneStats;
            droneSkills = droneSkills;
            ambulanceDroneLevels = ambulanceDroneStats;
        }

        public AmbulanceDroneLevels AmbulanceDroneLevels
        {
            get
            {
                return ambulanceDroneLevels;
            }
        }

        public void IncreaseSlowDownDyingLevel()
        {
            IncreaseStats( ref ambulanceDroneLevels.slowDownDyingLevel, ambulanceDroneLevels.maxSlowDownDecreasing );
        }

        public void DecreaseSlowDownDyingLevel()
        {
            DecreaseStats( ref ambulanceDroneLevels.slowDownDyingLevel );
        }

        protected override int GiveNrOfUsedPoints()
        {
            int nrOfUsedPoints = base.GiveNrOfUsedPoints();
            nrOfUsedPoints = ambulanceDroneLevels.slowDownDyingLevel;
            return nrOfUsedPoints;
        }

        protected override void ResetStatsAndGiveBackPoints()
        {
            base.ResetStatsAndGiveBackPoints();
            ambulanceDroneLevels.slowDownDyingLevel = 0;
        }
    }

    [Serializable]
    public class DroneSupplyUpgrades : DroneUpgrades
    {
        SupplyDroneLevels supplyDroneLevels = new SupplyDroneLevels();

        public DroneSupplyUpgrades()
        {
            basicDroneLvls = new BasicDroneLevels();
            droneSkills = new DronesSkills();
            supplyDroneLevels = new SupplyDroneLevels();
        }

        public DroneSupplyUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, SupplyDroneLevels supplyDroneStats ) : base( basicDroneStats, droneSkills )
        {
            basicDroneLvls = basicDroneStats;
            droneSkills = droneSkills;
            supplyDroneLevels = supplyDroneStats;
        }

        public SupplyDroneLevels SupplyDroneLvls
        {
            get
            {
                return supplyDroneLevels;
            }
        }

        public void IncreaseSizeOfPackage()
        {
            IncreaseStats( ref supplyDroneLevels.sizeOfPackage, supplyDroneLevels.maxSizeOfPackage );
        }

        public void DecreaseSizeOfPackage()
        {
            DecreaseStats( ref supplyDroneLevels.sizeOfPackage );
        }

        public void IncreaseArmorPackage()
        {
            IncreaseStats( ref supplyDroneLevels.armorOfPackage, supplyDroneLevels.maxArmorOfPackage );
        }

        public void DecreaseArmorPackage()
        {
            DecreaseStats( ref supplyDroneLevels.armorOfPackage );
        }

        protected override int GiveNrOfUsedPoints()
        {
            int nrOfUsedPoints = base.GiveNrOfUsedPoints();
            nrOfUsedPoints += supplyDroneLevels.sizeOfPackage;
            nrOfUsedPoints += supplyDroneLevels.armorOfPackage;
            return nrOfUsedPoints;
        }

        protected override void ResetStatsAndGiveBackPoints()
        {
            base.ResetStatsAndGiveBackPoints();
            supplyDroneLevels.sizeOfPackage = 0;
            supplyDroneLevels.armorOfPackage = 0;
        }
    }
}

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
        public int _id;
        public int _strength;
        public float _speed;
        public float _coolDownTime;

        public bool _readyToAction = true;
        public bool _inUse = false;
        public bool _destroyed = false;

        public Stats(int _id, int strength, float speed, float coolDownTime )
        {
            this._id = _id;
            _strength = strength;
            _speed = speed;
            _coolDownTime = coolDownTime;
        }

        public void _TakeDamage( int __damage )
        {
            _strength -= __damage;
            if ( _strength <= 0 )
                _destroyed = true;
        }
    }

    [Serializable]
    public class FirefiStats : Stats
    {
        
        public float _extSize;
        public int _ammo;

        public FirefiStats( int _id, int strength, float speed, float coolDownTime, float extSize, int ammo ) : base( _id, strength, speed, coolDownTime )
        {
            _extSize = extSize;
            _ammo = ammo;
        }
    }

    [Serializable]
    public class AmbStats : Stats
    {
        public float _slowingDownDyingPer;

        public AmbStats( int _id, int strength, float speed, float coolDownTime, float slowingDownDyingPer ) : base( _id, strength, speed, coolDownTime )
        {
            _slowingDownDyingPer = slowingDownDyingPer;
        }
    }

    [Serializable]
    public class SupStats : Stats
    {
        public int _ammoPackStrength;

        public SupStats( int _id, int strength, float speed, float coolDownTime, int ammoPackStrength ) : base( _id, strength, speed, coolDownTime )
        {
            _ammoPackStrength = ammoPackStrength;
        }
    }

    #endregion

    #region Data

    [Serializable]
    public struct BasicDroneLevels
    {
        public int _availablePoints;
        public int _ownedPoints;
        [Space]
        public int _armorLvl;
        public int _maxArmorLvl;
        [Space]
        public int _speedLvl;
        public int _maxSpeedLvl;
        [Space]
        public int _maxShorteningTimeLevel;
        public int _shorteningCoolingDownTimeLevel;
        public int _shorteningTimeBaseCost;
        public float _baseShorteningTime;
        [Space]
        public int _engineLevel;
        public int _maxEngineLvl;
        public int _engineBaseCost;
        public int _pointsPerEngineLvl;
    }

    [Serializable]
    public struct FirefighterDroneLevels
    {
        public int _ammoLvl;
        public int _maxAmmo;
        [Space]
        public int _sizeOfExtincionLvl;
        public int _maxLvLOfSizeOfExtinction;
    }

    [Serializable]
    public struct AmbulanceDroneLevels
    {
        public int _slowDownDyingLevel;
        public int _maxSlowDownDecreasing;
    }

    [Serializable]
    public struct SupplyDroneLevels
    {
        public int _sizeOfPackage;
        public int _maxSizeOfPackage;
        [Space]
        public int _armorOfPackage;
        public int _maxArmorOfPackage;
    }

    [Serializable]
    public struct DronesSkills
    {
        public bool _extinctionOnReachingTargetIsActive;
        public int _extinctionOnReachingTargetPointsCost;
        public int _extinctionOnReachingTargetMoneyCost;
        public bool _extinctionOnReachingTargetHasBeenBought;
    }

    [Serializable]
    public class UpgradesToSave
    {
        public DroneFireFighterUpgrades _droneFireFighterUpgrades;
        public DroneAmbulanceUpgrades _droneAmbulanceUpgrades;
        public DroneSupplyUpgrades _droneSupplyUpgrades;
        public FireBrigadeUpgrades _fireBrigadeUpgrades;

        public UpgradesToSave( DroneFireFighterUpgrades droneFireFighterUpgrades, DroneAmbulanceUpgrades droneAmbulanceUpgrades, DroneSupplyUpgrades droneSupplyUpgrades, FireBrigadeUpgrades fireBrigadeUpgrades )
        {
            _droneFireFighterUpgrades = droneFireFighterUpgrades;
            _droneAmbulanceUpgrades = droneAmbulanceUpgrades;
            _droneSupplyUpgrades = droneSupplyUpgrades;
            _fireBrigadeUpgrades = fireBrigadeUpgrades;
        }
    }
    #endregion

    [Serializable]
    public class DroneUpgrades
    {
        protected BasicDroneLevels _basicDroneLvls = new BasicDroneLevels();
        protected DronesSkills _droneSkills = new DronesSkills();

        public DroneUpgrades()
        {

        }

        public DroneUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills )
        {
            _basicDroneLvls = basicDroneStats;
            _droneSkills = droneSkills;
        }

        public BasicDroneLevels _GetBasicStats()
        {
            return _basicDroneLvls;
        }

        public DronesSkills _GetDroneSkills()
        {
            return _droneSkills;
        }

        protected void _IncreaseStats( ref int __curStats, int __maxStats )
        {
            if ( _basicDroneLvls._availablePoints > 0 )
            {
                if ( __curStats + 1 <= __maxStats )
                {
                    __curStats++;
                    _basicDroneLvls._availablePoints--;
                }
            }
        }

        protected void _DecreaseStats( ref int __lvl )
        {
            if ( __lvl > 0 )
            {
                __lvl--;
                _basicDroneLvls._availablePoints++;
            }
        }

        public void _IncreaseArmor()
        {
            _IncreaseStats( ref _basicDroneLvls._armorLvl, _basicDroneLvls._maxArmorLvl );
        }

        public void _DecreaseArmor()
        {
            _DecreaseStats( ref _basicDroneLvls._armorLvl );
        }

        public void _IncreaseSpeed()
        {
            _IncreaseStats( ref _basicDroneLvls._speedLvl, _basicDroneLvls._maxSpeedLvl );
        }

        public void _DecreaseSpeed()
        {
            _DecreaseStats( ref _basicDroneLvls._speedLvl );
        }

        protected bool _UpgradeStats( ref int __curLvl, int __maxLvl, ref int __money, int __baseCost )
        {
            if ( __curLvl + 1 <= __maxLvl
                && __money >= __baseCost * ( __curLvl + 1 ) )
            {
                __curLvl++;
                __money -= __baseCost * __curLvl;
                return true;
            }
            return false;
        }

        public void _UpgradeShorteningTime( ref int __money )
        {
            _UpgradeStats(
                ref _basicDroneLvls._shorteningCoolingDownTimeLevel,
                _basicDroneLvls._maxShorteningTimeLevel,
                ref __money,
                _basicDroneLvls._shorteningTimeBaseCost );
        }

        public void _UpgradeEngine( ref int __money )
        {
            if ( _UpgradeStats(
                ref _basicDroneLvls._engineLevel,
                _basicDroneLvls._maxEngineLvl,
                ref __money,
                _basicDroneLvls._engineBaseCost
                ) )
            {
                _basicDroneLvls._ownedPoints += _basicDroneLvls._pointsPerEngineLvl;
                _basicDroneLvls._availablePoints += _basicDroneLvls._pointsPerEngineLvl;
            }
        }

        public void _ToogleExtincionOnTargetSkill( ref int __money)
        {
            if ( _droneSkills._extinctionOnReachingTargetIsActive )
            {
                _droneSkills._extinctionOnReachingTargetIsActive = false;
                _basicDroneLvls._availablePoints += _droneSkills._extinctionOnReachingTargetPointsCost;
            }
            else
            {
                if(_basicDroneLvls._availablePoints - _droneSkills._extinctionOnReachingTargetPointsCost >= 0 )
                {
                    if(!_droneSkills._extinctionOnReachingTargetHasBeenBought && __money - _droneSkills._extinctionOnReachingTargetMoneyCost >= 0 )
                    {
                        __money -= _droneSkills._extinctionOnReachingTargetMoneyCost;
                        _droneSkills._extinctionOnReachingTargetHasBeenBought = true;
                    }

                    _droneSkills._extinctionOnReachingTargetIsActive = true;
                    _basicDroneLvls._availablePoints -= _droneSkills._extinctionOnReachingTargetPointsCost;
                }
            }
        }

        private bool _StatsAreSmallerOrEqualToOwnedPoints( int __usedPoints )
        {
            return __usedPoints <= _basicDroneLvls._ownedPoints;
        }

        protected virtual int _GiveNrOfUsedPoints()
        {
            int __usedPoints = 0;
            __usedPoints += _basicDroneLvls._armorLvl;
            __usedPoints += _basicDroneLvls._speedLvl;
            if ( _droneSkills._extinctionOnReachingTargetIsActive )
                __usedPoints += _droneSkills._extinctionOnReachingTargetPointsCost;
            return __usedPoints;
        }

        public void _MakeSureThatStatsAreCorrect()
        {
            if ( !_StatsAreSmallerOrEqualToOwnedPoints( _GiveNrOfUsedPoints() ) )
            {
                _ResetStatsAndGiveBackPoints();
            }

            if ( _basicDroneLvls._shorteningCoolingDownTimeLevel > _basicDroneLvls._maxShorteningTimeLevel )
            {
                _basicDroneLvls._shorteningCoolingDownTimeLevel = _basicDroneLvls._maxShorteningTimeLevel;
            }
        }

        protected virtual void _ResetStatsAndGiveBackPoints()
        {
            _basicDroneLvls._availablePoints = _basicDroneLvls._ownedPoints;
            _basicDroneLvls._armorLvl = 0;
            _basicDroneLvls._speedLvl = 0;
        }
    }

    [Serializable]
    public class DroneFireFighterUpgrades : DroneUpgrades
    {
        protected FirefighterDroneLevels _fireFighterDroneLevels = new FirefighterDroneLevels();

        public DroneFireFighterUpgrades()
        {
            _basicDroneLvls = new BasicDroneLevels();
            _droneSkills = new DronesSkills();
            _fireFighterDroneLevels = new FirefighterDroneLevels();
        }

        public DroneFireFighterUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, FirefighterDroneLevels fireFighterDroneStats ) : base( basicDroneStats, droneSkills )
        {
            _basicDroneLvls = basicDroneStats;
            _fireFighterDroneLevels = fireFighterDroneStats;
            _droneSkills = droneSkills;
        }

        public FirefighterDroneLevels _FireFighterDroneLevels
        {
            get
            {
                return _fireFighterDroneLevels;
            }
        }

        public void _IncreaseAmmo()
        {
            _IncreaseStats( ref _fireFighterDroneLevels._ammoLvl, _fireFighterDroneLevels._maxAmmo );
        }

        public void _DecreaseAmmo()
        {
            _DecreaseStats( ref _fireFighterDroneLevels._ammoLvl );
        }

        public void _IncreaseExtincionSize()
        {
            _IncreaseStats( ref _fireFighterDroneLevels._sizeOfExtincionLvl, _fireFighterDroneLevels._maxLvLOfSizeOfExtinction );
        }

        public void _DecreaseExtincionSize()
        {
            _DecreaseStats( ref _fireFighterDroneLevels._sizeOfExtincionLvl );
        }

        protected override int _GiveNrOfUsedPoints()
        {
            int __nrOfUsedPoints = base._GiveNrOfUsedPoints();
            __nrOfUsedPoints += _fireFighterDroneLevels._ammoLvl;
            __nrOfUsedPoints += _fireFighterDroneLevels._sizeOfExtincionLvl;
            return __nrOfUsedPoints;
        }

        protected override void _ResetStatsAndGiveBackPoints()
        {
            base._ResetStatsAndGiveBackPoints();
            _fireFighterDroneLevels._ammoLvl = 0;
            _fireFighterDroneLevels._sizeOfExtincionLvl = 0;
        }
    }

    [Serializable]
    public class DroneAmbulanceUpgrades : DroneUpgrades
    {
        protected AmbulanceDroneLevels _ambulanceDroneLevels = new AmbulanceDroneLevels();

        public DroneAmbulanceUpgrades()
        {
            _basicDroneLvls = new BasicDroneLevels();
            _droneSkills = new DronesSkills();
            _ambulanceDroneLevels = new AmbulanceDroneLevels();
        }

        public DroneAmbulanceUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, AmbulanceDroneLevels ambulanceDroneStats ) : base( basicDroneStats, droneSkills )
        {
            _basicDroneLvls = basicDroneStats;
            _droneSkills = droneSkills;
            _ambulanceDroneLevels = ambulanceDroneStats;
        }

        public AmbulanceDroneLevels _AmbulanceDroneLevels
        {
            get
            {
                return _ambulanceDroneLevels;
            }
        }

        public void _IncreaseSlowDownDyingLevel()
        {
            _IncreaseStats( ref _ambulanceDroneLevels._slowDownDyingLevel, _ambulanceDroneLevels._maxSlowDownDecreasing );
        }

        public void _DecreaseSlowDownDyingLevel()
        {
            _DecreaseStats( ref _ambulanceDroneLevels._slowDownDyingLevel );
        }

        protected override int _GiveNrOfUsedPoints()
        {
            int __nrOfUsedPoints = base._GiveNrOfUsedPoints();
            __nrOfUsedPoints = _ambulanceDroneLevels._slowDownDyingLevel;
            return __nrOfUsedPoints;
        }

        protected override void _ResetStatsAndGiveBackPoints()
        {
            base._ResetStatsAndGiveBackPoints();
            _ambulanceDroneLevels._slowDownDyingLevel = 0;
        }
    }

    [Serializable]
    public class DroneSupplyUpgrades : DroneUpgrades
    {
        SupplyDroneLevels _supplyDroneLevels = new SupplyDroneLevels();

        public DroneSupplyUpgrades()
        {
            _basicDroneLvls = new BasicDroneLevels();
            _droneSkills = new DronesSkills();
            _supplyDroneLevels = new SupplyDroneLevels();
        }

        public DroneSupplyUpgrades( BasicDroneLevels basicDroneStats, DronesSkills droneSkills, SupplyDroneLevels supplyDroneStats ) : base( basicDroneStats, droneSkills )
        {
            _basicDroneLvls = basicDroneStats;
            _droneSkills = droneSkills;
            _supplyDroneLevels = supplyDroneStats;
        }

        public SupplyDroneLevels _SupplyDroneLvls
        {
            get
            {
                return _supplyDroneLevels;
            }
        }

        public void _IncreaseSizeOfPackage()
        {
            _IncreaseStats( ref _supplyDroneLevels._sizeOfPackage, _supplyDroneLevels._maxSizeOfPackage );
        }

        public void _DecreaseSizeOfPackage()
        {
            _DecreaseStats( ref _supplyDroneLevels._sizeOfPackage );
        }

        public void _IncreaseArmorPackage()
        {
            _IncreaseStats( ref _supplyDroneLevels._armorOfPackage, _supplyDroneLevels._maxArmorOfPackage );
        }

        public void _DecreaseArmorPackage()
        {
            _DecreaseStats( ref _supplyDroneLevels._armorOfPackage );
        }

        protected override int _GiveNrOfUsedPoints()
        {
            int __nrOfUsedPoints = base._GiveNrOfUsedPoints();
            __nrOfUsedPoints += _supplyDroneLevels._sizeOfPackage;
            __nrOfUsedPoints += _supplyDroneLevels._armorOfPackage;
            return __nrOfUsedPoints;
        }

        protected override void _ResetStatsAndGiveBackPoints()
        {
            base._ResetStatsAndGiveBackPoints();
            _supplyDroneLevels._sizeOfPackage = 0;
            _supplyDroneLevels._armorOfPackage = 0;
        }
    }
}

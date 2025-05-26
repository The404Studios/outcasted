﻿namespace ConsoleEscapeFromTarkov.Utils
{
    /// <summary>
    /// Constants used throughout the game
    /// </summary>
    public static class Constants
    {
        // World size constants
        public const int WorldWidth = 100;
        public const int WorldHeight = 30;

        // Enemy constants
        public const int MaxEnemiesInRaid = 15;
        public const int BaseEnemySpawnCount = 10;

        // Difficulty constants
        public const int BaseDamageMultiplier = 1;
        public const int BaseEnemyHealthMultiplier = 1;

        // Player constants
        public const int BasePlayerHealth = 100;
        public const int BaseInventorySize = 20;
        public const int MaxQuickSlots = 5;

        // Item constants
        public const int MedkitSmallHealAmount = 30;
        public const int MedkitLargeHealAmount = 70;

        // Weapon constants
        public const int WeaponPistolDamage = 15;
        public const int WeaponRifleDamage = 30;
        public const int WeaponShotgunDamage = 40;
        public const int WeaponSniperDamage = 80;

        // Game mechanics constants
        public const int ExtractionTime = 3; // seconds
        public const int ExpPerLevel = 1000;
        public const int HealthPerLevel = 10;
        public const int InventorySpacePerLevel = 1;

        // UI constants
        public const int MessageLogCapacity = 10;
        public const int TargetFPS = 20;

        // Storage and persistence constants
        public const int InitialStorageCapacity = 50;
        public const int InitialPlayerMoney = 1000;
        public const int StorageUpgradeCost = 1000;  // Base cost per 10 slots
        public const int StorageUpgradeAmount = 10;  // Slots per upgrade

        // Market constants
        public const int MarketRefreshTimeInTicks = 500;
        public const float MerchantBuyMarkupBase = 1.0f;
        public const float MerchantSellMarkdownBase = 0.7f;
        public const float MerchantSpecialtyDiscount = 0.1f;
        public const float MerchantSpecialtyBonus = 0.1f;
    }
}
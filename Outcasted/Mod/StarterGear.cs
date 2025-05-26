using System;
using System.Collections.Generic;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages starter gear loadouts for the player
    /// </summary>
    public class StarterGear
    {
        private List<GearLoadout> availableLoadouts;
        private Random random;

        /// <summary>
        /// Constructor for StarterGear
        /// </summary>
        public StarterGear()
        {
            availableLoadouts = new List<GearLoadout>();
            random = new Random();

            // Initialize default loadouts
            InitializeDefaultLoadouts();
        }

        /// <summary>
        /// Initializes the default loadout options
        /// </summary>
        private void InitializeDefaultLoadouts()
        {
            // Basic Scavenger loadout
            var scavengerLoadout = new GearLoadout
            {
                Name = "Scavenger",
                Description = "Light gear focused on looting and stealth.",
                Weapons = new List<Weapon> { new Weapon("Pistol", 15, 8, 15, 3) },
                Armors = new List<Armor> { new Armor("Light Vest", 10) },
                MedKits = new List<MedKit> { new MedKit("Small Medkit", 30) },
                Ammo = new List<Ammo> { new Ammo("9mm Ammo", "Pistol", 24) },
                Valuables = new List<Valuable> { new Valuable("Compass", 100) }
            };
            availableLoadouts.Add(scavengerLoadout);

            // Assault loadout
            var assaultLoadout = new GearLoadout
            {
                Name = "Assault",
                Description = "Combat-focused with better weapons but fewer supplies.",
                Weapons = new List<Weapon> { new Weapon("SMG", 8, 30, 15, 1) },
                Armors = new List<Armor> { new Armor("Medium Armor", 20) },
                MedKits = new List<MedKit> { new MedKit("Small Medkit", 30) },
                Ammo = new List<Ammo> { new Ammo("SMG Ammo", "SMG", 30) },
                Valuables = new List<Valuable>()
            };
            availableLoadouts.Add(assaultLoadout);

            // Medic loadout
            var medicLoadout = new GearLoadout
            {
                Name = "Medic",
                Description = "Focused on healing and survival with extra medical supplies.",
                Weapons = new List<Weapon> { new Weapon("Pistol", 15, 8, 15, 3) },
                Armors = new List<Armor> { new Armor("Light Vest", 10) },
                MedKits = new List<MedKit> {
                    new MedKit("Small Medkit", 30),
                    new MedKit("Small Medkit", 30),
                    new MedKit("Large Medkit", 70)
                },
                Ammo = new List<Ammo> { new Ammo("9mm Ammo", "Pistol", 16) },
                Valuables = new List<Valuable>()
            };
            availableLoadouts.Add(medicLoadout);

            // Heavy loadout
            var heavyLoadout = new GearLoadout
            {
                Name = "Heavy",
                Description = "Heavy armor and shotgun, but slow and limited range.",
                Weapons = new List<Weapon> { new Weapon("Shotgun", 40, 6, 8, 8, 1) },
                Armors = new List<Armor> { new Armor("Heavy Armor", 30) },
                MedKits = new List<MedKit> { new MedKit("Small Medkit", 30) },
                Ammo = new List<Ammo> { new Ammo("Shotgun Shells", "Shotgun", 12) },
                Valuables = new List<Valuable>()
            };
            availableLoadouts.Add(heavyLoadout);

            // Sniper loadout
            var sniperLoadout = new GearLoadout
            {
                Name = "Sniper",
                Description = "Long-range sniper rifle, but limited close-combat ability.",
                Weapons = new List<Weapon> {
                    new Weapon("Sniper Rifle", 80, 5, 40, 10),
                    new Weapon("Pistol", 15, 8, 15, 3)
                },
                Armors = new List<Armor>(),
                MedKits = new List<MedKit> { new MedKit("Small Medkit", 30) },
                Ammo = new List<Ammo> {
                    new Ammo("Sniper Ammo", "Sniper Rifle", 10),
                    new Ammo("9mm Ammo", "Pistol", 8)
                },
                Valuables = new List<Valuable>()
            };
            availableLoadouts.Add(sniperLoadout);
        }

        /// <summary>
        /// Gets all available gear loadouts
        /// </summary>
        /// <returns>List of available loadouts</returns>
        public List<GearLoadout> GetAvailableLoadouts()
        {
            return availableLoadouts;
        }

        /// <summary>
        /// Gets a loadout by name
        /// </summary>
        /// <param name="name">Name of the loadout</param>
        /// <returns>The loadout, or null if not found</returns>
        public GearLoadout GetLoadoutByName(string name)
        {
            return availableLoadouts.Find(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a loadout by index
        /// </summary>
        /// <param name="index">Index of the loadout</param>
        /// <returns>The loadout, or null if index is invalid</returns>
        public GearLoadout GetLoadoutByIndex(int index)
        {
            if (index >= 0 && index < availableLoadouts.Count)
            {
                return availableLoadouts[index];
            }
            return null;
        }

        /// <summary>
        /// Gets a random loadout
        /// </summary>
        /// <returns>A random loadout</returns>
        public GearLoadout GetRandomLoadout()
        {
            if (availableLoadouts.Count == 0)
                return null;

            int index = random.Next(availableLoadouts.Count);
            return availableLoadouts[index];
        }

        /// <summary>
        /// Applies a loadout to player storage and inventory
        /// </summary>
        /// <param name="loadout">Loadout to apply</param>
        /// <param name="player">Player reference</param>
        /// <param name="storage">Storage reference</param>
        public void ApplyLoadoutToPlayer(GearLoadout loadout, Player player, PlayerStorage storage)
        {
            if (loadout == null)
                return;

            // Add all weapons
            foreach (var weapon in loadout.Weapons)
            {
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    player.AddToInventory(weapon);
                }
                else
                {
                    storage.AddItem(weapon);
                }
            }

            // Add all armor
            foreach (var armor in loadout.Armors)
            {
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    player.AddToInventory(armor);
                }
                else
                {
                    storage.AddItem(armor);
                }
            }

            // Add all medkits
            foreach (var medkit in loadout.MedKits)
            {
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    player.AddToInventory(medkit);
                }
                else
                {
                    storage.AddItem(medkit);
                }
            }

            // Add all ammo
            foreach (var ammo in loadout.Ammo)
            {
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    player.AddToInventory(ammo);
                }
                else
                {
                    storage.AddItem(ammo);
                }
            }

            // Add all valuables
            foreach (var valuable in loadout.Valuables)
            {
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    player.AddToInventory(valuable);
                }
                else
                {
                    storage.AddItem(valuable);
                }
            }

            // Auto-equip first weapon and armor
            if (loadout.Weapons.Count > 0 && player.Inventory.Contains(loadout.Weapons[0]))
            {
                player.EquipWeapon(loadout.Weapons[0]);
            }

            if (loadout.Armors.Count > 0 && player.Inventory.Contains(loadout.Armors[0]))
            {
                player.EquipArmor(loadout.Armors[0]);
            }

            // Assign first medkit to quickslot 0 if available
            if (loadout.MedKits.Count > 0)
            {
                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    if (player.Inventory[i] is MedKit)
                    {
                        player.AssignToQuickSlot(i, 0, 0);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the loadout selection menu
        /// </summary>
        public void RenderLoadoutSelection()
        {
            Console.Clear();

            // Draw header
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n   █▀▀ █▀█ █▀▀ █▀█ █▀█   █░░ █▀█ █▀█ █▀▄ █▀█ █░█ ▀█▀");
            Console.WriteLine("   █▄█ █▀▀ ▀▀█ █▀▄ █▄█   █▄▄ █▄█ █▀█ █▄▀ █▄█ █▄█ ░█░");
            Console.ResetColor();

            Console.WriteLine("\n   Select your starting gear loadout:\n");

            // Show all available loadouts
            for (int i = 0; i < availableLoadouts.Count; i++)
            {
                GearLoadout loadout = availableLoadouts[i];

                Console.WriteLine($"   [{i + 1}] {loadout.Name}");
                Console.WriteLine($"       {loadout.Description}");

                // Show gear summary
                Console.Write("       Weapons: ");
                foreach (var weapon in loadout.Weapons)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{weapon.Name} ");
                    Console.ResetColor();
                }
                Console.WriteLine();

                Console.Write("       Armor: ");
                foreach (var armor in loadout.Armors)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"{armor.Name} ");
                    Console.ResetColor();
                }
                Console.WriteLine();

                Console.Write("       Medical: ");
                foreach (var medkit in loadout.MedKits)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{medkit.Name} ");
                    Console.ResetColor();
                }
                Console.WriteLine("\n");
            }

            Console.WriteLine("   [R] Random Loadout");
            Console.WriteLine("\n   Press the corresponding number to select a loadout...");
        }

        /// <summary>
        /// Gets user input for loadout selection
        /// </summary>
        /// <returns>Selected loadout or null if canceled</returns>
        public GearLoadout GetUserLoadoutSelection()
        {
            RenderLoadoutSelection();

            // Wait for valid selection
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    return null; // Cancel selection
                }
                else if (key.Key == ConsoleKey.R)
                {
                    return GetRandomLoadout();
                }
                else if (key.KeyChar >= '1' && key.KeyChar <= '9')
                {
                    int index = key.KeyChar - '1';
                    GearLoadout loadout = GetLoadoutByIndex(index);

                    if (loadout != null)
                    {
                        return loadout;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a gear loadout configuration
    /// </summary>
    public class GearLoadout
    {
        /// <summary>
        /// Name of the loadout
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the loadout
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Weapons included in the loadout
        /// </summary>
        public List<Weapon> Weapons { get; set; }

        /// <summary>
        /// Armor included in the loadout
        /// </summary>
        public List<Armor> Armors { get; set; }

        /// <summary>
        /// Medical items included in the loadout
        /// </summary>
        public List<MedKit> MedKits { get; set; }

        /// <summary>
        /// Ammunition included in the loadout
        /// </summary>
        public List<Ammo> Ammo { get; set; }

        /// <summary>
        /// Valuable items included in the loadout
        /// </summary>
        public List<Valuable> Valuables { get; set; }

        /// <summary>
        /// Constructor for GearLoadout
        /// </summary>
        public GearLoadout()
        {
            Weapons = new List<Weapon>();
            Armors = new List<Armor>();
            MedKits = new List<MedKit>();
            Ammo = new List<Ammo>();
            Valuables = new List<Valuable>();
        }
    }
}
// Portuguese Colonists - startup code.
//
// RimWorld 1.6 reads human names and famous characters ("solid bios") only from its own built-in
// resources, so plain XML/text files in a mod are never picked up. This small class fills that gap
// once, at startup, using the game's own public methods. No Harmony, no patching of game code.
//
//   * Names/*.txt                  -> added to the vanilla random name pool (equal chance, no duplicates)
//   * Defs/FamousPawnDefs/*.xml    -> registered as famous characters, like the vanilla ones
//
// All content lives in those data files; this code should not need to change when adding names/figures.
// Rebuild with Source/build.ps1 (only needed if this file changes).

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace PortugueseColonists
{
    // Entry point: only remembers where the mod lives on disk.
    public class PortugueseColonistsMod : Mod
    {
        public static ModContentPack ContentPack;

        public PortugueseColonistsMod(ModContentPack content) : base(content)
        {
            ContentPack = content;
        }
    }

    public class FamousPawnName
    {
        public string first;
        public string nick;
        public string last;
    }

    // One famous Portuguese figure, defined in Defs/FamousPawnDefs/*.xml
    public class FamousPawnDef : Def
    {
        public FamousPawnName name;
        public GenderPossibility gender = GenderPossibility.Either;
        public BackstoryDef childhood;
        public BackstoryDef adulthood;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (name == null || name.first.NullOrEmpty() || name.last.NullOrEmpty())
            {
                yield return "needs a <name> with at least <first> and <last>";
            }
            if (childhood == null)
            {
                yield return "missing <childhood> backstory";
            }
            if (adulthood == null)
            {
                yield return "missing <adulthood> backstory";
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class PortugueseColonistsLoader
    {
        private const string LogPrefix = "[Portuguese Colonists] ";

        static PortugueseColonistsLoader()
        {
            try
            {
                AddNames();
            }
            catch (Exception e)
            {
                Log.Error(LogPrefix + "Failed to add names: " + e);
            }

            try
            {
                AddFamousPawns();
            }
            catch (Exception e)
            {
                Log.Error(LogPrefix + "Failed to add famous figures: " + e);
            }
        }

        // ---------------------------------------------------------------- names

        private static int namesAdded;
        private static int namesSkipped;

        private static void AddNames()
        {
            NameBank bank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);

            // Same files and slots the game itself uses for its built-in name lists.
            AddNamesFromFile(bank, PawnNameSlot.First, Gender.Male, "First_Male");
            AddNamesFromFile(bank, PawnNameSlot.First, Gender.Female, "First_Female");
            AddNamesFromFile(bank, PawnNameSlot.Nick, Gender.Male, "Nick_Male");
            AddNamesFromFile(bank, PawnNameSlot.Nick, Gender.Female, "Nick_Female");
            AddNamesFromFile(bank, PawnNameSlot.Nick, Gender.None, "Nick_Unisex");
            AddNamesFromFile(bank, PawnNameSlot.Last, Gender.None, "Last");

            Log.Message(LogPrefix + "Added " + namesAdded + " names to the random name pool ("
                + namesSkipped + " skipped because the game already had them).");
        }

        private static void AddNamesFromFile(NameBank bank, PawnNameSlot slot, Gender gender, string fileName)
        {
            string path = Path.Combine(Path.Combine(PortugueseColonistsMod.ContentPack.RootDir, "Names"), fileName + ".txt");
            if (!File.Exists(path))
            {
                Log.Warning(LogPrefix + "Name file not found: " + path);
                return;
            }

            // Never put a name in the pool twice: the game expects every name to be unique.
            HashSet<string> seen = new HashSet<string>(ExistingNames(bank, slot, gender));
            List<string> toAdd = new List<string>();
            foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
            {
                string name = line.Trim();
                if (name.Length == 0 || name.StartsWith("//"))
                {
                    continue;
                }
                if (seen.Add(name))
                {
                    toAdd.Add(name);
                }
                else
                {
                    namesSkipped++;
                }
            }

            bank.AddNames(slot, gender, toAdd);
            namesAdded += toAdd.Count;
        }

        private static IEnumerable<string> ExistingNames(NameBank bank, PawnNameSlot slot, Gender gender)
        {
            // NameBank.NamesFor is not public, so it is read via reflection (read-only).
            MethodInfo namesFor = typeof(NameBank).GetMethod("NamesFor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (namesFor == null)
            {
                Log.Warning(LogPrefix + "Could not read the existing name pool; duplicate check skipped.");
                return new List<string>();
            }
            return (List<string>)namesFor.Invoke(bank, new object[] { slot, gender });
        }

        // ---------------------------------------------------------------- famous figures

        private static void AddFamousPawns()
        {
            HashSet<string> existing = new HashSet<string>();
            foreach (PawnBio bio in SolidBioDatabase.allBios)
            {
                existing.Add(bio.name.ToStringFull);
            }

            int added = 0;
            foreach (FamousPawnDef def in DefDatabase<FamousPawnDef>.AllDefsListForReading)
            {
                if (def.name == null || def.childhood == null || def.adulthood == null)
                {
                    continue; // Already reported by ConfigErrors.
                }

                NameTriple name = new NameTriple(def.name.first, def.name.nick, def.name.last);
                name.ResolveMissingPieces();
                if (!existing.Add(name.ToStringFull))
                {
                    Log.Warning(LogPrefix + "Skipped " + def.defName + ": a famous character called " + name.ToStringFull + " already exists.");
                    continue;
                }

                // Mirrors how the game registers its own famous characters.
                PawnBio newBio = new PawnBio();
                newBio.gender = def.gender;
                newBio.name = name;
                newBio.childhood = def.childhood;
                newBio.adulthood = def.adulthood;
                PawnNameDatabaseSolid.AddPlayerContentName(name, def.gender);
                newBio.ResolveReferences();
                SolidBioDatabase.allBios.Add(newBio);
                added++;
            }

            Log.Message(LogPrefix + "Added " + added + " famous Portuguese figures.");
        }
    }
}

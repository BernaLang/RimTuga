# Portuguese Colonists

A RimWorld 1.5 / 1.6 mod that brings Portugal to the Rim:

- **Portuguese names**: first names, last names and nicknames are added to the game's random name pool, with the same chance as the vanilla names.
- **Famous Portuguese figures**: kings, navigators, poets, fadistas, footballers, TV hosts and more, as unique characters with hand-written backstories. Like the game's own famous characters, they occasionally appear as colonists, wanderers, traders or raiders.
- **Portuguese translation** of all backstories (European Portuguese, also used for Brazilian Portuguese).

Backstories use vanilla-level skill bonuses, so famous characters are not overpowered.

## Project layout

```
About/                About.xml (name, id, version, description) and Preview.png
Assemblies/           PortugueseColonists.dll, built from Source/ (generated, not in git)
Defs/
  BackstoryDefs/      PortugueseBackstories.xml  - childhood + adulthood backstory for each figure
  FamousPawnDefs/     PortugueseFigures.xml      - the famous figures (name, gender, backstories)
Languages/
  Portuguese (Português)/DefInjected/BackstoryDef/  - Portuguese translation of the backstories
Names/                Name lists, one name per line
Source/               C# startup code + build and package scripts (not shipped)
releases/             Output of the package script (generated, not in git)
README.md             Mod front page for GitHub: short description for players
PROJECT-README.md     This file: project guide for contributors
NamesInfo.md          Human-readable list of every name and persona in the mod
AGENTS.md             Rules and context for AI agents working on the project
```

### Why there is a DLL

RimWorld 1.6 only reads human names and famous characters from its own built-in files, so plain XML or text files in a mod are ignored. `Source/PortugueseColonists.cs` runs once at startup and adds our names and figures using the game's own public methods. It uses no Harmony and doesn't patch any game code.

You only need to rebuild it if you change the `.cs` file. Adding names, figures or translations never requires a rebuild.

> [NamesInfo.md](NamesInfo.md) lists every name and persona in human-readable form. Update it too when you add names or figures.

## Adding names

Edit the files in `Names/`. Use one name per line, save the files as UTF-8 (accents are fine), and remember that the nicknames depend on gender:

| File | What goes in it |
|---|---|
| `First_Male.txt` / `First_Female.txt` | First names |
| `Last.txt` | Last names (shared by everyone) |
| `Nick_Male.txt` / `Nick_Female.txt` | Gendered nicknames, e.g. *Padeiro* / *Padeira* |
| `Nick_Unisex.txt` | Nicknames that work for anyone, e.g. *Fadista*, *Tremoço* |

- Empty lines and lines starting with `//` are ignored.
- You don't need to worry about duplicates: names the game already has are skipped automatically, and the log says how many.
- **Keep nicknames short.** The nickname is what shows on the colonist bar.

## Adding a famous figure

Each figure needs three things: two backstories, the figure itself, and the translation. Use an existing figure as a template. Here is the full checklist, using *Rui Veloso* as an example.

### 1. Backstories: `Defs/BackstoryDefs/PortugueseBackstories.xml`

Add one `Childhood` and one `Adulthood` backstory, named `PT_<Person>_Child` and `PT_<Person>_Adult`:

```xml
<BackstoryDef>
  <defName>PT_RuiVeloso_Adult</defName>
  <slot>Adulthood</slot>
  <title>father of Portuguese rock</title>
  <titleShort>rocker</titleShort>
  <description>[PAWN_nameDef] picked up a guitar and ...</description>
  <skillGains>
    <Artistic>5</Artistic>
    <Social>2</Social>
  </skillGains>
  <workDisables>None</workDisables>
  <spawnCategories>
    <li>Offworld</li>
    <li>Outlander</li>
    <li>Pirate</li>
  </spawnCategories>
  <shuffleable>false</shuffleable>
</BackstoryDef>
```

- **`shuffleable` must be `false`**, otherwise random pawns could get this backstory.
- **Keep `spawnCategories`**, otherwise the figure never spawns. `Offworld` covers starting colonists, `Outlander` covers outlanders, traders and wanderers, and `Pirate` covers raiders. If the backstory has `<workDisables>Violent</workDisables>`, leave out `Pirate`; the game reports a config error for raiders who can't fight.
- **Text tokens:** `[PAWN_nameDef]` is the name, `[PAWN_pronoun]` is he/she, `[PAWN_possessive]` is his/her, and `[PAWN_objective]` is him/her. Use `\n\n` for a paragraph break.
- **Skills**: Animals, Artistic, Construction, Cooking, Crafting, Intellectual, Medicine, Melee, Mining, Plants, Shooting, Social.
- **Balance**, kept close to vanilla:
  - Childhood: 1–3 skills, biggest bonus +3, total about +3 to +4.
  - Adulthood: 2–3 skills, biggest bonus +5 (+6 at most), total about +6 to +8.
  - A good forced trait or no drawbacks means a lower total. A disabled work type means slightly higher.
- **Optional extras**, see existing entries for examples:
  - `<workDisables>Violent</workDisables>`, with several separated by commas.
  - `<forcedTraits><Kind>0</Kind></forcedTraits>`
  - `<bodyTypeGlobal>Thin</bodyTypeGlobal>`, which can be Male, Female, Thin, Fat or Hulk.

### 2. The figure: `Defs/FamousPawnDefs/PortugueseFigures.xml`

```xml
<PortugueseColonists.FamousPawnDef>
  <defName>PT_RuiVeloso</defName>
  <name>
    <first>Rui</first>
    <nick>Veloso</nick>
    <last>Veloso</last>
  </name>
  <gender>Male</gender>
  <childhood>PT_RuiVeloso_Child</childhood>
  <adulthood>PT_RuiVeloso_Adult</adulthood>
</PortugueseColonists.FamousPawnDef>
```

`gender` is `Male` or `Female`. The nickname shows on the colonist bar, so make it short and recognisable.

### 3. Translation: `Languages/Portuguese (Português)/DefInjected/BackstoryDef/PortugueseBackstories.xml`

Add three lines for each backstory:

```xml
<PT_RuiVeloso_Adult.title>pai do rock português</PT_RuiVeloso_Adult.title>
<PT_RuiVeloso_Adult.titleShort>roqueiro</PT_RuiVeloso_Adult.titleShort>
<PT_RuiVeloso_Adult.description>[PAWN_nameDef] pegou numa guitarra e ...</PT_RuiVeloso_Adult.description>
```

Each figure has a fixed gender, so write titles in the right gender (*fadista*, *campeã*) and use *ele/ela* directly instead of pronoun tokens. That reads much better in Portuguese.

The package script copies this translation to Brazilian Portuguese automatically. **Don't** create a Brazilian folder in the project.

## Building and releasing

Requires RimWorld installed (default `C:\Games\RimWorld`; pass `-GamePath` otherwise). No other tools are needed, because the scripts use the C# compiler that comes with Windows.

```powershell
# Build + check XML + assemble + zip into releases/
powershell -ExecutionPolicy Bypass -File Source\package.ps1

# Same, but skip rebuilding the DLL (when Source/ didn't change)
powershell -ExecutionPolicy Bypass -File Source\package.ps1 -SkipBuild
```

This produces:
- `releases/PortugueseColonists/`, to copy into `RimWorld\Mods\`
- `releases/PortugueseColonists-<version>.zip`, with the version taken from `<modVersion>` in `About/About.xml`

Raise `<modVersion>` in `About/About.xml` before each release.

## Installing and testing

1. Delete any old `RimWorld\Mods\PortugueseColonists` folder.
2. Copy `releases/PortugueseColonists/` into `RimWorld\Mods\`, so the path is `Mods\PortugueseColonists\About\About.xml`.
3. Enable **Portuguese Colonists** in the mod list, below Core and the DLCs, then restart.
4. With dev mode on, the log should show:
   - `[Portuguese Colonists] Added N names to the random name pool (...)`
   - `[Portuguese Colonists] Added N famous Portuguese figures.`

   Red errors usually name the file and the def that is wrong.

## Save compatibility

- **Adding the mod to an existing save is safe.** Pawns created from then on can get Portuguese names and famous identities. Pawns that already exist keep their names.
- **Removing the mod from a save is not recommended** once any famous figure has appeared. Their backstories come from this mod, so the game will log errors about the missing backstories and those pawns may break. Pawns with ordinary Portuguese names are fine, since names are saved as plain text.

# AGENTS.md

Guide for AI agents working on **Portuguese Colonists**, a RimWorld 1.5/1.6 mod that adds Portuguese names and famous Portuguese figures (personas) with custom backstories. Read [PROJECT-README.md](PROJECT-README.md) for the project overview and the step-by-step guides for adding names and figures; this file covers the rules and the context behind them. [README.md](README.md) is the player-facing front page of the GitHub repo.

## Ground rules

- **No git actions.** Don't commit, push, branch, tag or change git config. When a piece of work is done, **suggest a commit message** and let the user commit.
- **Always keep the docs in sync** in the same change, never as a follow-up:
  - [NamesInfo.md](NamesInfo.md): every name and persona. Update it whenever `Names/*.txt` or `Defs/FamousPawnDefs/` change, and keep the counts in its headings correct.
  - [PROJECT-README.md](PROJECT-README.md): project layout, how-to guides, build/release and save notes.
  - [README.md](README.md): the mod's front page for players. Keep its feature list and counts current (the exact number of famous figures, and the rounded name and nickname counts).
  - `AGENTS.md`: this file, when rules or project facts change.
- **Keep the mod simple.** The user wants a plain data mod. Don't add Harmony, mod settings, extra features or extra dependencies without asking first. Rejected so far:
  - weighting the chance of Portuguese names;
  - listing names more than once to raise their odds;
  - linking the project folder into the game.

  Names keep the **same chance as vanilla names**.
- **Mod identity is fixed:** name "Portuguese Colonists", author "Blank", packageId `Blank.PortugueseColonists`. Never change the packageId; it would break the mod for existing users.

## Why the mod has a DLL (don't "simplify" this away)

This was verified by reading the RimWorld 1.6 game code, not assumed:

- **Human names** (`First_Male`, `Last`, and so on) are loaded only from the game's built-in Unity resources. Files a mod puts in `Languages/*/Strings/Names/` are **ignored** for human names.
- **There is no `PawnBioDef` in 1.6.** Famous characters (`PawnBio`) are loaded only from built-in resources. Old tutorials and mods that use `<PawnBioDef>` don't apply.
- `Source/PortugueseColonists.cs` therefore runs once at startup (`[StaticConstructorOnStartup]`). It:
  - adds `Names/*.txt` to the vanilla `NameBank`, skipping any name the pool already has;
  - registers each `FamousPawnDef` as a `PawnBio`.

  It only uses the game's own public methods, plus one read-only reflection call to `NameBank.NamesFor` for the duplicate check. No Harmony, no patching.
- **Startup order:** the game loads its built-in famous characters before `StaticConstructorOnStartup` runs, so ours are added afterwards and aren't wiped.

## Project layout

| Path | Contents | Shipped |
|---|---|---|
| `About/` | `About.xml`, `Preview.png` | yes |
| `Assemblies/` | `PortugueseColonists.dll`, built by `Source/build.ps1` (git-ignored) | yes |
| `Defs/BackstoryDefs/` | Two backstories per figure: `PT_<Person>_Child` and `PT_<Person>_Adult` | yes |
| `Defs/FamousPawnDefs/` | Figures as `<PortugueseColonists.FamousPawnDef>` | yes |
| `Languages/Portuguese (Português)/` | European Portuguese translation (DefInjected) | yes, also copied to Brazilian |
| `Names/` | Name lists, one per line | yes |
| `Source/` | C# code, `build.ps1`, `package.ps1` | no |
| `releases/` | Package output (git-ignored) | no (generated) |
| `README.md` | Front page for players (GitHub) | no |
| `PROJECT-README.md`, `NamesInfo.md`, `AGENTS.md` | Docs for contributors and agents | no |
| `.gitignore` | Ignores generated output (`releases/`, the DLL) and IDE/OS files | no |

`About/PublishedFileId.txt`, which is created on the first Steam Workshop upload, must stay tracked in git.

## Content rules

### Names
- One name per line, UTF-8. Lines starting with `//` are comments.
- Nicknames depend on gender: put masculine forms in `Nick_Male`, feminine forms in `Nick_Female`, and words that work for anyone in `Nick_Unisex`.
- Keep nicknames short; the nickname is what shows on the colonist bar.
- **Rude and offensive slang is welcome.** The user explicitly asked not to filter out insults or crude regional slang (Porto/Northern *tripeiro* slang is a favourite). Do still check what a word means before adding it, and put it in the right gender list.

### Backstories (`Defs/BackstoryDefs/PortugueseBackstories.xml`)
- Always `<shuffleable>false</shuffleable>`, so they never appear on random pawns.
- **`spawnCategories` control which factions can roll a figure.** Without them the figure never spawns. Verified in the 1.6 code: a figure matches a faction if **either** backstory has the category, so give **both** backstories of a figure the same list.
  - **Every figure** gets `Offworld` (starting colonists) and `Outlander` (outlander towns, traders, wanderers). The user explicitly wants all figures available there.
  - **`Pirate`** (always-hostile pirate factions) is **only** for figures with a fighting, military, rebel, criminal or brash story. Current list: Afonso Henriques, Camões, Vasco da Gama, Brites de Almeida, D. Sebastião, Fernão de Magalhães, Maria da Fonte, Bocage, Marquês de Pombal, Diogo Alves, Salgueiro Maia, Ljubomir, Jorge Jesus, José Mourinho, Joana Marques, D. Pedro IV and Gil Eanes. Peaceful figures (writers, scientists, saints, TV people...) never get it. Never combine `Pirate` with `<workDisables>Violent</workDisables>`; the game reports a config error.
  - **`Cult`** (Anomaly's hostile cultists) is optional, for occult or sinister figures: currently Diogo Alves, Maya, O Velho do Restelo and the Padre Voador (suspected of sorcery by the Inquisition). **It's safe without Anomaly, and needs no `MayRequire`.** Verified in the 1.6 code: categories are plain strings, the config-error check never validates their names, and the base game's own Core backstories use `Cult` 115 times. Without Anomaly the category is simply never requested.
  - When adding a figure, decide its group and say which one in your summary to the user.
- **Balance stays at vanilla level.** The user explicitly doesn't want overpowered pawns.
  - Childhood: 1–3 skills, biggest bonus +3, total about +3 to +4.
  - Adulthood: 2–3 skills, biggest bonus +5 (+6 at most), total about +6 to +8.
  - A good forced trait means a lower total; a disabled work type means slightly higher.
- **Format, verified against vanilla 1.6 XML:**
  - `<skillGains><Melee>3</Melee></skillGains>`
  - `<forcedTraits><Brawler>0</Brawler></forcedTraits>`
  - `<workDisables>A, B</workDisables>`
  - `<bodyTypeGlobal>Thin</bodyTypeGlobal>`
- Write descriptions in English with the tokens `[PAWN_nameDef]`, `[PAWN_pronoun]`, `[PAWN_possessive]` and `[PAWN_objective]`.

### Figures (`Defs/FamousPawnDefs/PortugueseFigures.xml`)
- Use `name` (`first`/`nick`/`last`), `gender` (`Male`/`Female`), `childhood` and `adulthood`.
- Keep the nickname short and recognisable.
- Real people: keep it respectful, accurate and lightly humorous. **Check facts about recent or living people on the web** before writing. Avoid people who died very recently or in tragic circumstances (Diogo Jota was deliberately left out).

### Translation
- Write in **European Portuguese** (post-AO90 spelling), in the `Portuguese (Português)` folder only.
- **Don't create a Brazilian folder** in the project; `package.ps1` copies the translation to `PortugueseBrazilian (Português Brasileiro)` automatically.
- Each backstory needs `.title`, `.titleShort` and `.description`.
- Each figure has a fixed gender, so write titles in the right gender and use *ele/ela* directly instead of pronoun tokens.
- When you add or change English backstory text, update the translation in the same change.

## Building and checking

- The scripts need a local RimWorld 1.6 install. They default to `C:\Games\RimWorld`; pass `-GamePath` if it's elsewhere, and ask the user where the game is installed if you're unsure. Inside the install, the game code is in `RimWorldWin64_Data\Managed\Assembly-CSharp.dll` and vanilla defs are in `Data\*\Defs`, if you need to check how something works.
- No .NET SDK is needed. `Source/build.ps1` compiles with Windows' built-in `csc.exe` (C# 5 only: no `$""`, `?.`, `nameof`, `=>` members or `out var`) against the game's own runtime DLLs. Only rebuild when `Source/*.cs` changes.
- `Source/package.ps1 [-SkipBuild]` does the whole release:
  1. builds the DLL;
  2. checks that every XML file parses;
  3. copies the shipped folders into `releases/PortugueseColonists/`, plus the Brazilian copy of the translation;
  4. writes `releases/PortugueseColonists-<modVersion>.zip`.
- **PowerShell 5.1 misreads non-ASCII characters in `.ps1` files without a BOM.** Build accented paths such as "Português" from character codes (`[char]0x00EA`), as `package.ps1` does.
- After content changes, also check that:
  - every backstory referenced by a figure exists;
  - every backstory has all three translation keys and there are no stray keys;
  - no name list contains duplicates.
- **You can't run the game.** The user tests in-game. On Windows, RimWorld's log is at `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log`. A healthy load shows `[Portuguese Colonists] Added N names ...` and `Added N famous Portuguese figures.` with no red errors that mention `PT_`, `FamousPawnDef` or `PortugueseColonists`. Players usually run other mods too, so log messages from other mods aren't ours.

## Releases and saves

- The user installs by copying `releases/PortugueseColonists/` into `RimWorld\Mods\` after deleting the old folder.
- Raise `<modVersion>` in `About/About.xml` for every new release.
- **Adding the mod to an existing save is safe.** Removing it after a famous figure has spawned is not, because that figure's backstories disappear.
- Don't rename or remove existing backstory or figure `defName`s once released; saves reference them. Add new ones instead.

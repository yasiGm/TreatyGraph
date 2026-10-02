# TreatyGraph (SQL Server + C# / .NET)

Pick two countries and see how their relationship unfolded from 1816 onward: **alliances, wars, diplomatic ties and who was in charge**. Each country also has a profile page (leaders, inter-state wars, civil wars, alliance partners).

```
 CSV files                 C# ETL (Importer)              SQL Server                 ASP.NET Core API           Browser
 COW alliances ─┐          parse → clean dates →          dbo.Country                /api/meta                  plain HTML/JS
 ATOP 5.1      ─┤          merge phases → pair windows    dbo.DyadEvent  ──────────▶ /api/countries/{c}  ────▶  (wwwroot)
 COW wars      ─┼────────▶ one IEtlRule class per   ───▶ dbo.CountryWar   Dapper +  /api/pairs/{a}/{b}
 COW diplomacy ─┤          source, one Table class        dbo.Leader       stored
 Archigos      ─┘          per SQL table (see below)      usp_* procs        
```

| Project | What it is |
|---|---|
| `src/TreatyGraph.Core` | Class library: CSV reader, partial-date model, and the ETL rules. Every rule is its own class implementing `IEtlRule<T>`, grouped by topic under `Etl/` (`Alliances/`, `Wars/`, `Diplomacy/`, `Leaders/`, `Countries/`). |
| `src/TreatyGraph.Importer` | Console app: runs the rules, creates the schema, bulk-loads SQL Server. `Etl/RuleCatalog.cs` lists the rules; `Tables/` has one class per SQL table (its row shape + projection) and `Tables/TableCatalog.cs` lists them in FK order; `Database/` has the SQL plumbing. |
| `src/TreatyGraph.Api` | ASP.NET Core minimal API + Dapper; also serves the UI from `wwwroot`. One DTO / row class per file under `Data/Dtos/` and `Data/Rows/`. |
| `tests/TreatyGraph.Tests` | xUnit tests for the parsing / ETL rules |
| `sql/Tables/` | One `CREATE TABLE` script per table, named after the table (`Country.sql`, `DyadEvent.sql`, ...) |
| `sql/Procedures/` | One script per stored procedure, named after the procedure (`usp_GetPairTimeline.sql`, `usp_GetCountryWars.sql`, ...). The API runs no inline SQL - every query is one of these. |
| `sql/Queries/` | `ShowcaseQueries.sql` - analytics examples (not run by the importer) |

## Extending it

File names carry no numbers: the order in which tables are created, loaded and dropped comes from `TableCatalog` (a table comes after the tables it references), and the drops are generated from it in reverse, so there is no teardown script to keep in sync.

**Add a table**

1. `sql/Tables/TableName.sql` with the `CREATE TABLE` (and its indexes).
2. `src/TreatyGraph.Importer/Tables/TableNameTable.cs` - a class deriving from `Table<TableNameTable.Row>`, with a nested `Row` record whose property names match the column names one-to-one, and a `Rows(ImportData)` projection. `DataTableBuilder` builds the `DataTable` from the record by reflection.
3. One line in `TableCatalog.All`, after the tables it references.

The importer fails loudly if a script in `sql/Tables/` has no table class registered.

**Add a data source / rule**

1. A class in `src/TreatyGraph.Core/Etl/<Topic>/` implementing `IEtlRule<T>` (for example `IEtlRule<DyadEvent>` for a new pair-level layer); put its file path in `SourceFiles`.
2. One line in `RuleCatalog`. If it produces a new kind of row, also add it to `ImportData` / `ImportDataReader` and give it a table (above).

**Add a stored procedure**: one file in `sql/Procedures/` using `CREATE OR ALTER PROCEDURE`; every file there is run on import.

## Run it

Prerequisites: .NET 8 SDK (or newer), Docker Desktop (or any SQL Server 2016+ / Azure SQL database).

```bash
docker compose up -d                                   # 1. SQL Server on localhost:1433
dotnet test                                            # 2. run the unit tests
dotnet run --project src/TreatyGraph.Importer     # 3. create schema + load data (from the repo root)
dotnet run --project src/TreatyGraph.Api          # 4. open http://localhost:5080
```

The default connection string (dev container, `sa` user) is in `src/TreatyGraph.Api/appsettings.json` and in `ImportOptions.cs`.
> **Note:** the password in `docker-compose.yml`, `appsettings.json` and `ImportOptions.cs` (`Your_strong_Passw0rd!`) is a throw-away dev password for a local container that only listens on `localhost`. It is not a real credential - never reuse it for a real server.

To use another server (for example Azure SQL) pass `--connection "<ado.net string>"` to the importer (add `--no-create-db` if the database already exists) and set `ConnectionStrings__Default` for the API.

The raw CSVs must be in `data/raw/` (git-ignored, see "Data files" below).

### راهنمای سریع (فارسی)
۱) Docker Desktop را باز کن و `docker compose up -d` بزن. ۲) `dotnet test` را اجرا کن. ۳) `dotnet run --project src/TreatyGraph.Importer` دیتابیس را می‌سازد و پر می‌کند (ترتیب ساخت جدول‌ها از `TableCatalog` می‌آید؛ هر جدول یک فایل در `sql/Tables/` و هر پروسیجر یک فایل در `sql/Procedures/` دارد). ۴) `dotnet run --project src/TreatyGraph.Api` را اجرا کن و `http://localhost:5080` را باز کن. کوئری‌های تحلیلی SQL در `sql/Queries/ShowcaseQueries.sql` هستند.

## What is in the timeline

| Layer | Source | Coverage | Notes |
|---|---|---|---|
| Alliances | COW Formal Alliances v4.1 | 1816–2012 | Dyad-level start/end dates |
| Alliances | ATOP 5.1 | 1815–2018 | Member-level entry/exit turned into pair windows; phases of one treaty are merged |
| Wars | COW Inter-State War Data v4.0 | 1823–2007 | "Against each other" and "same side" are separate event types |
| Diplomacy | COW Diplomatic Exchange v2006.1 | 1817–2005 | **5-year snapshots only**; only changes in the coded level are kept |
| Leaders | Archigos 4.1 | to 2015 | **Pilot countries only**: Iran, Russia, UK, Germany, France, USA, Japan, China, Turkey |
| Countries | COW State System Membership v2024 | 1816–2024 | Defines who exists when |

The same record reported by two sources (a treaty in both COW and ATOP) is shown once with both badges (the UI merges identical rows).

## Read this before trusting a blank timeline

* "No records" means *these datasets* have nothing. Example: the 1826–28 Russo-Persian war and the 1941 Anglo-Soviet invasion of Iran are not in COW's inter-state war list, so they do not appear on the Iran–Russia timeline.
* Every dataset ends years ago; nothing here covers the last decade.
* "Ongoing" means *still open when that dataset ended* (COW alliances 2012, ATOP 2018), not necessarily in force today.
* Diplomatic data are 5-year snapshots; the 1950–1965 snapshots have no level detail (all coded "other").
* Country identity follows COW state-system membership (predecessor/successor states are separate codes).
* Some ATOP member codes are not COW states; the importer skips those rows and prints how many.

## Data files (put them under `data/raw/`)

```
data/raw/cow_alliances/alliance_v4.1_by_dyad.csv
data/raw/atop/atop5_1a.csv            (ATOP 5.1 alliance level)
data/raw/atop/atop5_1m.csv            (ATOP 5.1 member level)
data/raw/cow_wars/Inter-StateWarData_v4.0.csv
data/raw/cow_wars/Intra-StateWarData_v4.1.csv
data/raw/cow_diplomacy/Diplomatic_Exchange_2006v1.csv
data/raw/cow_states/statelist2024.csv
```

`data/leaders_pilot.csv` is **not** in the repository (git-ignored, like `data/raw/`). Build it yourself from Archigos 4.1 (https://rochester.edu/college/faculty/hgoemans/data.htm): keep the nine pilot countries and the columns `ccode,leader,start,end,exit`, with dates as `YYYY-MM-DD`.

## Ideas / next steps

* Full Archigos coverage; Wikidata for rulers before 1850 and for events after each dataset's end date.
* Trade (COW Trade) and MIDs as extra layers; a `SourceCoverage` table instead of the constant in `Sources.cs`.
* Explicit predecessor/successor links between states (Persia → Iran, USSR → Russia).
* Deploy: Azure App Service + Azure SQL.

## Data sources and citations

Please cite the datasets if you reuse the derived data, and check each dataset's page for its current terms (COW data are for non-commercial use).

* Gibler, Douglas M. 2009. *International Military Alliances, 1648–2008*. CQ Press. (COW Formal Alliances v4.1)
* Leeds, Brett Ashley, Jeffrey Ritter, Sara Mitchell, and Andrew Long. 2002. "Alliance Treaty Obligations and Provisions, 1815–1944." *International Interactions* 28: 237–260. (ATOP 5.1)
* Sarkees, Meredith Reid, and Frank Wayman. 2010. *Resort to War: 1816–2007*. CQ Press. (COW War Data)
* Bayer, Reşat. 2006. "Diplomatic Exchange Data set, v2006.1." https://correlatesofwar.org
* Correlates of War Project. State System Membership List, v2024. https://correlatesofwar.org
* Goemans, H. E., Kristian Skrede Gleditsch, and Giacomo Chiozza. 2009. "Introducing Archigos: A Dataset of Political Leaders." *Journal of Peace Research* 46(2): 269–283. (Archigos 4.1)

Code: Copyright (c) 2026 Yasaman Gandomi, licensed under the GNU AGPL-3.0 (see `LICENSE`). If you run a modified version as a network service, the AGPL requires you to offer its source to the service's users. The licence does not extend to the datasets, which stay under their owners' terms.

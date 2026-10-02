/* ---------------------------------------------------------------------------
   dbo.usp_GetCountryWars - one country's inter-state war participations.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountryWars
    @Ccode smallint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT WarName,
           CAST(StartYear AS int) AS StartYear, CAST(StartMonth AS int) AS StartMonth, CAST(StartDay AS int) AS StartDay,
           CAST(EndYear AS int)   AS EndYear,   CAST(EndMonth AS int)   AS EndMonth,   CAST(EndDay AS int)   AS EndDay,
           SortKey, Result, IsInitiator, Opponents, Allies, BattleDeaths
    FROM dbo.CountryWar
    WHERE Ccode = @Ccode
    ORDER BY SortKey;
END;
GO

/* ---------------------------------------------------------------------------
   dbo.usp_GetCountryCivilWars - one country's civil / internationalised wars.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountryCivilWars
    @Ccode smallint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT WarName,
           CAST(StartYear AS int) AS StartYear, CAST(StartMonth AS int) AS StartMonth, CAST(StartDay AS int) AS StartDay,
           CAST(EndYear AS int)   AS EndYear,   CAST(EndMonth AS int)   AS EndMonth,   CAST(EndDay AS int)   AS EndDay,
           SortKey, Sides, IsInternationalized
    FROM dbo.CivilWar
    WHERE Ccode = @Ccode
    ORDER BY SortKey;
END;
GO

/* ---------------------------------------------------------------------------
   dbo.usp_GetCountryAlliances - one country's alliances, with the partner country on each row.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountryAlliances
    @Ccode smallint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(p.Ccode AS int) AS Partner, p.Name AS PartnerName, e.Title, e.Source,
           CAST(e.StartYear AS int) AS StartYear, CAST(e.StartMonth AS int) AS StartMonth, CAST(e.StartDay AS int) AS StartDay,
           CAST(e.EndYear AS int)   AS EndYear,   CAST(e.EndMonth AS int)   AS EndMonth,   CAST(e.EndDay AS int)   AS EndDay,
           e.IsOngoing
    FROM dbo.DyadEvent AS e
    JOIN dbo.Country   AS p ON p.Ccode = CASE WHEN e.CcodeLo = @Ccode THEN e.CcodeHi ELSE e.CcodeLo END
    WHERE e.EventType = 'alliance'
      AND (e.CcodeLo = @Ccode OR e.CcodeHi = @Ccode)
    ORDER BY p.Name, e.SortKey;
END;
GO

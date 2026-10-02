/* ---------------------------------------------------------------------------
   dbo.usp_GetCountrySpans - state-system membership spans; all countries when @Ccode is NULL.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountrySpans
    @Ccode smallint = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(Ccode AS int)     AS Ccode,
           CAST(StartYear AS int) AS StartYear,
           CAST(EndYear AS int)   AS EndYear
    FROM dbo.CountrySpan
    WHERE @Ccode IS NULL OR Ccode = @Ccode
    ORDER BY Ccode, StartYear;
END;
GO

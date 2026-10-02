/* ---------------------------------------------------------------------------
   dbo.usp_GetCountries - every country, by name (country picker, /api/meta).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountries
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(Ccode AS int) AS Ccode, Abbr, Name
    FROM dbo.Country
    ORDER BY Name;
END;
GO

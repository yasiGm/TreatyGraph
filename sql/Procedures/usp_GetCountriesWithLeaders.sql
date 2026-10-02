/* ---------------------------------------------------------------------------
   dbo.usp_GetCountriesWithLeaders - codes of the countries that have leader data (/api/meta).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountriesWithLeaders
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT CAST(Ccode AS int) AS Ccode
    FROM dbo.Leader;
END;
GO

/* ---------------------------------------------------------------------------
   dbo.usp_GetCountry - one country by code (country profile).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountry
    @Ccode smallint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(Ccode AS int) AS Ccode, Abbr, Name
    FROM dbo.Country
    WHERE Ccode = @Ccode;
END;
GO

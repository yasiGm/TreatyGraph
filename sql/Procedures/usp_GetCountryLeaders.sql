/* ---------------------------------------------------------------------------
   dbo.usp_GetCountryLeaders - one country's leaders in chronological order.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCountryLeaders
    @Ccode smallint
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Name, StartDate, EndDate, ExitType
    FROM dbo.Leader
    WHERE Ccode = @Ccode
    ORDER BY StartDate;
END;
GO

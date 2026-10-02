/* ---------------------------------------------------------------------------
   dbo.usp_GetPairTimeline - all events between two countries in chronological
   order (argument order does not matter). Used by the API's /api/pairs endpoint.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPairTimeline
    @CcodeA smallint,
    @CcodeB smallint
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Lo smallint = CASE WHEN @CcodeA < @CcodeB THEN @CcodeA ELSE @CcodeB END;
    DECLARE @Hi smallint = CASE WHEN @CcodeA < @CcodeB THEN @CcodeB ELSE @CcodeA END;

    SELECT
        EventType,
        Source,
        CAST(StartYear AS int)  AS StartYear,
        CAST(StartMonth AS int) AS StartMonth,
        CAST(StartDay AS int)   AS StartDay,
        CAST(EndYear AS int)    AS EndYear,
        CAST(EndMonth AS int)   AS EndMonth,
        CAST(EndDay AS int)     AS EndDay,
        IsOngoing,
        IsLeftCensored,
        SortKey,
        Title,
        Detail
    FROM dbo.DyadEvent
    WHERE CcodeLo = @Lo
      AND CcodeHi = @Hi
    ORDER BY SortKey, Source, EventId;
END;
GO
